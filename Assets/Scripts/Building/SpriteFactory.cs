using UnityEngine;

/// <summary>
/// Runtime procedural sprite generator for all building types.
/// 64x64 pixel art with clear directional indicators.
/// Green arrow = output, Orange arrow = input.
/// Default orientation: output faces UP.
/// </summary>
public static class SpriteFactory
{
    private const int SIZE = 64;

    // Cached sprites
    private static Sprite _miner;
    private static Sprite _conveyorStraight;
    private static Sprite _conveyorCorner;
    private static Sprite _splitter;
    private static Sprite _merger;
    private static Sprite _productionStation;
    private static Sprite _collector;
    private static Sprite _square;
    private static Sprite _itemOre;
    private static Sprite _itemIngot;
    private static Sprite _itemPowder;
    private static Sprite _itemLime;
    private static Sprite _itemBatteryCore;
    private static Sprite _itemBatteryShell;
    private static Sprite _itemBattery;

    // Colors
    private static readonly Color COL_OUTPUT = new Color(0.2f, 0.85f, 0.3f);
    private static readonly Color COL_INPUT = new Color(0.9f, 0.6f, 0.15f);

    public static Sprite GetMiner()
    {
        if (_miner != null) return _miner;
        _miner = CreateMinerSprite();
        return _miner;
    }

    public static Sprite GetConveyorStraight()
    {
        if (_conveyorStraight != null) return _conveyorStraight;
        _conveyorStraight = CreateConveyorStraightSprite();
        return _conveyorStraight;
    }

    public static Sprite GetConveyorCorner()
    {
        if (_conveyorCorner != null) return _conveyorCorner;
        _conveyorCorner = CreateConveyorCornerSprite();
        return _conveyorCorner;
    }

    public static Sprite GetSplitter()
    {
        if (_splitter != null) return _splitter;
        _splitter = CreateSplitterSprite();
        return _splitter;
    }

    public static Sprite GetMerger()
    {
        if (_merger != null) return _merger;
        _merger = CreateMergerSprite();
        return _merger;
    }

    public static Sprite GetProductionStation()
    {
        if (_productionStation != null) return _productionStation;
        _productionStation = CreateProductionStationSprite();
        return _productionStation;
    }

    public static Sprite GetCollector()
    {
        if (_collector != null) return _collector;
        _collector = CreateCollectorSprite();
        return _collector;
    }

    public static Sprite GetSquare()
    {
        if (_square != null) return _square;
        Texture2D tex = new Texture2D(4, 4);
        Fill(tex, 4, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        _square = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        return _square;
    }

    /// <summary>
    /// Returns the appropriate item sprite for the given ItemType.
    /// Each category has a distinct silhouette for visual clarity.
    /// </summary>
    public static Sprite GetItemSprite(ItemType type = ItemType.IronOre)
    {
        switch (type)
        {
            case ItemType.IronOre:
            case ItemType.CopperOre:
            case ItemType.Coal:
            case ItemType.Stone:
                return GetItemOreSprite();

            case ItemType.IronIngot:
            case ItemType.CopperIngot:
                return GetItemIngotSprite();

            case ItemType.IronPowder:
            case ItemType.CopperPowder:
                return GetItemPowderSprite();

            case ItemType.Lime:
                return GetItemLimeSprite();

            case ItemType.BatteryCore:
                return GetItemBatteryCoreSprite();

            case ItemType.BatteryShell:
                return GetItemBatteryShellSprite();

            case ItemType.Battery:
                return GetItemBatterySprite();

            default:
                return GetItemOreSprite();
        }
    }

    // ========== Item: Ore Nugget (rounded rectangle) ==========
    private static Sprite GetItemOreSprite()
    {
        if (_itemOre != null) return _itemOre;
        int s = 16;
        Texture2D tex = new Texture2D(s, s);
        FillSmall(tex, s, Color.clear);

        Color border = new Color(0.25f, 0.25f, 0.25f);
        Color body = Color.white;
        Color highlight = new Color(1f, 1f, 1f, 0.9f);
        Color shadow = new Color(0.6f, 0.6f, 0.6f);

        // Rounded rectangle body
        for (int y = 2; y < 14; y++)
            for (int x = 3; x < 13; x++)
                tex.SetPixel(x, y, body);
        for (int y = 3; y < 13; y++)
        {
            tex.SetPixel(2, y, body);
            tex.SetPixel(13, y, body);
        }

        // Border
        for (int x = 3; x < 13; x++) { tex.SetPixel(x, 1, border); tex.SetPixel(x, 14, border); }
        for (int y = 3; y < 13; y++) { tex.SetPixel(1, y, border); tex.SetPixel(14, y, border); }
        tex.SetPixel(2, 2, border); tex.SetPixel(13, 2, border);
        tex.SetPixel(2, 13, border); tex.SetPixel(13, 13, border);

        // Highlight (top-left)
        for (int x = 4; x < 7; x++) tex.SetPixel(x, 11, highlight);
        tex.SetPixel(4, 10, highlight); tex.SetPixel(5, 10, highlight);
        tex.SetPixel(4, 9, highlight);

        // Shadow (bottom-right)
        for (int x = 9; x < 12; x++) tex.SetPixel(x, 3, shadow);
        tex.SetPixel(11, 4, shadow); tex.SetPixel(12, 4, shadow);
        tex.SetPixel(12, 5, shadow);

        _itemOre = FinalizeSmall(tex, s);
        return _itemOre;
    }

    // ========== Item: Ingot (flat rectangular bar with bevel) ==========
    private static Sprite GetItemIngotSprite()
    {
        if (_itemIngot != null) return _itemIngot;
        int s = 16;
        Texture2D tex = new Texture2D(s, s);
        FillSmall(tex, s, Color.clear);

        Color border = new Color(0.25f, 0.25f, 0.25f);
        Color body = Color.white;
        Color highlight = new Color(1f, 1f, 1f, 0.9f);
        Color shadow = new Color(0.55f, 0.55f, 0.55f);

        // Flat wide bar shape (wider than tall)
        for (int y = 4; y < 12; y++)
            for (int x = 1; x < 15; x++)
                tex.SetPixel(x, y, body);

        // Border
        for (int x = 1; x < 15; x++) { tex.SetPixel(x, 3, border); tex.SetPixel(x, 12, border); }
        for (int y = 4; y < 12; y++) { tex.SetPixel(0, y, border); tex.SetPixel(15, y, border); }
        tex.SetPixel(0, 3, border); tex.SetPixel(15, 3, border);
        tex.SetPixel(0, 12, border); tex.SetPixel(15, 12, border);

        // Top bevel highlight
        for (int x = 2; x < 14; x++) tex.SetPixel(x, 11, highlight);
        for (int x = 2; x < 6; x++) tex.SetPixel(x, 10, highlight);

        // Bottom shadow
        for (int x = 2; x < 14; x++) tex.SetPixel(x, 4, shadow);
        for (int x = 10; x < 14; x++) tex.SetPixel(x, 5, shadow);

        // Center line mark (trapezoid stamp)
        tex.SetPixel(7, 7, border); tex.SetPixel(8, 7, border);
        tex.SetPixel(7, 8, border); tex.SetPixel(8, 8, border);

        _itemIngot = FinalizeSmall(tex, s);
        return _itemIngot;
    }

    // ========== Item: Powder (scattered dots pile) ==========
    private static Sprite GetItemPowderSprite()
    {
        if (_itemPowder != null) return _itemPowder;
        int s = 16;
        Texture2D tex = new Texture2D(s, s);
        FillSmall(tex, s, Color.clear);

        Color body = Color.white;
        Color shadow = new Color(0.65f, 0.65f, 0.65f);

        // Triangular pile shape — wider at bottom
        // Row 1 (bottom, y=2): wide
        for (int x = 3; x < 13; x++) tex.SetPixel(x, 2, shadow);
        // Row 2 (y=3)
        for (int x = 3; x < 13; x++) tex.SetPixel(x, 3, body);
        // Row 3 (y=4)
        for (int x = 4; x < 12; x++) tex.SetPixel(x, 4, body);
        // Row 4 (y=5)
        for (int x = 4; x < 12; x++) tex.SetPixel(x, 5, body);
        // Row 5 (y=6)
        for (int x = 5; x < 11; x++) tex.SetPixel(x, 6, body);
        // Row 6 (y=7)
        for (int x = 5; x < 11; x++) tex.SetPixel(x, 7, body);
        // Row 7 (y=8)
        for (int x = 6; x < 10; x++) tex.SetPixel(x, 8, body);
        // Row 8 (y=9) - peak
        for (int x = 7; x < 9; x++) tex.SetPixel(x, 9, body);

        // Scattered loose particles above
        tex.SetPixel(5, 10, body);
        tex.SetPixel(10, 9, body);
        tex.SetPixel(8, 11, body);
        tex.SetPixel(3, 4, body);
        tex.SetPixel(12, 3, body);

        // Highlight on top-left slope
        tex.SetPixel(6, 8, new Color(1f, 1f, 1f, 0.9f));
        tex.SetPixel(6, 7, new Color(1f, 1f, 1f, 0.9f));
        tex.SetPixel(5, 6, new Color(1f, 1f, 1f, 0.9f));

        _itemPowder = FinalizeSmall(tex, s);
        return _itemPowder;
    }

    // ========== Item: Lime (angular crystal/chunk) ==========
    private static Sprite GetItemLimeSprite()
    {
        if (_itemLime != null) return _itemLime;
        int s = 16;
        Texture2D tex = new Texture2D(s, s);
        FillSmall(tex, s, Color.clear);

        Color border = new Color(0.3f, 0.3f, 0.3f);
        Color body = Color.white;
        Color highlight = new Color(1f, 1f, 1f, 0.95f);
        Color shadow = new Color(0.6f, 0.6f, 0.6f);

        // Angular pentagon / crystal chunk shape
        // Bottom row
        for (int x = 4; x < 13; x++) tex.SetPixel(x, 2, border);
        // Body rows
        for (int y = 3; y < 6; y++)
            for (int x = 3; x < 13; x++) tex.SetPixel(x, y, body);
        for (int y = 6; y < 9; y++)
            for (int x = 3; x < 12; x++) tex.SetPixel(x, y, body);
        for (int y = 9; y < 11; y++)
            for (int x = 4; x < 11; x++) tex.SetPixel(x, y, body);
        // Top peak
        for (int x = 5; x < 10; x++) tex.SetPixel(x, 11, body);
        for (int x = 6; x < 9; x++) tex.SetPixel(x, 12, body);

        // Left border
        tex.SetPixel(2, 3, border); tex.SetPixel(2, 4, border); tex.SetPixel(2, 5, border);
        tex.SetPixel(2, 6, border); tex.SetPixel(2, 7, border); tex.SetPixel(2, 8, border);
        tex.SetPixel(3, 9, border); tex.SetPixel(3, 10, border);
        tex.SetPixel(4, 11, border); tex.SetPixel(5, 12, border);

        // Right border
        tex.SetPixel(13, 3, border); tex.SetPixel(13, 4, border); tex.SetPixel(13, 5, border);
        tex.SetPixel(12, 6, border); tex.SetPixel(12, 7, border); tex.SetPixel(12, 8, border);
        tex.SetPixel(11, 9, border); tex.SetPixel(11, 10, border);
        tex.SetPixel(10, 11, border); tex.SetPixel(9, 12, border);

        // Crack line (diagonal)
        tex.SetPixel(6, 4, shadow); tex.SetPixel(7, 5, shadow);
        tex.SetPixel(8, 6, shadow); tex.SetPixel(8, 7, shadow);

        // Highlight facet
        for (int x = 4; x < 7; x++) tex.SetPixel(x, 9, highlight);
        tex.SetPixel(4, 8, highlight); tex.SetPixel(5, 8, highlight);

        _itemLime = FinalizeSmall(tex, s);
        return _itemLime;
    }

    // ========== Item: Battery Core (circle with cross/plus) ==========
    private static Sprite GetItemBatteryCoreSprite()
    {
        if (_itemBatteryCore != null) return _itemBatteryCore;
        int s = 16;
        Texture2D tex = new Texture2D(s, s);
        FillSmall(tex, s, Color.clear);

        Color border = new Color(0.2f, 0.2f, 0.25f);
        Color body = Color.white;
        Color cross = new Color(0.3f, 0.3f, 0.35f);
        Color glow = new Color(1f, 1f, 0.7f, 0.8f);

        // Circle body (radius ~5)
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - 7.5f, dy = y - 7.5f;
                float dist = dx * dx + dy * dy;
                if (dist <= 25f) // r=5
                    tex.SetPixel(x, y, body);
                else if (dist <= 36f) // r=6 border ring
                    tex.SetPixel(x, y, border);
            }

        // Cross / plus pattern in center
        for (int i = 4; i < 12; i++) { tex.SetPixel(i, 7, cross); tex.SetPixel(i, 8, cross); }
        for (int i = 4; i < 12; i++) { tex.SetPixel(7, i, cross); tex.SetPixel(8, i, cross); }

        // Center glow dot
        tex.SetPixel(7, 7, glow); tex.SetPixel(8, 7, glow);
        tex.SetPixel(7, 8, glow); tex.SetPixel(8, 8, glow);

        _itemBatteryCore = FinalizeSmall(tex, s);
        return _itemBatteryCore;
    }

    // ========== Item: Battery Shell (rectangular frame/outline) ==========
    private static Sprite GetItemBatteryShellSprite()
    {
        if (_itemBatteryShell != null) return _itemBatteryShell;
        int s = 16;
        Texture2D tex = new Texture2D(s, s);
        FillSmall(tex, s, Color.clear);

        Color border = new Color(0.25f, 0.25f, 0.25f);
        Color body = Color.white;
        Color inner = new Color(0.85f, 0.85f, 0.85f);

        // Outer rectangle
        for (int y = 2; y < 13; y++)
            for (int x = 3; x < 13; x++)
                tex.SetPixel(x, y, body);

        // Terminal bump on top
        for (int x = 6; x < 10; x++) tex.SetPixel(x, 13, body);
        for (int x = 6; x < 10; x++) tex.SetPixel(x, 14, border);
        tex.SetPixel(5, 13, border); tex.SetPixel(10, 13, border);

        // Hollow out center (frame only)
        for (int y = 4; y < 11; y++)
            for (int x = 5; x < 11; x++)
                tex.SetPixel(x, y, inner);

        // Border outline
        for (int x = 3; x < 13; x++) { tex.SetPixel(x, 1, border); tex.SetPixel(x, 13, border); }
        for (int y = 2; y < 13; y++) { tex.SetPixel(2, y, border); tex.SetPixel(13, y, border); }

        // Corner rivets
        tex.SetPixel(4, 3, border); tex.SetPixel(11, 3, border);
        tex.SetPixel(4, 11, border); tex.SetPixel(11, 11, border);

        _itemBatteryShell = FinalizeSmall(tex, s);
        return _itemBatteryShell;
    }

    // ========== Item: Battery (full battery with terminal + charge lines) ==========
    private static Sprite GetItemBatterySprite()
    {
        if (_itemBattery != null) return _itemBattery;
        int s = 16;
        Texture2D tex = new Texture2D(s, s);
        FillSmall(tex, s, Color.clear);

        Color border = new Color(0.2f, 0.2f, 0.22f);
        Color body = Color.white;
        Color charge = new Color(0.7f, 1f, 0.5f);   // green charge indicator
        Color highlight = new Color(1f, 1f, 1f, 0.9f);

        // Main body
        for (int y = 1; y < 12; y++)
            for (int x = 3; x < 13; x++)
                tex.SetPixel(x, y, body);

        // Terminal on top
        for (int x = 5; x < 11; x++) tex.SetPixel(x, 12, body);
        for (int x = 6; x < 10; x++) tex.SetPixel(x, 13, body);

        // Border
        for (int x = 3; x < 13; x++) { tex.SetPixel(x, 0, border); tex.SetPixel(x, 12, border); }
        for (int y = 1; y < 12; y++) { tex.SetPixel(2, y, border); tex.SetPixel(13, y, border); }
        for (int x = 5; x < 11; x++) tex.SetPixel(x, 12, border);
        for (int x = 6; x < 10; x++) tex.SetPixel(x, 14, border);
        tex.SetPixel(4, 12, border); tex.SetPixel(11, 12, border);
        tex.SetPixel(5, 13, border); tex.SetPixel(10, 13, border);

        // Charge level bars (3 horizontal bars inside)
        for (int x = 5; x < 11; x++) tex.SetPixel(x, 3, charge);
        for (int x = 5; x < 11; x++) tex.SetPixel(x, 5, charge);
        for (int x = 5; x < 11; x++) tex.SetPixel(x, 7, charge);

        // Lightning bolt hint in center
        tex.SetPixel(8, 9, charge); tex.SetPixel(7, 8, charge);
        tex.SetPixel(8, 8, charge); tex.SetPixel(7, 7, charge);

        // Top highlight
        for (int x = 4; x < 7; x++) tex.SetPixel(x, 10, highlight);

        _itemBattery = FinalizeSmall(tex, s);
        return _itemBattery;
    }

    private static void FillSmall(Texture2D tex, int size, Color color)
    {
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
    }

    private static Sprite FinalizeSmall(Texture2D tex, int size)
    {
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite GetSpriteForType(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Miner:             return GetMiner();
            case BuildingType.Conveyor:           return GetConveyorStraight();
            case BuildingType.Splitter:           return GetSplitter();
            case BuildingType.Merger:             return GetMerger();
            case BuildingType.ProductionStation:  return GetProductionStation();
            case BuildingType.ResourceCollector:  return GetCollector();
            default:                              return GetSquare();
        }
    }

    // ========== Miner ==========
    // Brown body, pickaxe icon, green output arrow on top
    private static Sprite CreateMinerSprite()
    {
        Texture2D tex = new Texture2D(SIZE, SIZE);
        Color body = new Color(0.45f, 0.3f, 0.15f);
        Color bodyLight = new Color(0.55f, 0.38f, 0.2f);
        Color bodyDark = new Color(0.35f, 0.22f, 0.1f);
        Color metal = new Color(0.75f, 0.75f, 0.8f);
        Color handle = new Color(0.5f, 0.35f, 0.15f);

        // Body fill with border
        FillWithBorder(tex, body, bodyDark, 2);

        // Inner panel (lighter area)
        FillRect(tex, 6, 6, 58, 52, bodyLight);

        // Pickaxe head (metallic V shape at top-center)
        // Left blade
        for (int i = 0; i < 12; i++)
        {
            DrawThickPixel(tex, 20 + i, 38 - i, metal, 2);
        }
        // Right blade
        for (int i = 0; i < 12; i++)
        {
            DrawThickPixel(tex, 44 - i, 38 - i, metal, 2);
        }
        // Handle (vertical down from center)
        FillRect(tex, 30, 12, 34, 30, handle);

        // Output arrow (green, top edge)
        DrawOutputArrow(tex, 0, COL_OUTPUT);

        return Finalize(tex);
    }

    // ========== Conveyor Straight ==========
    // Gray rails with chevron arrows pointing up
    private static Sprite CreateConveyorStraightSprite()
    {
        Texture2D tex = new Texture2D(SIZE, SIZE);
        Color bg = new Color(0.3f, 0.3f, 0.33f);
        Color rail = new Color(0.22f, 0.22f, 0.25f);
        Color belt = new Color(0.4f, 0.4f, 0.43f);
        Color arrow = new Color(0.55f, 0.6f, 0.65f);

        Fill(tex, SIZE, bg);

        // Left and right rails
        FillRect(tex, 0, 0, 8, SIZE, rail);
        FillRect(tex, SIZE - 8, 0, SIZE, SIZE, rail);

        // Belt surface (center)
        FillRect(tex, 8, 0, SIZE - 8, SIZE, belt);

        // Chevron arrows pointing up (^ shape, 3 chevrons)
        for (int c = 0; c < 3; c++)
        {
            int tipY = 18 + c * 18; // tip of the ^ at top
            for (int i = 0; i < 8; i++)
            {
                int lx = 32 - i - 1;
                int rx = 32 + i;
                int y = tipY - i; // spread downward from tip
                if (y >= 0 && lx >= 8 && rx < SIZE - 8)
                {
                    DrawThickPixel(tex, lx, y, arrow, 1);
                    DrawThickPixel(tex, rx, y, arrow, 1);
                }
            }
        }

        // Rail bolts (small dots)
        for (int y = 6; y < SIZE; y += 16)
        {
            FillRect(tex, 2, y, 6, y + 3, new Color(0.35f, 0.35f, 0.38f));
            FillRect(tex, SIZE - 6, y, SIZE - 2, y + 3, new Color(0.35f, 0.35f, 0.38f));
        }

        return Finalize(tex);
    }

    // ========== Conveyor Corner ==========
    // Default: input from Bottom, output to Right.
    // Dead simple: entire tile is belt, outer 2 edges are rails.
    private static Sprite CreateConveyorCornerSprite()
    {
        Texture2D tex = new Texture2D(SIZE, SIZE);
        Color rail = new Color(0.22f, 0.22f, 0.25f);
        Color belt = new Color(0.4f, 0.4f, 0.43f);
        Color arrow = new Color(0.55f, 0.6f, 0.65f);
        Color bolt = new Color(0.35f, 0.35f, 0.38f);
        int W = 8;

        // Entire tile = belt
        Fill(tex, SIZE, belt);

        // Outer two rails: LEFT wall + TOP wall
        FillRect(tex, 0, 0, W, SIZE, rail);
        FillRect(tex, 0, SIZE - W, SIZE, SIZE, rail);

        // Bolts
        for (int y = 10; y < SIZE - W; y += 16)
            FillRect(tex, 2, y, 6, y + 3, bolt);
        for (int x = W + 2; x < SIZE; x += 16)
            FillRect(tex, x, SIZE - 6, x + 3, SIZE - 2, bolt);

        // Arrow: L-shaped flow (bottom → right)
        int m = 32;
        FillRect(tex, m - 1, W, m + 2, m, arrow);
        FillRect(tex, m, m - 1, SIZE - W, m + 2, arrow);

        return Finalize(tex);
    }

    // ========== Splitter ==========
    // Blue body, 1 input (bottom/orange), 3 outputs (top/left/right/green)
    private static Sprite CreateSplitterSprite()
    {
        Texture2D tex = new Texture2D(SIZE, SIZE);
        Color body = new Color(0.18f, 0.3f, 0.55f);
        Color bodyDark = new Color(0.12f, 0.2f, 0.4f);
        Color bodyLight = new Color(0.25f, 0.4f, 0.65f);

        FillWithBorder(tex, body, bodyDark, 2);
        FillRect(tex, 6, 6, 58, 58, bodyLight);

        // Center hub (circle-ish)
        FillCircle(tex, 32, 32, 6, Color.white);

        // Input arrow bottom (orange) — pointing INTO the building
        DrawInputArrow(tex, 2, COL_INPUT);  // Direction.Down = 2

        // Output arrows (green) — top, right, left
        DrawOutputArrow(tex, 0, COL_OUTPUT); // Up
        DrawOutputArrow(tex, 1, COL_OUTPUT); // Right
        DrawOutputArrow(tex, 3, COL_OUTPUT); // Left

        // Lines from center to edges
        FillRect(tex, 30, 38, 34, 56, bodyDark); // up
        FillRect(tex, 30, 8, 34, 26, bodyDark);  // down
        FillRect(tex, 38, 30, 56, 34, bodyDark);  // right
        FillRect(tex, 8, 30, 26, 34, bodyDark);   // left

        return Finalize(tex);
    }

    // ========== Merger ==========
    // Green body, 3 inputs (bottom/left/right/orange), 1 output (top/green)
    private static Sprite CreateMergerSprite()
    {
        Texture2D tex = new Texture2D(SIZE, SIZE);
        Color body = new Color(0.15f, 0.4f, 0.25f);
        Color bodyDark = new Color(0.1f, 0.28f, 0.18f);
        Color bodyLight = new Color(0.2f, 0.5f, 0.32f);

        FillWithBorder(tex, body, bodyDark, 2);
        FillRect(tex, 6, 6, 58, 58, bodyLight);

        // Center hub
        FillCircle(tex, 32, 32, 6, Color.white);

        // Output arrow top (green)
        DrawOutputArrow(tex, 0, COL_OUTPUT);

        // Input arrows (orange) — bottom, left, right
        DrawInputArrow(tex, 2, COL_INPUT);  // Down
        DrawInputArrow(tex, 1, COL_INPUT);  // Right
        DrawInputArrow(tex, 3, COL_INPUT);  // Left

        // Lines from center to edges
        FillRect(tex, 30, 38, 34, 56, bodyDark);
        FillRect(tex, 30, 8, 34, 26, bodyDark);
        FillRect(tex, 38, 30, 56, 34, bodyDark);
        FillRect(tex, 8, 30, 26, 34, bodyDark);

        return Finalize(tex);
    }

    // ========== Production Station ==========
    // Yellow/orange body, gear icon, 2 inputs (sides), 1 output (top)
    private static Sprite CreateProductionStationSprite()
    {
        Texture2D tex = new Texture2D(SIZE, SIZE);
        Color body = new Color(0.55f, 0.4f, 0.12f);
        Color bodyDark = new Color(0.4f, 0.3f, 0.08f);
        Color bodyLight = new Color(0.65f, 0.5f, 0.18f);
        Color gear = new Color(0.85f, 0.78f, 0.5f);

        FillWithBorder(tex, body, bodyDark, 2);
        FillRect(tex, 6, 6, 58, 58, bodyLight);

        // Gear shape
        FillCircle(tex, 32, 32, 10, gear);
        FillCircle(tex, 32, 32, 5, bodyLight); // hollow center
        FillCircle(tex, 32, 32, 2, gear);      // center dot

        // Gear teeth (8 teeth)
        int cx = 32, cy = 32, tr = 12;
        for (int angle = 0; angle < 360; angle += 45)
        {
            float rad = angle * Mathf.Deg2Rad;
            int tx = cx + Mathf.RoundToInt(Mathf.Cos(rad) * tr);
            int ty = cy + Mathf.RoundToInt(Mathf.Sin(rad) * tr);
            FillRect(tex, tx - 2, ty - 2, tx + 2, ty + 2, gear);
        }

        // Output (top, green)
        DrawOutputArrow(tex, 0, COL_OUTPUT);

        // Inputs (left, right — orange)
        DrawInputArrow(tex, 1, COL_INPUT); // Right
        DrawInputArrow(tex, 3, COL_INPUT); // Left

        return Finalize(tex);
    }

    // ========== Collector ==========
    // Gold body, $ symbol, 4 input arrows (accepts from all sides)
    private static Sprite CreateCollectorSprite()
    {
        Texture2D tex = new Texture2D(SIZE, SIZE);
        Color body = new Color(0.8f, 0.7f, 0.2f);
        Color bodyDark = new Color(0.6f, 0.5f, 0.12f);
        Color bodyLight = new Color(0.9f, 0.8f, 0.35f);
        Color symbol = Color.white;

        FillWithBorder(tex, body, bodyDark, 3);
        FillRect(tex, 6, 6, 58, 58, bodyLight);

        // $ symbol (larger, more detailed)
        // Vertical bar
        FillRect(tex, 30, 12, 34, 52, symbol);
        // Top curve
        FillRect(tex, 20, 42, 44, 46, symbol);
        FillRect(tex, 18, 38, 22, 42, symbol);
        FillRect(tex, 20, 34, 30, 38, symbol);
        // Middle bar
        FillRect(tex, 20, 30, 44, 34, symbol);
        // Bottom curve
        FillRect(tex, 42, 22, 46, 30, symbol);
        FillRect(tex, 34, 18, 44, 22, symbol);
        FillRect(tex, 20, 18, 44, 22, symbol);

        // 4 input arrows (orange, all sides)
        DrawInputArrow(tex, 0, COL_INPUT);
        DrawInputArrow(tex, 1, COL_INPUT);
        DrawInputArrow(tex, 2, COL_INPUT);
        DrawInputArrow(tex, 3, COL_INPUT);

        return Finalize(tex);
    }

    // ========== Drawing Helpers ==========

    /// <summary>
    /// Draws an output arrow (triangle pointing OUTWARD from the building edge).
    /// dir: 0=Up, 1=Right, 2=Down, 3=Left
    /// </summary>
    private static void DrawOutputArrow(Texture2D tex, int dir, Color color)
    {
        int cx = SIZE / 2;
        int cy = SIZE / 2;

        switch (dir)
        {
            case 0: // Up — arrow at top edge pointing up
                for (int row = 0; row < 6; row++)
                    for (int col = -(5 - row); col <= (5 - row); col++)
                        SetSafe(tex, cx + col, SIZE - 3 - row, color);
                // Stem
                FillRect(tex, cx - 2, SIZE - 9, cx + 2, SIZE - 3, color);
                break;
            case 1: // Right
                for (int row = 0; row < 6; row++)
                    for (int col = -(5 - row); col <= (5 - row); col++)
                        SetSafe(tex, SIZE - 3 - row, cy + col, color);
                FillRect(tex, SIZE - 9, cy - 2, SIZE - 3, cy + 2, color);
                break;
            case 2: // Down
                for (int row = 0; row < 6; row++)
                    for (int col = -(5 - row); col <= (5 - row); col++)
                        SetSafe(tex, cx + col, 2 + row, color);
                FillRect(tex, cx - 2, 2, cx + 2, 8, color);
                break;
            case 3: // Left
                for (int row = 0; row < 6; row++)
                    for (int col = -(5 - row); col <= (5 - row); col++)
                        SetSafe(tex, 2 + row, cy + col, color);
                FillRect(tex, 2, cy - 2, 8, cy + 2, color);
                break;
        }
    }

    /// <summary>
    /// Draws an input arrow (triangle pointing INWARD toward building center).
    /// dir: 0=Up, 1=Right, 2=Down, 3=Left
    /// </summary>
    private static void DrawInputArrow(Texture2D tex, int dir, Color color)
    {
        int cx = SIZE / 2;
        int cy = SIZE / 2;

        switch (dir)
        {
            case 0: // Up edge — arrow points down (into building)
                for (int row = 0; row < 6; row++)
                    for (int col = -(5 - row); col <= (5 - row); col++)
                        SetSafe(tex, cx + col, SIZE - 8 + row, color);
                break;
            case 1: // Right edge — arrow points left
                for (int row = 0; row < 6; row++)
                    for (int col = -(5 - row); col <= (5 - row); col++)
                        SetSafe(tex, SIZE - 8 + row, cy + col, color);
                break;
            case 2: // Down edge — arrow points up
                for (int row = 0; row < 6; row++)
                    for (int col = -(5 - row); col <= (5 - row); col++)
                        SetSafe(tex, cx + col, 7 - row, color);
                break;
            case 3: // Left edge — arrow points right
                for (int row = 0; row < 6; row++)
                    for (int col = -(5 - row); col <= (5 - row); col++)
                        SetSafe(tex, 7 - row, cy + col, color);
                break;
        }
    }

    private static void Fill(Texture2D tex, int size, Color color)
    {
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
    }

    private static void FillWithBorder(Texture2D tex, Color fill, Color border, int borderWidth)
    {
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                bool isBorder = x < borderWidth || x >= SIZE - borderWidth ||
                                y < borderWidth || y >= SIZE - borderWidth;
                tex.SetPixel(x, y, isBorder ? border : fill);
            }
    }

    private static void FillRect(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
    {
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
                SetSafe(tex, x, y, color);
    }

    private static void FillCircle(Texture2D tex, int cx, int cy, int r, Color color)
    {
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                    SetSafe(tex, x, y, color);
    }

    private static void DrawThickPixel(Texture2D tex, int x, int y, Color color, int radius)
    {
        for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
                SetSafe(tex, x + dx, y + dy, color);
    }

    private static void SetSafe(Texture2D tex, int x, int y, Color color)
    {
        if (x >= 0 && x < SIZE && y >= 0 && y < SIZE)
            tex.SetPixel(x, y, color);
    }

    private static Sprite Finalize(Texture2D tex)
    {
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), SIZE);
    }
}
