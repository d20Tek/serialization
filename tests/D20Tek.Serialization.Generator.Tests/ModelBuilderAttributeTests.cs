using D20Tek.Serialization.Generation;
using D20Tek.Serialization.Generator.Tests.TestSupport;
using static D20Tek.Serialization.Generator.Tests.TestSupport.ModelBuilderTestSources;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class ModelBuilderAttributeTests
{
    [TestMethod]
    public void Build_SerializedNameAttribute_SetsSerializedName()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class Aliased
            {
                [D20Tek.Serialization.NameSerialized("custom_name")]
                public int Id { get; set; }
            }
            """;

        // act
        var member = BuildMember(source, "Sample.Aliased", "Id");

        // assert
        Assert.AreEqual("custom_name", member.SerializedName);
    }

    [TestMethod]
    public void Build_WithoutSerializedNameAttribute_HasNullSerializedName()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "IntValue");

        // assert
        Assert.IsNull(member.SerializedName);
    }

    [TestMethod]
    public void Build_NameSerializedAttribute_WithoutAliases_HasEmptyAliases()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class Aliased
            {
                [D20Tek.Serialization.NameSerialized("custom_name")]
                public int Id { get; set; }
            }
            """;

        // act
        var member = BuildMember(source, "Sample.Aliased", "Id");

        // assert
        Assert.AreEqual(0, member.Aliases.Count);
    }

    [TestMethod]
    public void Build_NameSerializedAttribute_WithAliases_SetsAliases()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class Aliased
            {
                [D20Tek.Serialization.NameSerialized("new_name", "old_name", "older_name")]
                public int Id { get; set; }
            }
            """;

        // act
        var member = BuildMember(source, "Sample.Aliased", "Id");

        // assert
        Assert.AreEqual("new_name", member.SerializedName);
        Assert.AreSequenceEqual(["old_name", "older_name"], [.. member.Aliases]);
    }

    [TestMethod]
    public void Build_WithoutNameSerializedAttribute_HasEmptyAliases()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "IntValue");

        // assert
        Assert.AreEqual(0, member.Aliases.Count);
    }

    [TestMethod]
    public void Build_NameSerializedAttribute_WithExplicitEmptyAliasesArray_HasEmptyAliases()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class Aliased
            {
                [D20Tek.Serialization.NameSerialized("custom_name", new string[0])]
                public int Id { get; set; }
            }
            """;

        // act
        var member = BuildMember(source, "Sample.Aliased", "Id");

        // assert
        Assert.AreEqual("custom_name", member.SerializedName);
        Assert.AreEqual(0, member.Aliases.Count);
    }

    [TestMethod]
    public void Build_NameSerializedAttribute_WithNullAliasesArgument_HasEmptyAliases()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class Aliased
            {
                [D20Tek.Serialization.NameSerialized("custom_name", null)]
                public int Id { get; set; }
            }
            """;

        // act
        var member = BuildMember(source, "Sample.Aliased", "Id");

        // assert
        Assert.AreEqual("custom_name", member.SerializedName);
        Assert.AreEqual(0, member.Aliases.Count);
    }

    [TestMethod]
    public void Build_NameSerializedAttribute_WithOtherAttributesPresent_StillResolvesAliases()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class Aliased
            {
                [D20Tek.Serialization.RequiredSerialized]
                [D20Tek.Serialization.NameSerialized("new_name", "old_name")]
                public int Id { get; set; }
            }
            """;

        // act
        var member = BuildMember(source, "Sample.Aliased", "Id");

        // assert
        Assert.AreEqual("new_name", member.SerializedName);
        Assert.AreSequenceEqual(["old_name"], [.. member.Aliases]);
    }

    [TestMethod]
    public void Build_RequiredSerializedAttribute_MarksMemberRequired()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithRequired
            {
                [D20Tek.Serialization.RequiredSerialized]
                public int Id { get; set; }
            }
            """;

        // act
        var member = BuildMember(source, "Sample.WithRequired", "Id");

        // assert
        Assert.IsTrue(member.IsRequired);
    }

    [TestMethod]
    public void Build_WithoutRequiredAttribute_MemberIsNotRequired()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "IntValue");

        // assert
        Assert.IsFalse(member.IsRequired);
    }

    [TestMethod]
    public void Build_IgnoredMember_IsExcluded()
    {
        // arrange
        const string source = """
            namespace Sample;

            [D20Tek.Serialization.Serializable]
            public sealed class WithIgnored
            {
                public int Id { get; set; }

                [D20Tek.Serialization.IgnoreSerialized]
                public int Secret { get; set; }
            }
            """;
        var symbol = CompilationFactory.GetTypeSymbol(source, "Sample.WithIgnored");

        // act
        var model = ModelBuilder.Build(symbol);

        // assert
        Assert.DoesNotContain(m => m.MemberName == "Secret", model.Members);
    }
}
