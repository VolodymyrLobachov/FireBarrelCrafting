using FireBarrelCrafting.Configuration;
using Il2Cpp;
using UnityEngine;

namespace FireBarrelCrafting.Conversion;

/// <summary>Knows which decorations this mod may convert, and into what (GRASP: Information Expert).</summary>
/// <remarks>
/// Unity objects must be null-checked with <c>==</c>, never <c>is null</c> / <c>?.</c> / <c>??</c>:
/// only the overloaded operator notices objects that were destroyed on the native side.
/// </remarks>
/// <param name="settings">Decides whether barrels may also become rim grills.</param>
/// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
internal sealed class SourceDetector(FireBarrelSettings settings)
{
    private readonly FireBarrelSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>Gets what a decoration can be turned into right now.</summary>
    /// <param name="decoration">The decoration to check.</param>
    /// <returns>
    /// The targets in wheel order; empty unless it is an active barrel or wheel that is not already a fire / stove
    /// and can be removed in a way the game saves.
    /// </returns>
    public IReadOnlyList<ConversionTarget> TargetsFor(DecorationItem decoration)
    {
        if (decoration == null)
        {
            return [];
        }

        GameObject source = decoration.gameObject;
        if (!source.activeInHierarchy)
        {
            return [];
        }

        IReadOnlyList<ConversionTarget> targets = ConversionSources.TargetsFor(source.name, _settings.RimGrillFromBarrels);
        bool usable = targets.Count > 0
            // Never touch something that already is a fire / stove.
            && source.GetComponent<WoodStove>() == null
            && source.GetComponentInChildren<Fire>(true) == null
            && SourceRemover.CanRemove(source);
        return usable ? targets : [];
    }

    /// <summary>Checks whether a decoration can be turned into <paramref name="target"/> right now.</summary>
    /// <param name="decoration">The decoration to check.</param>
    /// <param name="target">The wanted target.</param>
    /// <returns><see langword="true"/> if <see cref="TargetsFor"/> contains <paramref name="target"/>.</returns>
    public bool IsConvertible(DecorationItem decoration, ConversionTarget target) => TargetsFor(decoration).Contains(target);
}
