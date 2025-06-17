// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using Holo.Scripting;
using Holo.Scripting.Models;
using Shouldly;

namespace Holo.Tests;

public class ScriptConfigurationServiceTests
{
    [Fact]
    public void Set_Primitive_NotExists()
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
        var service = new ScriptConfigurationService(fs);

        service.Set(sut1, "test1", 10);

        var result = service.TryGet<int>(sut1, "test1", out var value);

        result.ShouldBeTrue();
        value.ShouldBe(10);
    }

    [Fact]
    public void Set_Primitive_Exists()
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
        var service = new ScriptConfigurationService(fs);
        service.TryGet<int>(sut1, "test1", out var initial);
        initial.ShouldNotBe(10);

        service.Set(sut1, "test1", 10);

        var result = service.TryGet<int>(sut1, "test1", out var value);

        result.ShouldBeTrue();
        value.ShouldBe(10);
    }

    [Fact]
    public void Set_Object_NotExists()
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
        var service = new ScriptConfigurationService(fs);

        service.Set(sut1, "test2", "planes");

        var result = service.TryGet<string>(sut1, "test2", out var value);

        result.ShouldBeTrue();
        value.ShouldBe("planes");
    }

    [Fact]
    public void Set_Object_Exists()
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
        var service = new ScriptConfigurationService(fs);
        service.TryGet<string>(sut1, "test2", out var initial);
        initial.ShouldNotBe("planes");

        service.Set(sut1, "test2", "planes");

        var result = service.TryGet<string>(sut1, "test2", out var value);

        result.ShouldBeTrue();
        value.ShouldBe("planes");
    }

    [Fact]
    public void Get_NoFile()
    {
        var sut1 = new Sut1();
        var fs = new MockFileSystem();
        var service = new ScriptConfigurationService(fs);

        var result = service.TryGet<string>(sut1, "test2", out var value);
        result.ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void Get_Primitive_NotExists()
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
        var service = new ScriptConfigurationService(fs);

        var result = service.TryGet<int>(sut1, "test1", out var value);
        result.ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Fact]
    public void Get_Primitive_Exists()
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
        var service = new ScriptConfigurationService(fs);

        var result = service.TryGet<int>(sut1, "test1", out var value);
        result.ShouldBeTrue();
        value.ShouldBe(14);
    }

    [Fact]
    public void Get_Object_NotExists()
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
        var service = new ScriptConfigurationService(fs);

        var result = service.TryGet<string>(sut1, "test2", out var value);
        result.ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void Get_Object_Exists()
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
        var service = new ScriptConfigurationService(fs);

        var result = service.TryGet<string>(sut1, "test2", out var value);
        result.ShouldBeTrue();
        value.ShouldBe("trains");
    }

    [Fact]
    public void Remove_NotExists()
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
        var service = new ScriptConfigurationService(fs);

        var result = service.Remove(sut1, "test1");
        result.ShouldBeFalse();
    }

    [Fact]
    public void Remove_Exists()
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
        var service = new ScriptConfigurationService(fs);

        var result = service.Remove(sut1, "test1");
        result.ShouldBeTrue();
    }

    [Fact]
    public void Contains_NotExists()
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
        var service = new ScriptConfigurationService(fs);

        var result = service.Contains(sut1, "test1");
        result.ShouldBeFalse();
    }

    [Fact]
    public void Contains_Exists()
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
        var service = new ScriptConfigurationService(fs);

        var result = service.Contains(sut1, "test1");
        result.ShouldBeTrue();
    }

    private const string Config1 = "{}";
    private const string Config2 = """{ "test1": 14, "test2": "trains" }""";

    private class Sut1 : IHoloExecutable
    {
        /// <inheritdoc />
        public ModuleInfo Info =>
            new ModuleInfo { DisplayName = "SUT 1", QualifiedName = "sut1.script" };
    }
}
