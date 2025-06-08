// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;
using Holo;
using Holo.Scripting;
using Holo.Scripting.Models;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class DepCtlWindowViewModel : ViewModelBase
{
    private Module? _selectedStoreModule;
    private Module? _selectedInstalledModule;
    private readonly ObservableCollection<Module> _updateCandidates;
    private ICommand? _reloadCommand;

    public Interaction<IMsBox<ButtonResult>, Unit> ShowMessageBox { get; }

    public ICommand InstallCommand { get; }
    public ICommand UninstallCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand UpdateAllCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand ExportCommand { get; }

    public DependencyControl DependencyControl => HoloContext.Instance.DependencyControl;

    public ReadOnlyCollection<Module> UpdateCandidates { get; }

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

    public bool InstallButtonEnabled =>
        SelectedStoreModule is not null
        && !DependencyControl.InstalledModules.Contains(SelectedStoreModule);

    public bool UninstallButtonEnabled => SelectedInstalledModule is not null;

    public bool UpdateButtonEnabled =>
        SelectedInstalledModule is not null && UpdateCandidates.Contains(SelectedInstalledModule);

    public bool UpdateAllButtonEnabled => UpdateCandidates.Count > 0;

    public DepCtlWindowViewModel(ICommand reloadCommand)
    {
        _reloadCommand = reloadCommand;
        _updateCandidates = new ObservableCollection<Module>(
            DependencyControl.GetUpdateCandidates()
        );
        UpdateCandidates = new ReadOnlyCollection<Module>(_updateCandidates);

        ShowMessageBox = new Interaction<IMsBox<ButtonResult>, Unit>();

        InstallCommand = CreateInstallCommand();
        UninstallCommand = CreateUninstallCommand();
        UpdateCommand = CreateUpdateCommand();
        UpdateAllCommand = CreateUpdateAllCommand();
        RefreshCommand = CreateRefreshCommand();
    }
}
