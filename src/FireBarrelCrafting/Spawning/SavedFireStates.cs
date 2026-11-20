using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FireBarrelCrafting.Spawning;

/// <summary>
/// Keeps the saved fire states of a scene until our fires exist to take them.
/// </summary>
/// <remarks>
/// When a scene loads, the game restores fires (<c>FireManager.Deserialize</c>) before it re-creates placed objects,
/// so the fire of a crafted fire barrel or rim grill does not exist yet and its saved state is skipped. The states are
/// kept here and handed to the fire once the object has been re-created and given its guid.
/// </remarks>
internal sealed class SavedFireStates
{
    private readonly Dictionary<string, string> _statesByFireGuid = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Replaces the kept states with those of the scene being loaded.</summary>
    /// <param name="fireManagerJson">
    /// The game's fire save data: <c>{"m_SerializedFires":[{"m_SearializedFire":"…","m_Guid":"…"}, …]}</c>
    /// ("Searialized" is the game's spelling). <see langword="null"/>, empty or malformed data keeps nothing.
    /// </param>
    /// <returns>The number of fire states kept.</returns>
    public int Capture(string? fireManagerJson)
    {
        _statesByFireGuid.Clear();
        if (string.IsNullOrEmpty(fireManagerJson))
        {
            return 0;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(fireManagerJson);
            if (!document.RootElement.TryGetProperty("m_SerializedFires", out JsonElement fires) || fires.ValueKind != JsonValueKind.Array)
            {
                return 0;
            }

            foreach (JsonElement fire in fires.EnumerateArray())
            {
                if (TryGetString(fire, "m_Guid", out string? guid) && TryGetString(fire, "m_SearializedFire", out string? state))
                {
                    _statesByFireGuid[guid] = state;
                }
            }
        }
        catch (JsonException)
        {
            // Not the format this mod knows: keep nothing, the fires simply start unlit as before.
            _statesByFireGuid.Clear();
        }

        return _statesByFireGuid.Count;
    }

    /// <summary>Takes the saved state of a fire; each state can be taken once.</summary>
    /// <param name="fireGuid">The fire's guid, see <see cref="FireIdentity.FireGuidFor"/>.</param>
    /// <param name="state">The state in the game's format, for <c>Fire.Deserialize</c>.</param>
    /// <returns><see langword="true"/> if the loaded scene had a saved state for this fire.</returns>
    public bool TryTake(string fireGuid, [NotNullWhen(true)] out string? state) => _statesByFireGuid.Remove(fireGuid, out state);

    private static bool TryGetString(JsonElement element, string name, [NotNullWhen(true)] out string? value)
    {
        value = element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out JsonElement property)
            && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        return !string.IsNullOrEmpty(value);
    }
}
