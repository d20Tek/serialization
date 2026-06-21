using FluentAssertions;

namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class SetupSmokeTests
{
    [TestMethod]
    public void CoreTestProject_IsConfigured()
    {
        // Validates the test harness (MSTest + FluentAssertions) is wired up correctly.
        true.Should().BeTrue();
    }
}
