// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.ComponentModel;
using AssCS;
using Holo.Models;

namespace Holo.Configuration;

public interface IConfiguration
{
    /// <summary>
    /// Characters-per-second threshold
    /// </summary>
    /// <remarks>This value may be overloaded by <see cref="Solution.Cps"/>.</remarks>
    uint Cps { get; set; }

    /// <summary>
    /// If whitespace should be included in <see cref="Event.Cps"/> calculation
    /// </summary>
    bool CpsIncludesWhitespace { get; set; }

    /// <summary>
    /// If punctuation should be included in <see cref="Event.Cps"/> calculation
    /// </summary>
    bool CpsIncludesPunctuation { get; set; }

    /// <summary>
    /// Soft linebreaks preference
    /// </summary>
    /// <remarks>This value may be overloaded by <see cref="Solution.UseSoftLinebreaks"/>.</remarks>
    bool UseSoftLinebreaks { get; set; }

    /// <summary>
    /// Whether autosave is enabled
    /// </summary>
    bool AutosaveEnabled { get; set; }

    /// <summary>
    /// Interval between autosave attempts, in seconds
    /// </summary>
    uint AutosaveInterval { get; set; }

    /// <summary>
    /// If whitespace should be included in <see cref="Event.MaxLineWidth"/> calculation
    /// </summary>
    bool LineWidthIncludesWhitespace { get; set; }

    /// <summary>
    /// If punctuation should be included in <see cref="Event.MaxLineWidth"/> calculation
    /// </summary>
    bool LineWidthIncludesPunctuation { get; set; }

    /// <summary>
    /// Current display language
    /// </summary>
    string Culture { get; set; }

    /// <summary>
    /// Display theme to use
    /// </summary>
    Theme Theme { get; set; }

    /// <summary>
    /// Vertical padding in the event grid
    /// </summary>
    uint GridPadding { get; set; }

    /// <summary>
    /// List of user-added repository URLs
    /// </summary>
    ReadOnlyObservableCollection<string> RepositoryUrls { get; }

    /// <summary>
    /// Add a repository
    /// </summary>
    /// <param name="url">Repository to add</param>
    void AddRepositoryUrl(string url);

    /// <summary>
    /// Remove a repository
    /// </summary>
    /// <param name="url">Repository to remove</param>
    /// <returns><see langword="true"/> if successful</returns>
    bool RemoveRepositoryUrl(string url);

    /// <summary>
    /// Write the solution to file
    /// </summary>
    /// <returns><see langword="true"/> if saving was successful</returns>
    bool Save();

    event PropertyChangedEventHandler? PropertyChanged;
}
