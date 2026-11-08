using MelonLoader;

namespace FireBarrelCrafting.Configuration;

/// <summary>
/// Strongly typed access to <c>UserData/MelonPreferences.cfg</c>, section <c>[FireBarrelCrafting]</c>.
/// Values are validated (clamped / parsed) and read on every access, so edits made while the game runs apply.
/// </summary>
internal sealed class ModSettings
{
    private const string CategoryId = "FireBarrelCrafting";

    private readonly MelonPreferences_Entry<string> _allowedTools;
    private readonly MelonPreferences_Entry<float> _toolConditionLossPercent;
    private readonly MelonPreferences_Entry<int> _craftMinutes;
    private readonly MelonPreferences_Entry<string> _barrelNamePrefixes;
    private readonly MelonPreferences_Entry<bool> _verboseLogging;

    private ModSettings(MelonPreferences_Category category)
    {
        _allowedTools = category.CreateEntry("AllowedTools", "GEAR_Hacksaw,GEAR_Hatchet,GEAR_HatchetImprovised",
            description: "Comma-separated gear names. Any ONE of these must be in your inventory; the best-condition one is used.");
        _toolConditionLossPercent = category.CreateEntry("ToolConditionLossPercent", 10f,
            description: "Condition (percent of the tool's max condition) the tool loses per conversion. 0 = no wear.");
        _craftMinutes = category.CreateEntry("CraftMinutes", 60,
            description: "In-game minutes the conversion takes.");
        _barrelNamePrefixes = category.CreateEntry("BarrelNamePrefixes", "OBJ_MetalBarrelA",
            description: "Comma-separated object-name prefixes of decorative barrels that can be converted.");
        _verboseLogging = category.CreateEntry("VerboseLogging", false,
            description: "Write extra diagnostic lines to the MelonLoader log.");
    }

    /// <summary>Gets the gear names of which any one is required for a conversion.</summary>
    public IReadOnlyList<string> AllowedTools => SettingList.Parse(_allowedTools.Value);

    /// <summary>Gets the tool wear per conversion, in percent of the tool's max condition (0–100).</summary>
    public float ToolConditionLossPercent => Math.Clamp(_toolConditionLossPercent.Value, 0f, 100f);

    /// <summary>Gets the in-game minutes a conversion takes (never negative).</summary>
    public int CraftMinutes => Math.Max(0, _craftMinutes.Value);

    /// <summary>Gets the object-name prefixes of decorative barrels that can be converted.</summary>
    public IReadOnlyList<string> BarrelNamePrefixes => SettingList.Parse(_barrelNamePrefixes.Value);

    /// <summary>Gets a value indicating whether diagnostic messages are logged.</summary>
    public bool VerboseLogging => _verboseLogging.Value;

    /// <summary>Registers the preference category and its entries, writing defaults for missing values.</summary>
    /// <returns>The settings.</returns>
    public static ModSettings Create()
    {
        MelonPreferences_Category category = MelonPreferences.CreateCategory(CategoryId, "Fire Barrel Crafting");
        var settings = new ModSettings(category);
        category.SaveToFile(false);
        return settings;
    }
}
