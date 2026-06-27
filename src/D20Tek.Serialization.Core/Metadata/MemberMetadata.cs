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
/// <param name="setter">A compiled delegate that writes the member value to an instance.</param>
internal sealed class MemberMetadata(
    string name,
    string serializedName,
    Type memberType,
    bool isRequired,
    bool ignoreNull,
    Func<object, object?> getter,
    Action<object, object?> setter)
{

    /// <summary>
    /// Gets the .NET member name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the name used when serializing the member, after applying any
    /// <see cref="SerializedNameAttribute"/> or configured <see cref="NamingPolicy"/>.
    /// </summary>
    public string SerializedName { get; } = serializedName;

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
    /// Gets a compiled delegate that writes the member value to an instance.
    /// </summary>
    public Action<object, object?> Setter { get; } = setter;
}
