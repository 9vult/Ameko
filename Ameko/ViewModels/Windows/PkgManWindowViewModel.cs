// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ameko.Services;
using Holo.Configuration;
using Holo.Providers;
using Holo.Scripting;
using Holo.Scripting.Models;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class PkgManWindowViewModel : ViewModelBase
{
    private readonly ILogger _logger;
    private readonly IScriptService _scriptService;
    private readonly IConfiguration _configuration;
    private readonly IMessageBoxService _messageBoxService;

    private readonly ObservableCollection<Module> _updateCandidates;
    private string _repoUrlInput;

    public ICommand InstallCommand { get; }
    public ICommand UninstallCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand UpdateAllCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand AddRepositoryCommand { get; }
    public ICommand RemoveRepositoryCommand { get; }

    public IPackageManager PackageManager { get; }

    public Module? SelectedStoreModule
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            this.RaisePropertyChanged(nameof(InstallButtonEnabled));
        }
    }

    public Module? SelectedInstalledModule
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            this.RaisePropertyChanged(nameof(UninstallButtonEnabled));
            this.RaisePropertyChanged(nameof(UpdateButtonEnabled));
        }
    }

    public Repository? SelectedRepository
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
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
        && !PackageManager.InstalledModules.Contains(SelectedStoreModule);

    public bool UninstallButtonEnabled => SelectedInstalledModule is not null;

    public bool UpdateButtonEnabled =>
        SelectedInstalledModule is not null && _updateCandidates.Contains(SelectedInstalledModule);

    public bool UpdateAllButtonEnabled => _updateCandidates.Count > 0;

    public bool AddRepoButtonEnabled =>
        !string.IsNullOrEmpty(RepoUrlInput) && Uri.TryCreate(RepoUrlInput, UriKind.Absolute, out _);

    public bool RemoveRepoButtonEnabled =>
        SelectedRepository?.Url != null
        && SelectedRepository.Url != PackageManager.BaseRepositoryUrl
        && _configuration.RepositoryUrls.Contains(SelectedRepository.Url);

    public PkgManWindowViewModel(
        IPackageManager packageManager,
        IScriptService scriptService,
        IConfiguration configuration,
        ILogger<PkgManWindowViewModel> logger,
        IMessageBoxService messageBoxService
    )
    {
        PackageManager = packageManager;

        _scriptService = scriptService;
        _configuration = configuration;
        _messageBoxService = messageBoxService;
        _logger = logger;

        _repoUrlInput = string.Empty;

        _updateCandidates = new ObservableCollection<Module>(PackageManager.GetUpdateCandidates());

        InstallCommand = CreateInstallCommand();
        UninstallCommand = CreateUninstallCommand();
        UpdateCommand = CreateUpdateCommand();
        UpdateAllCommand = CreateUpdateAllCommand();
        RefreshCommand = CreateRefreshCommand();
        AddRepositoryCommand = CreateAddRepositoryCommand();
        RemoveRepositoryCommand = CreateRemoveRepositoryCommand();
    }
}
