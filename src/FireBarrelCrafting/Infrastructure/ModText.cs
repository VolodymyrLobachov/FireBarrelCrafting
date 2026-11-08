namespace FireBarrelCrafting.Infrastructure;

/// <summary>All player-facing text and UI identifiers in one place.</summary>
internal static class ModText
{
    /// <summary>Loc id of our wheel entry; <c>LocalizationPatch</c> resolves it to <see cref="WheelLabel"/>.</summary>
    public const string WheelLocId = "FBC_CraftFireBarrel";

    /// <summary>Wheel label. Keep it short: a wheel slot fits about 16 characters ("Craft Fire Barrel" overflowed).</summary>
    public const string WheelLabel = "Fire Barrel";

    /// <summary>Game sprite for the craft entry.</summary>
    public const string WheelIcon = "ico_crafting";

    /// <summary>Progress bar caption.</summary>
    public const string Progress = "Crafting fire barrel...";

    /// <summary>HUD message after a successful conversion.</summary>
    public const string Success = "You made a fire barrel.";

    /// <summary>HUD message when decorations are stacked on the barrel.</summary>
    public const string RemoveItemsFirst = "Remove the items placed on the barrel first.";

    /// <summary>HUD message when the fire barrel prefab cannot be loaded.</summary>
    public const string PrefabMissing = "Fire barrel could not be created (see MelonLoader log).";

    /// <summary>Builds the HUD message shown when no allowed tool is in the inventory.</summary>
    /// <param name="tools">Readable list of acceptable tools, e.g. "Hacksaw or Hatchet".</param>
    /// <returns>The message.</returns>
    public static string ToolRequired(string tools) => $"You need a {tools} to make a fire barrel.";
}
