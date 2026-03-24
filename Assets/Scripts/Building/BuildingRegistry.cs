using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static registry mapping grid positions to building instances.
/// No connection graph — buildings query neighbors at runtime.
/// </summary>
public static class BuildingRegistry
{
    private static Dictionary<Vector2Int, BuildingBase> _buildings
        = new Dictionary<Vector2Int, BuildingBase>();

    public static void Register(Vector2Int pos, BuildingBase building)
    {
        _buildings[pos] = building;
    }

    public static void Unregister(Vector2Int pos)
    {
        _buildings.Remove(pos);
    }

    public static BuildingBase GetAt(Vector2Int pos)
    {
        _buildings.TryGetValue(pos, out BuildingBase building);
        return building;
    }

    public static bool HasBuildingAt(Vector2Int pos)
    {
        return _buildings.ContainsKey(pos);
    }

    /// <summary>
    /// Clears all entries. Call on game reset.
    /// </summary>
    public static void Clear()
    {
        _buildings.Clear();
    }
}
