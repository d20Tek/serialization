namespace D20Tek.Serialization.Generation;

/// <summary>
/// Emitted by the source generator on the assembly to advertise the generated
/// <see cref="IGeneratedSerializerRegistry"/> implementation, allowing the registry to be
/// discovered at runtime via reflection over assembly attributes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GeneratedSerializerRegistryAttribute"/> class.
/// </remarks>
/// <param name="registryType">The generated registry type for the assembly.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class GeneratedSerializerRegistryAttribute(Type registryType) : Attribute
{
    /// <summary>
    /// Gets the generated <see cref="IGeneratedSerializerRegistry"/> implementation type for
    /// the assembly.
    /// </summary>
    public Type RegistryType { get; } = registryType;
}
