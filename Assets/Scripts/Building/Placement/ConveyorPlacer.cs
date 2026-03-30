using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles conveyor belt placement with Factorio-style drag behavior.
///
/// Features:
///   - Hover preview with auto-snap to neighbor outputs (§1, §2)
///   - Mouse-down locks first-cell input direction (§3)
///   - Drag end auto-snaps output to neighbor inputs (§4)
///   - Mouse proximity fallback for output direction (§5)
///   - R key manual override during drag (§6)
///   - Backward drag prevention based on input direction (§7)
///   - Obstacle truncation with break semantics (§8)
///   - Priority chain for output direction (§priority)
/// </summary>
public class ConveyorPlacer : MonoBehaviour
{
    // ── Public state (read by BuildingPlacer) ───────────────────
    public bool IsDragging => _isDragging;

    // ── Drag state ──────────────────────────────────────────────
    private bool _isDragging = false;
    private Vector2Int _dragStart;
    private Vector2Int _lastMouseGridPos;

    // §3: Locked first-cell input direction (set on mouse-down, never changes during drag)
    private Direction _lockedInputDir;

    // L-path bend preference: true = horizontal first, false = vertical first
    private bool _horizontalFirst = true;
    private bool _bendDecided = false;

    // §6: R key manual override for the last cell's output during drag
    private Direction? _dragManualOutput = null;
    private Vector2Int _dragManualOutputCell; // reset when last cell changes

    // Computed path and ghosts
    private List<Vector2Int> _path = new List<Vector2Int>();
    private List<GameObject> _ghostSegments = new List<GameObject>();

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        BuildingPlacer placer = BuildingPlacer.Instance;
        if (placer == null) return;

        // Only active when conveyor is selected and not in remove mode
        if (!placer.SelectedType.HasValue ||
            placer.SelectedType.Value != BuildingType.Conveyor ||
            placer.RemoveMode)
        {
            if (_isDragging) CancelDrag();
            return;
        }

        // ── Mouse down — start drag (§3) ──
        if (Input.GetMouseButtonDown(0))
        {
            if (placer.IsMouseOverUI()) return;

            Vector2Int pos = GetMouseGridPos();
            if (!GridSystem.Instance.IsCellInBounds(pos)) return;
            if (!GridSystem.Instance.IsCellAvailable(pos)) return;

            _isDragging = true;
            _dragStart = pos;
            _lastMouseGridPos = pos;
            _bendDecided = false;
            _dragManualOutput = null;

            // §3: Lock the first-cell input direction from the hover preview
            Direction displayFacing = placer.ConveyorDisplayFacing;
            _lockedInputDir = displayFacing.Opposite(); // input is opposite of output/facing

            ComputePath(pos);
            UpdateGhostSegments();
        }

        // ── Mouse held — update path in real-time (§4, §5) ──
        if (_isDragging && Input.GetMouseButton(0))
        {
            Vector2Int mouseGridPos = GetMouseGridPos();

            // §6: R key manual override during drag
            if (Input.GetKeyDown(KeyCode.R))
            {
                CycleManualOutput();
            }

            // §6: If mouse moved to a different last-cell, reset manual override
            // We need to refresh every frame for mouse proximity (§4, §5)
            bool gridChanged = mouseGridPos != _lastMouseGridPos;
            if (gridChanged)
            {
                // Decide bend direction on first move away from start
                if (!_bendDecided && mouseGridPos != _dragStart)
                {
                    Vector2Int firstDelta = mouseGridPos - _dragStart;
                    _horizontalFirst = Mathf.Abs(firstDelta.x) >= Mathf.Abs(firstDelta.y);
                    _bendDecided = true;
                }

                _lastMouseGridPos = mouseGridPos;

                // Reset manual override when last cell changes
                _dragManualOutput = null;
            }

            // §7: Clamp endpoint so we can't drag backward
            Vector2Int clampedEnd = ClampEndpoint(mouseGridPos);

            ComputePath(clampedEnd);
            // Refresh ghosts every frame (mouse proximity changes even within same cell)
            UpdateGhostSegments();
        }

        // ── Mouse up — place all segments ──
        if (_isDragging && Input.GetMouseButtonUp(0))
        {
            FinishPlacement();
        }

        // ── Cancel ──
        if (_isDragging && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
        {
            CancelDrag();
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  §7: Clamp endpoint — prevent dragging backward
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Based on the locked input direction, clamp the endpoint so the player
    /// can't drag in the direction items come from.
    /// E.g., if input is from Down (items flow up), endpoint.y >= start.y.
    /// </summary>
    private Vector2Int ClampEndpoint(Vector2Int rawEnd)
    {
        int ex = rawEnd.x;
        int ey = rawEnd.y;

        // _lockedInputDir is the direction items come FROM.
        // We forbid dragging in that direction.
        switch (_lockedInputDir)
        {
            case Direction.Down:
                // Items come from below → can't go below start
                if (ey < _dragStart.y) ey = _dragStart.y;
                break;
            case Direction.Up:
                // Items come from above → can't go above start
                if (ey > _dragStart.y) ey = _dragStart.y;
                break;
            case Direction.Left:
                // Items come from left → can't go left of start
                if (ex < _dragStart.x) ex = _dragStart.x;
                break;
            case Direction.Right:
                // Items come from right → can't go right of start
                if (ex > _dragStart.x) ex = _dragStart.x;
                break;
        }

        return new Vector2Int(ex, ey);
    }

    // ═══════════════════════════════════════════════════════════
    //  §8: Path computation with obstacle truncation (break, not continue)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Computes an L-shaped path from _dragStart to endPos.
    /// Obstacles cause truncation (break), not skipping (continue).
    /// AddLeg returns the last valid position reached.
    /// </summary>
    private void ComputePath(Vector2Int endPos)
    {
        _path.Clear();

        int sx = _dragStart.x, sy = _dragStart.y;
        int ex = endPos.x, ey = endPos.y;

        if (_horizontalFirst)
        {
            // Horizontal leg: sx → ex at row sy
            Vector2Int legEnd = AddLeg(sx, sy, ex, sy, true);
            // Vertical leg: from legEnd to ey (skip corner already added)
            if (ey != legEnd.y)
                AddLeg(legEnd.x, legEnd.y, legEnd.x, ey, false);
        }
        else
        {
            // Vertical leg first: sy → ey at column sx
            Vector2Int legEnd = AddLeg(sx, sy, sx, ey, true);
            // Horizontal leg: from legEnd to ex
            if (ex != legEnd.x)
                AddLeg(legEnd.x, legEnd.y, ex, legEnd.y, false);
        }
    }

    /// <summary>
    /// Adds cells along a straight line from (x0,y0) to (x1,y1).
    /// If includeStart is false, skips the first cell (it's the corner already in path).
    /// BREAKS on obstacle or out-of-bounds (§8).
    /// Returns the last successfully added position (or start if nothing added).
    /// </summary>
    private Vector2Int AddLeg(int x0, int y0, int x1, int y1, bool includeStart)
    {
        int dx = x1 > x0 ? 1 : (x1 < x0 ? -1 : 0);
        int dy = y1 > y0 ? 1 : (y1 < y0 ? -1 : 0);
        int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));

        Vector2Int lastValid = new Vector2Int(x0, y0);

        for (int i = (includeStart ? 0 : 1); i <= steps; i++)
        {
            Vector2Int pos = new Vector2Int(x0 + dx * i, y0 + dy * i);

            // §8: BREAK on out-of-bounds or occupied
            if (!GridSystem.Instance.IsCellInBounds(pos)) break;
            if (!GridSystem.Instance.IsCellAvailable(pos)) break;

            _path.Add(pos);
            lastValid = pos;
        }

        return lastValid;
    }

    // ═══════════════════════════════════════════════════════════
    //  §6: R key manual override — cycle output directions
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Cycles the manual output override among the 3 valid directions
    /// (all except the input direction of the last cell) in CW order.
    /// </summary>
    private void CycleManualOutput()
    {
        if (_path.Count == 0) return;

        Direction inputDir = GetInputDirection(_path.Count - 1);
        Direction excludeDir = inputDir; // can't output back toward input

        // Build list of 3 valid output directions (CW order)
        List<Direction> valid = new List<Direction>();
        Direction d = Direction.Up;
        for (int i = 0; i < 4; i++)
        {
            if (d != excludeDir)
                valid.Add(d);
            d = d.RotateCW();
        }

        if (valid.Count == 0) return;

        if (!_dragManualOutput.HasValue)
        {
            _dragManualOutput = valid[0];
        }
        else
        {
            int idx = valid.IndexOf(_dragManualOutput.Value);
            _dragManualOutput = valid[(idx + 1) % valid.Count];
        }

        _dragManualOutputCell = _path[_path.Count - 1];
    }

    // ═══════════════════════════════════════════════════════════
    //  Direction computation for each path cell
    // ═══════════════════════════════════════════════════════════

    private Direction GetInputDirection(int index)
    {
        if (index == 0)
        {
            // First cell: use the locked input direction (§3)
            return _lockedInputDir;
        }
        // Middle/later cells: input comes from the previous cell
        return DirectionExtensions.FromVector(_path[index], _path[index - 1]);
    }

    /// <summary>
    /// Output direction for cell at index, using the priority chain (§priority):
    /// 1. R key manual override (_dragManualOutput) — only for last cell
    /// 2. Neighbor building input snap (TrySnapOutput)
    /// 3. Mouse proximity to empty neighbor (GetOutputByMouseProximity)
    /// 4. Path continuation direction, or CurrentFacing for single cell
    /// </summary>
    private Direction GetOutputDirection(int index)
    {
        Direction inputDir = GetInputDirection(index);
        Direction excludeDir = inputDir; // can't output back toward where items come from

        // Not the last cell — output toward next cell in path
        if (index < _path.Count - 1)
        {
            return DirectionExtensions.FromVector(_path[index], _path[index + 1]);
        }

        // ── Last cell — apply priority chain ──

        BuildingPlacer placer = BuildingPlacer.Instance;
        Vector2Int pos = _path[index];

        // Priority 1: R key manual override (§6)
        if (_dragManualOutput.HasValue && _dragManualOutputCell == pos)
        {
            return _dragManualOutput.Value;
        }

        // Priority 2: Snap to neighbor building's input port (§4)
        Direction? snapOutput = placer.TrySnapOutputToNeighborInput(pos, excludeDir);
        if (snapOutput.HasValue)
        {
            return snapOutput.Value;
        }

        // Priority 3: Mouse proximity to nearest neighbor cell (§5)
        Direction? proxOutput = placer.GetOutputByMouseProximity(pos, excludeDir);
        if (proxOutput.HasValue)
        {
            return proxOutput.Value;
        }

        // Priority 4: Path continuation or CurrentFacing
        if (_path.Count >= 2)
        {
            // Continue in the direction of the last leg
            return DirectionExtensions.FromVector(_path[index - 1], _path[index]);
        }

        // Single cell fallback
        Direction fallback = placer.ConveyorDisplayFacing;
        if (fallback == excludeDir)
            fallback = excludeDir.Opposite();
        return fallback;
    }

    // ═══════════════════════════════════════════════════════════
    //  Placement
    // ═══════════════════════════════════════════════════════════

    private void FinishPlacement()
    {
        if (_path.Count == 0)
        {
            CancelDrag();
            return;
        }

        for (int i = 0; i < _path.Count; i++)
        {
            Direction inputDir = GetInputDirection(i);
            Direction outputDir = GetOutputDirection(i);

            // Final safety: don't allow input == output
            if (inputDir == outputDir)
                outputDir = inputDir.Opposite();

            CreateConveyorSegment(_path[i], inputDir, outputDir);
        }

        Debug.Log($"[ConveyorPlacer] Placed {_path.Count} conveyor segment(s).");
        ClearGhostSegments();
        _isDragging = false;
        _path.Clear();
    }

    private void CreateConveyorSegment(Vector2Int pos, Direction inputDir, Direction outputDir)
    {
        GameObject instance = new GameObject($"Conveyor_{pos.x}_{pos.y}");

        SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
        bool isCorner = inputDir.Opposite() != outputDir;
        sr.sprite = isCorner ? SpriteFactory.GetConveyorCorner() : SpriteFactory.GetConveyorStraight();
        sr.sortingOrder = 5;

        ConveyorBelt conveyor = instance.AddComponent<ConveyorBelt>();
        conveyor.Initialize(pos, inputDir, outputDir);
    }

    // ═══════════════════════════════════════════════════════════
    //  Ghost preview (refreshed every frame during drag)
    // ═══════════════════════════════════════════════════════════

    private void UpdateGhostSegments()
    {
        ClearGhostSegments();

        for (int i = 0; i < _path.Count; i++)
        {
            Vector3 worldPos = GridSystem.Instance.GridToWorld(_path[i]);
            GameObject ghost = new GameObject("ConveyorGhost");
            ghost.transform.position = worldPos;

            SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 20;

            Direction inputDir = GetInputDirection(i);
            Direction outputDir = GetOutputDirection(i);

            // Safety
            if (inputDir == outputDir)
                outputDir = inputDir.Opposite();

            bool isCorner = inputDir.Opposite() != outputDir;
            sr.sprite = isCorner ? SpriteFactory.GetConveyorCorner() : SpriteFactory.GetConveyorStraight();
            sr.color = new Color(0.5f, 0.8f, 1f, 0.5f);

            if (isCorner)
            {
                ConveyorBelt.GetCornerTransform(inputDir, outputDir, out float angle, out bool flipX);
                ghost.transform.rotation = Quaternion.Euler(0, 0, angle);
                sr.flipX = flipX;
            }
            else
            {
                ghost.transform.rotation = Quaternion.Euler(0, 0, outputDir.ToRotationZ());
            }

            _ghostSegments.Add(ghost);
        }
    }

    private void ClearGhostSegments()
    {
        foreach (var ghost in _ghostSegments)
        {
            if (ghost != null) Destroy(ghost);
        }
        _ghostSegments.Clear();
    }

    private void CancelDrag()
    {
        ClearGhostSegments();
        _isDragging = false;
        _path.Clear();
        _dragManualOutput = null;
    }

    private Vector2Int GetMouseGridPos()
    {
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        return GridSystem.Instance.WorldToGrid(mouseWorld);
    }
}
