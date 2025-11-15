// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Ameko.Converters;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Windows;
using Ameko.Views.Windows;
using AssCS.IO;
using AssCS.Utilities;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Holo;
using Holo.Configuration;
using Holo.Configuration.Keybinds;
using Holo.Models;
using Holo.Providers;
using Holo.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;

namespace Ameko;

public partial class App : Application
{
    public override void Initialize()
    {
        CultureService.SetCulture();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var provider = AmekoServiceProvider.Build();

        // Activate some key services
        _ = provider.GetRequiredService<ThemeService>();
        // May have to move this if it gets too resource-intensive
        provider.GetRequiredService<ILayoutProvider>().Reload();

        // Queue up welcome message before other services start adding messages
        provider
            .GetRequiredService<IMessageService>()
            .Enqueue(I18N.Resources.Message_Welcome, TimeSpan.FromSeconds(7));

        // Set up the tab item template
        Resources["WorkspaceTabTemplate"] = new WorkspaceTabTemplate(provider);

        // Set up the CPS warn converter
        Resources["CpsWarnConverter"] = new CpsWarnConverter(
            provider.GetRequiredService<IProjectProvider>(),
            provider.GetRequiredService<IConfiguration>()
        );

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            if (Program.Args.Length == 2 && Program.Args[0] == "--display-crash-report")
            {
                var vm = new CrashReporterWindowViewModel(
                    StringEncoder.Base64Decode(Program.Args[1])
                );
                desktop.MainWindow = new CrashReporterWindow { DataContext = vm };
            }
            else // Normal operation
            {
                var vm = provider.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = provider.GetRequiredService<MainWindow>();
                DataContext = vm;
                desktop.MainWindow.DataContext = vm;
            }
        }

        base.OnFrameworkInitializationCompleted();

        // Start long process loading in the background after GUI finishes loading
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // Check if there's anything to open
            if (Program.Args.Length > 0)
                await InitializeStartupProject(provider);

            InitializeKeybindService(provider);
            InitializeScriptService(provider);
            InitializePackageManager(provider);
            InitializeDiscordRpcService(provider);
#if !DEBUG // Skip update checking on debug builds
            await InitializeUpdateService(provider);
#endif
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
    /// <param name="provider">Service Provider</param>
    private static void InitializeKeybindService(IServiceProvider provider)
    {
        // Commence keybind registration
        _ = provider.GetRequiredService<IKeybindService>();
    }

    /// <summary>
    /// Initialize the Discord rich presence service
    /// </summary>
    /// <param name="provider">Service Provider</param>
    private static void InitializeDiscordRpcService(IServiceProvider provider)
    {
        _ = provider.GetRequiredService<DiscordRpcService>();
    }

    /// <summary>
    /// Initialize the Update service and check for updates
    /// </summary>
    /// <param name="provider"></param>
    private static async Task InitializeUpdateService(IServiceProvider provider)
    {
        var service = provider.GetRequiredService<UpdateService>();
        await service.CheckForUpdates();
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
                provider
                    .GetRequiredService<ILogger<App>>()
                    .LogError(ex, "ScriptService failed to initialize.");
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
                provider
                    .GetRequiredService<ILogger<App>>()
                    .LogError(ex, "Dependency Control failed to initialize.");
            }
        });
    }

    private async Task InitializeStartupProject(IServiceProvider provider)
    {
        var fs = provider.GetRequiredService<IFileSystem>();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        var msgBoxService = provider.GetRequiredService<IMessageBoxService>();
        var msgService = provider.GetRequiredService<IMessageService>();
        List<Uri> subs = [];
        List<Uri> projects = [];
        foreach (var arg in Program.Args)
        {
            var path = arg;
            if (!Path.IsPathRooted(path)) // De-relative any relative paths
            {
                path = Path.GetFullPath(Path.Combine(fs.Directory.GetCurrentDirectory(), arg));
            }
            if (fs.File.Exists(path))
            {
                switch (Path.GetExtension(path))
                {
                    case ".ass":
                    case ".srt":
                        subs.Add(new Uri(path));
                        break;
                    case ".aproj":
                        projects.Add(new Uri(path));
                        break;
                    default:
                        continue;
                }
            }
        }

        if (projects.Count > 0)
        {
            projectProvider.Current = projectProvider.CreateFromFile(projects.First());
        }
        foreach (var uri in subs)
        {
            var ext = Path.GetExtension(uri.LocalPath);
            var doc = ext switch
            {
                ".ass" => new AssParser().Parse(fs, uri),
                ".srt" => new SrtParser().Parse(fs, uri),
                ".txt" => new TxtParser().Parse(fs, uri),
                _ => throw new ArgumentOutOfRangeException(nameof(uri)),
            };

            Workspace wsp;
            if (ext == ".ass")
            {
                wsp = projectProvider.Current.AddWorkspace(doc, uri);
                wsp.IsSaved = true;
                if (doc.GarbageManager.TryGetInt("Active Line", out var lineIdx))
                {
                    var line = doc.EventManager.Events.FirstOrDefault(e => e.Index == lineIdx + 1);
                    if (line is not null)
                        wsp.SelectionManager.Select(line);
                }
            }
            else
            {
                // Non-ass sourced documents need to be re-saved as an ass file
                wsp = projectProvider.Current.AddWorkspace(doc);
                wsp.IsSaved = false;
            }

            if (!doc.GarbageManager.TryGetString("Video File", out var relVideoPath))
                continue;

            var videoPath = Path.GetFullPath(
                Path.Combine(Path.GetDirectoryName(uri.LocalPath) ?? "/", relVideoPath)
            );
            if (fs.File.Exists(videoPath))
            {
                var result = await msgBoxService.ShowAsync(
                    I18N.Other.MsgBox_LoadVideo_Title,
                    $"{I18N.Other.MsgBox_LoadVideo_Body}\n\n{relVideoPath}",
                    MsgBoxButtonSet.YesNo,
                    MsgBoxButton.Yes
                );
                if (result != MsgBoxButton.Yes)
                    continue;
                wsp.MediaController.OpenVideo(videoPath);
                wsp.MediaController.SetSubtitles(wsp.Document);
                if (doc.GarbageManager.TryGetInt("Video Position", out var frame))
                    wsp.MediaController.SeekTo(frame.Value); // Seek for clamp safety
            }
            else
            {
                // Video not found
                msgService.Enqueue(
                    string.Format(I18N.Other.Message_VideoNotFound, Path.GetFileName(videoPath)),
                    TimeSpan.FromSeconds(7)
                );
            }

            projectProvider.Current.WorkingSpace = wsp;
        }
    }
}
