// SPDX-License-Identifier: MPL-2.0

using AssCS;
using NLog;

namespace Holo;

/// <summary>
/// A group of related files for editing, part of a <see cref="Solution"/>
/// </summary>
/// <param name="document">Ass document</param>
/// <param name="id">Workspace ID</param>
/// <param name="savePath">Path the <paramref name="document"/> saves to,
/// or <see langword="null"/> if unsaved</param>
/// <remarks>
/// A workspace is the related content normally displayed in a single
/// tab in the editor, which includes a <see cref="Document"/>, and optionally
/// supporting files, like video or audio.
/// </remarks>
public class Workspace(Document document, int id, Uri? savePath = null) : BindableBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly Document _document = document;
    private readonly int _id = id;
    private Uri? _savePath = savePath;
    private bool _isSaved = true;

    /// <summary>
    /// The ass document in the workspace
    /// </summary>
    public Document Document => _document;

    /// <summary>
    /// The ID of the workspace
    /// </summary>
    public int Id => _id;

    /// <summary>
    /// The path the <see cref="Document"/> is saved to,
    /// or <see langword="null"/> if the document has not been saved.
    /// </summary>
    public Uri? SavePath
    {
        get => _savePath;
        set => SetProperty(ref _savePath, value);
    }

    /// <summary>
    /// <see langword="true"/> if the <see cref="Document"/> has been saved and is up to date
    /// </summary>
    public bool IsSaved => _isSaved;

    /// <summary>
    /// Title to display in the GUI
    /// </summary>
    /// <remarks>Title is prefixed with <c>*</c> if there are unsaved changes</remarks>
    public string Title =>
        SavePath is not null
            ? $"{(IsSaved ? '*' : string.Empty)}{Path.GetFileNameWithoutExtension(SavePath.LocalPath)}"
            : $"New {Id}";
}
