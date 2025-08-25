// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using Holo.Configuration.Keybinds;
using Holo.IO;
using Shouldly;

namespace Holo.Tests;

public class KeybindRegistrarTests
{
    [Fact]
    public void Constructor()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        k.GetKeybinds(KeybindContext.Global).Count().ShouldBe(0);
    }

    [Fact]
    public void RegisterKeybind_NotExists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.RegisterKeybind(bind);

        result.ShouldBeTrue();
        k.GetKeybinds(KeybindContext.Grid).Count().ShouldBe(1);
    }

    [Fact]
    public void RegisterKeybind_Exists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);
        var result = k.RegisterKeybind(bind);

        result.ShouldBeFalse();
        k.GetKeybinds(KeybindContext.Grid).Count().ShouldBe(1);
    }

    [Fact]
    public void DeregisterKeybind_NotExists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.DeregisterKeybind(bind.QualifiedName);

        result.ShouldBeFalse();
        k.GetKeybinds(KeybindContext.Grid).ShouldBeEmpty();
    }

    [Fact]
    public void DeregisterKeybind_Exists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);
        var result = k.DeregisterKeybind(bind.QualifiedName);

        result.ShouldBeTrue();
        k.GetKeybinds(KeybindContext.Grid).ShouldBeEmpty();
    }

    [Fact]
    public void RegisterKeybinds()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind1 = new Keybind("test.bind1", "Ctrl+A", KeybindContext.Grid);
        var bind2 = new Keybind("test.bind2", "Ctrl+D", KeybindContext.Audio);

        var result = k.RegisterKeybinds([bind1, bind2], false);

        result.ShouldBeTrue();
        k.GetKeybinds(KeybindContext.Grid).Count().ShouldBe(1);
        k.GetKeybinds(KeybindContext.Audio).Count().ShouldBe(1);
    }

    [Fact]
    public void ApplyOverride_Key_Exists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);
        var result = k.ApplyOverride(bind.QualifiedName, keybind: "Ctrl+J");

        result.ShouldBeTrue();
        k.GetKeybinds(KeybindContext.Grid).First().Key.ShouldBe("Ctrl+J");
    }

    [Fact]
    public void ApplyOverride_NotExists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.ApplyOverride(bind.QualifiedName, keybind: "Ctrl+J");

        result.ShouldBeFalse();
    }

    [Fact]
    public void ClearOverride_Exists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);
        k.ApplyOverride(bind.QualifiedName, "Ctrl+J");
        var result = k.ClearOverride(bind.QualifiedName);

        result.ShouldBeTrue();
        k.GetKeybinds(KeybindContext.Grid).First().OverrideKey.ShouldBeNull();
    }

    [Fact]
    public void ClearOverride_NotExists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.ClearOverride(bind.QualifiedName);

        result.ShouldBeFalse();
    }

    [Fact]
    public void GetKeybind_Exists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);

        var result = k.GetKeybind(bind.QualifiedName);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void GetKeybind_NotExists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.GetKeybind(bind.QualifiedName);

        result.ShouldBeNull();
    }

    [Fact]
    public void TryGetKeybind_Exists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);

        var result = k.TryGetKeybind(bind.QualifiedName, out var bind2);

        result.ShouldBeTrue();
        bind2.ShouldNotBeNull();
    }

    [Fact]
    public void TryGetKeybind_NotExists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.TryGetKeybind(bind.QualifiedName, out var bind2);

        result.ShouldBeFalse();
        bind2.ShouldBeNull();
    }

    [Fact]
    public void IsKeybindRegistered_True()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);

        var result = k.IsKeybindRegistered(bind.QualifiedName);

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsKeybindRegistered_False()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.IsKeybindRegistered(bind.QualifiedName);

        result.ShouldBeFalse();
    }

    [Fact]
    public void GetOverridenKeybinds()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        var bind1 = new Keybind("test.bind1", "Ctrl+A", KeybindContext.Grid);
        var bind2 = new Keybind("test.bind2", "Ctrl+D", KeybindContext.Audio);

        k.RegisterKeybinds([bind1, bind2], false);
        k.ApplyOverride(bind2.QualifiedName, "T");

        var result = k.GetOverridenKeybinds().ToList();

        result.ShouldNotBeEmpty();
        result.First().ShouldBe(bind2);
    }

    [Fact]
    public void Parse_NotExists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);
        k.Parse();

        k.GetKeybinds(KeybindContext.Global).ShouldBeEmpty();
    }

    [Fact]
    public void Parse_Exists()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Keybinds.LocalPath, new MockFileData(ExampleKeybinds1) },
            }
        );
        var k = new KeybindRegistrar(fs);
        k.Parse();

        k.GetKeybinds(KeybindContext.Global).ShouldNotBeEmpty();
        k.GetKeybinds(KeybindContext.Global).First().DefaultKey.ShouldBe("Ctrl+O");
    }

    [Fact]
    public void Save_Exists()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Keybinds.LocalPath, new MockFileData(ExampleKeybinds2) },
            }
        );
        var k = new KeybindRegistrar(fs);
        k.Parse();

        k.GetKeybind("ameko.document.open")?.OverrideKey.ShouldNotBeNull();
        k.ClearOverride("ameko.document.open");

        k.Save();
        fs.FileExists(Paths.Keybinds.LocalPath).ShouldBeTrue();
    }

    [Fact]
    public void Save_NotExists()
    {
        var fs = new MockFileSystem();
        var k = new KeybindRegistrar(fs);

        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);
        k.RegisterKeybind(bind);

        k.Save();
        fs.FileExists(Paths.Keybinds.LocalPath).ShouldBeTrue();
    }

    private const string ExampleKeybinds1 = """
        {
            "ameko.document.open": {
                "DefaultKey": "Ctrl\u002BO",
                "OverrideKey": null,
                "DefaultContext": "global",
                "OverrideContext": null
            }
        }
        """;

    private const string ExampleKeybinds2 = """
        {
            "ameko.document.open": {
                "DefaultKey": "Ctrl\u002BO",
                "OverrideKey": "Ctrl\u002BOJ",
                "DefaultContext": "global",
                "OverrideContext": null
            }
        }
        """;
}
