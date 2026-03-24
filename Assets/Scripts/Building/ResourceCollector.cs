using UnityEngine;

/// <summary>
/// Resource collector: accepts items from all 4 directions.
/// Converts each ore to $1. Auto-spawns at map center.
/// </summary>
public class ResourceCollector : BuildingBase
{
    public override BuildingType Type => BuildingType.ResourceCollector;

    protected override void SetupPorts()
    {
        _ports.Clear();
        _ports.Add(new BuildingPort(PortType.Input, Direction.Up));
        _ports.Add(new BuildingPort(PortType.Input, Direction.Right));
        _ports.Add(new BuildingPort(PortType.Input, Direction.Down));
        _ports.Add(new BuildingPort(PortType.Input, Direction.Left));
    }

    public override bool CanAcceptItem(Direction fromDirection)
    {
        // Always accept from any direction
        return true;
    }

    public override bool OnItemReceived(ItemType itemType, Direction fromDirection)
    {
        // Convert item to money based on its value
        int value = RecipeDatabase.GetSellValue(itemType);
        MoneyManager.Instance.AddMoney(value);
        Debug.Log($"[ResourceCollector] Sold {itemType} for ${value}");
        return true;
    }

    /// <summary>
    /// Collector cannot be removed by the player.
    /// </summary>
    public override void OnRemoved()
    {
        Debug.LogWarning("[ResourceCollector] Cannot remove the collector!");
    }
}
