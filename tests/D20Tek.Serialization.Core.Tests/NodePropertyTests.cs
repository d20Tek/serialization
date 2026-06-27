using D20Tek.Serialization.Dom;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class NodePropertyTests
{
    [TestMethod]
    public void Constructor_SetsNameAndValue()
    {
        // arrange
        var value = Node.CreateString("Alice");

        // act
        var property = new NodeProperty("name", value);

        // assert
        Assert.AreEqual("name", property.Name);
        Assert.AreEqual(NodeKind.String, property.Value.Kind);
        Assert.AreEqual("Alice", property.Value.GetString());
    }

    [TestMethod]
    public void Constructor_WithNullName_Throws()
    {
        // arrange

        // act + assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => new NodeProperty(null!, Node.Null));
    }

    [TestMethod]
    public void Default_HasNullNameAndNullKindValue()
    {
        // arrange

        // act
        NodeProperty property = default;

        // assert
        Assert.IsNull(property.Name);
        Assert.AreEqual(NodeKind.Null, property.Value.Kind);
    }
}
