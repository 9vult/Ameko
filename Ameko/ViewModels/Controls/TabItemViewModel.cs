// SPDX-License-Identifier: GPL-3.0-only

using Holo;
using ReactiveUI;

namespace Ameko.ViewModels.Controls;

public partial class TabItemViewModel(string title, Workspace workspace) : ViewModelBase
{
    private string _title = title;

    public Workspace Workspace => workspace;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
}
