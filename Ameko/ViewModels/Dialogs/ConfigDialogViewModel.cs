// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using Holo.Configuration;
using Holo.Models;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class ConfigDialogViewModel(IConfiguration config) : ViewModelBase
{
    internal static List<Tuple<RichPresenceLevel, string>> RichPresenceLevelChoices =>
        [
            new(RichPresenceLevel.Disabled, I18N.Config.Config_RpcLevel_Disabled),
            new(RichPresenceLevel.Enabled, I18N.Config.Config_RpcLevel_Enabled),
            new(RichPresenceLevel.TimeOnly, I18N.Config.Config_RpcLevel_TimeOnly),
        ];

    internal static List<Tuple<PropagateFields, string>> PropagateFieldsChoices =>
        [
            new(PropagateFields.All, I18N.Config.Config_PropFields_All),
            new(PropagateFields.None, I18N.Config.Config_PropFields_None),
            new(PropagateFields.NonText, I18N.Config.Config_PropFields_NonText),
        ];

    internal static List<Tuple<Theme, string>> ThemeChoices =>
        [
            new(Theme.Default, I18N.Config.Config_Theme_Default),
            new(Theme.Light, I18N.Config.Config_Theme_Light),
            new(Theme.Dark, I18N.Config.Config_Theme_Dark),
        ];

    internal static List<Tuple<SaveFrames, string>> SaveFramesChoices =>
        [
            new(SaveFrames.WithVideo, I18N.Config.Config_SaveFrames_WithVideo),
            new(SaveFrames.WithSubtitles, I18N.Config.Config_SaveFrames_WithSubtitles),
        ];

    public uint Cps
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.Cps;

    public bool CpsIncludesWhitespace
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.CpsIncludesWhitespace;

    public bool CpsIncludesPunctuation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.CpsIncludesPunctuation;

    public bool UseSoftLinebreaks
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.UseSoftLinebreaks;

    public int DefaultLayer
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.DefaultLayer;

    public bool AutosaveEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.AutosaveEnabled;

    public uint AutosaveInterval
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.AutosaveInterval;

    public bool LineWidthIncludesWhitespace
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.LineWidthIncludesWhitespace;

    public bool LineWidthIncludesPunctuation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.LineWidthIncludesPunctuation;

    public RichPresenceLevel RichPresenceLevel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.RichPresenceLevel;

    public SaveFrames SaveFrames
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.SaveFrames;

    public string Culture
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.Culture;

    public string SpellcheckCulture
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.SpellcheckCulture;

    public Theme Theme
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.Theme;

    public uint GridPadding
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.GridPadding;

    public decimal EditorFontSize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.EditorFontSize;

    public decimal GridFontSize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.GridFontSize;

    public decimal ReferenceFontSize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.ReferenceFontSize;

    public PropagateFields PropagateFields
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = config.PropagateFields;

    /// <summary>
    /// A janky reimplementation of a tuple since binding requires `get;` properties
    /// </summary>
    /// <param name="item1">First item in the tuple</param>
    /// <param name="item2">Second item in the tuple</param>
    /// <typeparam name="T1">Type of <paramref name="item1"/></typeparam>
    /// <typeparam name="T2">Type of <paramref name="item2"/></typeparam>
    internal class Tuple<T1, T2>(T1 item1, T2 item2)
    {
        /// <summary>
        /// The first item in the tuple
        /// </summary>
        public T1 Item1 { get; } = item1;

        /// <summary>
        /// The second item in the tuple
        /// </summary>
        public T2 Item2 { get; } = item2;
    }
}
