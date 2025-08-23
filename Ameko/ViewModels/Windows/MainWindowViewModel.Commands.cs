// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Ameko.Services;
using Ameko.ViewModels.Dialogs;
using Ameko.Views.Dialogs;
using Ameko.Views.Windows;
using AssCS;
using AssCS.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Holo;
using Holo.Models;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Create a new file
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateNewCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var wsp = SolutionProvider.Current.AddWorkspace();
            SolutionProvider.Current.WorkingSpace = wsp;
        });
    }

    /// <summary>
    /// Display the Open Subtitle dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Info("Preparing to open subtitles");
            var uris = await OpenSubtitle.Handle(Unit.Default);

            Workspace? latest = null;

            foreach (var uri in uris)
            {
                var doc = Path.GetExtension(uri.LocalPath) switch
                {
                    ".ass" => new AssParser().Parse(_fileSystem, uri),
                    ".txt" => new TxtParser().Parse(_fileSystem, uri),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                latest = SolutionProvider.Current.AddWorkspace(doc, uri);
                latest.IsSaved = true;
                Logger.Info($"Opened subtitle file {latest.Title}");
            }

            if (latest is not null)
                SolutionProvider.Current.WorkingSpace = latest;
        });
    }

    /// <summary>
    /// Display either the Save Subtitle or Save Subtitle As dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (SolutionProvider.Current.IsWorkspaceLoaded)
            {
                _ = await _ioService.SaveSubtitle(
                    SaveSubtitleAs,
                    SolutionProvider.Current.WorkingSpace
                );
            }
        });
    }

    /// <summary>
    /// Display the Save Subtitle As dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveSubtitleAsCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (SolutionProvider.Current.IsWorkspaceLoaded)
            {
                _ = await _ioService.SaveSubtitleAs(
                    SaveSubtitleAs,
                    SolutionProvider.Current.WorkingSpace
                );
            }
        });
    }

    /// <summary>
    /// Display the Save As dialog for exporting
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateExportSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (SolutionProvider.Current.IsWorkspaceLoaded)
            {
                _ = await _ioService.ExportSubtitle(
                    ExportSubtitle,
                    SolutionProvider.Current.WorkingSpace
                );
            }
        });
    }

    /// <summary>
    /// Display the Open Solution File dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenSolutionCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Info("Preparing to open solution file");
            var uri = await OpenSolution.Handle(Unit.Default);

            if (uri is null)
            {
                Logger.Info("Opening solution file aborted");
                return;
            }

            foreach (var wsp in SolutionProvider.Current.LoadedWorkspaces.ToArray())
            {
                await _ioService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (SolutionProvider.Current.LoadedWorkspaces.Count > 0)
            {
                Logger.Info(
                    $"Opening solution file aborted - {SolutionProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                );
                return;
            }

            SolutionProvider.Current = Solution.Parse(_fileSystem, uri);
            Logger.Info("Loaded solution file");
        });
    }

    /// <summary>
    /// Display the Open Folder as Solution File dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenFolderAsSolutionCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Info("Preparing to open a directory as a solution");
            var uri = await OpenFolderAsSolution.Handle(Unit.Default);

            if (uri is null)
            {
                Logger.Info("Opening solution directory aborted");
                return;
            }

            foreach (var wsp in SolutionProvider.Current.LoadedWorkspaces.ToArray())
            {
                await _ioService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (SolutionProvider.Current.LoadedWorkspaces.Count > 0)
            {
                Logger.Info(
                    $"Opening solution directory aborted - {SolutionProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                );
                return;
            }

            SolutionProvider.Current = Solution.LoadDirectory(_fileSystem, uri);
            Logger.Info("Loaded solution directory");
        });
    }

    /// <summary>
    /// Display the Save Solution File dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveSolutionCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            _ = await _ioService.SaveSolution(SaveSolutionAs, SolutionProvider.Current);
        });
    }

    /// <summary>
    /// Close the currently active tab
    /// </summary>
    private ReactiveCommand<int, Unit> CreateCloseTabCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int id) =>
            {
                if (SolutionProvider.Current.IsWorkspaceLoaded)
                {
                    var wsp = SolutionProvider.Current.GetWorkspace(id);
                    if (wsp is null)
                        return;

                    Logger.Trace($"Closing tab {wsp.Title}");
                    await _ioService.SafeCloseWorkspace(wsp, SaveSubtitleAs);
                }
            }
        );
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    private static ReactiveCommand<Unit, Unit> CreateQuitCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (
                Application.Current?.ApplicationLifetime
                is not IClassicDesktopStyleApplicationLifetime desktop
            )
                return;

            Logger.Info("Shutting down...");
            desktop.Shutdown();
        });
    }

    /// <summary>
    /// Display the <see cref="StylesManagerWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowStylesManagerCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (SolutionProvider.Current.IsWorkspaceLoaded)
            {
                var vm = _stylesManagerFactory.Create(
                    SolutionProvider.Current,
                    SolutionProvider.Current.WorkingSpace.Document
                );
                await ShowStylesManager.Handle(vm);
            }
        });
    }

    /// <summary>
    /// Display the <see cref="ShiftTimesDialog"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowShiftTimesDialogCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (SolutionProvider.Current.IsWorkspaceLoaded)
            {
                var vm = new ShiftTimesDialogViewModel(SolutionProvider.Current.WorkingSpace);
                await ShowShiftTimesDialog.Handle(vm);
            }
        });
    }

    /// <summary>
    /// Display the Open Video dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenVideoCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Info("Preparing to open video");
            var uri = await OpenVideo.Handle(Unit.Default);

            if (uri is null)
                return;

            var wsp = SolutionProvider.Current.WorkingSpace;
            if (wsp is null)
            {
                SolutionProvider.Current.WorkingSpace = wsp =
                    SolutionProvider.Current.AddWorkspace();
            }

            wsp.MediaController.OpenVideo(uri.LocalPath);
            wsp.MediaController.SetSubtitles(wsp.Document);
        });
    }

    /// <summary>
    /// Execute a Script
    /// </summary>
    private ReactiveCommand<string, Unit> CreateExecuteScriptCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (string qualifiedName) =>
            {
                await _scriptService.ExecuteScriptAsync(qualifiedName);
            }
        );
    }

    /// <summary>
    /// Reload scripts
    /// </summary>
    private ReactiveCommand<bool, Unit> CreateReloadScriptsCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (bool isManual) =>
            {
                await _scriptService.Reload(isManual);
            }
        );
    }

    /// <summary>
    /// Display the <see cref="PkgManWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowPackageManagerCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = _serviceProvider.GetRequiredService<PkgManWindowViewModel>();
            await ShowPackageManager.Handle(vm);
        });
    }

    public ReactiveCommand<string, Unit> CreateSelectLayoutCommand()
    {
        return ReactiveCommand.Create(
            (string name) =>
            {
                Logger.Trace($"Switching to layout {name}");
                var layout = LayoutProvider.Layouts.FirstOrDefault(l => l.Name == name);
                if (layout is null)
                    return;
                LayoutProvider.Current = layout;
            }
        );
    }

    public ReactiveCommand<Unit, Unit> CreateRefreshLayoutsCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            LayoutProvider.Reload();
        });
    }

    /// <summary>
    /// Display the <see cref="LogWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowLogWindowCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = _serviceProvider.GetRequiredService<LogWindowViewModel>();
            await ShowLogWindow.Handle(vm);
        });
    }

    /// <summary>
    /// Display the <see cref="AboutWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowAboutWindowCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new AboutWindowViewModel();
            await ShowAboutWindow.Handle(vm);
        });
    }

    /// <summary>
    /// Remove workspace from the solution
    /// </summary>
    private ReactiveCommand<int, Unit> CreateRemoveDocumentFromSolutionCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int id) =>
            {
                Logger.Trace(
                    $"Displaying message to confirm removal of document {id} from solution"
                );

                var box = MessageBoxManager.GetMessageBoxStandard(
                    title: I18N.Other.MsgBox_RemoveDocument_Title,
                    text: I18N.Other.MsgBox_RemoveDocument_Body,
                    ButtonEnum.YesNo
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Yes)
                {
                    SolutionProvider.Current.RemoveWorkspace(id);
                }
            }
        );
    }

    /// <summary>
    /// Remove directory from the solution
    /// </summary>
    private ReactiveCommand<int, Unit> CreateRemoveDirectoryFromSolutionCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int id) =>
            {
                Logger.Trace(
                    $"Displaying message to confirm removal of directory {id} from solution"
                );

                var box = MessageBoxManager.GetMessageBoxStandard(
                    title: I18N.Other.MsgBox_RemoveDirectory_Title,
                    text: I18N.Other.MsgBox_RemoveDirectory_Body,
                    ButtonEnum.YesNo
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Yes)
                {
                    SolutionProvider.Current.RemoveDirectory(id);
                }
            }
        );
    }

    /// <summary>
    /// Rename solution directory
    /// </summary>
    private ReactiveCommand<int, Unit> CreateRenameDirectoryCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int id) =>
            {
                if (SolutionProvider.Current.FindItemById(id) is not DirectoryItem dirItem)
                    return;

                Logger.Trace(
                    $"Displaying input box for rename of directory {id} ({dirItem.Title})"
                );

                var box = MessageBoxManager.GetMessageBoxStandard(
                    new MessageBoxStandardParams
                    {
                        ContentTitle = I18N.Other.MsgBox_NameDirectory_Title,
                        ContentMessage = I18N.Other.MsgBox_NameDirectory_Body,
                        ButtonDefinitions = ButtonEnum.OkCancel,
                        Icon = Icon.None,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        InputParams = new InputParams
                        {
                            DefaultValue = dirItem.Name ?? string.Empty,
                            Label = I18N.Other.MsgBox_NameItem_Label,
                        },
                    }
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Ok && !string.IsNullOrWhiteSpace(box.InputValue))
                {
                    dirItem.Name = box.InputValue;
                }
            }
        );
    }

    /// <summary>
    /// Rename solution document
    /// </summary>
    private ReactiveCommand<int, Unit> CreateRenameDocumentCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int id) =>
            {
                if (SolutionProvider.Current.FindItemById(id) is not DocumentItem docItem)
                    return;

                Logger.Trace($"Displaying input box for rename of document {id} ({docItem.Title})");

                var box = MessageBoxManager.GetMessageBoxStandard(
                    new MessageBoxStandardParams
                    {
                        ContentTitle = I18N.Other.MsgBox_NameDocument_Title,
                        ContentMessage = I18N.Other.MsgBox_NameDocument_Body,
                        ButtonDefinitions = ButtonEnum.OkCancel,
                        Icon = Icon.None,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        InputParams = new InputParams
                        {
                            DefaultValue = docItem.Name ?? string.Empty,
                            Label = I18N.Other.MsgBox_NameItem_Label,
                        },
                    }
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Ok && !string.IsNullOrWhiteSpace(box.InputValue))
                {
                    docItem.Name = box.InputValue;
                }
            }
        );
    }
}
