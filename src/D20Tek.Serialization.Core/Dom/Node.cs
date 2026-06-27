namespace D20Tek.Serialization.Dom;

/// <summary>
/// Represents a single value in the shared, format-agnostic document object model (DOM). A node
/// is an immutable value that is one of the <see cref="NodeKind"/> kinds: null, boolean, number,
/// string, object, or array. Object and array nodes compose child nodes to form a tree.
/// </summary>
/// <remarks>
/// The default value of <see cref="Node"/> is a <see cref="NodeKind.Null"/> node. Typed accessors
/// throw <see cref="InvalidOperationException"/> when the node is not of the requested kind.
/// </remarks>
public readonly struct Node
{
    private readonly NodeKind _kind;
    private readonly bool _boolean;
    private readonly double _number;
    private readonly object? _reference;

    private Node(NodeKind kind, bool boolean, double number, object? reference)
    {
        _kind = kind;
        _boolean = boolean;
        _number = number;
        _reference = reference;
    }

    /// <summary>
    /// Gets the kind of value this node represents.
    /// </summary>
    public NodeKind Kind => _kind;

    /// <summary>
    /// Gets a node representing a <see langword="null"/> value.
    /// </summary>
    internal static Node Null { get; } = new(NodeKind.Null, false, 0d, null);

    /// <summary>
    /// Creates a boolean node.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>A node of kind <see cref="NodeKind.Boolean"/>.</returns>
    internal static Node CreateBoolean(bool value) => new(NodeKind.Boolean, value, 0d, null);

    /// <summary>
    /// Creates a number node.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <returns>A node of kind <see cref="NodeKind.Number"/>.</returns>
    internal static Node CreateNumber(double value) => new(NodeKind.Number, false, value, null);

    /// <summary>
    /// Creates a string node.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A node of kind <see cref="NodeKind.String"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    internal static Node CreateString(string value) =>
        new(NodeKind.String, false, 0d, value ?? throw new ArgumentNullException(nameof(value)));

    /// <summary>
    /// Creates an array node from the supplied child nodes.
    /// </summary>
    /// <param name="items">The ordered elements of the array.</param>
    /// <returns>A node of kind <see cref="NodeKind.Array"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
    internal static Node CreateArray(IReadOnlyList<Node> items) =>
        new(NodeKind.Array, false, 0d, items ?? throw new ArgumentNullException(nameof(items)));

    /// <summary>
    /// Creates an object node from the supplied properties.
    /// </summary>
    /// <param name="properties">The ordered properties of the object.</param>
    /// <returns>A node of kind <see cref="NodeKind.Object"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="properties"/> is <see langword="null"/>.</exception>
    internal static Node CreateObject(IReadOnlyList<NodeProperty> properties) =>
        new(NodeKind.Object, false, 0d, properties ?? throw new ArgumentNullException(nameof(properties)));

    /// <summary>
    /// Gets the value of this node as a string.
    /// </summary>
    /// <returns>The string value.</returns>
    /// <exception cref="InvalidOperationException">The node is not a <see cref="NodeKind.String"/>.</exception>
    public string GetString()
    {
        EnsureKind(NodeKind.String);
        return (string)_reference!;
    }

    /// <summary>
    /// Gets the value of this node as a double-precision number.
    /// </summary>
    /// <returns>The numeric value.</returns>
    /// <exception cref="InvalidOperationException">The node is not a <see cref="NodeKind.Number"/>.</exception>
    public double GetDouble()
    {
        EnsureKind(NodeKind.Number);
        return _number;
    }

    /// <summary>
    /// Gets the value of this node as a boolean.
    /// </summary>
    /// <returns>The boolean value.</returns>
    /// <exception cref="InvalidOperationException">The node is not a <see cref="NodeKind.Boolean"/>.</exception>
    public bool GetBoolean()
    {
        EnsureKind(NodeKind.Boolean);
        return _boolean;
    }

    /// <summary>
    /// Gets the child nodes of this array.
    /// </summary>
    /// <returns>The ordered elements of the array.</returns>
    /// <exception cref="InvalidOperationException">The node is not a <see cref="NodeKind.Array"/>.</exception>
    public IReadOnlyList<Node> GetArray()
    {
        EnsureKind(NodeKind.Array);
        return (IReadOnlyList<Node>)_reference!;
    }

    /// <summary>
    /// Gets the properties of this object.
    /// </summary>
    /// <returns>The ordered properties of the object.</returns>
    /// <exception cref="InvalidOperationException">The node is not a <see cref="NodeKind.Object"/>.</exception>
    public IReadOnlyList<NodeProperty> GetObject()
    {
        EnsureKind(NodeKind.Object);
        return (IReadOnlyList<NodeProperty>)_reference!;
    }

    /// <summary>
    /// Gets the value of the named property on this object node.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The value node for the named property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The node is not a <see cref="NodeKind.Object"/>.</exception>
    /// <exception cref="KeyNotFoundException">No property with the specified name exists.</exception>
    public Node this[string propertyName]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(propertyName);

            var properties = GetObject();
            for (var i = 0; i < properties.Count; i++)
            {
                if (properties[i].Name == propertyName)
                {
                    return properties[i].Value;
                }
            }

            throw new KeyNotFoundException(
                $"The object node does not contain a property named '{propertyName}'.");
        }
    }

    /// <summary>
    /// Gets the element at the specified index of this array node.
    /// </summary>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>The element node at the specified index.</returns>
    /// <exception cref="InvalidOperationException">The node is not a <see cref="NodeKind.Array"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative or not less than the number of elements.
    /// </exception>
    public Node this[int index]
    {
        get
        {
            var items = GetArray();
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, items.Count);

            return items[index];
        }
    }

    private void EnsureKind(NodeKind expected)
    {
        if (_kind != expected)
        {
            throw new InvalidOperationException(
                $"The node kind '{_kind}' does not support this operation; expected '{expected}'.");
        }
    }
}
