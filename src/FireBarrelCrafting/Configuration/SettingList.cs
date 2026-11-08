namespace FireBarrelCrafting.Configuration;

/// <summary>Parses comma/semicolon separated setting values.</summary>
internal static class SettingList
{
    private static readonly char[] Separators = [',', ';'];

    /// <summary>Splits a list setting into its items.</summary>
    /// <param name="value">Raw setting value, e.g. <c>"GEAR_Hacksaw, GEAR_Hatchet"</c>.</param>
    /// <returns>Trimmed, non-empty, case-insensitively distinct items in their original order; empty for blank input.</returns>
    public static IReadOnlyList<string> Parse(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                   .Distinct(StringComparer.OrdinalIgnoreCase)
                   .ToArray();
}
