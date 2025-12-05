// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using System.Threading.Tasks;
using Ameko.DataModels;
using AssCS;
using Avalonia.Input;
using Holo;
using Holo.Media.Providers;
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
    /// Save a subtitle file to a specific location
    /// </summary>
    /// <param name="wsp">Workspace containing the document being saved</param>
    /// <param name="path">Path to save the <paramref name="wsp"/> to</param>
    /// <returns><see langword="true"/> if successful</returns>
    bool SaveSubtitle(Workspace wsp, Uri path);

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
    /// Save a project file, displaying a SaveFileDialog if needed
    /// </summary>
    /// <param name="interaction">Interaction to use for displaying the dialog</param>
    /// <param name="prj">Project to save</param>
    /// <returns>Workspace containing the document being saved</returns>
    Task<bool> SaveProject(Interaction<string, Uri?> interaction, Project prj);

    /// <summary>
    /// Load subtitle files into Workspaces
    /// </summary>
    /// <param name="interaction">Open File Dialog interaction</param>
    /// <param name="prj">Project to add the workspaces to</param>
    /// <returns><see langword="true"/> if successful</returns>
    Task<bool> OpenSubtitleFiles(Interaction<Unit, Uri[]?> interaction, Project prj);

    /// <summary>
    /// Load a subtitle file into a Workspace
    /// </summary>
    /// <param name="uri">URI of the subtitle file</param>
    /// <param name="prj">Project to add the workspace to</param>
    /// <returns><see langword="true"/> if successful</returns>
    Task<bool> OpenSubtitleFile(Uri uri, Project prj);

    /// <summary>
    /// Open a project file
    /// </summary>
    /// <param name="uri">URI of the project file</param>
    /// <param name="saveAs">Interaction for displaying the Save As dialog</param>
    /// <returns><see langword="true"/> if the project was opened</returns>
    Task<bool> OpenProjectFile(Uri uri, Interaction<string, Uri?> saveAs);

    /// <summary>
    /// Open a project directory
    /// </summary>
    /// <param name="uri">URI of the directory</param>
    /// <param name="saveAs">Interaction for displaying the Save As dialog</param>
    /// <returns><see langword="true"/> if the project was opened</returns>
    Task<bool> OpenProjectDirectory(Uri uri, Interaction<string, Uri?> saveAs);

    /// <summary>
    /// Attach a reference file to the workspace
    /// </summary>
    /// <param name="interaction">Open File Dialog interaction</param>
    /// <param name="wsp">Workspace to attach the file to to</param>
    /// <returns><see langword="true"/> if successful</returns>
    Task<bool> AttachReferenceFile(Interaction<Unit, Uri?> interaction, Workspace wsp);

    /// <summary>
    /// Attach a reference file to the workspace
    /// </summary>
    /// <param name="uri">URI of the reference file</param>
    /// <param name="wsp">Workspace to attach the file to</param>
    /// <returns><see langword="true"/> if the file was attached</returns>
    Task<bool> AttachReferenceFile(Uri uri, Workspace wsp);

    /// <summary>
    /// Open a video file and attach it to the <paramref name="workspace"/>
    /// </summary>
    /// <param name="interaction">Open File Dialog interaction</param>
    /// <param name="workspace">Workspace to open the video in</param>
    /// <returns><see langword="true"/> if successful</returns>
    Task<bool> OpenVideoFileAsync(Interaction<Unit, Uri?> interaction, Workspace workspace);

    /// <summary>
    /// Open a linked video file (From the <see cref="Document"/>), if it exists
    /// </summary>
    /// <param name="workspace">Workspace to open the video in</param>
    /// <returns><see langword="true"/> if successful</returns>
    Task<bool> OpenVideoFileAsync(Workspace workspace);

    /// <summary>
    /// Open a video file and attach it to the <paramref name="workspace"/>
    /// </summary>
    /// <param name="uri">URI of the video file</param>
    /// <param name="workspace">Workspace to open the video in</param>
    Task<bool> OpenVideoFileAsync(Uri uri, Workspace workspace);

    /// <summary>
    /// Save a frame to file
    /// </summary>
    /// <param name="interaction">Save File Dialog, if needed</param>
    /// <param name="workspace">Workspace</param>
    /// <param name="mode">Save Frame Mode</param>
    /// <returns></returns>
    Task<bool> SaveFrameToFile(
        Interaction<Unit, Uri?> interaction,
        Workspace workspace,
        SaveFrameMode mode
    );

    /// <summary>
    /// Save a frame to file
    /// </summary>
    /// <param name="interaction">Clipboard interaction</param>
    /// <param name="workspace">Workspace</param>
    /// <param name="mode">Save Frame Mode</param>
    /// <returns></returns>
    Task<bool> CopyFrameToClipboard(
        Interaction<string, Unit> interaction,
        Workspace workspace,
        SaveFrameMode mode
    );
}
