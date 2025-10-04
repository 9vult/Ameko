using System.Reactive;
using System.Reactive.Linq;
using Ameko.Services;
using Ameko.ViewModels.Windows;
using AutoFixture;
using AutoFixture.Xunit2;
using Holo.Configuration;
using Holo.Providers;
using Holo.Scripting;
using Holo.Scripting.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ReactiveUI;
using Shouldly;

namespace Ameko.ViewModel.Tests.Windows;

public class PkgManWindowViewModelTests
{
    [Theory, AutoNSubstituteData]
    public void InstallButton_ShouldBeDisabled_WhenStoreIsEmpty(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService
    )
    {
        // Arrange
        packageManager.ModuleStore.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([])
        );

        var vm = new PkgManWindowViewModel(
            packageManager,
            scriptService,
            configuration,
            NullLogger<PkgManWindowViewModel>.Instance,
            messageBoxService
        );

        // Assert
        vm.InstallButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public void InstalledButtons_ShouldBeDisabled_WhenNoModulesInstalled(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService
    )
    {
        // Arrange
        packageManager.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([])
        );
        packageManager.GetUpdateCandidates().Returns([]);

        var vm = new PkgManWindowViewModel(
            packageManager,
            scriptService,
            configuration,
            NullLogger<PkgManWindowViewModel>.Instance,
            messageBoxService
        );

        // Assert
        vm.UninstallButtonEnabled.ShouldBeFalse();
        vm.UpdateButtonEnabled.ShouldBeFalse();
        vm.UpdateAllButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task InstallCommand_ShouldInstallModule_AndReloadScripts_WhenSuccessful(
        IFixture fixture,
        Module module
    )
    {
        // Arrange
        var pkgMan = fixture.Freeze<IPackageManager>();
        var scriptService = fixture.Freeze<IScriptService>();

        pkgMan.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([module])
        );
        pkgMan.InstallModule(module).Returns(InstallationResult.Success);
        pkgMan.GetUpdateCandidates().Returns([]);

        var vm = fixture.Create<PkgManWindowViewModel>();
        vm.SelectedStoreModule = module;

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.InstallCommand).Execute();

        // Assert
        await scriptService.Received().Reload(false);
        vm.InstallButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task UninstallCommand_ShouldUninstallModule_AndReloadScripts_WhenSuccessful(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService,
        Module module
    )
    {
        // Arrange
        packageManager.UninstallModule(module).Returns(InstallationResult.Success);
        packageManager.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([])
        );
        packageManager.GetUpdateCandidates().Returns([]);

        var vm = new PkgManWindowViewModel(
            packageManager,
            scriptService,
            configuration,
            NullLogger<PkgManWindowViewModel>.Instance,
            messageBoxService
        )
        {
            SelectedInstalledModule = module,
        };

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.UninstallCommand).Execute();

        // Assert
        await scriptService.Received().Reload(false);
        vm.UninstallButtonEnabled.ShouldBeFalse();
        vm.UpdateButtonEnabled.ShouldBeFalse();
        vm.UpdateAllButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateCommand_ShouldUpdateModule_AndReloadScripts_WhenSuccessful(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService,
        Module module
    )
    {
        // Arrange
        packageManager.UpdateModule(module).Returns(InstallationResult.Success);
        packageManager.GetUpdateCandidates().Returns([]);
        packageManager.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([module])
        );

        var vm = new PkgManWindowViewModel(
            packageManager,
            scriptService,
            configuration,
            NullLogger<PkgManWindowViewModel>.Instance,
            messageBoxService
        )
        {
            SelectedInstalledModule = module,
        };

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.UpdateCommand).Execute();

        // Assert
        await scriptService.Received().Reload(false);
        vm.UpdateButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateAllCommand_ShouldUpdateModules_AndReloadScripts_WhenSuccessful(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService,
        Module module1,
        Module module2
    )
    {
        // Arrange
        packageManager.UpdateModule(module1).Returns(InstallationResult.Success);
        packageManager.UpdateModule(module2).Returns(InstallationResult.Success);
        packageManager.GetUpdateCandidates().Returns(x => [module1, module2], x => []);
        packageManager.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([module1, module2])
        );

        var vm = new PkgManWindowViewModel(
            packageManager,
            scriptService,
            configuration,
            NullLogger<PkgManWindowViewModel>.Instance,
            messageBoxService
        )
        {
            SelectedInstalledModule = module1,
        };

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.UpdateAllCommand).Execute();

        // Assert
        await scriptService.Received().Reload(false);
        vm.UpdateButtonEnabled.ShouldBeFalse();
        vm.UpdateAllButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task RefreshCommand_ShouldRefresh(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService
    )
    {
        // Arrange
        var repoUrls = new AssCS.Utilities.ReadOnlyObservableCollection<string>(
            ["https://test.com/test.json"]
        );
        configuration.RepositoryUrls.Returns(repoUrls);
        var vm = new PkgManWindowViewModel(
            packageManager,
            scriptService,
            configuration,
            NullLogger<PkgManWindowViewModel>.Instance,
            messageBoxService
        );

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.RefreshCommand).Execute();

        // Assert
        await packageManager.Received().SetUpBaseRepository();
        await packageManager.Received().AddAdditionalRepositories(repoUrls);
    }
}
