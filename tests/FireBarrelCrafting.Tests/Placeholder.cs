using System.Runtime.CompilerServices;

namespace FireBarrelCrafting.Tests;

/// <summary>Creates dependency instances for tests that only need a non-null reference.</summary>
/// <remarks>
/// Most mod types depend on objects that only the running game / MelonLoader can construct.
/// For constructor guard tests a non-null reference is all that matters, so the constructor is skipped.
/// </remarks>
internal static class Placeholder
{
    /// <summary>Creates an instance of <typeparamref name="T"/> without running any constructor.</summary>
    /// <typeparam name="T">Type to create.</typeparam>
    /// <returns>A zero-initialised instance; do not call members that use its state.</returns>
    public static T Of<T>()
        where T : class => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
}
