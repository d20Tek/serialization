using D20Tek.Serialization.Generation;
using D20Tek.Serialization.Generator.Tests.TestSupport;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class ModelBuilderConstructorTests
{
    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Build_ConstructorParameterWithNoMatchingProperty_ReportsInaccessibleSetterDiagnostic()
    {
        // arrange — the only non-default constructor takes a parameter that does not
        // correspond to any serializable property by name, so the constructor cannot be
        // matched and the get-only property falls back to being reported as unsupported.
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithUnmatchedConstructorParameter
            {
                public WithUnmatchedConstructorParameter(int unrelatedParameterName)
                {
                    Id = unrelatedParameterName;
                }

                public int Id { get; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithUnmatchedConstructorParameter");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "Id", model.Members);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Build_ConstructorParameterTypeMismatch_ReportsInaccessibleSetterDiagnostic()
    {
        // arrange — the constructor parameter name matches a property, but the declared types
        // differ, so the type-equality check fails and the constructor is rejected.
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithMismatchedConstructorType
            {
                public WithMismatchedConstructorType(string id)
                {
                    Id = int.Parse(id);
                }

                public int Id { get; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithMismatchedConstructorType");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "Id", model.Members);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Build_MultipleConstructorsWithOnlySmallerMatching_SelectsMatchingConstructor()
    {
        // arrange — the largest constructor has an unmatched parameter and must be rejected,
        // falling through to the smaller constructor that fully matches, exercising both the
        // "matched = false; break;" path and the subsequent successful match on retry.
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithMultipleConstructors
            {
                public WithMultipleConstructors(int id, string extra)
                {
                    Id = id;
                }

                public WithMultipleConstructors(int id)
                {
                    Id = id;
                }

                public int Id { get; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithMultipleConstructors");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.Contains(m => m.MemberName == "Id", model.Members);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Build_NonPublicConstructorOnly_IsExcludedFromCandidates()
    {
        // arrange — the only non-static constructor is private, so it must be filtered out by
        // the `DeclaredAccessibility == Public` check, leaving no candidate constructor at all
        // (not even one to reject for parameter mismatch), and the get-only property falls back
        // to being reported as unsupported.
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithPrivateConstructor
            {
                private WithPrivateConstructor(int id)
                {
                    Id = id;
                }

                public static WithPrivateConstructor Create(int id) => new(id);

                public int Id { get; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithPrivateConstructor");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "Id", model.Members);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Build_StaticConstructorAndMatchingInstanceConstructor_IgnoresStaticConstructor()
    {
        // arrange — the type declares a static constructor (filtered out by `!c.IsStatic`) in
        // addition to a public instance constructor that fully matches, so the static
        // constructor must not interfere with resolving the matching instance constructor.
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithStaticConstructor
            {
                static WithStaticConstructor()
                {
                }

                public WithStaticConstructor(int id)
                {
                    Id = id;
                }

                public int Id { get; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithStaticConstructor");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.Contains(m => m.MemberName == "Id", model.Members);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Build_ConstructorParameterNameMatchesStaticProperty_ReportsInaccessibleSetterDiagnostic()
    {
        // arrange — the constructor parameter name matches a static property, but static
        // members must be excluded from the candidate-property lookup (`!p.IsStatic`), so the
        // constructor cannot be matched and the instance get-only property is reported instead.
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithStaticPropertyNameCollision
            {
                public WithStaticPropertyNameCollision(int id)
                {
                    InstanceId = id;
                }

                public static int Id { get; set; }

                public int InstanceId { get; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithStaticPropertyNameCollision");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "InstanceId", model.Members);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Build_ConstructorParameterNameMatchesIndexer_ReportsInaccessibleSetterDiagnostic()
    {
        // arrange — the constructor parameter name matches the compiler-generated indexer
        // property name ("Item"), but indexers must be excluded from the candidate-property
        // lookup (`!p.IsIndexer`), so the constructor cannot be matched.
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithIndexerNameCollision
            {
                public WithIndexerNameCollision(int item)
                {
                    Value = item;
                }

                public int this[int index] { get => index; set { } }

                public int Value { get; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithIndexerNameCollision");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "Value", model.Members);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Build_ConstructorParameterNameMatchesNonPublicProperty_ReportsInaccessibleSetterDiagnostic()
    {
        // arrange — the constructor parameter name matches an internal property, but
        // non-public members must be excluded from the candidate-property lookup
        // (`DeclaredAccessibility == Public`), so the constructor cannot be matched.
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithNonPublicPropertyNameCollision
            {
                public WithNonPublicPropertyNameCollision(int id)
                {
                    Id = id;
                    InstanceId = id;
                }

                internal int Id { get; set; }

                public int InstanceId { get; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithNonPublicPropertyNameCollision");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.Contains(d => d.Descriptor.Id == "D20SER002", model.Diagnostics);
        Assert.DoesNotContain(m => m.MemberName == "InstanceId", model.Members);
    }
}
