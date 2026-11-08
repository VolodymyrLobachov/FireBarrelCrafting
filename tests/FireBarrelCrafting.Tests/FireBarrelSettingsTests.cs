using FireBarrelCrafting.Configuration;

namespace FireBarrelCrafting.Tests;

/// <summary>The validated properties must stay sane even if the JSON file was edited by hand.</summary>
[TestClass]
public sealed class FireBarrelSettingsTests
{
    [TestMethod]
    [DataRow(-5, 0f)]
    [DataRow(10, 10f)]
    [DataRow(250, 100f)]
    public void ToolConditionLossPercent_ClampsToZeroToHundred(int raw, float expected)
    {
        // Arrange
        FireBarrelSettings settings = Placeholder.Of<FireBarrelSettings>();
        settings.ToolWearPercent = raw;

        // Act
        float result = settings.ToolConditionLossPercent;

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(-30, 0)]
    [DataRow(0, 0)]
    [DataRow(60, 60)]
    public void CraftMinutes_IsNeverNegative(int raw, int expected)
    {
        // Arrange
        FireBarrelSettings settings = Placeholder.Of<FireBarrelSettings>();
        settings.CraftTimeMinutes = raw;

        // Act
        int result = settings.CraftMinutes;

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void AllowedTools_ListsOnlyEnabledToolsInMenuOrder()
    {
        // Arrange
        FireBarrelSettings settings = Placeholder.Of<FireBarrelSettings>();
        settings.AllowHacksaw = true;
        settings.AllowHatchet = false;
        settings.AllowImprovisedHatchet = true;
        settings.AllowPrybar = true;

        // Act
        IReadOnlyList<string> tools = settings.AllowedTools;

        // Assert
        tools.Should().Equal("GEAR_Hacksaw", "GEAR_HatchetImprovised", "GEAR_Prybar");
    }

    [TestMethod]
    public void AllowedTools_NothingEnabled_IsEmpty()
    {
        // Arrange
        FireBarrelSettings settings = Placeholder.Of<FireBarrelSettings>();

        // Act
        IReadOnlyList<string> tools = settings.AllowedTools;

        // Assert
        tools.Should().BeEmpty();
    }
}
