namespace D20Tek.Serialization;

/// <summary>
/// Controls how the binary deserializer responds to unknown tags and unknown properties
/// encountered in the input stream.
/// </summary>
public enum BinaryDecodingMode
{
    /// <summary>
    /// Unknown tags and unknown properties cause a <see cref="SerializationException"/> to be
    /// thrown, rejecting any input that does not exactly match the expected shape.
    /// </summary>
    Strict,

    /// <summary>
    /// Unknown tags and unknown properties are skipped, enabling forward-compatible decoding of
    /// inputs produced by newer schema versions.
    /// </summary>
    Lenient,
}
