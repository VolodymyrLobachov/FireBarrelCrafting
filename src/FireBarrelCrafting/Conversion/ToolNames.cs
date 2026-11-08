namespace FireBarrelCrafting.Conversion;

/// <summary>Turns gear ids such as <c>GEAR_Hacksaw</c> into readable names for HUD messages.</summary>
internal static class ToolNames
{
    private const string GearPrefix = "GEAR_";

    private static readonly Dictionary<string, string> KnownNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GEAR_Hacksaw"] = "Hacksaw",
        ["GEAR_Hatchet"] = "Hatchet",
        ["GEAR_HatchetImprovised"] = "Improvised Hatchet",
        ["GEAR_Prybar"] = "Prybar",
        ["GEAR_Hammer"] = "Hammer",
    };

    /// <summary>Converts one gear id to a readable name.</summary>
    /// <param name="gearName">Gear id, e.g. <c>GEAR_Hacksaw</c>.</param>
    /// <returns>A known English name, otherwise the id without its <c>GEAR_</c> prefix.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="gearName"/> is <see langword="null"/>.</exception>
    public static string Friendly(string gearName)
    {
        ArgumentNullException.ThrowIfNull(gearName);

        if (KnownNames.TryGetValue(gearName, out string? name))
        {
            return name;
        }

        return gearName.StartsWith(GearPrefix, StringComparison.OrdinalIgnoreCase) ? gearName[GearPrefix.Length..] : gearName;
    }

    /// <summary>Describes a set of alternative tools as one phrase.</summary>
    /// <param name="gearNames">Gear ids, any one of which is acceptable.</param>
    /// <returns>For example <c>"Hacksaw, Hatchet or Improvised Hatchet"</c>; <c>"tool"</c> for an empty list.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="gearNames"/> is <see langword="null"/>.</exception>
    public static string DescribeAlternatives(IReadOnlyList<string> gearNames)
    {
        ArgumentNullException.ThrowIfNull(gearNames);

        return gearNames.Count switch
        {
            0 => "tool",
            1 => Friendly(gearNames[0]),
            _ => string.Join(", ", gearNames.Take(gearNames.Count - 1).Select(Friendly)) + " or " + Friendly(gearNames[^1]),
        };
    }
}
