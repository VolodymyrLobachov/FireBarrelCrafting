using FireBarrelCrafting.Configuration;
using Il2Cpp;
using UnityEngine;

namespace FireBarrelCrafting.Conversion;

/// <summary>Knows what a convertible decorative barrel is (GRASP: Information Expert).</summary>
/// <remarks>
/// Unity objects must be null-checked with <c>==</c>, never <c>is null</c> / <c>?.</c> / <c>??</c>:
/// only the overloaded operator notices objects that were destroyed on the native side.
/// </remarks>
/// <param name="settings">Supplies the barrel name prefixes.</param>
/// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
internal sealed class BarrelDetector(ModSettings settings)
{
    private readonly ModSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>Finds the convertible barrel a game object belongs to.</summary>
    /// <param name="gameObject">The object, or one of its children, e.g. what the player clicked.</param>
    /// <returns>The barrel's <see cref="DecorationItem"/>, or <see langword="null"/> if it is not a convertible barrel.</returns>
    public DecorationItem? FindConvertible(GameObject? gameObject)
    {
        if (gameObject == null)
        {
            return null;
        }

        DecorationItem decoration = gameObject.GetComponentInParent<DecorationItem>();
        return decoration != null && IsConvertible(decoration) ? decoration : null;
    }

    /// <summary>Checks whether a decoration is a decorative barrel this mod may turn into a fire barrel.</summary>
    /// <param name="decoration">The decoration to check.</param>
    /// <returns>
    /// <see langword="true"/> for an active barrel with a matching name and a <see cref="BreakDown"/>
    /// that is not already a fire / stove.
    /// </returns>
    public bool IsConvertible(DecorationItem decoration)
    {
        if (decoration == null)
        {
            return false;
        }

        GameObject barrel = decoration.gameObject;
        return barrel.activeInHierarchy
            && HasBarrelName(barrel.name)
            // Never touch something that already is a fire / stove.
            && barrel.GetComponent<WoodStove>() == null
            && barrel.GetComponentInChildren<Fire>(true) == null
            // BreakDown is how the removal of the decorative barrel gets saved by the game.
            && barrel.GetComponent<BreakDown>() != null;
    }

    private bool HasBarrelName(string name) =>
        _settings.BarrelNamePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
