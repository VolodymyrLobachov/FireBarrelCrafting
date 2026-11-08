using ModSettings;

namespace FireBarrelCrafting.Configuration;

/// <summary>
/// Settings shown in the game's Mod Settings menu and saved to <c>Mods/FireBarrelCrafting.json</c>.
/// </summary>
/// <remarks>
/// ModSettings turns every public field into a menu entry, so public fields are the raw values only, and
/// they may only use types ModSettings 2.2.5 supports: numbers, <see cref="bool"/> and enums (no strings).
/// Code reads the validated properties below, which also guard against hand-edited JSON.
/// </remarks>
internal sealed class FireBarrelSettings : JsonModSettings
{
    /// <summary>Whether a hacksaw can be used; read <see cref="AllowedTools"/> instead.</summary>
    [Section("Tools (any one is required)")]
    [Name("Hacksaw")]
    [Description("Allow the hacksaw to make fire barrels.")]
    public bool AllowHacksaw = true;

    /// <summary>Whether a hatchet can be used; read <see cref="AllowedTools"/> instead.</summary>
    [Name("Hatchet")]
    [Description("Allow the hatchet to make fire barrels.")]
    public bool AllowHatchet = true;

    /// <summary>Whether an improvised hatchet can be used; read <see cref="AllowedTools"/> instead.</summary>
    [Name("Improvised Hatchet")]
    [Description("Allow the improvised hatchet to make fire barrels.")]
    public bool AllowImprovisedHatchet = true;

    /// <summary>Whether a prybar can be used; read <see cref="AllowedTools"/> instead.</summary>
    [Name("Prybar")]
    [Description("Allow the prybar to make fire barrels.")]
    public bool AllowPrybar;

    /// <summary>Raw tool wear; read <see cref="ToolConditionLossPercent"/> instead.</summary>
    [Section("Crafting")]
    [Name("Tool wear (%)")]
    [Description("Condition the tool loses per fire barrel, in percent of its max condition. 0 = no wear.")]
    [Slider(0, 100)]
    public int ToolWearPercent = 10;

    /// <summary>Raw craft time; read <see cref="CraftMinutes"/> instead.</summary>
    [Name("Craft time (minutes)")]
    [Description("In-game minutes it takes to make a fire barrel.")]
    [Slider(0, 240)]
    public int CraftTimeMinutes = 60;

    /// <summary>Gets or sets a value indicating whether diagnostic messages are logged.</summary>
    [Section("Diagnostics")]
    [Name("Verbose logging")]
    [Description("Write extra diagnostic lines to MelonLoader/Latest.log.")]
    public bool VerboseLogging;

    /// <summary>Gets the gear names of the enabled tools, any one of which is required for a conversion.</summary>
    public IReadOnlyList<string> AllowedTools
    {
        get
        {
            var tools = new List<string>(4);
            AddIf(tools, AllowHacksaw, "GEAR_Hacksaw");
            AddIf(tools, AllowHatchet, "GEAR_Hatchet");
            AddIf(tools, AllowImprovisedHatchet, "GEAR_HatchetImprovised");
            AddIf(tools, AllowPrybar, "GEAR_Prybar");
            return tools;
        }
    }

    /// <summary>Gets the tool wear per conversion, in percent of the tool's max condition (0–100).</summary>
    public float ToolConditionLossPercent => Math.Clamp(ToolWearPercent, 0, 100);

    /// <summary>Gets the in-game minutes a conversion takes (never negative).</summary>
    public int CraftMinutes => Math.Max(0, CraftTimeMinutes);

    private static void AddIf(List<string> tools, bool enabled, string gearName)
    {
        if (enabled)
        {
            tools.Add(gearName);
        }
    }
}
