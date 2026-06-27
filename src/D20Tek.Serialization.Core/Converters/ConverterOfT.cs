namespace D20Tek.Serialization;

/// <summary>
/// Base type for format-agnostic converters that customize how values of type
/// <typeparamref name="T"/> are serialized and deserialized.
/// </summary>
/// <typeparam name="T">The type handled by this converter.</typeparam>
public abstract class Converter<T> : Converter
{
    /// <summary>
    /// Determines whether this converter can handle the specified type. The default
    /// implementation returns <see langword="true"/> only when <paramref name="typeToConvert"/>
    /// is exactly <typeparamref name="T"/>.
    /// </summary>
    /// <param name="typeToConvert">The type to check for conversion support.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="typeToConvert"/> is <typeparamref name="T"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(T);

    /// <summary>
    /// Reads and converts the current value from the specified reader.
    /// </summary>
    /// <param name="reader">The format reader positioned on the value to read.</param>
    /// <param name="options">The serializer options in effect.</param>
    /// <returns>The deserialized value.</returns>
    public abstract T? Read(IFormatReader reader, SerializerOptions options);

    /// <summary>
    /// Converts and writes the specified value to the given writer.
    /// </summary>
    /// <param name="writer">The format writer to write the value to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options in effect.</param>
    public abstract void Write(IFormatWriter writer, T value, SerializerOptions options);
}
