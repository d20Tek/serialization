namespace D20Tek.Serialization.Generation;

/// <summary>
/// Shared string-expression helpers used by <see cref="WriteMemberEmitter"/> and
/// <see cref="ReadMemberEmitter"/> when generating member access and literal expressions.
/// </summary>
internal static class MemberExpressionHelper
{
    public static string UnderlyingNonNullType(MemberModel member) =>
        member.FullyQualifiedTypeName.EndsWith("?", StringComparison.Ordinal)
            ? member.FullyQualifiedTypeName.Substring(0, member.FullyQualifiedTypeName.Length - 1)
            : member.FullyQualifiedTypeName;

    public static string NonNullAccess(MemberModel member, string access) =>
        member.Strategy == MemberStrategy.String ? access : $"{access}.Value";

    public static string SerializedNameExpression(MemberModel member) =>
        member.SerializedName is not null
            ? Literal(member.SerializedName)
            : $"options.PropertyNamingPolicy?.ConvertName({Literal(member.MemberName)}) ?? {Literal(member.MemberName)}";

    public static string Literal(string value) =>
        "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}
