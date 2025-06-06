// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Ameko.Services;
using Ameko.ViewModels.Controls;
using Ameko.Views.Windows;
using AssCS;
using AssCS.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Holo;
using Holo.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using NLog;
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
            var wsp = Context.Solution.AddWorkspace();
            Context.Solution.WorkingSpace = wsp;
        });
    }

    /// <summary>
    /// Display the Open Subtitle dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Log.Info("Preparing to open subtitles");
            var uris = await OpenSubtitle.Handle(Unit.Default);

            Workspace? latest = null;

            foreach (var uri in uris)
            {
                var doc = Path.GetExtension(uri.LocalPath) switch
                {
                    ".ass" => new AssParser().Parse(uri.LocalPath),
                    ".txt" => new TxtParser().Parse(uri.LocalPath),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                latest = Context.Solution.AddWorkspace(doc, uri);
                latest.IsSaved = true;
                Log.Info($"Opened subtitle file {latest.Title}");
            }

            if (latest is not null)
                Context.Solution.WorkingSpace = latest;
        });
    }

    /// <summary>
    /// Display either the Save Subtitle or Save Subtitle As dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            _ = await IoService.SaveSubtitle(SaveSubtitleAs, Context.Solution.WorkingSpace);
        });
    }

    /// <summary>
    /// Display the Save Subtitle As dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveSubtitleAsCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            _ = await IoService.SaveSubtitleAs(SaveSubtitleAs, Context.Solution.WorkingSpace);
        });
    }

    /// <summary>
    /// Display the Save As dialog for exporting
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateExportSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            _ = await IoService.ExportSubtitle(ExportSubtitle, Context.Solution.WorkingSpace);
        });
    }

    /// <summary>
    /// Display the Open Solution File dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenSolutionCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Log.Info("Preparing to open solution file");
            var uri = await OpenSolution.Handle(Unit.Default);

            if (uri is null)
            {
                Log.Info("Opening solution file aborted");
                return;
            }

            foreach (var wsp in Context.Solution.LoadedWorkspaces.ToArray())
            {
                await IoService.SafeCloseWorkspace(wsp, SaveSubtitleAs, false);
            }

            if (Context.Solution.LoadedWorkspaces.Count > 0)
            {
                Log.Info(
                    $"Opening solution file aborted - {Context.Solution.LoadedWorkspaces.Count} workspaces remain open"
                );
                return;
            }

            HoloContext.Instance.Solution = Solution.Parse(uri);
            Log.Info("Loaded solution file");
        });
    }

    /// <summary>
    /// Display the Open Solution File dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveSolutionCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            _ = await IoService.SaveSolution(SaveSolutionAs, Context.Solution);
        });
    }

    /// <summary>
    /// Close the currently active tab
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateCloseTabCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            Log.Trace($"Closing tab {Context.Solution.WorkingSpace.Title}");
            await IoService.SafeCloseWorkspace(Context.Solution.WorkingSpace, SaveSubtitleAs);
        });
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

            Log.Info("Shutting down...");
            desktop.Shutdown();
        });
    }

    /// <summary>
    /// Display the <see cref="LogWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowLogWindowCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new LogWindowViewModel();
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
                Log.Trace($"Displaying message to confirm removal of document {id} from solution");

                var box = MessageBoxManager.GetMessageBoxStandard(
                    title: I18N.Resources.MsgBox_RemoveDocument_Title,
                    text: I18N.Resources.MsgBox_RemoveDocument_Title,
                    ButtonEnum.YesNo
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Yes)
                {
                    Context.Solution.RemoveWorkspace(id);
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
                Log.Trace($"Displaying message to confirm removal of directory {id} from solution");

                var box = MessageBoxManager.GetMessageBoxStandard(
                    title: I18N.Resources.MsgBox_RemoveDirectory_Title,
                    text: I18N.Resources.MsgBox_RemoveDirectory_Body,
                    ButtonEnum.YesNo
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Yes)
                {
                    Context.Solution.RemoveDirectory(id);
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
                if (Context.Solution.FindItemById(id) is not DirectoryItem dirItem)
                    return;

                Log.Trace($"Displaying input box for rename of directory {id} ({dirItem.Title})");

                var box = MessageBoxManager.GetMessageBoxStandard(
                    new MessageBoxStandardParams
                    {
                        ContentTitle = I18N.Resources.MsgBox_NameDirectory_Title,
                        ContentMessage = I18N.Resources.MsgBox_NameDirectory_Body,
                        ButtonDefinitions = ButtonEnum.OkCancel,
                        Icon = Icon.None,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        InputParams = new InputParams
                        {
                            DefaultValue = dirItem.Name ?? string.Empty,
                            Label = I18N.Resources.MsgBox_NameItem_Label,
                        },
                    }
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Ok)
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
                if (Context.Solution.FindItemById(id) is not DocumentItem docItem)
                    return;

                Log.Trace($"Displaying input box for rename of document {id} ({docItem.Title})");

                var box = MessageBoxManager.GetMessageBoxStandard(
                    new MessageBoxStandardParams
                    {
                        ContentTitle = I18N.Resources.MsgBox_NameDocument_Title,
                        ContentMessage = I18N.Resources.MsgBox_NameDocument_Body,
                        ButtonDefinitions = ButtonEnum.OkCancel,
                        Icon = Icon.None,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        InputParams = new InputParams
                        {
                            DefaultValue = docItem.Name ?? string.Empty,
                            Label = I18N.Resources.MsgBox_NameItem_Label,
                        },
                    }
                );
                var boxResult = await box.ShowAsync();

                if (boxResult == ButtonResult.Ok)
                {
                    docItem.Name = box.InputValue;
                }
            }
        );
    }
}
