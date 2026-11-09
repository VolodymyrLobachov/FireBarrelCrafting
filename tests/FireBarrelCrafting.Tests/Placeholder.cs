using System.Runtime.CompilerServices;

namespace FireBarrelCrafting.Tests;

/// <summary>Creates instances of mod types without running their constructors.</summary>
/// <remarks>
/// Most mod types depend on objects that only the running game / MelonLoader can construct.
/// Tests that only exercise plain fields and properties can skip the constructor entirely.
/// </remarks>
internal static class Placeholder
{
    /// <summary>Creates an instance of <typeparamref name="T"/> without running any constructor.</summary>
    /// <typeparam name="T">Type to create.</typeparam>
    /// <returns>A zero-initialised instance; do not call members that use its state.</returns>
    public static T Of<T>()
        where T : class => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
}
