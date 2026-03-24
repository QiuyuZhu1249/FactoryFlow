using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles conveyor belt placement with Factorio-style drag behavior.
/// - Click: place single conveyor at mouse position using current facing.
/// - Drag: computes an L-shaped path from start to cursor in real-time.
///   Bend direction is determined by which axis the mouse moves first.
///   Ghost preview updates every frame. On release, all segments are placed.
/// </summary>
public class ConveyorPlacer : MonoBehaviour
{
    private bool _isDragging = false;
    private Vector2Int _dragStart;
    private Vector2Int _lastMousePos;

    // L-path bend preference: true = horizontal first, false = vertical first
    private bool _horizontalFirst = true;
    private bool _bendDecided = false;

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
        if (!placer.SelectedType.HasValue || placer.SelectedType.Value != BuildingType.Conveyor || placer.RemoveMode)
        {
            if (_isDragging) CancelDrag();
            return;
        }

        // Mouse down — start drag
        if (Input.GetMouseButtonDown(0))
        {
            if (placer.IsMouseOverUI()) return;

            Vector2Int pos = GetMouseGridPos();
            if (!GridSystem.Instance.IsCellInBounds(pos)) return;

            _isDragging = true;
            _dragStart = pos;
            _lastMousePos = pos;
            _bendDecided = false;
            ComputePath(pos);
            UpdateGhostSegments();
        }

        // Mouse held — update path in real-time
        if (_isDragging && Input.GetMouseButton(0))
        {
            Vector2Int mousePos = GetMouseGridPos();
            if (mousePos != _lastMousePos)
            {
                // Decide bend direction on first move away from start
                if (!_bendDecided && mousePos != _dragStart)
                {
                    Vector2Int firstDelta = mousePos - _dragStart;
                    // If first movement is more horizontal, do horizontal first
                    _horizontalFirst = Mathf.Abs(firstDelta.x) >= Mathf.Abs(firstDelta.y);
                    _bendDecided = true;
                }

                _lastMousePos = mousePos;
                ComputePath(mousePos);
                UpdateGhostSegments();
            }
        }

        // Mouse up — place all segments
        if (_isDragging && Input.GetMouseButtonUp(0))
        {
            FinishPlacement();
        }

        // Cancel
        if (_isDragging && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
        {
            CancelDrag();
        }
    }

    /// <summary>
    /// Computes an L-shaped path from _dragStart to endPos.
    /// Skips cells that are already occupied.
    /// </summary>
    private void ComputePath(Vector2Int endPos)
    {
        _path.Clear();

        int sx = _dragStart.x, sy = _dragStart.y;
        int ex = endPos.x, ey = endPos.y;

        if (_horizontalFirst)
        {
            // Horizontal leg: sx → ex at row sy
            AddLeg(sx, sy, ex, sy, true);
            // Vertical leg: sy → ey at column ex (skip the corner, already added)
            if (ey != sy)
                AddLeg(ex, sy, ex, ey, false);
        }
        else
        {
            // Vertical leg first: sy → ey at column sx
            AddLeg(sx, sy, sx, ey, true);
            // Horizontal leg: sx → ex at row ey (skip the corner)
            if (ex != sx)
                AddLeg(sx, ey, ex, ey, false);
        }
    }

    /// <summary>
    /// Adds cells along a straight line from (x0,y0) to (x1,y1).
    /// If includeStart is false, skips the first cell (it's the corner already in path).
    /// Skips occupied cells.
    /// </summary>
    private void AddLeg(int x0, int y0, int x1, int y1, bool includeStart)
    {
        int dx = x1 > x0 ? 1 : (x1 < x0 ? -1 : 0);
        int dy = y1 > y0 ? 1 : (y1 < y0 ? -1 : 0);
        int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));

        for (int i = (includeStart ? 0 : 1); i <= steps; i++)
        {
            Vector2Int pos = new Vector2Int(x0 + dx * i, y0 + dy * i);
            if (!GridSystem.Instance.IsCellInBounds(pos)) continue;
            if (!GridSystem.Instance.IsCellAvailable(pos)) continue;
            _path.Add(pos);
        }
    }

    private void FinishPlacement()
    {
        if (_path.Count == 0)
        {
            CancelDrag();
            return;
        }

        // Single click (no drag, or same cell) — place one conveyor using current facing
        if (_path.Count == 1 && _path[0] == _dragStart)
        {
            Direction facing = BuildingPlacer.Instance.CurrentFacing;
            if (GridSystem.Instance.IsCellAvailable(_path[0]))
                CreateConveyorSegment(_path[0], facing.Opposite(), facing);
        }
        else
        {
            // Multi-segment path — auto-detect directions
            for (int i = 0; i < _path.Count; i++)
            {
                Direction inputDir = GetInputDirection(i);
                Direction outputDir = GetOutputDirection(i);
                CreateConveyorSegment(_path[i], inputDir, outputDir);
            }
        }

        Debug.Log($"[ConveyorPlacer] Placed {_path.Count} conveyor segment(s).");
        ClearGhostSegments();
        _isDragging = false;
        _path.Clear();
    }

    private Direction GetInputDirection(int index)
    {
        if (index == 0)
        {
            if (_path.Count > 1)
                return DirectionExtensions.FromVector(_path[0], _path[1]).Opposite();
            else
                return BuildingPlacer.Instance.CurrentFacing.Opposite();
        }
        return DirectionExtensions.FromVector(_path[index], _path[index - 1]);
    }

    private Direction GetOutputDirection(int index)
    {
        if (index < _path.Count - 1)
            return DirectionExtensions.FromVector(_path[index], _path[index + 1]);
        if (_path.Count >= 2)
            return DirectionExtensions.FromVector(_path[index - 1], _path[index]);
        return BuildingPlacer.Instance.CurrentFacing;
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

    private void CancelDrag()
    {
        ClearGhostSegments();
        _isDragging = false;
        _path.Clear();
    }

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

            Direction inputDir, outputDir;
            if (_path.Count == 1)
            {
                outputDir = BuildingPlacer.Instance.CurrentFacing;
                inputDir = outputDir.Opposite();
            }
            else
            {
                inputDir = GetInputDirection(i);
                outputDir = GetOutputDirection(i);
            }

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

    private Vector2Int GetMouseGridPos()
    {
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        return GridSystem.Instance.WorldToGrid(mouseWorld);
    }
}
