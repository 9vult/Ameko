// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Sockets;
using Holo.Scripting;
using Holo.Scripting.Models;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Shouldly;

namespace Holo.Tests;

public class PackageManagerTests
{
    [Fact]
    public void IsPackageInstalled_True()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.PackagePath(TestScriptPackage), new MockFileData(string.Empty) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var dc = new PackageManager(fileSystem, lg, new HttpClient());

        dc.IsPackageInstalled(TestScriptPackage).ShouldBeTrue();
    }

    [Fact]
    public void IsPackageInstalled_False()
    {
        var fileSystem = new MockFileSystem();
        var lg = NullLogger<PackageManager>.Instance;
        var dc = new PackageManager(fileSystem, lg, new HttpClient());

        dc.IsPackageInstalled(TestScriptPackage).ShouldBeFalse();
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
    public void PackagePath_Script()
    {
        var path = PackageManager.PackagePath(TestScriptPackage);
        path.ShouldEndWith($"{TestScriptPackage.QualifiedName}.cs");
    }

    [Fact]
    public void PackagePath_Library()
    {
        var path = PackageManager.PackagePath(TestLibraryPackage);
        path.ShouldEndWith($"{TestLibraryPackage.QualifiedName}.lib.cs");
    }

    [Fact]
    public void PackagePath_Scriptlet()
    {
        var path = PackageManager.PackagePath(TestScriptletPackage);
        path.ShouldEndWith($"{TestLibraryPackage.QualifiedName}.js");
    }

    [Fact]
    public void SidecarPath_Script()
    {
        var path = PackageManager.SidecarPath(TestScriptPackage);
        path.ShouldEndWith($"{TestScriptPackage.QualifiedName}.json");
    }

    [Fact]
    public void SidecarPath_Library()
    {
        var path = PackageManager.SidecarPath(TestLibraryPackage);
        path.ShouldEndWith($"{TestLibraryPackage.QualifiedName}.lib.json");
    }

    [Fact]
    public void SidecarPath_Scriptlet()
    {
        var path = PackageManager.SidecarPath(TestScriptletPackage);
        path.ShouldEndWith($"{TestLibraryPackage.QualifiedName}.json");
    }

    [Fact]
    public async Task SetUpBaseRepository_Handles_404()
    {
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(new FileSystem(), lg, new HttpClient(mockClient));

        mockClient.When(HttpMethod.Get, dc.BaseRepositoryUrl).Respond(HttpStatusCode.NotFound);

        await dc.SetUpBaseRepository();

        dc.Repositories.Count.ShouldBe(0);
        dc.PackageStore.Count.ShouldBe(0);
    }

    [Fact]
    public async Task SetUpBaseRepository()
    {
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(new FileSystem(), lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        dc.Repositories.Count.ShouldBe(1);
        dc.PackageStore.Count.ShouldBe(1);
    }

    [Fact]
    public async Task SetUpBaseRepository_NoInternetConnection()
    {
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(new FileSystem(), lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Throw(new HttpRequestException("No internet connection", new SocketException()));

        await dc.SetUpBaseRepository();

        dc.Repositories.Count.ShouldBe(0);
        dc.PackageStore.Count.ShouldBe(0);
    }

    [Fact]
    public async Task InstallPackage_NoInternetConnection()
    {
        var fileSystem = new MockFileSystem();
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);
        mockClient
            .When(HttpMethod.Get, ScriptExample1Url)
            .Throw(new HttpRequestException("No internet connection", new SocketException()));

        await dc.SetUpBaseRepository();

        var result = await dc.InstallPackage(
            dc.PackageStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.Failure);
    }

    [Fact]
    public async Task InstallPackage_AlreadyInstalled()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.PackagePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(string.Empty) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        var result = await dc.InstallPackage(
            dc.PackageStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.AlreadyInstalled);
    }

    [Fact]
    public async Task InstallPackage_NoDependencies()
    {
        var fileSystem = new MockFileSystem();
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);
        mockClient
            .When(HttpMethod.Get, ScriptExample1Url)
            .Respond("application/text", ScriptExample1);

        await dc.SetUpBaseRepository();

        var result = await dc.InstallPackage(
            dc.PackageStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.Success);
        fileSystem.FileExists(PackageManager.PackagePath(Script1)).ShouldBeTrue();
    }

    [Fact]
    public async Task UninstallPackage_NotInstalled()
    {
        var fileSystem = new MockFileSystem();
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        var result = dc.UninstallPackage(
            dc.PackageStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.NotInstalled);
    }

    [Fact]
    public async Task UninstallPackage()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.PackagePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(string.Empty) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        var result = dc.UninstallPackage(
            dc.PackageStore.First(m => m.QualifiedName == Script1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.Success);
        fileSystem.FileExists(PackageManager.PackagePath(Script1)).ShouldBeFalse();
    }

    [Fact]
    public async Task UninstallPackage_IsDependent()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.PackagePath(Script2), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script2), new MockFileData(string.Empty) },
                { PackageManager.PackagePath(Lib1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Lib1), new MockFileData(string.Empty) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository2Json);

        await dc.SetUpBaseRepository();

        var result = dc.UninstallPackage(
            dc.PackageStore.First(m => m.QualifiedName == Lib1.QualifiedName)
        );

        result.ShouldBe(InstallationResult.IsRequiredDependency);
        fileSystem.FileExists(PackageManager.PackagePath(Lib1)).ShouldBeTrue();
        fileSystem.FileExists(PackageManager.SidecarPath(Lib1)).ShouldBeTrue();
    }

    [Fact]
    public async Task UninstallPackage_IsDependent_IsUpdate()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.PackagePath(Script2), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script2), new MockFileData(string.Empty) },
                { PackageManager.PackagePath(Lib1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Lib1), new MockFileData(string.Empty) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository2Json);

        await dc.SetUpBaseRepository();

        var result = dc.UninstallPackage(
            dc.PackageStore.First(m => m.QualifiedName == Lib1.QualifiedName),
            true
        );

        result.ShouldBe(InstallationResult.Success);
        fileSystem.FileExists(PackageManager.PackagePath(Lib1)).ShouldBeFalse();
        fileSystem.FileExists(PackageManager.SidecarPath(Lib1)).ShouldBeFalse();
    }

    [Fact]
    public async Task IsPackageUpToDate_True()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.PackagePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(ScriptExample1Json) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        // Check if up to date
        dc.IsPackageUpToDate(Script1).ShouldBeTrue();
    }

    [Fact]
    public async Task IsPackageUpToDate_False()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.PackagePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(ScriptExample1Json) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1JsonUpdated);

        await dc.SetUpBaseRepository();

        // Check if up to date
        dc.IsPackageUpToDate(Script1).ShouldBeFalse();
    }

    [Fact]
    public async Task GetUpdateCandidates_NoCandidates()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.PackagePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(ScriptExample1Json) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

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
                { PackageManager.PackagePath(Script1), new MockFileData(string.Empty) },
                { PackageManager.SidecarPath(Script1), new MockFileData(ScriptExample1Json) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

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
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

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
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

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
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

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
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

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
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(fileSystem, lg, new HttpClient(mockClient));

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

    private static readonly Package TestScriptPackage = new Package
    {
        Type = PackageType.Script,
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

    private static readonly Package TestLibraryPackage = new Package
    {
        Type = PackageType.Library,
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

    private static readonly Package TestScriptletPackage = new Package
    {
        Type = PackageType.Scriptlet,
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

    private static readonly Package Script1 = new Package
    {
        Type = PackageType.Script,
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

    private static readonly Package Script2 = new Package
    {
        Type = PackageType.Script,
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

    private static readonly Package Lib1 = new Package
    {
        Type = PackageType.Library,
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
          "Packages": [
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
          "Packages": [
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
          "Packages": [
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
                    new PackageInfo
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
