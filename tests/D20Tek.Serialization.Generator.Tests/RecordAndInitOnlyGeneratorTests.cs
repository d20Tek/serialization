using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Generator.Tests;

/// <summary>
/// Audits the source generator's behavior for <c>record</c> types and <c>init</c>-only
/// property accessors, since the v1.0 design primarily targeted mutable POCOs with
/// <c>get; set;</c> accessors.
/// </summary>
[TestClass]
public sealed class RecordAndInitOnlyGeneratorTests
{
    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Generate_RecordWithMutableProperties_ProducesCompilingOutput()
    {
        // arrange — a record with ordinary get; set; properties (no init-only accessors)
        const string source = """
            namespace Sample.Models;

            [D20Tek.Serialization.Serializable]
            public sealed record Person
            {
                public int Id { get; set; }
                public string? Name { get; set; }
            }
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert — mutable-property records behave like ordinary classes and compile cleanly
        Assert.IsTrue(result.GeneratorDiagnostics.IsEmpty);
        var errors = result.GetCompilationErrors();
        Assert.IsTrue(
            errors.IsEmpty,
            $"Generated output failed to compile: {string.Join("; ", errors.Select(e => e.GetMessage()))}");
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Generate_RecordWithInitOnlyProperties_ProducesCompilingOutput()
    {
        // arrange — a positional/init-only record, the idiomatic modern C# shape
        const string source = """
            namespace Sample.Models;

            [D20Tek.Serialization.Serializable]
            public sealed record Person
            {
                public int Id { get; init; }
                public string? Name { get; init; }
            }
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert — the generator does not reject init-only accessors as unsupported, and now
        // routes construction through an object initializer so init-only properties compile.
        Assert.IsTrue(result.GeneratorDiagnostics.IsEmpty);
        var errors = result.GetCompilationErrors();
        Assert.IsTrue(
            errors.IsEmpty,
            $"Generated output failed to compile: {string.Join("; ", errors.Select(e => e.GetMessage()))}");
    }

    [TestMethod]
    public void Generate_RecordWithInitOnlyProperty_IsNotReportedAsInaccessibleSetter()
    {
        // arrange — an init-only accessor is public, so it must not be misclassified as the
        // "inaccessible setter" (get-only) diagnostic.
        const string source = """
            namespace Sample.Models;

            [D20Tek.Serialization.Serializable]
            public sealed record Person
            {
                public int Id { get; init; }
            }
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert
        Assert.DoesNotContain([ExcludeFromCodeCoverage](d) => d.Id == "D20SER002", result.GeneratorDiagnostics);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Generate_PositionalRecord_ProducesCompilingOutputViaConstructor()
    {
        // arrange — a positional record has no parameterless constructor, so the generator
        // must match its get-only properties to the primary constructor's parameters by name.
        const string source = """
            namespace Sample.Models;

            [D20Tek.Serialization.Serializable]
            public sealed record Person(int Id, string? Name);
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert — the get-only Id/Name properties are matched to the primary constructor's
        // parameters instead of being reported as having an inaccessible setter, and the
        // generated Read method constructs the record through that constructor.
        Assert.DoesNotContain([ExcludeFromCodeCoverage](d) => d.Id == "D20SER002", result.GeneratorDiagnostics);
        var errors = result.GetCompilationErrors();
        Assert.IsTrue(
            errors.IsEmpty,
            $"Generated output failed to compile: {string.Join("; ", errors.Select(e => e.GetMessage()))}");
    }
}
