namespace D20Tek.Serialization;

/// <summary>
/// Defines a format-agnostic, forward-only reader over a structured document. Concrete
/// formats (such as CBOR) implement this interface to expose their encoding as a sequence
/// of logical read operations.
/// </summary>
/// <remarks>
/// The reader is positioned on a current value, classified by <see cref="ValueKind"/>. The
/// typed accessors (for example <see cref="GetString"/> or <see cref="GetInt64"/>) read the
/// current value and advance the reader past it. Structural calls must be balanced:
/// <see cref="ReadStartObject"/>/<see cref="ReadEndObject"/> and
/// <see cref="ReadStartArray"/>/<see cref="ReadEndArray"/>.
/// </remarks>
public interface IFormatReader
{
    /// <summary>
    /// Gets the <see cref="ValueKind"/> of the value the reader is currently positioned on.
    /// </summary>
    ValueKind ValueKind { get; }

    /// <summary>
    /// Reads the token that begins an object and positions the reader on its first member
    /// (or its end if the object is empty).
    /// </summary>
    void ReadStartObject();

    /// <summary>
    /// Reads the token that ends the current object, balancing the most recent
    /// <see cref="ReadStartObject"/>.
    /// </summary>
    void ReadEndObject();

    /// <summary>
    /// Reads the token that begins an array and positions the reader on its first element
    /// (or its end if the array is empty).
    /// </summary>
    void ReadStartArray();

    /// <summary>
    /// Reads the token that ends the current array, balancing the most recent
    /// <see cref="ReadStartArray"/>.
    /// </summary>
    void ReadEndArray();

    /// <summary>
    /// Attempts to read the name of the next object member. When a member is present, the
    /// reader is advanced to its value.
    /// </summary>
    /// <param name="name">
    /// When this method returns <see langword="true"/>, contains the property name; otherwise
    /// an empty string.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a property name was read; <see langword="false"/> when the
    /// end of the current object has been reached.
    /// </returns>
    bool TryReadPropertyName(out string name);

    /// <summary>
    /// Determines whether the current value is <see langword="null"/>, consuming it when it is.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the current value was <see langword="null"/> and has been
    /// consumed; otherwise <see langword="false"/>.
    /// </returns>
    bool IsNull();

    /// <summary>
    /// Reads the current value as a boolean and advances the reader.
    /// </summary>
    /// <returns>The boolean value.</returns>
    bool GetBoolean();

    /// <summary>
    /// Reads the current value as a 64-bit signed integer and advances the reader.
    /// </summary>
    /// <returns>The integer value.</returns>
    long GetInt64();

    /// <summary>
    /// Reads the current value as a double-precision floating-point number and advances the reader.
    /// </summary>
    /// <returns>The floating-point value.</returns>
    double GetDouble();

    /// <summary>
    /// Reads the current value as a text string and advances the reader.
    /// </summary>
    /// <returns>The string value.</returns>
    string GetString();

    /// <summary>
    /// Returns the raw UTF-8 bytes of the current string value without allocating a managed
    /// string, then advances the reader.
    /// </summary>
    /// <returns>A read-only span over the current string's UTF-8 encoded bytes.</returns>
    /// <remarks>
    /// The returned span is only valid while the reader's underlying source buffer remains
    /// alive and unmodified. Callers must copy the data if they need to retain it.
    /// </remarks>
    ReadOnlySpan<byte> GetRawStringBytes();

    /// <summary>
    /// Returns the raw encoded bytes of the current number value without allocating, then
    /// advances the reader.
    /// </summary>
    /// <returns>A read-only span over the current number's raw bytes.</returns>
    /// <remarks>
    /// The returned span is only valid while the reader's underlying source buffer remains
    /// alive and unmodified. Callers must copy the data if they need to retain it.
    /// </remarks>
    ReadOnlySpan<byte> GetRawNumberBytes();

    /// <summary>
    /// Skips the current value, including any nested object or array, and advances the reader
    /// past it. Used to discard unknown properties during deserialization.
    /// </summary>
    void SkipValue();
}
