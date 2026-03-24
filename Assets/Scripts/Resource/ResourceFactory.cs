using UnityEngine;

public class ResourceFactory : MonoBehaviour
{
    [Header("Resource Prefabs")]
    [SerializeField] private GameObject _ironOrePrefab;
    [SerializeField] private GameObject _copperOrePrefab;
    [SerializeField] private GameObject _coalPrefab;
    [SerializeField] private GameObject _stonePrefab;

    public GameObject CreateResource(ItemType type, Vector3 position)
    {
        GameObject prefab = GetPrefab(type);

        if (prefab == null)
        {
            Debug.LogWarning($"[ResourceFactory] No prefab assigned for {type}.");
            return null;
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.name = $"{type}_{instance.GetInstanceID()}";

        Debug.Log($"[ResourceFactory] Created {type} at {position}.");
        return instance;
    }

    private GameObject GetPrefab(ItemType type)
    {
        switch (type)
        {
            case ItemType.IronOre:   return _ironOrePrefab;
            case ItemType.CopperOre: return _copperOrePrefab;
            case ItemType.Coal:      return _coalPrefab;
            case ItemType.Stone:     return _stonePrefab;
            default:                 return null;
        }
    }
}
