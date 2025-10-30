// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Runtime.InteropServices;
using Ameko.Services;
using Avalonia.Controls;
using Avalonia.Input;

namespace Ameko.Views.Windows;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void VersionLabel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var window = GetTopLevel(this);
        _ = window!.Clipboard!.SetTextAsync(
            $"""
            Version: Ameko {VersionService.FullLabel}
            OS: {RuntimeInformation.OSDescription}
            Platform: {SystemService.Platform}
            Platform Architecture: {RuntimeInformation.OSArchitecture}
            Desktop Environment: {SystemService.DesktopEnvironment}
            Display Server: {SystemService.WindowManager}
            Framework: {RuntimeInformation.FrameworkDescription}
            """
        );
    }
}
