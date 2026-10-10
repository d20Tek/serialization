using D20Tek.Serialization.Generation;
using static D20Tek.Serialization.Generator.Tests.TestSupport.ModelBuilderTestSources;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class ModelBuilderMemberClassificationTests
{
    [TestMethod]
    public void Build_BooleanProperty_ClassifiesAsBoolean()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "Flag");

        // assert
        Assert.AreEqual(MemberStrategy.Boolean, member.Strategy);
    }

    [TestMethod]
    [DataRow("ByteValue")]
    [DataRow("SByteValue")]
    [DataRow("ShortValue")]
    [DataRow("UShortValue")]
    [DataRow("IntValue")]
    [DataRow("UIntValue")]
    [DataRow("LongValue")]
    [DataRow("ULongValue")]
    public void Build_IntegralProperty_ClassifiesAsInt64(string memberName)
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", memberName);

        // assert
        Assert.AreEqual(MemberStrategy.Int64, member.Strategy);
    }

    [TestMethod]
    [DataRow("FloatValue")]
    [DataRow("DoubleValue")]
    public void Build_FloatingPointProperty_ClassifiesAsDouble(string memberName)
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", memberName);

        // assert
        Assert.AreEqual(MemberStrategy.Double, member.Strategy);
    }

    [TestMethod]
    public void Build_StringProperty_ClassifiesAsString()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "StringValue");

        // assert
        Assert.AreEqual(MemberStrategy.String, member.Strategy);
    }

    [TestMethod]
    public void Build_EnumProperty_ClassifiesAsEnum()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "EnumValue");

        // assert
        Assert.AreEqual(MemberStrategy.Enum, member.Strategy);
    }

    [TestMethod]
    public void Build_NullableValueType_IsNullableAndClassifiesUnderlying()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "NullableInt");

        // assert
        Assert.AreEqual(MemberStrategy.Int64, member.Strategy);
        Assert.IsTrue(member.IsNullable);
    }

    [TestMethod]
    public void Build_NullableEnum_IsNullableAndClassifiesAsEnum()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "NullableEnum");

        // assert
        Assert.AreEqual(MemberStrategy.Enum, member.Strategy);
        Assert.IsTrue(member.IsNullable);
    }

    [TestMethod]
    public void Build_NullableReferenceType_IsNullable()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "NullableString");

        // assert
        Assert.AreEqual(MemberStrategy.String, member.Strategy);
        Assert.IsTrue(member.IsNullable);
    }

    [TestMethod]
    public void Build_NonNullableReferenceType_IsNotNullable()
    {
        // arrange & act
        var member = BuildMember(AllTypesSource, "Sample.AllTypes", "StringValue");

        // assert
        Assert.IsFalse(member.IsNullable);
    }
}
