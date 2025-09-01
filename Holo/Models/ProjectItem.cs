// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using AssCS;

namespace Holo.Models;

/// <summary>
/// Represents an item in a <see cref="Project"/>.
/// </summary>
public abstract class ProjectItem : BindableBase
{
    private string? _name;

    /// <summary>
    /// Unique identifier for the item.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Optional display name override.
    /// </summary>
    public string? Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value);
            RaisePropertyChanged(nameof(Title));
        }
    }

    /// <summary>
    /// The linked workspace,
    /// or <see langword="null"/> if not loaded or the item is a directory.
    /// </summary>
    public Workspace? Workspace { get; set; }

    /// <summary>
    /// File system save location.
    /// </summary>
    public Uri? Uri { get; set; }

    /// <summary>
    /// Indicates whether the item has a file system location.
    /// </summary>
    public bool IsSavedToFileSystem => Uri is not null;

    /// <summary>
    /// Indicates whether the workspace is loaded.
    /// </summary>
    public bool IsLoaded => Workspace is not null;

    /// <summary>
    /// Child items (only applicable to directories).
    /// </summary>
    public RangeObservableCollection<ProjectItem> Children { get; init; } = [];

    /// <summary>
    /// The type of this item.
    /// </summary>
    public virtual ProjectItemType Type => ProjectItemType.Unknown;

    /// <summary>
    /// Display title for the item.
    /// </summary>
    public virtual string Title => "Untitled Item";
}

public class DocumentItem : ProjectItem
{
    public override ProjectItemType Type => ProjectItemType.Document;

    public override string Title =>
        Name
        ?? (
            IsLoaded ? Workspace!.Title
            : IsSavedToFileSystem ? Path.GetFileNameWithoutExtension(Uri!.LocalPath)
            : $"New {Id}"
        );
}

public class DirectoryItem : ProjectItem
{
    public override ProjectItemType Type => ProjectItemType.Directory;
    public override string Title => Name ?? "Untitled Directory";
}
