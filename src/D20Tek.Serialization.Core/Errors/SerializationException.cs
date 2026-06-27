namespace D20Tek.Serialization;

/// <summary>
/// The exception thrown when serialization or deserialization fails. Carries the JSONPath
/// location of the error and, when applicable, the expected and actual value kinds.
/// </summary>
public sealed class SerializationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class.
    /// </summary>
    /// <param name="message">The error message describing the failure.</param>
    /// <param name="path">The JSONPath location where the error occurred.</param>
    /// <param name="expected">The value kind that was expected, if applicable.</param>
    /// <param name="actual">The value kind that was actually encountered, if applicable.</param>
    public SerializationException(
        string message,
        string path,
        ValueKind? expected = null,
        ValueKind? actual = null)
        : base(message)
    {
        Path = path;
        Expected = expected;
        Actual = actual;
    }

    /// <summary>
    /// Gets the JSONPath location where the error occurred (for example, <c>$.items[3].price</c>).
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the value kind that was expected at <see cref="Path"/>, or <see langword="null"/>
    /// when not applicable.
    /// </summary>
    public ValueKind? Expected { get; }

    /// <summary>
    /// Gets the value kind that was actually encountered at <see cref="Path"/>, or
    /// <see langword="null"/> when not applicable.
    /// </summary>
    public ValueKind? Actual { get; }
}
