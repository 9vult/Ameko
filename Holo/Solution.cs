// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
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
    private readonly StyleManager _styleManager;

    private Uri? _savePath;
    private bool _isSaved;

    private int _docId = 1;
    private Workspace _workingSpace;

    private int? _cps;
    private bool? _cpsIncludesWhitespace;
    private bool? _cpsIncludesPunctuation;
    private bool? _useSoftLinebreaks;

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
    public StyleManager StyleManager => _styleManager;

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
    internal int NextId => _docId++;

    /// <summary>
    /// The currently-selected workspace ID
    /// </summary>
    public int WorkingSpaceId => _workingSpace.Id;

    /// <summary>
    /// The currently-loaded workspace
    /// </summary>
    public Workspace WorkingSpace
    {
        get => _workingSpace;
        set
        {
            SetProperty(ref _workingSpace, value);
            RaisePropertyChanged(nameof(WorkingSpaceId));
        }
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
    /// Solution title/name
    /// </summary>
    public string Title =>
        SavePath is not null
            ? Path.GetFileNameWithoutExtension(SavePath.LocalPath)
            : $"Default Solution";

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
        if (WorkingSpaceId == id)
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
            var document = parser.Parse(item.Uri!.LocalPath);

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

        if (WorkingSpaceId != id)
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
        if (string.IsNullOrEmpty(SavePath?.LocalPath))
            return false;

        var fp = SavePath.LocalPath;
        using var writer = new StreamWriter(fp, false);
        return Save(writer, SavePath);
    }

    /// <summary>
    /// Write the solution to file
    /// </summary>
    /// <param name="writer">Writer to write to</param>
    /// <param name="savePath">Path the solution for relative filepath parsing</param>
    /// <returns><see langword="true"/> if saving was successful</returns>
    public bool Save(TextWriter writer, Uri savePath)
    {
        Logger.Info($"Saving solution {Title}");

        var fp = savePath.LocalPath;
        var dir = Path.GetDirectoryName(fp) ?? string.Empty;

        var items = new List<SolutionItemModel>();

        try
        {
            var model = new SolutionModel
            {
                Version = SolutionModel.CurrentApiVersion,
                ReferencedDocuments = ConvertToModels(_referencedItems, dir),
                Styles = _styleManager.Styles.Select(s => s.AsAss()).ToArray(),
                Cps = _cps,
                CpsIncludesWhitespace = _cpsIncludesWhitespace,
                CpsIncludesPunctuation = _cpsIncludesPunctuation,
                UseSoftLinebreaks = _useSoftLinebreaks,
            };

            var content = JsonSerializer.Serialize(model, JsonOptions);
            writer.Write(content);
            return true;
        }
        catch (JsonException je)
        {
            Logger.Error(je);
            return false;
        }
        catch (IOException ioe)
        {
            Logger.Error(ioe);
            return false;
        }
    }

    /// <summary>
    /// Parse a saved solution file
    /// </summary>
    /// <param name="filePath">Path to the solution file</param>
    /// <returns></returns>
    public static Solution Parse(Uri filePath)
    {
        using var reader = new StreamReader(filePath.LocalPath);
        return Parse(reader, filePath);
    }

    /// <summary>
    /// Parse a saved solution file
    /// </summary>
    /// <param name="reader">Reader to read the solution from</param>
    /// <param name="filePath">Path to the solution file for relative filepath parsing</param>
    /// <returns><see cref="Solution"/> object</returns>
    public static Solution Parse(TextReader reader, Uri filePath)
    {
        var dir = Path.GetDirectoryName(filePath.LocalPath) ?? string.Empty;

        try
        {
            var model =
                JsonSerializer.Deserialize<SolutionModel>(reader.ReadToEnd(), JsonOptions)
                ?? throw new InvalidDataException("Solution model serialization failed");

            // If the solution has no referenced documents, initialize it with one
            var sln = new Solution(model.ReferencedDocuments.Length != 0) { _savePath = filePath };

            // De-relative the file paths in the solution
            sln._referencedItems.AddRange(
                ConvertFromModels(model.ReferencedDocuments, dir, ref sln._docId)
            );
            sln._cps = model.Cps;
            sln._cpsIncludesWhitespace = model.CpsIncludesWhitespace;
            sln._cpsIncludesPunctuation = model.CpsIncludesPunctuation;
            sln._useSoftLinebreaks = model.UseSoftLinebreaks;

            model
                .Styles.Select(s => Style.FromAss(sln._styleManager.NextId, s))
                .ToList()
                .ForEach(sln._styleManager.Add);

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
        catch (JsonException je)
        {
            Logger.Error(je);
            return new Solution();
        }
        catch (IOException ioe)
        {
            Logger.Error(ioe);
            return new Solution();
        }
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
        List<SolutionItemModel> result = [];
        result.AddRange(
            from item in items
            where item is not { Type: SolutionItemType.Document, IsSavedToFileSystem: false }
            select new SolutionItemModel
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
            }
        );
        return result.ToArray();
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
    /// <param name="isEmpty">If the solution should be created
    /// without a default <see cref="Workspace"/></param>
    public Solution(bool isEmpty = false)
    {
        _referencedItems = [];
        _loadedWorkspaces = [];
        _styleManager = new StyleManager();

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
