// SPDX-License-Identifier: MPL-2.0

using AssCS.Utilities;

namespace AssCS.Tests;

public class StringExtensionsTests
{
    #region Double

    [Test]
    public async Task ParseAssDouble_Empty()
    {
        await Assert.That(string.Empty.ParseAssDouble()).IsEqualTo(0);
    }

    [Test]
    public async Task ParseAssDouble_Invalid()
    {
        await Assert.That("a".ParseAssDouble()).IsEqualTo(0);
    }

    [Test]
    public async Task ParseAssDouble_Decimal()
    {
        await Assert.That("123.45".ParseAssDouble()).IsEqualTo(123.45d);
    }

    [Test]
    public async Task ParseAssDouble_Decimal_Positive()
    {
        await Assert.That("+123.45".ParseAssDouble()).IsEqualTo(123.45d);
    }

    [Test]
    public async Task ParseAssDouble_Decimal_Negative()
    {
        await Assert.That("-123.45".ParseAssDouble()).IsEqualTo(-123.45d);
    }

    [Test]
    public async Task ParseAssDouble_Exponential()
    {
        await Assert.That("3e4".ParseAssDouble()).IsEqualTo(3e4d);
    }

    [Test]
    public async Task ParseAssDouble_PositiveExponential()
    {
        await Assert.That("3e+4".ParseAssDouble()).IsEqualTo(3e4d);
    }

    [Test]
    public async Task ParseAssDouble_NegativeExponential()
    {
        await Assert.That("3e-4".ParseAssDouble()).IsEqualTo(3e-4d);
    }

    [Test]
    public async Task ParseAssDouble_Exponential_Positive()
    {
        await Assert.That("+3e4".ParseAssDouble()).IsEqualTo(3e4d);
    }

    [Test]
    public async Task ParseAssDouble_Exponential_Negative()
    {
        await Assert.That("-3e4".ParseAssDouble()).IsEqualTo(-3e4d);
    }

    [Test]
    public async Task ParseAssDouble_PositiveExponential_Positive()
    {
        await Assert.That("+3e+4".ParseAssDouble()).IsEqualTo(3e4d);
    }

    [Test]
    public async Task ParseAssDouble_PositiveExponential_Negative()
    {
        await Assert.That("-3e+4".ParseAssDouble()).IsEqualTo(-3e4d);
    }

    [Test]
    public async Task ParseAssDouble_NegativeExponential_Positive()
    {
        await Assert.That("+3e-4".ParseAssDouble()).IsEqualTo(3e-4d);
    }

    [Test]
    public async Task ParseAssDouble_NegativeExponential_Negative()
    {
        await Assert.That("-3e-4".ParseAssDouble()).IsEqualTo(-3e-4d);
    }

    [Test]
    public async Task ParseAssDouble_Hex()
    {
        await Assert.That("0x15".ParseAssDouble()).IsEqualTo(0x15);
    }

    [Test]
    public async Task ParseAssDouble_Hex_Positive()
    {
        await Assert.That("+0x15".ParseAssDouble()).IsEqualTo(0x15);
    }

    [Test]
    public async Task ParseAssDouble_Hex_Negative()
    {
        await Assert.That("-0x15".ParseAssDouble()).IsEqualTo(-0x15);
    }

    [Test]
    public async Task ParseAssDouble_Whitespace_InvalidText()
    {
        await Assert.That(" \t\v20.39445InvalidText".ParseAssDouble()).IsEqualTo(20.39445);
    }

    [Test]
    public async Task ParseAssDouble_InvalidText_AtStart()
    {
        await Assert.That(@"InvalidText-20".ParseAssDouble()).IsEqualTo(0);
    }

    [Test]
    public async Task ParseAssDouble_MultiplePoints()
    {
        await Assert.That(@"20.39.445.678".ParseAssDouble()).IsEqualTo(20.39);
    }

    [Test]
    public async Task ParseAssDouble_TrailingPoint()
    {
        await Assert.That(@"20.".ParseAssDouble()).IsEqualTo(20);
    }

    [Test]
    public async Task ParseAssDouble_MultipleSigns()
    {
        await Assert.That(@"+-20".ParseAssDouble()).IsEqualTo(0);
    }

    #endregion Double

    #region Integer

    [Test]
    public async Task ParseAssInt_Empty()
    {
        await Assert.That(string.Empty.ParseAssInt()).IsEqualTo(0);
    }

    [Test]
    public async Task ParseAssInt_Invalid()
    {
        await Assert.That("a".ParseAssInt()).IsEqualTo(0);
    }

    [Test]
    public async Task ParseAssInt_Decimal()
    {
        await Assert.That("123.45".ParseAssInt()).IsEqualTo(123);
    }

    [Test]
    public async Task ParseAssInt_Decimal_Positive()
    {
        await Assert.That("+123.45".ParseAssInt()).IsEqualTo(123);
    }

    [Test]
    public async Task ParseAssInt_Decimal_Negative()
    {
        await Assert.That("-123.45".ParseAssInt()).IsEqualTo(-123);
    }

    [Test]
    public async Task ParseAssInt_Exponential()
    {
        await Assert.That("3e4".ParseAssInt()).IsEqualTo((int)3e4);
    }

    [Test]
    public async Task ParseAssInt_PositiveExponential()
    {
        await Assert.That("3e+4".ParseAssInt()).IsEqualTo((int)3e4);
    }

    [Test]
    public async Task ParseAssInt_NegativeExponential()
    {
        await Assert.That("3e-4".ParseAssInt()).IsEqualTo((int)3e-4);
    }

    [Test]
    public async Task ParseAssInt_Exponential_Positive()
    {
        await Assert.That("+3e4".ParseAssInt()).IsEqualTo((int)3e4);
    }

    [Test]
    public async Task ParseAssInt_Exponential_Negative()
    {
        await Assert.That("-3e4".ParseAssInt()).IsEqualTo((int)-3e4);
    }

    [Test]
    public async Task ParseAssInt_PositiveExponential_Positive()
    {
        await Assert.That("+3e+4".ParseAssInt()).IsEqualTo((int)3e4);
    }

    [Test]
    public async Task ParseAssInt_PositiveExponential_Negative()
    {
        await Assert.That("-3e+4".ParseAssInt()).IsEqualTo((int)-3e4);
    }

    [Test]
    public async Task ParseAssInt_NegativeExponential_Positive()
    {
        await Assert.That("+3e-4".ParseAssInt()).IsEqualTo((int)3e-4);
    }

    [Test]
    public async Task ParseAssInt_NegativeExponential_Negative()
    {
        await Assert.That("-3e-4".ParseAssInt()).IsEqualTo((int)-3e-4);
    }

    [Test]
    public async Task ParseAssInt_Hex()
    {
        await Assert.That("0x15".ParseAssInt()).IsEqualTo(0x15);
    }

    [Test]
    public async Task ParseAssInt_Hex_Positive()
    {
        await Assert.That("+0x15".ParseAssInt()).IsEqualTo(0x15);
    }

    [Test]
    public async Task ParseAssInt_Hex_Negative()
    {
        await Assert.That("-0x15".ParseAssInt()).IsEqualTo(-0x15);
    }

    [Test]
    public async Task ParseAssInt_Whitespace_InvalidText()
    {
        await Assert.That(" \t\v203InvalidText".ParseAssInt()).IsEqualTo(203);
    }

    [Test]
    public async Task ParseAssInt_InvalidText_AtStart()
    {
        await Assert.That(@"InvalidText-20".ParseAssInt()).IsEqualTo(0);
    }

    [Test]
    public async Task ParseAssInt_MultiplePoints()
    {
        await Assert.That(@"20.39.445.678".ParseAssInt()).IsEqualTo(20);
    }

    [Test]
    public async Task ParseAssInt_TrailingPoint()
    {
        await Assert.That(@"20.".ParseAssInt()).IsEqualTo(20);
    }

    [Test]
    public async Task ParseAssInt_MultipleSigns()
    {
        await Assert.That(@"+-20".ParseAssInt()).IsEqualTo(0);
    }

    #endregion Integer
}
