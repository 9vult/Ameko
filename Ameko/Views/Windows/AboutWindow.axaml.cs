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
            Ameko
            Version: {VersionService.FullLabel}

            OS: {RuntimeInformation.OSDescription}
            Architecture: {RuntimeInformation.OSArchitecture}
            Framework: {RuntimeInformation.FrameworkDescription}
            """
        );
    }
}
