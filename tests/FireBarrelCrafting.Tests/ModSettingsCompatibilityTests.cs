using System.Reflection;
using FireBarrelCrafting.Configuration;
using ModSettings;

namespace FireBarrelCrafting.Tests;

/// <summary>
/// Runs the installed ModSettings' own field validation against <see cref="FireBarrelSettings"/>, so an unsupported
/// field (e.g. a <see cref="string"/>, which ModSettings 2.2.5 rejects) fails here instead of at game start.
/// </summary>
[TestClass]
public sealed class ModSettingsCompatibilityTests
{
    private static IEnumerable<object[]> SettingFields =>
        typeof(FireBarrelSettings)
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Select(settingField => new object[] { settingField.Name });

    [TestMethod]
    [DynamicData(nameof(SettingFields))]
    public void Field_PassesModSettingsValidation(string fieldName)
    {
        // Arrange
        FireBarrelSettings settings = Placeholder.Of<FireBarrelSettings>();
        FieldInfo field = typeof(FireBarrelSettings).GetField(fieldName)!;
        MethodInfo validate = typeof(ModSettingsBase).Assembly
            .GetType("ModSettings.Attributes", throwOnError: true)!
            .GetMethod("ValidateFieldAttributes", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ModSettings.Attributes.ValidateFieldAttributes not found; was ModSettings updated?");

        // Act
        Action act = () => validate.Invoke(null, [settings, field]);

        // Assert
        act.Should().NotThrow();
    }
}
