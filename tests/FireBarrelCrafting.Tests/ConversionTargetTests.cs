using FireBarrelCrafting.Conversion;

namespace FireBarrelCrafting.Tests;

[TestClass]
public sealed class ConversionTargetTests
{
    /// <summary>A wheel slot fits about this many characters ("Craft Fire Barrel", 17, overflowed).</summary>
    private const int MaxWheelLabelLength = 16;

    [TestMethod]
    public void All_Targets_HaveUniqueLocIdsAndPrefabs()
    {
        // Act
        var locIds = ConversionTarget.All.Select(target => target.WheelLocId);
        var prefabs = ConversionTarget.All.Select(target => target.PrefabAddress);

        // Assert
        locIds.Should().OnlyHaveUniqueItems();
        prefabs.Should().OnlyHaveUniqueItems();
    }

    [TestMethod]
    public void All_Targets_HaveLabelsThatFitAWheelSlot()
    {
        // Act
        var labelLengths = ConversionTarget.All.Select(target => target.WheelLabel.Length);

        // Assert
        labelLengths.Should().OnlyContain(length => length > 0 && length <= MaxWheelLabelLength);
    }

    [TestMethod]
    [DataRow("FBC_CraftFireBarrel")]
    [DataRow("FBC_CraftRimGrill")]
    public void FindByLocId_OwnLocId_ReturnsThatTarget(string locId)
    {
        // Act
        ConversionTarget? result = ConversionTarget.FindByLocId(locId);

        // Assert
        result.Should().NotBeNull();
        result!.WheelLocId.Should().Be(locId);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("GAMEPLAY_Destroy")]
    public void FindByLocId_ForeignLocId_ReturnsNull(string? locId)
    {
        // Act
        ConversionTarget? result = ConversionTarget.FindByLocId(locId);

        // Assert
        result.Should().BeNull();
    }
}
