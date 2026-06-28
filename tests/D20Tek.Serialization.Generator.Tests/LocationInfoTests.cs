using D20Tek.Serialization.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class LocationInfoTests
{
    [TestMethod]
    public void CreateFrom_LocationWithoutSourceTree_ReturnsNull()
    {
        // arrange

        // act
        var info = LocationInfo.CreateFrom(Location.None);

        // assert
        Assert.IsNull(info);
    }

    [TestMethod]
    public void CreateFrom_NullLocation_ReturnsNull()
    {
        // arrange

        // act
        var info = LocationInfo.CreateFrom(null);

        // assert
        Assert.IsNull(info);
    }

    [TestMethod]
    public void CreateFrom_LocationWithSourceTree_CapturesFilePath()
    {
        // arrange
        var location = CreateSampleLocation();

        // act
        var info = LocationInfo.CreateFrom(location);

        // assert
        Assert.IsNotNull(info);
        Assert.AreEqual("Sample.cs", info.FilePath);
    }

    [TestMethod]
    public void CreateFrom_LocationWithSourceTree_CapturesSpan()
    {
        // arrange
        var location = CreateSampleLocation();

        // act
        var info = LocationInfo.CreateFrom(location);

        // assert
        Assert.IsNotNull(info);
        Assert.AreEqual(location.SourceSpan.Start, info.Span.Start);
        Assert.AreEqual(location.SourceSpan.Length, info.Span.Length);
    }

    [TestMethod]
    public void ToLocation_ReproducesFilePathAndSpan()
    {
        // arrange
        var original = CreateSampleLocation();
        var info = LocationInfo.CreateFrom(original);

        // act
        var location = info!.ToLocation();

        // assert
        Assert.AreEqual("Sample.cs", location.GetLineSpan().Path);
        Assert.AreEqual(original.SourceSpan.Start, location.SourceSpan.Start);
        Assert.AreEqual(original.SourceSpan.Length, location.SourceSpan.Length);
    }

    [TestMethod]
    public void ToLocation_ReproducesLinePositions()
    {
        // arrange
        var original = CreateSampleLocation();
        var info = LocationInfo.CreateFrom(original);

        // act
        var location = info!.ToLocation();

        // assert
        var lineSpan = location.GetLineSpan().Span;
        Assert.AreEqual(original.GetLineSpan().Span.Start.Line, lineSpan.Start.Line);
        Assert.AreEqual(original.GetLineSpan().Span.Start.Character, lineSpan.Start.Character);
    }

    [TestMethod]
    public void TextSpanInfo_WithSameValues_AreEqual()
    {
        // arrange
        var first = new TextSpanInfo(3, 7);
        var second = new TextSpanInfo(3, 7);

        // act & assert
        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void LinePositionSpanInfo_WithSameValues_AreEqual()
    {
        // arrange
        var first = new LinePositionSpanInfo(1, 2, 3, 4);
        var second = new LinePositionSpanInfo(1, 2, 3, 4);

        // act & assert
        Assert.AreEqual(first, second);
    }

    private static Location CreateSampleLocation()
    {
        var tree = CSharpSyntaxTree.ParseText("class Probe { }", path: "Sample.cs");
        return Location.Create(tree, new TextSpan(6, 5));
    }
}
