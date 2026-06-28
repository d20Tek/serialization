using D20Tek.Serialization.Generation;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class DiagnosticInfoTests
{
    [TestMethod]
    public void ToDiagnostic_UsesDescriptorId()
    {
        // arrange
        var info = new DiagnosticInfo(
            DiagnosticDescriptors.UnsupportedMemberType,
            null,
            ImmutableArray.Create("Type", "Member", "System.Object").ToEquatableArray());

        // act
        var diagnostic = info.ToDiagnostic();

        // assert
        Assert.AreEqual("D20SER001", diagnostic.Id);
    }

    [TestMethod]
    public void ToDiagnostic_FormatsMessageWithArguments()
    {
        // arrange
        var info = new DiagnosticInfo(
            DiagnosticDescriptors.UnsupportedMemberType,
            null,
            ImmutableArray.Create("MyType", "MyMember", "System.Object").ToEquatableArray());

        // act
        var diagnostic = info.ToDiagnostic();

        // assert
        var message = diagnostic.GetMessage();
        Assert.Contains("MyType", message);
        Assert.Contains("MyMember", message);
        Assert.Contains("System.Object", message);
    }

    [TestMethod]
    public void ToDiagnostic_WithNullLocation_UsesNoneLocation()
    {
        // arrange
        var info = new DiagnosticInfo(
            DiagnosticDescriptors.UnsupportedSerializableType,
            null,
            ImmutableArray.Create("MyType").ToEquatableArray());

        // act
        var diagnostic = info.ToDiagnostic();

        // assert
        Assert.AreEqual(Location.None, diagnostic.Location);
    }

    [TestMethod]
    public void ToDiagnostic_WithLocation_ProjectsLocation()
    {
        // arrange
        var locationInfo = new LocationInfo(
            "Test.cs",
            new TextSpanInfo(5, 10),
            new LinePositionSpanInfo(1, 2, 1, 12));
        var info = new DiagnosticInfo(
            DiagnosticDescriptors.InaccessibleSetter,
            locationInfo,
            ImmutableArray.Create("MyType", "MyMember").ToEquatableArray());

        // act
        var diagnostic = info.ToDiagnostic();

        // assert
        Assert.AreEqual("Test.cs", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(5, diagnostic.Location.SourceSpan.Start);
        Assert.AreEqual(10, diagnostic.Location.SourceSpan.Length);
    }

    [TestMethod]
    public void Records_WithSameValues_AreEqual()
    {
        // arrange
        var first = new DiagnosticInfo(
            DiagnosticDescriptors.UnsupportedSerializableType,
            null,
            ImmutableArray.Create("MyType").ToEquatableArray());
        var second = new DiagnosticInfo(
            DiagnosticDescriptors.UnsupportedSerializableType,
            null,
            ImmutableArray.Create("MyType").ToEquatableArray());

        // act & assert
        Assert.AreEqual(first, second);
    }
}
