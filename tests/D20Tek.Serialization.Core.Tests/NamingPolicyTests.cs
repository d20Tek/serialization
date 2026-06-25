namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class NamingPolicyTests
{
    [TestMethod]
    public void Default_ReturnsNull()
    {
        // arrange

        // act
        var policy = NamingPolicy.Default;

        // assert
        Assert.IsNull(policy);
    }

    [TestMethod]
    public void CamelCase_ReturnsSameInstanceEachTime()
    {
        // arrange

        // act
        var first = NamingPolicy.CamelCase;
        var second = NamingPolicy.CamelCase;

        // assert
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
        // arrange

        // act
        var result = NamingPolicy.CamelCase.ConvertName(input);

        // assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("name", "name")]
    [DataRow("firstName", "firstName")]
    [DataRow("a", "a")]
    public void CamelCase_LeavesAlreadyCamelCaseNamesUnchanged(string input, string expected)
    {
        // arrange

        // act
        var result = NamingPolicy.CamelCase.ConvertName(input);

        // assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("123")]
    [DataRow("_internal")]
    public void CamelCase_HandlesEdgeCaseNames(string input)
    {
        // arrange

        // act
        // Names that do not start with an uppercase letter are returned unchanged.
        var result = NamingPolicy.CamelCase.ConvertName(input);

        // assert
        Assert.AreEqual(input, result);
    }
}
