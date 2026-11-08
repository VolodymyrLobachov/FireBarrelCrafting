using FireBarrelCrafting.Configuration;
using FireBarrelCrafting.Conversion;
using FireBarrelCrafting.Infrastructure;
using FireBarrelCrafting.Interaction;
using FireBarrelCrafting.Spawning;
using MelonLoader;

namespace FireBarrelCrafting.Tests;

/// <summary>Every service rejects a missing dependency up front instead of failing later in-game.</summary>
[TestClass]
public sealed class ConstructorGuardTests
{
    private static readonly ModSettings Settings = Placeholder.Of<ModSettings>();
    private static readonly ModLog Log = Placeholder.Of<ModLog>();
    private static readonly BarrelDetector Detector = Placeholder.Of<BarrelDetector>();
    private static readonly CraftingTools Tools = Placeholder.Of<CraftingTools>();
    private static readonly FireBarrelRegistry Registry = Placeholder.Of<FireBarrelRegistry>();
    private static readonly FireBarrelFactory Factory = Placeholder.Of<FireBarrelFactory>();
    private static readonly BarrelConversionService Conversion = Placeholder.Of<BarrelConversionService>();

    private static IEnumerable<object[]> MissingDependencies =>
    [
        Case("logger", () => new ModLog(null!, () => false)),
        Case("isVerbose", () => new ModLog(Placeholder.Of<MelonLogger.Instance>(), null!)),
        Case("settings", () => new BarrelDetector(null!)),
        Case("settings", () => new CraftingTools(null!, Log)),
        Case("log", () => new CraftingTools(Settings, null!)),
        Case("log", () => new FireBarrelRegistry(null!)),
        Case("registry", () => new FireBarrelFactory(null!, Log)),
        Case("log", () => new FireBarrelFactory(Registry, null!)),
        Case("settings", () => new BarrelConversionService(null!, Detector, Tools, Factory, Log)),
        Case("detector", () => new BarrelConversionService(Settings, null!, Tools, Factory, Log)),
        Case("tools", () => new BarrelConversionService(Settings, Detector, null!, Factory, Log)),
        Case("factory", () => new BarrelConversionService(Settings, Detector, Tools, null!, Log)),
        Case("log", () => new BarrelConversionService(Settings, Detector, Tools, Factory, null!)),
        Case("detector", () => new ConversionWheel(null!, Conversion, Log)),
        Case("conversion", () => new ConversionWheel(Detector, null!, Log)),
        Case("log", () => new ConversionWheel(Detector, Conversion, null!)),
    ];

    [TestMethod]
    [DynamicData(nameof(MissingDependencies))]
    public void Constructor_MissingDependency_ThrowsArgumentNullException(string parameterName, Action construct)
    {
        // Act
        Action act = construct;

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(parameterName);
    }

    [TestMethod]
    public void Constructors_AllDependenciesPresent_DoNotThrow()
    {
        // Act
        Action act = () =>
        {
            _ = new BarrelDetector(Settings);
            _ = new CraftingTools(Settings, Log);
            _ = new FireBarrelRegistry(Log);
            _ = new FireBarrelFactory(Registry, Log);
            _ = new BarrelConversionService(Settings, Detector, Tools, Factory, Log);
            _ = new ConversionWheel(Detector, Conversion, Log);
        };

        // Assert
        act.Should().NotThrow();
    }

    private static object[] Case(string parameterName, Action construct) => [parameterName, construct];
}
