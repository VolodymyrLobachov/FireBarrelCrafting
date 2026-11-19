namespace FireBarrelCrafting.Conversion;

/// <summary>Which decorations can be turned into what, decided by prefab name only (pure logic, no game types).</summary>
internal static class ConversionSources
{
    /// <summary>
    /// Name prefix of the decorative metal barrel. Scene instances are named "OBJ_MetalBarrelA_Prefab",
    /// "OBJ_MetalBarrelA_Prefab (12)", "... (PLACED)" and so on.
    /// Plastic barrels (OBJ_PlasticBarrelA) are deliberately excluded: plastic would just melt.
    /// </summary>
    private const string MetalBarrelPrefix = "OBJ_MetalBarrelA";

    /// <summary>
    /// Name prefix of the car wheel with its steel rim, the part a rim grill is made of.
    /// OBJ_TireB is a bare tire without a rim, and OBJ_TireRackA/B/C are racks; neither matches.
    /// </summary>
    private const string WheelWithRimPrefix = "OBJ_TireA_";

    private static readonly IReadOnlyList<ConversionTarget> FireBarrelOnly = [ConversionTarget.FireBarrel];
    private static readonly IReadOnlyList<ConversionTarget> RimGrillOnly = [ConversionTarget.RimGrill];

    /// <summary>Gets what a decoration with the given name can be turned into.</summary>
    /// <param name="objectName">The decoration's GameObject name; may be <see langword="null"/>.</param>
    /// <param name="rimGrillFromBarrels">Whether metal barrels may also become rim grills.</param>
    /// <returns>The targets in wheel order; empty if the decoration is not a conversion source.</returns>
    public static IReadOnlyList<ConversionTarget> TargetsFor(string? objectName, bool rimGrillFromBarrels)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return [];
        }

        if (objectName.StartsWith(MetalBarrelPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return rimGrillFromBarrels ? ConversionTarget.All : FireBarrelOnly;
        }

        if (objectName.StartsWith(WheelWithRimPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return RimGrillOnly;
        }

        return [];
    }
}
