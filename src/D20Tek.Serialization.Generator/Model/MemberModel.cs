namespace D20Tek.Serialization.Generation;

/// <summary>
/// A value-equatable description of a single serialized member, mirroring the runtime
/// <c>MemberMetadata</c> shape but built from compile-time symbols.
/// </summary>
/// <param name="MemberName">The .NET property name.</param>
/// <param name="SerializedName">
/// The resolved serialized name when fixed by <c>[SerializedName]</c>; otherwise <see langword="null"/>
/// to indicate the runtime naming policy should be applied to <paramref name="MemberName"/>.
/// </param>
/// <param name="FullyQualifiedTypeName">The fully qualified member type (for casts in generated code).</param>
/// <param name="Strategy">How the member is read/written.</param>
/// <param name="IsNullable">Whether the member may hold <see langword="null"/> (reference type or <see cref="System.Nullable{T}"/>).</param>
/// <param name="IsRequired">Whether the member is annotated with <c>[RequiredSerialized]</c>.</param>
internal sealed record MemberModel(
    string MemberName,
    string? SerializedName,
    string FullyQualifiedTypeName,
    MemberStrategy Strategy,
    bool IsNullable,
    bool IsRequired) : IEquatable<MemberModel>;
