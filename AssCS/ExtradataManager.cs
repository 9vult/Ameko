// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using AssCS.History;

namespace AssCS;

/// <summary>
/// Manages the extradata linked in a document
/// </summary>
public class ExtradataManager
{
    private readonly List<ExtradataEntry> _extradata = [];

    /// <summary>
    /// The next available extradata ID
    /// </summary>
    public int NextId
    {
        get => field++;
        internal set;
    }

    /// <summary>
    /// Add an extradata entry to the document
    /// </summary>
    /// <param name="key">Key of the entry</param>
    /// <param name="value">Base64-encoded value</param>
    /// <returns>ID of the entry</returns>
    /// <remarks>
    /// If an identical entry already exists in the document, then the ID of
    /// the pre-existing entry will be returned, and a duplicate entry
    /// will not be added.
    /// </remarks>
    public int Add(string key, string value)
    {
        foreach (var ed in _extradata.Where(ed => ed.Key == key && ed.Value == value))
        {
            return ed.Id;
        }

        int id = NextId;
        _extradata.Add(new ExtradataEntry(id, 0, key, value));
        return id;
    }

    /// <summary>
    /// Add an extradata entry to the document
    /// </summary>
    /// <param name="entry">Entry to add</param>
    /// <returns>ID of the entry</returns>
    /// <remarks>
    /// No duplicate checking is performed.
    /// </remarks>
    public int Add(ExtradataEntry entry)
    {
        _extradata.Add(entry);
        return entry.Id;
    }

    /// <summary>
    /// Get extradatas by ID
    /// </summary>
    /// <param name="ids">Collection of IDs</param>
    /// <returns>Read-only collection of extradatas</returns>
    public ReadOnlyCollection<ExtradataEntry> Get(IEnumerable<int> ids)
    {
        return new ReadOnlyCollection<ExtradataEntry>(
            _extradata.Where(e => ids.Contains(e.Id)).ToList()
        );
    }

    /// <summary>
    /// Get all the extradata entries
    /// </summary>
    /// <returns>Read-only collection of extradatas</returns>
    public ReadOnlyCollection<ExtradataEntry> Get()
    {
        return new ReadOnlyCollection<ExtradataEntry>(_extradata);
    }

    /// <summary>
    /// Sort the extradata
    /// </summary>
    public void Sort()
    {
        _extradata.Sort();
    }

    /// <summary>
    /// Set the value of some extradata
    /// </summary>
    /// <param name="line">Line the extradata is linked to</param>
    /// <param name="key">Entry key</param>
    /// <param name="value">New entry value</param>
    /// <param name="delete">Whether to delete</param>
    public void SetValue(Event line, string key, string value, bool delete)
    {
        List<int> toErase = [];
        var dirty = false;
        var found = false;

        var entries = Get(line.LinkedExtradatas);

        foreach (var entry in entries)
        {
            if (entry.Key == key)
            {
                if (!delete && entry.Value == value)
                    found = true;
            }
            else
            {
                toErase.Add(entry.Id);
                dirty = true;
            }
        }

        // The key is already set
        if (found && !dirty)
            return;

        foreach (var id in toErase)
        {
            line.LinkedExtradatas.Remove(id);
        }

        if (!delete && !found)
            line.LinkedExtradatas.Add(Add(key, value));
    }

    /// <summary>
    /// Clean the extradata of a document
    /// </summary>
    /// <param name="document">Document to clean</param>
    /// <remarks>
    /// Unused, duplicated, and expired extradata will be removed
    /// </remarks>
    public void Clean(Document document)
    {
        if (_extradata.Count == 0)
            return;

        HashSet<int> usedIds = [];
        foreach (var line in document.EventManager.Events)
        {
            if (line.LinkedExtradatas.Count == 0)
                continue;

            // Find the id for each key
            Dictionary<string, int> usedKeys = _extradata
                .Where(e => line.LinkedExtradatas.Contains(e.Id))
                .ToDictionary(e => e.Key, e => e.Id);
            usedIds = [.. usedKeys.Values];

            // Check for duplicated or missing keys
            if (usedKeys.Count == line.LinkedExtradatas.Count)
                continue;

            line.LinkedExtradatas = usedKeys.Select(k => k.Value).ToList();
            line.LinkedExtradatas.Sort();
        }

        foreach (var e in _extradata)
        {
            if (!usedIds.Contains(e.Id))
                e.Expiration = 0;
            else
                e.Expiration += 1;
        }

        // Erase unused entries
        if (usedIds.Count != _extradata.Count)
            _extradata.RemoveAll(e => e.Expiration >= 10);
    }

    #region History

    /// <summary>
    /// Get the current state of the manager for history operations
    /// </summary>
    /// <returns>Manager state</returns>
    internal IReadOnlyList<ExtradataEntry> GetState()
    {
        return _extradata.Select(e => e.Clone()).ToList();
    }

    /// <summary>
    /// Restore state from a history operation
    /// </summary>
    /// <param name="commit">Commit to restore from</param>
    internal void RestoreState(Commit commit)
    {
        _extradata.Clear();
        _extradata.AddRange(commit.Extradata.Select(e => e.Clone()));
    }

    #endregion History
}
