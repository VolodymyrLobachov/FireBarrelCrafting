using FireBarrelCrafting.Spawning;
using HarmonyLib;
using Il2CppTLD.Placement;

namespace FireBarrelCrafting.Patches;

/// <summary>When the game re-creates one of our fire barrels from a save, give it back its identity.</summary>
[HarmonyPatch(typeof(Placeable), nameof(Placeable.FindOrCreateAndDeserialize))]
internal static class PlaceableRestorePatch
{
    /// <summary>Runs after the game found or created a placeable from save data.</summary>
    /// <param name="guid">The saved placeable guid.</param>
    /// <param name="__result">The placeable the game found or created.</param>
    private static void Postfix(string guid, Placeable __result)
    {
        if (__result == null || !FireBarrelIdentity.IsModGuid(guid) || Mod.Services is not { } services)
        {
            return;
        }

        try
        {
            services.Registry.Restore(__result.gameObject, guid);
        }
        catch (Exception e)
        {
            // Never break save loading: the barrel still exists, only its fire state may not line up.
            services.Log.Error($"Restoring fire barrel {guid} failed", e);
        }
    }
}
