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

    // Ghost preview
    private GameObject _ghostPreview;
    private SpriteRenderer _ghostRenderer;

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
            CurrentFacing = CurrentFacing.RotateCW();
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
        if (SelectedType == null || RemoveMode ||
            SelectedType.Value == BuildingType.Conveyor)
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
        _ghostPreview.transform.rotation = Quaternion.Euler(0, 0, CurrentFacing.ToRotationZ());

        // Show the actual building sprite as ghost
        _ghostRenderer.sprite = SpriteFactory.GetSpriteForType(SelectedType.Value);

        // Color based on validity
        bool valid = GridSystem.Instance.IsCellAvailable(gridPos);
        if (SelectedType.Value == BuildingType.Miner)
            valid = valid && TilemapReader.Instance.HasOre(gridPos);

        _ghostRenderer.color = valid
            ? new Color(1f, 1f, 1f, 0.5f)
            : new Color(1f, 0.3f, 0.3f, 0.5f);
    }

    #endregion
}
