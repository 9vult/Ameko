// SPDX-License-Identifier: MPL-2.0

using FluentAssertions;

namespace AssCS.Tests;

public class StyleTests
{
    private const string BasicStyle =
        "Style: Default,Noto Serif,69,&H00FFFFFF,&H00000000,&H0012291F,&HA012291F,-1,0,0,0,100,100,0,0,1,4.4,2.2,2,275,275,60,1";

    [Fact]
    public void FromAss()
    {
        Style s = Style.FromAss(1, BasicStyle);

        s.Name.Should().Be("Default");
        s.FontFamily.Should().Be("Noto Serif");
        s.FontSize.Should().Be(69);
        s.PrimaryColor.Should().Be(Color.FromRgb(0xFF, 0xFF, 0xFF));
        s.SecondaryColor.Should().Be(Color.FromRgb(0x0, 0x0, 0x0));
        s.OutlineColor.Should().Be(Color.FromRgb(0x1F, 0x29, 0x12));
        s.ShadowColor.Should().Be(Color.FromRgba(0x1F, 0x29, 0x12, 0xA0));
        s.IsBold.Should().BeTrue();
        s.IsItalic.Should().BeFalse();
        s.IsUnderline.Should().BeFalse();
        s.IsStrikethrough.Should().BeFalse();
        s.ScaleX.Should().Be(100);
        s.ScaleY.Should().Be(100);
        s.Spacing.Should().Be(0);
        s.Angle.Should().Be(0);
        s.BorderStyle.Should().Be(1);
        s.BorderThickness.Should().Be(4.4d);
        s.ShadowDistance.Should().Be(2.2d);
        s.Alignment.Should().Be(2);
        s.Margins.Should().Be(new Margins(275, 275, 60));
        s.Encoding.Should().Be(1);
    }

    [Fact]
    public void FromAss_Malformed()
    {
        Action action = () => Style.FromAss(1, BasicStyle.Replace(',', 'A'));

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AsAss()
    {
        Style s = Style.FromAss(1, BasicStyle);

        s.AsAss().Should().Be(BasicStyle);
    }

    [Fact]
    public void Clone()
    {
        Style s1 = Style.FromAss(1, BasicStyle);
        Style s2 = s1.Clone();

        s2.Should().Be(s1);
    }

    [Fact]
    public void Equals_True()
    {
        var style1 = new Style(1) { Name = "TestStyle" };
        var style2 = new Style(1) { Name = "TestStyle" };

        style1.Should().Be(style2);
    }

    [Fact]
    public void Equals_False()
    {
        var style1 = new Style(1) { Name = "StyleA" };
        var style2 = new Style(2) { Name = "StyleB" };

        style1.Should().NotBe(style2);
    }
}
