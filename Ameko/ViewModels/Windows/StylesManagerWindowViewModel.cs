// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Windows.Input;
using AssCS;
using Holo;
using Holo.Configuration;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class StylesManagerWindowViewModel : ViewModelBase
{
    private readonly IPersistence _persistence;

    private Style? _selectedGlobalStyle;
    private Style? _selectedProjectStyle;
    private Style? _selectedDocumentStyle;

    public Project Project { get; }
    public Document Document { get; }
    public IGlobals Globals { get; }

    public Style? SelectedGlobalStyle
    {
        get => _selectedGlobalStyle;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedGlobalStyle, value);
            this.RaisePropertyChanged(nameof(GlobalButtonsEnabled));
        }
    }

    public Style? SelectedProjectStyle
    {
        get => _selectedProjectStyle;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedProjectStyle, value);
            this.RaisePropertyChanged(nameof(ProjectButtonsEnabled));
        }
    }

    public Style? SelectedDocumentStyle
    {
        get => _selectedDocumentStyle;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDocumentStyle, value);
            this.RaisePropertyChanged(nameof(DocumentButtonsEnabled));
        }
    }

    public bool GlobalButtonsEnabled => SelectedGlobalStyle is not null;
    public bool ProjectButtonsEnabled => SelectedProjectStyle is not null;
    public bool DocumentButtonsEnabled => SelectedDocumentStyle is not null;

    public Interaction<StyleEditorWindowViewModel, Unit> ShowStyleEditorWindow { get; }

    #region Commands
    public ICommand CopyToCommand { get; }
    public ICommand DuplicateCommand { get; }
    public ICommand EditStyleCommand { get; }
    public ICommand DeleteCommand { get; }
    // public ICommand NewStyleCommand { get; } TODO: New Style command
    #endregion

    public StylesManagerWindowViewModel(
        IPersistence persistence,
        IGlobals globals,
        Project project,
        Document document
    )
    {
        _persistence = persistence;

        Globals = globals;
        Project = project;
        Document = document;
        DuplicateCommand = CreateDuplicateCommand();
        DeleteCommand = CreateDeleteCommand();
        CopyToCommand = CreateCopyToCommand();
        EditStyleCommand = CreateEditStyleCommand();

        ShowStyleEditorWindow = new Interaction<StyleEditorWindowViewModel, Unit>();
    }
}
