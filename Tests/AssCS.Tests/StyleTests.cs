// SPDX-License-Identifier: MPL-2.0

using Shouldly;

namespace AssCS.Tests;

public class StyleTests
{
    private const string BasicStyle =
        "Style: Default,Noto Serif,69,&H00FFFFFF,&H00000000,&H0012291F,&HA012291F,-1,0,0,0,100,100,0,0,1,4.4,2.2,2,275,275,60,1";

    [Fact]
    public void FromAss()
    {
        Style s = Style.FromAss(1, BasicStyle);

        s.Name.ShouldBe("Default");
        s.FontFamily.ShouldBe("Noto Serif");
        s.FontSize.ShouldBe(69);
        s.PrimaryColor.ShouldBe(Color.FromRgb(0xFF, 0xFF, 0xFF));
        s.SecondaryColor.ShouldBe(Color.FromRgb(0x0, 0x0, 0x0));
        s.OutlineColor.ShouldBe(Color.FromRgb(0x1F, 0x29, 0x12));
        s.ShadowColor.ShouldBe(Color.FromRgba(0x1F, 0x29, 0x12, 0xA0));
        s.IsBold.ShouldBeTrue();
        s.IsItalic.ShouldBeFalse();
        s.IsUnderline.ShouldBeFalse();
        s.IsStrikethrough.ShouldBeFalse();
        s.ScaleX.ShouldBe(100);
        s.ScaleY.ShouldBe(100);
        s.Spacing.ShouldBe(0);
        s.Angle.ShouldBe(0);
        s.BorderStyle.ShouldBe(1);
        s.BorderThickness.ShouldBe(4.4d);
        s.ShadowDistance.ShouldBe(2.2d);
        s.Alignment.ShouldBe(2);
        s.Margins.ShouldBe(new Margins(275, 275, 60));
        s.Encoding.ShouldBe(1);
    }

    [Fact]
    public void FromAss_Malformed()
    {
        Action action = () => Style.FromAss(1, BasicStyle.Replace(',', 'A'));

        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void AsAss()
    {
        Style s = Style.FromAss(1, BasicStyle);

        s.AsAss().ShouldBe(BasicStyle);
    }

    [Fact]
    public void Clone()
    {
        Style s1 = Style.FromAss(1, BasicStyle);
        Style s2 = s1.Clone();

        s2.ShouldBe(s1);
    }

    [Fact]
    public void Equals_True()
    {
        var style1 = new Style(1) { Name = "TestStyle" };
        var style2 = new Style(1) { Name = "TestStyle" };

        style1.ShouldBe(style2);
    }

    [Fact]
    public void Equals_False()
    {
        var style1 = new Style(1) { Name = "StyleA" };
        var style2 = new Style(2) { Name = "StyleB" };

        style1.ShouldNotBe(style2);
    }
}
