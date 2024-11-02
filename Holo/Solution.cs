// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using AssCS;
using Holo.Models;
using NLog;
using Tomlet;
using Tomlet.Exceptions;

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
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public Solution(bool isEmpty = false)
    {
        _referencedFiles = [];
        _loadedFiles = [];
        _styleManager = new();

        if (!isEmpty)
        {
            var id = NextFileId;
            var defaultWorkspace = new Workspace(new Document(), id);
            var defaultLink = new Link(id, defaultWorkspace, null);

            _referencedFiles.Add(defaultLink);
            _loadedFiles.Add(defaultWorkspace);
            _workingFileId = id;
        }
    }

    private readonly RangeObservableCollection<Link> _referencedFiles;
    private readonly RangeObservableCollection<Workspace> _loadedFiles;
    private readonly StyleManager _styleManager;

    private Uri? _savePath;
    private bool _isSaved;

    private int _fileId = 0;
    private int _workingFileId = 0;

    private int _cps = 0;
    private bool? _useSoftLinebreaks = null;

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
    /// Next available ID for files
    /// </summary>
    internal int NextFileId => _fileId++;

    /// <summary>
    /// The currently-selected file ID
    /// </summary>
    public int WorkingFileId
    {
        get => _workingFileId;
        set => SetProperty(ref _workingFileId, value);
    }

    /// <summary>
    /// The currently-loaded file
    /// </summary>
    public Workspace? WorkingFile => _loadedFiles.First(f => f.Id == _workingFileId);

    /// <summary>
    /// Workspace-scoped characters-per-second threshold
    /// </summary>
    public int Cps
    {
        get => _cps;
        set
        {
            SetProperty(ref _cps, value);
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
    /// Write the solution to file
    /// </summary>
    /// <returns><see langword="true"/> if saving was successful</returns>
    /// <remarks>
    /// <see cref="SavePath"/> must be set prior to calling this method.
    /// </remarks>
    public bool Save()
    {
        if (SavePath is null)
            return false;

        logger.Info($"Saving solution {Title}");

        var fp = SavePath.LocalPath;
        var dir = Path.GetDirectoryName(fp) ?? string.Empty;

        try
        {
            var model = new SolutionModel
            {
                Version = SolutionModel.CURRENT_API_VERSION,
                ReferencedFiles = _referencedFiles
                    .Where(f => f.IsSaved)
                    .Select(f => Path.GetRelativePath(dir, f.Uri!.LocalPath))
                    .ToList(),
                Styles = _styleManager.Styles.Select(s => s.AsAss()).ToList(),
                Cps = _cps,
                UseSoftLinebreaks = _useSoftLinebreaks,
            };

            using var writer = new StreamWriter(fp, false);
            var content = TomletMain.TomlStringFrom(model);
            writer.Write(content);
            return true;
        }
        catch (TomlException te)
        {
            logger.Error(te);
            return false;
        }
        catch (IOException ioe)
        {
            logger.Error(ioe);
            return false;
        }
    }

    /// <summary>
    /// Parse a saved solution file
    /// </summary>
    /// <param name="filePath">Path to the solution file</param>
    /// <returns><see cref="Solution"/> object</returns>
    public static Solution Parse(Uri filePath)
    {
        var fp = filePath.LocalPath;
        var dir = Path.GetDirectoryName(fp) ?? string.Empty;

        try
        {
            using var reader = new StreamReader(fp);
            var model = TomletMain.To<SolutionModel>(reader.ReadToEnd());

            // If the solution has no referenced files, initialize it with one
            var sln = new Solution(model.ReferencedFiles.Count != 0) { _savePath = filePath };

            // De-relative the file paths in the solution
            sln._referencedFiles.AddRange(
                model.ReferencedFiles.Select(f => new Link(
                    sln.NextFileId,
                    null,
                    new Uri(Path.Combine(dir, f))
                ))
            );
            sln._cps = model.Cps;
            sln._useSoftLinebreaks = model.UseSoftLinebreaks;

            model
                .Styles.Select(s => Style.FromAss(sln._styleManager.NextId, s))
                .ToList()
                .ForEach(sln._styleManager.Add);

            sln.WorkingFileId = sln._referencedFiles.First().Id;
            sln.IsSaved = true;
            return sln;
        }
        catch (TomlException te)
        {
            logger.Error(te);
            return new Solution();
        }
        catch (IOException ioe)
        {
            logger.Error(ioe);
            return new Solution();
        }
    }

    /// <summary>
    /// A simple link between a <see cref="Workspace"/> and a <see cref="Uri"/>
    /// </summary>
    /// <param name="id">Workspace ID</param>
    /// <param name="workspace">The linked workspace</param>
    /// <param name="uri">The linked URI</param>
    private struct Link(int id, Workspace? workspace = null, Uri? uri = null)
    {
        public int Id = id;
        public Workspace? Workspace = workspace;
        public Uri? Uri = uri;

        /// <summary>
        /// Indicates whether the <see cref="Workspace"/>
        /// is (or can be) read from disk
        /// </summary>
        public readonly bool IsSaved => Uri != null;

        /// <summary>
        /// Indicated whether the <see cref="Workspace"/>
        /// is currently loaded in the <see cref="Solution"/>
        /// </summary>
        public readonly bool IsLoaded => Workspace != null;
    }
}
