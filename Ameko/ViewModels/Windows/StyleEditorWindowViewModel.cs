// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using Ameko.Services;
using AssCS;
using Avalonia.Platform;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class StyleEditorWindowViewModel : ViewModelBase
{
    private readonly Style _backupStyle;
    private string _styleName;

    public Style Style { get; init; }

    public string StyleName
    {
        get => _styleName;
        set => this.RaiseAndSetIfChanged(ref _styleName, value);
    }

    public StyleEditorWindowViewModel(Style style)
    {
        Style = style;
        _backupStyle = Style.Clone();
    }
}
