namespace D20Tek.Serialization.Metadata;

/// <summary>
/// Describes a type and its ordered collection of serializable members, produced by the
/// reflection-based metadata builder and consumed by the reflection serialization fallback.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TypeMetadata"/> class.
/// </remarks>
/// <param name="type">The type the metadata describes.</param>
/// <param name="members">The ordered serializable members of the type.</param>
internal sealed class TypeMetadata(Type type, IReadOnlyList<MemberMetadata> members)
{
    /// <summary>
    /// Gets the type the metadata describes.
    /// </summary>
    public Type Type { get; } = type;

    /// <summary>
    /// Gets the ordered serializable members of the type.
    /// </summary>
    public IReadOnlyList<MemberMetadata> Members { get; } = members;
}
