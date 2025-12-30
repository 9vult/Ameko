// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Tests;

public class ColorTests
{
    [Test]
    public async Task FromAss_NoAlpha()
    {
        var c = Color.FromAss("&H00FF33");

        await Assert.That(c.Alpha).IsEqualTo<byte>(0x00);
        await Assert.That(c.Blue).IsEqualTo<byte>(0x00);
        await Assert.That(c.Green).IsEqualTo<byte>(0xFF);
        await Assert.That(c.Red).IsEqualTo<byte>(0x33);
    }

    [Test]
    public async Task FromAss_WithAlpha()
    {
        var c = Color.FromAss("&H4400FF33&");

        await Assert.That(c.Alpha).IsEqualTo<byte>(0x44);
        await Assert.That(c.Blue).IsEqualTo<byte>(0x00);
        await Assert.That(c.Green).IsEqualTo<byte>(0xFF);
        await Assert.That(c.Red).IsEqualTo<byte>(0x33);
    }

    [Test]
    public async Task FromAss_AlphaOnly()
    {
        var c = Color.FromAss("&H44&");

        await Assert.That(c.Alpha).IsEqualTo<byte>(0x44);
        await Assert.That(c.Blue).IsEqualTo<byte>(0x00);
        await Assert.That(c.Green).IsEqualTo<byte>(0x00);
        await Assert.That(c.Red).IsEqualTo<byte>(0x00);
    }

    [Test]
    public async Task FromRgb()
    {
        var c = Color.FromRgb(0, 0xFF, 0x33);

        await Assert.That(c.Alpha).IsEqualTo<byte>(0x00);
        await Assert.That(c.Blue).IsEqualTo<byte>(0x33);
        await Assert.That(c.Green).IsEqualTo<byte>(0xFF);
        await Assert.That(c.Red).IsEqualTo<byte>(0x00);
    }

    [Test]
    public async Task FromRgba()
    {
        var c = Color.FromRgba(0, 0xFF, 0x33, 0x80);

        await Assert.That(c.Alpha).IsEqualTo<byte>(0x80);
        await Assert.That(c.Blue).IsEqualTo<byte>(0x33);
        await Assert.That(c.Green).IsEqualTo<byte>(0xFF);
        await Assert.That(c.Red).IsEqualTo<byte>(0x00);
    }

    [Test]
    public async Task FromHex_Rgb()
    {
        var c = Color.FromHex("#FF00AA");

        await Assert.That(c.Alpha).IsEqualTo<byte>(0x00);
        await Assert.That(c.Blue).IsEqualTo<byte>(0xAA);
        await Assert.That(c.Green).IsEqualTo<byte>(0x00);
        await Assert.That(c.Red).IsEqualTo<byte>(0xFF);
    }

    [Test]
    public async Task FromHex_Rgba()
    {
        var c = Color.FromHex("#FF00AA55");

        await Assert.That(c.Alpha).IsEqualTo<byte>(0x55);
        await Assert.That(c.Blue).IsEqualTo<byte>(0xAA);
        await Assert.That(c.Green).IsEqualTo<byte>(0x00);
        await Assert.That(c.Red).IsEqualTo<byte>(0xFF);
    }

    [Test]
    public async Task FromHtml()
    {
        var c = Color.FromHtml("red");

        await Assert.That(c.Alpha).IsEqualTo<byte>(0x00);
        await Assert.That(c.Blue).IsEqualTo<byte>(0x00);
        await Assert.That(c.Green).IsEqualTo<byte>(0x00);
        await Assert.That(c.Red).IsEqualTo<byte>(0xFF);
    }

    [Test]
    public async Task AsStyleColor_RGB()
    {
        var c = Color.FromRgb(0x44, 0xFF, 0x25);

        await Assert.That(c.AsStyleColor()).IsEqualTo("&H0025FF44");
    }

    [Test]
    public async Task AsStyleColor_RGBA()
    {
        var c = Color.FromRgba(0x44, 0xFF, 0x25, 0x50);

        await Assert.That(c.AsStyleColor()).IsEqualTo("&H5025FF44");
    }

    [Test]
    public async Task AsOverrideColor_RGB()
    {
        var c = Color.FromRgb(0x44, 0xFF, 0x25);

        await Assert.That(c.AsOverrideColor()).IsEqualTo("&H25FF44&");
    }

    [Test]
    public async Task AsOverrideColor_RGBA()
    {
        var c = Color.FromRgba(0x44, 0xFF, 0x25, 0x50);

        await Assert.That(c.AsOverrideColor()).IsEqualTo("&H25FF44&");
    }

    [Test]
    public async Task AsOverrideAlpha()
    {
        var c = Color.FromA(0x44);

        await Assert.That(c.AsOverrideAlpha()).IsEqualTo("&H44&");
    }

    [Test]
    public async Task Contrast()
    {
        var white = Color.FromRgb(255, 255, 255);
        var black = Color.FromRgb(0, 0, 0);

        await Assert.That(Color.Contrast(white, black)).IsBetween(20.9999, 21.0001);
    }

    [Test]
    public async Task Luminance()
    {
        var white = Color.FromRgb(255, 255, 255);
        var black = Color.FromRgb(0, 0, 0);

        await Assert.That(white.Luminance).IsBetween(0.9999, 1.0001);
        await Assert.That(black.Luminance).IsEqualTo(0);
    }

    [Test]
    public async Task Add()
    {
        var color1 = Color.FromRgba(200, 200, 200, 200);
        var color2 = Color.FromRgba(100, 100, 100, 100);

        var result = color1 + color2;

        await Assert.That(result.Alpha).IsEqualTo<byte>(255);
        await Assert.That(result.Blue).IsEqualTo<byte>(255);
        await Assert.That(result.Green).IsEqualTo<byte>(255);
        await Assert.That(result.Red).IsEqualTo<byte>(255);
    }

    [Test]
    public async Task Subtract()
    {
        var color1 = Color.FromRgba(100, 100, 100, 100);
        var color2 = Color.FromRgba(150, 150, 150, 150);

        var result = color1 - color2;

        await Assert.That(result.Alpha).IsEqualTo<byte>(0);
        await Assert.That(result.Blue).IsEqualTo<byte>(0);
        await Assert.That(result.Green).IsEqualTo<byte>(0);
        await Assert.That(result.Red).IsEqualTo<byte>(0);
    }

    [Test]
    public async Task Equals_True()
    {
        var color1 = Color.FromRgba(10, 20, 30, 40);
        var color2 = Color.FromRgba(10, 20, 30, 40);

        await Assert.That(color1.Equals(color2)).IsTrue();
    }

    [Test]
    public async Task Equals_False()
    {
        var color1 = Color.FromRgba(10, 20, 30, 40);
        var color2 = Color.FromRgba(50, 60, 70, 80);

        await Assert.That(color1.Equals(color2)).IsFalse();
    }
}
