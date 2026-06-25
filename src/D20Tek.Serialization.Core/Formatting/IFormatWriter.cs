namespace D20Tek.Serialization;

/// <summary>
/// Defines a format-agnostic, forward-only writer that emits structured values. Concrete
/// formats (such as CBOR) implement this interface to translate the logical write calls
/// into their own encoding.
/// </summary>
/// <remarks>
/// Calls must form a well-formed document: every <see cref="WriteStartObject"/> is balanced
/// by a <see cref="WriteEndObject"/>, every <see cref="WriteStartArray"/> by a
/// <see cref="WriteEndArray"/>, and each object member is written as a
/// <see cref="WritePropertyName(string)"/> call immediately followed by the call that writes
/// its value.
/// </remarks>
public interface IFormatWriter
{
    /// <summary>
    /// Writes the token that begins an object. Subsequent writes represent its members until
    /// the matching <see cref="WriteEndObject"/> is written.
    /// </summary>
    void WriteStartObject();

    /// <summary>
    /// Writes the token that ends the current object, balancing the most recent
    /// <see cref="WriteStartObject"/>.
    /// </summary>
    void WriteEndObject();

    /// <summary>
    /// Writes the token that begins an array. Subsequent writes represent its elements until
    /// the matching <see cref="WriteEndArray"/> is written.
    /// </summary>
    void WriteStartArray();

    /// <summary>
    /// Writes the token that ends the current array, balancing the most recent
    /// <see cref="WriteStartArray"/>.
    /// </summary>
    void WriteEndArray();

    /// <summary>
    /// Writes the name of the next object member. Must be followed immediately by a call that
    /// writes the member's value.
    /// </summary>
    /// <param name="name">The property name to write.</param>
    void WritePropertyName(string name);

    /// <summary>
    /// Writes a <see langword="null"/> value.
    /// </summary>
    void WriteNull();

    /// <summary>
    /// Writes a boolean value.
    /// </summary>
    /// <param name="value">The boolean value to write.</param>
    void WriteBoolean(bool value);

    /// <summary>
    /// Writes a 64-bit signed integer value.
    /// </summary>
    /// <param name="value">The integer value to write.</param>
    void WriteNumber(long value);

    /// <summary>
    /// Writes a double-precision floating-point value.
    /// </summary>
    /// <param name="value">The floating-point value to write.</param>
    void WriteNumber(double value);

    /// <summary>
    /// Writes a text string value.
    /// </summary>
    /// <param name="value">The string value to write.</param>
    void WriteString(string value);
}
