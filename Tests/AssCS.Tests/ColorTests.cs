// SPDX-License-Identifier: MPL-2.0

using Shouldly;

namespace AssCS.Tests;

public class ColorTests
{
    [Fact]
    public void FromAss_NoAlpha()
    {
        var c = Color.FromAss("&H00FF33");

        c.Alpha.ShouldBe<byte>(0x00);
        c.Blue.ShouldBe<byte>(0x00);
        c.Green.ShouldBe<byte>(0xFF);
        c.Red.ShouldBe<byte>(0x33);
    }

    [Fact]
    public void FromAss_Alpha()
    {
        var c = Color.FromAss("&H4400FF33&");

        c.Alpha.ShouldBe<byte>(0x44);
        c.Blue.ShouldBe<byte>(0x00);
        c.Green.ShouldBe<byte>(0xFF);
        c.Red.ShouldBe<byte>(0x33);
    }

    [Fact]
    public void FromAss_Malformed()
    {
        Action action = () => Color.FromAss("&H4400FF533&");

        action
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Color &H4400FF533& is invalid or malformed.");
    }

    [Fact]
    public void FromRgb()
    {
        var c = Color.FromRgb(0, 0xFF, 0x33);
        c.Red.ShouldBe<byte>(0x00);
        c.Green.ShouldBe<byte>(0xFF);
        c.Blue.ShouldBe<byte>(0x33);
        c.Alpha.ShouldBe<byte>(0x00);
    }

    [Fact]
    public void FromRgba()
    {
        var c = Color.FromRgba(0, 0xFF, 0x33, 0x80);
        c.Red.ShouldBe<byte>(0x00);
        c.Green.ShouldBe<byte>(0xFF);
        c.Blue.ShouldBe<byte>(0x33);
        c.Alpha.ShouldBe<byte>(0x80);
    }

    [Fact]
    public void FromHex_Rgb()
    {
        var c = Color.FromHex("#FF00AA");
        c.Red.ShouldBe<byte>(0xFF);
        c.Green.ShouldBe<byte>(0x00);
        c.Blue.ShouldBe<byte>(0xAA);
        c.Alpha.ShouldBe<byte>(0x00);
    }

    [Fact]
    public void FromHex_Rgba()
    {
        var c = Color.FromHex("#FF00AA55");
        c.Red.ShouldBe<byte>(0xFF);
        c.Green.ShouldBe<byte>(0x00);
        c.Blue.ShouldBe<byte>(0xAA);
        c.Alpha.ShouldBe<byte>(0x55);
    }

    [Fact]
    public void FromHtml()
    {
        var c = Color.FromHtml("red");
        c.Red.ShouldBe<byte>(0xFF);
        c.Green.ShouldBe<byte>(0x00);
        c.Blue.ShouldBe<byte>(0x00);
        c.Alpha.ShouldBe<byte>(0x00);
    }

    [Fact]
    public void AsStyleColor_RGB()
    {
        var c = Color.FromRgb(0x44, 0xFF, 0x25);

        c.AsStyleColor().ShouldBe("&H0025FF44");
    }

    [Fact]
    public void AsStyleColor_RGBA()
    {
        var c = Color.FromRgba(0x44, 0xFF, 0x25, 0x50);

        c.AsStyleColor().ShouldBe("&H5025FF44");
    }

    [Fact]
    public void AsOverrideColor_RGB()
    {
        var c = Color.FromRgb(0x44, 0xFF, 0x25);

        c.AsOverrideColor().ShouldBe("&H25FF44&");
    }

    [Fact]
    public void AsOverrideColor_RGBA()
    {
        var c = Color.FromRgba(0x44, 0xFF, 0x25, 0x50);

        c.AsOverrideColor().ShouldBe("&H25FF44&");
    }

    [Fact]
    public void Contrast()
    {
        var white = Color.FromRgb(255, 255, 255);
        var black = Color.FromRgb(0, 0, 0);

        Color.Contrast(white, black).ShouldBeInRange(20.9999, 21.0001);
    }

    [Fact]
    public void Luminance()
    {
        var white = Color.FromRgb(255, 255, 255);
        var black = Color.FromRgb(0, 0, 0);

        white.Luminance.ShouldBeInRange(0.9999, 1.0001);
        black.Luminance.ShouldBe(0);
    }

    [Fact]
    public void Add()
    {
        var color1 = Color.FromRgba(200, 200, 200, 200);
        var color2 = Color.FromRgba(100, 100, 100, 100);

        var result = color1 + color2;

        result.Red.ShouldBe<byte>(255);
        result.Green.ShouldBe<byte>(255);
        result.Blue.ShouldBe<byte>(255);
        result.Alpha.ShouldBe<byte>(255);
    }

    [Fact]
    public void Subtract()
    {
        var color1 = Color.FromRgba(100, 100, 100, 100);
        var color2 = Color.FromRgba(150, 150, 150, 150);

        var result = color1 - color2;

        result.Red.ShouldBe<byte>(0);
        result.Green.ShouldBe<byte>(0);
        result.Blue.ShouldBe<byte>(0);
        result.Alpha.ShouldBe<byte>(0);
    }

    [Fact]
    public void Equals_True()
    {
        var color1 = Color.FromRgba(10, 20, 30, 40);
        var color2 = Color.FromRgba(10, 20, 30, 40);

        color1.ShouldBe(color2);
    }

    [Fact]
    public void Equals_False()
    {
        var color1 = Color.FromRgba(10, 20, 30, 40);
        var color2 = Color.FromRgba(50, 60, 70, 80);

        color1.ShouldNotBe(color2);
    }
}
