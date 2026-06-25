namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class SerializerOptionsTests
{
    [TestMethod]
    public void Defaults_AreApplied()
    {
        var options = new TestSerializerOptions();

        Assert.IsNull(options.PropertyNamingPolicy);
        Assert.IsFalse(options.IgnoreNullValues);
        Assert.IsNotNull(options.Converters);
        Assert.IsEmpty(options.Converters);
    }

    [TestMethod]
    public void PropertyNamingPolicy_CanBeAssigned()
    {
        var options = new TestSerializerOptions
        {
            PropertyNamingPolicy = NamingPolicy.CamelCase,
        };

        Assert.AreSame(NamingPolicy.CamelCase, options.PropertyNamingPolicy);
    }

    [TestMethod]
    public void IgnoreNullValues_CanBeAssigned()
    {
        var options = new TestSerializerOptions { IgnoreNullValues = true };

        Assert.IsTrue(options.IgnoreNullValues);
    }

    [TestMethod]
    public void Converters_AreMutableAndPreserveAddedItems()
    {
        var options = new TestSerializerOptions();
        var converter = new TestConverter();

        options.Converters.Add(converter);

        Assert.HasCount(1, options.Converters);
        Assert.AreSame(converter, options.Converters[0]);
    }

    private sealed class TestSerializerOptions : SerializerOptions
    {
    }

    private sealed class TestConverter : Converter
    {
        public override bool CanConvert(Type typeToConvert) => false;
    }
}
