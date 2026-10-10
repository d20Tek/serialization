using D20Tek.Serialization.Generation;
using D20Tek.Serialization.Generator.Tests.TestSupport;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class ModelBuilderTypeTests
{
    [TestMethod]
    public void Build_SetsTypeNameAndNamespace()
    {
        // arrange
        var symbol = CompilationFactory.GetTypeSymbol(ModelBuilderTestSources.AllTypesSource, "Sample.AllTypes");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.AreEqual("AllTypes", model.TypeName);
        Assert.AreEqual("Sample", model.Namespace);
    }

    [TestMethod]
    public void Build_SetsFullyQualifiedName()
    {
        // arrange
        var symbol = CompilationFactory.GetTypeSymbol(ModelBuilderTestSources.AllTypesSource, "Sample.AllTypes");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.AreEqual("global::Sample.AllTypes", model.FullyQualifiedName);
    }

    [TestMethod]
    public void Build_Class_IsNotValueType()
    {
        // arrange
        var symbol = CompilationFactory.GetTypeSymbol(ModelBuilderTestSources.AllTypesSource, "Sample.AllTypes");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.IsFalse(model.IsValueType);
    }

    [TestMethod]
    public void Build_Struct_IsValueType()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public struct Point
            {
                public int X { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.Point");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.IsTrue(model.IsValueType);
    }

    [TestMethod]
    public void Build_GlobalNamespaceType_HasNullNamespace()
    {
        // arrange
        const string source = """
            [D20Tek.Serialization.Serializable]
            public sealed class RootType
            {
                public int Id { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "RootType");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.IsNull(model.Namespace);
    }
}
