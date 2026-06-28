using D20Tek.Serialization.Generation;
using System.Collections;
using System.Collections.Immutable;

namespace D20Tek.Serialization.Generator.Tests;

[TestClass]
public sealed class EquatableArrayTests
{
    private static readonly string[] expected = ["a", "b", "c"];
    private static readonly string[] expectedArray = ["a", "b"];

    [TestMethod]
    public void Empty_HasZeroCount()
    {
        // arrange & act
        var array = EquatableArray<string>.Empty;

        // assert
        Assert.AreEqual(0, array.Count);
    }

    [TestMethod]
    public void Count_ReturnsElementCount()
    {
        // arrange
        var array = Create("a", "b", "c");

        // act
        var count = array.Count;

        // assert
        Assert.AreEqual(3, count);
    }

    [TestMethod]
    public void Count_DefaultStruct_ReturnsZero()
    {
        // arrange
        EquatableArray<string> array = default;

        // act
        var count = array.Count;

        // assert
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void Indexer_ReturnsElementAtIndex()
    {
        // arrange
        var array = Create("a", "b", "c");

        // act
        var value = array[1];

        // assert
        Assert.AreEqual("b", value);
    }

    [TestMethod]
    public void Equals_SameElements_ReturnsTrue()
    {
        // arrange
        var first = Create("a", "b");
        var second = Create("a", "b");

        // act
        var areEqual = first.Equals(second);

        // assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void Equals_DifferentElements_ReturnsFalse()
    {
        // arrange
        var first = Create("a", "b");
        var second = Create("a", "c");

        // act
        var areEqual = first.Equals(second);

        // assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void Equals_DifferentLengths_ReturnsFalse()
    {
        // arrange
        var first = Create("a", "b");
        var second = Create("a");

        // act
        var areEqual = first.Equals(second);

        // assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void Equals_BothDefault_ReturnsTrue()
    {
        // arrange
        EquatableArray<string> first = default;
        EquatableArray<string> second = default;

        // act
        var areEqual = first.Equals(second);

        // assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void Equals_DefaultVersusInitialized_ReturnsFalse()
    {
        // arrange
        EquatableArray<string> first = default;
        var second = Create("a");

        // act
        var areEqual = first.Equals(second);

        // assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void Equals_Object_SameElements_ReturnsTrue()
    {
        // arrange
        var first = Create("a", "b");
        object second = Create("a", "b");

        // act
        var areEqual = first.Equals(second);

        // assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void Equals_Object_DifferentType_ReturnsFalse()
    {
        // arrange
        var first = Create("a", "b");

        // act
        var areEqual = first.Equals("not an array");

        // assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void GetHashCode_SameElements_ReturnsSameHash()
    {
        // arrange
        var first = Create("a", "b");
        var second = Create("a", "b");

        // act & assert
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_DefaultStruct_ReturnsZero()
    {
        // arrange
        EquatableArray<string> array = default;

        // act
        var hash = array.GetHashCode();

        // assert
        Assert.AreEqual(0, hash);
    }

    [TestMethod]
    public void GetEnumerator_IteratesAllElements()
    {
        // arrange
        var array = Create("a", "b", "c");

        // act
        var items = array.ToList();

        // assert
        CollectionAssert.AreEqual(expected, items);
    }

    [TestMethod]
    public void GetEnumerator_DefaultStruct_YieldsNoElements()
    {
        // arrange
        EquatableArray<string> array = default;

        // act
        var items = array.ToList();

        // assert
        Assert.IsEmpty(items);
    }

    [TestMethod]
    public void NonGenericGetEnumerator_IteratesAllElements()
    {
        // arrange
        var array = Create("a", "b");
        var items = new List<string>();

        // act
        var enumerator = ((IEnumerable)array).GetEnumerator();
        while (enumerator.MoveNext())
        {
            items.Add((string)enumerator.Current!);
        }

        // assert
        CollectionAssert.AreEqual(expectedArray, items);
    }

    [TestMethod]
    public void ToEquatableArray_WrapsImmutableArray()
    {
        // arrange
        var source = ImmutableArray.Create(1, 2, 3);

        // act
        var array = source.ToEquatableArray();

        // assert
        Assert.AreEqual(3, array.Count);
        Assert.AreEqual(2, array[1]);
    }

    private static EquatableArray<string> Create(params string[] values) =>
        ImmutableArray.Create(values).ToEquatableArray();
}
