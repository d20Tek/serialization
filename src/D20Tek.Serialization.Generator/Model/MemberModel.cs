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
/// <param name="Aliases">
/// Legacy serialized names also accepted when deserializing the member, from
/// <c>[NameSerialized]</c>'s <c>aliases</c> parameter. Empty when none are configured.
/// </param>
/// <param name="IsInitOnly">
/// Whether the member's setter is an <c>init</c> accessor, requiring construction through an
/// object initializer rather than a post-construction assignment.
/// </param>
/// <param name="ConstructorParameterName">
/// The name of the matching parameter on the type's selected constructor (preserving its
/// declared casing), when the member must be supplied through constructor-based construction
/// rather than a settable property. <see langword="null"/> when the member is assigned through
/// a setter or object initializer instead.
/// </param>
internal sealed record MemberModel(
    string MemberName,
    string? SerializedName,
    string FullyQualifiedTypeName,
    MemberStrategy Strategy,
    bool IsNullable,
    bool IsRequired,
    EquatableArray<string> Aliases,
    bool IsInitOnly = false,
    string? ConstructorParameterName = null) : IEquatable<MemberModel>;
