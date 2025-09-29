// SPDX-License-Identifier: MPL-2.0

using AssCS.Utilities;
using Shouldly;

namespace AssCS.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void ParseAssDouble_Empty()
    {
        string.Empty.ParseAssDouble().ShouldBe(0);
    }

    [Fact]
    public void ParseAssDouble_Invalid()
    {
        "a".ParseAssDouble().ShouldBe(0);
    }

    [Fact]
    public void ParseAssDouble_Decimal()
    {
        "123.45".ParseAssDouble().ShouldBe(123.45d);
    }

    [Fact]
    public void ParseAssDouble_Decimal_Positive()
    {
        "+123.45".ParseAssDouble().ShouldBe(123.45d);
    }

    [Fact]
    public void ParseAssDouble_Decimal_Negative()
    {
        "-123.45".ParseAssDouble().ShouldBe(-123.45d);
    }

    [Fact]
    public void ParseAssDouble_Exponential()
    {
        "3e4".ParseAssDouble().ShouldBe(3e4d);
    }

    [Fact]
    public void ParseAssDouble_Exponential_Positive()
    {
        "+3e4".ParseAssDouble().ShouldBe(3e4d);
    }

    [Fact]
    public void ParseAssDouble_Exponential_Negative()
    {
        "-3e4".ParseAssDouble().ShouldBe(-3e4d);
    }

    [Fact]
    public void ParseAssDouble_Hex()
    {
        "0x15".ParseAssDouble().ShouldBe(0x15);
    }

    [Fact]
    public void ParseAssDouble_Hex_Positive()
    {
        "+0x15".ParseAssDouble().ShouldBe(0x15);
    }

    [Fact]
    public void ParseAssDouble_Hex_Negative()
    {
        "-0x15".ParseAssDouble().ShouldBe(-0x15);
    }

    [Fact]
    public void ParseAssInt_Empty()
    {
        string.Empty.ParseAssInt().ShouldBe(0);
    }

    [Fact]
    public void ParseAssInt_Invalid()
    {
        "a".ParseAssInt().ShouldBe(0);
    }

    [Fact]
    public void ParseAssInt_Decimal()
    {
        "123.45".ParseAssInt().ShouldBe(123);
    }

    [Fact]
    public void ParseAssInt_Decimal_Positive()
    {
        "+123.45".ParseAssInt().ShouldBe(123);
    }

    [Fact]
    public void ParseAssInt_Decimal_Negative()
    {
        "-123.45".ParseAssInt().ShouldBe(-123);
    }

    [Fact]
    public void ParseAssInt_Exponential()
    {
        "3e4".ParseAssInt().ShouldBe((int)3e4);
    }

    [Fact]
    public void ParseAssInt_Exponential_Positive()
    {
        "+3e4".ParseAssInt().ShouldBe((int)3e4);
    }

    [Fact]
    public void ParseAssInt_Exponential_Negative()
    {
        "-3e4".ParseAssInt().ShouldBe((int)-3e4);
    }

    [Fact]
    public void ParseAssInt_Hex()
    {
        "0x15".ParseAssInt().ShouldBe(0x15);
    }

    [Fact]
    public void ParseAssInt_Hex_Positive()
    {
        "+0x15".ParseAssInt().ShouldBe(0x15);
    }

    [Fact]
    public void ParseAssInt_Hex_Negative()
    {
        "-0x15".ParseAssInt().ShouldBe(-0x15);
    }
}
