namespace D20Tek.Serialization.Metadata;

/// <summary>
/// Describes a single serializable member (property or field) discovered through reflection,
/// including its serialized name, metadata flags, and compiled accessor delegates used by the
/// reflection-based serialization fallback.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MemberMetadata"/> class.
/// </remarks>
/// <param name="name">The .NET member name.</param>
/// <param name="serializedName">The name used when serializing the member.</param>
/// <param name="memberType">The declared type of the member.</param>
/// <param name="isRequired">Whether the member is required during deserialization.</param>
/// <param name="ignoreNull">Whether the member is omitted when its value is <see langword="null"/>.</param>
/// <param name="getter">A compiled delegate that reads the member value from an instance.</param>
/// <param name="setter">
/// A compiled delegate that writes the member value to an instance, or <see langword="null"/>
/// when the member has no accessible setter and is instead supplied through constructor-based
/// construction (see <see cref="ConstructorParameterIndex"/>).
/// </param>
/// <param name="aliases">
/// Legacy serialized names also accepted when deserializing the member, in addition to
/// <paramref name="serializedName"/>. Empty when the member has no configured aliases.
/// </param>
/// <param name="constructorParameterIndex">
/// The zero-based position of the matching parameter on the type's selected constructor, when
/// the member must be supplied through constructor-based construction rather than a setter.
/// <see langword="null"/> when the member is assigned through <paramref name="setter"/> instead.
/// </param>
internal sealed class MemberMetadata(
    string name,
    string serializedName,
    Type memberType,
    bool isRequired,
    bool ignoreNull,
    Func<object, object?> getter,
    Action<object, object?>? setter,
    IReadOnlyList<string>? aliases = null,
    int? constructorParameterIndex = null)
{
    /// <summary>
    /// Gets the .NET member name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the name used when serializing the member, after applying any
    /// <see cref="NameSerializedAttribute"/> or configured <see cref="NamingPolicy"/>.
    /// </summary>
    public string SerializedName { get; } = serializedName;

    /// <summary>
    /// Gets the legacy serialized names also accepted when deserializing the member, in
    /// addition to <see cref="SerializedName"/>. Empty when the member has no configured
    /// aliases.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; } = aliases ?? [];

    /// <summary>
    /// Gets the declared type of the member.
    /// </summary>
    public Type MemberType { get; } = memberType;

    /// <summary>
    /// Gets a value indicating whether the member is required during deserialization.
    /// </summary>
    public bool IsRequired { get; } = isRequired;

    /// <summary>
    /// Gets a value indicating whether the member is omitted during serialization when its
    /// value is <see langword="null"/>.
    /// </summary>
    public bool IgnoreNull { get; } = ignoreNull;

    /// <summary>
    /// Gets a compiled delegate that reads the member value from an instance.
    /// </summary>
    public Func<object, object?> Getter { get; } = getter;

    /// <summary>
    /// Gets a compiled delegate that writes the member value to an instance, or
    /// <see langword="null"/> when the member is supplied through constructor-based
    /// construction instead.
    /// </summary>
    public Action<object, object?>? Setter { get; } = setter;

    /// <summary>
    /// Gets the zero-based position of the matching parameter on the type's selected
    /// constructor, when the member must be supplied through constructor-based construction.
    /// <see langword="null"/> when the member is assigned through <see cref="Setter"/> instead.
    /// </summary>
    public int? ConstructorParameterIndex { get; } = constructorParameterIndex;
}
