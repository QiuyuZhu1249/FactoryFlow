using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private Dictionary<ItemType, int> _inventory = new Dictionary<ItemType, int>();

    private void Awake()
    {
        // Initialize all resource types to 0
        _inventory[ItemType.IronOre] = 0;
        _inventory[ItemType.CopperOre] = 0;
        _inventory[ItemType.Coal] = 0;
        _inventory[ItemType.Stone] = 0;
    }

    public void AddResource(ItemType type, int amount)
    {
        if (type == ItemType.None || amount <= 0) return;

        _inventory[type] += amount;
        Debug.Log($"[ResourceManager] Added {amount} {type}. Total: {_inventory[type]}");
    }

    public bool RemoveResource(ItemType type, int amount)
    {
        if (type == ItemType.None || amount <= 0) return false;

        if (!HasEnoughResource(type, amount))
        {
            Debug.LogWarning($"[ResourceManager] Not enough {type}. Have: {_inventory[type]}, Need: {amount}");
            return false;
        }

        _inventory[type] -= amount;
        Debug.Log($"[ResourceManager] Removed {amount} {type}. Remaining: {_inventory[type]}");
        return true;
    }

    public int GetResourceCount(ItemType type)
    {
        if (_inventory.TryGetValue(type, out int count))
            return count;
        return 0;
    }

    public bool HasEnoughResource(ItemType type, int amount)
    {
        return GetResourceCount(type) >= amount;
    }

    public Dictionary<ItemType, int> GetAllResources()
    {
        return new Dictionary<ItemType, int>(_inventory);
    }
}
