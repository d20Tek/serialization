using D20Tek.Serialization.Binary.Cbor;
using D20Tek.Serialization.Dom;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests.Cbor;

[TestClass]
public sealed class CborNodeParserErrorTests
{
    // --- Indefinite-length rejection ---

    [TestMethod]
    public void Parse_IndefiniteLengthMap_Throws()
    {
        // arrange — raw CBOR: 0xBF (indefinite-length map start) + 0xFF (break)
        byte[] bytes = [0xBF, 0xFF];

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>(
            [ExcludeFromCodeCoverage] () => CborNodeParser.Parse(bytes));
    }

    [TestMethod]
    public void Parse_IndefiniteLengthArray_Throws()
    {
        // arrange — raw CBOR: 0x9F (indefinite-length array start) + 0xFF (break)
        byte[] bytes = [0x9F, 0xFF];

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>(
            [ExcludeFromCodeCoverage] () => CborNodeParser.Parse(bytes));
    }

    // --- SinglePrecisionFloat (raw CBOR) ---

    [TestMethod]
    public void Parse_SinglePrecisionFloat_RawCborBytes_ReturnsNumberNode()
    {
        // arrange — raw CBOR: 0xFA (single-precision float) + big-endian IEEE 754 for 3.14f
        byte[] bytes = [0xFA, 0x40, 0x48, 0xF5, 0xC3];

        // act
        var node = CborNodeParser.Parse(bytes);

        // assert
        Assert.AreEqual(NodeKind.Number, node.Kind);
        Assert.AreEqual(3.14f, (float)node.GetDouble(), 0.001f);
    }

    // --- Unexpected state rejection ---

    [TestMethod]
    public void Parse_UndefinedSimpleValue_ThrowsForUnexpectedState()
    {
        // arrange — raw CBOR: 0xF7 (undefined simple value, not handled in ReadNode switch)
        byte[] bytes = [0xF7];

        // act & assert
        Assert.ThrowsExactly<InvalidOperationException>(
            [ExcludeFromCodeCoverage] () => CborNodeParser.Parse(bytes));
    }
}
