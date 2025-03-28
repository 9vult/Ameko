// SPDX-License-Identifier: GPL-3.0-only

using Ameko.Services;

namespace Ameko.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = $"Welcome to Ameko {VersionService.FullLabel}!";
}
