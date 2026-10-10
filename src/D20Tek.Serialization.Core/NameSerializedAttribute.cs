namespace D20Tek.Serialization;

/// <summary>
/// Overrides the serialized name of a property or field, taking precedence over any
/// configured <see cref="NamingPolicy"/>. Optionally registers one or more legacy aliases that
/// are still accepted when reading, to support renaming members without breaking payloads
/// written under the old name.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NameSerializedAttribute"/> class.
/// </remarks>
/// <param name="name">The name to use when serializing the annotated member.</param>
/// <param name="aliases">
/// Optional legacy names that are also accepted when deserializing the annotated member, in
/// addition to <paramref name="name"/>. Aliases are read-only: they are never written during
/// serialization. Use this to rename a member while remaining able to read payloads produced
/// before the rename.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NameSerializedAttribute(string name, params string[] aliases) : Attribute
{
    /// <summary>
    /// Gets the name to use when serializing the annotated member.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the legacy names also accepted when deserializing the annotated member.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; } = aliases;
}
