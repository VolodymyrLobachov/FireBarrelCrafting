using FireBarrelCrafting.Configuration;

namespace FireBarrelCrafting.Tests;

[TestClass]
public sealed class SettingListTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow(" , ;, ")]
    public void Parse_BlankInput_ReturnsEmpty(string? value)
    {
        // Act
        IReadOnlyList<string> result = SettingList.Parse(value);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_MixedSeparatorsAndSpaces_ReturnsTrimmedItemsInOrder()
    {
        // Arrange
        const string value = " GEAR_Hacksaw ,GEAR_Hatchet; GEAR_Prybar ";

        // Act
        IReadOnlyList<string> result = SettingList.Parse(value);

        // Assert
        result.Should().Equal("GEAR_Hacksaw", "GEAR_Hatchet", "GEAR_Prybar");
    }

    [TestMethod]
    public void Parse_CaseInsensitiveDuplicates_KeepsFirstOccurrence()
    {
        // Arrange
        const string value = "GEAR_Hacksaw,gear_hacksaw,GEAR_Hatchet";

        // Act
        IReadOnlyList<string> result = SettingList.Parse(value);

        // Assert
        result.Should().Equal("GEAR_Hacksaw", "GEAR_Hatchet");
    }
}
