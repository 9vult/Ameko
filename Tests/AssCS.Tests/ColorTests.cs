// SPDX-License-Identifier: MPL-2.0

using FluentAssertions;

namespace AssCS.Tests;

public class ColorTests
{
    [Fact]
    public void FromAss_NoAlpha()
    {
        var c = Color.FromAss("&H00FF33");

        c.Alpha.Should().Be(0x00);
        c.Blue.Should().Be(0x00);
        c.Green.Should().Be(0xFF);
        c.Red.Should().Be(0x33);
    }

    [Fact]
    public void FromAss_Alpha()
    {
        var c = Color.FromAss("&H4400FF33&");

        c.Alpha.Should().Be(0x44);
        c.Blue.Should().Be(0x00);
        c.Green.Should().Be(0xFF);
        c.Red.Should().Be(0x33);
    }

    [Fact]
    public void FromAss_Malformed()
    {
        Action action = () => Color.FromAss("&H4400FF533&");

        action
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Color &H4400FF533& is invalid or malformed.");
    }

    [Fact]
    public void AsStyleColor_RGB()
    {
        var c = Color.FromRgb(0x44, 0xFF, 0x25);

        c.AsStyleColor().Should().Be("&H0025FF44");
    }

    [Fact]
    public void AsStyleColor_RGBA()
    {
        var c = Color.FromRgba(0x44, 0xFF, 0x25, 0x50);

        c.AsStyleColor().Should().Be("&H5025FF44");
    }

    [Fact]
    public void AsOverrideColor_RGB()
    {
        var c = Color.FromRgb(0x44, 0xFF, 0x25);

        c.AsOverrideColor().Should().Be("&H25FF44&");
    }

    [Fact]
    public void AsOverrideColor_RGBA()
    {
        var c = Color.FromRgba(0x44, 0xFF, 0x25, 0x50);

        c.AsOverrideColor().Should().Be("&H25FF44&");
    }

    [Fact]
    public void Contrast()
    {
        var white = Color.FromRgb(255, 255, 255);
        var black = Color.FromRgb(0, 0, 0);

        Color.Contrast(white, black).Should().BeInRange(20.9999, 21.0001);
    }

    [Fact]
    public void Luminance()
    {
        var white = Color.FromRgb(255, 255, 255);
        var black = Color.FromRgb(0, 0, 0);

        white.Luminance.Should().BeInRange(0.9999, 1.0001);
        black.Luminance.Should().Be(0);
    }

    [Fact]
    public void Add()
    {
        var color1 = Color.FromRgba(200, 200, 200, 200);
        var color2 = Color.FromRgba(100, 100, 100, 100);

        var result = color1 + color2;

        result.Red.Should().Be(255);
        result.Green.Should().Be(255);
        result.Blue.Should().Be(255);
        result.Alpha.Should().Be(255);
    }

    [Fact]
    public void Subtract()
    {
        var color1 = Color.FromRgba(100, 100, 100, 100);
        var color2 = Color.FromRgba(150, 150, 150, 150);

        var result = color1 - color2;

        result.Red.Should().Be(0);
        result.Green.Should().Be(0);
        result.Blue.Should().Be(0);
        result.Alpha.Should().Be(0);
    }

    [Fact]
    public void Equals_True()
    {
        var color1 = Color.FromRgba(10, 20, 30, 40);
        var color2 = Color.FromRgba(10, 20, 30, 40);

        color1.Should().Be(color2);
    }

    [Fact]
    public void Equals_False()
    {
        var color1 = Color.FromRgba(10, 20, 30, 40);
        var color2 = Color.FromRgba(50, 60, 70, 80);

        color1.Should().NotBe(color2);
    }
}
