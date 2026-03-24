using UnityEngine;

public enum ItemType
{
    None,

    // Raw ores (mined)
    IronOre,
    CopperOre,
    Coal,
    Stone,

    // Smelted ingots
    IronIngot,    // IronOre + Coal = $20
    CopperIngot,  // CopperOre + Coal = $15

    // Powders
    IronPowder,   // IronOre + Stone = $5
    CopperPowder, // CopperIngot + Stone = $10

    // Processed
    Lime,         // Stone + Coal = $10

    // Advanced
    BatteryCore,  // CopperPowder + IronPowder = $35
    BatteryShell, // CopperIngot + IronIngot = $40
    Battery       // BatteryCore + BatteryShell = $75
}
