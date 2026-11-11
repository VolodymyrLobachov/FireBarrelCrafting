using FireBarrelCrafting.Spawning;

namespace FireBarrelCrafting.Tests;

[TestClass]
public sealed class FireIdentityTests
{
    /// <summary>
    /// A real guid from a save made with this mod (fire barrel crafted in 1.0.0 testing).
    /// Saves contain these values, so the pinned tests below must never be "fixed" by changing the expectation:
    /// if they fail, the code broke compatibility with existing saves.
    /// </summary>
    private const string SavedObjectGuid = "fbc0ffee-ca44-428a-9048-a09293b0a9e0";

    /// <summary>The fire guid the game stored for <see cref="SavedObjectGuid"/>'s burn state.</summary>
    private const string SavedFireGuid = "4980a41f-08b4-1ca6-5f97-80d82a9f814b";

    [TestMethod]
    public void NewGuid_ReturnsWellFormedGuidRecognisedAsOurs()
    {
        // Act
        string guid = FireIdentity.NewGuid();

        // Assert
        Guid.TryParseExact(guid, "D", out _).Should().BeTrue();
        FireIdentity.IsModGuid(guid).Should().BeTrue();
    }

    [TestMethod]
    public void IsModGuid_GuidFromExistingSave_ReturnsTrue()
    {
        // Act
        bool result = FireIdentity.IsModGuid(SavedObjectGuid);

        // Assert
        result.Should().BeTrue("objects crafted by earlier versions must still be restored after an update");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("3f2504e0-4f89-11d3-9a0c-0305e82c3301")]
    public void IsModGuid_ForeignOrMissingGuid_ReturnsFalse(string? guid)
    {
        // Act
        bool result = FireIdentity.IsModGuid(guid);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void FireGuidFor_GuidFromExistingSave_ReturnsTheSavedFireGuid()
    {
        // Act
        string result = FireIdentity.FireGuidFor(SavedObjectGuid);

        // Assert
        result.Should().Be(SavedFireGuid, "the burn state of fires in existing saves is stored under this guid");
    }

    [TestMethod]
    public void FireGuidFor_DifferentObjects_ReturnsDifferentGuids()
    {
        // Act
        string first = FireIdentity.FireGuidFor(FireIdentity.NewGuid());
        string second = FireIdentity.FireGuidFor(FireIdentity.NewGuid());

        // Assert
        first.Should().NotBe(second);
    }
}
