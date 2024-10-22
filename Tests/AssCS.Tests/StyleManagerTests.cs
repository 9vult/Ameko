// SPDX-License-Identifier: MPL-2.0

using FluentAssertions;

namespace AssCS.Tests;

public class StyleManagerTests
{
    [Fact]
    public void Add()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };

        sm.Add(s1);

        sm.Styles.Contains(s1).Should().BeTrue();
    }

    [Fact]
    public void Add_Duplicate()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };
        var s2 = new Style(2) { Name = "Style1" };
        sm.Add(s1);
        Action action = () => sm.Add(s2);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Remove_Name_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };
        sm.Add(s1);

        sm.Remove("Style1").Should().BeTrue();
    }

    [Fact]
    public void Remove_Id_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };
        sm.Add(s1);

        sm.Remove(1).Should().BeTrue();
    }

    [Fact]
    public void Remove_Name_NotExists()
    {
        var sm = new StyleManager();

        sm.Remove("Style1").Should().BeFalse();
    }

    [Fact]
    public void Remove_Id_NotExists()
    {
        var sm = new StyleManager();

        sm.Remove(1).Should().BeFalse();
    }

    [Fact]
    public void AddOrReplace()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1", FontFamily = "Arial" };
        sm.Add(s1);

        var s2 = new Style(2) { Name = "Style1", FontFamily = "Comic Sans MS" };
        sm.AddOrReplace(s2);

        sm.Get("Style1").Id.Should().Be(2);
    }

    [Fact]
    public void Get_Name_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        sm.Get("Default").Should().BeSameAs(s1);
    }

    [Fact]
    public void Get_Id_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        sm.Get(1).Should().BeSameAs(s1);
    }

    [Fact]
    public void Get_Name_NotExists()
    {
        var sm = new StyleManager();

        Action action = () => sm.Get("Default");
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Get_Id_NotExists()
    {
        var sm = new StyleManager();

        Action action = () => sm.Get(1);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryGet_Name_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        var result = sm.TryGet("Default", out var style);
        result.Should().BeTrue();
        style.Should().BeSameAs(s1);
    }

    [Fact]
    public void TryGet_Id_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        var result = sm.TryGet(1, out var style);
        result.Should().BeTrue();
        style.Should().BeSameAs(s1);
    }

    [Fact]
    public void TryGet_Name_NotExists()
    {
        var sm = new StyleManager();

        var result = sm.TryGet("Default", out var style);
        result.Should().BeFalse();
        style.Should().BeNull();
    }

    [Fact]
    public void TryGet_Id_NotExists()
    {
        var sm = new StyleManager();

        var result = sm.TryGet(1, out var style);
        result.Should().BeFalse();
        style.Should().BeNull();
    }
}
