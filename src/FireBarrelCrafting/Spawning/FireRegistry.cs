using FireBarrelCrafting.Infrastructure;
using Il2Cpp;
using Il2CppTLD.Placement;
using UnityEngine;

namespace FireBarrelCrafting.Spawning;

/// <summary>
/// Applies everything our fires (fire barrels, rim grills) need, both when they are created and every time the game
/// re-creates them from a save: registration with the game's save systems (<see cref="PlaceableManager"/> saves and
/// re-creates the object, <see cref="ObjectGuid"/>s let the game match its fire and breakdown state) and the
/// runtime fix-ups the game does not save.
/// </summary>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException"><paramref name="log"/> is <see langword="null"/>.</exception>
internal sealed class FireRegistry(ModLog log)
{
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    /// <summary>Registers a freshly created fire so the game saves it.</summary>
    /// <param name="fireObject">The instantiated fire barrel or rim grill.</param>
    /// <param name="guid">Guid from <see cref="FireIdentity.NewGuid"/>.</param>
    public void Register(GameObject fireObject, string guid)
    {
        Placeable placeable = fireObject.GetComponent<Placeable>();
        if (placeable == null)
        {
            _log.Warning($"Prefab '{fireObject.name}' has no Placeable; it will not be saved.");
        }
        else
        {
            RegisterPlaceable(placeable, guid);
        }

        AssignObjectGuids(fireObject, guid);
        PrioritizeDecorationInteraction(fireObject);
        ClearStartingCharcoal(fireObject);

        // A fresh fire has never burnt, so it starts with nothing to harvest. This state is saved by the game,
        // so it is done only here, never on restore (that would wipe charcoal the player has since earned).
        Fire fire = fireObject.GetComponentInChildren<Fire>(true);
        if (fire != null)
        {
            fire.RemoveAllCharcoal();
        }
    }

    /// <summary>Gives a fire the game re-created from a save back its identity and puts it on the floor.</summary>
    /// <param name="fireObject">The re-created fire barrel or rim grill.</param>
    /// <param name="guid">The guid it was saved under.</param>
    public void Restore(GameObject fireObject, string guid)
    {
        AssignObjectGuids(fireObject, guid);
        PrioritizeDecorationInteraction(fireObject);
        ClearStartingCharcoal(fireObject);
        // Fire barrels crafted by pre-release builds could have been saved floating.
        GroundAligner.SnapDown(fireObject);
        _log.Debug($"Restored '{fireObject.name}' {guid} at {fireObject.transform.position}.");
    }

    /// <summary>
    /// Makes edit mode pick the decoration itself rather than its children. The vanilla rim grill prefab has this
    /// off (the fire barrel has it on), so its rack and cooking slots swallow the click and it cannot be moved.
    /// It is a prefab value, not saved by the game, hence it is re-applied on every restore.
    /// </summary>
    private static void PrioritizeDecorationInteraction(GameObject fireObject)
    {
        DecorationItem decoration = fireObject.GetComponent<DecorationItem>();
        if (decoration != null)
        {
            decoration.m_PrioritizeInteractionOverChildren = true;
        }
    }

    /// <summary>
    /// Vanilla fire prefabs come with charcoal already inside, as if found used in the world. A fire the player just
    /// made has not burnt yet, so it must not. It is a prefab value, not saved by the game, hence it is re-applied
    /// on every restore. Charcoal from actually burning fuel is unaffected.
    /// </summary>
    private static void ClearStartingCharcoal(GameObject fireObject)
    {
        Fire fire = fireObject.GetComponentInChildren<Fire>(true);
        if (fire != null)
        {
            fire.m_NumStartingCharcoalPieces = 0;
        }
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
            _log.Warning($"Placeable of '{placeable.name}' has no valid addressable; it may not survive a reload.");
        }

        _log.Debug($"Placeable registered: '{previousGuid}' -> '{guid}'.");
    }

    private void AssignObjectGuids(GameObject fireObject, string guid)
    {
        ObjectGuid.MaybeAttachObjectGuidAddOrReplace(fireObject, guid);

        Fire fire = fireObject.GetComponentInChildren<Fire>(true);
        if (fire == null)
        {
            _log.Warning($"'{fireObject.name}' has no Fire component.");
            return;
        }

        ObjectGuid.MaybeAttachObjectGuidAddOrReplace(fire.gameObject, FireIdentity.FireGuidFor(guid));
    }
}
