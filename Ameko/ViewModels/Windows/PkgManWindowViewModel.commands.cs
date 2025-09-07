// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using Holo.Models;
using Holo.Scripting.Models;
using Material.Icons;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class PkgManWindowViewModel
{
    /// <summary>
    /// Install a module
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateInstallCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedStoreModule is null)
                return;

            var result = await PackageManager.InstallModule(SelectedStoreModule);

            await _messageBoxService.ShowAsync(result);

            if (result == InstallationResult.Success)
            {
                this.RaisePropertyChanged(nameof(InstallButtonEnabled));
                this.RaisePropertyChanged(nameof(UninstallButtonEnabled));

                await _scriptService.Reload(false);
            }
        });
    }

    /// <summary>
    /// Uninstall a module
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateUninstallCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedInstalledModule is null)
                return;

            var result = PackageManager.UninstallModule(SelectedInstalledModule);

            await _messageBoxService.ShowAsync(result);

            if (result == InstallationResult.Success)
            {
                SelectedInstalledModule = null;
                this.RaisePropertyChanged(nameof(InstallButtonEnabled));
                this.RaisePropertyChanged(nameof(UninstallButtonEnabled));

                await _scriptService.Reload(false);
            }
        });
    }

    /// <summary>
    /// Update a module
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateUpdateCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedInstalledModule is null)
                return;

            var result = await PackageManager.UpdateModule(SelectedInstalledModule);
            _updateCandidates.Clear();
            _updateCandidates.AddRange(PackageManager.GetUpdateCandidates());

            await _messageBoxService.ShowAsync(result);

            if (result == InstallationResult.Success)
            {
                // Raise all the properties because updates can result in installations
                this.RaisePropertyChanged(nameof(InstallButtonEnabled));
                this.RaisePropertyChanged(nameof(UninstallButtonEnabled));
                this.RaisePropertyChanged(nameof(UpdateButtonEnabled));
                this.RaisePropertyChanged(nameof(UpdateAllButtonEnabled));

                await _scriptService.Reload(false);
            }
        });
    }

    /// <summary>
    /// Update all modules
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateUpdateAllCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (_updateCandidates.Count == 0)
                return;

            var finalResult = InstallationResult.Success;

            foreach (var module in _updateCandidates)
            {
                var result = await PackageManager.UpdateModule(module);
                // Pick a failure, any failure
                if (
                    finalResult == InstallationResult.Success
                    && result != InstallationResult.Success
                )
                {
                    finalResult = result;
                }
            }

            _updateCandidates.Clear();
            _updateCandidates.AddRange(PackageManager.GetUpdateCandidates());

            // Raise all the properties because updates can result in installations
            this.RaisePropertyChanged(nameof(InstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UninstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateAllButtonEnabled));

            await _messageBoxService.ShowAsync(finalResult);

            await _scriptService.Reload(false);
        });
    }

    /// <summary>
    /// Refresh the Module Store
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateRefreshCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            await PackageManager.SetUpBaseRepository();
            await PackageManager.AddAdditionalRepositories(_configuration.RepositoryUrls);

            _updateCandidates.Clear();
            _updateCandidates.AddRange(PackageManager.GetUpdateCandidates());

            // Clear out
            SelectedInstalledModule = null;
            SelectedStoreModule = null;

            // Raise all the properties
            this.RaisePropertyChanged(nameof(InstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UninstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateAllButtonEnabled));

            await _messageBoxService.ShowAsync(
                I18N.PkgMan.PkgManWindow_Title,
                I18N.PkgMan.PkgMan_MsgBox_Refreshed,
                MessageBoxButtons.Ok
            );
        });
    }

    /// <summary>
    /// Add a repository
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateAddRepositoryCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var input = RepoUrlInput.Trim();
            if (string.IsNullOrWhiteSpace(input))
                return;

            Logger.Info($"Adding repository {input}");

            var result = await PackageManager.AddRepository(input);

            switch (result)
            {
                case InstallationResult.Success:
                    _configuration.AddRepositoryUrl(input);
                    await _messageBoxService.ShowAsync(result);
                    break;
                case InstallationResult.AlreadyInstalled:
                    Logger.Error($"Could not add repository because it was already installed.");
                    await _messageBoxService.ShowAsync(
                        I18N.PkgMan.PkgManWindow_Title,
                        I18N.PkgMan.PkgMan_Result_Repository_AlreadyInstalled,
                        MessageBoxButtons.Ok
                    );
                    break;
                case InstallationResult.Failure:
                    Logger.Error("Failed to add repository");
                    await _messageBoxService.ShowAsync(result);
                    break;
                default:
                    Logger.Error("Invalid repository add response");
                    throw new ArgumentOutOfRangeException();
            }
            // Clean up
            RepoUrlInput = string.Empty;
        });
    }

    /// <summary>
    /// Remove a repository
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateRemoveRepositoryCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedRepository?.Url is null)
                return;

            Logger.Info($"Removing repository {SelectedRepository.Name}");

            var result = PackageManager.RemoveRepository(SelectedRepository.Name);

            switch (result)
            {
                case InstallationResult.Success:
                    _configuration.RemoveRepositoryUrl(SelectedRepository.Url);
                    await _messageBoxService.ShowAsync(result);
                    break;
                case InstallationResult.NotInstalled:
                    Logger.Error($"Could not remove repository because it was not installed");
                    await _messageBoxService.ShowAsync(
                        I18N.PkgMan.PkgManWindow_Title,
                        I18N.PkgMan.PkgMan_Result_Repository_NotInstalled,
                        MessageBoxButtons.Ok
                    );
                    break;
                default:
                    Logger.Error("Invalid repository remove response");
                    throw new ArgumentOutOfRangeException();
            }

            try
            {
                _configuration.RemoveRepositoryUrl(SelectedRepository.Url);
                await PackageManager.SetUpBaseRepository();
                await PackageManager.AddAdditionalRepositories(_configuration.RepositoryUrls);
                await _messageBoxService.ShowAsync(InstallationResult.Success);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Removing repository failed");
                await _messageBoxService.ShowAsync(
                    I18N.PkgMan.PkgManWindow_Title,
                    ex.Message,
                    MessageBoxButtons.Ok,
                    MaterialIconKind.Error
                );
            }
            // Clean up
            SelectedRepository = null;
        });
    }
}
