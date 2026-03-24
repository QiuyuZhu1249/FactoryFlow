using UnityEngine;

/// <summary>
/// Production Station: 2 inputs (left/right sides), 1 output (facing).
/// Buffers one item per input. When both slots filled and recipe matches,
/// crafts the output after a short delay and pushes it forward.
/// </summary>
public class ProductionStation : BuildingBase
{
    public override BuildingType Type => BuildingType.ProductionStation;

    // Input buffers (one per side)
    private ItemType _slotLeft = ItemType.None;
    private ItemType _slotRight = ItemType.None;

    // Crafting state
    private ItemType _outputItem = ItemType.None;
    private bool _isCrafting = false;
    private float _craftTimer = 0f;
    private const float CRAFT_TIME = 1.5f;

    // Output buffer
    private bool _hasOutput = false;

    // Visual indicators
    private SpriteRenderer _leftIndicator;
    private SpriteRenderer _rightIndicator;
    private SpriteRenderer _outputIndicator;
    private SpriteRenderer _progressBar;

    protected override void SetupPorts()
    {
        _ports.Clear();
        // Output in facing direction
        _ports.Add(new BuildingPort(PortType.Output, Facing));
        // 2 inputs on the sides perpendicular to facing
        _ports.Add(new BuildingPort(PortType.Input, Facing.RotateCW()));
        _ports.Add(new BuildingPort(PortType.Input, Facing.RotateCCW()));

        CreateIndicators();
    }

    public override bool CanAcceptItem(Direction fromDirection)
    {
        if (fromDirection == Facing.RotateCW() && _slotRight == ItemType.None)
            return true;
        if (fromDirection == Facing.RotateCCW() && _slotLeft == ItemType.None)
            return true;
        return false;
    }

    public override bool OnItemReceived(ItemType itemType, Direction fromDirection)
    {
        if (fromDirection == Facing.RotateCW() && _slotRight == ItemType.None)
        {
            _slotRight = itemType;
            UpdateIndicators();
            TryStartCrafting();
            return true;
        }
        if (fromDirection == Facing.RotateCCW() && _slotLeft == ItemType.None)
        {
            _slotLeft = itemType;
            UpdateIndicators();
            TryStartCrafting();
            return true;
        }
        return false;
    }

    private void TryStartCrafting()
    {
        if (_isCrafting || _hasOutput) return;
        if (_slotLeft == ItemType.None || _slotRight == ItemType.None) return;

        if (RecipeDatabase.TryFind(_slotLeft, _slotRight, out var recipe))
        {
            _outputItem = recipe.Output;
            _isCrafting = true;
            _craftTimer = 0f;
            Debug.Log($"[ProductionStation] Crafting {_slotLeft} + {_slotRight} → {_outputItem}");
        }
        else
        {
            Debug.Log($"[ProductionStation] No recipe for {_slotLeft} + {_slotRight}");
        }
    }

    private void Update()
    {
        if (_isCrafting)
        {
            _craftTimer += Time.deltaTime;
            UpdateProgressBar();

            if (_craftTimer >= CRAFT_TIME)
            {
                // Crafting complete
                _slotLeft = ItemType.None;
                _slotRight = ItemType.None;
                _isCrafting = false;
                _hasOutput = true;
                UpdateIndicators();
                Debug.Log($"[ProductionStation] Produced {_outputItem}");
            }
        }

        if (_hasOutput)
        {
            TryPushOutput();
        }
    }

    private void TryPushOutput()
    {
        Vector2Int nextPos = GridPosition + Facing.ToVector2Int();
        BuildingBase neighbor = BuildingRegistry.GetAt(nextPos);

        if (neighbor != null)
        {
            Direction inputDir = Facing.Opposite();
            if (neighbor.CanAcceptItem(inputDir))
            {
                neighbor.OnItemReceived(_outputItem, inputDir);
                _hasOutput = false;
                _outputItem = ItemType.None;
                UpdateIndicators();

                // Check if new inputs already arrived during output wait
                TryStartCrafting();
            }
        }
    }

    private void CreateIndicators()
    {
        // Small colored squares showing buffered items
        _leftIndicator = CreateSmallIndicator("LeftSlot",
            Facing.RotateCCW().ToVector2Int(), -0.15f);
        _rightIndicator = CreateSmallIndicator("RightSlot",
            Facing.RotateCW().ToVector2Int(), -0.15f);
        _outputIndicator = CreateSmallIndicator("OutputSlot",
            Facing.ToVector2Int(), 0f);

        // Progress bar — position counter-rotated to stay visually at bottom
        GameObject barGo = new GameObject("ProgressBar");
        barGo.transform.SetParent(transform);
        Vector3 barPos = new Vector3(0, -0.35f, 0);
        barPos = Quaternion.Euler(0, 0, -Facing.ToRotationZ()) * barPos;
        barGo.transform.localPosition = barPos;
        barGo.transform.localScale = new Vector3(0f, 0.06f, 1f);
        barGo.transform.localRotation = Quaternion.Euler(0, 0, -Facing.ToRotationZ());
        _progressBar = barGo.AddComponent<SpriteRenderer>();
        _progressBar.sprite = SpriteFactory.GetSquare();
        _progressBar.color = new Color(0.2f, 0.9f, 0.3f);
        _progressBar.sortingOrder = 8;

        UpdateIndicators();
    }

    private SpriteRenderer CreateSmallIndicator(string name, Vector2Int offset, float zOffset)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        // Position slightly toward the port side
        Vector3 localPos = new Vector3(offset.x * 0.25f, offset.y * 0.25f, 0);
        // Undo parent rotation so indicators stay axis-aligned
        localPos = Quaternion.Euler(0, 0, -Facing.ToRotationZ()) * localPos;
        go.transform.localPosition = localPos;
        go.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
        go.transform.localRotation = Quaternion.Euler(0, 0, -Facing.ToRotationZ());

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.GetSquare();
        sr.sortingOrder = 8;
        sr.enabled = false;

        return sr;
    }

    private void UpdateIndicators()
    {
        // Left slot
        if (_slotLeft != ItemType.None)
        {
            _leftIndicator.enabled = true;
            _leftIndicator.color = ConveyorItem.GetColorForType(_slotLeft);
        }
        else
        {
            _leftIndicator.enabled = false;
        }

        // Right slot
        if (_slotRight != ItemType.None)
        {
            _rightIndicator.enabled = true;
            _rightIndicator.color = ConveyorItem.GetColorForType(_slotRight);
        }
        else
        {
            _rightIndicator.enabled = false;
        }

        // Output
        if (_hasOutput)
        {
            _outputIndicator.enabled = true;
            _outputIndicator.color = ConveyorItem.GetColorForType(_outputItem);
        }
        else
        {
            _outputIndicator.enabled = false;
        }

        // Progress bar hidden when not crafting
        if (_progressBar != null && !_isCrafting)
        {
            _progressBar.transform.localScale = new Vector3(0f, 0.06f, 1f);
        }
    }

    private void UpdateProgressBar()
    {
        if (_progressBar == null) return;
        float progress = Mathf.Clamp01(_craftTimer / CRAFT_TIME);
        _progressBar.transform.localScale = new Vector3(progress * 0.8f, 0.06f, 1f);
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
    }
}
