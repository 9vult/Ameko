// SPDX-License-Identifier: MPL-2.0

using AssCS.History;

namespace AssCS;

/// <summary>
/// A single Advanced Substation Alpha subtitle file
/// </summary>
public class Document(AssVersion version = AssVersion.V400P)
{
    private readonly AssVersion _version = version;
    private readonly EventManager _eventManager = new();
    private readonly StyleManager _styleManager = new();
    private readonly HistoryManager _historyManager = new();

    /// <summary>
    /// ASS version of the document
    /// </summary>
    public AssVersion Version => _version;

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
