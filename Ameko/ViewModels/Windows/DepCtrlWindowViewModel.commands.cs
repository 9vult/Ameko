// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using Holo.Scripting.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class DepCtrlWindowViewModel
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

            var result = await DependencyControl.InstallModule(SelectedStoreModule);

            await ShowMessageBox.Handle(_messageBoxService.GetBox(result));

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

            var result = DependencyControl.UninstallModule(SelectedInstalledModule);

            await ShowMessageBox.Handle(_messageBoxService.GetBox(result));

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

            var result = await DependencyControl.UpdateModule(SelectedInstalledModule);
            _updateCandidates.Clear();
            _updateCandidates.AddRange(DependencyControl.GetUpdateCandidates());

            await ShowMessageBox.Handle(_messageBoxService.GetBox(result));

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
                var result = await DependencyControl.UpdateModule(module);
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
            _updateCandidates.AddRange(DependencyControl.GetUpdateCandidates());

            // Raise all the properties because updates can result in installations
            this.RaisePropertyChanged(nameof(InstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UninstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateAllButtonEnabled));

            await ShowMessageBox.Handle(_messageBoxService.GetBox(finalResult));

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
            await DependencyControl.SetUpBaseRepository();
            await DependencyControl.AddAdditionalRepositories(_configuration.RepositoryUrls);

            _updateCandidates.Clear();
            _updateCandidates.AddRange(DependencyControl.GetUpdateCandidates());

            // Clear out
            SelectedInstalledModule = null;
            SelectedStoreModule = null;

            // Raise all the properties
            this.RaisePropertyChanged(nameof(InstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UninstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateAllButtonEnabled));

            await ShowMessageBox.Handle(
                _messageBoxService.GetInfoBox(
                    I18N.Resources.DepCtrlWindow_Title,
                    I18N.Resources.DepCtrl_MsgBox_Refreshed
                )
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
            if (string.IsNullOrWhiteSpace(RepoUrlInput))
                return;

            if (_configuration.RepositoryUrls.Contains(RepoUrlInput.Trim()))
            {
                await ShowMessageBox.Handle(
                    _messageBoxService.GetInfoBox(
                        I18N.Resources.DepCtrlWindow_Title,
                        I18N.Resources.DepCtrl_MsgBox_RepoAlreadyAdded
                    )
                );
                return;
            }

            try
            {
                using var client = new HttpClient();
                var repo = await Repository.Build(RepoUrlInput, client);
                if (repo is null)
                {
                    Logger.Error($"Building repository failed");
                    await ShowMessageBox.Handle(
                        _messageBoxService.GetBox(InstallationResult.Failure)
                    );
                    return;
                }

                _configuration.AddRepositoryUrl(RepoUrlInput);
                await DependencyControl.SetUpBaseRepository();
                await DependencyControl.AddAdditionalRepositories(_configuration.RepositoryUrls);
                await ShowMessageBox.Handle(_messageBoxService.GetBox(InstallationResult.Success));
                _repoUrlInput = string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Building repository failed");
                await ShowMessageBox.Handle(
                    _messageBoxService.GetInfoBox(I18N.Resources.DepCtrlWindow_Title, ex.Message)
                );
            }
        });
    }

    /// <summary>
    /// Remove a repository
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateRemoveRepositoryCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (_selectedRepository?.Url is null)
                return;

            try
            {
                _configuration.RemoveRepositoryUrl(_selectedRepository.Url);
                await DependencyControl.SetUpBaseRepository();
                await DependencyControl.AddAdditionalRepositories(_configuration.RepositoryUrls);
                await ShowMessageBox.Handle(_messageBoxService.GetBox(InstallationResult.Success));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Removing repository failed");
                await ShowMessageBox.Handle(
                    _messageBoxService.GetInfoBox(I18N.Resources.DepCtrlWindow_Title, ex.Message)
                );
            }
        });
    }
}
