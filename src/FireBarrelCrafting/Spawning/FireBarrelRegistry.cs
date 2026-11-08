using FireBarrelCrafting.Infrastructure;
using Il2Cpp;
using Il2CppTLD.Placement;
using UnityEngine;

namespace FireBarrelCrafting.Spawning;

/// <summary>
/// Ties our fire barrels into the game's own save systems: <see cref="PlaceableManager"/> saves and re-creates
/// the barrel, <see cref="ObjectGuid"/>s let the game match its fire and breakdown state.
/// </summary>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException"><paramref name="log"/> is <see langword="null"/>.</exception>
internal sealed class FireBarrelRegistry(ModLog log)
{
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    /// <summary>Registers a freshly created barrel so the game saves it.</summary>
    /// <param name="fireBarrel">The instantiated fire barrel.</param>
    /// <param name="guid">Guid from <see cref="FireBarrelIdentity.NewGuid"/>.</param>
    public void Register(GameObject fireBarrel, string guid)
    {
        Placeable placeable = fireBarrel.GetComponent<Placeable>();
        if (placeable == null)
        {
            _log.Warning("Fire barrel prefab has no Placeable; it will not be saved.");
        }
        else
        {
            RegisterPlaceable(placeable, guid);
        }

        AssignObjectGuids(fireBarrel, guid);
    }

    /// <summary>Gives a barrel the game re-created from a save back its identity and puts it on the floor.</summary>
    /// <param name="fireBarrel">The re-created fire barrel.</param>
    /// <param name="guid">The guid it was saved under.</param>
    public void Restore(GameObject fireBarrel, string guid)
    {
        AssignObjectGuids(fireBarrel, guid);
        // Barrels crafted by pre-release builds could have been saved floating.
        GroundAligner.SnapDown(fireBarrel);
        _log.Debug($"Restored fire barrel {guid} at {fireBarrel.transform.position}.");
    }

    private void RegisterPlaceable(Placeable placeable, string guid)
    {
        var placeables = PlaceableManager.s_Placeables;
        string previousGuid = placeable.m_Guid;

        // Awake may already have registered the instance under the prefab's guid; move it to ours.
        if (!string.IsNullOrEmpty(previousGuid) && placeables.ContainsKey(previousGuid))
        {
            Placeable registered = placeables[previousGuid].m_Handle;
            if (registered != null && registered.Pointer == placeable.Pointer)
            {
                placeables.Remove(previousGuid);
            }
        }

        placeable.m_Guid = guid;
        if (!placeables.ContainsKey(guid))
        {
            PlaceableManager.Add(placeable);
        }

        if (placeable.m_Addressable == null || !placeable.m_Addressable.RuntimeKeyIsValid())
        {
            _log.Warning("Fire barrel Placeable has no valid addressable; it may not survive a reload.");
        }

        _log.Debug($"Placeable registered: '{previousGuid}' -> '{guid}'.");
    }

    private void AssignObjectGuids(GameObject fireBarrel, string guid)
    {
        ObjectGuid.MaybeAttachObjectGuidAddOrReplace(fireBarrel, guid);

        Fire fire = fireBarrel.GetComponentInChildren<Fire>(true);
        if (fire == null)
        {
            _log.Warning("Fire barrel has no Fire component.");
            return;
        }

        ObjectGuid.MaybeAttachObjectGuidAddOrReplace(fire.gameObject, FireBarrelIdentity.FireGuidFor(guid));
    }
}
