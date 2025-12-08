// SPDX-License-Identifier: GPL-3.0-only

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Ameko.Views.Windows;

public partial class CrashReporterWindow : Window
{
    public CrashReporterWindow()
    {
        InitializeComponent();

        Closed += (_, _) =>
        {
            if (
                Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime desktop
            )
                desktop.Shutdown();
            else
                Environment.Exit(0);
        };
    }
}
