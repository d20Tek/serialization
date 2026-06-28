using D20Tek.Serialization.Generation;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Generator.Tests.TestSupport;

/// <summary>
/// Convenience builders for constructing <see cref="SerializableTypeModel"/> and
/// <see cref="MemberModel"/> values directly, so emitter tests can exercise specific shapes
/// without round-tripping through Roslyn symbols.
/// </summary>
internal static class ModelFactory
{
    public static MemberModel Member(
        string name,
        MemberStrategy strategy,
        string? fullyQualifiedTypeName = null,
        string? serializedName = null,
        bool isNullable = false,
        bool isRequired = false) =>
        new(
            name,
            serializedName,
            fullyQualifiedTypeName ?? DefaultTypeName(strategy, isNullable),
            strategy,
            isNullable,
            isRequired);

    public static SerializableTypeModel Type(
        string typeName = "Sample",
        string? @namespace = "Demo",
        bool isValueType = false,
        params MemberModel[] members) =>
        new(
            typeName,
            @namespace,
            @namespace is null ? $"global::{typeName}" : $"global::{@namespace}.{typeName}",
            isValueType,
            members.ToImmutableArray().ToEquatableArray(),
            ImmutableArray<DiagnosticInfo>.Empty.ToEquatableArray());

    [ExcludeFromCodeCoverage]
    private static string DefaultTypeName(MemberStrategy strategy, bool isNullable)
    {
        var baseName = strategy switch
        {
            MemberStrategy.Boolean => "bool",
            MemberStrategy.Int64 => "int",
            MemberStrategy.Double => "double",
            MemberStrategy.String => "string",
            MemberStrategy.Enum => "global::Demo.Color",
            _ => "object",
        };

        return isNullable ? baseName + "?" : baseName;
    }
}
