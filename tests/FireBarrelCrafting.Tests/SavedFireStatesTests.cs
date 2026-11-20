using FireBarrelCrafting.Spawning;

namespace FireBarrelCrafting.Tests;

[TestClass]
public sealed class SavedFireStatesTests
{
    /// <summary>Fire guid of a crafted fire barrel in a real save (object guid fbc0ffee-54cc-4a09-a2b3-c4a7248a0aae).</summary>
    private const string CraftedFireGuid = "80b4d93f-a946-188a-1175-d3b5ca19e6e2";

    /// <summary>Its state, verbatim from that save.</summary>
    private const string CraftedFireState =
        """{"m_HoursPlayed":9.447603,"m_Position":[1.19532585,1.14345324,5.637146],"m_Rotation":[0.0,0.7918113,0.0,-0.6107658],"m_FireStateProxy":"FullBurn","m_ElapsedOnTODSecondsProxy":179.923431,"m_ElapsedOnTODSecondsUnmodifiedProxy":179.923431,"m_MaxOnTODSecondsProxy":3600.0,"m_FuelHeatIncrease":6.0,"m_FullBurnTriggered":true,"m_StartedByPlayer":true,"m_TrackedBurntItems":[]}""";

    /// <summary>The game's fire save data for a scene, in the shape of that save (the vanilla fire's state shortened).</summary>
    private static readonly string FireManagerJson =
        "{\"m_SerializedFires\":["
        + "{\"m_Position\":[-2.612,-0.054,-6.802],\"m_InWoodStove\":true,\"m_SearializedFire\":\"{\\\"m_FireStateProxy\\\":\\\"FullBurn\\\"}\",\"m_Guid\":\"38d710bd-88ee-4f93-b22c-ee1358b44734\"},"
        + "{\"m_Position\":[1.19532585,0.009999871,5.637146],\"m_InWoodStove\":true,\"m_SearializedFire\":"
        + System.Text.Json.JsonSerializer.Serialize(CraftedFireState)
        + ",\"m_Guid\":\"" + CraftedFireGuid + "\"}"
        + "],\"m_DestroyedFires\":[]}";

    [TestMethod]
    public void TryTake_FireFromCapturedScene_ReturnsItsSavedStateUnchanged()
    {
        // Arrange
        var states = new SavedFireStates();
        states.Capture(FireManagerJson);

        // Act
        bool found = states.TryTake(CraftedFireGuid, out string? state);

        // Assert
        found.Should().BeTrue();
        state.Should().Be(CraftedFireState);
    }

    [TestMethod]
    public void TryTake_SameFireTwice_ReturnsItsStateOnlyOnce()
    {
        // Arrange
        var states = new SavedFireStates();
        states.Capture(FireManagerJson);
        states.TryTake(CraftedFireGuid, out _);

        // Act
        bool foundAgain = states.TryTake(CraftedFireGuid, out _);

        // Assert
        foundAgain.Should().BeFalse("a reloaded scene must not get the state of an earlier load");
    }

    [TestMethod]
    public void Capture_NextScene_DropsStatesOfThePreviousScene()
    {
        // Arrange
        var states = new SavedFireStates();
        states.Capture(FireManagerJson);

        // Act
        states.Capture("""{"m_SerializedFires":[],"m_DestroyedFires":[]}""");

        // Assert
        states.TryTake(CraftedFireGuid, out _).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("not json")]
    [DataRow("""{"m_SerializedFires":"unexpected"}""")]
    public void Capture_UnknownData_KeepsNothing(string? json)
    {
        // Arrange
        var states = new SavedFireStates();

        // Act
        int count = states.Capture(json);

        // Assert
        count.Should().Be(0);
    }
}
