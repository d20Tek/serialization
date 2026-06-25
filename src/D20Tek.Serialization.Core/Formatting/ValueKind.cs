namespace D20Tek.Serialization;

/// <summary>
/// Identifies the kind of value currently positioned by an <see cref="IFormatReader"/>
/// or written through an <see cref="IFormatWriter"/>. This is a format-agnostic
/// classification shared by all serializers.
/// </summary>
public enum ValueKind
{
    /// <summary>The value is a <see langword="null"/>.</summary>
    Null,

    /// <summary>The value is a boolean (<see langword="true"/> or <see langword="false"/>).</summary>
    Boolean,

    /// <summary>The value is a numeric value (integer or floating point).</summary>
    Number,

    /// <summary>The value is a text string.</summary>
    String,

    /// <summary>The value is an object: an ordered set of named properties.</summary>
    Object,

    /// <summary>The value is an array: an ordered sequence of values.</summary>
    Array,
}
