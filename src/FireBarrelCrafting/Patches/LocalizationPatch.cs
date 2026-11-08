using FireBarrelCrafting.Infrastructure;
using HarmonyLib;
using Il2Cpp;

namespace FireBarrelCrafting.Patches;

/// <summary>Resolves our wheel loc id to readable text.</summary>
[HarmonyPatch(typeof(Localization), nameof(Localization.Get), [typeof(string)])]
internal static class LocalizationPatch
{
    /// <summary>Runs after the vanilla lookup.</summary>
    /// <param name="key">The requested loc id.</param>
    /// <param name="__result">The localized text, replaced for our loc id.</param>
    private static void Postfix(string key, ref string __result)
    {
        if (key == ModText.WheelLocId)
        {
            __result = ModText.WheelLabel;
        }
    }
}
