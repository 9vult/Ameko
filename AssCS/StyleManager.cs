// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AssCS.History;

namespace AssCS;

/// <summary>
/// Manages the styles used in a document
/// </summary>
public class StyleManager : BindableBase
{
    private readonly RangeObservableCollection<Style> _styles;
    private int _id;

    /// <summary>
    /// Number of styles
    /// </summary>
    public int Count => _styles.Count;

    /// <summary>
    /// An observable collection of the styles currently in the document
    /// </summary>
    public Utilities.ReadOnlyObservableCollection<Style> Styles { get; }

    /// <summary>
    /// Obtain the next available style ID for use in the document
    /// </summary>
    /// <remarks>
    /// <para>
    /// As the style's ID is used for history operations, IDs cannot be reused.
    /// </para><para>
    /// In general, it can be assumed that any given style will retain its
    /// ID throughout its life, however not every ID may be present in the
    /// document at all times. In addition, all copies of a style should
    /// share an ID.
    /// </para><para>
    /// For example, a style that has been edited will have the same ID as
    /// the copy stored in the document's <see cref="History.HistoryManager"/>.
    /// </para>
    /// </remarks>
    public int NextId => _id++;

    /// <summary>
    /// Add a new style to the document
    /// </summary>
    /// <param name="style">The style to add</param>
    /// <exception cref="ArgumentException">
    /// If the <paramref name="style"/> name or ID is already in use
    /// </exception>
    public void Add(Style style)
    {
        if (_styles.Any(s => s.Name == style.Name || s.Id == style.Id))
            throw new ArgumentException(
                $"A style with name={style.Name} or id={style.Id} already exists!"
            );
        _styles.Add(style);
    }

    /// <summary>
    /// Remove a style from the document by name
    /// </summary>
    /// <param name="name">Name of the style to remove</param>
    /// <returns><see langword="true"/> if the style was removed</returns>
    public bool Remove(string name)
    {
        var style = _styles.FirstOrDefault(s => s.Name == name);
        return style is not null && _styles.Remove(style);
    }

    /// <summary>
    /// Remove a style from the document by ID
    /// </summary>
    /// <param name="id">ID of the style to remove</param>
    /// <returns><see langword="true"/> if the style was removed</returns>
    public bool Remove(int id)
    {
        var style = _styles.FirstOrDefault(s => s.Id == id);
        return style is not null && _styles.Remove(style);
    }

    /// <summary>
    /// Add a style to the document, or replace the one with the same name,
    /// if one already exists
    /// </summary>
    /// <param name="style">Style to add</param>
    public void AddOrReplace(Style style)
    {
        Remove(style.Name);
        Add(style);
    }

    /// <summary>
    /// Get a style by name
    /// </summary>
    /// <param name="name">Name of the style</param>
    /// <returns>The style with the requested name</returns>
    /// <exception cref="ArgumentException">
    /// If a style with the given <paramref name="name"/> does not exist
    /// </exception>
    public Style Get(string name)
    {
        var style = _styles.FirstOrDefault(s => s.Name == name);
        if (style is not null)
            return style;
        throw new ArgumentException($"A style with name={name} was not found");
    }

    /// <summary>
    /// Get a style by name
    /// </summary>
    /// <param name="name">Name of the style</param>
    /// <param name="value">The style with the requested name, if it exists</param>
    /// <returns>
    /// <see langword="true"/> if the event exists in the document; otherwise, <see langword="false"/>
    /// </returns>
    public bool TryGet(string name, [MaybeNullWhen(false)] out Style value)
    {
        value = _styles.FirstOrDefault(s => s.Name == name);
        return value is not null;
    }

    /// <summary>
    /// Get a style by ID
    /// </summary>
    /// <param name="id">ID of the style</param>
    /// <returns>The style with the requested ID</returns>
    /// <exception cref="ArgumentException">
    /// If a style with the ID does not exist</exception>
    public Style Get(int id)
    {
        var style = _styles.FirstOrDefault(s => s.Id == id);
        if (style is not null)
            return style;
        throw new ArgumentException($"A style with id={id} was not found");
    }

    /// <summary>
    /// Get a style by ID
    /// </summary>
    /// <param name="id">ID of the style</param>
    /// <param name="value">The style with the requested ID, if it exists</param>
    /// <returns>
    /// <see langword="true"/> if the event exists in the document; otherwise, <see langword="false"/>
    /// </returns>
    public bool TryGet(int id, [MaybeNullWhen(false)] out Style value)
    {
        value = _styles.FirstOrDefault(s => s.Id == id);
        return value is not null;
    }

    #region History

    /// <summary>
    /// Get the current state of the manager for history operations
    /// </summary>
    /// <returns>Manager state</returns>
    internal IReadOnlyList<Style> GetState()
    {
        return _styles.Select(s => s.Clone()).ToList();
    }

    /// <summary>
    /// Restore state from a history operation
    /// </summary>
    /// <param name="commit">Commit to restore from</param>
    internal void RestoreState(Commit commit)
    {
        _styles.ReplaceRange(commit.Styles.Select(s => s.Clone()));
    }

    #endregion History

    /// <summary>
    /// Clear everything and load in a default style
    /// </summary>
    public void LoadDefault()
    {
        _styles.Clear();
        _id = 0;
        Add(new Style(NextId));
    }

    public StyleManager()
    {
        _styles = [];
        Styles = new Utilities.ReadOnlyObservableCollection<Style>(_styles);
    }
}
