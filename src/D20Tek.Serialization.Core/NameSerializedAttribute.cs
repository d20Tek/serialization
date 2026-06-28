namespace D20Tek.Serialization;

/// <summary>
/// Overrides the serialized name of a property or field, taking precedence over any
/// configured <see cref="NamingPolicy"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NameSerializedAttribute"/> class.
/// </remarks>
/// <param name="name">The name to use when serializing the annotated member.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NameSerializedAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the name to use when serializing the annotated member.
    /// </summary>
    public string Name { get; } = name;
}
