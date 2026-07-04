namespace D20Tek.Serialization;

/// <summary>
/// Represents a single named property of a <see cref="BinaryElement"/> whose
/// <see cref="BinaryElement.ValueKind"/> is <see cref="Dom.NodeKind.Object"/>:
/// a name paired with a value element.
/// </summary>
public readonly struct BinaryProperty
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryProperty"/> struct.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value element.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    internal BinaryProperty(string name, BinaryElement value)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(name);
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Gets the property name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the property value element.
    /// </summary>
    public BinaryElement Value { get; }
}
