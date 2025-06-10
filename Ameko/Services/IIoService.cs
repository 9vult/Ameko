// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Threading.Tasks;
using Holo;
using ReactiveUI;

namespace Ameko.Services;

public interface IIoService
{
    /// <summary>
    /// Save a subtitle file, displaying a SaveFileDialog if needed
    /// </summary>
    /// <param name="interaction">Interaction to use for displaying the dialog</param>
    /// <param name="wsp"></param>
    /// <returns>Workspace containing the document being saved</returns>
    Task<bool> SaveSubtitle(Interaction<string, Uri?> interaction, Workspace wsp);

    /// <summary>
    /// Save a subtitle file with a new name
    /// </summary>
    /// <param name="interaction">Interaction to use for displaying the dialog</param>
    /// <param name="wsp">Workspace containing the document being saved</param>
    /// <returns><see langword="true"/> if successful</returns>
    Task<bool> SaveSubtitleAs(Interaction<string, Uri?> interaction, Workspace wsp);

    /// <summary>
    /// Export a subtitle, displaying the Save As dialog
    /// </summary>
    /// <param name="interaction">Interaction to use for displaying the dialog</param>
    /// <param name="wsp">Workspace containing the document being exported</param>
    /// <returns><see langword="true"/> if successful</returns>
    Task<bool> ExportSubtitle(Interaction<string, Uri?> interaction, Workspace wsp);

    /// <summary>
    /// Safely close a workspace, verifying save state
    /// </summary>
    /// <param name="wsp">Workspace to close</param>
    /// <param name="saveAs">SaveAs interaction</param>
    /// <param name="replaceIfLast">Should the workspace be replaced if it's the last one</param>
    /// <returns><see langword="true"/> if the workspace was closed</returns>
    Task<bool> SafeCloseWorkspace(
        Workspace wsp,
        Interaction<string, Uri?> saveAs,
        bool replaceIfLast = true
    );

    /// <summary>
    /// Save a solution file, displaying a SaveFileDialog if needed
    /// </summary>
    /// <param name="interaction">Interaction to use for displaying the dialog</param>
    /// <param name="sln">Solution to save</param>
    /// <returns>Workspace containing the document being saved</returns>
    Task<bool> SaveSolution(Interaction<string, Uri?> interaction, Solution sln);
}
