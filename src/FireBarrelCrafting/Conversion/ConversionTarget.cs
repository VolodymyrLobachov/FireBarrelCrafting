namespace FireBarrelCrafting.Conversion;

/// <summary>Something a decorative barrel can be turned into: which vanilla prefab, and how it is named in the UI.</summary>
/// <param name="PrefabAddress">Addressables key of the vanilla prefab (from the game's catalog.json).</param>
/// <param name="WheelLocId">Loc id of the wheel entry; <c>LocalizationPatch</c> resolves it to <paramref name="WheelLabel"/>.</param>
/// <param name="WheelLabel">Wheel label. Keep it short: a wheel slot fits about 16 characters.</param>
/// <param name="Noun">Lower-case name with its article, used inside HUD sentences, e.g. "a fire barrel".</param>
internal sealed record ConversionTarget(string PrefabAddress, string WheelLocId, string WheelLabel, string Noun)
{
    /// <summary>The vanilla fire barrel.</summary>
    public static readonly ConversionTarget FireBarrel = new(
        "Assets/Prefabs/Interactive/INTERACTIVE_FireBarrel.prefab", "FBC_CraftFireBarrel", "Fire Barrel", "a fire barrel");

    /// <summary>The vanilla rim grill: a low fire with a cooking rack.</summary>
    public static readonly ConversionTarget RimGrill = new(
        "Assets/Prefabs/Interactive/INTERACTIVE_RimGrill.prefab", "FBC_CraftRimGrill", "Rim Grill", "a rim grill");

    /// <summary>Gets every target, in the order their entries appear in the wheel.</summary>
    public static IReadOnlyList<ConversionTarget> All { get; } = [FireBarrel, RimGrill];

    /// <summary>Gets the progress bar caption.</summary>
    public string Progress => $"Crafting {WheelLabel.ToLowerInvariant()}...";

    /// <summary>Gets the HUD message after a successful conversion.</summary>
    public string Success => $"You made {Noun}.";

    /// <summary>Gets the HUD message when the prefab cannot be loaded.</summary>
    public string PrefabMissing => $"{WheelLabel} could not be created (see MelonLoader log).";

    /// <summary>Finds the target a wheel loc id belongs to.</summary>
    /// <param name="locId">Loc id to look up; may be <see langword="null"/>.</param>
    /// <returns>The target, or <see langword="null"/> if the loc id is not one of ours.</returns>
    public static ConversionTarget? FindByLocId(string? locId)
    {
        foreach (ConversionTarget target in All)
        {
            if (target.WheelLocId == locId)
            {
                return target;
            }
        }

        return null;
    }

    /// <summary>Builds the HUD message shown when no allowed tool is in the inventory.</summary>
    /// <param name="tools">Readable list of acceptable tools, e.g. "Hacksaw or Hatchet".</param>
    /// <returns>The message.</returns>
    public string ToolRequired(string tools) => $"You need a {tools} to make {Noun}.";
}
