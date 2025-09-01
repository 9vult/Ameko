// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Ameko.Messages;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Dialogs;
using Ameko.Views.Dialogs;
using Ameko.Views.Windows;
using AssCS;
using AssCS.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Holo;
using Holo.Configuration.Keybinds;
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
            var wsp = IProjectProvider.Current.AddWorkspace();
            IProjectProvider.Current.WorkingSpace = wsp;
        });
    }

    /// <summary>
    /// Display the Open Subtitle dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Debug("Preparing to open subtitles");
            var uris = await OpenSubtitle.Handle(Unit.Default);

            Workspace? latest = null;

            foreach (var uri in uris)
            {
                var ext = Path.GetExtension(uri.LocalPath);
                var doc = ext switch
                {
                    ".ass" => new AssParser().Parse(_fileSystem, uri),
                    ".srt" => new SrtParser().Parse(_fileSystem, uri),
                    ".txt" => new TxtParser().Parse(_fileSystem, uri),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                if (ext == ".ass")
                {
                    latest = IProjectProvider.Current.AddWorkspace(doc, uri);
                    latest.IsSaved = true;
                }
                else
                {
                    // Non-ass sourced documents need to be re-saved as an ass file
                    latest = IProjectProvider.Current.AddWorkspace(doc);
                    latest.IsSaved = false;
                }

                Logger.Info($"Opened subtitle file {latest.Title}");
            }

            if (latest is not null)
                IProjectProvider.Current.WorkingSpace = latest;
        });
    }

    /// <summary>
    /// Open a subtitle without using a dialog
    /// </summary>
    /// <returns></returns>
    private ReactiveCommand<Uri, Unit> CreateOpenSubtitleNoGuiCommand()
    {
        return ReactiveCommand.Create(
            (Uri uri) =>
            {
                Logger.Debug("Opening subtitle (no-gui)");

                var ext = Path.GetExtension(uri.LocalPath);
                var doc = ext switch
                {
                    ".ass" => new AssParser().Parse(_fileSystem, uri),
                    ".txt" => new TxtParser().Parse(_fileSystem, uri),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                Workspace latest;

                if (ext == ".ass")
                {
                    latest = IProjectProvider.Current.AddWorkspace(doc, uri);
                    latest.IsSaved = true;
                }
                else
                {
                    // Non-ass sourced documents need to be re-saved as an ass file
                    latest = IProjectProvider.Current.AddWorkspace(doc);
                    latest.IsSaved = false;
                }
                Logger.Info($"Opened subtitle file {latest.Title}");
                IProjectProvider.Current.WorkingSpace = latest;
            }
        );
    }

    /// <summary>
    /// Display either the Save Subtitle or Save Subtitle As dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (IProjectProvider.Current.IsWorkspaceLoaded)
            {
                _ = await _ioService.SaveSubtitle(
                    SaveSubtitleAs,
                    IProjectProvider.Current.WorkingSpace
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
            if (IProjectProvider.Current.IsWorkspaceLoaded)
            {
                _ = await _ioService.SaveSubtitleAs(
                    SaveSubtitleAs,
                    IProjectProvider.Current.WorkingSpace
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
            if (IProjectProvider.Current.IsWorkspaceLoaded)
            {
                _ = await _ioService.ExportSubtitle(
                    ExportSubtitle,
                    IProjectProvider.Current.WorkingSpace
                );
            }
        });
    }

    /// <summary>
    /// Display the Open Project File dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenProjectCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Debug("Preparing to open project file");
            var uri = await OpenProject.Handle(Unit.Default);

            if (uri is null)
            {
                Logger.Info("Opening project file aborted");
                return;
            }

            foreach (var wsp in IProjectProvider.Current.LoadedWorkspaces.ToArray())
            {
                await _ioService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (IProjectProvider.Current.LoadedWorkspaces.Count > 0)
            {
                Logger.Info(
                    $"Opening project file aborted - {IProjectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                );
                return;
            }

            var prj = Project.Parse(_fileSystem, uri);
            IProjectProvider.Current = prj;
            Logger.Info("Loaded project file");

            var culture = prj.SpellcheckCulture;
            if (culture is not null && !_dictionaryService.TryGetDictionary(culture, out _))
            {
                var lang = SpellcheckLanguage.AvailableLanguages.FirstOrDefault(l =>
                    l.Locale == culture
                );
                if (lang is null)
                {
                    Logger.Warn($"Language {culture} not found, ignoring for now...");
                    return;
                }
                Logger.Info($"Prompting user to download dictionary for {culture}");
                var vm = new InstallDictionaryDialogViewModel(_dictionaryService, lang, true);
                await ShowInstallDictionaryDialog.Handle(vm);
                _spellcheckService.RebuildDictionary();
            }
        });
    }

    /// <summary>
    /// Open a project without using a dialog
    /// </summary>
    private ReactiveCommand<Uri, Unit> CreateOpenProjectNoGuiCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (Uri uri) =>
            {
                Logger.Debug("Preparing to open project file (no-gui)");

                foreach (var wsp in IProjectProvider.Current.LoadedWorkspaces.ToArray())
                {
                    await _ioService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
                }

                if (IProjectProvider.Current.LoadedWorkspaces.Count > 0)
                {
                    Logger.Info(
                        $"Opening project file aborted - {IProjectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                    );
                    return;
                }

                IProjectProvider.Current = Project.Parse(_fileSystem, uri);
                Logger.Info("Loaded project file");
            }
        );
    }

    /// <summary>
    /// Display the Open Folder as Project File dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenFolderAsProjectCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Debug("Preparing to open a directory as a project");
            var uri = await OpenFolderAsProject.Handle(Unit.Default);

            if (uri is null)
            {
                Logger.Info("Opening project directory aborted");
                return;
            }

            foreach (var wsp in IProjectProvider.Current.LoadedWorkspaces.ToArray())
            {
                await _ioService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (IProjectProvider.Current.LoadedWorkspaces.Count > 0)
            {
                Logger.Info(
                    $"Opening project directory aborted - {IProjectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                );
                return;
            }

            IProjectProvider.Current = Project.LoadDirectory(_fileSystem, uri);
            Logger.Info("Loaded project directory");
        });
    }

    /// <summary>
    /// Display the Save Project File dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveProjectCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            _ = await _ioService.SaveProject(SaveProjectAs, IProjectProvider.Current);
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
                if (IProjectProvider.Current.IsWorkspaceLoaded)
                {
                    var wsp = IProjectProvider.Current.GetWorkspace(id);
                    if (wsp is null)
                        return;

                    Logger.Debug($"Closing tab {wsp.Title}");
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
    /// Undo
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateUndoCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            IProjectProvider.Current.WorkingSpace?.Undo();
            Dispatcher.UIThread.Post(
                () =>
                {
                    IProjectProvider.Current.WorkingSpace?.SelectionManager.EndSelectionChange();
                },
                DispatcherPriority.Background
            );
        });
    }

    /// <summary>
    /// Redo
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateRedoCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            IProjectProvider.Current.WorkingSpace?.Redo();
            Dispatcher.UIThread.Post(
                () =>
                {
                    IProjectProvider.Current.WorkingSpace?.SelectionManager.EndSelectionChange();
                },
                DispatcherPriority.Background
            );
        });
    }

    /// <summary>
    /// Show search dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowSearchDialogCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var tabFactory = _serviceProvider.GetRequiredService<ITabFactory>();
            var vm = new SearchDialogViewModel(IProjectProvider, tabFactory);
            await ShowSearchDialog.Handle(vm);
        });
    }

    /// <summary>
    /// Display the <see cref="StylesManagerWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowStylesManagerCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (IProjectProvider.Current.IsWorkspaceLoaded)
            {
                var vm = _stylesManagerFactory.Create(
                    IProjectProvider.Current,
                    IProjectProvider.Current.WorkingSpace.Document
                );
                await ShowStylesManager.Handle(vm);
            }
        });
    }

    /// <summary>
    /// Display the "attach reference file" dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateAttachReferenceFileCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Debug("Preparing to attach a reference file");
            var uri = await AttachReferenceFile.Handle(Unit.Default);
            if (uri is null)
                return;

            var wsp = IProjectProvider.Current.WorkingSpace;
            if (wsp is null)
                return;

            var ext = Path.GetExtension(uri.LocalPath);
            wsp.ReferenceFileManager.Reference = ext switch
            {
                ".ass" => new AssParser().Parse(_fileSystem, uri),
                ".srt" => new SrtParser().Parse(_fileSystem, uri),
                _ => throw new ArgumentOutOfRangeException(),
            };
        });
    }

    /// <summary>
    /// Detatch the reference file
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateDetachReferenceFileCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            Logger.Debug("Detaching reference file");
            var wsp = IProjectProvider.Current.WorkingSpace;
            if (wsp is null)
                return;
            wsp.ReferenceFileManager.Reference = null;
        });
    }

    /// <summary>
    /// Display the <see cref="ShiftTimesDialog"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowShiftTimesDialogCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (IProjectProvider.Current.IsWorkspaceLoaded)
            {
                var vm = new ShiftTimesDialogViewModel(IProjectProvider.Current.WorkingSpace);
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
            Logger.Debug("Preparing to open video");
            var uri = await OpenVideo.Handle(Unit.Default);

            if (uri is null)
                return;

            var wsp = IProjectProvider.Current.WorkingSpace;
            if (wsp is null)
            {
                IProjectProvider.Current.WorkingSpace = wsp =
                    IProjectProvider.Current.AddWorkspace();
            }

            wsp.MediaController.OpenVideo(uri.LocalPath);
            wsp.MediaController.SetSubtitles(wsp.Document);
        });
    }

    /// <summary>
    /// Open a video without using a dialog
    /// </summary>
    private ReactiveCommand<Uri, Unit> CreateOpenVideoNoGuiCommand()
    {
        return ReactiveCommand.Create(
            (Uri uri) =>
            {
                Logger.Debug("Preparing to open video (no-gui)");

                var wsp = IProjectProvider.Current.WorkingSpace;
                if (wsp is null)
                {
                    IProjectProvider.Current.WorkingSpace = wsp =
                        IProjectProvider.Current.AddWorkspace();
                }

                wsp.MediaController.OpenVideo(uri.LocalPath);
                wsp.MediaController.SetSubtitles(wsp.Document);
            }
        );
    }

    /// <summary>
    /// Display the Jump dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowJumpDialogCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Debug("Opening Jump dialog");

            var wsp = IProjectProvider.Current.WorkingSpace;
            if (wsp is null)
                return;

            var videoLoaded = wsp.MediaController.IsVideoLoaded;

            var vm = new JumpDialogViewModel(videoLoaded);
            var result = await ShowJumpDialog.Handle(vm);

            if (result is null)
                return;

            if (result.Frame != 0)
            {
                wsp.MediaController.SeekTo(result.Frame);
                return;
            }

            if (result.Time != Time.FromSeconds(0))
            {
                wsp.MediaController.SeekTo(result.Time);
                return;
            }

            var @event = wsp.Document.EventManager.Events.ElementAtOrDefault(result.Line - 1);
            if (@event is null)
                return;

            wsp.MediaController.SeekTo(@event.Start);
            // Publish a scroll message (?)
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
                Logger.Debug($"Switching to layout {name}");
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
    /// Display the <see cref="KeybindsWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowKeybindsWindowCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = _serviceProvider.GetRequiredService<KeybindsWindowViewModel>();
            await ShowKeybindsWindow.Handle(vm);
        });
    }

    /// <summary>
    /// Remove workspace from the project
    /// </summary>
    private ReactiveCommand<int, Unit> CreateRemoveDocumentFromProjectCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int id) =>
            {
                Logger.Debug(
                    $"Displaying message to confirm removal of document {id} from project"
                );

                var box = MessageBoxManager.GetMessageBoxStandard(
                    title: I18N.Other.MsgBox_RemoveDocument_Title,
                    text: I18N.Other.MsgBox_RemoveDocument_Body,
                    ButtonEnum.YesNo
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Yes)
                {
                    IProjectProvider.Current.RemoveWorkspace(id);
                }
            }
        );
    }

    /// <summary>
    /// Remove directory from the project
    /// </summary>
    private ReactiveCommand<int, Unit> CreateRemoveDirectoryFromProjectCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int id) =>
            {
                Logger.Debug(
                    $"Displaying message to confirm removal of directory {id} from project"
                );

                var box = MessageBoxManager.GetMessageBoxStandard(
                    title: I18N.Other.MsgBox_RemoveDirectory_Title,
                    text: I18N.Other.MsgBox_RemoveDirectory_Body,
                    ButtonEnum.YesNo
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Yes)
                {
                    IProjectProvider.Current.RemoveDirectory(id);
                }
            }
        );
    }

    /// <summary>
    /// Rename project directory
    /// </summary>
    private ReactiveCommand<int, Unit> CreateRenameDirectoryCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int id) =>
            {
                if (IProjectProvider.Current.FindItemById(id) is not DirectoryItem dirItem)
                    return;

                Logger.Debug(
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
    /// Rename project document
    /// </summary>
    private ReactiveCommand<int, Unit> CreateRenameDocumentCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int id) =>
            {
                if (IProjectProvider.Current.FindItemById(id) is not DocumentItem docItem)
                    return;

                Logger.Debug($"Displaying input box for rename of document {id} ({docItem.Title})");

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
