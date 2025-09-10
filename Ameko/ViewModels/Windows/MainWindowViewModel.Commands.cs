// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Ameko.Utilities;
using Ameko.ViewModels.Dialogs;
using Ameko.Views.Dialogs;
using Ameko.Views.Windows;
using AssCS;
using AssCS.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Holo;
using Holo.Models;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
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
            var wsp = ProjectProvider.Current.AddWorkspace();
            ProjectProvider.Current.WorkingSpace = wsp;
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
                try
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
                        latest = ProjectProvider.Current.AddWorkspace(doc, uri);
                        latest.IsSaved = true;
                    }
                    else
                    {
                        // Non-ass sourced documents need to be re-saved as an ass file
                        latest = ProjectProvider.Current.AddWorkspace(doc);
                        latest.IsSaved = false;
                    }

                    Logger.Info($"Opened subtitle file {latest.Title}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to parse file {uri.LocalPath}");
                    Logger.Error(ex);
                    await _messageBoxService.ShowAsync(
                        I18N.Resources.Error,
                        $"{I18N.Resources.Error_FailedToParse}\n\n{ex.Message}",
                        MsgBoxButtonSet.Ok,
                        MsgBoxButton.Ok,
                        MaterialIconKind.Error
                    );
                }
            }

            if (latest is not null)
                ProjectProvider.Current.WorkingSpace = latest;
        });
    }

    /// <summary>
    /// Open a subtitle without using a dialog
    /// </summary>
    /// <returns></returns>
    private ReactiveCommand<Uri, Unit> CreateOpenSubtitleNoGuiCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (Uri uri) =>
            {
                Logger.Debug("Opening subtitle (no-gui)");

                try
                {
                    var ext = Path.GetExtension(uri.LocalPath);
                    var doc = ext switch
                    {
                        ".ass" => new AssParser().Parse(_fileSystem, uri),
                        ".srt" => new SrtParser().Parse(_fileSystem, uri),
                        ".txt" => new TxtParser().Parse(_fileSystem, uri),
                        _ => throw new ArgumentOutOfRangeException(),
                    };

                    Workspace latest;

                    if (ext == ".ass")
                    {
                        latest = ProjectProvider.Current.AddWorkspace(doc, uri);
                        latest.IsSaved = true;
                    }
                    else
                    {
                        // Non-ass sourced documents need to be re-saved as an ass file
                        latest = ProjectProvider.Current.AddWorkspace(doc);
                        latest.IsSaved = false;
                    }

                    Logger.Info($"Opened subtitle file {latest.Title}");
                    ProjectProvider.Current.WorkingSpace = latest;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to parse file {uri.LocalPath}");
                    Logger.Error(ex);
                    await _messageBoxService.ShowAsync(
                        I18N.Resources.Error,
                        $"{I18N.Resources.Error_FailedToParse}\n\n{ex.Message}",
                        MsgBoxButtonSet.Ok,
                        MsgBoxButton.Ok,
                        MaterialIconKind.Error
                    );
                }
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
            if (ProjectProvider.Current.IsWorkspaceLoaded)
            {
                _ = await IoService.SaveSubtitle(
                    SaveSubtitleAs,
                    ProjectProvider.Current.WorkingSpace
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
            if (ProjectProvider.Current.IsWorkspaceLoaded)
            {
                _ = await IoService.SaveSubtitleAs(
                    SaveSubtitleAs,
                    ProjectProvider.Current.WorkingSpace
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
            if (ProjectProvider.Current.IsWorkspaceLoaded)
            {
                _ = await IoService.ExportSubtitle(
                    ExportSubtitle,
                    ProjectProvider.Current.WorkingSpace
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

            foreach (var wsp in ProjectProvider.Current.LoadedWorkspaces.ToArray())
            {
                await IoService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (ProjectProvider.Current.LoadedWorkspaces.Count > 0)
            {
                Logger.Info(
                    $"Opening project file aborted - {ProjectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                );
                return;
            }

            var prj = Project.Parse(_fileSystem, uri);
            ProjectProvider.Current = prj;
            Logger.Info("Loaded project file");

            var culture = prj.SpellcheckCulture;
            if (culture is not null && !_spellcheckService.IsDictionaryInstalled(culture))
            {
                Logger.Info($"Prompting user to download dictionary for {culture}");
                var lang = SpellcheckLanguage.AvailableLanguages.First(l => l.Locale == culture);
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

                foreach (var wsp in ProjectProvider.Current.LoadedWorkspaces.ToArray())
                {
                    await IoService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
                }

                if (ProjectProvider.Current.LoadedWorkspaces.Count > 0)
                {
                    Logger.Info(
                        $"Opening project file aborted - {ProjectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                    );
                    return;
                }

                ProjectProvider.Current = Project.Parse(_fileSystem, uri);
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

            foreach (var wsp in ProjectProvider.Current.LoadedWorkspaces.ToArray())
            {
                await IoService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (ProjectProvider.Current.LoadedWorkspaces.Count > 0)
            {
                Logger.Info(
                    $"Opening project directory aborted - {ProjectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                );
                return;
            }

            ProjectProvider.Current = Project.LoadDirectory(_fileSystem, uri);
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
            _ = await IoService.SaveProject(SaveProjectAs, ProjectProvider.Current);
        });
    }

    /// <summary>
    /// Close the currently active tab
    /// </summary>
    private ReactiveCommand<int?, Unit> CreateCloseTabCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (int? id) =>
            {
                if (ProjectProvider.Current.IsWorkspaceLoaded)
                {
                    var wsp = id.HasValue
                        ? ProjectProvider.Current.GetWorkspace(id.Value)
                        : ProjectProvider.Current.WorkingSpace;
                    if (wsp is null)
                        return;

                    // If the user opened a project file, don't open a new workspace
                    var isLoadedProject = ProjectProvider.Current.SavePath is not null;

                    Logger.Debug($"Closing tab {wsp.Title}");
                    await IoService.SafeCloseWorkspace(wsp, SaveSubtitleAs, !isLoadedProject);
                }
            }
        );
    }

    /// <summary>
    /// Close the currently active tab
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateCloseProjectCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Debug("Preparing to close project");

            foreach (var wsp in ProjectProvider.Current.LoadedWorkspaces.ToArray())
            {
                await IoService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (ProjectProvider.Current.LoadedWorkspaces.Count > 0)
            {
                Logger.Info(
                    $"Closing project aborted - {ProjectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                );
                return;
            }

            var prj = new Project(_fileSystem);
            ProjectProvider.Current = prj;
            Logger.Debug("Successfully closed project and opened a new one");
        });
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateQuitCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Logger.Debug("Preparing to quit");

            foreach (var wsp in ProjectProvider.Current.LoadedWorkspaces.ToArray())
            {
                await IoService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (ProjectProvider.Current.LoadedWorkspaces.Count > 0)
            {
                Logger.Info(
                    $"Quit aborted - {ProjectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
                );
                return;
            }

            if (
                Application.Current?.ApplicationLifetime
                is not IClassicDesktopStyleApplicationLifetime desktop
            )
                return;

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
            ProjectProvider.Current.WorkingSpace?.Undo();
            Dispatcher.UIThread.Post(
                () =>
                {
                    ProjectProvider.Current.WorkingSpace?.SelectionManager.EndSelectionChange();
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
            ProjectProvider.Current.WorkingSpace?.Redo();
            Dispatcher.UIThread.Post(
                () =>
                {
                    ProjectProvider.Current.WorkingSpace?.SelectionManager.EndSelectionChange();
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
            var vm = new SearchDialogViewModel(ProjectProvider, tabFactory);
            await ShowSearchDialog.Handle(vm);
        });
    }

    /// <summary>
    /// Show spellcheck dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowSpellcheckDialogCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var tabFactory = _serviceProvider.GetRequiredService<ITabFactory>();
            var vm = new SpellcheckDialogViewModel(ProjectProvider, _spellcheckService, tabFactory);
            await ShowSpellcheckDialog.Handle(vm);
        });
    }

    /// <summary>
    /// Display the <see cref="StylesManagerWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowStylesManagerCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (ProjectProvider.Current.IsWorkspaceLoaded)
            {
                var vm = _stylesManagerFactory.Create(
                    ProjectProvider.Current,
                    ProjectProvider.Current.WorkingSpace.Document
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

            var wsp = ProjectProvider.Current.WorkingSpace;
            if (wsp is null)
                return;

            var ext = Path.GetExtension(uri.LocalPath);
            try
            {
                wsp.ReferenceFileManager.Reference = ext switch
                {
                    ".ass" => new AssParser().Parse(_fileSystem, uri),
                    ".srt" => new SrtParser().Parse(_fileSystem, uri),
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to parse file {uri.LocalPath}");
                Logger.Error(ex);
                await _messageBoxService.ShowAsync(
                    I18N.Resources.Error,
                    $"{I18N.Resources.Error_FailedToParse}\n\n{ex.Message}",
                    MsgBoxButtonSet.Ok,
                    MsgBoxButton.Ok,
                    MaterialIconKind.Error
                );
            }
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
            var wsp = ProjectProvider.Current.WorkingSpace;
            if (wsp is null)
                return;
            wsp.ReferenceFileManager.Reference = null;
        });
    }

    /// <summary>
    ///Attach a reference file without a open file dialog
    /// </summary>
    private ReactiveCommand<Uri, Unit> CreateAttachReferenceFileNoGuiCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (Uri uri) =>
            {
                Logger.Debug("Preparing to attach a reference file");

                var wsp = ProjectProvider.Current.WorkingSpace;
                if (wsp is null)
                    return;

                var ext = Path.GetExtension(uri.LocalPath);
                try
                {
                    wsp.ReferenceFileManager.Reference = ext switch
                    {
                        ".ass" => new AssParser().Parse(_fileSystem, uri),
                        ".srt" => new SrtParser().Parse(_fileSystem, uri),
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to parse file {uri.LocalPath}");
                    Logger.Error(ex);
                    await _messageBoxService.ShowAsync(
                        I18N.Resources.Error,
                        $"{I18N.Resources.Error_FailedToParse}\n\n{ex.Message}",
                        MsgBoxButtonSet.Ok,
                        MsgBoxButton.Ok,
                        MaterialIconKind.Error
                    );
                }
            }
        );
    }

    /// <summary>
    /// Display the <see cref="ShiftTimesDialog"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowShiftTimesDialogCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (ProjectProvider.Current.IsWorkspaceLoaded)
            {
                var vm = new ShiftTimesDialogViewModel(ProjectProvider.Current.WorkingSpace);
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

            var wsp = ProjectProvider.Current.WorkingSpace;
            if (wsp is null)
            {
                ProjectProvider.Current.WorkingSpace = wsp = ProjectProvider.Current.AddWorkspace();
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

                var wsp = ProjectProvider.Current.WorkingSpace;
                if (wsp is null)
                {
                    ProjectProvider.Current.WorkingSpace = wsp =
                        ProjectProvider.Current.AddWorkspace();
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

            var wsp = ProjectProvider.Current.WorkingSpace;
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
                await ScriptService.ExecuteScriptAsync(qualifiedName);
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
                await ScriptService.Reload(isManual);
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

    /// <summary>
    /// Display the <see cref="PlaygroundWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowPlaygroundCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = _serviceProvider.GetRequiredService<PlaygroundWindowViewModel>();
            await ShowPlaygroundWindow.Handle(vm);
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

                var boxResult = await _messageBoxService.ShowAsync(
                    I18N.Other.MsgBox_RemoveDocument_Title,
                    I18N.Other.MsgBox_RemoveDocument_Body,
                    MsgBoxButtonSet.YesNo,
                    MsgBoxButton.Yes,
                    MaterialIconKind.QuestionMark
                );

                if (boxResult == MsgBoxButton.Yes)
                {
                    ProjectProvider.Current.RemoveWorkspace(id);
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

                var boxResult = await _messageBoxService.ShowAsync(
                    I18N.Other.MsgBox_RemoveDirectory_Title,
                    I18N.Other.MsgBox_RemoveDirectory_Body,
                    MsgBoxButtonSet.YesNo,
                    MsgBoxButton.Yes,
                    MaterialIconKind.QuestionMark
                );

                if (boxResult == MsgBoxButton.Yes)
                {
                    ProjectProvider.Current.RemoveDirectory(id);
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
                if (ProjectProvider.Current.FindItemById(id) is not DirectoryItem dirItem)
                    return;

                Logger.Debug(
                    $"Displaying input box for rename of directory {id} ({dirItem.Title})"
                );

                var result = await _messageBoxService.ShowInputAsync(
                    I18N.Other.MsgBox_NameDirectory_Title,
                    I18N.Other.MsgBox_NameDirectory_Body,
                    dirItem.Title,
                    MsgBoxButtonSet.OkCancel,
                    MsgBoxButton.Ok,
                    MaterialIconKind.Rename
                );

                if (result is null)
                    return;

                var (boxResult, userInput) = result.Value;

                if (boxResult == MsgBoxButton.Ok && !string.IsNullOrWhiteSpace(userInput))
                {
                    dirItem.Name = userInput;
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
                if (ProjectProvider.Current.FindItemById(id) is not DocumentItem docItem)
                    return;

                Logger.Debug($"Displaying input box for rename of document {id} ({docItem.Title})");

                var result = await _messageBoxService.ShowInputAsync(
                    I18N.Other.MsgBox_NameDocument_Title,
                    I18N.Other.MsgBox_NameDocument_Body,
                    docItem.Title,
                    MsgBoxButtonSet.OkCancel,
                    MsgBoxButton.Ok,
                    MaterialIconKind.Rename
                );

                if (result is null)
                    return;

                var (boxResult, userInput) = result.Value;

                if (boxResult == MsgBoxButton.Ok && !string.IsNullOrWhiteSpace(userInput))
                {
                    docItem.Name = userInput;
                }
            }
        );
    }

    /// <summary>
    /// Open project document
    /// </summary>
    private ReactiveCommand<int, Unit> CreateOpenDocumentCommand()
    {
        return ReactiveCommand.Create(
            (int id) =>
            {
                if (ProjectProvider.Current.FindItemById(id) is not DocumentItem docItem)
                    return;

                TryLoadReferenced(docItem.Id);
            }
        );
    }

    /// <summary>
    /// Check if the configuration-specified spellcheck dictionary is installed
    /// </summary>
    private ICommand CreateCheckSpellcheckDictionaryCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var culture = _configuration.SpellcheckCulture;
            if (!_spellcheckService.IsDictionaryInstalled(culture)) // Not installed
            {
                Logger.Info($"Prompting user to download dictionary for {culture}");
                var lang = SpellcheckLanguage.AvailableLanguages.First(l => l.Locale == culture);
                var vm = new InstallDictionaryDialogViewModel(_dictionaryService, lang, false);
                await ShowInstallDictionaryDialog.Handle(vm);
                _spellcheckService.RebuildDictionary();
            }
            else
                _spellcheckService.RebuildDictionary(); // Installed
        });
    }
}
