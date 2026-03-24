using System.Collections.Generic;

/// <summary>
/// Defines all crafting recipes for Production Stations.
/// Each recipe: 2 inputs → 1 output + sell value.
/// Order of inputs does not matter (commutative).
/// </summary>
public static class RecipeDatabase
{
    public struct Recipe
    {
        public ItemType InputA;
        public ItemType InputB;
        public ItemType Output;
        public int SellValue;

        public Recipe(ItemType a, ItemType b, ItemType output, int sellValue)
        {
            InputA = a;
            InputB = b;
            Output = output;
            SellValue = sellValue;
        }
    }

    private static List<Recipe> _recipes;

    public static List<Recipe> All
    {
        get
        {
            if (_recipes == null) InitRecipes();
            return _recipes;
        }
    }

    private static void InitRecipes()
    {
        _recipes = new List<Recipe>
        {
            // Tier 1: ore + coal = ingots/lime
            new Recipe(ItemType.IronOre,   ItemType.Coal,  ItemType.IronIngot,   20),
            new Recipe(ItemType.CopperOre, ItemType.Coal,  ItemType.CopperIngot, 15),
            new Recipe(ItemType.Stone,     ItemType.Coal,  ItemType.Lime,        10),

            // Tier 2: ore/ingot + stone = powders
            new Recipe(ItemType.IronOre,     ItemType.Stone, ItemType.IronPowder,   5),
            new Recipe(ItemType.CopperOre, ItemType.Stone, ItemType.CopperPowder, 10),

            // Tier 3: advanced components
            new Recipe(ItemType.CopperPowder, ItemType.IronPowder, ItemType.BatteryCore,  35),
            new Recipe(ItemType.CopperIngot,  ItemType.IronIngot,  ItemType.BatteryShell, 40),

            // Tier 4: final product
            new Recipe(ItemType.BatteryCore,  ItemType.BatteryShell, ItemType.Battery, 75),
        };
    }

    /// <summary>
    /// Finds a recipe matching the two input items (order doesn't matter).
    /// Returns true if found, with the recipe in 'result'.
    /// </summary>
    public static bool TryFind(ItemType itemA, ItemType itemB, out Recipe result)
    {
        foreach (var r in All)
        {
            if ((r.InputA == itemA && r.InputB == itemB) ||
                (r.InputA == itemB && r.InputB == itemA))
            {
                result = r;
                return true;
            }
        }
        result = default;
        return false;
    }

    /// <summary>
    /// Returns the sell value for an item type.
    /// Raw ores sell for $1 at the collector.
    /// Crafted items sell for their recipe value.
    /// </summary>
    public static int GetSellValue(ItemType type)
    {
        foreach (var r in All)
        {
            if (r.Output == type) return r.SellValue;
        }
        // Raw ores default to $1
        return 1;
    }
}
