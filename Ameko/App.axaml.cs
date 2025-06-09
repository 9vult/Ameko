// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Linq;
using System.Threading.Tasks;
using Ameko.Services;
using Ameko.Templates;
using Ameko.ViewModels.Windows;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NLog;
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
        var provider = AmekoServiceProvider.Build();

        // Activate the culture and theme services
        _ = provider.GetRequiredService<CultureService>();
        _ = provider.GetRequiredService<ThemeService>();

        // Set up the tab item template
        Resources["WorkspaceTabTemplate"] = new WorkspaceTabTemplate(provider);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        // Start script loading in the background after GUI finishes loading
        _ = Task.Run(() =>
        {
            try
            {
                provider.GetRequiredService<ScriptService>().Reload(isManual: false);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "ScriptService failed to initialize.");
            }
        });

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
}
