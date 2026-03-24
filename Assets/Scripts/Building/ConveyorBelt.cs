using UnityEngine;

/// <summary>
/// Conveyor belt segment: moves items from input to output direction.
/// Can be straight or corner piece depending on input/output directions.
/// Each conveyor holds at most 1 item at a time (backpressure).
/// </summary>
public class ConveyorBelt : BuildingBase
{
    public override BuildingType Type => BuildingType.Conveyor;

    public Direction InputDirection { get; private set; }
    public Direction OutputDirection { get; private set; }
    public bool IsCorner => InputDirection.Opposite() != OutputDirection;

    // Item currently on this conveyor
    private ConveyorItem _currentItem;
    private bool _hasItem = false;

    /// <summary>
    /// Special initializer for conveyors with explicit input/output directions.
    /// </summary>
    public void Initialize(Vector2Int gridPos, Direction inputDir, Direction outputDir)
    {
        InputDirection = inputDir;
        OutputDirection = outputDir;

        // Set facing to output direction for base class
        base.Initialize(gridPos, outputDir);

        // Override rotation and sprite to match conveyor visuals
        UpdateVisual();

        // Auto-connect: check if placing this conveyor should modify a neighbor
        AutoConnectNeighbors();
    }

    protected override void SetupPorts()
    {
        _ports.Clear();
        _ports.Add(new BuildingPort(PortType.Input, InputDirection));
        _ports.Add(new BuildingPort(PortType.Output, OutputDirection));
    }

    public override bool CanAcceptItem(Direction fromDirection)
    {
        return !_hasItem && fromDirection == InputDirection;
    }

    public override bool OnItemReceived(ItemType itemType, Direction fromDirection)
    {
        if (_hasItem) return false;
        if (fromDirection != InputDirection) return false;

        // Spawn visual item at our position
        _currentItem = ConveyorItem.Create(itemType, transform.position, this);
        _hasItem = true;
        return true;
    }

    private void Update()
    {
        if (!_hasItem || _currentItem == null) return;

        // Move item toward output
        _currentItem.Progress += Time.deltaTime; // 1 grid/sec

        // Lerp position
        Vector3 startPos = transform.position;
        Vector3 endPos = GridSystem.Instance.GridToWorld(GridPosition + OutputDirection.ToVector2Int());
        _currentItem.transform.position = Vector3.Lerp(startPos, endPos, _currentItem.Progress);

        // Item reached the end of this conveyor
        if (_currentItem.Progress >= 1f)
        {
            TryHandOffItem();
        }
    }

    private void TryHandOffItem()
    {
        Vector2Int nextPos = GridPosition + OutputDirection.ToVector2Int();
        BuildingBase neighbor = BuildingRegistry.GetAt(nextPos);

        if (neighbor != null)
        {
            Direction inputDir = OutputDirection.Opposite();
            if (neighbor.CanAcceptItem(inputDir))
            {
                ItemType type = _currentItem.Type;
                Destroy(_currentItem.gameObject);
                _currentItem = null;
                _hasItem = false;
                neighbor.OnItemReceived(type, inputDir);
                return;
            }
        }

        // Can't hand off — item stays at end position (blocked)
        _currentItem.Progress = 1f;
    }

    /// <summary>
    /// Updates sprite and rotation to match current input/output directions.
    /// </summary>
    public void UpdateVisual()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (!IsCorner)
        {
            // Straight piece: sprite default is "up", rotate to match output
            sr.sprite = SpriteFactory.GetConveyorStraight();
            transform.rotation = Quaternion.Euler(0, 0, OutputDirection.ToRotationZ());
        }
        else
        {
            // Corner piece: sprite default is "bottom input, right output"
            // We need to rotate and/or flip to match the actual in/out combo
            sr.sprite = SpriteFactory.GetConveyorCorner();
            float angle;
            bool flipX;
            GetCornerTransform(InputDirection, OutputDirection, out angle, out flipX);
            transform.rotation = Quaternion.Euler(0, 0, angle);
            sr.flipX = flipX;
        }
    }

    /// <summary>
    /// Calculates rotation and flip for corner pieces.
    /// Default corner sprite: input from Bottom (InputDirection=Down), output to Right.
    /// Outer rails on LEFT + TOP in default orientation.
    /// </summary>
    public static void GetCornerTransform(Direction inputDir, Direction outputDir, out float angle, out bool flipX)
    {
        // source = InputDirection = the side items come FROM
        // Default sprite: source=Down (bottom), output=Right
        Direction source = inputDir;
        flipX = false;
        angle = 0f;

        if (source == Direction.Down && outputDir == Direction.Right)
        { angle = 0f; flipX = false; }
        else if (source == Direction.Down && outputDir == Direction.Left)
        { angle = 0f; flipX = true; }
        else if (source == Direction.Right && outputDir == Direction.Up)
        { angle = 90f; flipX = false; }
        else if (source == Direction.Right && outputDir == Direction.Down)
        { angle = 90f; flipX = true; }
        else if (source == Direction.Up && outputDir == Direction.Left)
        { angle = 180f; flipX = false; }
        else if (source == Direction.Up && outputDir == Direction.Right)
        { angle = 180f; flipX = true; }
        else if (source == Direction.Left && outputDir == Direction.Down)
        { angle = -90f; flipX = false; }
        else if (source == Direction.Left && outputDir == Direction.Up)
        { angle = -90f; flipX = true; }
    }

    /// <summary>
    /// When a new conveyor is placed, check if the neighbor conveyor's END
    /// should auto-bend to connect to us.
    /// Only modifies the neighbor if it's a conveyor whose output points at us
    /// but its current output direction doesn't — i.e., the neighbor is at
    /// our InputDirection side and we can redirect its output toward us.
    /// </summary>
    private void AutoConnectNeighbors()
    {
        // Check the cell that our input comes from
        Vector2Int inputNeighborPos = GridPosition + InputDirection.ToVector2Int();
        BuildingBase inputNeighbor = BuildingRegistry.GetAt(inputNeighborPos);

        if (inputNeighbor is ConveyorBelt prevConveyor)
        {
            // If the previous conveyor's output doesn't currently point at us,
            // AND its output cell is empty (it's a dead end), redirect it.
            Direction dirTowardUs = InputDirection.Opposite();
            if (prevConveyor.OutputDirection != dirTowardUs)
            {
                // Check if the previous conveyor's current output target is empty
                Vector2Int prevOutputPos = prevConveyor.GridPosition + prevConveyor.OutputDirection.ToVector2Int();
                BuildingBase prevTarget = BuildingRegistry.GetAt(prevOutputPos);

                if (prevTarget == null)
                {
                    // Redirect: change the previous conveyor's output to point at us
                    prevConveyor.ChangeOutput(dirTowardUs);
                }
            }
        }
    }

    /// <summary>
    /// Changes the output direction (used by auto-connect).
    /// Updates ports and visual.
    /// </summary>
    public void ChangeOutput(Direction newOutput)
    {
        OutputDirection = newOutput;
        Facing = newOutput;
        _ports.Clear();
        _ports.Add(new BuildingPort(PortType.Input, InputDirection));
        _ports.Add(new BuildingPort(PortType.Output, OutputDirection));
        UpdateVisual();
        Debug.Log($"[Conveyor] Auto-bent at {GridPosition}: now {InputDirection}→{OutputDirection}");
    }

    public override void OnRemoved()
    {
        if (_currentItem != null)
        {
            Destroy(_currentItem.gameObject);
        }
        base.OnRemoved();
    }
}
