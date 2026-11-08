// Il2CppInterop-generated UnityEngine.CoreModule.dll exposes a PUBLIC, constructor-less
// System.Runtime.CompilerServices.NullableAttribute. The compiler would bind to it and fail with CS0656,
// so we define the attributes it emits for nullable reference types here; source types take precedence.
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.All, Inherited = false)]
internal sealed class NullableAttribute : Attribute
{
    public NullableAttribute(byte flag) => NullableFlags = [flag];

    public NullableAttribute(byte[] flags) => NullableFlags = flags;

    public byte[] NullableFlags { get; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Struct, Inherited = false)]
internal sealed class NullableContextAttribute(byte flag) : Attribute
{
    public byte Flag { get; } = flag;
}
