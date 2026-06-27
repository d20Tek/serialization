using System.Text;

namespace D20Tek.Serialization.Errors;

/// <summary>
/// Maintains the stack of object property names and array indices visited while reading,
/// and renders the current location as a JSONPath-style path (for example,
/// <c>$.items[3].price</c>). Intended to be reused by format readers, regardless of the
/// underlying format, to produce accurate error locations.
/// </summary>
internal sealed class SerializationPathBuilder
{
    private const string _pathPrefix = "$";
    private readonly List<PathSegment> _segments = [];

    /// <summary>
    /// Gets the number of segments currently on the stack.
    /// </summary>
    public int Depth => _segments.Count;

    /// <summary>
    /// Pushes a property-name segment onto the stack.
    /// </summary>
    /// <param name="name">The property name being entered.</param>
    public void PushProperty(string name) => _segments.Add(PathSegment.Property(name));

    /// <summary>
    /// Pushes an array-index segment onto the stack.
    /// </summary>
    /// <param name="index">The array index being entered.</param>
    public void PushIndex(int index) => _segments.Add(PathSegment.ArrayIndex(index));

    /// <summary>
    /// Replaces the array-index segment at the top of the stack with a new index. Typically
    /// called as a reader advances through the elements of an array.
    /// </summary>
    /// <param name="index">The new array index.</param>
    public void SetIndex(int index)
    {
        if (_segments.Count > 0 && _segments[_segments.Count - 1].IsArrayIndex)
        {
            _segments[^1] = PathSegment.ArrayIndex(index);
        }
        else
        {
            PushIndex(index);
        }
    }

    /// <summary>
    /// Removes the top segment from the stack.
    /// </summary>
    public void Pop()
    {
        if (_segments.Count > 0) _segments.RemoveAt(_segments.Count - 1);
    }

    /// <summary>
    /// Renders the current location as a JSONPath-style path rooted at <c>$</c>.
    /// </summary>
    /// <returns>The path string for the current stack state.</returns>
    public string ToPath()
    {
        var builder = new StringBuilder(_pathPrefix);
        foreach (var segment in _segments)
        {
            segment.AppendTo(builder);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds a JSONPath-style path from the supplied segments, rooted at <c>$</c>.
    /// </summary>
    /// <param name="segments">The ordered segments describing the location.</param>
    /// <returns>The rendered path string.</returns>
    public static string BuildPath(IEnumerable<PathSegment> segments)
    {
        var builder = new StringBuilder(_pathPrefix);
        foreach (var segment in segments)
        {
            segment.AppendTo(builder);
        }

        return builder.ToString();
    }

    /// <inheritdoc />
    public override string ToString() => ToPath();
}
