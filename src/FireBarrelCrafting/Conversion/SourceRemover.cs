using System.Diagnostics.CodeAnalysis;
using FireBarrelCrafting.Spawning;
using Il2Cpp;
using Il2CppTLD.Placement;
using UnityEngine;

namespace FireBarrelCrafting.Conversion;

/// <summary>Removes a conversion source (barrel, wheel) from the world in a way the game saves.</summary>
/// <remarks>
/// Unity objects must be null-checked with <c>==</c>, never <c>is null</c> / <c>?.</c> / <c>??</c>:
/// only the overloaded operator notices objects that were destroyed on the native side.
/// </remarks>
internal static class SourceRemover
{
    /// <summary>Checks whether the source can be removed so that it stays removed after a reload.</summary>
    /// <param name="source">The decoration's GameObject.</param>
    /// <returns>
    /// <see langword="true"/> if it has a <see cref="BreakDown"/> (the game saves its removal by guid) or was placed
    /// by the player (it is saved in the <see cref="PlaceableManager"/> list, so taking it out of that list is enough).
    /// </returns>
    public static bool CanRemove(GameObject source) =>
        source.GetComponent<BreakDown>() != null || TryGetPlacedByPlayer(source, out _);

    /// <summary>Removes the source; check <see cref="CanRemove"/> first.</summary>
    /// <param name="decoration">The decoration to remove.</param>
    public static void Remove(DecorationItem decoration)
    {
        GameObject source = decoration.gameObject;

        BreakDown breakDown = source.GetComponent<BreakDown>();
        if (breakDown != null)
        {
            // No yield: the source became the new object.
            breakDown.DoBreakDown(false);
            return;
        }

        decoration.RemoveFromHierarchy();
        if (TryGetPlacedByPlayer(source, out Placeable? placeable))
        {
            PlaceableManager.Remove(placeable);
        }

        if (source != null)
        {
            UnityEngine.Object.Destroy(source);
        }
    }

    private static bool TryGetPlacedByPlayer(GameObject source, [NotNullWhen(true)] out Placeable? placeable)
    {
        placeable = source.GetComponent<Placeable>();
        // Scene decorations carry a Placeable too; only the ones in the manager's list are saved through it.
        if (placeable == null || !PlaceableRegistration.IsRegisteredUnderOwnGuid(placeable))
        {
            placeable = null;
            return false;
        }

        return true;
    }
}
