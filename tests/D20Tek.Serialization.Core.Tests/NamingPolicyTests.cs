namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class NamingPolicyTests
{
    [TestMethod]
    public void Default_ReturnsNull()
    {
        Assert.IsNull(NamingPolicy.Default);
    }

    [TestMethod]
    public void CamelCase_ReturnsSameInstanceEachTime()
    {
        var first = NamingPolicy.CamelCase;
        var second = NamingPolicy.CamelCase;

        Assert.IsNotNull(first);
        Assert.AreSame(first, second);
    }

    [TestMethod]
    [DataRow("Name", "name")]
    [DataRow("FirstName", "firstName")]
    [DataRow("ID", "id")]
    [DataRow("XMLData", "xmlData")]
    [DataRow("IOStream", "ioStream")]
    [DataRow("A", "a")]
    public void CamelCase_ConvertsPascalCaseNames(string input, string expected)
    {
        Assert.AreEqual(expected, NamingPolicy.CamelCase.ConvertName(input));
    }

    [TestMethod]
    [DataRow("name", "name")]
    [DataRow("firstName", "firstName")]
    [DataRow("a", "a")]
    public void CamelCase_LeavesAlreadyCamelCaseNamesUnchanged(string input, string expected)
    {
        Assert.AreEqual(expected, NamingPolicy.CamelCase.ConvertName(input));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("123")]
    [DataRow("_internal")]
    public void CamelCase_HandlesEdgeCaseNames(string input)
    {
        // Names that do not start with an uppercase letter are returned unchanged.
        Assert.AreEqual(input, NamingPolicy.CamelCase.ConvertName(input));
    }
}
