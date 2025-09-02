// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Linq;
using System.Threading.Tasks;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Windows;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Holo.Configuration;
using Holo.Configuration.Keybinds;
using Holo.Providers;
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

        // Activate some key services
        _ = provider.GetRequiredService<CultureService>();
        _ = provider.GetRequiredService<ThemeService>();
        // May have to move this if it gets too resource-intensive
        provider.GetRequiredService<ILayoutProvider>().Reload();

        // Queue up welcome message before other services start adding messages
        provider
            .GetRequiredService<IMessageService>()
            .Enqueue(I18N.Resources.Message_Welcome, TimeSpan.FromSeconds(7));

        // Set up the tab item template
        Resources["WorkspaceTabTemplate"] = new WorkspaceTabTemplate(provider);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            var vm = provider.GetRequiredService<MainWindowViewModel>();
            DataContext = vm;
            desktop.MainWindow = new MainWindow { DataContext = vm };
        }

        base.OnFrameworkInitializationCompleted();

        // Start long process loading in the background after GUI finishes loading
        Dispatcher.UIThread.Post(() =>
        {
            InitializeKeybindService(provider);
            InitializeScriptService(provider);
            InitializePackageManager(provider);
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
    /// Initialize the <see cref="KeybindService"/> (and <see cref="KeybindRegistrar"/>)
    /// </summary>
    /// <param name="provider"></param>
    private static void InitializeKeybindService(IServiceProvider provider)
    {
        // Commence keybind registration
        _ = provider.GetRequiredService<IKeybindService>();
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
                await provider.GetRequiredService<IScriptService>().Reload(isManual: false);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "ScriptService failed to initialize.");
            }
        });
    }

    /// <summary>
    /// Initialize <see cref="PackageManager"/>
    /// </summary>
    /// <param name="provider">Service Provider</param>
    private static void InitializePackageManager(IServiceProvider provider)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var pkgMan = provider.GetRequiredService<IPackageManager>();
                await pkgMan.SetUpBaseRepository();
                await pkgMan.AddAdditionalRepositories(
                    provider.GetRequiredService<IConfiguration>().RepositoryUrls
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
