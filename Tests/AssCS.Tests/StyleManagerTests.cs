// SPDX-License-Identifier: MPL-2.0

using Shouldly;

namespace AssCS.Tests;

public class StyleManagerTests
{
    [Fact]
    public void Add()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };

        sm.Add(s1);

        sm.Styles.Contains(s1).ShouldBeTrue();
    }

    [Fact]
    public void Add_Duplicate()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };
        var s2 = new Style(2) { Name = "Style1" };
        sm.Add(s1);
        Action action = () => sm.Add(s2);

        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Remove_Name_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };
        sm.Add(s1);

        sm.Remove("Style1").ShouldBeTrue();
    }

    [Fact]
    public void Remove_Id_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };
        sm.Add(s1);

        sm.Remove(1).ShouldBeTrue();
    }

    [Fact]
    public void Remove_Name_NotExists()
    {
        var sm = new StyleManager();

        sm.Remove("Style1").ShouldBeFalse();
    }

    [Fact]
    public void Remove_Id_NotExists()
    {
        var sm = new StyleManager();

        sm.Remove(1).ShouldBeFalse();
    }

    [Fact]
    public void AddOrReplace()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1", FontFamily = "Arial" };
        sm.Add(s1);

        var s2 = new Style(2) { Name = "Style1", FontFamily = "Comic Sans MS" };
        sm.AddOrReplace(s2);

        sm.Get("Style1").Id.ShouldBe(2);
    }

    [Fact]
    public void Get_Name_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        sm.Get("Default").ShouldBeSameAs(s1);
    }

    [Fact]
    public void Get_Id_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        sm.Get(1).ShouldBeSameAs(s1);
    }

    [Fact]
    public void Get_Name_NotExists()
    {
        var sm = new StyleManager();

        Action action = () => sm.Get("Default");
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Get_Id_NotExists()
    {
        var sm = new StyleManager();

        Action action = () => sm.Get(1);
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void TryGet_Name_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        var result = sm.TryGet("Default", out var style);
        result.ShouldBeTrue();
        style.ShouldBeSameAs(s1);
    }

    [Fact]
    public void TryGet_Id_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        var result = sm.TryGet(1, out var style);
        result.ShouldBeTrue();
        style.ShouldBeSameAs(s1);
    }

    [Fact]
    public void TryGet_Name_NotExists()
    {
        var sm = new StyleManager();

        var result = sm.TryGet("Default", out var style);
        result.ShouldBeFalse();
        style.ShouldBeNull();
    }

    [Fact]
    public void TryGet_Id_NotExists()
    {
        var sm = new StyleManager();

        var result = sm.TryGet(1, out var style);
        result.ShouldBeFalse();
        style.ShouldBeNull();
    }
}
