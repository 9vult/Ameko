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
        packageManager.PackageStore.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Package>([])
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
    public void InstalledButtons_ShouldBeDisabled_WhenNoPackagesInstalled(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService
    )
    {
        // Arrange
        packageManager.InstalledPackages.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Package>([])
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
    public async Task InstallCommand_ShouldInstallPackage_AndReloadScripts_WhenSuccessful(
        IFixture fixture,
        Package package
    )
    {
        // Arrange
        var pkgMan = fixture.Freeze<IPackageManager>();
        var scriptService = fixture.Freeze<IScriptService>();

        pkgMan.InstalledPackages.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Package>([package])
        );
        pkgMan.InstallPackage(package).Returns(InstallationResult.Success);
        pkgMan.GetUpdateCandidates().Returns([]);

        var vm = fixture.Create<PkgManWindowViewModel>();
        vm.SelectedStorePackage = package;

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.InstallCommand).Execute();

        // Assert
        await scriptService.Received().Reload(false);
        vm.InstallButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task UninstallCommand_ShouldUninstallPackage_AndReloadScripts_WhenSuccessful(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService,
        Package package
    )
    {
        // Arrange
        packageManager.UninstallPackage(package).Returns(InstallationResult.Success);
        packageManager.InstalledPackages.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Package>([])
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
            SelectedInstalledPackage = package,
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
    public async Task UpdateCommand_ShouldUpdatePackage_AndReloadScripts_WhenSuccessful(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService,
        Package package
    )
    {
        // Arrange
        packageManager.UpdatePackage(package).Returns(InstallationResult.Success);
        packageManager.GetUpdateCandidates().Returns([]);
        packageManager.InstalledPackages.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Package>([package])
        );

        var vm = new PkgManWindowViewModel(
            packageManager,
            scriptService,
            configuration,
            NullLogger<PkgManWindowViewModel>.Instance,
            messageBoxService
        )
        {
            SelectedInstalledPackage = package,
        };

        // Act
        await ((ReactiveCommand<Unit, Unit>)vm.UpdateCommand).Execute();

        // Assert
        await scriptService.Received().Reload(false);
        vm.UpdateButtonEnabled.ShouldBeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateAllCommand_ShouldUpdatePackages_AndReloadScripts_WhenSuccessful(
        [Frozen] IPackageManager packageManager,
        [Frozen] IScriptService scriptService,
        [Frozen] IConfiguration configuration,
        [Frozen] IMessageBoxService messageBoxService,
        Package package1,
        Package package2
    )
    {
        // Arrange
        packageManager.UpdatePackage(package1).Returns(InstallationResult.Success);
        packageManager.UpdatePackage(package2).Returns(InstallationResult.Success);
        packageManager.GetUpdateCandidates().Returns(x => [package1, package2], x => []);
        packageManager.InstalledPackages.Returns(
            new AssCS.Utilities.ReadOnlyObservableCollection<Package>([package1, package2])
        );

        var vm = new PkgManWindowViewModel(
            packageManager,
            scriptService,
            configuration,
            NullLogger<PkgManWindowViewModel>.Instance,
            messageBoxService
        )
        {
            SelectedInstalledPackage = package1,
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
