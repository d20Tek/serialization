namespace D20Tek.Serialization;

/// <summary>
/// Marks the annotated property or field as required. Deserialization fails with a
/// <c>SerializationException</c> when a required member is missing from the input.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class RequiredSerializedAttribute : Attribute
{
}
