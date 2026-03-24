using UnityEngine;

/// <summary>
/// Miner building: placed on ore tiles, produces 1 ore per second.
/// Has a single output port in the Facing direction.
/// Shows buffered ore icon when output is blocked.
/// </summary>
public class MinerBuilding : BuildingBase
{
    public override BuildingType Type => BuildingType.Miner;

    public ItemType OreType { get; private set; } = ItemType.None;

    private float _productionTimer = 0f;
    private float _productionInterval = 1f;
    private bool _hasBufferedItem = false;

    // Visual: small ore icon showing buffered production
    private GameObject _bufferIcon;
    private SpriteRenderer _bufferRenderer;

    public override void Initialize(Vector2Int gridPos, Direction facing)
    {
        base.Initialize(gridPos, facing);

        // Detect ore type from tilemap
        OreType = TilemapReader.Instance.GetOreTypeAt(gridPos);
        Debug.Log($"[Miner] Initialized at {gridPos}, ore type: {OreType}, facing: {facing}");

        if (OreType == ItemType.None)
        {
            Debug.LogWarning($"[Miner] No ore detected at {gridPos}!");
        }

        // Create buffer icon (hidden by default)
        CreateBufferIcon();
    }

    protected override void SetupPorts()
    {
        _ports.Clear();
        _ports.Add(new BuildingPort(PortType.Output, Facing));
    }

    private void Update()
    {
        if (OreType == ItemType.None) return;

        _productionTimer += Time.deltaTime;
        if (_productionTimer >= _productionInterval)
        {
            _productionTimer -= _productionInterval;

            if (!_hasBufferedItem)
            {
                // Produce into buffer
                _hasBufferedItem = true;
                UpdateBufferIcon(true);
            }
        }

        // Try to push buffered item to output
        if (_hasBufferedItem)
        {
            TryPushBufferedItem();
        }
    }

    private void TryPushBufferedItem()
    {
        Vector2Int outputPos = GridPosition + Facing.ToVector2Int();
        BuildingBase neighbor = BuildingRegistry.GetAt(outputPos);

        if (neighbor == null) return;

        // The item comes FROM our direction (opposite of facing)
        // e.g., if miner faces Up, item arrives at neighbor FROM below = Direction.Down
        Direction fromDir = Facing.Opposite();
        if (!neighbor.CanAcceptItem(fromDir)) return;

        bool accepted = neighbor.OnItemReceived(OreType, fromDir);
        if (accepted)
        {
            _hasBufferedItem = false;
            UpdateBufferIcon(false);
        }
    }

    private void CreateBufferIcon()
    {
        _bufferIcon = new GameObject("BufferIcon");
        _bufferIcon.transform.SetParent(transform);

        // Position the icon at the output edge
        Vector3 offset = new Vector3(
            Facing.ToVector2Int().x * 0.3f,
            Facing.ToVector2Int().y * 0.3f,
            0);
        _bufferIcon.transform.localPosition = offset;
        _bufferIcon.transform.localRotation = Quaternion.identity;
        _bufferIcon.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        _bufferRenderer = _bufferIcon.AddComponent<SpriteRenderer>();
        _bufferRenderer.sprite = SpriteFactory.GetItemSprite();
        _bufferRenderer.color = ConveyorItem.GetColorForType(OreType);
        _bufferRenderer.sortingOrder = 8;

        _bufferIcon.SetActive(false);
    }

    private void UpdateBufferIcon(bool show)
    {
        if (_bufferIcon != null)
            _bufferIcon.SetActive(show);
    }

    public override void OnRemoved()
    {
        if (_bufferIcon != null)
            Destroy(_bufferIcon);
        base.OnRemoved();
    }
}
