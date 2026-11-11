using FireBarrelCrafting.Infrastructure;
using Il2CppTLD.Placement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FireBarrelCrafting.Spawning;

/// <summary>Creates vanilla fires (fire barrel, rim grill) in the world (Factory pattern / GRASP: Creator).</summary>
/// <param name="registry">Registers created objects with the game's save systems.</param>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
internal sealed class FireFactory(FireRegistry registry, ModLog log)
{
    private readonly FireRegistry _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    // Loaded once and kept for the session: the prefabs are small and conversions should not stall on loading.
    private readonly Dictionary<string, GameObject> _prefabs = [];

    /// <summary>Checks whether a prefab can be loaded.</summary>
    /// <param name="prefabAddress">Addressables key of the prefab.</param>
    /// <returns><see langword="true"/> if <see cref="Create"/> can be called with this address.</returns>
    public bool IsPrefabAvailable(string prefabAddress) => LoadPrefab(prefabAddress) != null;

    /// <summary>Creates the object, rests it on the floor and registers it so the game saves it.</summary>
    /// <param name="prefabAddress">Addressables key of the prefab; it must have a <c>Placeable</c> and a child <c>Fire</c>.</param>
    /// <param name="position">World position for the prefab pivot.</param>
    /// <param name="rotation">World rotation.</param>
    /// <param name="floorHeight">World Y the object's bottom should rest on; <see langword="null"/> keeps the pivot at <paramref name="position"/>.</param>
    /// <returns>The guid the new object is saved under.</returns>
    /// <exception cref="InvalidOperationException">The prefab cannot be loaded; check <see cref="IsPrefabAvailable"/> first.</exception>
    public string Create(string prefabAddress, Vector3 position, Quaternion rotation, float? floorHeight)
    {
        GameObject? prefab = LoadPrefab(prefabAddress);
        if (prefab == null)
        {
            throw new InvalidOperationException($"Prefab '{prefabAddress}' is not available.");
        }

        GameObject created = UnityEngine.Object.Instantiate(prefab, position, rotation).Cast<GameObject>();
        created.name = prefab.name;
        created.transform.SetParent(PlaceableManager.FindOrCreateCategoryRoot().transform, true);

        if (floorHeight is float floor)
        {
            GroundAligner.AlignBottomTo(created, floor);
        }
        GroundAligner.SnapDown(created);

        string guid = FireIdentity.NewGuid();
        _registry.Register(created, guid);
        return guid;
    }

    private GameObject? LoadPrefab(string prefabAddress)
    {
        if (_prefabs.TryGetValue(prefabAddress, out GameObject? cached) && cached != null)
        {
            return cached;
        }

        GameObject? prefab = null;
        try
        {
            prefab = Addressables.LoadAssetAsync<GameObject>(prefabAddress).WaitForCompletion();
        }
        catch (Exception e)
        {
            // Il2CppInterop surfaces every native failure as Il2CppException; there is no narrower type to catch.
            _log.Error($"Loading '{prefabAddress}' threw", e);
        }

        if (prefab == null)
        {
            _log.Error($"Could not load prefab '{prefabAddress}'. The game may have renamed it.");
            return null;
        }

        _prefabs[prefabAddress] = prefab;
        return prefab;
    }
}
