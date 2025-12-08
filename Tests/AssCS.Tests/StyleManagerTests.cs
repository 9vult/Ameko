// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Tests;

public class StyleManagerTests
{
    [Test]
    public async Task Add()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };

        sm.Add(s1);

        await Assert.That(sm.Styles.Contains(s1)).IsTrue();
    }

    [Test]
    public async Task Add_Duplicate()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };
        var s2 = new Style(2) { Name = "Style1" };
        sm.Add(s1);

        await Assert.That(() => sm.Add(s2)).Throws<ArgumentException>();
    }

    [Test]
    public async Task Remove_Name_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };
        sm.Add(s1);

        await Assert.That(sm.Remove("Style1")).IsTrue();
    }

    [Test]
    public async Task Remove_Id_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1" };
        sm.Add(s1);

        await Assert.That(sm.Remove(1)).IsTrue();
    }

    [Test]
    public async Task Remove_Name_NotExists()
    {
        var sm = new StyleManager();

        await Assert.That(sm.Remove("Style1")).IsFalse();
    }

    [Test]
    public async Task Remove_Id_NotExists()
    {
        var sm = new StyleManager();

        await Assert.That(sm.Remove(1)).IsFalse();
    }

    [Test]
    public async Task AddOrReplace()
    {
        var sm = new StyleManager();
        var s1 = new Style(1) { Name = "Style1", FontFamily = "Arial" };
        sm.Add(s1);

        var s2 = new Style(2) { Name = "Style1", FontFamily = "Comic Sans MS" };
        sm.AddOrReplace(s2);

        await Assert.That(sm.Get("Style1").Id).IsEqualTo(2);
    }

    [Test]
    public async Task Get_Name_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        await Assert.That(sm.Get("Default")).IsEqualTo(s1);
    }

    [Test]
    public async Task Get_Id_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        await Assert.That(sm.Get(1)).IsEqualTo(s1);
    }

    [Test]
    public async Task Get_Name_NotExists()
    {
        var sm = new StyleManager();

        await Assert.That(() => sm.Get("Default")).Throws<ArgumentException>();
    }

    [Test]
    public async Task Get_Id_NotExists()
    {
        var sm = new StyleManager();

        await Assert.That(() => sm.Get(1)).Throws<ArgumentException>();
    }

    [Test]
    public async Task TryGet_Name_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        var result = sm.TryGet("Default", out var style);
        await Assert.That(result).IsTrue();
        await Assert.That(style).IsEqualTo(s1);
    }

    [Test]
    public async Task TryGet_Id_Exists()
    {
        var sm = new StyleManager();
        var s1 = new Style(1);
        sm.Add(s1);

        var result = sm.TryGet(1, out var style);
        await Assert.That(result).IsTrue();
        await Assert.That(style).IsEqualTo(s1);
    }

    [Test]
    public async Task TryGet_Name_NotExists()
    {
        var sm = new StyleManager();

        var result = sm.TryGet("Default", out var style);
        await Assert.That(result).IsFalse();
        await Assert.That(style).IsNull();
    }

    [Test]
    public async Task TryGet_Id_NotExists()
    {
        var sm = new StyleManager();

        var result = sm.TryGet(1, out var style);
        await Assert.That(result).IsFalse();
        await Assert.That(style).IsNull();
    }
}
