using D20Tek.Serialization.Metadata;

namespace D20Tek.Serialization.Binary.Reflection;

internal static class ConversionHelper
{
    public static Dictionary<string, MemberMetadata> BuildMemberLookup(TypeMetadata metadata)
    {
        var lookup = new Dictionary<string, MemberMetadata>(metadata.Members.Count, StringComparer.Ordinal);
        foreach (var member in metadata.Members)
        {
            lookup[member.SerializedName] = member;
        }

        return lookup;
    }

    public static Type GetUnderlyingType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    public static bool IsIntegerType(Type type) =>
        type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) ||
        type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong);

    public static bool IsFloatingPointType(Type type) =>
        type == typeof(float) || type == typeof(double) || type == typeof(decimal);

    public static object ConvertInteger(long raw, Type target) =>
        target switch
        {
            _ when target == typeof(byte) => (byte)raw,
            _ when target == typeof(sbyte) => (sbyte)raw,
            _ when target == typeof(short) => (short)raw,
            _ when target == typeof(ushort) => (ushort)raw,
            _ when target == typeof(int) => (int)raw,
            _ when target == typeof(uint) => (uint)raw,
            _ when target == typeof(ulong) => (ulong)raw,
            _ => raw
        };

    public static object ConvertFloat(double raw, Type target) =>
        target switch
        {
            _ when target == typeof(float) => (float)raw,
            _ when target == typeof(decimal) => (decimal)raw,
            _ => raw
        };
}
