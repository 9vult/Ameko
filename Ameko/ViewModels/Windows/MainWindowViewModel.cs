// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Ameko.Services;
using Ameko.ViewModels.Controls;
using Holo;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Interactions
    public Interaction<LogWindowViewModel, Unit> ShowLogWindow { get; }
    #endregion

    #region Commands
    public ICommand ShowLogWindowCommand { get; }
    #endregion

    private readonly ObservableCollection<TabItemViewModel> _tabItems;

    /// <summary>
    /// Window title
    /// </summary>
    public string WindowTitle { get; } = $"Ameko {VersionService.FullLabel}";

    /// <summary>
    /// Currently-open <see cref="Solution"/>
    /// </summary>
    public Solution Solution
    {
        get => HoloContext.Instance.Solution;
        set
        {
            HoloContext.Instance.Solution = value;
            this.RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Read-only collection of tab items
    /// </summary>
    /// <remarks>Each tab item represents a <see cref="Workspace"/></remarks>
    public ReadOnlyObservableCollection<TabItemViewModel> TabItems { get; }

    public MainWindowViewModel()
    {
        #region Interactions
        ShowLogWindow = new Interaction<LogWindowViewModel, Unit>();
        #endregion

        #region Commands
        ShowLogWindowCommand = CreateShowLogWindowCommand();
        #endregion

        _tabItems = new ObservableCollection<TabItemViewModel>(
            Solution.LoadedWorkspaces.Select(w => new TabItemViewModel("item", w)).ToList()
        );
        TabItems = new ReadOnlyObservableCollection<TabItemViewModel>(_tabItems);
    }
}
