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

    private readonly RangeObservableCollection<Link> _referencedDocuments;
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
    /// All documents referenced by the solution
    /// </summary>
    public ReadOnlyObservableCollection<Link> ReferencedDocuments { get; }

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

    public void SetWorkingSpace(int id)
    {
        var result = _loadedWorkspaces.FirstOrDefault(f => f.Id == id);
        WorkingSpace = result ?? throw new ArgumentOutOfRangeException(nameof(id));
    }

    /// <summary>
    /// Add a blank <see cref="Workspace"/> to the solution
    /// </summary>
    /// <returns>The created workspace</returns>
    public Workspace AddWorkspace()
    {
        Logger.Trace("Adding a new default workspace");
        var space = new Workspace(new Document(true), NextId);
        var link = new Link(space.Id, space);

        _referencedDocuments.Add(link);
        _loadedWorkspaces.Add(space);
        WorkingSpace = space;
        return space;
    }

    /// <summary>
    /// Add a <see cref="Workspace"/> to the solution
    /// </summary>
    /// <param name="space">Workspace to add</param>
    /// <returns>The workspace</returns>
    public Workspace AddWorkspace(Workspace space)
    {
        Logger.Trace($"Adding workspace {space.DisplayTitle}");
        var link = new Link(space.Id, space, space.SavePath);
        _referencedDocuments.Add(link);
        _loadedWorkspaces.Add(space);
        WorkingSpace = space;
        return space;
    }

    /// <summary>
    /// Use a <see cref="Document"/> to add a <see cref="Workspace"/> to the solution
    /// </summary>
    /// <param name="document">Document to add</param>
    /// <param name="savePath">Path the document is saved at</param>
    /// <returns>The created workspace</returns>
    public Workspace AddWorkspace(Document document, Uri savePath)
    {
        var wsp = new Workspace(document, NextId, savePath);
        return AddWorkspace(wsp);
    }

    /// <summary>
    /// Remove a <see cref="Workspace"/> from the solution
    /// </summary>
    /// <param name="id">ID of the workspace to remove</param>
    /// <returns><see langword="true"/> if the space was removed</returns>
    /// <remarks>
    /// If the workspace to remove is the <see cref="WorkingSpace"/>, then
    /// the <see cref="WorkingSpace"/> will be set to any other currently open
    /// <see cref="Workspace"/>. If there are no other open workspaces, a new
    /// workspace will be created and selected.
    /// </remarks>
    public bool RemoveWorkspace(int id)
    {
        Logger.Trace($"Removing workspace {id} from the solution");
        if (WorkingSpaceId == id)
        {
            WorkingSpace =
                _loadedWorkspaces.Count > 1
                    ? _loadedWorkspaces.First(w => w.Id != id)
                    : AddWorkspace();
        }
        _loadedWorkspaces.RemoveAll(d => d.Id == id);
        return _referencedDocuments.RemoveAll(d => d.Id == id) != 0;
    }

    /// <summary>
    /// Open a document in the solution
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
            var matches = _referencedDocuments.Where(l => l.Id == id).ToList();
            if (matches.Count == 0)
            {
                Logger.Error($"Referenced document {id} was not found");
                return -1;
            }

            var link = matches.First();
            if (!link.IsSaved)
            {
                Logger.Error(
                    $"Referenced document {id} could not be opened because it does not exist on disk"
                );
                return -1;
            }
            var parser = new AssParser();
            var doc = parser.Parse(link.Uri!.LocalPath);

            link.Workspace = new Workspace(doc, id, link.Uri);
            _loadedWorkspaces.Add(link.Workspace);
            WorkingSpace = GetWorkspace(id)!;
            return id;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            return -1;
        }
    }

    /// <summary>
    /// Close a document in the workspace
    /// </summary>
    /// <param name="id">ID of the document to close</param>
    /// <param name="replaceIfLast">If the document should be replaced if it's the last open</param>
    /// <returns><see langword="true"/> if the document was closed</returns>
    /// <remarks>
    /// <para>
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
            return _loadedWorkspaces.RemoveAll(d => d.Id == id) != 0;
        if (_loadedWorkspaces.Count > 1)
            WorkingSpace = _loadedWorkspaces.First(w => w.Id != id);
        else if (replaceIfLast)
            WorkingSpace = AddWorkspace();

        return _loadedWorkspaces.RemoveAll(d => d.Id == id) != 0;
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

        try
        {
            var model = new SolutionModel
            {
                Version = SolutionModel.CurrentApiVersion,
                ReferencedDocuments = _referencedDocuments
                    .Where(f => f.IsSaved)
                    .Select(f => Path.GetRelativePath(dir, f.Uri!.LocalPath))
                    .ToArray(),
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
            sln._referencedDocuments.AddRange(
                model.ReferencedDocuments.Select(f => new Link(
                    sln.NextId,
                    null,
                    new Uri(Path.Combine(dir, f))
                ))
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
            var first = sln._referencedDocuments.FirstOrDefault();
            if (first is null)
            {
                var wsp = sln.AddWorkspace();
                first = new Link(wsp.Id, wsp);
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
    /// Initialize a solution
    /// </summary>
    /// <param name="isEmpty">If the solution should be created
    /// without a default <see cref="Workspace"/></param>
    public Solution(bool isEmpty = false)
    {
        _referencedDocuments = [];
        _loadedWorkspaces = [];
        _styleManager = new StyleManager();

        ReferencedDocuments = new ReadOnlyObservableCollection<Link>(_referencedDocuments);
        LoadedWorkspaces = new ReadOnlyObservableCollection<Workspace>(_loadedWorkspaces);

        if (isEmpty)
            return;

        var id = NextId;
        var defaultWorkspace = new Workspace(new Document(true), id);
        var defaultLink = new Link(id, defaultWorkspace, null);

        _referencedDocuments.Add(defaultLink);
        _loadedWorkspaces.Add(defaultWorkspace);
        _workingSpace = defaultWorkspace;
    }
}
