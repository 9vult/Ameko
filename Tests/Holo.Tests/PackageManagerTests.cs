// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Sockets;
using Holo.Scripting;
using Holo.Scripting.Models;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;

namespace Holo.Tests;

public class PackageManagerTests
{
    [Test]
    public async Task IsPackageInstalled_True()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { PackageManager.PackagePath(TestScriptPackage), new MockFileData(string.Empty) },
            }
        );
        var lg = NullLogger<PackageManager>.Instance;
        var dc = new PackageManager(fileSystem, lg, new HttpClient());

        await Assert.That(dc.IsPackageInstalled(TestScriptPackage)).IsTrue();
    }

    [Test]
    public async Task IsPackageInstalled_False()
    {
        var fileSystem = new MockFileSystem();
        var lg = NullLogger<PackageManager>.Instance;
        var dc = new PackageManager(fileSystem, lg, new HttpClient());

        await Assert.That(dc.IsPackageInstalled(TestScriptPackage)).IsFalse();
    }

    [Test]
    public async Task ValidateQualifiedName_Simple()
    {
        await Assert.That(PackageManager.ValidateQualifiedName("my.cool.script")).IsTrue();
    }

    [Test]
    public async Task ValidateQualifiedName_Complex()
    {
        await Assert.That(PackageManager.ValidateQualifiedName("joe19.co_ol.script")).IsTrue();
    }

    [Test]
    public async Task ValidateQualifiedName_Invalid()
    {
        await Assert.That(PackageManager.ValidateQualifiedName("joe19.co!ol.script")).IsFalse();
    }

    [Test]
    public async Task PackagePath_Script()
    {
        var path = PackageManager.PackagePath(TestScriptPackage);
        await Assert.That(path).EndsWith($"{TestScriptPackage.QualifiedName}.cs");
    }

    [Test]
    public async Task PackagePath_Library()
    {
        var path = PackageManager.PackagePath(TestLibraryPackage);
        await Assert.That(path).EndsWith($"{TestLibraryPackage.QualifiedName}.lib.cs");
    }

    [Test]
    public async Task PackagePath_Scriptlet()
    {
        var path = PackageManager.PackagePath(TestScriptletPackage);
        await Assert.That(path).EndsWith($"{TestLibraryPackage.QualifiedName}.js");
    }

    [Test]
    public async Task SidecarPath_Script()
    {
        var path = PackageManager.SidecarPath(TestScriptPackage);
        await Assert.That(path).EndsWith($"{TestScriptPackage.QualifiedName}.json");
    }

    [Test]
    public async Task SidecarPath_Library()
    {
        var path = PackageManager.SidecarPath(TestLibraryPackage);
        await Assert.That(path).EndsWith($"{TestLibraryPackage.QualifiedName}.lib.json");
    }

    [Test]
    public async Task SidecarPath_Scriptlet()
    {
        var path = PackageManager.SidecarPath(TestScriptletPackage);
        await Assert.That(path).EndsWith($"{TestLibraryPackage.QualifiedName}.json");
    }

    [Test]
    public async Task SetUpBaseRepository_Handles_404()
    {
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(new FileSystem(), lg, new HttpClient(mockClient));

        mockClient.When(HttpMethod.Get, dc.BaseRepositoryUrl).Respond(HttpStatusCode.NotFound);

        await dc.SetUpBaseRepository();

        await Assert.That(dc.Repositories.Count).IsEqualTo(0);
        await Assert.That(dc.PackageStore.Count).IsEqualTo(0);
    }

    [Test]
    public async Task SetUpBaseRepository()
    {
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(new FileSystem(), lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Respond("application/json", Repository1Json);

        await dc.SetUpBaseRepository();

        await Assert.That(dc.Repositories.Count).IsEqualTo(1);
        await Assert.That(dc.PackageStore.Count).IsEqualTo(1);
    }

    [Test]
    public async Task SetUpBaseRepository_NoInternetConnection()
    {
        var lg = NullLogger<PackageManager>.Instance;
        var mockClient = new MockHttpMessageHandler();
        var dc = new PackageManager(new FileSystem(), lg, new HttpClient(mockClient));

        mockClient
            .When(HttpMethod.Get, dc.BaseRepositoryUrl)
            .Throw(new HttpRequestException("No internet connection", new SocketException()));

        await dc.SetUpBaseRepository();

        await Assert.That(dc.Repositories.Count).IsEqualTo(0);
        await Assert.That(dc.PackageStore.Count).IsEqualTo(0);
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.Failure);
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.AlreadyInstalled);
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.Success);
        await Assert.That(fileSystem.FileExists(PackageManager.PackagePath(Script1))).IsTrue();
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.NotInstalled);
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.Success);
        await Assert.That(fileSystem.FileExists(PackageManager.PackagePath(Script1))).IsFalse();
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.IsRequiredDependency);
        await Assert.That(fileSystem.FileExists(PackageManager.PackagePath(Lib1))).IsTrue();
        await Assert.That(fileSystem.FileExists(PackageManager.SidecarPath(Lib1))).IsTrue();
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.Success);
        await Assert.That(fileSystem.FileExists(PackageManager.PackagePath(Lib1))).IsFalse();
        await Assert.That(fileSystem.FileExists(PackageManager.SidecarPath(Lib1))).IsFalse();
    }

    [Test]
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
        await Assert.That(dc.IsPackageUpToDate(Script1)).IsTrue();
    }

    [Test]
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
        await Assert.That(dc.IsPackageUpToDate(Script1)).IsFalse();
    }

    [Test]
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
        await Assert.That(dc.GetUpdateCandidates()).IsEmpty();
    }

    [Test]
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
        await Assert.That(dc.GetUpdateCandidates().Count).IsEqualTo(1);
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.Failure);
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.AlreadyInstalled);
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.Success);
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.NotInstalled);
    }

    [Test]
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

        await Assert.That(result).IsEqualTo(InstallationResult.Success);
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
        Changelog = [],
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
        Changelog = [],
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
        Changelog = [],
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
        Changelog = [],
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
        Changelog = [],
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
        Changelog = [],
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
              "Url": "https://dc.ameko.moe/scripts/9volt/9volt.example1.cs",
              "Changelog": []
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
              "Url": "https://dc.ameko.moe/scripts/9volt/9volt.example1.cs",
              "Changelog": []
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
              "Url": "https://dc.ameko.moe/scripts/9volt/9volt.example2.cs",
              "Changelog": []
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
              "Url": "https://dc.ameko.moe/scripts/9volt/9volt.calculator.lib.cs",
              "Changelog": []
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
          "Url": "https://dc.ameko.moe/scripts/9volt/9volt.example1.cs",
          "Changelog": []
        }
        """;
}
