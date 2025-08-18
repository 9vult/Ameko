// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Sockets;
using Holo.Scripting;
using Holo.Scripting.Models;
using RichardSzalay.MockHttp;
using Shouldly;

namespace Holo.Tests;

public class PackageManagerTests
{
    [Fact]
    public void IsModuleInstalled_True()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.ModulePath(TestScriptModule), new MockFileData(string.Empty) },
            }
        );

        var dc = new PackageManager(fileSystem, new HttpClient());

        dc.IsModuleInstalled(TestScriptModule).ShouldBeTrue();
    }

    [Fact]
    public void IsModuleInstalled_False()
    {
        var fileSystem = new MockFileSystem();

        var dc = new PackageManager(fileSystem, new HttpClient());

        dc.IsModuleInstalled(TestScriptModule).ShouldBeFalse();
    }

    [Fact]
    public void ValidateQualifiedName_Simple()
    {
        PackageManager.ValidateQualifiedName("my.cool.script").ShouldBeTrue();
    }

    [Fact]
    public void ValidateQualifiedName_Complex()
    {
        PackageManager.ValidateQualifiedName("joe19.co_ol.script").ShouldBeTrue();
    }

    [Fact]
    public void ValidateQualifiedName_Invalid()
    {
        PackageManager.ValidateQualifiedName("joe19.co!ol.script").ShouldBeFalse();
    }

    [Fact]
    public void ModulePath_Script()
    {
        var path = PackageManager.ModulePath(TestScriptModule);
        path.ShouldEndWith($"{TestScriptModule.QualifiedName}.cs");
    }

    [Fact]
    public void ModulePath_Library()
    {
        var path = PackageManager.ModulePath(TestLibraryModule);
        path.ShouldEndWith($"{TestLibraryModule.QualifiedName}.lib.cs");
    }

    [Fact]
    public void ModulePath_Scriptlet()
    {
        var path = PackageManager.ModulePath(TestScriptletModule);
        path.ShouldEndWith($"{TestLibraryModule.QualifiedName}.js");
    }

    [Fact]
    public void SidecarPath_Script()
    {
        var path = PackageManager.SidecarPath(TestScriptModule);
        path.ShouldEndWith($"{TestScriptModule.QualifiedName}.json");
    }

    [Fact]
    public void SidecarPath_Library()
    {
        var path = PackageManager.SidecarPath(TestLibraryModule);
        path.ShouldEndWith($"{TestLibraryModule.QualifiedName}.lib.json");
    }

    [Fact]
    public void SidecarPath_Scriptlet()
    {
        var path = PackageManager.SidecarPath(TestScriptletModule);
        path.ShouldEndWith($"{TestLibraryModule.QualifiedName}.json");
    }

    [Fact]
    public async Task SetUpBaseRepository_Handles_404()
    {
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(new FileSystem(), new HttpClient(mockClient));

        mockClient.When(HttpMethod.Get, dc.BaseRepositoryUrl).Respond(HttpStatusCode.NotFound);

        await dc.SetUpBaseRepository();

        dc.Repositories.Count.ShouldBe(0);
        dc.ModuleStore.Count.ShouldBe(0);
    }

    [Fact]
    public async Task SetUpBaseRepository()
    {
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(new FileSystem(), new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        dc.Repositories.Count.ShouldBe(1);
        dc.ModuleStore.Count.ShouldBe(1);
    }

    [Fact]
    public async Task SetUpBaseRepository_NoInternetConnection()
    {
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(new FileSystem(), new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Throw(new HttpRequestException("No internet connection", new SocketException()));

        await dc.SetUpBaseRepository();

        dc.Repositories.Count.ShouldBe(0);
        dc.ModuleStore.Count.ShouldBe(0);
    }

    [Fact]
    public async Task InstallModule_NoInternetConnection()
    {
        var fileSystem = new MockFileSystem();
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);
        mockClient
            .When(HttpMethod.Get, ScriptExample1Url)
            .Throw(new HttpRequestException("No internet connection", new SocketException()));

        await dc.SetUpBaseRepository();

        var result = await dc.InstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.Failure);
    }

    [Fact]
    public async Task InstallModule_AlreadyInstalled()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.ModulePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(string.Empty) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        var result = await dc.InstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.AlreadyInstalled);
    }

    [Fact]
    public async Task InstallModule_NoDependencies()
    {
        var fileSystem = new MockFileSystem();
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);
        mockClient
            .When(HttpMethod.Get, ScriptExample1Url)
            .Respond("application/text", ScriptExample1);

        await dc.SetUpBaseRepository();

        var result = await dc.InstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.Success);
        fileSystem.FileExists(PackageManager.ModulePath(Script1)).ShouldBeTrue();
    }

    [Fact]
    public async Task UninstallModule_NotInstalled()
    {
        var fileSystem = new MockFileSystem();
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        var result = dc.UninstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.NotInstalled);
    }

    [Fact]
    public async Task UninstallModule()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.ModulePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(string.Empty) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        var result = dc.UninstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.Success);
        fileSystem.FileExists(PackageManager.ModulePath(Script1)).ShouldBeFalse();
    }

    [Fact]
    public async Task UninstallModule_IsDependent()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.ModulePath(Script2), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script2), new MockFileData(string.Empty) },
                { PackageManager.ModulePath(Lib1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Lib1), new MockFileData(string.Empty) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository2Json);

        await dc.SetUpBaseRepository();

        var result = dc.UninstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == Lib1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.IsRequiredDependency);
        fileSystem.FileExists(PackageManager.ModulePath(Lib1)).ShouldBeTrue();
        fileSystem.FileExists(PackageManager.SidecarPath(Lib1)).ShouldBeTrue();
    }

    [Fact]
    public async Task UninstallModule_IsDependent_IsUpdate()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.ModulePath(Script2), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script2), new MockFileData(string.Empty) },
                { PackageManager.ModulePath(Lib1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Lib1), new MockFileData(string.Empty) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository2Json);

        await dc.SetUpBaseRepository();

        var result = dc.UninstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == Lib1.QualifiedName),
            true
        );

        result.ShouldBe(InstallationResult.Success);
        fileSystem.FileExists(PackageManager.ModulePath(Lib1)).ShouldBeFalse();
        fileSystem.FileExists(PackageManager.SidecarPath(Lib1)).ShouldBeFalse();
    }

    [Fact]
    public async Task IsModuleUpToDate_True()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.ModulePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(ScriptExample1Json) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        // Check if up to date
        dc.IsModuleUpToDate(Script1).ShouldBeTrue();
    }

    [Fact]
    public async Task IsModuleUpToDate_False()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.ModulePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(ScriptExample1Json) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1JsonUpdated);

        await dc.SetUpBaseRepository();

        // Check if up to date
        dc.IsModuleUpToDate(Script1).ShouldBeFalse();
    }

    [Fact]
    public async Task GetUpdateCandidates_NoCandidates()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.ModulePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(ScriptExample1Json) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        // Check if up to date
        dc.GetUpdateCandidates().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetUpdateCandidates_HasCandidates()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.ModulePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(ScriptExample1Json) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1JsonUpdated);

        await dc.SetUpBaseRepository();

        // Check if up to date
        dc.GetUpdateCandidates().Count.ShouldBe(1);
    }

    [Fact]
    public async Task AddRepository_NoInternetConnection()
    {
        var fileSystem = new MockFileSystem();
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        const string repoUrl = "https://coolrepo.com/pkgman.json";

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);
        mockClient
            .When(HttpMethod.Get, repoUrl)
            .Throw(new HttpRequestException("No internet connection", new SocketException()));

        await dc.SetUpBaseRepository();
        var result = await dc.AddRepository(repoUrl);

        result.ShouldBe(InstallationResult.Failure);
    }

    [Fact]
    public async Task AddRepository_AlreadyInstalled()
    {
        var fileSystem = new MockFileSystem();
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();
        var result = await dc.AddRepository(dc.BaseRepositoryUrl);

        result.ShouldBe(InstallationResult.AlreadyInstalled);
    }

    [Fact]
    public async Task AddRepository()
    {
        var fileSystem = new MockFileSystem();
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        const string repoUrl = "https://coolrepo.com/pkgman.json";

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);
        mockClient.When(HttpMethod.Get, repoUrl).Respond("application/json", Repository2Json);

        await dc.SetUpBaseRepository();
        var result = await dc.AddRepository(repoUrl);

        result.ShouldBe(InstallationResult.Success);
    }

    [Fact]
    public async Task RemoveRepository_NotInstalled()
    {
        var fileSystem = new MockFileSystem();
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();
        var result = dc.RemoveRepository("Jeff's Repository");

        result.ShouldBe(InstallationResult.NotInstalled);
    }

    [Fact]
    public async Task RemoveRepository()
    {
        var fileSystem = new MockFileSystem();
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, new HttpClient(mockClient));

        const string repoUrl = "https://coolrepo.com/pkgman.json";

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);
        mockClient.When(HttpMethod.Get, repoUrl).Respond("application/json", Repository2Json);

        await dc.SetUpBaseRepository();
        await dc.AddRepository(repoUrl);

        var result = dc.RemoveRepository("Ameko Dependency Control Base 2");

        result.ShouldBe(InstallationResult.Success);
    }

    private static readonly Module TestScriptModule = new Module
    {
        Type = ModuleType.Script,
        DisplayName = "Test",
        QualifiedName = "author.test",
        Description = string.Empty,
        Author = "author",
        Version = 0,
        IsBetaChannel = false,
        Dependencies = [],
        Tags = [],
        Url = string.Empty,
    };

    private static readonly Module TestLibraryModule = new Module
    {
        Type = ModuleType.Library,
        DisplayName = "Test",
        QualifiedName = "author.test",
        Description = string.Empty,
        Author = "author",
        Version = 0,
        IsBetaChannel = false,
        Dependencies = [],
        Tags = [],
        Url = string.Empty,
    };

    private static readonly Module TestScriptletModule = new Module
    {
        Type = ModuleType.Scriptlet,
        DisplayName = "Test",
        QualifiedName = "author.test",
        Description = string.Empty,
        Author = "author",
        Version = 0,
        IsBetaChannel = false,
        Dependencies = [],
        Tags = [],
        Url = string.Empty,
    };

    private static readonly Module Script1 = new Module
    {
        Type = ModuleType.Script,
        DisplayName = "Example Script 1",
        QualifiedName = "9volt.example1",
        Description = "An example script for testing purposes",
        Author = "9volt",
        Version = 0.1m,
        IsBetaChannel = false,
        Dependencies = [],
        Tags = ["Example", "Dialogue"],
        Url = "https://dc.ameko.moe/scripts/9volt/9volt.example1.cs",
    };

    private static readonly Module Script2 = new Module
    {
        Type = ModuleType.Script,
        DisplayName = "Example Script 2",
        QualifiedName = "9volt.example2",
        Description = "An example script for testing purposes",
        Author = "9volt",
        Version = 0.1m,
        IsBetaChannel = false,
        Dependencies = [],
        Tags = ["Example", "Dialogue"],
        Url = "https://dc.ameko.moe/scripts/9volt/9volt.example2.cs",
    };

    private static readonly Module Lib1 = new Module
    {
        Type = ModuleType.Library,
        DisplayName = "CalculatorLib",
        QualifiedName = "9volt.calculator",
        Description = "A basic library for testing",
        Author = "9volt",
        Version = 0.1m,
        IsBetaChannel = false,
        Dependencies = [],
        Tags = ["Calculator"],
        Url = "https://dc.ameko.moe/scripts/9volt/9volt.calculator.lib.cs",
    };

    private const string Repository1Json = """
        {
          "Name": "Ameko Dependency Control Base",
          "Description": "Default Dependency Control repository included with Ameko",
          "Maintainer": "9volt",
          "IsBetaChannel": false,
          "Modules": [
            {
              "Type": "Script",
              "DisplayName": "Example Script 1",
              "QualifiedName": "9volt.example1",
              "Description": "An example script for testing purposes",
              "Author": "9volt",
              "Version": 0.1,
              "IsBetaChannel": false,
              "Dependencies": [],
              "Tags": [ "Example", "Dialogue" ],
              "Url": "https://dc.ameko.moe/scripts/9volt/9volt.example1.cs"
            }
          ],
          "Repositories": []
        }
        """;

    private const string Repository1JsonUpdated = """
        {
          "Name": "Ameko Dependency Control Base",
          "Description": "Default Dependency Control repository included with Ameko",
          "Maintainer": "9volt",
          "IsBetaChannel": false,
          "Modules": [
            {
              "Type": "Script",
              "DisplayName": "Example Script 1",
              "QualifiedName": "9volt.example1",
              "Description": "An example script for testing purposes",
              "Author": "9volt",
              "Version": 0.5,
              "IsBetaChannel": false,
              "Dependencies": [],
              "Tags": [ "Example", "Dialogue" ],
              "Url": "https://dc.ameko.moe/scripts/9volt/9volt.example1.cs"
            }
          ],
          "Repositories": []
        }
        """;

    private const string Repository2Json = """
        {
          "Name": "Ameko Dependency Control Base 2",
          "Description": "Default Dependency Control repository included with Ameko",
          "Maintainer": "9volt",
          "IsBetaChannel": false,
          "Modules": [
            {
              "Type": "Script",
              "DisplayName": "Example Script 2",
              "QualifiedName": "9volt.example2",
              "Description": "An example script for testing purposes",
              "Author": "9volt",
              "Version": 0.1,
              "IsBetaChannel": false,
              "Dependencies": [ "9volt.calculator" ],
              "Tags": [ "Example", "Dialogue" ],
              "Url": "https://dc.ameko.moe/scripts/9volt/9volt.example2.cs"
            },
            {
              "Type": "Library",
              "DisplayName": "CalculatorLib",
              "QualifiedName": "9volt.calculator",
              "Description": "A basic library for testing",
              "Author": "9volt",
              "Version": 0.1,
              "IsBetaChannel": false,
              "Dependencies": [],
              "Tags": [ "Calculator" ],
              "Url": "https://dc.ameko.moe/scripts/9volt/9volt.calculator.lib.cs"
            }
          ],
          "Repositories": []
        }
        """;

    private const string ScriptExample1Url = "https://dc.ameko.moe/scripts/9volt/9volt.example1.cs";
    private const string ScriptExample1 = """
        // SPDX-License-Identifier: MIT
        using AssCS;
        using Holo;
        using Holo.Scripting;
        using System.Threading.Tasks;

        public class Example1 : HoloScript
        {
            public override async Task<ExecutionResult> ExecuteAsync ()
            {
                Logger.Info($"Example1 executed!");
                return ExecutionResult.Success;
            }
            
            public Example1()
                : base(
                    new ModuleInfo
                    {
                        DisplayName = "Example Script 1",
                        QualifiedName = "9volt.example1",
                        Exports = [],
                        LogDisplay = LogDisplay.OnError,
                    }
                ) { }
        }
        """;

    private const string ScriptExample1Json = """
        {
          "Type": "Script",
          "DisplayName": "Example Script 1",
          "QualifiedName": "9volt.example1",
          "Description": "An example script for testing purposes",
          "Author": "9volt",
          "Version": 0.1,
          "IsBetaChannel": false,
          "Dependencies": [],
          "Tags": [ "Example", "Dialogue" ],
          "Url": "https://dc.ameko.moe/scripts/9volt/9volt.example1.cs"
        }
        """;
}
