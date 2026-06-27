namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class SerializationPathBuilderTests
{
    [TestMethod]
    public void ToPath_WithNoSegments_ReturnsRoot()
    {
        // arrange
        var builder = new SerializationPathBuilder();

        // act
        var path = builder.ToPath();

        // assert
        Assert.AreEqual("$", path);
        Assert.AreEqual(0, builder.Depth);
    }

    [TestMethod]
    public void ToPath_WithNestedProperties_BuildsDottedPath()
    {
        // arrange
        var builder = new SerializationPathBuilder();
        builder.PushProperty("user");
        builder.PushProperty("address");
        builder.PushProperty("street");

        // act
        var path = builder.ToPath();

        // assert
        Assert.AreEqual("$.user.address.street", path);
    }

    [TestMethod]
    public void ToPath_WithPropertyIndexAndProperty_BuildsArrayPath()
    {
        // arrange
        var builder = new SerializationPathBuilder();
        builder.PushProperty("items");
        builder.PushIndex(3);
        builder.PushProperty("price");

        // act
        var path = builder.ToPath();

        // assert
        Assert.AreEqual("$.items[3].price", path);
    }

    [TestMethod]
    public void Pop_RemovesTopSegment()
    {
        // arrange
        var builder = new SerializationPathBuilder();
        builder.PushProperty("user");
        builder.PushProperty("address");

        // act
        builder.Pop();

        // assert
        Assert.AreEqual("$.user", builder.ToPath());
        Assert.AreEqual(1, builder.Depth);
    }

    [TestMethod]
    public void SetIndex_ReplacesTopArrayIndex()
    {
        // arrange
        var builder = new SerializationPathBuilder();
        builder.PushProperty("items");
        builder.PushIndex(0);

        // act
        builder.SetIndex(5);

        // assert
        Assert.AreEqual("$.items[5]", builder.ToPath());
        Assert.AreEqual(2, builder.Depth);
    }

    [TestMethod]
    public void SetIndex_WhenTopIsNotArrayIndex_PushesNewIndex()
    {
        // arrange
        var builder = new SerializationPathBuilder();
        builder.PushProperty("items");

        // act
        builder.SetIndex(2);

        // assert
        Assert.AreEqual("$.items[2]", builder.ToPath());
        Assert.AreEqual(2, builder.Depth);
    }

    [TestMethod]
    public void SetIndex_WhenEmpty_PushesNewIndex()
    {
        // arrange
        var builder = new SerializationPathBuilder();

        // act
        builder.SetIndex(4);

        // assert
        Assert.AreEqual("$[4]", builder.ToPath());
        Assert.AreEqual(1, builder.Depth);
    }

    [TestMethod]
    public void SetIndex_WhenTopIsRootArrayIndex_ReplacesTopArrayIndex()
    {
        // arrange
        var builder = new SerializationPathBuilder();
        builder.PushIndex(0);

        // act
        builder.SetIndex(7);

        // assert
        Assert.AreEqual("$[7]", builder.ToPath());
        Assert.AreEqual(1, builder.Depth);
    }

    [TestMethod]
    public void Pop_WhenEmpty_DoesNotThrow()
    {
        // arrange
        var builder = new SerializationPathBuilder();

        // act
        builder.Pop();

        // assert
        Assert.AreEqual("$", builder.ToPath());
    }

    [TestMethod]
    public void BuildPath_FromSegments_RendersPath()
    {
        // arrange
        var segments = new[]
        {
            PathSegment.Property("items"),
            PathSegment.ArrayIndex(3),
            PathSegment.Property("price"),
        };

        // act
        var path = SerializationPathBuilder.BuildPath(segments);

        // assert
        Assert.AreEqual("$.items[3].price", path);
    }

    [TestMethod]
    public void ToString_ReturnsSameAsToPath()
    {
        // arrange
        var builder = new SerializationPathBuilder();
        builder.PushProperty("user");
        builder.PushProperty("name");

        // act
        var result = builder.ToString();

        // assert
        Assert.AreEqual(builder.ToPath(), result);
    }
}
