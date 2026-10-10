namespace D20Tek.Serialization;

/// <summary>
/// Registers a concrete derived type for a polymorphic base type or interface annotated with
/// <see cref="PolymorphicAttribute"/>, associating it with a discriminator value written to and
/// read from the type discriminator property.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SerializedDerivedTypeAttribute"/> class.
/// </remarks>
/// <param name="derivedType">The concrete derived type.</param>
/// <param name="typeDiscriminator">
/// The discriminator value written to the wire to identify <paramref name="derivedType"/>.
/// </param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class SerializedDerivedTypeAttribute(Type derivedType, string typeDiscriminator) : Attribute
{
    /// <summary>
    /// Gets the concrete derived type registered for polymorphic serialization.
    /// </summary>
    public Type DerivedType { get; } = derivedType;

    /// <summary>
    /// Gets the discriminator value used to identify <see cref="DerivedType"/> on the wire.
    /// </summary>
    public string TypeDiscriminator { get; } = typeDiscriminator;
}
