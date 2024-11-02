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

    private readonly RangeObservableCollection<Uri> _referencedFiles = [];
    private readonly RangeObservableCollection<Workspace> _loadedFiles = [];
    private readonly RangeObservableCollection<Style> _styles = [];

    private Uri? _savePath;
    private bool _isSaved;

    private int _fileId = 0;
    private int _styleId = 0;
    private int? _workingFileId = null;

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
    public bool IsSaved { get; private set; }

    /// <summary>
    /// Next available ID for files
    /// </summary>
    internal int NextFileId => _fileId++;

    /// <summary>
    /// Next available ID for styles
    /// </summary>
    internal int NextStyleId => _styleId++;

    /// <summary>
    /// The currently-selected file
    /// </summary>
    public int? WorkingFileId => _workingFileId;

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

    public void Save() { }

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

            var sln = new Solution();
            sln._savePath = filePath;

            // De-relative the file paths in the solution
            sln._referencedFiles.AddRange(
                model.ReferencedFiles.Select(f => new Uri(Path.Combine(dir, f)))
            );
            sln._styles.AddRange(model.Styles.Select(s => Style.FromAss(sln.NextStyleId, s)));
            sln._cps = model.Cps;
            sln._useSoftLinebreaks = model.UseSoftLinebreaks;

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
}
