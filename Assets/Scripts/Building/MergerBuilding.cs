using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Merger: up to 3 inputs (3 sides), 1 output (facing direction).
/// Alternates which input it pulls from.
/// </summary>
public class MergerBuilding : BuildingBase
{
    public override BuildingType Type => BuildingType.Merger;

    private int _inputIndex = 0;
    private Queue<ItemType>[] _inputQueues;
    private Direction[] _inputDirs;
    private float _transferTimer = 0f;
    private float _transferInterval = 0.5f;

    public override void Initialize(Vector2Int gridPos, Direction facing)
    {
        _inputDirs = new Direction[]
        {
            facing.Opposite(),
            facing.RotateCW(),
            facing.RotateCCW()
        };
        _inputQueues = new Queue<ItemType>[3];
        for (int i = 0; i < 3; i++)
            _inputQueues[i] = new Queue<ItemType>();

        base.Initialize(gridPos, facing);
    }

    protected override void SetupPorts()
    {
        _ports.Clear();
        // Output in facing direction
        _ports.Add(new BuildingPort(PortType.Output, Facing));
        // Inputs on the other 3 sides
        _ports.Add(new BuildingPort(PortType.Input, Facing.Opposite()));
        _ports.Add(new BuildingPort(PortType.Input, Facing.RotateCW()));
        _ports.Add(new BuildingPort(PortType.Input, Facing.RotateCCW()));
    }

    public override bool CanAcceptItem(Direction fromDirection)
    {
        for (int i = 0; i < _inputDirs.Length; i++)
        {
            if (_inputDirs[i] == fromDirection)
                return _inputQueues[i].Count < 1; // max 1 item queued per input
        }
        return false;
    }

    public override bool OnItemReceived(ItemType itemType, Direction fromDirection)
    {
        for (int i = 0; i < _inputDirs.Length; i++)
        {
            if (_inputDirs[i] == fromDirection && _inputQueues[i].Count < 1)
            {
                _inputQueues[i].Enqueue(itemType);
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        _transferTimer += Time.deltaTime;
        if (_transferTimer >= _transferInterval)
        {
            _transferTimer = 0f;
            TryOutputItem();
        }
    }

    private void TryOutputItem()
    {
        // Check if output neighbor can accept
        BuildingBase neighbor = GetNeighbor(Facing);
        if (neighbor == null || !neighbor.CanAcceptItem(Facing.Opposite())) return;

        // Try each input in round-robin order
        for (int attempt = 0; attempt < 3; attempt++)
        {
            int idx = (_inputIndex + attempt) % 3;
            if (_inputQueues[idx].Count > 0)
            {
                ItemType type = _inputQueues[idx].Dequeue();
                _inputIndex = (idx + 1) % 3;
                neighbor.OnItemReceived(type, Facing.Opposite());
                return;
            }
        }
    }
}
