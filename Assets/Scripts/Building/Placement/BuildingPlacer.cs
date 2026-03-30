using UnityEngine;

/// <summary>
/// Handles building placement, rotation, and removal.
/// Keyboard: 1-5 select building, R rotate, X toggle remove mode.
/// Mouse: left click to place/remove.
/// </summary>
public class BuildingPlacer : MonoBehaviour
{
    public static BuildingPlacer Instance { get; private set; }

    // State
    public BuildingType? SelectedType { get; private set; } = null;
    public Direction CurrentFacing { get; private set; } = Direction.Up;
    public bool RemoveMode { get; private set; } = false;

    // ── Conveyor hover preview ──────────────────────────────────
    /// <summary>
    /// The facing actually displayed on the conveyor hover ghost
    /// (may differ from CurrentFacing due to auto-snap).
    /// ConveyorPlacer reads this on mouse-down to lock the first-cell input.
    /// </summary>
    public Direction ConveyorDisplayFacing { get; private set; }

    // Ghost preview
    private GameObject _ghostPreview;
    private SpriteRenderer _ghostRenderer;

    // Conveyor hover auto-snap state
    private Vector2Int _lastConveyorHoverCell;
    private bool _conveyorManualRotate = false; // true after R key on current cell

    // Conveyor placement
    private ConveyorPlacer _conveyorPlacer;

    private Camera _mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _conveyorPlacer = GetComponent<ConveyorPlacer>();
        if (_conveyorPlacer == null)
            _conveyorPlacer = gameObject.AddComponent<ConveyorPlacer>();

        CreateGhostPreview();
        ConveyorDisplayFacing = CurrentFacing;
    }

    private void Update()
    {
        HandleInput();
        UpdateGhostPreview();
    }

    private void HandleInput()
    {
        // Building selection (1-5)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectBuilding(BuildingType.Miner);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectBuilding(BuildingType.Conveyor);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectBuilding(BuildingType.Splitter);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectBuilding(BuildingType.Merger);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectBuilding(BuildingType.ProductionStation);

        // Rotate
        if (Input.GetKeyDown(KeyCode.R))
        {
            // During conveyor drag, R is handled by ConveyorPlacer — don't touch CurrentFacing
            if (_conveyorPlacer != null && _conveyorPlacer.IsDragging)
                return;

            CurrentFacing = CurrentFacing.RotateCW();

            // If hovering conveyor, mark as manual rotate for this cell
            if (SelectedType.HasValue && SelectedType.Value == BuildingType.Conveyor)
            {
                _conveyorManualRotate = true;
                ConveyorDisplayFacing = CurrentFacing;
            }

            Debug.Log($"[BuildingPlacer] Facing: {CurrentFacing}");
        }

        // Toggle remove mode
        if (Input.GetKeyDown(KeyCode.X))
        {
            RemoveMode = !RemoveMode;
            if (RemoveMode) SelectedType = null;
            Debug.Log($"[BuildingPlacer] Mode: {(RemoveMode ? "REMOVE" : "BUILD")}");
        }

        // Escape to deselect
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SelectedType = null;
            RemoveMode = false;
        }

        // Left click to place/remove (conveyor handled by ConveyorPlacer)
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverUI()) return;

            Vector2Int gridPos = GetMouseGridPos();

            if (RemoveMode)
            {
                TryRemoveBuilding(gridPos);
            }
            else if (SelectedType.HasValue && SelectedType.Value != BuildingType.Conveyor)
            {
                TryPlaceBuilding(gridPos);
            }
        }
    }

    public void SelectBuilding(BuildingType type)
    {
        RemoveMode = false;
        SelectedType = type;
        _conveyorManualRotate = false;
        Debug.Log($"[BuildingPlacer] Selected: {type}");
    }

    private void TryPlaceBuilding(Vector2Int gridPos)
    {
        if (!GridSystem.Instance.IsCellInBounds(gridPos))
        {
            Debug.LogWarning("[BuildingPlacer] Out of bounds!");
            return;
        }

        if (!GridSystem.Instance.IsCellAvailable(gridPos))
        {
            Debug.LogWarning("[BuildingPlacer] Cell is occupied!");
            return;
        }

        BuildingType type = SelectedType.Value;

        // Miner validation: must be on ore
        if (type == BuildingType.Miner)
        {
            if (!TilemapReader.Instance.HasOre(gridPos))
            {
                Debug.LogWarning("[BuildingPlacer] Miners can only be placed on ore tiles!");
                return;
            }
        }

        // Create building dynamically
        GameObject instance = new GameObject($"{type}_{gridPos.x}_{gridPos.y}");
        SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.GetSpriteForType(type);
        sr.sortingOrder = 5;

        BuildingBase building = AddBuildingComponent(instance, type);
        building.Initialize(gridPos, CurrentFacing);
    }

    private void TryRemoveBuilding(Vector2Int gridPos)
    {
        BuildingBase building = BuildingRegistry.GetAt(gridPos);
        if (building == null)
        {
            Debug.Log("[BuildingPlacer] Nothing to remove here.");
            return;
        }

        if (building.Type == BuildingType.ResourceCollector)
        {
            Debug.LogWarning("[BuildingPlacer] Cannot remove the collector!");
            return;
        }

        building.OnRemoved();
    }

    private BuildingBase AddBuildingComponent(GameObject obj, BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Miner:             return obj.AddComponent<MinerBuilding>();
            case BuildingType.Splitter:           return obj.AddComponent<SplitterBuilding>();
            case BuildingType.Merger:             return obj.AddComponent<MergerBuilding>();
            case BuildingType.ProductionStation:  return obj.AddComponent<ProductionStation>();
            default: return obj.AddComponent<MinerBuilding>();
        }
    }

    public Vector2Int GetMouseGridPos()
    {
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        return GridSystem.Instance.WorldToGrid(mouseWorld);
    }

    public Vector3 GetMouseWorldPos()
    {
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        return mouseWorld;
    }

    public bool IsMouseOverUI()
    {
        return Input.mousePosition.y < 80f;
    }

    #region Ghost Preview

    private void CreateGhostPreview()
    {
        _ghostPreview = new GameObject("GhostPreview");
        _ghostRenderer = _ghostPreview.AddComponent<SpriteRenderer>();
        _ghostRenderer.sortingOrder = 20;
        _ghostPreview.SetActive(false);
    }

    private void UpdateGhostPreview()
    {
        // Hide ghost when no selection, remove mode, or conveyor is dragging
        if (SelectedType == null || RemoveMode)
        {
            _ghostPreview.SetActive(false);
            return;
        }

        // If conveyor is dragging, hide the hover ghost (drag ghosts take over)
        if (SelectedType.Value == BuildingType.Conveyor &&
            _conveyorPlacer != null && _conveyorPlacer.IsDragging)
        {
            _ghostPreview.SetActive(false);
            return;
        }

        Vector2Int gridPos = GetMouseGridPos();
        if (!GridSystem.Instance.IsCellInBounds(gridPos))
        {
            _ghostPreview.SetActive(false);
            return;
        }

        _ghostPreview.SetActive(true);
        Vector3 worldPos = GridSystem.Instance.GridToWorld(gridPos);
        _ghostPreview.transform.position = worldPos;

        // ── Conveyor-specific hover logic ──
        if (SelectedType.Value == BuildingType.Conveyor)
        {
            UpdateConveyorHoverPreview(gridPos);
            return;
        }

        // ── Non-conveyor buildings ──
        _ghostPreview.transform.rotation = Quaternion.Euler(0, 0, CurrentFacing.ToRotationZ());
        _ghostRenderer.sprite = SpriteFactory.GetSpriteForType(SelectedType.Value);
        _ghostRenderer.flipX = false;

        bool valid = GridSystem.Instance.IsCellAvailable(gridPos);
        if (SelectedType.Value == BuildingType.Miner)
            valid = valid && TilemapReader.Instance.HasOre(gridPos);

        _ghostRenderer.color = valid
            ? new Color(1f, 1f, 1f, 0.5f)
            : new Color(1f, 0.3f, 0.3f, 0.5f);
    }

    /// <summary>
    /// Conveyor hover ghost: auto-snap input direction to neighbor outputs,
    /// display correct straight/corner sprite, color red if occupied.
    /// </summary>
    private void UpdateConveyorHoverPreview(Vector2Int gridPos)
    {
        // If mouse moved to a new cell, reset manual rotate
        if (gridPos != _lastConveyorHoverCell)
        {
            _conveyorManualRotate = false;
            _lastConveyorHoverCell = gridPos;
        }

        Direction displayFacing;
        Direction inputDir;
        Direction outputDir;

        if (_conveyorManualRotate)
        {
            // Manual: use CurrentFacing as output
            outputDir = CurrentFacing;
            inputDir = outputDir.Opposite();
            displayFacing = CurrentFacing;
        }
        else
        {
            // Auto-snap: check neighbors for buildings whose output faces this cell
            Direction? snappedInput = TrySnapInputToNeighborOutput(gridPos);
            if (snappedInput.HasValue)
            {
                inputDir = snappedInput.Value;
                // Output = opposite of input (straight conveyor for hover)
                // But we can also use CurrentFacing as output if it's not the same as input
                outputDir = CurrentFacing;
                if (outputDir == inputDir)
                    outputDir = inputDir.Opposite();
                displayFacing = outputDir;
            }
            else
            {
                outputDir = CurrentFacing;
                inputDir = outputDir.Opposite();
                displayFacing = CurrentFacing;
            }
        }

        ConveyorDisplayFacing = displayFacing;

        // Determine sprite (corner vs straight)
        bool isCorner = inputDir.Opposite() != outputDir;
        if (isCorner)
        {
            _ghostRenderer.sprite = SpriteFactory.GetConveyorCorner();
            ConveyorBelt.GetCornerTransform(inputDir, outputDir, out float angle, out bool flipX);
            _ghostPreview.transform.rotation = Quaternion.Euler(0, 0, angle);
            _ghostRenderer.flipX = flipX;
        }
        else
        {
            _ghostRenderer.sprite = SpriteFactory.GetConveyorStraight();
            _ghostPreview.transform.rotation = Quaternion.Euler(0, 0, outputDir.ToRotationZ());
            _ghostRenderer.flipX = false;
        }

        // Color: red if occupied, white-transparent if valid
        bool valid = GridSystem.Instance.IsCellAvailable(gridPos);
        _ghostRenderer.color = valid
            ? new Color(1f, 1f, 1f, 0.5f)
            : new Color(1f, 0.3f, 0.3f, 0.5f);
    }

    /// <summary>
    /// Check the 4 neighbors of gridPos. If any building has an output port
    /// that faces TOWARD gridPos, return the direction items would come FROM
    /// (i.e., the input direction for a conveyor placed at gridPos).
    /// If multiple candidates, pick the one whose cell center is closest to mouse.
    /// Returns null if no neighbor output points at us.
    /// </summary>
    public Direction? TrySnapInputToNeighborOutput(Vector2Int gridPos)
    {
        Vector3 mouseWorld = GetMouseWorldPos();
        Direction? best = null;
        float bestDist = float.MaxValue;

        Direction[] dirs = { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
        foreach (Direction dir in dirs)
        {
            Vector2Int neighborPos = gridPos + dir.ToVector2Int();
            BuildingBase neighbor = BuildingRegistry.GetAt(neighborPos);
            if (neighbor == null) continue;

            // The neighbor needs an output that faces toward gridPos
            // i.e., neighbor output direction == dir.Opposite()
            Direction outputTowardUs = dir.Opposite();
            if (neighbor.HasOutputFacing(outputTowardUs))
            {
                // This neighbor's output points at us; items come FROM dir
                Vector3 neighborWorld = GridSystem.Instance.GridToWorld(neighborPos);
                float dist = Vector3.Distance(mouseWorld, neighborWorld);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = dir; // items come from this direction
                }
            }
        }

        return best;
    }

    /// <summary>
    /// Check the 4 neighbors of gridPos. If any building has an input port
    /// that faces TOWARD gridPos (meaning it can accept items from gridPos),
    /// return the direction we should output toward.
    /// Excludes excludeDir (the input direction — can't output back).
    /// If multiple candidates, pick closest to mouse.
    /// </summary>
    public Direction? TrySnapOutputToNeighborInput(Vector2Int gridPos, Direction excludeDir)
    {
        Vector3 mouseWorld = GetMouseWorldPos();
        Direction? best = null;
        float bestDist = float.MaxValue;

        Direction[] dirs = { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
        foreach (Direction dir in dirs)
        {
            if (dir == excludeDir) continue; // can't output back to input

            Vector2Int neighborPos = gridPos + dir.ToVector2Int();
            BuildingBase neighbor = BuildingRegistry.GetAt(neighborPos);
            if (neighbor == null) continue;

            // The neighbor needs an input that faces toward gridPos
            // i.e., neighbor input direction == dir.Opposite()
            Direction inputFromUs = dir.Opposite();
            if (neighbor.HasInputFacing(inputFromUs))
            {
                Vector3 neighborWorld = GridSystem.Instance.GridToWorld(neighborPos);
                float dist = Vector3.Distance(mouseWorld, neighborWorld);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = dir; // output toward this direction
                }
            }
        }

        return best;
    }

    /// <summary>
    /// Among the 4 neighbors of gridPos, find the empty in-bounds cell closest to mouse.
    /// Excludes excludeDir. Returns the direction toward that cell.
    /// </summary>
    public Direction? GetOutputByMouseProximity(Vector2Int gridPos, Direction excludeDir)
    {
        Vector3 mouseWorld = GetMouseWorldPos();
        Direction? best = null;
        float bestDist = float.MaxValue;

        Direction[] dirs = { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
        foreach (Direction dir in dirs)
        {
            if (dir == excludeDir) continue;

            Vector2Int neighborPos = gridPos + dir.ToVector2Int();
            if (!GridSystem.Instance.IsCellInBounds(neighborPos)) continue;

            // Only consider empty cells (or any cell — proximity is just a hint)
            Vector3 neighborWorld = GridSystem.Instance.GridToWorld(neighborPos);
            float dist = Vector3.Distance(mouseWorld, neighborWorld);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = dir;
            }
        }

        return best;
    }

    #endregion
}
