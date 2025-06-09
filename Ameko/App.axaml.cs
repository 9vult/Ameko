// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Ameko.Services;
using Ameko.ViewModels.Windows;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Holo;
using Holo.Models;
using MainWindow = Ameko.Views.Windows.MainWindow;

namespace Ameko;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        I18N.Resources.Culture = new CultureInfo("en-US"); // TODO: Learn how to set this dynamically

        var provider = HoloServiceProvider.Build();

        var context = HoloContext.Instance; // Load Holo
        _ = ScriptService.Instance; // TODO: not this

        // Set the startup theme and subscribe to changes
        SetTheme(context);
        context.Configuration.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(context.Configuration.Theme))
                SetTheme(context);
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel(provider) };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins
            .DataValidators.OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private void SetTheme(HoloContext context)
    {
        if (Current is not null)
        {
            Current.RequestedThemeVariant = context.Configuration.Theme switch
            {
                Theme.Default => ThemeVariant.Default,
                Theme.Dark => ThemeVariant.Dark,
                Theme.Light => ThemeVariant.Light,
                _ => ThemeVariant.Default,
            };
        }
    }
}
