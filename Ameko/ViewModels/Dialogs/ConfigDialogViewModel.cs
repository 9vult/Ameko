// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using Holo.Configuration;
using Holo.Models;
using Holo.Providers;

namespace Ameko.ViewModels.Dialogs;

public partial class ConfigDialogViewModel(
    IConfiguration config,
    IDictionaryService dictionaryService,
    ICultureService cultureService
) : ViewModelBase
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

    public IConfiguration Config { get; } = config;
    public ICultureService CultureService { get; } = cultureService;
    public IDictionaryService DictionaryService { get; } = dictionaryService;

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

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is not Tuple<T1, T2> other)
                return false;
            if (Item1 is not null)
                return Item1.Equals(other.Item1);
            return other.Item1 is null;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (Item1 is null)
                return 0;
            return EqualityComparer<T1>.Default.GetHashCode(Item1);
        }
    }
}
