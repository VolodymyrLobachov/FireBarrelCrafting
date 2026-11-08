using System.Security.Cryptography;
using System.Text;

namespace FireBarrelCrafting.Spawning;

/// <summary>
/// Guid scheme for fire barrels made by this mod. The marker prefix lets us recognise our barrels after a
/// reload without any save data of our own; the fire guid is derived so it is identical on every load.
/// </summary>
internal static class FireBarrelIdentity
{
    /// <summary>Valid hex, so the result is still a well-formed guid.</summary>
    private const string Marker = "fbc0ffee";

    /// <summary>Creates a new, unique barrel guid carrying this mod's marker.</summary>
    /// <returns>A guid in <c>D</c> format starting with the marker.</returns>
    public static string NewGuid() => string.Concat(Marker, Guid.NewGuid().ToString("D").AsSpan(Marker.Length));

    /// <summary>Checks whether a placeable guid was created by <see cref="NewGuid"/>.</summary>
    /// <param name="guid">Guid to check; may be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the guid belongs to a fire barrel made by this mod.</returns>
    public static bool IsModGuid(string? guid) =>
        guid is not null && guid.StartsWith(Marker, StringComparison.OrdinalIgnoreCase);

    /// <summary>Derives the guid of the barrel's <c>Fire</c> child, which FireManager saves fuel/burn state by.</summary>
    /// <param name="barrelGuid">The barrel's guid.</param>
    /// <returns>A deterministic guid in <c>D</c> format, different from <paramref name="barrelGuid"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="barrelGuid"/> is <see langword="null"/>.</exception>
    public static string FireGuidFor(string barrelGuid)
    {
        ArgumentNullException.ThrowIfNull(barrelGuid);
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(barrelGuid + "/fire"))).ToString("D");
    }
}
