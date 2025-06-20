﻿// SPDX-License-Identifier: MPL-2.0

namespace AssCS;

/// <summary>
/// Manages the script information headers for a document
/// </summary>
public class ScriptInfoManager
{
    private readonly OrderedDictionary<string, string> _data = [];

    /// <summary>
    /// Number of entries in the manager
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Set an info
    /// </summary>
    /// <param name="key">Info name</param>
    /// <param name="value">Value</param>
    public void Set(string key, string value)
    {
        _data[key] = value;
    }

    /// <summary>
    /// Get an info
    /// </summary>
    /// <param name="key">Info to get</param>
    /// <returns>The <paramref name="key"/> value, or <see langword="null"/> if it does not exist</returns>
    public string? Get(string key)
    {
        return _data.GetValueOrDefault(key);
    }

    /// <summary>
    /// Get all the info
    /// </summary>
    /// <returns>Read-only collection of key/value pairs</returns>
    public IReadOnlyCollection<KeyValuePair<string, string>> GetAll()
    {
        return _data;
    }

    /// <summary>
    /// Remove an info
    /// </summary>
    /// <param name="key">Info to remove</param>
    /// <returns><see langword="true"/> if the info was removed</returns>
    public bool Remove(string key)
    {
        return _data.Remove(key, out _);
    }

    /// <summary>
    /// Clear all the infos
    /// </summary>
    public void Clear()
    {
        _data.Clear();
    }

    /// <summary>
    /// Load the default infos for a document
    /// </summary>
    public void LoadDefault()
    {
        Clear();
        Set("Title", "Default File");
        Set("Original Script", "");
        Set("Original Translation", "");
        Set("Original Editing", "");
        Set("Original Timing", "");
        Set("Synch Point", "");
        Set("Script Updated By", "");
        Set("Update Details", "");
        Set("ScriptType", "v4.00+");
        Set("PlayResX", "1920");
        Set("PlayResY", "1080");
        Set("Timer", "0.0000");
        Set("WrapStyle", "0");
        Set("ScaledBorderAndShadow", "yes");
        Set("YCbCr Matrix", "TV.709");
    }
}
