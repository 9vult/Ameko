// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using System.Net;
using Holo.Scripting;
using Holo.Scripting.Models;
using RichardSzalay.MockHttp;
using Shouldly;

namespace Holo.Tests;

public class DependencyControlTests
{
    [Fact]
    public void IsModuleInstalled_True()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { DependencyControl.ModulePath("author.test"), new MockFileData(string.Empty) },
            }
        );

        var dc = new DependencyControl(fileSystem, new HttpClient());

        dc.IsModuleInstalled("author.test").ShouldBeTrue();
    }

    [Fact]
    public void IsModuleInstalled_False()
    {
        var fileSystem = new MockFileSystem();

        var dc = new DependencyControl(fileSystem, new HttpClient());

        dc.IsModuleInstalled("author.test").ShouldBeFalse();
    }

    [Fact]
    public void ModulePath()
    {
        const string moduleName = "author.test";
        var path = DependencyControl.ModulePath(moduleName);
        path.ShouldEndWith($"{moduleName}.cs");
    }

    [Fact]
    public async Task SetUpBaseRepository_Handles_404()
    {
        var mockClient = new MockHttpMessageHandler();
        mockClient
            .When(HttpMethod.Get, DependencyControl.BaseRepositoryUrl)
            .Respond(HttpStatusCode.NotFound);
        var dc = new DependencyControl(new FileSystem(), new HttpClient(mockClient));

        await dc.SetUpBaseRepository();

        dc.Repositories.Count.ShouldBe(0);
        dc.ModuleStore.Count.ShouldBe(0);
    }

    [Fact]
    public async Task SetUpBaseRepository()
    {
        var mockClient = new MockHttpMessageHandler();
        mockClient
            .When(HttpMethod.Get, DependencyControl.BaseRepositoryUrl)
            .Respond("application/json", RepositoryJson);
        var dc = new DependencyControl(new FileSystem(), new HttpClient(mockClient));

        await dc.SetUpBaseRepository();

        dc.Repositories.Count.ShouldBe(1);
        dc.ModuleStore.Count.ShouldBe(1);
    }

    [Fact]
    public async Task InstallModule_AlreadyInstalled()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { DependencyControl.ModulePath("9volt.example1"), new MockFileData(string.Empty) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        mockClient
            .When(HttpMethod.Get, DependencyControl.BaseRepositoryUrl)
            .Respond("application/json", RepositoryJson);
        var dc = new DependencyControl(fileSystem, new HttpClient(mockClient));
        await dc.SetUpBaseRepository();

        var result = await dc.InstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == "9volt.example1")
        );

        result.ShouldBe(InstallationResult.AlreadyInstalled);
    }

    [Fact]
    public async Task InstallModule_NoDependencies()
    {
        var fileSystem = new MockFileSystem();

        var mockClient = new MockHttpMessageHandler();
        mockClient
            .When(HttpMethod.Get, DependencyControl.BaseRepositoryUrl)
            .Respond("application/json", RepositoryJson);
        mockClient
            .When(HttpMethod.Get, ScriptExample1Url)
            .Respond("application/text", ScriptExample1);
        var dc = new DependencyControl(fileSystem, new HttpClient(mockClient));
        await dc.SetUpBaseRepository();

        var result = await dc.InstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == "9volt.example1")
        );

        result.ShouldBe(InstallationResult.Success);
        fileSystem.FileExists(DependencyControl.ModulePath("9volt.example1")).ShouldBeTrue();
    }

    [Fact]
    public async Task UninstallModule_NotInstalled()
    {
        var fileSystem = new MockFileSystem();

        var mockClient = new MockHttpMessageHandler();
        mockClient
            .When(HttpMethod.Get, DependencyControl.BaseRepositoryUrl)
            .Respond("application/json", RepositoryJson);
        var dc = new DependencyControl(fileSystem, new HttpClient(mockClient));
        await dc.SetUpBaseRepository();

        var result = dc.UninstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == "9volt.example1")
        );

        result.ShouldBe(InstallationResult.NotInstalled);
    }

    [Fact]
    public async Task UninstallModule()
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { DependencyControl.ModulePath("9volt.example1"), new MockFileData(string.Empty) },
            }
        );

        var mockClient = new MockHttpMessageHandler();
        mockClient
            .When(HttpMethod.Get, DependencyControl.BaseRepositoryUrl)
            .Respond("application/json", RepositoryJson);
        var dc = new DependencyControl(fileSystem, new HttpClient(mockClient));
        await dc.SetUpBaseRepository();

        var result = dc.UninstallModule(
            dc.ModuleStore.First(m => m.QualifiedName == "9volt.example1")
        );

        result.ShouldBe(InstallationResult.Success);
        fileSystem.FileExists(DependencyControl.ModulePath("9volt.example1")).ShouldBeFalse();
    }

    private const string RepositoryJson = """
        {
          "Name": "Ameko Dependency Control Base",
          "Description": "Default Dependency Control repository included with Ameko",
          "Maintainer": "9volt",
          "IsBetaChannel": false,
          "Modules": [
            {
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

    private const string ScriptExample1Url = "https://dc.ameko.moe/scripts/9volt/9volt.example1.cs";
    private const string ScriptExample1 = """
        // SPDX-License-Identifier: MIT
        using AssCS;
        using Holo;
        using Holo.Scripting;
        public class Example1 : HoloScript
        {
            public override ScriptInfo Info { get; init; } = new()
            {
                DisplayName = "Example Script 1",
                QualifiedName = "9volt.example1",
                Description = "An example script for testing purposes",
                Author = "9volt",
                Version = 0.1m,
                Exports = [],
                LogDisplay = LogDisplay.Ephemeral
            };
            public override async Task<ExecutionResult> ExecuteAsync ()
            {
                Logger.Info($"Example1 executed!");
                return ExecutionResult.Success;
            }
        }
        """;
}
