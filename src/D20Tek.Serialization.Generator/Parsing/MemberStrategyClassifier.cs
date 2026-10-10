using Microsoft.CodeAnalysis;

namespace D20Tek.Serialization.Generation;

/// <summary>
/// Classifies an <see cref="ITypeSymbol"/> into the <see cref="MemberStrategy"/> used to emit
/// read/write code, unwrapping <see cref="Nullable{T}"/> and nullable reference annotations.
/// </summary>
internal static class MemberStrategyClassifier
{
    public static bool TryClassify(ITypeSymbol type, out MemberStrategy strategy, out bool isNullable)
    {
        isNullable = false;

        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable &&
            nullable.TypeArguments.Length == 1)
        {
            isNullable = true;
            return TryClassifyCore(nullable.TypeArguments[0], out strategy);
        }

        if (type.IsReferenceType)
        {
            isNullable = type.NullableAnnotation != NullableAnnotation.NotAnnotated;
        }

        return TryClassifyCore(type, out strategy);
    }

    private static bool TryClassifyCore(ITypeSymbol type, out MemberStrategy strategy)
    {
        if (type.TypeKind == TypeKind.Enum)
        {
            strategy = MemberStrategy.Enum;
            return true;
        }

        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
                strategy = MemberStrategy.Boolean;
                return true;
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                strategy = MemberStrategy.Int64;
                return true;
            case SpecialType.System_Single:
            case SpecialType.System_Double:
                strategy = MemberStrategy.Double;
                return true;
            case SpecialType.System_String:
                strategy = MemberStrategy.String;
                return true;
            default:
                strategy = default;
                return false;
        }
    }
}
