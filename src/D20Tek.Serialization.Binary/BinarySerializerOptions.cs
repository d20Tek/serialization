namespace D20Tek.Serialization;

/// <summary>
/// Provides configuration that controls how values are serialized to and deserialized from the
/// binary (CBOR) format. Extends the format-agnostic <see cref="SerializerOptions"/> with
/// binary-specific settings.
/// </summary>
public sealed class BinarySerializerOptions : SerializerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether public fields (in addition to properties) are
    /// included during serialization. The default is <see langword="false"/>.
    /// </summary>
    public bool IncludeFields { get; set; }

    /// <summary>
    /// Gets or sets the mode that controls how unknown tags and unknown properties are handled
    /// during deserialization. The default is <see cref="BinaryDecodingMode.Lenient"/>.
    /// </summary>
    public BinaryDecodingMode DecodingMode { get; set; } = BinaryDecodingMode.Lenient;

    /// <summary>
    /// Gets or sets an optional <see cref="BinaryProfile"/> whose
    /// <see cref="BinaryProfile.Configure(BinarySerializerOptions)"/> method is applied to this
    /// instance during options resolution. A <see langword="null"/> value (the default) applies
    /// no profile.
    /// </summary>
    public BinaryProfile? Profile { get; set; }
}
