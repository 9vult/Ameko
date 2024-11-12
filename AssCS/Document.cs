// SPDX-License-Identifier: MPL-2.0

using AssCS.History;

namespace AssCS;

/// <summary>
/// A single Advanced Substation Alpha subtitle file
/// </summary>
public class Document
{
    private AssVersion _version;
    private readonly EventManager _eventManager = new();
    private readonly StyleManager _styleManager = new();
    private readonly HistoryManager _historyManager = new();
    private readonly ExtradataManager _extradataManager = new();
    private readonly ScriptInfoManager _scriptInfoManager = new();
    private readonly GarbageManager _garbageManager = new();

    /// <summary>
    /// ASS version of the document
    /// </summary>
    public AssVersion Version
    {
        get => _version;
        internal set => _version = value;
    }

    /// <summary>
    /// The document's Event Manager
    /// </summary>
    public EventManager EventManager => _eventManager;

    /// <summary>
    /// The document's Style Manager
    /// </summary>
    public StyleManager StyleManager => _styleManager;

    /// <summary>
    /// The document's History Manager
    /// </summary>
    public HistoryManager HistoryManager => _historyManager;

    /// <summary>
    /// The document's Extradata Manager
    /// </summary>
    public ExtradataManager ExtradataManager => _extradataManager;

    /// <summary>
    /// The document's Script Info Manager
    /// </summary>
    public ScriptInfoManager ScriptInfoManager => _scriptInfoManager;

    /// <summary>
    /// The document's Aegisub Project Garbage Manager
    /// </summary>
    public GarbageManager GarbageManager => _garbageManager;

    /// <summary>
    /// Load the default state
    /// </summary>
    public void LoadDefault()
    {
        _styleManager.LoadDefault();
        _eventManager.LoadDefault();
    }

    /// <summary>
    /// A single Advanced Substation Alpha subtitle file
    /// </summary>
    /// <param name="initDefault">If <see cref="LoadDefault"/> should be called automatically</param>
    /// <param name="version">ASS Version</param>
    public Document(bool initDefault, AssVersion version = AssVersion.V400P)
    {
        _version = version;

        if (initDefault)
            LoadDefault();
    }
}

/// <summary>
/// ASS file version
/// </summary>
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
