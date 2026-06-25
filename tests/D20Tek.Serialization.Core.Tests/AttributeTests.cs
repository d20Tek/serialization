using System.Reflection;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class AttributeTests
{
    [TestMethod]
    public void SerializableAttribute_IsSealed()
    {
        Assert.IsTrue(typeof(SerializableAttribute).IsSealed);
    }

    [TestMethod]
    public void SerializableAttribute_TargetsClassesAndStructs()
    {
        var usage = GetUsage<SerializableAttribute>();

        Assert.AreEqual(AttributeTargets.Class | AttributeTargets.Struct, usage.ValidOn);
    }

    [TestMethod]
    public void SerializedNameAttribute_IsSealed()
    {
        Assert.IsTrue(typeof(SerializedNameAttribute).IsSealed);
    }

    [TestMethod]
    public void SerializedNameAttribute_TargetsPropertiesAndFields()
    {
        var usage = GetUsage<SerializedNameAttribute>();

        Assert.AreEqual(AttributeTargets.Property | AttributeTargets.Field, usage.ValidOn);
    }

    [TestMethod]
    public void SerializedNameAttribute_ExposesNameFromConstructor()
    {
        var attribute = new SerializedNameAttribute("first_name");

        Assert.AreEqual("first_name", attribute.Name);
    }

    [TestMethod]
    public void IgnoreSerializedAttribute_IsSealed()
    {
        Assert.IsTrue(typeof(IgnoreSerializedAttribute).IsSealed);
    }

    [TestMethod]
    public void IgnoreSerializedAttribute_TargetsPropertiesAndFields()
    {
        var usage = GetUsage<IgnoreSerializedAttribute>();

        Assert.AreEqual(AttributeTargets.Property | AttributeTargets.Field, usage.ValidOn);
    }

    [TestMethod]
    public void RequiredSerializedAttribute_IsSealed()
    {
        Assert.IsTrue(typeof(RequiredSerializedAttribute).IsSealed);
    }

    [TestMethod]
    public void RequiredSerializedAttribute_TargetsPropertiesAndFields()
    {
        var usage = GetUsage<RequiredSerializedAttribute>();

        Assert.AreEqual(AttributeTargets.Property | AttributeTargets.Field, usage.ValidOn);
    }

    private static AttributeUsageAttribute GetUsage<TAttribute>()
        where TAttribute : Attribute =>
        typeof(TAttribute).GetCustomAttribute<AttributeUsageAttribute>()
            ?? throw new InvalidOperationException(
                $"{typeof(TAttribute).Name} is missing an AttributeUsage declaration.");
}
