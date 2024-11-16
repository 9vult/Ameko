// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using AssCS.History;

namespace AssCS;

/// <summary>
/// A single Advanced Substation Alpha subtitle file
/// </summary>
public class Document
{
    /// <summary>
    /// ASS version of the document
    /// </summary>
    public AssVersion Version { get; internal set; }

    /// <summary>
    /// The document's Event Manager
    /// </summary>
    public EventManager EventManager { get; } = new();

    /// <summary>
    /// The document's Style Manager
    /// </summary>
    public StyleManager StyleManager { get; } = new();

    /// <summary>
    /// The document's History Manager
    /// </summary>
    public HistoryManager HistoryManager { get; } = new();

    /// <summary>
    /// The document's Extradata Manager
    /// </summary>
    public ExtradataManager ExtradataManager { get; } = new();

    /// <summary>
    /// The document's Script Info Manager
    /// </summary>
    public ScriptInfoManager ScriptInfoManager { get; } = new();

    /// <summary>
    /// The document's Aegisub Project Garbage Manager
    /// </summary>
    public GarbageManager GarbageManager { get; } = new();

    /// <summary>
    /// Load the default state
    /// </summary>
    public void LoadDefault()
    {
        StyleManager.LoadDefault();
        EventManager.LoadDefault();
    }

    /// <summary>
    /// A single Advanced Substation Alpha subtitle file
    /// </summary>
    /// <param name="initDefault">If <see cref="LoadDefault"/> should be called automatically</param>
    /// <param name="version">ASS Version</param>
    public Document(bool initDefault, AssVersion version = AssVersion.V400P)
    {
        Version = version;

        if (initDefault)
            LoadDefault();
    }
}

/// <summary>
/// ASS file version
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum AssVersion
{
    /// <summary>
    /// v4.00
    /// </summary>
    V400 = 0,

    /// <summary>
    /// v4.00+
    /// </summary>
    V400P = 1,

    /// <summary>
    /// v4.00++
    /// </summary>
    V400PP = 2,

    /// <summary>
    /// Unknown
    /// </summary>
    UNKNOWN = -1,
}
