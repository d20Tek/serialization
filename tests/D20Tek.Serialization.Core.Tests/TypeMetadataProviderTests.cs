using D20Tek.Serialization.Metadata;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class TypeMetadataProviderTests
{
    [TestMethod]
    public void GetOrAdd_ReturnsMetadataForType()
    {
        // arrange
        var provider = new TypeMetadataProvider();
        var options = new TestOptions();

        // act
        var metadata = provider.GetOrAdd(typeof(SampleModel), options, includeFields: false);

        // assert
        Assert.IsNotNull(metadata);
        Assert.AreEqual(typeof(SampleModel), metadata.Type);
        Assert.Contains(m => m.Name == nameof(SampleModel.Id), metadata.Members);
    }

    [TestMethod]
    public void GetOrAdd_ReturnsCachedInstance_ForSameTypeAndOptions()
    {
        // arrange
        var provider = new TypeMetadataProvider();
        var options = new TestOptions();

        // act
        var first = provider.GetOrAdd(typeof(SampleModel), options, includeFields: false);
        var second = provider.GetOrAdd(typeof(SampleModel), options, includeFields: false);

        // assert
        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void GetOrAdd_ReturnsDistinctInstances_ForDifferentOptions()
    {
        // arrange
        var provider = new TypeMetadataProvider();

        // act
        var first = provider.GetOrAdd(typeof(SampleModel), new TestOptions(), includeFields: false);
        var second = provider.GetOrAdd(typeof(SampleModel), new TestOptions(), includeFields: false);

        // assert
        Assert.AreNotSame(first, second);
    }

    [TestMethod]
    public void GetOrAdd_CachesIndependently_ForDifferentTypes()
    {
        // arrange
        var provider = new TypeMetadataProvider();
        var options = new TestOptions();

        // act
        var model = provider.GetOrAdd(typeof(SampleModel), options, includeFields: false);
        var other = provider.GetOrAdd(typeof(OtherModel), options, includeFields: false);

        // assert
        Assert.AreEqual(typeof(SampleModel), model.Type);
        Assert.AreEqual(typeof(OtherModel), other.Type);
        Assert.AreNotSame(model, other);
    }

    private sealed class TestOptions : SerializerOptions
    {
    }

    private sealed class SampleModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }

    private sealed class OtherModel
    {
        public string? Title { get; set; }
    }
}
