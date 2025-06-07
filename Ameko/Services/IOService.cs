// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AssCS.IO;
using Holo;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;
using ReactiveUI;

namespace Ameko.Services;

public static class IoService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Save a subtitle file, displaying a SaveFileDialog if needed
    /// </summary>
    /// <param name="interaction">Interaction to use for displaying the dialog</param>
    /// <param name="wsp"></param>
    /// <returns>Workspace containing the document being saved</returns>
    public static async Task<bool> SaveSubtitle(
        Interaction<string, Uri?> interaction,
        Workspace wsp
    )
    {
        Log.Info($"Preparing to save subtitle file {wsp.Title}");
        var uri = wsp.SavePath ?? await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Log.Info("Save operation cancelled");
            return false;
        }

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        writer.Write(uri);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        Log.Info($"Saved subtitle file {wsp.Title}");
        return true;
    }

    /// <summary>
    /// Save a subtitle file with a new name
    /// </summary>
    /// <param name="interaction">Interaction to use for displaying the dialog</param>
    /// <param name="wsp">Workspace containing the document being saved</param>
    /// <returns><see langword="true"/> if successful</returns>
    public static async Task<bool> SaveSubtitleAs(
        Interaction<string, Uri?> interaction,
        Workspace wsp
    )
    {
        Log.Info($"Preparing to save subtitle file {wsp.Title}");
        var uri = await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Log.Info("Save operation cancelled");
            return false;
        }

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        writer.Write(uri);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        Log.Info($"Saved subtitle file {wsp.Title}");
        return true;
    }

    /// <summary>
    /// Export a subtitle, displaying the Save As dialog
    /// </summary>
    /// <param name="interaction">Interaction to use for displaying the dialog</param>
    /// <param name="wsp">Workspace containing the document being exported</param>
    /// <returns><see langword="true"/> if successful</returns>
    public static async Task<bool> ExportSubtitle(
        Interaction<string, Uri?> interaction,
        Workspace wsp
    )
    {
        Log.Info($"Preparing to export subtitle file {wsp.Title}");
        var uri = await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Log.Info("Export operation cancelled");
            return false;
        }

        var writer = new TxtWriter(wsp.Document, ConsumerService.AmekoInfo);
        writer.Write(uri, true);
        Log.Info($"Exported {wsp.Title}");
        return true;
    }

    /// <summary>
    /// Safely close a workspace, verifying save state
    /// </summary>
    /// <param name="wsp">Workspace to close</param>
    /// <param name="saveAs">SaveAs interaction</param>
    /// <param name="replaceIfLast">Should the workspace be replaced if it's the last one</param>
    /// <returns><see langword="true"/> if the workspace was closed</returns>
    public static async Task<bool> SafeCloseWorkspace(
        Workspace wsp,
        Interaction<string, Uri?> saveAs,
        bool replaceIfLast = true
    )
    {
        Log.Trace($"Closing workspace {wsp.Title}");
        var sln = HoloContext.Instance.Solution;

        if (wsp.IsSaved)
        {
            sln.CloseDocument(wsp.Id, replaceIfLast);
            WorkspaceViewModelService.Deregister(wsp.Id);
            return true;
        }

        Log.Trace($"Displaying message box because workspace {wsp.Title} is not saved");

        var box = MessageBoxManager.GetMessageBoxStandard(
            title: I18N.Resources.MsgBox_Save_Title,
            text: string.Format(I18N.Resources.MsgBox_Save_Body, wsp.Title),
            ButtonEnum.YesNoCancel
        );
        var boxResult = await box.ShowAsync();

        switch (boxResult)
        {
            case ButtonResult.Yes:
                var saved = await SaveSubtitle(saveAs, wsp);
                if (!saved)
                {
                    Log.Info("Tab close operation aborted");
                    return false;
                }
                goto case ButtonResult.No; // lol
            case ButtonResult.No:
                sln.CloseDocument(wsp.Id, replaceIfLast);
                WorkspaceViewModelService.Deregister(wsp.Id);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Save a solution file, displaying a SaveFileDialog if needed
    /// </summary>
    /// <param name="interaction">Interaction to use for displaying the dialog</param>
    /// <param name="sln">Solution to save</param>
    /// <returns>Workspace containing the document being saved</returns>
    public static async Task<bool> SaveSolution(Interaction<string, Uri?> interaction, Solution sln)
    {
        Log.Info($"Preparing to save solution file {sln.Title}");
        var uri = sln.SavePath ?? await interaction.Handle(sln.Title);

        if (uri is null)
        {
            Log.Info("Save operation cancelled");
            return false;
        }

        sln.SavePath = uri;
        sln.Save();

        Log.Info($"Saved solution file {sln.Title}");
        return true;
    }
}
