// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using Holo.Configuration;
using Holo.Models;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class ConfigDialogViewModel(IConfiguration config) : ViewModelBase
{
    public static readonly List<(RichPresenceLevel, string)> RichPresenceLevelChoices =
    [
        (RichPresenceLevel.Disabled, I18N.Config.Config_RpcLevel_Disabled),
        (RichPresenceLevel.Enabled, I18N.Config.Config_RpcLevel_Enabled),
        (RichPresenceLevel.TimeOnly, I18N.Config.Config_RpcLevel_TimeOnly),
    ];

    public static readonly List<(PropagateFields, string)> PropagateFieldsChoices =
    [
        (PropagateFields.All, I18N.Config.Config_PropFields_All),
        (PropagateFields.None, I18N.Config.Config_PropFields_None),
        (PropagateFields.NonText, I18N.Config.Config_PropFields_NonText),
    ];

    public static readonly List<(Theme, string)> ThemeChoices =
    [
        (Theme.Default, I18N.Config.Config_Theme_Default),
        (Theme.Light, I18N.Config.Config_Theme_Light),
        (Theme.Dark, I18N.Config.Config_Theme_Dark),
    ];

    public static readonly List<(SaveFrames, string)> SaveFramesChoices =
    [
        (SaveFrames.WithVideo, I18N.Config.Config_SaveFrames_WithVideo),
        (SaveFrames.WithSubtitles, I18N.Config.Config_SaveFrames_WithSubtitles),
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
}
