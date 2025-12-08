// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using Holo.Scripting;
using Holo.Scripting.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Holo.Tests;

public class ScriptConfigurationServiceTests
{
    [Test]
    public async Task Set_Primitive_NotExists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config1)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        service.Set(sut1, "test1", 10);

        var result = service.TryGet<int>(sut1, "test1", out var value);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo(10);
    }

    [Test]
    public async Task Set_Primitive_Exists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config2)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);
        service.TryGet<int>(sut1, "test1", out var initial);
        await Assert.That(initial).IsNotEqualTo(10);

        service.Set(sut1, "test1", 10);

        var result = service.TryGet<int>(sut1, "test1", out var value);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo(10);
    }

    [Test]
    public async Task Set_Object_NotExists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config1)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        service.Set(sut1, "test2", "planes");

        var result = service.TryGet<string>(sut1, "test2", out var value);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo("planes");
    }

    [Test]
    public async Task Set_Object_Exists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config2)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);
        service.TryGet<string>(sut1, "test2", out var initial);
        await Assert.That(initial).IsNotEqualTo("planes");

        service.Set(sut1, "test2", "planes");

        var result = service.TryGet<string>(sut1, "test2", out var value);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo("planes");
    }

    [Test]
    public async Task Get_NoFile()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem();
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        var result = service.TryGet<string>(sut1, "test2", out var value);
        await Assert.That(result).IsFalse();
        await Assert.That(value).IsNull();
    }

    [Test]
    public async Task Get_Primitive_NotExists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config1)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        var result = service.TryGet<int>(sut1, "test1", out var value);
        await Assert.That(result).IsFalse();
        await Assert.That(value).IsEqualTo(0);
    }

    [Test]
    public async Task Get_Primitive_Exists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config2)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        var result = service.TryGet<int>(sut1, "test1", out var value);
        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo(14);
    }

    [Test]
    public async Task Get_Object_NotExists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config1)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        var result = service.TryGet<string>(sut1, "test2", out var value);
        await Assert.That(result).IsFalse();
        await Assert.That(value).IsNull();
    }

    [Test]
    public async Task Get_Object_Exists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config2)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        var result = service.TryGet<string>(sut1, "test2", out var value);
        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo("trains");
    }

    [Test]
    public async Task Remove_NotExists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config1)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        var result = service.Remove(sut1, "test1");
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task Remove_Exists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config2)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        var result = service.Remove(sut1, "test1");
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Contains_NotExists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config1)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        var result = service.Contains(sut1, "test1");
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task Contains_Exists()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                {
                    ScriptConfigurationService.ConfigPath(sut1.Info.QualifiedName),
                    new MockFileData(Config2)
                },
            }
        );
        var lg = NullLogger<ScriptConfigurationService>.Instance;
        var service = new ScriptConfigurationService(fs, lg);

        var result = service.Contains(sut1, "test1");
        await Assert.That(result).IsTrue();
    }

    private const string Config1 = "{}";
    private const string Config2 = """{ "test1": 14, "test2": "trains" }""";

    private class Sut1 : IHoloExecutable
    {
        /// <inheritdoc />
        public PackageInfo Info =>
            new PackageInfo { DisplayName = "SUT 1", QualifiedName = "sut1.script" };
    }
}
