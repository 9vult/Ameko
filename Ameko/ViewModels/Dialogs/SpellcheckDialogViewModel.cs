// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ameko.DataModels;
using Ameko.Utilities;
using Ameko.ViewModels.Controls;
using AssCS;
using DynamicData;
using Holo;
using Holo.Models;
using Holo.Providers;
using ReactiveUI;
using ChangeType = AssCS.History.ChangeType;

namespace Ameko.ViewModels.Dialogs;

public partial class SpellcheckDialogViewModel : ViewModelBase
{
    private readonly IProjectProvider _projectProvider;

    private IEnumerator<SpellcheckSuggestion> _spellcheckSuggestions;

    private readonly Workspace? _workspace;
    private readonly TabItemViewModel? _tabVm;

    private string _misspelledWord = string.Empty;
    private string _misspelledWordCopy = string.Empty;
    private readonly ObservableCollection<string> _suggestions;

    public string DictionaryInUse { get; }

    public string MisspelledWord
    {
        get => _misspelledWord;
        set
        {
            this.RaiseAndSetIfChanged(ref _misspelledWord, value);
            this.RaisePropertyChanged(nameof(CanChange));
        }
    }

    public ReadOnlyObservableCollection<string> Suggestions { get; }

    public string? SelectedSuggestion
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            this.RaisePropertyChanged(nameof(CanChange));
        }
    }

    public int EventId { get; private set; }

    public bool HaveSuggestions
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool CanChange =>
        MisspelledWord != _misspelledWordCopy || SelectedSuggestion is not null;

    public ICommand ChangeCommand { get; }
    public ICommand FindNextCommand { get; }
    public ICommand AddToProjectCommand { get; }
    public ICommand AddToGlobalsCommand { get; }
    public ICommand AddToBothCommand { get; }

    public SpellcheckDialogViewModel(
        IProjectProvider projectProvider,
        ISpellcheckService spellcheckService,
        ITabFactory tabFactory
    )
    {
        _projectProvider = projectProvider;

        DictionaryInUse = string.Format(
            I18N.Spellcheck.Spellcheck_Label_Dictionary,
            spellcheckService.CurrentLanguage.Name
        );

        _suggestions = [];
        Suggestions = new ReadOnlyObservableCollection<string>(_suggestions);

        _spellcheckSuggestions = spellcheckService.CheckSpelling().GetEnumerator();

        _workspace = _projectProvider.Current.WorkingSpace;
        if (_workspace is not null)
            tabFactory.TryGetViewModel(_workspace, out _tabVm);

        ChangeCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (!CanChange || _workspace is null)
                return;

            var replacement =
                MisspelledWord != _misspelledWordCopy ? MisspelledWord : SelectedSuggestion!;

            var @event = _workspace.Document.EventManager.Get(EventId);
            _workspace.Document.HistoryManager.BeginTransaction(@event);
            @event.Text = @event.Text.Replace(_misspelledWordCopy, replacement);
            _workspace.Commit(@event, ChangeType.Modify);

            await LoadNextSuggestion();
        });

        FindNextCommand = ReactiveCommand.CreateFromTask(async () => await LoadNextSuggestion());

        AddToProjectCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            spellcheckService.AddWordToProject(_misspelledWord);
            // Restart in case it shows up again
            _spellcheckSuggestions = spellcheckService.CheckSpelling().GetEnumerator();
            await LoadNextSuggestion();
        });

        AddToGlobalsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            spellcheckService.AddWordToGlobals(_misspelledWord);
            // Restart in case it shows up again
            _spellcheckSuggestions = spellcheckService.CheckSpelling().GetEnumerator();
            await LoadNextSuggestion();
        });

        AddToBothCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            spellcheckService.AddWordToProject(_misspelledWord);
            spellcheckService.AddWordToGlobals(_misspelledWord);
            // Restart in case it shows up again
            _spellcheckSuggestions = spellcheckService.CheckSpelling().GetEnumerator();
            await LoadNextSuggestion();
        });

        _ = LoadNextSuggestion();
    }

    private async Task LoadNextSuggestion()
    {
        if (!_spellcheckSuggestions.MoveNext())
        {
            HaveSuggestions = false;
            return;
        }
        var suggestion = _spellcheckSuggestions.Current;

        MisspelledWord = suggestion.Word;
        _misspelledWordCopy = suggestion.Word;

        _suggestions.Clear();
        _suggestions.AddRange(suggestion.Suggestions);

        SelectedSuggestion = _suggestions.FirstOrDefault();

        EventId = suggestion.EventId;

        // Scroll and select
        var @event = _workspace?.Document.EventManager.Get(EventId);
        if (@event is not null && _tabVm is not null)
            await _tabVm.ScrollToAndSelectEvent.Handle(@event);

        this.RaisePropertyChanged(nameof(CanChange));
    }
}
