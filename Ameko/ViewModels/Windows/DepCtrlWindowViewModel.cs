// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;
using Ameko.Services;
using Holo;
using Holo.Scripting;
using Holo.Scripting.Models;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using NLog;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class DepCtrlWindowViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IScriptService _scriptService;
    private readonly Configuration _configuration;
    private readonly IMessageBoxService _messageBoxService;

    private Module? _selectedStoreModule;
    private Module? _selectedInstalledModule;
    private Repository? _selectedRepository;
    private readonly ObservableCollection<Module> _updateCandidates;
    private string _repoUrlInput;

    public Interaction<IMsBox<ButtonResult>, Unit> ShowMessageBox { get; }

    public ICommand InstallCommand { get; }
    public ICommand UninstallCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand UpdateAllCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand AddRepositoryCommand { get; }
    public ICommand RemoveRepositoryCommand { get; }

    public IDependencyControl DependencyControl { get; }

    public Module? SelectedStoreModule
    {
        get => _selectedStoreModule;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedStoreModule, value);
            this.RaisePropertyChanged(nameof(InstallButtonEnabled));
        }
    }

    public Module? SelectedInstalledModule
    {
        get => _selectedInstalledModule;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedInstalledModule, value);
            this.RaisePropertyChanged(nameof(UninstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateButtonEnabled));
        }
    }

    public Repository? SelectedRepository
    {
        get => _selectedRepository;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedRepository, value);
            this.RaisePropertyChanged(nameof(RemoveRepoButtonEnabled));
        }
    }

    public string RepoUrlInput
    {
        get => _repoUrlInput;
        set
        {
            this.RaiseAndSetIfChanged(ref _repoUrlInput, value);
            this.RaisePropertyChanged(nameof(AddRepoButtonEnabled));
        }
    }

    public bool InstallButtonEnabled =>
        SelectedStoreModule is not null
        && !DependencyControl.InstalledModules.Contains(SelectedStoreModule);

    public bool UninstallButtonEnabled => SelectedInstalledModule is not null;

    public bool UpdateButtonEnabled =>
        SelectedInstalledModule is not null && _updateCandidates.Contains(SelectedInstalledModule);

    public bool UpdateAllButtonEnabled => _updateCandidates.Count > 0;

    public bool AddRepoButtonEnabled =>
        !string.IsNullOrEmpty(RepoUrlInput) && Uri.TryCreate(RepoUrlInput, UriKind.Absolute, out _);

    public bool RemoveRepoButtonEnabled =>
        SelectedRepository?.Url != null
        && SelectedRepository.Url != DependencyControl.BaseRepositoryUrl
        && _configuration.RepositoryUrls.Contains(SelectedRepository.Url);

    public DepCtrlWindowViewModel(
        IDependencyControl dependencyControl,
        IScriptService scriptService,
        Configuration configuration,
        IMessageBoxService messageBoxService
    )
    {
        DependencyControl = dependencyControl;

        _scriptService = scriptService;
        _configuration = configuration;
        _messageBoxService = messageBoxService;

        _repoUrlInput = string.Empty;

        _updateCandidates = new ObservableCollection<Module>(
            DependencyControl.GetUpdateCandidates()
        );

        ShowMessageBox = new Interaction<IMsBox<ButtonResult>, Unit>();

        InstallCommand = CreateInstallCommand();
        UninstallCommand = CreateUninstallCommand();
        UpdateCommand = CreateUpdateCommand();
        UpdateAllCommand = CreateUpdateAllCommand();
        RefreshCommand = CreateRefreshCommand();
        AddRepositoryCommand = CreateAddRepositoryCommand();
        RemoveRepositoryCommand = CreateRemoveRepositoryCommand();
    }
}
