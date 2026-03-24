using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all placeable buildings.
/// Handles grid registration, port definitions, and lifecycle.
/// </summary>
public abstract class BuildingBase : MonoBehaviour
{
    public Vector2Int GridPosition { get; protected set; }
    public Direction Facing { get; protected set; }
    public abstract BuildingType Type { get; }

    protected List<BuildingPort> _ports = new List<BuildingPort>();

    /// <summary>
    /// Called after instantiation to set up the building on the grid.
    /// </summary>
    public virtual void Initialize(Vector2Int gridPos, Direction facing)
    {
        GridPosition = gridPos;
        Facing = facing;

        // Set world position
        Vector3 worldPos = GridSystem.Instance.GridToWorld(gridPos);
        transform.position = worldPos;

        // Set rotation
        transform.rotation = Quaternion.Euler(0, 0, facing.ToRotationZ());

        // Occupy cell and register
        GridSystem.Instance.OccupyCell(gridPos);
        BuildingRegistry.Register(gridPos, this);

        // Set up ports (subclass defines these)
        SetupPorts();

        Debug.Log($"[{Type}] Placed at {gridPos}, facing {facing}.");
    }

    /// <summary>
    /// Override to define input/output ports based on Facing direction.
    /// </summary>
    protected abstract void SetupPorts();

    /// <summary>
    /// Called when an item arrives at this building from a neighbor.
    /// </summary>
    public virtual bool OnItemReceived(ItemType itemType, Direction fromDirection)
    {
        return false;
    }

    /// <summary>
    /// Checks if this building has an output port facing the given direction.
    /// </summary>
    public bool HasOutputFacing(Direction dir)
    {
        foreach (var port in _ports)
        {
            if (port.Type == PortType.Output && port.Direction == dir)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if this building has an input port facing the given direction.
    /// </summary>
    public bool HasInputFacing(Direction dir)
    {
        foreach (var port in _ports)
        {
            if (port.Type == PortType.Input && port.Direction == dir)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if this building can currently accept an item from the given direction.
    /// Override for backpressure logic.
    /// </summary>
    public virtual bool CanAcceptItem(Direction fromDirection)
    {
        return HasInputFacing(fromDirection);
    }

    /// <summary>
    /// Called when the building is removed by the player.
    /// </summary>
    public virtual void OnRemoved()
    {
        GridSystem.Instance.FreeCell(GridPosition);
        BuildingRegistry.Unregister(GridPosition);
        Debug.Log($"[{Type}] Removed from {GridPosition}.");
        Destroy(gameObject);
    }

    /// <summary>
    /// Returns the neighbor building in the given direction, if any.
    /// </summary>
    protected BuildingBase GetNeighbor(Direction dir)
    {
        Vector2Int neighborPos = GridPosition + dir.ToVector2Int();
        return BuildingRegistry.GetAt(neighborPos);
    }
}
