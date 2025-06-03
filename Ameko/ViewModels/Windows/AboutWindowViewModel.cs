// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using Ameko.Services;
using Avalonia.Platform;

namespace Ameko.ViewModels.Windows;

public partial class AboutWindowViewModel : ViewModelBase
{
    // TODO: Migrate to MarkdownViewer.Core when it is out of beta
    public static string Version => VersionService.FullLabel;
}
