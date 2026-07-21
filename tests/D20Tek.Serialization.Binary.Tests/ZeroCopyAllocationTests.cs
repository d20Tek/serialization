using D20Tek.Serialization.Binary.Cbor;
using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class ZeroCopyAllocationTests
{
    // --- GetRawStringBytes does not allocate a managed string ---

    [TestMethod]
    public void GetRawStringBytes_DoesNotAllocateManagedString()
    {
        // arrange — encode a text string "hello" in CBOR
        var writer = new CborFormatWriter();
        writer.WriteString("hello");
        var data = writer.Encode();
        var reader = new CborFormatReader(data, BinaryDecodingMode.Lenient);

        // Warm up to avoid first-call allocations
        _ = GC.GetAllocatedBytesForCurrentThread();

        // act — read raw bytes; this should NOT allocate a managed string
        var before = GC.GetAllocatedBytesForCurrentThread();
        var rawBytes = reader.GetRawStringBytes();
        var after = GC.GetAllocatedBytesForCurrentThread();

        // assert — the span contains "hello" UTF-8 bytes and allocation is minimal
        Assert.AreEqual(5, rawBytes.Length);
        Assert.AreEqual((byte)'h', rawBytes[0]);
        Assert.AreEqual((byte)'e', rawBytes[1]);
        Assert.AreEqual((byte)'l', rawBytes[2]);
        Assert.AreEqual((byte)'l', rawBytes[3]);
        Assert.AreEqual((byte)'o', rawBytes[4]);

        // The zero-copy path should allocate far less than a GetString() call would.
        // We allow a small budget for internal bookkeeping but no string allocation.
        var allocated = after - before;
        Assert.IsTrue(allocated < 200,
            $"GetRawStringBytes allocated {allocated} bytes; expected near-zero (no managed string).");
    }

    [TestMethod]
    public void GetString_AllocatesMoreThanGetRawStringBytes()
    {
        // arrange — encode a longer string to make allocation difference measurable
        var longString = new string('x', 1000);
        var writer = new CborFormatWriter();
        writer.WriteString(longString);
        var data = writer.Encode();

        // Warm up
        _ = GC.GetAllocatedBytesForCurrentThread();

        // act — measure GetString allocation
        var reader1 = new CborFormatReader(data, BinaryDecodingMode.Lenient);
        var beforeString = GC.GetAllocatedBytesForCurrentThread();
        _ = reader1.GetString();
        var afterString = GC.GetAllocatedBytesForCurrentThread();
        var stringAllocated = afterString - beforeString;

        // act — measure GetRawStringBytes allocation
        var reader2 = new CborFormatReader(data, BinaryDecodingMode.Lenient);
        var beforeRaw = GC.GetAllocatedBytesForCurrentThread();
        _ = reader2.GetRawStringBytes();
        var afterRaw = GC.GetAllocatedBytesForCurrentThread();
        var rawAllocated = afterRaw - beforeRaw;

        // assert — raw path allocates significantly less than string path
        Assert.IsTrue(rawAllocated < stringAllocated,
            $"GetRawStringBytes ({rawAllocated}B) should allocate less than GetString ({stringAllocated}B).");
    }

    // --- GetRawNumberBytes does not allocate ---

    [TestMethod]
    public void GetRawNumberBytes_DoesNotAllocate()
    {
        // arrange — encode a 64-bit integer in CBOR
        var writer = new CborFormatWriter();
        writer.WriteNumber(1000L);
        var data = writer.Encode();
        var reader = new CborFormatReader(data, BinaryDecodingMode.Lenient);

        // Warm up
        _ = GC.GetAllocatedBytesForCurrentThread();

        // act
        var before = GC.GetAllocatedBytesForCurrentThread();
        var rawBytes = reader.GetRawNumberBytes();
        var after = GC.GetAllocatedBytesForCurrentThread();

        // assert — bytes are non-empty (CBOR integer encoding) and near-zero allocation
        Assert.IsTrue(rawBytes.Length > 0);
        var allocated = after - before;
        Assert.IsTrue(allocated < 200,
            $"GetRawNumberBytes allocated {allocated} bytes; expected near-zero.");
    }

    [TestMethod]
    public void GetRawNumberBytes_Double_DoesNotAllocate()
    {
        // arrange — encode a 64-bit double in CBOR
        var writer = new CborFormatWriter();
        writer.WriteNumber(3.14159);
        var data = writer.Encode();
        var reader = new CborFormatReader(data, BinaryDecodingMode.Lenient);

        // Warm up
        _ = GC.GetAllocatedBytesForCurrentThread();

        // act
        var before = GC.GetAllocatedBytesForCurrentThread();
        var rawBytes = reader.GetRawNumberBytes();
        var after = GC.GetAllocatedBytesForCurrentThread();

        // assert — 9 bytes: 1 header + 8 bytes for double
        Assert.AreEqual(9, rawBytes.Length);
        var allocated = after - before;
        Assert.IsTrue(allocated < 200,
            $"GetRawNumberBytes (double) allocated {allocated} bytes; expected near-zero.");
    }

    // --- GetRawStringBytes returns correct UTF-8 for various strings ---

    [TestMethod]
    [DataRow("")]
    [DataRow("a")]
    [DataRow("IETF")]
    [DataRow("The quick brown fox")]
    public void GetRawStringBytes_ReturnsCorrectUtf8(string input)
    {
        // arrange
        var writer = new CborFormatWriter();
        writer.WriteString(input);
        var data = writer.Encode();
        var reader = new CborFormatReader(data, BinaryDecodingMode.Lenient);

        // act
        var rawBytes = reader.GetRawStringBytes();

        // assert
        var expectedBytes = System.Text.Encoding.UTF8.GetBytes(input);
        Assert.AreEqual(expectedBytes.Length, rawBytes.Length);
        for (var i = 0; i < expectedBytes.Length; i++)
        {
            Assert.AreEqual(expectedBytes[i], rawBytes[i],
                $"Byte mismatch at index {i} for input \"{input}\".");
        }
    }
}
