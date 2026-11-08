using FireBarrelCrafting.Infrastructure;
using Il2CppTLD.Placement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FireBarrelCrafting.Spawning;

/// <summary>Creates vanilla fire barrels in the world (Factory pattern / GRASP: Creator).</summary>
/// <param name="registry">Registers created barrels with the game's save systems.</param>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
internal sealed class FireBarrelFactory(FireBarrelRegistry registry, ModLog log)
{
    /// <summary>Addressables key of the vanilla fire barrel prefab (from the game's catalog.json).</summary>
    public const string PrefabAddress = "Assets/Prefabs/Interactive/INTERACTIVE_FireBarrel.prefab";

    private readonly FireBarrelRegistry _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    // Loaded once and kept for the session: the prefab is small and conversions should not stall on loading.
    private GameObject? _prefab;

    /// <summary>Gets a value indicating whether the fire barrel prefab can be loaded.</summary>
    public bool IsPrefabAvailable => LoadPrefab() != null;

    /// <summary>Creates a fire barrel, rests it on the floor and registers it so the game saves it.</summary>
    /// <param name="position">World position for the prefab pivot.</param>
    /// <param name="rotation">World rotation.</param>
    /// <param name="floorHeight">World Y the barrel's bottom should rest on; <see langword="null"/> keeps the pivot at <paramref name="position"/>.</param>
    /// <returns>The guid the new barrel is saved under.</returns>
    /// <exception cref="InvalidOperationException">The prefab cannot be loaded; check <see cref="IsPrefabAvailable"/> first.</exception>
    public string Create(Vector3 position, Quaternion rotation, float? floorHeight)
    {
        GameObject? prefab = LoadPrefab();
        if (prefab == null)
        {
            throw new InvalidOperationException($"Prefab '{PrefabAddress}' is not available.");
        }

        GameObject fireBarrel = UnityEngine.Object.Instantiate(prefab, position, rotation).Cast<GameObject>();
        fireBarrel.name = prefab.name;
        fireBarrel.transform.SetParent(PlaceableManager.FindOrCreateCategoryRoot().transform, true);

        if (floorHeight is float floor)
        {
            GroundAligner.AlignBottomTo(fireBarrel, floor);
        }
        GroundAligner.SnapDown(fireBarrel);

        string guid = FireBarrelIdentity.NewGuid();
        _registry.Register(fireBarrel, guid);
        return guid;
    }

    private GameObject? LoadPrefab()
    {
        if (_prefab != null)
        {
            return _prefab;
        }

        try
        {
            _prefab = Addressables.LoadAssetAsync<GameObject>(PrefabAddress).WaitForCompletion();
        }
        catch (Exception e)
        {
            // Il2CppInterop surfaces every native failure as Il2CppException; there is no narrower type to catch.
            _log.Error($"Loading '{PrefabAddress}' threw", e);
        }

        if (_prefab == null)
        {
            _log.Error($"Could not load fire barrel prefab '{PrefabAddress}'. The game may have renamed it.");
            return null;
        }

        return _prefab;
    }
}
