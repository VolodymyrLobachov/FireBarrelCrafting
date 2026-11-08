using FireBarrelCrafting.Conversion;
using FireBarrelCrafting.Infrastructure;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppGeneric = Il2CppSystem.Collections.Generic;

namespace FireBarrelCrafting.Interaction;

/// <summary>
/// Replaces the single vanilla "Destroy" interaction of a decorative barrel with the game's action wheel:
/// "Destroy" (runs the untouched vanilla interaction) and "Fire Barrel" (starts the conversion).
/// </summary>
/// <param name="detector">Decides which decorations get the wheel.</param>
/// <param name="conversion">Started by the "Fire Barrel" entry.</param>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
internal sealed class ConversionWheel(BarrelDetector detector, BarrelConversionService conversion, ModLog log)
{
    private readonly BarrelDetector _detector = detector ?? throw new ArgumentNullException(nameof(detector));
    private readonly BarrelConversionService _conversion = conversion ?? throw new ArgumentNullException(nameof(conversion));
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    private bool _runningVanillaInteraction;

    /// <summary>Opens the wheel if <paramref name="decoration"/> is a convertible barrel.</summary>
    /// <param name="decoration">The decoration the player interacted with.</param>
    /// <returns><see langword="true"/> if the wheel was opened and the vanilla interaction must be skipped.</returns>
    public bool TryOpen(DecorationItem decoration)
    {
        if (_runningVanillaInteraction || !_detector.IsConvertible(decoration))
        {
            return false;
        }

        var items = new Il2CppGeneric.List<ActionPickerItemData>();
        items.Add(CreateItem(ModText.DestroyIcon, ModText.DestroyLocId, () => RunVanillaInteraction(decoration)));
        items.Add(CreateItem(ModText.WheelIcon, ModText.WheelLocId, () => _conversion.Begin(decoration)));

        _log.Debug($"Opening wheel for '{decoration.name}'.");
        InterfaceManager.GetPanel<Panel_ActionPicker>()
            .ShowActionPicker(decoration.gameObject, items.Cast<Il2CppGeneric.IEnumerable<ActionPickerItemData>>());
        return true;
    }

    private void RunVanillaInteraction(DecorationItem decoration)
    {
        _runningVanillaInteraction = true;
        try
        {
            decoration.PerformInteraction();
        }
        finally
        {
            _runningVanillaInteraction = false;
        }
    }

    private static ActionPickerItemData CreateItem(string icon, string locId, Action onSelected) =>
        new(icon, locId, DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(onSelected));
}
