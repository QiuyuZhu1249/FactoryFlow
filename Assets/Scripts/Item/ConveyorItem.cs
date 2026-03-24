using UnityEngine;

/// <summary>
/// Visual representation of an item moving on a conveyor belt.
/// Small colored square matching ore type, moves at 1 grid/sec.
/// </summary>
public class ConveyorItem : MonoBehaviour
{
    public ItemType Type { get; private set; }
    public float Progress { get; set; } = 0f;

    private SpriteRenderer _renderer;

    /// <summary>
    /// Creates a conveyor item at the given position.
    /// </summary>
    public static ConveyorItem Create(ItemType type, Vector3 position, BuildingBase parentBuilding)
    {
        GameObject go = new GameObject($"Item_{type}");
        go.transform.position = position;
        go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.GetItemSprite(type);
        sr.color = GetColorForType(type);
        sr.sortingOrder = 10; // Above buildings

        ConveyorItem item = go.AddComponent<ConveyorItem>();
        item.Type = type;
        item._renderer = sr;

        return item;
    }

    public static Color GetColorForType(ItemType type)
    {
        switch (type)
        {
            // Raw ores
            case ItemType.IronOre:      return new Color(0.55f, 0.6f, 0.8f);    // Steel blue
            case ItemType.CopperOre:    return new Color(0.9f, 0.45f, 0.15f);   // Bright copper orange
            case ItemType.Coal:         return new Color(0.18f, 0.18f, 0.22f);   // Near black
            case ItemType.Stone:        return new Color(0.65f, 0.62f, 0.55f);   // Sandy gray

            // Ingots (shiny metallic)
            case ItemType.IronIngot:    return new Color(0.7f, 0.75f, 0.85f);   // Polished silver-blue
            case ItemType.CopperIngot:  return new Color(0.85f, 0.55f, 0.2f);   // Polished copper

            // Powders (muted/dusty)
            case ItemType.IronPowder:   return new Color(0.5f, 0.52f, 0.6f);    // Dusty gray-blue
            case ItemType.CopperPowder: return new Color(0.75f, 0.5f, 0.3f);    // Dusty copper

            // Processed
            case ItemType.Lime:         return new Color(0.92f, 0.92f, 0.85f);  // Off-white cream

            // Advanced (unique tints)
            case ItemType.BatteryCore:  return new Color(0.4f, 0.85f, 0.6f);    // Teal green
            case ItemType.BatteryShell: return new Color(0.6f, 0.6f, 0.65f);    // Metallic gray
            case ItemType.Battery:      return new Color(0.3f, 0.9f, 0.4f);     // Bright green

            default:                    return Color.white;
        }
    }

    private static Sprite _cachedSprite;

    private static Sprite GetDefaultSprite()
    {
        if (_cachedSprite != null) return _cachedSprite;

        // Create a simple white square texture
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        _cachedSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        return _cachedSprite;
    }
}
