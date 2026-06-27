namespace D20Tek.Serialization.Core.Tests;

[TestClass]
public sealed class SerializationExceptionTests
{
    [TestMethod]
    public void Constructor_SetsMessageAndPath()
    {
        // arrange
        const string message = "Unexpected token.";
        const string path = "$.user.name";

        // act
        var exception = new SerializationException(message, path);

        // assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreEqual(path, exception.Path);
        Assert.IsNull(exception.Expected);
        Assert.IsNull(exception.Actual);
    }

    [TestMethod]
    public void Constructor_CapturesExpectedAndActualValueKinds()
    {
        // arrange
        const string path = "$.items[3].price";

        // act
        var exception = new SerializationException(
            "Type mismatch.",
            path,
            ValueKind.Number,
            ValueKind.String);

        // assert
        Assert.AreEqual(path, exception.Path);
        Assert.AreEqual(ValueKind.Number, exception.Expected);
        Assert.AreEqual(ValueKind.String, exception.Actual);
    }

    [TestMethod]
    public void Exception_IsThrowableAndCaughtAsException()
    {
        // arrange
        var thrown = new SerializationException("boom", "$");

        // act
        var caught = Assert.ThrowsExactly<SerializationException>(() => throw thrown);

        // assert
        Assert.AreSame(thrown, caught);
    }
}
