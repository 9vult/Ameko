using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Ameko.Services;
using Ameko.ViewModels.Windows;
using AutoFixture;
using AutoFixture.Xunit2;
using Holo;
using Holo.Scripting;
using Holo.Scripting.Models;
using NSubstitute;
using ReactiveUI;
using Shouldly;

namespace Ameko.ViewModel.Tests;

public class DepCtrlWindowViewModelTests
{
    [Theory, AutoNSubstituteData]
    public void InstallButton_ShouldBeDisabled_WhenStoreIsEmpty(
        [Frozen] IDependencyControl dependencyControl,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService
    )
    {
        // Arrange
        dependencyControl.ModuleStore.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([])
        );

        var vm = new DepCtrlWindowViewModel(
            dependencyControl,
            scriptService,
            configuration,
            messageBoxService
        );

        // Assert
        vm.InstallButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public void InstalledButtons_ShouldBeDisabled_WhenNoModulesInstalled(
        [Frozen] IDependencyControl dependencyControl,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService
    )
    {
        // Arrange
        dependencyControl.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([])
        );
        dependencyControl.GetUpdateCandidates().Returns([]);

        var vm = new DepCtrlWindowViewModel(
            dependencyControl,
            scriptService,
            configuration,
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
        var dependencyControl = fixture.Freeze<IDependencyControl>();
        var scriptService = fixture.Freeze<IScriptService>();

        dependencyControl.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([module])
        );
        dependencyControl.InstallModule(module).Returns(InstallationResult.Success);
        dependencyControl.GetUpdateCandidates().Returns([]);

        var vm = fixture.Create<DepCtrlWindowViewModel>();
        vm.SelectedStoreModule = module;
        vm.ShowMessageBox.RegisterHandler(ctx => ctx.SetOutput(Unit.Default));

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.InstallCommand).Execute();

        // Assert
        await scriptService.Received().Reload(false);
        vm.InstallButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task UninstallCommand_ShouldUninstallModule_AndReloadScripts_WhenSuccessful(
        [Frozen] IDependencyControl dependencyControl,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService,
        Module module
    )
    {
        // Arrange
        dependencyControl.UninstallModule(module).Returns(InstallationResult.Success);
        dependencyControl.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([])
        );
        dependencyControl.GetUpdateCandidates().Returns([]);

        var vm = new DepCtrlWindowViewModel(
            dependencyControl,
            scriptService,
            configuration,
            messageBoxService
        )
        {
            SelectedInstalledModule = module,
        };

        vm.ShowMessageBox.RegisterHandler(ctx => ctx.SetOutput(Unit.Default));

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
        [Frozen] IDependencyControl dependencyControl,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService,
        Module module
    )
    {
        // Arrange
        dependencyControl.UpdateModule(module).Returns(InstallationResult.Success);
        dependencyControl.GetUpdateCandidates().Returns([]);
        dependencyControl.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([module])
        );

        var vm = new DepCtrlWindowViewModel(
            dependencyControl,
            scriptService,
            configuration,
            messageBoxService
        )
        {
            SelectedInstalledModule = module,
        };

        vm.ShowMessageBox.RegisterHandler(ctx => ctx.SetOutput(Unit.Default));

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.UpdateCommand).Execute();

        // Assert
        await scriptService.Received().Reload(false);
        vm.UpdateButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateAllCommand_ShouldUpdateModules_AndReloadScripts_WhenSuccessful(
        [Frozen] IDependencyControl dependencyControl,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService,
        Module module1,
        Module module2
    )
    {
        // Arrange
        dependencyControl.UpdateModule(module1).Returns(InstallationResult.Success);
        dependencyControl.UpdateModule(module2).Returns(InstallationResult.Success);
        dependencyControl.GetUpdateCandidates().Returns(x => [module1, module2], x => []);
        dependencyControl.InstalledModules.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Module>([module1, module2])
        );

        var vm = new DepCtrlWindowViewModel(
            dependencyControl,
            scriptService,
            configuration,
            messageBoxService
        )
        {
            SelectedInstalledModule = module1,
        };

        vm.ShowMessageBox.RegisterHandler(ctx => ctx.SetOutput(Unit.Default));

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.UpdateAllCommand).Execute();

        // Assert
        await scriptService.Received().Reload(false);
        vm.UpdateButtonEnabled.ShouldBeFalse();
        vm.UpdateAllButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task RefreshCommand_ShouldRefresh(
        [Frozen] IDependencyControl dependencyControl,
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
        var vm = new DepCtrlWindowViewModel(
            dependencyControl,
            scriptService,
            configuration,
            messageBoxService
        );

        vm.ShowMessageBox.RegisterHandler(ctx => ctx.SetOutput(Unit.Default));

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.RefreshCommand).Execute();

        // Assert
        await dependencyControl.Received().SetUpBaseRepository();
        await dependencyControl.Received().AddAdditionalRepositories(repoUrls);
    }
}
