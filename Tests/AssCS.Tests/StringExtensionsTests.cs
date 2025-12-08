// SPDX-License-Identifier: MPL-2.0

using AssCS.Utilities;

namespace AssCS.Tests;

public class StringExtensionsTests
{
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
}
