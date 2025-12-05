// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Ameko.Utilities;
using Ameko.ViewModels.Dialogs;
using Ameko.Views.Dialogs;
using Ameko.Views.Windows;
using AssCS;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Holo;
using Holo.Models;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            _ = await IoService.OpenSubtitleFiles(OpenSubtitle, ProjectProvider.Current);
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
                _ = await IoService.OpenSubtitleFile(uri, ProjectProvider.Current);
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
            _logger.LogDebug("Preparing to open project file");
            var uri = await OpenProject.Handle(Unit.Default);

            if (uri is null)
            {
                _logger.LogInformation("Opening project file aborted");
                return;
            }

            if (!await IoService.OpenProjectFile(uri, SaveSubtitleAs))
                return;

            var culture = ProjectProvider.Current.SpellcheckCulture;
            if (culture is not null && !_spellcheckService.IsDictionaryInstalled(culture))
            {
                _logger.LogInformation(
                    "Prompting user to download dictionary for {Culture}",
                    culture
                );
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
                _logger.LogDebug("Preparing to open project file (no-gui)");

                if (!await IoService.OpenProjectFile(uri, SaveSubtitleAs))
                    return;

                var culture = ProjectProvider.Current.SpellcheckCulture;
                if (culture is not null && !_spellcheckService.IsDictionaryInstalled(culture))
                {
                    _logger.LogInformation(
                        "Prompting user to download dictionary for {Culture}",
                        culture
                    );
                    var lang = SpellcheckLanguage.AvailableLanguages.First(l =>
                        l.Locale == culture
                    );
                    var vm = new InstallDictionaryDialogViewModel(_dictionaryService, lang, true);
                    await ShowInstallDictionaryDialog.Handle(vm);
                    _spellcheckService.RebuildDictionary();
                }
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
            _logger.LogDebug("Preparing to open a directory as a project");
            var uri = await OpenFolderAsProject.Handle(Unit.Default);

            if (uri is null)
            {
                _logger.LogInformation("Opening project directory aborted");
                return;
            }

            _ = await IoService.OpenProjectDirectory(uri, SaveSubtitleAs);
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

                    _logger.LogDebug("Closing tab {WspTitle}", wsp.Title);
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
            _logger.LogDebug("Preparing to close project");

            foreach (var wsp in ProjectProvider.Current.LoadedWorkspaces.ToArray())
            {
                await IoService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (ProjectProvider.Current.LoadedWorkspaces.Count > 0)
            {
                _logger.LogInformation(
                    "Closing project aborted - {LoadedWorkspacesCount} workspaces remain open",
                    ProjectProvider.Current.LoadedWorkspaces.Count
                );
                return;
            }

            ProjectProvider.Current = ProjectProvider.Create();
            _logger.LogDebug("Successfully closed project and opened a new one");
        });
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateQuitCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            _logger.LogDebug("Preparing to quit");

            foreach (var wsp in ProjectProvider.Current.LoadedWorkspaces.ToArray())
            {
                await IoService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (ProjectProvider.Current.LoadedWorkspaces.Count > 0)
            {
                _logger.LogInformation(
                    "Quit aborted - {LoadedWorkspacesCount} workspaces remain open",
                    ProjectProvider.Current.LoadedWorkspaces.Count
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
            var wsp = ProjectProvider.Current.WorkingSpace;
            if (wsp is null)
                return;

            await IoService.AttachReferenceFile(AttachReferenceFile, wsp);
        });
    }

    /// <summary>
    /// Detatch the reference file
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateDetachReferenceFileCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            _logger.LogDebug("Detaching reference file");
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
                var wsp = ProjectProvider.Current.WorkingSpace;
                if (wsp is null)
                    return;
                await IoService.AttachReferenceFile(uri, wsp);
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
            var wsp = ProjectProvider.Current.WorkingSpace;
            if (wsp is null)
            {
                ProjectProvider.Current.WorkingSpace = wsp = ProjectProvider.Current.AddWorkspace();
            }

            await IoService.OpenVideoFileAsync(OpenVideo, wsp);
        });
    }

    /// <summary>
    /// Open a video without using a dialog
    /// </summary>
    private ReactiveCommand<Uri, Unit> CreateOpenVideoNoGuiCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (Uri uri) =>
            {
                var wsp = ProjectProvider.Current.WorkingSpace;
                if (wsp is null)
                {
                    ProjectProvider.Current.WorkingSpace = wsp =
                        ProjectProvider.Current.AddWorkspace();
                }

                await IoService.OpenVideoFileAsync(uri, wsp);
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
            _logger.LogDebug("Opening Jump dialog");

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
                _logger.LogDebug("Switching to layout {Name}", name);
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
    /// Display the <see cref="HelpWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowHelpWindowCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = _serviceProvider.GetRequiredService<HelpWindowViewModel>();
            await ShowHelpWindow.Handle(vm);
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
    /// Display the <see cref="KeybindsDialog"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowKeybindsDialogCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = _serviceProvider.GetRequiredService<KeybindsDialogViewModel>();
            await ShowKeybindsDialog.Handle(vm);
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
                _logger.LogDebug(
                    "Displaying message to confirm removal of document {Id} from project",
                    id
                );

                var boxResult = await _messageBoxService.ShowAsync(
                    I18N.Other.MsgBox_RemoveDocument_Title,
                    $"{I18N.Other.MsgBox_RemoveDocument_Body}\n\n{I18N.Other.MsgBox_RemoveDocument_Disclaimer}",
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
                _logger.LogDebug(
                    "Displaying message to confirm removal of directory {Id} from project",
                    id
                );

                var boxResult = await _messageBoxService.ShowAsync(
                    I18N.Other.MsgBox_RemoveDirectory_Title,
                    $"{I18N.Other.MsgBox_RemoveDirectory_Body}\n\n{I18N.Other.MsgBox_RemoveDirectory_Disclaimer}",
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

                _logger.LogDebug(
                    "Displaying input box for rename of directory {Id} ({DirItemTitle})",
                    id,
                    dirItem.Title
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

                _logger.LogDebug(
                    "Displaying input box for rename of document {Id} ({DocItemTitle})",
                    id,
                    docItem.Title
                );

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
    private ReactiveCommand<Unit, Unit> CreateCheckSpellcheckDictionaryCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var culture = Configuration.SpellcheckCulture;
            if (!_spellcheckService.IsDictionaryInstalled(culture)) // Not installed
            {
                _logger.LogInformation(
                    "Prompting user to download dictionary for {Culture}",
                    culture
                );
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
