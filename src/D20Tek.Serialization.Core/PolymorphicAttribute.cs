namespace D20Tek.Serialization;

/// <summary>
/// Marks a base class or interface as the root of a polymorphic serialization hierarchy. When a
/// member is declared with this type, the serializer writes/reads an additional type
/// discriminator property so the concrete runtime type can be recovered on deserialization.
/// </summary>
/// <remarks>
/// Use <see cref="SerializedDerivedTypeAttribute"/> on the same type to register each concrete
/// derived type and the discriminator value used to identify it on the wire.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public sealed class PolymorphicAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the property name used for the type discriminator. The default is
    /// <c>"$type"</c>.
    /// </summary>
    public string TypeDiscriminatorPropertyName { get; set; } = "$type";
}
