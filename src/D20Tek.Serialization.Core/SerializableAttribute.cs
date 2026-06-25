namespace D20Tek.Serialization;

/// <summary>
/// Marks a class or struct as serializable, instructing the source generator to emit a
/// strongly-typed serializer and to register it in the assembly's generated serializer registry.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class SerializableAttribute : Attribute
{
}
