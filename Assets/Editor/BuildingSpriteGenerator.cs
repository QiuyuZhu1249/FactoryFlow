using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Generates simple building sprite textures and saves as PNG assets.
/// Menu: FactoryFlow > Generate Building Sprites
/// </summary>
public static class BuildingSpriteGenerator
{
    private const int SIZE = 32;
    private const string OUTPUT_PATH = "Assets/Sprites/Buildings";

    [MenuItem("FactoryFlow/Generate Building Sprites")]
    public static void GenerateAll()
    {
        if (!Directory.Exists(OUTPUT_PATH))
            Directory.CreateDirectory(OUTPUT_PATH);

        GenerateMinerSprite();
        GenerateConveyorStraightSprite();
        GenerateConveyorCornerSprite();
        GenerateSplitterSprite();
        GenerateMergerSprite();
        GenerateProductionStationSprite();
        GenerateCollectorSprite();

        AssetDatabase.Refresh();
        Debug.Log("[SpriteGen] All building sprites generated!");
    }

    private static void GenerateMinerSprite()
    {
        Color bg = new Color(0.55f, 0.35f, 0.15f);
        Color border = new Color(0.4f, 0.25f, 0.1f);
        Color detail = new Color(0.9f, 0.8f, 0.6f);

        Texture2D tex = CreateBase(bg, border);

        // Draw pickaxe shape
        // Handle (diagonal)
        for (int i = 0; i < 10; i++)
        {
            SetPixelSafe(tex, 10 + i, 8 + i, detail);
            SetPixelSafe(tex, 11 + i, 8 + i, detail);
        }
        // Head (horizontal)
        for (int x = 16; x < 26; x++)
        {
            SetPixelSafe(tex, x, 18, detail);
            SetPixelSafe(tex, x, 19, detail);
        }
        // Point down
        for (int i = 0; i < 4; i++)
        {
            SetPixelSafe(tex, 24 + i, 17 - i, detail);
            SetPixelSafe(tex, 25 + i, 17 - i, detail);
        }

        // Output arrow (top)
        DrawOutputArrow(tex, Direction.Up, new Color(0.2f, 0.8f, 0.2f));

        SaveSprite(tex, "Miner");
    }

    private static void GenerateConveyorStraightSprite()
    {
        Color bg = new Color(0.35f, 0.35f, 0.4f);
        Color border = new Color(0.25f, 0.25f, 0.3f);
        Color arrow = new Color(0.6f, 0.65f, 0.7f);

        Texture2D tex = CreateBase(bg, border);

        // Draw chevrons pointing up (>>>)
        for (int row = 0; row < 3; row++)
        {
            int baseY = 8 + row * 8;
            for (int i = 0; i < 5; i++)
            {
                SetPixelSafe(tex, 16 - i, baseY + i, arrow);
                SetPixelSafe(tex, 16 + i, baseY + i, arrow);
                SetPixelSafe(tex, 15 - i, baseY + i, arrow);
                SetPixelSafe(tex, 17 + i, baseY + i, arrow);
            }
        }

        // Side rails
        for (int y = 2; y < 30; y++)
        {
            SetPixelSafe(tex, 4, y, border);
            SetPixelSafe(tex, 5, y, border);
            SetPixelSafe(tex, 26, y, border);
            SetPixelSafe(tex, 27, y, border);
        }

        SaveSprite(tex, "ConveyorStraight");
    }

    private static void GenerateConveyorCornerSprite()
    {
        Color bg = new Color(0.35f, 0.35f, 0.4f);
        Color border = new Color(0.25f, 0.25f, 0.3f);
        Color arrow = new Color(0.6f, 0.65f, 0.7f);

        Texture2D tex = CreateBase(bg, border);

        // L-shape: input from bottom, output to right
        // Bottom rail
        for (int y = 2; y < 18; y++)
        {
            SetPixelSafe(tex, 4, y, border);
            SetPixelSafe(tex, 5, y, border);
            SetPixelSafe(tex, 26, y, border);
            SetPixelSafe(tex, 27, y, border);
        }
        // Right rail
        for (int x = 14; x < 30; x++)
        {
            SetPixelSafe(tex, x, 26, border);
            SetPixelSafe(tex, x, 27, border);
            SetPixelSafe(tex, x, 4, border);
            SetPixelSafe(tex, x, 5, border);
        }

        // Arrow curving from bottom to right
        for (int i = 0; i < 4; i++)
        {
            SetPixelSafe(tex, 16, 8 + i, arrow);
            SetPixelSafe(tex, 15, 8 + i, arrow);
        }
        for (int i = 0; i < 4; i++)
        {
            SetPixelSafe(tex, 18 + i, 16, arrow);
            SetPixelSafe(tex, 18 + i, 15, arrow);
        }

        SaveSprite(tex, "ConveyorCorner");
    }

    private static void GenerateSplitterSprite()
    {
        Color bg = new Color(0.2f, 0.35f, 0.6f);
        Color border = new Color(0.15f, 0.25f, 0.45f);
        Color arrow = new Color(0.7f, 0.8f, 1f);

        Texture2D tex = CreateBase(bg, border);

        // Input arrow (bottom center, pointing up)
        for (int i = -2; i <= 2; i++)
        {
            SetPixelSafe(tex, 16 + i, 6, arrow);
            SetPixelSafe(tex, 16 + i, 7, arrow);
        }
        SetPixelSafe(tex, 16, 8, arrow);
        SetPixelSafe(tex, 16, 9, arrow);

        // Center dot
        for (int x = 14; x < 18; x++)
            for (int y = 14; y < 18; y++)
                SetPixelSafe(tex, x, y, arrow);

        // 3 output arrows (top, left, right)
        // Top
        SetPixelSafe(tex, 16, 24, arrow); SetPixelSafe(tex, 16, 25, arrow);
        SetPixelSafe(tex, 15, 23, arrow); SetPixelSafe(tex, 17, 23, arrow);
        // Left
        SetPixelSafe(tex, 6, 16, arrow); SetPixelSafe(tex, 7, 16, arrow);
        SetPixelSafe(tex, 8, 15, arrow); SetPixelSafe(tex, 8, 17, arrow);
        // Right
        SetPixelSafe(tex, 24, 16, arrow); SetPixelSafe(tex, 25, 16, arrow);
        SetPixelSafe(tex, 23, 15, arrow); SetPixelSafe(tex, 23, 17, arrow);

        SaveSprite(tex, "Splitter");
    }

    private static void GenerateMergerSprite()
    {
        Color bg = new Color(0.2f, 0.5f, 0.3f);
        Color border = new Color(0.15f, 0.35f, 0.2f);
        Color arrow = new Color(0.6f, 1f, 0.7f);

        Texture2D tex = CreateBase(bg, border);

        // 3 input arrows (bottom, left, right)
        // Bottom
        SetPixelSafe(tex, 16, 6, arrow); SetPixelSafe(tex, 16, 7, arrow);
        SetPixelSafe(tex, 15, 8, arrow); SetPixelSafe(tex, 17, 8, arrow);
        // Left
        SetPixelSafe(tex, 6, 16, arrow); SetPixelSafe(tex, 7, 16, arrow);
        SetPixelSafe(tex, 8, 15, arrow); SetPixelSafe(tex, 8, 17, arrow);
        // Right
        SetPixelSafe(tex, 24, 16, arrow); SetPixelSafe(tex, 25, 16, arrow);
        SetPixelSafe(tex, 23, 15, arrow); SetPixelSafe(tex, 23, 17, arrow);

        // Center dot
        for (int x = 14; x < 18; x++)
            for (int y = 14; y < 18; y++)
                SetPixelSafe(tex, x, y, arrow);

        // Output arrow (top)
        SetPixelSafe(tex, 16, 24, arrow); SetPixelSafe(tex, 16, 25, arrow);
        SetPixelSafe(tex, 15, 23, arrow); SetPixelSafe(tex, 17, 23, arrow);
        SetPixelSafe(tex, 14, 22, arrow); SetPixelSafe(tex, 18, 22, arrow);

        SaveSprite(tex, "Merger");
    }

    private static void GenerateProductionStationSprite()
    {
        Color bg = new Color(0.6f, 0.45f, 0.1f);
        Color border = new Color(0.45f, 0.35f, 0.08f);
        Color gear = new Color(0.9f, 0.85f, 0.6f);

        Texture2D tex = CreateBase(bg, border);

        // Draw gear shape (circle with teeth)
        int cx = 16, cy = 16, r = 7;
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist < r && dist > r - 3)
                    SetPixelSafe(tex, x, y, gear);
                if (dist < 3)
                    SetPixelSafe(tex, x, y, gear);
            }
        }
        // Gear teeth
        for (int i = -2; i <= 2; i++)
        {
            SetPixelSafe(tex, cx + i, cy + r + 1, gear);
            SetPixelSafe(tex, cx + i, cy - r - 1, gear);
            SetPixelSafe(tex, cx + r + 1, cy + i, gear);
            SetPixelSafe(tex, cx - r - 1, cy + i, gear);
        }

        SaveSprite(tex, "ProductionStation");
    }

    private static void GenerateCollectorSprite()
    {
        Color bg = new Color(0.85f, 0.75f, 0.25f);
        Color border = new Color(0.65f, 0.55f, 0.15f);
        Color symbol = Color.white;

        Texture2D tex = CreateBase(bg, border);

        // Draw $ symbol
        for (int y = 6; y < 26; y++) SetPixelSafe(tex, 16, y, symbol);
        for (int x = 10; x < 22; x++) SetPixelSafe(tex, x, 22, symbol);
        for (int x = 10; x < 22; x++) SetPixelSafe(tex, x, 16, symbol);
        for (int x = 10; x < 22; x++) SetPixelSafe(tex, x, 10, symbol);
        for (int x = 10; x < 16; x++) SetPixelSafe(tex, x, 20, symbol);
        for (int x = 16; x < 22; x++) SetPixelSafe(tex, x, 12, symbol);

        SaveSprite(tex, "Collector");
    }

    // --- Helpers ---

    private static Texture2D CreateBase(Color fill, Color border)
    {
        Texture2D tex = new Texture2D(SIZE, SIZE);
        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                if (x <= 1 || x >= SIZE - 2 || y <= 1 || y >= SIZE - 2)
                    tex.SetPixel(x, y, border);
                else
                    tex.SetPixel(x, y, fill);
            }
        }
        return tex;
    }

    private static void DrawOutputArrow(Texture2D tex, Direction dir, Color color)
    {
        int cx = 16, cy = 16;
        switch (dir)
        {
            case Direction.Up:
                for (int i = -3; i <= 3; i++) SetPixelSafe(tex, cx + i, 28, color);
                for (int i = -2; i <= 2; i++) SetPixelSafe(tex, cx + i, 29, color);
                for (int i = -1; i <= 1; i++) SetPixelSafe(tex, cx + i, 30, color);
                break;
        }
    }

    private static void SetPixelSafe(Texture2D tex, int x, int y, Color color)
    {
        if (x >= 0 && x < SIZE && y >= 0 && y < SIZE)
            tex.SetPixel(x, y, color);
    }

    private static void SaveSprite(Texture2D tex, string name)
    {
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        byte[] png = tex.EncodeToPNG();
        string path = $"{OUTPUT_PATH}/{name}.png";
        File.WriteAllBytes(path, png);
        Debug.Log($"[SpriteGen] Saved {path}");
    }
}
