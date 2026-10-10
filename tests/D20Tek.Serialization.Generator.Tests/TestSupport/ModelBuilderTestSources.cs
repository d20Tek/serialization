using D20Tek.Serialization.Generation;

namespace D20Tek.Serialization.Generator.Tests.TestSupport;

/// <summary>
/// Shared compile-time sources and helpers used across the split <c>ModelBuilderTests</c>
/// classes.
/// </summary>
internal static class ModelBuilderTestSources
{
    internal const string AllTypesSource = """
        namespace Sample;

        public enum Color { Red, Green }

        [D20Tek.Serialization.Serializable]
        public sealed class AllTypes
        {
            public bool Flag { get; set; }
            public byte ByteValue { get; set; }
            public sbyte SByteValue { get; set; }
            public short ShortValue { get; set; }
            public ushort UShortValue { get; set; }
            public int IntValue { get; set; }
            public uint UIntValue { get; set; }
            public long LongValue { get; set; }
            public ulong ULongValue { get; set; }
            public float FloatValue { get; set; }
            public double DoubleValue { get; set; }
            public string StringValue { get; set; }
            public Color EnumValue { get; set; }
            public int? NullableInt { get; set; }
            public Color? NullableEnum { get; set; }
            public string? NullableString { get; set; }
        }
        """;

    internal static MemberModel BuildMember(string source, string metadataName, string memberName)
    {
        var symbol = CompilationFactory.GetTypeSymbol(source, metadataName);
        var model = ModelBuilder.Build(symbol);
        return model.Members.Single(m => m.MemberName == memberName);
    }
}
