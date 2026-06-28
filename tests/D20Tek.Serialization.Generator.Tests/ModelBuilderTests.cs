using D20Tek.Serialization.Generation;
using D20Tek.Serialization.Generator.Tests.TestSupport;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class ModelBuilderTests
{
    private const string AllTypesSource = """
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

    private static readonly string[] expected = ["First", "Second", "Third"];

    [TestMethod]
    public void Build_SetsTypeNameAndNamespace()
    {
        // arrange
        var symbol = CompilationFactory.GetTypeSymbol(AllTypesSource, "Sample.AllTypes");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.AreEqual("AllTypes", model.TypeName);
        Assert.AreEqual("Sample", model.Namespace);
    }

    [TestMethod]
    public void Build_SetsFullyQualifiedName()
    {
        // arrange
        var symbol = CompilationFactory.GetTypeSymbol(AllTypesSource, "Sample.AllTypes");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.AreEqual("global::Sample.AllTypes", model.FullyQualifiedName);
    }

    [TestMethod]
    public void Build_Class_IsNotValueType()
    {
        // arrange
        var symbol = CompilationFactory.GetTypeSymbol(AllTypesSource, "Sample.AllTypes");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.IsFalse(model.IsValueType);
    }

    [TestMethod]
    public void Build_Struct_IsValueType()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public struct Point
            {
                public int X { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.Point");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.IsTrue(model.IsValueType);
    }

    [TestMethod]
    public void Build_GlobalNamespaceType_HasNullNamespace()
    {
        // arrange
        const string source = """
            [D20Tek.Serialization.Serializable]
            public sealed class RootType
            {
                public int Id { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "RootType");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.IsNull(model.Namespace);
    }

    [TestMethod]
    public void Build_BooleanProperty_ClassifiesAsBoolean()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "Flag");

        // assert
        Assert.AreEqual(MemberStrategy.Boolean, member.Strategy);
    }

    [TestMethod]
    [DataRow("ByteValue")]
    [DataRow("SByteValue")]
    [DataRow("ShortValue")]
    [DataRow("UShortValue")]
    [DataRow("IntValue")]
    [DataRow("UIntValue")]
    [DataRow("LongValue")]
    [DataRow("ULongValue")]
    public void Build_IntegralProperty_ClassifiesAsInt64(string memberName)
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", memberName);

        // assert
        Assert.AreEqual(MemberStrategy.Int64, member.Strategy);
    }

    [TestMethod]
    [DataRow("FloatValue")]
    [DataRow("DoubleValue")]
    public void Build_FloatingPointProperty_ClassifiesAsDouble(string memberName)
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", memberName);

        // assert
        Assert.AreEqual(MemberStrategy.Double, member.Strategy);
    }

    [TestMethod]
    public void Build_StringProperty_ClassifiesAsString()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "StringValue");

        // assert
        Assert.AreEqual(MemberStrategy.String, member.Strategy);
    }

    [TestMethod]
    public void Build_EnumProperty_ClassifiesAsEnum()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "EnumValue");

        // assert
        Assert.AreEqual(MemberStrategy.Enum, member.Strategy);
    }

    [TestMethod]
    public void Build_NullableValueType_IsNullableAndClassifiesUnderlying()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "NullableInt");

        // assert
        Assert.AreEqual(MemberStrategy.Int64, member.Strategy);
        Assert.IsTrue(member.IsNullable);
    }

    [TestMethod]
    public void Build_NullableEnum_IsNullableAndClassifiesAsEnum()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "NullableEnum");

        // assert
        Assert.AreEqual(MemberStrategy.Enum, member.Strategy);
        Assert.IsTrue(member.IsNullable);
    }

    [TestMethod]
    public void Build_NullableReferenceType_IsNullable()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "NullableString");

        // assert
        Assert.AreEqual(MemberStrategy.String, member.Strategy);
        Assert.IsTrue(member.IsNullable);
    }

    [TestMethod]
    public void Build_NonNullableReferenceType_IsNotNullable()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "StringValue");

        // assert
        Assert.IsFalse(member.IsNullable);
    }

    [TestMethod]
    public void Build_SerializedNameAttribute_SetsSerializedName()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class Aliased
            {
                [D20Tek.Serialization.NameSerialized("custom_name")]
                public int Id { get; set; }
            }
            """;

        // act
        var member = BuildMember(source, "Sample.Aliased", "Id");

        // assert
        Assert.AreEqual("custom_name", member.SerializedName);
    }

    [TestMethod]
    public void Build_WithoutSerializedNameAttribute_HasNullSerializedName()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "IntValue");

        // assert
        Assert.IsNull(member.SerializedName);
    }

    [TestMethod]
    public void Build_RequiredSerializedAttribute_MarksMemberRequired()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithRequired
            {
                [D20Tek.Serialization.RequiredSerialized]
                public int Id { get; set; }
            }
            """;

        // act
        var member = BuildMember(source, "Sample.WithRequired", "Id");

        // assert
        Assert.IsTrue(member.IsRequired);
    }

    [TestMethod]
    public void Build_WithoutRequiredAttribute_MemberIsNotRequired()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "IntValue");

        // assert
        Assert.IsFalse(member.IsRequired);
    }

    [TestMethod]
    public void Build_IgnoredMember_IsExcluded()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithIgnored
            {
                public int Id { get; set; }

                [D20Tek.Serialization.IgnoreSerialized]
                public int Secret { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithIgnored");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(m => m.MemberName == "Secret", model.Members);
    }

    [TestMethod]
    public void Build_GetOnlyProperty_ReportsInaccessibleSetterDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithGetOnly
            {
                public int Id { get; set; }
                public string Name { get; } = string.Empty;
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithGetOnly");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "Name", model.Members);
    }

    [TestMethod]
    public void Build_PrivateSetter_ReportsInaccessibleSetterDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithPrivateSetter
            {
                public int Id { get; set; }
                public string Name { get; private set; } = string.Empty;
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithPrivateSetter");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
    }

    [TestMethod]
    public void Build_UnsupportedMemberType_ReportsDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithUnsupported
            {
                public int Id { get; set; }
                public object? Extra { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithUnsupported");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER001", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "Extra", model.Members);
    }

    [TestMethod]
    public void Build_NestedType_ReportsUnsupportedSerializableTypeDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            public sealed class Outer
            {
                [D20Tek.Serialization.Serializable]
                public sealed class Inner
                {
                    public int Id { get; set; }
                }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.Outer+Inner");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER003", model.Diagnostics);
    }

    [TestMethod]
    public void Build_StaticProperty_IsExcludedWithoutDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithStatic
            {
                public int Id { get; set; }
                public static int Counter { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithStatic");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(m => m.MemberName == "Counter", model.Members);
        Assert.AreEqual(0, model.Diagnostics.Count);
    }

    [TestMethod]
    public void Build_NonPublicProperty_IsExcluded()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithInternal
            {
                public int Id { get; set; }
                internal int Hidden { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithInternal");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(m => m.MemberName == "Hidden", model.Members);
    }

    [TestMethod]
    public void Build_Indexer_IsExcluded()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithIndexer
            {
                public int Id { get; set; }
                public string this[int index] { get => string.Empty; set { } }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithIndexer");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(m => m.MemberName == "Item", model.Members);
    }

    [TestMethod]
    public void Build_PreservesMemberDeclarationOrder()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class Ordered
            {
                public int First { get; set; }
                public int Second { get; set; }
                public int Third { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.Ordered");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        CollectionAssert.AreEqual(expected, model.Members.Select(m => m.MemberName).ToArray());
    }

    private static MemberModel BuildMember(string source, string metadataName, string memberName)
    {
        var symbol = CompilationFactory.GetTypeSymbol(source, metadataName);
        var model = ModelBuilder.Build(symbol);
        return model.Members.Single(m => m.MemberName == memberName);
    }
}
