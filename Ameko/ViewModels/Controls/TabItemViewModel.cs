// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.ObjectModel;
using AssCS;
using Holo;
using ReactiveUI;

namespace Ameko.ViewModels.Controls;

public partial class TabItemViewModel : ViewModelBase
{
    private string _title;
    private readonly Workspace _workspace;

    public Workspace Workspace => _workspace;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public TabItemViewModel(string title, Workspace workspace)
    {
        _title = title;
        _workspace = workspace;
    }
}
