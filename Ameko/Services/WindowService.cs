// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Threading.Tasks;
using Ameko.Views.Windows;
using Avalonia.Controls;
using Holo.Providers;

namespace Ameko.Services;

public class WindowService(MainWindow mainWindow) : IWindowService
{
    /// <inheritdoc />
    public void ShowWindow(
        object window,
        int? width = null,
        int? height = null,
        bool canResize = false
    )
    {
        if (window is not Window win)
            throw new ArgumentException("Alleged window is not a window!");

        win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        win.CanResize = canResize;
        CalculateSize(win, width, height);
        win.Show();
    }

    /// <inheritdoc />
    public async Task ShowDialogAsync(
        object window,
        int? width = null,
        int? height = null,
        bool canResize = false
    )
    {
        if (window is not Window win)
            throw new ArgumentException("Alleged window is not a window!");

        win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        win.CanResize = canResize;
        CalculateSize(win, width, height);
        await win.ShowDialog(mainWindow);
    }

    /// <inheritdoc />
    public async Task<T> ShowDialogAsync<T>(
        object window,
        int? width = null,
        int? height = null,
        bool canResize = false
    )
        where T : class
    {
        if (window is not Window win)
            throw new ArgumentException("Alleged window is not a window!");

        win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        win.CanResize = canResize;
        CalculateSize(win, width, height);
        return await win.ShowDialog<T>(mainWindow);
    }

    /// <summary>
    /// Calculate the size of the window
    /// </summary>
    private static void CalculateSize(Window window, int? width, int? height)
    {
        if (!width.HasValue && !height.HasValue)
            window.SizeToContent = SizeToContent.WidthAndHeight;
        else
        {
            if (width.HasValue)
                window.Width = width.Value;
            else
                window.SizeToContent = SizeToContent.Width;
            if (height.HasValue)
                window.Height = height.Value;
            else
                window.SizeToContent = SizeToContent.Height;
        }
    }
}
