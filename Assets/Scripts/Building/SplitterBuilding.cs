using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Splitter: 1 input (back of facing), up to 3 outputs (other 3 sides).
/// Distributes items round-robin among connected outputs.
/// </summary>
public class SplitterBuilding : BuildingBase
{
    public override BuildingType Type => BuildingType.Splitter;

    private int _outputIndex = 0;
    private ItemType _heldItemType = ItemType.None;
    private bool _hasItem = false;
    private float _transferTimer = 0f;
    private float _transferInterval = 0.3f;

    protected override void SetupPorts()
    {
        _ports.Clear();
        // Input from behind (opposite of facing)
        _ports.Add(new BuildingPort(PortType.Input, Facing.Opposite()));
        // Outputs on the other 3 sides
        _ports.Add(new BuildingPort(PortType.Output, Facing));
        _ports.Add(new BuildingPort(PortType.Output, Facing.RotateCW()));
        _ports.Add(new BuildingPort(PortType.Output, Facing.RotateCCW()));
    }

    public override bool CanAcceptItem(Direction fromDirection)
    {
        // Accept from input side only
        if (_hasItem) return false;
        return fromDirection == Facing.Opposite();
    }

    public override bool OnItemReceived(ItemType itemType, Direction fromDirection)
    {
        if (_hasItem) return false;
        if (fromDirection != Facing.Opposite()) return false;

        _heldItemType = itemType;
        _hasItem = true;
        Debug.Log($"[Splitter] Received {itemType} from {fromDirection}, facing={Facing}");
        return true;
    }

    private void Update()
    {
        if (!_hasItem) return;

        _transferTimer += Time.deltaTime;
        if (_transferTimer >= _transferInterval)
        {
            _transferTimer = 0f;
            TryDistributeItem();
        }
    }

    private void TryDistributeItem()
    {
        // Get output directions that have a connected building that can accept
        List<Direction> activeOutputs = new List<Direction>();
        Direction[] outputDirs = { Facing, Facing.RotateCW(), Facing.RotateCCW() };

        foreach (var dir in outputDirs)
        {
            BuildingBase neighbor = GetNeighbor(dir);
            if (neighbor != null)
            {
                Direction fromDir = dir.Opposite();
                if (neighbor.CanAcceptItem(fromDir))
                {
                    activeOutputs.Add(dir);
                }
            }
        }

        if (activeOutputs.Count == 0)
        {
            return;
        }

        // Round-robin among available outputs
        _outputIndex = _outputIndex % activeOutputs.Count;
        Direction outputDir = activeOutputs[_outputIndex];
        _outputIndex = (_outputIndex + 1) % activeOutputs.Count;

        BuildingBase target = GetNeighbor(outputDir);
        ItemType type = _heldItemType;
        _heldItemType = ItemType.None;
        _hasItem = false;

        target.OnItemReceived(type, outputDir.Opposite());
        Debug.Log($"[Splitter] Sent {type} to {outputDir}");
    }
}
