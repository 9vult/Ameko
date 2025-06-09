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
using Avalonia.Threading;
using Holo;
using Holo.Scripting;
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

        base.OnFrameworkInitializationCompleted();

        // Start long process loading in the background after GUI finishes loading
        Dispatcher.UIThread.Post(() =>
        {
            InitializeScriptService(provider);
            InitializeDependencyControl(provider);
        });
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

    /// <summary>
    /// Initialize the <see cref="ScriptService"/>
    /// </summary>
    /// <param name="provider">Service Provider</param>
    private static void InitializeScriptService(IServiceProvider provider)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await provider.GetRequiredService<ScriptService>().Reload(isManual: false);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "ScriptService failed to initialize.");
            }
        });
    }

    /// <summary>
    /// Initialize <see cref="DependencyControl"/>
    /// </summary>
    /// <param name="provider">Service Provider</param>
    private static void InitializeDependencyControl(IServiceProvider provider)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var depCtrl = provider.GetRequiredService<DependencyControl>();
                await depCtrl.SetUpBaseRepository();
                await depCtrl.AddAdditionalRepositories(
                    provider.GetRequiredService<Configuration>().RepositoryUrls
                );
            }
            catch (Exception ex)
            {
                LogManager
                    .GetCurrentClassLogger()
                    .Error(ex, "Dependency Control failed to initialize.");
            }
        });
    }
}
