using D20Tek.Serialization.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class TypeMetadataBuilderTests
{
    [TestMethod]
    public void Build_SetsTypeOnMetadata()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        Assert.AreEqual(typeof(SampleModel), metadata.Type);
    }

    [TestMethod]
    public void Build_IncludesPublicReadWriteProperties()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        Assert.Contains(m => m.Name == nameof(SampleModel.Id), metadata.Members);
        Assert.Contains(m => m.Name == nameof(SampleModel.Name), metadata.Members);
    }

    [TestMethod]
    public void Build_ExcludesIgnoredMembers()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: true);

        // assert
        Assert.DoesNotContain(m => m.Name == nameof(SampleModel.Ignored), metadata.Members);
    }

    [TestMethod]
    public void Build_ExcludesReadOnlyProperty()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        Assert.DoesNotContain(m => m.Name == nameof(TypeMetadataBuilderTests.SampleModel.ReadOnly), metadata.Members);
    }

    [TestMethod]
    public void Build_ExcludesWriteOnlyProperty()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        Assert.DoesNotContain(m => m.Name == "WriteOnly", metadata.Members);
    }

    [TestMethod]
    public void Build_ExcludesIndexer()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        Assert.DoesNotContain(m => m.Name == "Item", metadata.Members);
    }

    [TestMethod]
    public void Build_SerializedNameAttribute_TakesPrecedenceOverNamingPolicy()
    {
        // arrange
        var options = new TestOptions { PropertyNamingPolicy = NamingPolicy.CamelCase };

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), options, includeFields: false);

        // assert
        var member = metadata.Members.Single(m => m.Name == nameof(SampleModel.Aliased));
        Assert.AreEqual("custom_name", member.SerializedName);
    }

    [TestMethod]
    public void Build_NamingPolicy_AppliedToSerializedName()
    {
        // arrange
        var options = new TestOptions { PropertyNamingPolicy = NamingPolicy.CamelCase };

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), options, includeFields: false);

        // assert
        var member = metadata.Members.Single(m => m.Name == nameof(SampleModel.Id));
        Assert.AreEqual("id", member.SerializedName);
    }

    [TestMethod]
    public void Build_WithoutNamingPolicy_UsesMemberName()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        var member = metadata.Members.Single(m => m.Name == nameof(SampleModel.Id));
        Assert.AreEqual(nameof(SampleModel.Id), member.SerializedName);
    }

    [TestMethod]
    public void Build_RequiredAttribute_SetsIsRequired()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        var required = metadata.Members.Single(m => m.Name == nameof(SampleModel.RequiredValue));
        var optional = metadata.Members.Single(m => m.Name == nameof(SampleModel.Name));
        Assert.IsTrue(required.IsRequired);
        Assert.IsFalse(optional.IsRequired);
    }

    [TestMethod]
    public void Build_IgnoreNull_ReflectsOptions()
    {
        // arrange
        var options = new TestOptions { IgnoreNullValues = true };

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), options, includeFields: false);

        // assert
        Assert.IsTrue(metadata.Members.All(m => m.IgnoreNull));
    }

    [TestMethod]
    public void Build_SetsMemberType()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        Assert.AreEqual(typeof(int), metadata.Members.Single(m => m.Name == nameof(SampleModel.Id)).MemberType);
        Assert.AreEqual(typeof(string), metadata.Members.Single(m => m.Name == nameof(SampleModel.Name)).MemberType);
    }

    [TestMethod]
    public void Build_WithIncludeFields_IncludesPublicFields()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: true);

        // assert
        Assert.Contains(m => m.Name == nameof(SampleModel.Field), metadata.Members);
    }

    [TestMethod]
    public void Build_WithoutIncludeFields_ExcludesFields()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        Assert.DoesNotContain(m => m.Name == nameof(SampleModel.Field), metadata.Members);
    }

    [TestMethod]
    public void Build_WithIncludeFields_ExcludesReadOnlyFields()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: true);

        // assert
        Assert.DoesNotContain(m => m.Name == nameof(SampleModel.ReadOnlyField), metadata.Members);
    }

    [TestMethod]
    public void Member_Getter_ReadsPropertyValue()
    {
        // arrange
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);
        var member = metadata.Members.Single(m => m.Name == nameof(SampleModel.Id));
        var instance = new SampleModel { Id = 123 };

        // act
        var value = member.Getter(instance);

        // assert
        Assert.AreEqual(123, value);
    }

    [TestMethod]
    public void Member_Setter_WritesPropertyValue()
    {
        // arrange
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: false);
        var member = metadata.Members.Single(m => m.Name == nameof(SampleModel.Name));
        var instance = new SampleModel();

        // act
        member.Setter!(instance, "hello");

        // assert
        Assert.AreEqual("hello", instance.Name);
    }

    [TestMethod]
    public void Member_GetterAndSetter_WorkForFields()
    {
        // arrange
        var metadata = TypeMetadataBuilder.Build(typeof(SampleModel), new TestOptions(), includeFields: true);
        var member = metadata.Members.Single(m => m.Name == nameof(SampleModel.Field));
        var instance = new SampleModel();

        // act
        member.Setter!(instance, 77);
        var value = member.Getter(instance);

        // assert
        Assert.AreEqual(77, instance.Field);
        Assert.AreEqual(77, value);
    }

    [TestMethod]
    public void Build_TypeWithoutParameterlessConstructor_ResolvesMatchingConstructor()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(PositionalModel), new TestOptions(), includeFields: false);

        // assert
        Assert.IsNotNull(metadata.Constructor);
        var idMember = metadata.Members.Single(m => m.Name == nameof(PositionalModel.Id));
        var nameMember = metadata.Members.Single(m => m.Name == nameof(PositionalModel.Name));
        Assert.AreEqual(0, idMember.ConstructorParameterIndex);
        Assert.AreEqual(1, nameMember.ConstructorParameterIndex);
        Assert.IsNull(idMember.Setter);
        Assert.IsNull(nameMember.Setter);
    }

    [TestMethod]
    public void Build_TypeWithConstructorAndExtraSettableProperty_MapsOnlyMatchedMember()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(
            typeof(PositionalModelWithExtraProperty), new TestOptions(), includeFields: false);

        // assert
        Assert.IsNotNull(metadata.Constructor);
        var idMember = metadata.Members.Single(m => m.Name == nameof(PositionalModelWithExtraProperty.Id));
        var extraMember = metadata.Members.Single(m => m.Name == nameof(PositionalModelWithExtraProperty.Extra));
        Assert.AreEqual(0, idMember.ConstructorParameterIndex);
        Assert.IsNull(idMember.Setter);
        Assert.IsNull(extraMember.ConstructorParameterIndex);
        Assert.IsNotNull(extraMember.Setter);
    }

    [TestMethod]
    public void Build_TypeWithNoParameterlessOrMatchingConstructor_LeavesConstructorNull()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(
            typeof(UnmatchedConstructorModel), new TestOptions(), includeFields: false);

        // assert
        Assert.IsNull(metadata.Constructor);
    }

    [TestMethod]
    public void Build_ValueType_DoesNotResolveConstructor()
    {
        // arrange

        // act
        var metadata = TypeMetadataBuilder.Build(typeof(SampleStruct), new TestOptions(), includeFields: false);

        // assert
        Assert.IsNull(metadata.Constructor);
        var member = metadata.Members.Single(m => m.Name == nameof(SampleStruct.Value));
        Assert.IsNotNull(member.Setter);
    }

    [TestMethod]
    public void Build_ConstructorParameterNameMatchesIndexer_IsIgnoredAndConstructorNotResolved()
    {
        // arrange — the type has no parameterless constructor, and its only other public
        // member whose name could match a parameter is an indexer, which must be excluded from
        // the candidate-property lookup (GetIndexParameters().Length == 0 filter) so it cannot
        // satisfy the constructor match.
        var metadata = TypeMetadataBuilder.Build(
            typeof(TypeWithIndexerOnly), new TestOptions(), includeFields: false);

        // act

        // assert
        Assert.IsNull(metadata.Constructor);
    }

    [TestMethod]
    public void Build_ConstructorParameterNameMatchesWriteOnlyProperty_ConstructorNotResolved()
    {
        // arrange — the type has no parameterless constructor, and the only property whose
        // name matches the constructor parameter is write-only (CanRead is false), so it must
        // be excluded from the candidate-property lookup and the constructor cannot be matched.
        var metadata = TypeMetadataBuilder.Build(
            typeof(TypeWithWriteOnlyPropertyOnly), new TestOptions(), includeFields: false);

        // act

        // assert
        Assert.IsNull(metadata.Constructor);
    }

    private sealed class TestOptions : SerializerOptions
    {
    }

    [ExcludeFromCodeCoverage]
    private sealed class SampleModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        [NameSerialized("custom_name")]
        public string? Aliased { get; set; }

        [IgnoreSerialized]
        public string? Ignored { get; set; }

        [RequiredSerialized]
        public string? RequiredValue { get; set; }

        public static string ReadOnly => "read-only";

        public static string WriteOnly { set { } }

        // Assigned through the compiled reflection setter under test.
#pragma warning disable CS0649
        public int Field;
#pragma warning restore CS0649

        public readonly int ReadOnlyField = 42;

        public string this[int index]
        {
            get => string.Empty;
            set { }
        }
    }

    [ExcludeFromCodeCoverage]
    private sealed class PositionalModel(int id, string? name)
    {
        public int Id { get; } = id;

        public string? Name { get; } = name;
    }

    [ExcludeFromCodeCoverage]
    private sealed class PositionalModelWithExtraProperty(int id)
    {
        public int Id { get; } = id;

        public string? Extra { get; set; }
    }

    [ExcludeFromCodeCoverage]
    private sealed class UnmatchedConstructorModel(int unrelatedParameterName)
    {
        public int Value { get; } = unrelatedParameterName;
    }

    [ExcludeFromCodeCoverage]
    private sealed class TypeWithIndexerOnly
    {
        private readonly int[] _values = new int[4];

        public TypeWithIndexerOnly(int index)
        {
            _ = index;
        }

        // Named "index" to match the constructor parameter name, but as an indexer it must be
        // excluded from the candidate-property lookup used to resolve the constructor.
        public int this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }
    }

    [ExcludeFromCodeCoverage]
    private sealed class TypeWithWriteOnlyPropertyOnly
    {
        public TypeWithWriteOnlyPropertyOnly(int value)
        {
            Value = value;
        }

        // Named "value" to match the constructor parameter name, but write-only (no getter), so
        // it must be excluded from the candidate-property lookup used to resolve the constructor.
        public int Value
        {
            set { }
        }
    }

    [ExcludeFromCodeCoverage]
    private struct SampleStruct
    {
        public int Value { get; set; }
    }
}
