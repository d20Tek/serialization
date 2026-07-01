using D20Tek.Serialization.Binary.Reflection;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
public sealed class ConversionHelperTests
{
    // --- ConvertInteger: sbyte ---

    [TestMethod]
    public void ConvertInteger_SByte_ReturnsBoxedSByte()
    {
        var result = ConversionHelper.ConvertInteger(42L, typeof(sbyte));

        Assert.IsInstanceOfType<sbyte>(result);
        Assert.AreEqual((sbyte)42, result);
    }

    [TestMethod]
    public void ConvertInteger_SByte_NegativeValue()
    {
        var result = ConversionHelper.ConvertInteger(-100L, typeof(sbyte));

        Assert.IsInstanceOfType<sbyte>(result);
        Assert.AreEqual((sbyte)-100, result);
    }

    // --- ConvertInteger: ushort ---

    [TestMethod]
    public void ConvertInteger_UShort_ReturnsBoxedUShort()
    {
        var result = ConversionHelper.ConvertInteger(50000L, typeof(ushort));

        Assert.IsInstanceOfType<ushort>(result);
        Assert.AreEqual((ushort)50000, result);
    }

    // --- ConvertInteger: uint ---

    [TestMethod]
    public void ConvertInteger_UInt_ReturnsBoxedUInt()
    {
        var result = ConversionHelper.ConvertInteger(3000000000L, typeof(uint));

        Assert.IsInstanceOfType<uint>(result);
        Assert.AreEqual(3000000000u, result);
    }

    // --- ConvertInteger: ulong ---

    [TestMethod]
    public void ConvertInteger_ULong_ReturnsBoxedULong()
    {
        var result = ConversionHelper.ConvertInteger(123456789L, typeof(ulong));

        Assert.IsInstanceOfType<ulong>(result);
        Assert.AreEqual(123456789UL, result);
    }

    // --- ConvertFloat: decimal ---

    [TestMethod]
    public void ConvertFloat_Decimal_ReturnsBoxedDecimal()
    {
        var result = ConversionHelper.ConvertFloat(3.14, typeof(decimal));

        Assert.IsInstanceOfType<decimal>(result);
        Assert.AreEqual(3.14m, (decimal)result, 0.0001m);
    }
}
