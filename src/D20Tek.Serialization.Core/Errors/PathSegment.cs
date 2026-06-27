using System.Text;

namespace D20Tek.Serialization;

/// <summary>
/// Represents a single segment of an object tree path: either an object property name (rendered as
/// <c>.name</c>) or an array index (rendered as <c>[index]</c>).
/// </summary>
internal readonly struct PathSegment
{
    private readonly string? _propertyName;
    private readonly int _index;

    private PathSegment(string? propertyName, int index)
    {
        _propertyName = propertyName;
        _index = index;
    }

    /// <summary>
    /// Gets a value indicating whether this segment represents an array index.
    /// </summary>
    public bool IsArrayIndex => _propertyName is null;

    /// <summary>
    /// Creates a property-name segment.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>A segment that renders as <c>.name</c>.</returns>
    public static PathSegment Property(string name) => new(name, -1);

    /// <summary>
    /// Creates an array-index segment.
    /// </summary>
    /// <param name="index">The array index.</param>
    /// <returns>A segment that renders as <c>[index]</c>.</returns>
    public static PathSegment ArrayIndex(int index) => new(null, index);

    /// <summary>
    /// Appends the rendered form of this segment to the specified <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="builder">The builder to append to.</param>
    public void AppendTo(StringBuilder builder)
    {
        if (_propertyName is null)
        {
            builder.Append('[').Append(_index).Append(']');
        }
        else
        {
            builder.Append('.').Append(_propertyName);
        }
    }
}
