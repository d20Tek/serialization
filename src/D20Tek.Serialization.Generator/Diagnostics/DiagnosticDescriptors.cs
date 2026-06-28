using Microsoft.CodeAnalysis;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Central definitions of the diagnostics reported by <see cref="SerializableGenerator"/>.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "D20Tek.Serialization";

    /// <summary>
    /// Reported when a <c>[Serializable]</c> member uses a type the basic generator cannot emit.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsupportedMemberType = new(
        id: "D20SER001",
        title: "Unsupported serialized member type",
        messageFormat: "Property '{0}.{1}' has unsupported type '{2}' and will be skipped by the generated serializer",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Reported when a <c>[Serializable]</c> member lacks an accessible setter and cannot be read back.
    /// </summary>
    public static readonly DiagnosticDescriptor InaccessibleSetter = new(
        id: "D20SER002",
        title: "Serialized member has no accessible setter",
        messageFormat: "Property '{0}.{1}' has no accessible setter and will be skipped by the generated serializer",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Reported when <c>[Serializable]</c> is applied to a type that is not partial-compatible for
    /// emission (for example, a nested type whose containing types prevent generation).
    /// </summary>
    public static readonly DiagnosticDescriptor UnsupportedSerializableType = new(
        id: "D20SER003",
        title: "Unsupported serializable type",
        messageFormat: "Type '{0}' cannot have a serializer generated because it is a nested type; move it to a namespace scope",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
