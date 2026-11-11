using System.Security.Cryptography;
using System.Text;

namespace FireBarrelCrafting.Spawning;

/// <summary>
/// Guid scheme for fires (fire barrels, rim grills) made by this mod. The marker prefix lets us recognise our objects
/// after a reload without any save data of our own; the fire guid is derived so it is identical on every load.
/// </summary>
internal static class FireIdentity
{
    /// <summary>Valid hex, so the result is still a well-formed guid.</summary>
    private const string Marker = "fbc0ffee";

    /// <summary>Creates a new, unique object guid carrying this mod's marker.</summary>
    /// <returns>A guid in <c>D</c> format starting with the marker.</returns>
    public static string NewGuid() => string.Concat(Marker, Guid.NewGuid().ToString("D").AsSpan(Marker.Length));

    /// <summary>Checks whether a placeable guid was created by <see cref="NewGuid"/>.</summary>
    /// <param name="guid">Guid to check; may be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the guid belongs to an object made by this mod.</returns>
    public static bool IsModGuid(string? guid) =>
        guid is not null && guid.StartsWith(Marker, StringComparison.OrdinalIgnoreCase);

    /// <summary>Derives the guid of the object's <c>Fire</c> child, which FireManager saves fuel/burn state by.</summary>
    /// <param name="objectGuid">The fire barrel's or rim grill's guid.</param>
    /// <returns>A deterministic guid in <c>D</c> format, different from <paramref name="objectGuid"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectGuid"/> is <see langword="null"/>.</exception>
    public static string FireGuidFor(string objectGuid)
    {
        ArgumentNullException.ThrowIfNull(objectGuid);
        // The "/fire" salt is part of existing saves: changing it would orphan the burn state of every crafted fire.
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(objectGuid + "/fire"))).ToString("D");
    }
}
