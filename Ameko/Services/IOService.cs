// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AssCS.IO;
using Holo;
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
        writer.Write(uri.LocalPath);
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
        writer.Write(uri.LocalPath);
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
        writer.Write(uri.LocalPath, true);
        Log.Info($"Exported {wsp.Title}");
        return true;
    }
}
