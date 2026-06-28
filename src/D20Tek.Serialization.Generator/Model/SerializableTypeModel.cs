namespace D20Tek.Serialization.Generation;

/// <summary>
/// A value-equatable description of a <c>[Serializable]</c> type and its serialized members,
/// produced by the model builder and consumed by the source emitters. Includes any diagnostics
/// discovered while building the model so they can be reported alongside emitted output.
/// </summary>
/// <param name="TypeName">The simple type name.</param>
/// <param name="Namespace">The containing namespace, or <see langword="null"/> for the global namespace.</param>
/// <param name="FullyQualifiedName">The global-qualified type name used in generated references.</param>
/// <param name="IsValueType">Whether the type is a struct (affects the generated <c>Read</c> return).</param>
/// <param name="Members">The serialized members, in declaration order.</param>
/// <param name="Diagnostics">Diagnostics discovered while building this type's model.</param>
internal sealed record SerializableTypeModel(
    string TypeName,
    string? Namespace,
    string FullyQualifiedName,
    bool IsValueType,
    EquatableArray<MemberModel> Members,
    EquatableArray<DiagnosticInfo> Diagnostics) : IEquatable<SerializableTypeModel>
{
    /// <summary>
    /// Gets the name of the generated serializer class for this type (unique within the assembly).
    /// </summary>
    public string SerializerTypeName => $"{TypeName}Serializer";

    /// <summary>
    /// Gets a flattened identifier (namespace + type) safe for use in generated member names.
    /// </summary>
    public string FlattenedIdentifier =>
        (Namespace is null ? TypeName : $"{Namespace}.{TypeName}").Replace('.', '_');
}
