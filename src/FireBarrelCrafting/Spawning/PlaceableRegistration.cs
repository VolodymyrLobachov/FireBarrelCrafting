using Il2CppTLD.Placement;

namespace FireBarrelCrafting.Spawning;

/// <summary>Queries about the game's <see cref="PlaceableManager"/> list, which is what the game saves placed objects from.</summary>
internal static class PlaceableRegistration
{
    /// <summary>Checks whether this very instance is in the manager's list under its own guid.</summary>
    /// <param name="placeable">The placeable to check.</param>
    /// <returns>
    /// <see langword="false"/> if its guid is empty, not listed, or listed for a different instance (scene decorations
    /// and fresh prefab instances can share a guid with the listed object).
    /// </returns>
    public static bool IsRegisteredUnderOwnGuid(Placeable placeable)
    {
        string guid = placeable.m_Guid;
        var placeables = PlaceableManager.s_Placeables;
        if (string.IsNullOrEmpty(guid) || !placeables.ContainsKey(guid))
        {
            return false;
        }

        Placeable registered = placeables[guid].m_Handle;
        return registered != null && registered.Pointer == placeable.Pointer;
    }
}
