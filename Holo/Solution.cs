// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json;
using AssCS;
using AssCS.IO;
using Holo.Models;
using NLog;

namespace Holo;

/// <summary>
/// A solution consisting of multiple <see cref="Workspace"/>s,
/// <see cref="Style"/>s, and options common to the files within.
/// </summary>
/// <remarks>
/// <para>
/// A solution primarily consists of multiple, potentially open,
/// <see cref="Document"/>s. Not all documents in the solution need
/// be opened at the same time.
/// </para><para>
/// Additionally, all open files exist within a solution. If the user
/// has not opened a solution, the files implicitly are opened in the
/// default solution.
/// </para><para>
/// Solutions can be saved to disk with the <c>*.asln</c> file extension.
/// </para>
/// </remarks>
public class Solution : BindableBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };

    private readonly RangeObservableCollection<SolutionItem> _referencedItems;
    private readonly RangeObservableCollection<Workspace> _loadedWorkspaces;

    /// <summary>
    /// The filesystem being used
    /// </summary>
    /// <remarks>This allows for filesystem mocking to be used in tests</remarks>
    private readonly IFileSystem _fileSystem;

    private Uri? _savePath;
    private bool _isSaved;

    private int _docId = 1;
    private Workspace? _workingSpace;

    private int? _cps;
    private bool? _cpsIncludesWhitespace;
    private bool? _cpsIncludesPunctuation;
    private bool? _useSoftLinebreaks;
    private int? _defaultLayer;

    /// <summary>
    /// All items referenced by the solution
    /// </summary>
    public ReadOnlyObservableCollection<SolutionItem> ReferencedItems { get; }

    /// <summary>
    /// Currently-open workspaces
    /// </summary>
    public ReadOnlyObservableCollection<Workspace> LoadedWorkspaces { get; }

    /// <summary>
    /// The solution's style manager
    /// </summary>
    public StyleManager StyleManager { get; }

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
    public bool IsSaved
    {
        get => _isSaved;
        private set => SetProperty(ref _isSaved, value);
    }

    /// <summary>
    /// Next available ID for documents/workspaces
    /// </summary>
    private int NextId => _docId++;

    /// <summary>
    /// Whether there is currently a loaded <see cref="Workspace"/>
    /// </summary>
    [MemberNotNullWhen(true, nameof(WorkingSpace))]
    public bool IsWorkspaceLoaded => WorkingSpace is not null;

    /// <summary>
    /// The currently-loaded workspace
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> if no workspace is loaded
    /// </remarks>
    public Workspace? WorkingSpace
    {
        get => _workingSpace;
        set => SetProperty(ref _workingSpace, value);
    }

    /// <summary>
    /// Workspace-scoped characters-per-second threshold
    /// </summary>
    public int? Cps
    {
        get => _cps;
        set
        {
            SetProperty(ref _cps, value);
            IsSaved = false;
        }
    }

    /// <summary>
    /// If whitespace should be included in <see cref="Event.Cps"/> calculation
    /// </summary>
    public bool? CpsIncludesWhitespace
    {
        get => _cpsIncludesWhitespace;
        set
        {
            SetProperty(ref _cpsIncludesWhitespace, value);
            IsSaved = false;
        }
    }

    /// <summary>
    /// If punctuation should be included in <see cref="Event.Cps"/> calculation
    /// </summary>
    public bool? CpsIncludesPunctuation
    {
        get => _cpsIncludesPunctuation;
        set
        {
            SetProperty(ref _cpsIncludesPunctuation, value);
            IsSaved = false;
        }
    }

    /// <summary>
    /// Workspace-scoped soft linebreaks preference
    /// </summary>
    public bool? UseSoftLinebreaks
    {
        get => _useSoftLinebreaks;
        set
        {
            SetProperty(ref _useSoftLinebreaks, value);
            IsSaved = false;
        }
    }

    /// <summary>
    /// Workspace-scoped default layer preference
    /// </summary>
    public int? DefaultLayer
    {
        get => _defaultLayer;
        set
        {
            SetProperty(ref _defaultLayer, value);
            IsSaved = false;
        }
    }

    /// <summary>
    /// Solution title/name
    /// </summary>
    public string Title =>
        SavePath is not null
            ? Path.GetFileNameWithoutExtension(SavePath.LocalPath)
            : "Default Solution";

    /// <summary>
    /// Get a loaded <see cref="Workspace"/> by ID
    /// </summary>
    /// <param name="id">ID of the desired workspace</param>
    /// <returns>The workspace with the given ID, or <see langword="null"/> if one does not exist</returns>
    public Workspace? GetWorkspace(int id)
    {
        return _loadedWorkspaces.FirstOrDefault(f => f.Id == id);
    }

    /// <summary>
    /// Add a new, empty <see cref="Workspace"/> to the solution, and open it
    /// </summary>
    /// <param name="parentId">Optional ID for adding to a directory</param>
    /// <returns>The created workspace</returns>
    public Workspace AddWorkspace(int parentId = -1)
    {
        Logger.Trace("Creating a default workspace");
        var wsp = new Workspace(new Document(true), NextId);
        return AddWorkspace(wsp, parentId);
    }

    /// <summary>
    /// Add a new <see cref="Workspace"/> to the solution using an existing <see cref="Document"/>, and open it
    /// </summary>
    /// <param name="document">Document to add</param>
    /// <param name="savePath">Save path of the document</param>
    /// <param name="parentId">Optional ID for adding to a directory</param>
    /// <returns>The created workspace</returns>
    public Workspace AddWorkspace(Document document, Uri savePath, int parentId = -1)
    {
        Logger.Trace($"Creating a workspace from document at {savePath}");
        var wsp = new Workspace(document, NextId, savePath);
        return AddWorkspace(wsp, parentId);
    }

    /// <summary>
    /// Add an existing <see cref="Workspace"/> to the solution, and open it
    /// </summary>
    /// <param name="wsp">The workspace to add</param>
    /// <param name="parentId">Optional ID for adding to a directory</param>
    /// <returns>The workspace</returns>
    public Workspace AddWorkspace(Workspace wsp, int parentId = -1)
    {
        Logger.Trace($"Adding workspace {wsp.DisplayTitle}, parent: {parentId}");
        var docItem = new DocumentItem
        {
            Id = wsp.Id,
            Workspace = wsp,
            Uri = wsp.SavePath,
        };

        if (parentId < 0)
        {
            _referencedItems.Add(docItem);
        }
        else
        {
            var parent = FindItemById(parentId);
            if (parent is not null)
                parent.Children.Add(docItem);
            else // Parent not found
                _referencedItems.Add(docItem);
        }

        _loadedWorkspaces.Add(wsp);
        WorkingSpace = wsp;
        return wsp;
    }

    /// <summary>
    /// Add a virtual directory to the solution
    /// </summary>
    /// <param name="name">Name of the directory</param>
    /// <param name="parentId">Optional ID for adding to a directory</param>
    /// <returns>The directory item</returns>
    public SolutionItem AddDirectory(string name, int parentId = -1)
    {
        Logger.Trace($"Adding a new directory, parent: {parentId}");
        var dirItem = new DirectoryItem { Id = NextId, Name = name };

        if (parentId < 0)
        {
            _referencedItems.Add(dirItem);
        }
        else
        {
            var parent = FindItemById(parentId);
            if (parent is not null)
                parent.Children.Add(dirItem);
            else // Parent not found
                _referencedItems.Add(dirItem);
        }
        return dirItem;
    }

    /// <summary>
    /// Remove a directory from the <see cref="ReferencedItems"/>
    /// </summary>
    /// <param name="id">ID of the directory</param>
    /// <returns><see langword="true"/> is successfully removed</returns>
    /// <remarks>Removing a directory will remove all subdirectories and documents!</remarks>
    public bool RemoveDirectory(int id)
    {
        Logger.Trace($"Removing directory with id: {id}");
        var dir = FindItemById(id);
        return dir?.Type == SolutionItemType.Directory && RemoveItemById(id);
    }

    /// <summary>
    /// Remove a <see cref="Workspace"/> from the solution
    /// </summary>
    /// <param name="id">ID of the workspace to remove</param>
    /// <returns><see langword="true"/> if the workspace was removed</returns>
    /// <remarks>
    /// If the workspace being removed is the <see cref="WorkingSpace"/>, then
    /// the <see cref="WorkingSpace"/> will be set to any other currently open
    /// <see cref="Workspace"/>. If there are no other open workspaces, a new
    /// workspace will be created and selected.
    /// </remarks>
    public bool RemoveWorkspace(int id)
    {
        Logger.Trace($"Removing workspace {id}");
        if (WorkingSpace?.Id == id)
        {
            WorkingSpace =
                _loadedWorkspaces.Count > 1
                    ? _loadedWorkspaces.First(w => w.Id != id)
                    : AddWorkspace();
        }

        _loadedWorkspaces.RemoveAll(w => w.Id == id);
        return RemoveItemById(id);
    }

    /// <summary>
    /// Open a referenced document
    /// </summary>
    /// <param name="id">ID of the document to open</param>
    /// <returns>ID of the document or -1 on failure</returns>
    /// <remarks>
    /// A new <see cref="Workspace"/> containing the document and any supporting
    /// files will be created and set as the <see cref="WorkingSpace"/>
    /// </remarks>
    public int OpenDocument(int id)
    {
        Logger.Trace($"Opening referenced document {id}");
        try
        {
            var item = FindItemById(id);
            if (item is null || item.Type != SolutionItemType.Document)
            {
                Logger.Error($"A referenced document with id {id} was not found");
                return -1;
            }

            if (item.Type != SolutionItemType.Document)
            {
                Logger.Error($"Failed to parse document reference with id {id}");
                return -1;
            }

            if (!item.IsSavedToFileSystem)
            {
                Logger.Error($"Document item with id {id} was not saved to file system");
                return -1;
            }

            var parser = new AssParser();
            var document = parser.Parse(_fileSystem, item.Uri!);

            item.Workspace = new Workspace(document, item.Id, item.Uri);
            _loadedWorkspaces.Add(item.Workspace);
            WorkingSpace = item.Workspace;
            return item.Id;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            return -1;
        }
    }

    /// <summary>
    /// Close an open document
    /// </summary>
    /// <param name="id">ID of the document to close</param>
    /// <param name="replaceIfLast">Open a new document if this is the last one open</param>
    /// <returns><see langword="true"/> if the document was closed</returns>
    /// <remarks>
    ///<para>
    /// It is assumed that the caller has saved the file prior
    /// </para><para>
    /// If the document closed is in the <see cref="WorkingSpace"/>, then
    /// the <see cref="WorkingSpace"/> will be set to any other currently open
    /// <see cref="Workspace"/>. If there are no other open workspaces, a new
    /// workspace will be created and selected, so long as <paramref name="replaceIfLast"/>
    /// is <see langword="true"/>.
    /// </para>
    /// </remarks>
    public bool CloseDocument(int id, bool replaceIfLast = true)
    {
        Logger.Trace($"Closing referenced document {id}");

        if (WorkingSpace?.Id != id)
            return _loadedWorkspaces.RemoveAll(w => w.Id == id) != 0;
        if (_loadedWorkspaces.Count > 1)
            WorkingSpace = _loadedWorkspaces.First(w => w.Id != id);
        else if (replaceIfLast)
            WorkingSpace = AddWorkspace();

        return _loadedWorkspaces.RemoveAll(w => w.Id == id) != 0;
    }

    /// <summary>
    /// Write the solution to file
    /// </summary>
    /// <returns><see langword="true"/> if saving was successful</returns>
    /// <remarks>
    /// <see cref="SavePath"/> must be set prior to calling this method.
    /// </remarks>
    public bool Save()
    {
        if (SavePath is null)
            throw new InvalidOperationException("SavePath is null");

        var path = SavePath.LocalPath;
        Logger.Info($"Saving solution {Title} to {path}");

        var dir = _fileSystem.Path.GetDirectoryName(path) ?? string.Empty;

        try
        {
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            using var fs = _fileSystem.FileStream.New(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            using var writer = new StreamWriter(fs);

            var model = new SolutionModel
            {
                Version = SolutionModel.CurrentApiVersion,
                ReferencedDocuments = ConvertToModels(_referencedItems, dir),
                Styles = StyleManager.Styles.Select(s => s.AsAss()).ToArray(),
                Cps = _cps,
                CpsIncludesWhitespace = _cpsIncludesWhitespace,
                CpsIncludesPunctuation = _cpsIncludesPunctuation,
                UseSoftLinebreaks = _useSoftLinebreaks,
            };

            var content = JsonSerializer.Serialize(model, JsonOptions);
            writer.Write(content);
            return true;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            return false;
        }
    }

    /// <summary>
    /// Parse a saved solution file
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="savePath">Path to the solution file</param>
    /// <returns><see cref="Solution"/> object</returns>
    public static Solution Parse(IFileSystem fileSystem, Uri savePath)
    {
        var path = savePath.LocalPath;
        Logger.Info($"Parsing solution {path}");

        try
        {
            var dir = fileSystem.Path.GetDirectoryName(path) ?? string.Empty;

            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!fileSystem.File.Exists(path))
                throw new FileNotFoundException($"Solution {path} was not found");

            using var fs = fileSystem.FileStream.New(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var reader = new StreamReader(fs);

            var model =
                JsonSerializer.Deserialize<SolutionModel>(reader.ReadToEnd(), JsonOptions)
                ?? throw new InvalidDataException("Solution model deserialization failed");

            // If the solution has no referenced documents, initialize it with one
            var sln = new Solution(fileSystem, model.ReferencedDocuments.Length != 0)
            {
                _savePath = savePath,
            };

            // De-relative the file paths in the solution
            sln._referencedItems.AddRange(
                ConvertFromModels(model.ReferencedDocuments, dir, ref sln._docId)
            );
            sln._cps = model.Cps;
            sln._cpsIncludesWhitespace = model.CpsIncludesWhitespace;
            sln._cpsIncludesPunctuation = model.CpsIncludesPunctuation;
            sln._useSoftLinebreaks = model.UseSoftLinebreaks;

            model
                .Styles.Select(s => Style.FromAss(sln.StyleManager.NextId, s))
                .ToList()
                .ForEach(sln.StyleManager.Add);

            // Load first workspace or create a new one
            var first = sln._referencedItems.FirstOrDefault();
            if (first is null)
            {
                var wsp = sln.AddWorkspace();
                first = new DocumentItem { Id = wsp.Id, Workspace = wsp };
            }
            else
            {
                sln.OpenDocument(first.Id);
            }

            sln.WorkingSpace = first.Workspace!;
            sln.IsSaved = true;
            return sln;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            return new Solution(fileSystem);
        }
    }

    /// <summary>
    /// Load a directory as a solution
    /// </summary>
    /// <param name="fileSystem">Filesystem to use</param>
    /// <param name="dirPath">Path to the directory</param>
    /// <returns>Populated solution</returns>
    public static Solution LoadDirectory(IFileSystem fileSystem, Uri dirPath)
    {
        var root = dirPath.LocalPath;
        Logger.Info($"Generating solution from directory {root}...");
        var sln = new Solution(fileSystem, isEmpty: true);
        if (!fileSystem.Directory.Exists(Path.GetDirectoryName(root)))
        {
            return new Solution(fileSystem, isEmpty: false);
        }

        var fileCount = 0;
        var dirCount = 0;
        var stack = new Stack<(string, ObservableCollection<SolutionItem>)>();
        stack.Push((root, sln._referencedItems));

        while (stack.Count > 0)
        {
            var (currentPath, currentCollection) = stack.Pop();

            // Subdirectories
            foreach (var subDirectory in fileSystem.Directory.EnumerateDirectories(currentPath))
            {
                var dirName = fileSystem.Path.GetFileName(subDirectory);

                // Skip .directories
                if (dirName.StartsWith('.'))
                {
                    Logger.Trace($"Skipping .directory {subDirectory}");
                    continue;
                }
                // Skip directories that don't have subdirectories or ass files
                if (
                    fileSystem.Directory.GetDirectories(subDirectory).Length == 0
                    && fileSystem.Directory.GetFiles(subDirectory, "*.ass").Length == 0
                )
                {
                    Logger.Trace($"Skipping {subDirectory} as it doesn't contain relevant files");
                    continue;
                }

                var dirItem = new DirectoryItem { Id = sln.NextId, Name = dirName };
                currentCollection.Add(dirItem);

                stack.Push((subDirectory, dirItem.Children));
                dirCount++;
            }

            // Files
            foreach (var file in fileSystem.Directory.EnumerateFiles(currentPath, "*.ass"))
            {
                var docItem = new DocumentItem { Id = sln.NextId, Uri = new Uri(file) };
                currentCollection.Add(docItem);
                fileCount++;
            }
        }
        Logger.Info($"Done! Solution contains {dirCount} directories and {fileCount} files");
        return sln.ReferencedItems.Count > 0 ? sln : new Solution(fileSystem, isEmpty: false);
    }

    /// <summary>
    /// Find a referenced <see cref="SolutionItem"/> by ID
    /// </summary>
    /// <param name="id">ID of the item to find</param>
    /// <returns>The item, or <see langword="null"/> if not found</returns>
    /// <remarks>Uses breadth-first search</remarks>
    public SolutionItem? FindItemById(int id)
    {
        var queue = new Queue<SolutionItem>(_referencedItems);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.Id == id)
                return current;

            if (current.Type != SolutionItemType.Directory)
                continue;

            foreach (var child in current.Children)
                queue.Enqueue(child);
        }

        return null;
    }

    /// <summary>
    /// Remove a referenced <see cref="SolutionItem"/> by ID
    /// </summary>
    /// <param name="id">ID of the item to remove</param>
    /// <returns><see langword="true"/> if successfully removed</returns>
    /// <remarks>Uses breadth-first search</remarks>
    private bool RemoveItemById(int id)
    {
        var queue = new Queue<(ObservableCollection<SolutionItem> Parent, SolutionItem Item)>();

        // Seed the queue
        foreach (var item in _referencedItems)
        {
            queue.Enqueue((_referencedItems, item));
        }

        while (queue.Count > 0)
        {
            var (parent, current) = queue.Dequeue();

            if (current.Id == id)
                return parent.Remove(current);

            if (current.Type != SolutionItemType.Directory)
                continue;

            foreach (var item in current.Children)
                queue.Enqueue((current.Children, item));
        }

        return false;
    }

    /// <summary>
    /// Convert from <see cref="SolutionItem"/> to <see cref="SolutionItemModel"/>
    /// </summary>
    /// <param name="items">List of solution items</param>
    /// <param name="dir">Directory for making relative paths</param>
    /// <returns>Array of <see cref="SolutionItemModel"/>s</returns>
    private static SolutionItemModel[] ConvertToModels(IList<SolutionItem> items, string dir)
    {
        return items
            .Where(item =>
                item is not { Type: SolutionItemType.Document, IsSavedToFileSystem: false }
            )
            .Select(item => new SolutionItemModel
            {
                Name = item.Name,
                Type = item.Type,
                RelativePath =
                    item.Type == SolutionItemType.Document
                        ? Path.GetRelativePath(dir, item.Uri!.LocalPath)
                        : null,
                Children =
                    item.Type == SolutionItemType.Directory
                        ? ConvertToModels(item.Children, dir).ToArray()
                        : [],
            })
            .ToArray();
    }

    /// <summary>
    /// Convert from <see cref="SolutionItemModel"/> to <see cref="SolutionItem"/>
    /// </summary>
    /// <param name="models">Array of models</param>
    /// <param name="dir">Directory for resolving relative paths</param>
    /// <param name="nextId">Initial ID</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns>List of <see cref="SolutionItem"/>s</returns>
    private static List<SolutionItem> ConvertFromModels(
        SolutionItemModel[] models,
        string dir,
        ref int nextId
    )
    {
        List<SolutionItem> result = [];

        foreach (var model in models)
        {
            result.Add(
                model.Type switch
                {
                    SolutionItemType.Document => new DocumentItem
                    {
                        Id = nextId++,
                        Name = model.Name,
                        Uri = model.RelativePath is not null
                            ? new Uri(Path.Combine(dir, model.RelativePath))
                            : null,
                    },
                    SolutionItemType.Directory => new DirectoryItem
                    {
                        Id = nextId++,
                        Name = model.Name,
                        Children = new RangeObservableCollection<SolutionItem>(
                            ConvertFromModels(model.Children, dir, ref nextId)
                        ),
                    },
                    _ => throw new InvalidOperationException(nameof(model)),
                }
            );
        }

        return result;
    }

    /// <summary>
    /// Initialize a solution
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="isEmpty">If the solution should be created
    /// without a default <see cref="Workspace"/></param>
    public Solution(IFileSystem fileSystem, bool isEmpty = false)
    {
        _fileSystem = fileSystem;
        _referencedItems = [];
        _loadedWorkspaces = [];
        StyleManager = new StyleManager();

        ReferencedItems = new ReadOnlyObservableCollection<SolutionItem>(_referencedItems);
        LoadedWorkspaces = new ReadOnlyObservableCollection<Workspace>(_loadedWorkspaces);

        if (isEmpty)
            return;

        var id = NextId;
        var defaultWorkspace = new Workspace(new Document(true), id);
        var defaultLink = new DocumentItem { Id = id, Workspace = defaultWorkspace };

        _referencedItems.Add(defaultLink);
        _loadedWorkspaces.Add(defaultWorkspace);
        _workingSpace = defaultWorkspace;
    }
}
