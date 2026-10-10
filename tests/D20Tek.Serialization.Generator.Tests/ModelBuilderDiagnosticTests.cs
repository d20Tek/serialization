using D20Tek.Serialization.Generation;
using D20Tek.Serialization.Generator.Tests.TestSupport;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class ModelBuilderDiagnosticTests
{
    private static readonly string[] expected = ["First", "Second", "Third"];

    [TestMethod]
    public void Build_GetOnlyProperty_ReportsInaccessibleSetterDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithGetOnly
            {
                public int Id { get; set; }
                public string Name { get; } = string.Empty;
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithGetOnly");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "Name", model.Members);
    }

    [TestMethod]
    public void Build_PrivateSetter_ReportsInaccessibleSetterDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithPrivateSetter
            {
                public int Id { get; set; }
                public string Name { get; private set; } = string.Empty;
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithPrivateSetter");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
    }

    [TestMethod]
    public void Build_UnsupportedMemberType_ReportsDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithUnsupported
            {
                public int Id { get; set; }
                public object? Extra { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithUnsupported");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER001", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "Extra", model.Members);
    }

    [TestMethod]
    public void Build_NestedType_ReportsUnsupportedSerializableTypeDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            public sealed class Outer
            {
                [D20Tek.Serialization.Serializable]
                public sealed class Inner
                {
                    public int Id { get; set; }
                }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.Outer+Inner");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER003", model.Diagnostics);
    }

    [TestMethod]
    public void Build_StaticProperty_IsExcludedWithoutDiagnostic()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithStatic
            {
                public int Id { get; set; }
                public static int Counter { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithStatic");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(m => m.MemberName == "Counter", model.Members);
        Assert.AreEqual(0, model.Diagnostics.Count);
    }

    [TestMethod]
    public void Build_NonPublicProperty_IsExcluded()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithInternal
            {
                public int Id { get; set; }
                internal int Hidden { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithInternal");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(m => m.MemberName == "Hidden", model.Members);
    }

    [TestMethod]
    public void Build_Indexer_IsExcluded()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithIndexer
            {
                public int Id { get; set; }
                public string this[int index] { get => string.Empty; set { } }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithIndexer");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(m => m.MemberName == "Item", model.Members);
    }

    [TestMethod]
    public void Build_PreservesMemberDeclarationOrder()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class Ordered
            {
                public int First { get; set; }
                public int Second { get; set; }
                public int Third { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.Ordered");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.AreSequenceEqual(expected, [.. model.Members.Select(m => m.MemberName)]);
    }
}
