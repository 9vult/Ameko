// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using Holo.Configuration.Keybinds;
using Holo.IO;
using Microsoft.Extensions.Logging.Abstractions;

namespace Holo.Tests;

public class KeybindRegistrarTests
{
    [Test]
    public async Task Constructor()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        await Assert.That(k.GetKeybinds(KeybindContext.Global).Count()).IsEqualTo(0);
    }

    [Test]
    public async Task RegisterKeybind_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.RegisterKeybind(bind);

        await Assert.That(result).IsTrue();
        await Assert.That(k.GetKeybinds(KeybindContext.Grid).Count()).IsEqualTo(1);
    }

    [Test]
    public async Task RegisterKeybind_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);
        var result = k.RegisterKeybind(bind);

        await Assert.That(result).IsFalse();
        await Assert.That(k.GetKeybinds(KeybindContext.Grid).Count()).IsEqualTo(1);
    }

    [Test]
    public async Task DeregisterKeybind_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.DeregisterKeybind(bind.QualifiedName);

        await Assert.That(result).IsFalse();
        await Assert.That(k.GetKeybinds(KeybindContext.Grid)).IsEmpty();
    }

    [Test]
    public async Task DeregisterKeybind_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);
        var result = k.DeregisterKeybind(bind.QualifiedName);

        await Assert.That(result).IsTrue();
        await Assert.That(k.GetKeybinds(KeybindContext.Grid)).IsEmpty();
    }

    [Test]
    public async Task RegisterKeybinds()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind1 = new Keybind("test.bind1", "Ctrl+A", KeybindContext.Grid);
        var bind2 = new Keybind("test.bind2", "Ctrl+D", KeybindContext.Audio);

        var result = k.RegisterKeybinds([bind1, bind2], false);

        await Assert.That(result).IsTrue();
        await Assert.That(k.GetKeybinds(KeybindContext.Grid).Count()).IsEqualTo(1);
        await Assert.That(k.GetKeybinds(KeybindContext.Audio).Count()).IsEqualTo(1);
    }

    [Test]
    public async Task ApplyOverride_Key_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);
        var result = k.ApplyOverride(bind.QualifiedName, keybind: "Ctrl+J", null, null);

        await Assert.That(result).IsTrue();
        await Assert.That(k.GetKeybinds(KeybindContext.Grid).First().Key).IsEqualTo("Ctrl+J");
    }

    [Test]
    public async Task ApplyOverride_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.ApplyOverride(bind.QualifiedName, keybind: "Ctrl+J", null, null);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ClearOverride_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);
        k.ApplyOverride(bind.QualifiedName, "Ctrl+J", null, null);
        var result = k.ClearOverride(bind.QualifiedName);

        await Assert.That(result).IsTrue();
        await Assert.That(k.GetKeybinds(KeybindContext.Grid).First().OverrideKey).IsNull();
    }

    [Test]
    public async Task ClearOverride_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.ClearOverride(bind.QualifiedName);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetKeybind_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);

        var result = k.GetKeybind(bind.QualifiedName);

        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task GetKeybind_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.GetKeybind(bind.QualifiedName);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryGetKeybind_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);

        var result = k.TryGetKeybind(bind.QualifiedName, out var bind2);

        await Assert.That(result).IsTrue();
        await Assert.That(bind2).IsNotNull();
    }

    [Test]
    public async Task TryGetKeybind_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.TryGetKeybind(bind.QualifiedName, out var bind2);

        await Assert.That(result).IsFalse();
        await Assert.That(bind2).IsNull();
    }

    [Test]
    public async Task IsKeybindRegistered_True()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        k.RegisterKeybind(bind);

        var result = k.IsKeybindRegistered(bind.QualifiedName);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsKeybindRegistered_False()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);

        var result = k.IsKeybindRegistered(bind.QualifiedName);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetOverridenKeybinds()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        var bind1 = new Keybind("test.bind1", "Ctrl+A", KeybindContext.Grid);
        var bind2 = new Keybind("test.bind2", "Ctrl+D", KeybindContext.Audio);

        k.RegisterKeybinds([bind1, bind2], false);
        k.ApplyOverride(bind2.QualifiedName, "T", null, null);

        var result = k.GetOverridenKeybinds().ToList();

        await Assert.That(result).IsNotEmpty();
        await Assert.That(result.First()).IsEqualTo(bind2);
    }

    [Test]
    public async Task Parse_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        k.Parse();

        await Assert.That(k.GetKeybinds(KeybindContext.Global)).IsEmpty();
    }

    [Test]
    public async Task Parse_Exists()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Keybinds.LocalPath, new MockFileData(ExampleKeybinds1) },
            }
        );
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        k.Parse();

        await Assert.That(k.GetKeybinds(KeybindContext.Global)).IsNotEmpty();
        await Assert
            .That(k.GetKeybinds(KeybindContext.Global).First().DefaultKey)
            .IsEqualTo("Ctrl+O");
    }

    [Test]
    public async Task Save_Exists()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Keybinds.LocalPath, new MockFileData(ExampleKeybinds2) },
            }
        );
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);
        k.Parse();

        await Assert.That(k.GetKeybind("ameko.document.open")?.OverrideKey).IsNotNull();
        await Assert.That(k.GetKeybind("ameko.document.open")?.OverrideContext).IsNotNull();
        k.ClearOverride("ameko.document.open");

        k.Save();
        await Assert.That(fs.FileExists(Paths.Keybinds.LocalPath)).IsTrue();
    }

    [Test]
    public async Task Save_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<KeybindRegistrar>.Instance;
        var k = new KeybindRegistrar(fs, lg);

        var bind = new Keybind("test.bind", "Ctrl+A", KeybindContext.Grid);
        k.RegisterKeybind(bind);

        k.Save();
        await Assert.That(fs.FileExists(Paths.Keybinds.LocalPath)).IsTrue();
    }

    private const string ExampleKeybinds1 = """
        {
            "ameko.document.open": {
                "DefaultKey": "Ctrl\u002BO",
                "OverrideKey": null,
                "DefaultContext": "global",
                "OverrideContext": null,
                "IsEnabled": true
            }
        }
        """;

    private const string ExampleKeybinds2 = """
        {
            "ameko.document.open": {
                "DefaultKey": "Ctrl\u002BO",
                "OverrideKey": "Ctrl\u002BOJ",
                "DefaultContext": "global",
                "OverrideContext": "grid",
                "IsEnabled": true
            }
        }
        """;
}
