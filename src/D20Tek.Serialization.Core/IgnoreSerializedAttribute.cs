namespace D20Tek.Serialization;

/// <summary>
/// Excludes the annotated property or field from serialization and deserialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class IgnoreSerializedAttribute : Attribute
{
}
