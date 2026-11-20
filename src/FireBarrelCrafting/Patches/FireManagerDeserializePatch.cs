using HarmonyLib;
using Il2Cpp;

namespace FireBarrelCrafting.Patches;

/// <summary>Keeps the saved fire states of the loading scene for our fires, which the game re-creates later.</summary>
[HarmonyPatch(typeof(FireManager), nameof(FireManager.Deserialize))]
internal static class FireManagerDeserializePatch
{
    /// <summary>Runs before the game restores the scene's fires.</summary>
    /// <param name="__0">The scene's fire save data (by position: the interop parameter name is not part of the API).</param>
    private static void Prefix(string __0)
    {
        if (Mod.Services is not { } services)
        {
            return;
        }

        try
        {
            int count = services.SavedFires.Capture(__0);
            services.Log.Debug($"Kept {count} saved fire state(s) for crafted fires.");
        }
        catch (Exception e)
        {
            // Never break save loading: crafted fires would only start unlit.
            services.Log.Error("Keeping the saved fire states failed", e);
        }
    }
}
