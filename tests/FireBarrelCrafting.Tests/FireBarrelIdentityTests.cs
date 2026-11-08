using FireBarrelCrafting.Spawning;

namespace FireBarrelCrafting.Tests;

[TestClass]
public sealed class FireBarrelIdentityTests
{
    [TestMethod]
    public void NewGuid_ReturnsWellFormedGuidRecognisedAsOurs()
    {
        // Act
        string guid = FireBarrelIdentity.NewGuid();

        // Assert
        Guid.TryParseExact(guid, "D", out _).Should().BeTrue();
        FireBarrelIdentity.IsModGuid(guid).Should().BeTrue();
    }

    [TestMethod]
    public void NewGuid_CalledTwice_ReturnsDifferentValues()
    {
        // Act
        string first = FireBarrelIdentity.NewGuid();
        string second = FireBarrelIdentity.NewGuid();

        // Assert
        first.Should().NotBe(second);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("3f2504e0-4f89-11d3-9a0c-0305e82c3301")]
    public void IsModGuid_ForeignOrMissingGuid_ReturnsFalse(string? guid)
    {
        // Act
        bool result = FireBarrelIdentity.IsModGuid(guid);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void FireGuidFor_SameBarrel_IsDeterministicAndDistinct()
    {
        // Arrange
        string barrel = FireBarrelIdentity.NewGuid();

        // Act
        string first = FireBarrelIdentity.FireGuidFor(barrel);
        string second = FireBarrelIdentity.FireGuidFor(barrel);

        // Assert
        first.Should().Be(second).And.NotBe(barrel);
        Guid.TryParseExact(first, "D", out _).Should().BeTrue();
    }

    [TestMethod]
    public void FireGuidFor_DifferentBarrels_ReturnsDifferentGuids()
    {
        // Act
        string first = FireBarrelIdentity.FireGuidFor(FireBarrelIdentity.NewGuid());
        string second = FireBarrelIdentity.FireGuidFor(FireBarrelIdentity.NewGuid());

        // Assert
        first.Should().NotBe(second);
    }

    [TestMethod]
    public void FireGuidFor_Null_Throws()
    {
        // Act
        Action act = () => FireBarrelIdentity.FireGuidFor(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("barrelGuid");
    }
}
