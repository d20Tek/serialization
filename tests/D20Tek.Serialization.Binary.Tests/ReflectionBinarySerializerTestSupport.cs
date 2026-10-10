using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Binary.Reflection;

namespace D20Tek.Serialization.Binary.Tests;

/// <summary>
/// Shared test models and serialize/deserialize helpers used by the split
/// <c>ReflectionBinarySerializer*Tests</c> classes.
/// </summary>
internal static class ReflectionBinarySerializerTestSupport
{
    internal static byte[] Serialize<T>(
        this ReflectionBinarySerializer serializer, T value, BinarySerializerOptions options)
    {
        var writer = new CborFormatWriter();
        serializer.Write(writer, value, options);
        return writer.Encode();
    }

    internal static T? Deserialize<T>(
        this ReflectionBinarySerializer serializer, byte[] bytes, BinarySerializerOptions options)
    {
        var reader = new CborFormatReader(bytes);
        return serializer.Read<T>(reader, options);
    }

    public class SimpleModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    public class ParentModel
    {
        public string? Label { get; set; }
        public SimpleModel? Child { get; set; }
    }

    public class ModelWithFields
    {
        public int Id;
        public string? Description;

        // A property to ensure it's always serialized
        public string? Tag { get; set; }
    }

    public class RequiredModel
    {
        [RequiredSerialized]
        public string? RequiredField { get; set; }
    }

    public enum TestStatus
    {
        Inactive = 0,
        Active = 1,
        Suspended = 2
    }

    public class EnumModel
    {
        public TestStatus Status { get; set; }
    }

    public class NullableModel
    {
        public double? Score { get; set; }
    }

    public class IntegersModel
    {
        public byte ByteVal { get; set; }
        public short ShortVal { get; set; }
        public int IntVal { get; set; }
        public long LongVal { get; set; }
    }

    public class FloatsModel
    {
        public float FloatVal { get; set; }
        public double DoubleVal { get; set; }
    }

    public class CustomNameModel
    {
        [NameSerialized("custom_value")]
        public string? Value { get; set; }
    }

    public class IgnoredMemberModel
    {
        public string? Visible { get; set; }

        [IgnoreSerialized]
        public string? Secret { get; set; }
    }

    public class AliasModel
    {
        [NameSerialized("new_name", "old_name", "older_name")]
        public string? Value { get; set; }
    }

    public class AliasRequiredModel
    {
        [NameSerialized("new_name", "old_name")]
        [RequiredSerialized]
        public string? Value { get; set; }
    }
}
