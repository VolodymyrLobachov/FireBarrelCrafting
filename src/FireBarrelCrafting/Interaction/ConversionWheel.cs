using FireBarrelCrafting.Conversion;
using FireBarrelCrafting.Infrastructure;
using Il2Cpp;
using Il2CppInterop.Runtime;

namespace FireBarrelCrafting.Interaction;

/// <summary>
/// Adds a "Fire Barrel" entry to the interaction wheel of convertible barrels.
/// </summary>
/// <remarks>
/// The entry is appended to the decoration's own action list (<see cref="DecorationItem.m_ItemData"/>), which the
/// vanilla interaction turns into the wheel. Vanilla entries and entries from other mods (e.g. Safehouse
/// Customization Plus adds "Break Down" the same way) therefore stay in one wheel, untouched.
/// </remarks>
/// <param name="detector">Decides which decorations get the entry.</param>
/// <param name="conversion">Started when the entry is selected.</param>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
internal sealed class ConversionWheel(BarrelDetector detector, BarrelConversionService conversion, ModLog log)
{
    private readonly BarrelDetector _detector = detector ?? throw new ArgumentNullException(nameof(detector));
    private readonly BarrelConversionService _conversion = conversion ?? throw new ArgumentNullException(nameof(conversion));
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    /// <summary>Adds the "Fire Barrel" entry if <paramref name="decoration"/> is a convertible barrel and lacks it.</summary>
    /// <param name="decoration">A decoration whose interaction was just set up.</param>
    public void AddEntryIfConvertible(DecorationItem decoration)
    {
        if (!_detector.IsConvertible(decoration))
        {
            return;
        }

        var items = decoration.m_ItemData;
        if (items == null || ContainsEntry(items))
        {
            return;
        }

        Action onSelected = () => _conversion.Begin(decoration);
        items.Add(new ActionPickerItemData(ModText.WheelIcon, ModText.WheelLocId,
            DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(onSelected)));

        _log.Debug($"Added Fire Barrel entry to '{decoration.name}' ({items.Count} entries).");
    }

    private static bool ContainsEntry(Il2CppSystem.Collections.Generic.List<ActionPickerItemData> items)
    {
        // Indexed loop on purpose: enumerating Il2Cpp lists from managed code has failed before.
        for (int i = 0; i < items.Count; i++)
        {
            ActionPickerItemData item = items[i];
            if (item != null && item.m_LocID == ModText.WheelLocId)
            {
                return true;
            }
        }

        return false;
    }
}
