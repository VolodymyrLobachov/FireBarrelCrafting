using FireBarrelCrafting.Configuration;
using FireBarrelCrafting.Conversion;
using FireBarrelCrafting.Infrastructure;
using FireBarrelCrafting.Interaction;
using FireBarrelCrafting.Spawning;
using MelonLoader;

// BuildInfo is generated from the project file (Version, Authors) at build time.
[assembly: MelonInfo(typeof(FireBarrelCrafting.Mod), FireBarrelCrafting.BuildInfo.Name, FireBarrelCrafting.BuildInfo.Version, FireBarrelCrafting.BuildInfo.Author)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace FireBarrelCrafting;

/// <summary>
/// MelonLoader entry point and composition root: builds the object graph once.
/// Harmony patches are static by nature, so they reach the graph through <see cref="Services"/>.
/// </summary>
public sealed class Mod : MelonMod
{
    private const string SettingsMenuTitle = "Fire Barrel Crafting";

    /// <summary>Gets the services used by the Harmony patches; <see langword="null"/> until the mod has initialised.</summary>
    internal static ModServices? Services { get; private set; }

    /// <summary>Creates the settings and all services. Called once by MelonLoader.</summary>
    public override void OnInitializeMelon()
    {
        var settings = new FireBarrelSettings();
        settings.AddToModSettings(SettingsMenuTitle);
        var log = new ModLog(LoggerInstance, () => settings.VerboseLogging);

        var detector = new BarrelDetector();
        var registry = new FireRegistry(log);
        var factory = new FireFactory(registry, log);
        var tools = new CraftingTools(settings, log);
        var conversion = new BarrelConversionService(settings, detector, tools, factory, log);
        var wheel = new ConversionWheel(detector, conversion, log);

        Services = new ModServices(wheel, registry, log);
        log.Info($"v{BuildInfo.Version} loaded. Tools: {string.Join(", ", settings.AllowedTools)}.");
    }
}

/// <summary>What the Harmony patches need, and nothing more.</summary>
/// <param name="Wheel">Adds the "Fire Barrel" and "Rim Grill" entries to barrel interaction wheels.</param>
/// <param name="Registry">Restores our fire barrels and rim grills when a save is loaded.</param>
/// <param name="Log">Mod logger.</param>
internal sealed record ModServices(ConversionWheel Wheel, FireRegistry Registry, ModLog Log);
