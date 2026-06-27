namespace D20Tek.Serialization.Dom;

/// <summary>
/// Identifies the kind of value represented by a <see cref="Node"/> in the shared,
/// format-agnostic document object model.
/// </summary>
public enum NodeKind
{
    /// <summary>The node is a <see langword="null"/> value.</summary>
    Null,

    /// <summary>The node is a boolean (<see langword="true"/> or <see langword="false"/>).</summary>
    Boolean,

    /// <summary>The node is a numeric value.</summary>
    Number,

    /// <summary>The node is a text string.</summary>
    String,

    /// <summary>The node is an object: an ordered set of named properties.</summary>
    Object,

    /// <summary>The node is an array: an ordered sequence of nodes.</summary>
    Array,
}
