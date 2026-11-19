using FireBarrelCrafting.Conversion;

namespace FireBarrelCrafting.Tests;

[TestClass]
public sealed class ConversionSourcesTests
{
    [TestMethod]
    [DataRow("OBJ_MetalBarrelA_Prefab")]
    [DataRow("OBJ_MetalBarrelA_Prefab (12)")]
    [DataRow("OBJ_MetalBarrelA_Prefab (PLACED)")]
    public void TargetsFor_MetalBarrelWithRimGrillAllowed_ReturnsBothTargets(string name)
    {
        // Act
        var result = ConversionSources.TargetsFor(name, rimGrillFromBarrels: true);

        // Assert
        result.Should().Equal(ConversionTarget.FireBarrel, ConversionTarget.RimGrill);
    }

    [TestMethod]
    public void TargetsFor_MetalBarrelWithRimGrillDisallowed_ReturnsOnlyFireBarrel()
    {
        // Act
        var result = ConversionSources.TargetsFor("OBJ_MetalBarrelA_Prefab", rimGrillFromBarrels: false);

        // Assert
        result.Should().Equal(ConversionTarget.FireBarrel);
    }

    [TestMethod]
    [DataRow("OBJ_TireA_Prefab", true)]
    [DataRow("OBJ_TireA_Prefab (PLACED)", false)]
    [DataRow("obj_tirea_prefab (3)", true)]
    public void TargetsFor_WheelWithRim_ReturnsOnlyRimGrill(string name, bool rimGrillFromBarrels)
    {
        // Act
        var result = ConversionSources.TargetsFor(name, rimGrillFromBarrels);

        // Assert
        result.Should().Equal(ConversionTarget.RimGrill);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("OBJ_TireB_Prefab (PLACED)")]
    [DataRow("OBJ_TireRackA_Prefab")]
    [DataRow("OBJ_PlasticBarrelA_Prefab")]
    [DataRow("OBJ_WagonWheelA_Prefab")]
    public void TargetsFor_OtherObject_ReturnsNothing(string? name)
    {
        // Act
        var result = ConversionSources.TargetsFor(name, rimGrillFromBarrels: true);

        // Assert
        result.Should().BeEmpty();
    }
}
