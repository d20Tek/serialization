namespace D20Tek.Serialization.Generation;

/// <summary>
/// Defines the contract for a serializer produced by the source generator for a single
/// <c>[Serializable]</c> type. Generated implementations translate values of type
/// <typeparamref name="T"/> to and from a format-agnostic
/// <see cref="IFormatWriter"/>/<see cref="IFormatReader"/>.
/// </summary>
/// <typeparam name="T">The type handled by this serializer.</typeparam>
public interface IGeneratedSerializer<T>
{
    /// <summary>
    /// Writes the specified value to the supplied writer.
    /// </summary>
    /// <param name="writer">The format writer that receives the serialized value.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">The serializer options in effect.</param>
    void Write(IFormatWriter writer, T value, SerializerOptions options);

    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from the supplied reader.
    /// </summary>
    /// <param name="reader">The format reader positioned on the value to read.</param>
    /// <param name="options">The serializer options in effect.</param>
    /// <returns>The deserialized value, or <see langword="null"/> for a null reference.</returns>
    T? Read(IFormatReader reader, SerializerOptions options);
}
