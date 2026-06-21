namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class SetupSmokeTests
{
    [TestMethod]
    public void BinaryTestProject_IsConfigured()
    {
        // Validates the test harness (MSTest) is wired up correctly.
#pragma warning disable MSTEST0032 // Assertion condition is always true
        Assert.IsTrue(true);
#pragma warning restore MSTEST0032 // Assertion condition is always true
    }
}
