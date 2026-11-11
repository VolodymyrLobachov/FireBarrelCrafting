using HarmonyLib;
using Il2Cpp;

namespace FireBarrelCrafting.Patches;

/// <summary>
/// After a decoration builds its interaction wheel entries, add our entries for convertible barrels.
/// </summary>
[HarmonyPatch(typeof(DecorationItem), nameof(DecorationItem.SetupInteraction))]
internal static class DecorationWheelPatch
{
    /// <summary>Runs after the vanilla setup has filled <see cref="DecorationItem.m_ItemData"/>.</summary>
    /// <param name="__instance">The decoration being set up.</param>
    private static void Postfix(DecorationItem __instance)
    {
        if (Mod.Services is not { } services)
        {
            return;
        }

        try
        {
            services.Wheel.AddEntriesIfConvertible(__instance);
        }
        catch (Exception e)
        {
            // Never break the game's interaction: without our entries the barrel simply behaves as vanilla.
            services.Log.Error("Adding the wheel entries failed", e);
        }
    }
}
