using HarmonyLib;
using Il2Cpp;

namespace FireBarrelCrafting.Patches;

/// <summary>Decorative barrels interact through <see cref="DecorationItem.PerformInteraction"/>; open our wheel instead.</summary>
[HarmonyPatch(typeof(DecorationItem), nameof(DecorationItem.PerformInteraction))]
internal static class DecorationInteractionPatch
{
    /// <summary>Runs before the vanilla interaction.</summary>
    /// <param name="__instance">The decoration the player interacted with.</param>
    /// <param name="__result">Return value of the skipped vanilla method.</param>
    /// <returns><see langword="false"/> to skip the vanilla interaction because our wheel was opened.</returns>
    private static bool Prefix(DecorationItem __instance, ref bool __result)
    {
        if (Mod.Services is not { } services)
        {
            return true;
        }

        try
        {
            if (!services.Wheel.TryOpen(__instance))
            {
                return true;
            }

            __result = true;
            return false;
        }
        catch (Exception e)
        {
            // Never break the game's interaction: fall back to vanilla on any failure.
            services.Log.Error("Opening the fire barrel wheel failed; using the vanilla interaction", e);
            return true;
        }
    }
}
