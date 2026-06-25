namespace D20Tek.Serialization;

/// <summary>
/// Provides format-agnostic configuration that controls how values are serialized and
/// deserialized. Format-specific option types derive from this base.
/// </summary>
public abstract class SerializerOptions
{
    /// <summary>
    /// Gets or sets the naming policy applied to property names during serialization.
    /// A <see langword="null"/> value (the default) preserves member names as-is.
    /// </summary>
    public NamingPolicy? PropertyNamingPolicy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether properties with <see langword="null"/>
    /// values are omitted during serialization. The default is <see langword="false"/>.
    /// </summary>
    public bool IgnoreNullValues { get; set; }

    /// <summary>
    /// Gets the list of custom converters used to control serialization of specific types.
    /// Converters are resolved before generated and reflection-based serializers.
    /// </summary>
    public IList<Converter> Converters { get; } = [];
}
