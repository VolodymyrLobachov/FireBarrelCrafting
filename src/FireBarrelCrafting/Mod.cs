using FireBarrelCrafting.Configuration;
using FireBarrelCrafting.Conversion;
using FireBarrelCrafting.Infrastructure;
using FireBarrelCrafting.Interaction;
using FireBarrelCrafting.Spawning;
using MelonLoader;

[assembly: MelonInfo(typeof(FireBarrelCrafting.Mod), FireBarrelCrafting.BuildInfo.Name, FireBarrelCrafting.BuildInfo.Version, FireBarrelCrafting.BuildInfo.Author)]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace FireBarrelCrafting;

/// <summary>Assembly metadata shown by MelonLoader.</summary>
internal static class BuildInfo
{
    /// <summary>Mod name.</summary>
    public const string Name = "FireBarrelCrafting";

    /// <summary>Mod version; keep in sync with <c>Version</c> in the project file.</summary>
    public const string Version = "1.0.0";

    /// <summary>Mod author.</summary>
    public const string Author = "VolodymyrLobachov";
}

/// <summary>
/// MelonLoader entry point and composition root: builds the object graph once.
/// Harmony patches are static by nature, so they reach the graph through <see cref="Services"/>.
/// </summary>
public sealed class Mod : MelonMod
{
    /// <summary>Gets the services used by the Harmony patches; <see langword="null"/> until the mod has initialised.</summary>
    internal static ModServices? Services { get; private set; }

    /// <summary>Creates the settings and all services. Called once by MelonLoader.</summary>
    public override void OnInitializeMelon()
    {
        var settings = ModSettings.Create();
        var log = new ModLog(LoggerInstance, () => settings.VerboseLogging);

        var detector = new BarrelDetector(settings);
        var registry = new FireBarrelRegistry(log);
        var factory = new FireBarrelFactory(registry, log);
        var tools = new CraftingTools(settings, log);
        var conversion = new BarrelConversionService(settings, detector, tools, factory, log);
        var wheel = new ConversionWheel(detector, conversion, log);

        Services = new ModServices(wheel, registry, log);
        log.Info($"v{BuildInfo.Version} loaded. Tools: {string.Join(", ", settings.AllowedTools)}.");
    }
}

/// <summary>What the Harmony patches need, and nothing more.</summary>
/// <param name="Wheel">Opens the Destroy / Fire Barrel wheel.</param>
/// <param name="Registry">Restores our fire barrels when a save is loaded.</param>
/// <param name="Log">Mod logger.</param>
internal sealed record ModServices(ConversionWheel Wheel, FireBarrelRegistry Registry, ModLog Log);
