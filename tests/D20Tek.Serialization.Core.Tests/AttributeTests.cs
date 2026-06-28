using D20Tek.Serialization.Generation;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class AttributeTests
{
    [TestMethod]
    public void SerializableAttribute_IsSealed()
    {
        // arrange

        // act
        var isSealed = typeof(SerializableAttribute).IsSealed;

        // assert
        Assert.IsTrue(isSealed);
    }

    [TestMethod]
    public void SerializableAttribute_TargetsClassesAndStructs()
    {
        // arrange

        // act
        var usage = GetUsage<SerializableAttribute>();

        // assert
        Assert.AreEqual(AttributeTargets.Class | AttributeTargets.Struct, usage.ValidOn);
    }

    [TestMethod]
    public void SerializedNameAttribute_IsSealed()
    {
        // arrange

        // act
        var isSealed = typeof(SerializedNameAttribute).IsSealed;

        // assert
        Assert.IsTrue(isSealed);
    }

    [TestMethod]
    public void SerializedNameAttribute_TargetsPropertiesAndFields()
    {
        // arrange

        // act
        var usage = GetUsage<SerializedNameAttribute>();

        // assert
        Assert.AreEqual(AttributeTargets.Property | AttributeTargets.Field, usage.ValidOn);
    }

    [TestMethod]
    public void SerializedNameAttribute_ExposesNameFromConstructor()
    {
        // arrange
        var attribute = new SerializedNameAttribute("first_name");

        // act
        var name = attribute.Name;

        // assert
        Assert.AreEqual("first_name", name);
    }

    [TestMethod]
    public void IgnoreSerializedAttribute_IsSealed()
    {
        // arrange

        // act
        var isSealed = typeof(IgnoreSerializedAttribute).IsSealed;

        // assert
        Assert.IsTrue(isSealed);
    }

    [TestMethod]
    public void IgnoreSerializedAttribute_TargetsPropertiesAndFields()
    {
        // arrange

        // act
        var usage = GetUsage<IgnoreSerializedAttribute>();

        // assert
        Assert.AreEqual(AttributeTargets.Property | AttributeTargets.Field, usage.ValidOn);
    }

    [TestMethod]
    public void RequiredSerializedAttribute_IsSealed()
    {
        // arrange

        // act
        var isSealed = typeof(RequiredSerializedAttribute).IsSealed;

        // assert
        Assert.IsTrue(isSealed);
    }

    [TestMethod]
    public void RequiredSerializedAttribute_TargetsPropertiesAndFields()
    {
        // arrange

        // act
        var usage = GetUsage<RequiredSerializedAttribute>();

        // assert
        Assert.AreEqual(AttributeTargets.Property | AttributeTargets.Field, usage.ValidOn);
    }

    [TestMethod]
    public void GeneratedSerializerRegistryAttribute_IsSealed()
    {
        // arrange

        // act
        var isSealed = typeof(GeneratedSerializerRegistryAttribute).IsSealed;

        // assert
        Assert.IsTrue(isSealed);
    }

    [TestMethod]
    public void GeneratedSerializerRegistryAttribute_TargetsAssembly()
    {
        // arrange

        // act
        var usage = GetUsage<GeneratedSerializerRegistryAttribute>();

        // assert
        Assert.AreEqual(AttributeTargets.Assembly, usage.ValidOn);
    }

    [TestMethod]
    public void GeneratedSerializerRegistryAttribute_DoesNotAllowMultiple()
    {
        // arrange

        // act
        var usage = GetUsage<GeneratedSerializerRegistryAttribute>();

        // assert
        Assert.IsFalse(usage.AllowMultiple);
    }

    [TestMethod]
    public void GeneratedSerializerRegistryAttribute_ExposesRegistryTypeFromConstructor()
    {
        // arrange
        var attribute = new GeneratedSerializerRegistryAttribute(typeof(AttributeTests));

        // act
        var registryType = attribute.RegistryType;

        // assert
        Assert.AreEqual(typeof(AttributeTests), registryType);
    }

    [ExcludeFromCodeCoverage]
    private static AttributeUsageAttribute GetUsage<TAttribute>()
        where TAttribute : Attribute =>
        typeof(TAttribute).GetCustomAttribute<AttributeUsageAttribute>()
            ?? throw new InvalidOperationException(
                $"{typeof(TAttribute).Name} is missing an AttributeUsage declaration.");
}
