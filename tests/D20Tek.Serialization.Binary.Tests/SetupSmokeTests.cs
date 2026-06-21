using FluentAssertions;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class SetupSmokeTests
{
    [TestMethod]
    public void BinaryTestProject_IsConfigured()
    {
        // Validates the test harness (MSTest + FluentAssertions) is wired up correctly.
        true.Should().BeTrue();
    }
}
