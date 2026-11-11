using Il2Cpp;
using UnityEngine;

namespace FireBarrelCrafting.Conversion;

/// <summary>Knows what a convertible decorative barrel is (GRASP: Information Expert).</summary>
/// <remarks>
/// Unity objects must be null-checked with <c>==</c>, never <c>is null</c> / <c>?.</c> / <c>??</c>:
/// only the overloaded operator notices objects that were destroyed on the native side.
/// </remarks>
internal sealed class BarrelDetector
{
    /// <summary>
    /// Name prefix of the decorative metal barrel prefab. Scene instances are named
    /// "OBJ_MetalBarrelA_Prefab", "OBJ_MetalBarrelA_Prefab (12)", "... (PLACED)" and so on.
    /// Plastic barrels (OBJ_PlasticBarrelA) are deliberately excluded: plastic would just melt.
    /// </summary>
    private const string BarrelNamePrefix = "OBJ_MetalBarrelA";

    /// <summary>Checks whether a decoration is a decorative barrel this mod may turn into a fire barrel or rim grill.</summary>
    /// <param name="decoration">The decoration to check.</param>
    /// <returns>
    /// <see langword="true"/> for an active metal barrel with a <see cref="BreakDown"/> that is not already a fire / stove.
    /// </returns>
    public bool IsConvertible(DecorationItem decoration)
    {
        if (decoration == null)
        {
            return false;
        }

        GameObject barrel = decoration.gameObject;
        return barrel.activeInHierarchy
            && barrel.name.StartsWith(BarrelNamePrefix, StringComparison.OrdinalIgnoreCase)
            // Never touch something that already is a fire / stove.
            && barrel.GetComponent<WoodStove>() == null
            && barrel.GetComponentInChildren<Fire>(true) == null
            // BreakDown is how the removal of the decorative barrel gets saved by the game.
            && barrel.GetComponent<BreakDown>() != null;
    }
}
