using FireBarrelCrafting.Conversion;
using FireBarrelCrafting.Infrastructure;
using Il2Cpp;
using Il2CppInterop.Runtime;

namespace FireBarrelCrafting.Interaction;

/// <summary>
/// Adds one entry per <see cref="ConversionTarget"/> ("Fire Barrel", "Rim Grill") to the interaction wheel of convertible barrels.
/// </summary>
/// <remarks>
/// The entries are appended to the decoration's own action list (<see cref="DecorationItem.m_ItemData"/>), which the
/// vanilla interaction turns into the wheel. Vanilla entries and entries from other mods (e.g. Safehouse
/// Customization Plus adds "Break Down" the same way) therefore stay in one wheel, untouched.
/// </remarks>
/// <param name="detector">Decides which decorations get the entries.</param>
/// <param name="conversion">Started when an entry is selected.</param>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
internal sealed class ConversionWheel(BarrelDetector detector, BarrelConversionService conversion, ModLog log)
{
    private readonly BarrelDetector _detector = detector ?? throw new ArgumentNullException(nameof(detector));
    private readonly BarrelConversionService _conversion = conversion ?? throw new ArgumentNullException(nameof(conversion));
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    /// <summary>
    /// Number of entries the game's action wheel can show: <c>Panel_ActionPicker</c> has exactly five buttons
    /// (Option1–Option5 in the game's UI assets). Entries beyond that must not be added.
    /// </summary>
    internal const int WheelSlots = 5;

    /// <summary>Adds the entries a convertible barrel does not have yet, as long as the wheel has free slots.</summary>
    /// <param name="decoration">A decoration whose interaction was just set up.</param>
    public void AddEntriesIfConvertible(DecorationItem decoration)
    {
        if (!_detector.IsConvertible(decoration))
        {
            return;
        }

        var items = decoration.m_ItemData;
        if (items == null)
        {
            return;
        }

        foreach (ConversionTarget target in ConversionTarget.All)
        {
            if (ContainsEntry(items, target.WheelLocId))
            {
                continue;
            }

            if (items.Count >= WheelSlots)
            {
                // Other mods filled the wheel; overflowing it would break the game's UI.
                _log.Warning($"No free wheel slot for {target.WheelLabel} on '{decoration.name}' ({items.Count}/{WheelSlots} used).");
                return;
            }

            Action onSelected = () => _conversion.Begin(decoration, target);
            items.Add(new ActionPickerItemData(ModText.WheelIcon, target.WheelLocId,
                DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(onSelected)));

            _log.Debug($"Added {target.WheelLabel} entry to '{decoration.name}' ({items.Count} entries).");
        }
    }

    private static bool ContainsEntry(Il2CppSystem.Collections.Generic.List<ActionPickerItemData> items, string locId)
    {
        // Indexed loop on purpose: enumerating Il2Cpp lists from managed code has failed before.
        for (int i = 0; i < items.Count; i++)
        {
            ActionPickerItemData item = items[i];
            if (item != null && item.m_LocID == locId)
            {
                return true;
            }
        }

        return false;
    }
}
