using FireBarrelCrafting.Conversion;

namespace FireBarrelCrafting.Tests;

[TestClass]
public sealed class ToolNamesTests
{
    [TestMethod]
    [DataRow("GEAR_Hacksaw", "Hacksaw")]
    [DataRow("gear_hatchetimprovised", "Improvised Hatchet")]
    [DataRow("GEAR_SomethingNew", "SomethingNew")]
    [DataRow("CustomTool", "CustomTool")]
    public void Friendly_GearId_ReturnsReadableName(string gearName, string expected)
    {
        // Act
        string result = ToolNames.Friendly(gearName);

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void Friendly_Null_Throws()
    {
        // Act
        Action act = () => ToolNames.Friendly(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void DescribeAlternatives_SeveralTools_JoinsWithCommasAndOr()
    {
        // Arrange
        string[] tools = ["GEAR_Hacksaw", "GEAR_Hatchet", "GEAR_HatchetImprovised"];

        // Act
        string result = ToolNames.DescribeAlternatives(tools);

        // Assert
        result.Should().Be("Hacksaw, Hatchet or Improvised Hatchet");
    }

    [TestMethod]
    public void DescribeAlternatives_SingleTool_ReturnsItsName()
    {
        // Act
        string result = ToolNames.DescribeAlternatives(["GEAR_Hacksaw"]);

        // Assert
        result.Should().Be("Hacksaw");
    }

    [TestMethod]
    public void DescribeAlternatives_NoTools_ReturnsGenericWord()
    {
        // Act
        string result = ToolNames.DescribeAlternatives([]);

        // Assert
        result.Should().Be("tool");
    }

    [TestMethod]
    public void DescribeAlternatives_Null_Throws()
    {
        // Act
        Action act = () => ToolNames.DescribeAlternatives(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
