using D20Tek.Serialization.Dom;

namespace D20Tek.Serialization;

/// <summary>
/// Represents a single value in the binary DOM, providing typed accessors for scalars and
/// navigation for objects and arrays. This is a lightweight, read-only view over the internal
/// <see cref="Node"/> tree produced by parsing CBOR-encoded data.
/// </summary>
/// <remarks>
/// Typed getters throw <see cref="InvalidOperationException"/> when the element is not of the
/// requested kind. Indexers throw <see cref="KeyNotFoundException"/> (for objects) or
/// <see cref="ArgumentOutOfRangeException"/> (for arrays) when the requested property or index
/// does not exist.
/// </remarks>
public readonly struct BinaryElement
{
    private readonly Node _node;

    internal BinaryElement(Node node) => _node = node;

    /// <summary>
    /// Gets the kind of value this element represents.
    /// </summary>
    public NodeKind ValueKind => _node.Kind;

    /// <summary>
    /// Gets the value of this element as a <see cref="string"/>.
    /// </summary>
    /// <returns>The string value.</returns>
    /// <exception cref="InvalidOperationException">
    /// The element is not a <see cref="NodeKind.String"/>.
    /// </exception>
    public string GetString() => _node.GetString();

    /// <summary>
    /// Gets the value of this element as an <see cref="int"/>.
    /// </summary>
    /// <returns>The 32-bit integer value.</returns>
    /// <exception cref="InvalidOperationException">
    /// The element is not a <see cref="NodeKind.Number"/>.
    /// </exception>
    public int GetInt32() => (int)_node.GetDouble();

    /// <summary>
    /// Gets the value of this element as a <see cref="long"/>.
    /// </summary>
    /// <returns>The 64-bit integer value.</returns>
    /// <exception cref="InvalidOperationException">
    /// The element is not a <see cref="NodeKind.Number"/>.
    /// </exception>
    public long GetInt64() => (long)_node.GetDouble();

    /// <summary>
    /// Gets the value of this element as a <see cref="double"/>.
    /// </summary>
    /// <returns>The double-precision floating-point value.</returns>
    /// <exception cref="InvalidOperationException">
    /// The element is not a <see cref="NodeKind.Number"/>.
    /// </exception>
    public double GetDouble() => _node.GetDouble();

    /// <summary>
    /// Gets the value of this element as a <see cref="bool"/>.
    /// </summary>
    /// <returns>The boolean value.</returns>
    /// <exception cref="InvalidOperationException">
    /// The element is not a <see cref="NodeKind.Boolean"/>.
    /// </exception>
    public bool GetBoolean() => _node.GetBoolean();

    /// <summary>
    /// Gets the value of the named property on this object element.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The value element for the named property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// The element is not a <see cref="NodeKind.Object"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// No property with the specified name exists.
    /// </exception>
    public BinaryElement this[string propertyName] => new(_node[propertyName]);

    /// <summary>
    /// Gets the element at the specified index of this array element.
    /// </summary>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="InvalidOperationException">
    /// The element is not a <see cref="NodeKind.Array"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative or not less than the number of elements.
    /// </exception>
    public BinaryElement this[int index] => new(_node[index]);

    /// <summary>
    /// Enumerates the properties of this object element.
    /// </summary>
    /// <returns>An enumerable of <see cref="BinaryProperty"/> values.</returns>
    /// <exception cref="InvalidOperationException">
    /// The element is not a <see cref="NodeKind.Object"/>.
    /// </exception>
    public IEnumerable<BinaryProperty> EnumerateObject()
    {
        var properties = _node.GetObject();
        for (var i = 0; i < properties.Count; i++)
        {
            var p = properties[i];
            yield return new BinaryProperty(p.Name, new BinaryElement(p.Value));
        }
    }

    /// <summary>
    /// Enumerates the elements of this array element.
    /// </summary>
    /// <returns>An enumerable of <see cref="BinaryElement"/> values.</returns>
    /// <exception cref="InvalidOperationException">
    /// The element is not a <see cref="NodeKind.Array"/>.
    /// </exception>
    public IEnumerable<BinaryElement> EnumerateArray()
    {
        var items = _node.GetArray();
        for (var i = 0; i < items.Count; i++)
        {
            yield return new BinaryElement(items[i]);
        }
    }
}
