﻿// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive.Linq;
using System.Windows.Input;
using Ameko.ViewModels.Dialogs;
using AssCS;
using Holo.Configuration;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class StyleEditorWindowViewModel : ViewModelBase
{
    private readonly Style _backupStyle;
    private readonly StyleManager _styleManager;
    private readonly Document? _document;
    private string _styleName;

    public Interaction<ColorDialogViewModel, Color?> ShowColorDialog { get; }
    public ICommand EditColorCommand { get; }

    public Style Style { get; init; }

    public Color PrimaryColor => Style.PrimaryColor;
    public Color SecondaryColor => Style.SecondaryColor;
    public Color OutlineColor => Style.OutlineColor;
    public Color ShadowColor => Style.ShadowColor;

    /// <summary>
    /// Editable style name
    /// </summary>
    public string StyleName
    {
        get => _styleName;
        set => this.RaiseAndSetIfChanged(ref _styleName, value);
    }

    /// <summary>
    /// Determines if the <see cref="StyleName"/> is invalid
    /// </summary>
    public bool IsNameInvalid =>
        _styleName != _backupStyle.Name
        && (string.IsNullOrWhiteSpace(_styleName) || _styleManager.TryGet(_styleName, out _));

    /// <summary>
    /// Commit a style name change
    /// </summary>
    /// <remarks>
    /// If the style is from a <see cref="Document"/>,
    /// <see cref="EventManager.ChangeStyle(string, string)"/> will be called.
    /// </remarks>
    /// <returns><see langword="true"/> if we are good to go</returns>
    public bool CommitNameChange()
    {
        if (IsNameInvalid)
            return false;
        if (StyleName == _backupStyle.Name)
            return true;

        Style.Name = StyleName;
        _document?.EventManager.ChangeStyle(_backupStyle.Name, StyleName);

        return true;
    }

    /// <summary>
    /// Initialize a style editor
    /// </summary>
    /// <param name="persistence">Application persistence</param>
    /// <param name="style">Style being edited</param>
    /// <param name="manager">Manager the <paramref name="style"/> belongs to</param>
    /// <param name="document">Document the manager belongs to, if applicable</param>
    public StyleEditorWindowViewModel(
        IPersistence persistence,
        Style style,
        StyleManager manager,
        Document? document
    )
    {
        Style = style;
        _styleName = Style.Name;
        _backupStyle = Style.Clone();
        _styleManager = manager;
        _document = document;

        ShowColorDialog = new Interaction<ColorDialogViewModel, Color?>();

        EditColorCommand = ReactiveCommand.CreateFromTask(
            async (Color color) =>
            {
                var vm = new ColorDialogViewModel(persistence, color);
                _ = await ShowColorDialog.Handle(vm);

                // Get the buttons to update
                this.RaisePropertyChanged(nameof(PrimaryColor));
                this.RaisePropertyChanged(nameof(SecondaryColor));
                this.RaisePropertyChanged(nameof(OutlineColor));
                this.RaisePropertyChanged(nameof(ShadowColor));
            }
        );
    }
}
