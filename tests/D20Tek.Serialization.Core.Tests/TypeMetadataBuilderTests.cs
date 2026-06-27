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
        member.Setter(instance, "hello");

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
        member.Setter(instance, 77);
        var value = member.Getter(instance);

        // assert
        Assert.AreEqual(77, instance.Field);
        Assert.AreEqual(77, value);
    }

    private sealed class TestOptions : SerializerOptions
    {
    }

    [ExcludeFromCodeCoverage]
    private sealed class SampleModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        [SerializedName("custom_name")]
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
}
