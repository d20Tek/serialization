using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class SerializableGeneratorTests
{
    private const string SimpleModelSource = """
        namespace Sample.Models;

        [D20Tek.Serialization.Serializable]
        public sealed class Person
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }
        """;

    [TestMethod]
    public void Generate_SerializableType_EmitsSerializerSource()
    {
        // arrange

        // act
        var result = TestSupport.GeneratorRunResult.Run(SimpleModelSource);

        // assert
        var source = result.GetSourceContaining("Person.Serializer.g.cs");
        Assert.Contains("class PersonSerializer", source);
    }

    [TestMethod]
    public void Generate_SerializableType_EmitsRegistrySource()
    {
        // arrange

        // act
        var result = TestSupport.GeneratorRunResult.Run(SimpleModelSource);

        // assert
        var source = result.GetSourceContaining("GeneratedSerializerRegistry.g.cs");
        Assert.Contains("class GeneratedSerializerRegistry", source);
    }

    [TestMethod]
    public void Generate_SerializableType_ReportsNoGeneratorDiagnostics()
    {
        // arrange

        // act
        var result = TestSupport.GeneratorRunResult.Run(SimpleModelSource);

        // assert
        Assert.IsTrue(result.GeneratorDiagnostics.IsEmpty);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Generate_SerializableType_ProducesCompilingOutput()
    {
        // arrange

        // act
        var result = TestSupport.GeneratorRunResult.Run(SimpleModelSource);

        // assert
        var errors = result.GetCompilationErrors();
        Assert.IsTrue(
            errors.IsEmpty,
            $"Generated output failed to compile: {string.Join("; ", errors.Select(e => e.GetMessage()))}");
    }

    [TestMethod]
    public void Generate_NoSerializableTypes_DoesNotEmitRegistry()
    {
        // arrange
        const string source = """
            namespace Sample.Models;

            public sealed class Plain
            {
                public int Id { get; set; }
            }
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert
        Assert.DoesNotContain([ExcludeFromCodeCoverage](k) => k.Contains("GeneratedSerializerRegistry"), result.GeneratedSources.Keys);
    }

    [TestMethod]
    public void Generate_NestedSerializableType_ReportsUnsupportedDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample.Models;

            public sealed class Outer
            {
                [D20Tek.Serialization.Serializable]
                public sealed class Inner
                {
                    public int Id { get; set; }
                }
            }
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert
        Assert.Contains(d => d.Id == "D20SER003", result.GeneratorDiagnostics);
    }

    [TestMethod]
    public void Generate_UnsupportedMemberType_ReportsDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample.Models;

            [D20Tek.Serialization.Serializable]
            public sealed class WithUnsupported
            {
                public int Id { get; set; }
                public object? Extra { get; set; }
            }
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert
        Assert.Contains(d => d.Id == "D20SER001", result.GeneratorDiagnostics);
    }

    [TestMethod]
    public void Generate_InaccessibleSetter_ReportsDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample.Models;

            [D20Tek.Serialization.Serializable]
            public sealed class WithGetOnly
            {
                public int Id { get; set; }
                public string ReadOnly { get; } = string.Empty;
            }
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert
        Assert.Contains(d => d.Id == "D20SER002", result.GeneratorDiagnostics);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Generate_StructSerializableType_ProducesCompilingOutput()
    {
        // arrange
        const string source = """
            namespace Sample.Models;

            [D20Tek.Serialization.Serializable]
            public struct Point
            {
                public int X { get; set; }
                public int Y { get; set; }
            }
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert
        var errors = result.GetCompilationErrors();
        Assert.IsTrue(
            errors.IsEmpty,
            $"Generated output failed to compile: {string.Join("; ", errors.Select(e => e.GetMessage()))}");
    }

    [TestMethod]
    public void Generate_MultipleSerializableTypes_EmitsRegistryEntriesForEach()
    {
        // arrange
        const string source = """
            namespace Sample.Models;

            [D20Tek.Serialization.Serializable]
            public sealed class First
            {
                public int Id { get; set; }
            }

            [D20Tek.Serialization.Serializable]
            public sealed class Second
            {
                public int Id { get; set; }
            }
            """;

        // act
        var result = TestSupport.GeneratorRunResult.Run(source);

        // assert
        var registry = result.GetSourceContaining("GeneratedSerializerRegistry.g.cs");
        Assert.Contains("Sample.Models.First", registry);
        Assert.Contains("Sample.Models.Second", registry);
    }
}
