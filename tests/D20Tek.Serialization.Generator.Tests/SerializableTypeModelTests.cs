using D20Tek.Serialization.Generation;
using D20Tek.Serialization.Generator.Tests.TestSupport;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class SerializableTypeModelTests
{
    [TestMethod]
    public void SerializerTypeName_AppendsSerializerSuffix()
    {
        // arrange
        var model = ModelFactory.Type("Person", "Sample");

        // act
        var name = model.SerializerTypeName;

        // assert
        Assert.AreEqual("PersonSerializer", name);
    }

    [TestMethod]
    public void FlattenedIdentifier_WithNamespace_ReplacesDotsWithUnderscores()
    {
        // arrange
        var model = ModelFactory.Type("Person", "Sample.Models");

        // act
        var identifier = model.FlattenedIdentifier;

        // assert
        Assert.AreEqual("Sample_Models_Person", identifier);
    }

    [TestMethod]
    public void FlattenedIdentifier_WithoutNamespace_UsesTypeName()
    {
        // arrange
        var model = ModelFactory.Type("RootType", @namespace: null);

        // act
        var identifier = model.FlattenedIdentifier;

        // assert
        Assert.AreEqual("RootType", identifier);
    }

    [TestMethod]
    public void Records_WithSameValues_AreEqual()
    {
        // arrange
        var first = ModelFactory.Type("Person", "Sample", members: ModelFactory.Member("Id", MemberStrategy.Int64));
        var second = ModelFactory.Type("Person", "Sample", members: ModelFactory.Member("Id", MemberStrategy.Int64));

        // act & assert
        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void Records_WithDifferentMembers_AreNotEqual()
    {
        // arrange
        var first = ModelFactory.Type("Person", "Sample", members: ModelFactory.Member("Id", MemberStrategy.Int64));
        var second = ModelFactory.Type("Person", "Sample", members: ModelFactory.Member("Name", MemberStrategy.String));

        // act & assert
        Assert.AreNotEqual(first, second);
    }
}
