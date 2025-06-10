// SPDX-License-Identifier: GPL-3.0-only

using System.Windows.Input;
using AssCS;
using Holo;
using Holo.Configuration;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class StylesManagerWindowViewModel : ViewModelBase
{
    private Style? _selectedGlobalStyle;
    private Style? _selectedSolutionStyle;
    private Style? _selectedDocumentStyle;

    public Solution Solution { get; }
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

    public Style? SelectedSolutionStyle
    {
        get => _selectedSolutionStyle;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSolutionStyle, value);
            this.RaisePropertyChanged(nameof(SolutionButtonsEnabled));
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
    public bool SolutionButtonsEnabled => SelectedSolutionStyle is not null;
    public bool DocumentButtonsEnabled => SelectedDocumentStyle is not null;

    #region Commands
    public ICommand CopyToCommand { get; }
    public ICommand DuplicateCommand { get; }
    public ICommand EditStyleCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand NewStyleCommand { get; }
    #endregion

    public StylesManagerWindowViewModel(IGlobals globals, Solution solution, Document document)
    {
        Globals = globals;
        Solution = solution;
        Document = document;
        DuplicateCommand = CreateDuplicateCommand();
        DeleteCommand = CreateDeleteCommand();
        CopyToCommand = CreateCopyToCommand();
    }
}
