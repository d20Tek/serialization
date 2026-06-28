namespace D20Tek.Serialization.Dom;

/// <summary>
/// Represents a single named property of an object <see cref="Node"/>: a name paired with its
/// value node.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NodeProperty"/> struct.
/// </remarks>
/// <param name="name">The property name.</param>
/// <param name="value">The property value.</param>
/// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
public readonly struct NodeProperty(string name, Node value)
{
    /// <summary>
    /// Gets the property name.
    /// </summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <summary>
    /// Gets the property value.
    /// </summary>
    public Node Value { get; } = value;
}
