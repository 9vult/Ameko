using Ameko.DataModels;
using Ameko.Services;
using AssCS;
using DynamicData;
using Holo;
using Holo.Data;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class TabItemViewModel : ViewModelBase
    {
        private string _title;
        private FileWrapper _wrapper;
        private readonly int _id;
        private Event? _selectedEvent;
        private int _selectionStart;
        private int _selectionEnd;
        private int _focusLostSelectionEnd;

        private bool _lockASR = true;

        private static readonly Event FALLBACK_EVENT = new Event(-1) { Text="<No Event Selected>" };

        public ObservableCollection<string> Actors { get; private set; }
        public ObservableCollection<string> Effects { get; private set; }
        public bool DisplayActorsColumn
        {
            get => Actors.Count > 0;
        }
        public bool DisplayEffectsColumn
        {
            get => Effects.Count > 0;
        }

        public List<ScalePercentage> ScaleOptions => ScalePercentage.Scales;

        public Interaction<TabItemViewModel, string?> CopySelectedEvents { get; }
        public Interaction<TabItemViewModel, string?> CutSelectedEvents { get; }
        public Interaction<TabItemViewModel, string[]?> Paste { get; }
        public Interaction<Event, Unit> ScrollIntoViewInteraction { get; }
        public Interaction<StyleEditorViewModel, StyleEditorViewModel?> ShowStyleEditor { get; }
        public Interaction<PasteOverWindowViewModel, PasteOverField> ShowPasteOverFieldDialog { get; }

        public ICommand DeleteSelectedCommand { get; }
        public ICommand CutSelectedEventsCommand { get; }
        public ICommand CopySelectedEventsCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand PasteOverCommand { get; }
        public ICommand DuplicateSelectedEventsCommand { get; }
        public ICommand NextOrAddEventCommand { get; }
        public ICommand InsertBeforeCommand { get; }
        public ICommand InsertAfterCommand { get; }
        public ICommand SplitEventCommand { get; }
        public ICommand MergeEventsCommand { get; }
        public ICommand ActivateScriptCommand { get; }
        public ICommand ToggleTagCommand { get; }
        public ICommand EditFileStyleCommand { get; }
        public ICommand ScrollChangeScaleCommand { get; }

        public ICommand StartPlayingCommand { get; }
        public ICommand StopPlayingCommand { get; }
        public ICommand PlaySelectionCommand { get; }
        public ICommand ToggleAutoSeekCommand { get; }

        public ICommand UpdateSubtitlesCommand { get; }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public FileWrapper Wrapper
        {
            get => _wrapper;
            set => this.RaiseAndSetIfChanged(ref _wrapper, value);
        }

        public string Display
        {
            get => $"{Title}{(!Wrapper.UpToDate ? "*" : "")}";
        }

        public int SelectionStart
        {
            get => _selectionStart;
            set => this.RaiseAndSetIfChanged(ref _selectionStart, value);
        }

        public int SelectionEnd
        {
            get => _selectionEnd;
            set => this.RaiseAndSetIfChanged(ref _selectionEnd, value);
        }

        public int FocusLostSelectionEnd
        {
            get => _focusLostSelectionEnd;
            set => this.RaiseAndSetIfChanged(ref _focusLostSelectionEnd, value);
        }

        public bool LockASR
        {
            get => _lockASR;
            set => this.RaiseAndSetIfChanged(ref _lockASR, value);
        }

        public int ID => _id;
        public RangeObservableCollection<Event> Events { get; private set; }
        public int SelectedIndex { get; set; }

        public Event? SelectedEvent
        {
            get
            {
                if (_selectedEvent != null) return _selectedEvent;
                else return FALLBACK_EVENT;
            }
            private set => this.RaiseAndSetIfChanged(ref _selectedEvent, value);
        }

        public void UpdateEventSelection(List<Event> selectedEvents, Event selectedEvent)
        {
            Wrapper.SelectedEventsChanged(selectedEvents, selectedEvent);
            Actors.Clear();
            Actors.AddRange(Wrapper.File.EventManager.Actors);
            Effects.Clear();
            Effects.AddRange(Wrapper.File.EventManager.Effects);
            this.RaisePropertyChanged(nameof(DisplayActorsColumn));
            this.RaisePropertyChanged(nameof(DisplayEffectsColumn));
        }

        private void UpdateEventsCallback(object? sender, EventArgs e)
        {
            // Events.Clear();
            // Events.AddRange(Wrapper.File.EventManager.Ordered);
            Events.ReplaceRange(Wrapper.File.EventManager.Ordered);
        }

        public void UpdateSelectionsOutsideCallback()
        {
            SelectedEvent = Wrapper.SelectedEvent;
        }
        private void UpdateSelectionsCallback(object? sender, EventArgs e)
        {
            SelectedEvent = Wrapper.SelectedEvent;
        }

        public TabItemViewModel(string title, FileWrapper wrapper)
        {
            _title = title;
            _wrapper = wrapper;
            _id = wrapper.ID;

            Events = new RangeObservableCollection<Event>(Wrapper.File.EventManager.Ordered);
            Wrapper.File.EventManager.EventsUpdated += UpdateEventsCallback;
            Wrapper.PropertyChanged += UpdateSelectionsCallback;

            Actors = new ObservableCollection<string>(Wrapper.File.EventManager.Actors);
            Effects = new ObservableCollection<string>(Wrapper.File.EventManager.Effects);

            CopySelectedEvents = new Interaction<TabItemViewModel, string?>();
            CutSelectedEvents = new Interaction<TabItemViewModel, string?>();
            Paste = new Interaction<TabItemViewModel, string[]?>();
            ScrollIntoViewInteraction = new Interaction<Event, Unit>();
            ShowPasteOverFieldDialog = new Interaction<PasteOverWindowViewModel, PasteOverField>();
            ShowStyleEditor = new Interaction<StyleEditorViewModel, StyleEditorViewModel?>();

            DeleteSelectedCommand = ReactiveCommand.Create(() =>
            {
                // TODO: Add checking!
                if (Wrapper.SelectedEvent == null || Wrapper.SelectedEventCollection == null) return;
                Wrapper.Remove(Wrapper.SelectedEventCollection, Wrapper.SelectedEvent);
            });

            CopySelectedEventsCommand = ReactiveCommand.Create(async () => { 
                await CopySelectedEvents.Handle(this); 
            });
            CutSelectedEventsCommand = ReactiveCommand.Create(async () => { await CutSelectedEvents.Handle(this); });
            PasteCommand = ReactiveCommand.Create(() => IOCommandService.PasteLines(Paste, this));
            PasteOverCommand = ReactiveCommand.Create(() => IOCommandService.PasteOverLines(Paste, ShowPasteOverFieldDialog, this));
            DuplicateSelectedEventsCommand = ReactiveCommand.Create(Wrapper.DuplicateSelected);
            InsertBeforeCommand = ReactiveCommand.Create(Wrapper.InsertBeforeSelected);
            InsertAfterCommand = ReactiveCommand.Create(Wrapper.InsertAfterSelected);
            SplitEventCommand = ReactiveCommand.Create(Wrapper.SplitSelected);
            MergeEventsCommand = ReactiveCommand.Create(Wrapper.MergeSelectedAdj);
            NextOrAddEventCommand = ReactiveCommand.Create(Wrapper.NextOrAdd);
            ToggleTagCommand = ReactiveCommand.Create((string tag) =>
            {
                (SelectionStart, SelectionEnd) = Wrapper.ToggleTag(tag, SelectionStart, FocusLostSelectionEnd);
                FocusLostSelectionEnd = SelectionEnd;
            });

            EditFileStyleCommand = ReactiveCommand.Create(async () =>
            {
                if (SelectedEvent == null) return;
                var style = wrapper.File.StyleManager.Get(SelectedEvent.Style);
                if (style == null) return;
                var editor = new StyleEditorViewModel(style, StyleLocation.File);
                await ShowStyleEditor.Handle(editor);
            });

            ScrollChangeScaleCommand = ReactiveCommand.Create((bool positive) =>
            {
                var idx = ScalePercentage.Scales.IndexOf(Wrapper.AVManager.Video.DisplayScale);
                if (idx == 0 && !positive) return;
                if (idx == ScalePercentage.Scales.Count - 1 && positive) return;

                Wrapper.AVManager.Video.DisplayScale = ScalePercentage.Scales[positive ? idx+1 : idx-1];
            });

            StartPlayingCommand = ReactiveCommand.Create(() =>
            {
                Wrapper.AVManager.Video.PlayToEnd();
            });

            StopPlayingCommand = ReactiveCommand.Create(() =>
            {
                Wrapper.AVManager.Video.StopPlaying();
            });

            PlaySelectionCommand = ReactiveCommand.Create(() =>
            {
                if (Wrapper.SelectedEventCollection == null) return;
                Wrapper.AVManager.Video.PlaySelection(Wrapper.SelectedEventCollection);
            });

            ToggleAutoSeekCommand = ReactiveCommand.Create(() =>
            {
                Wrapper.AVManager.Video.IsAutoSeekEnabled = !Wrapper.AVManager.Video.IsAutoSeekEnabled;
            });

            ActivateScriptCommand = ReactiveCommand.Create<string>(async (string scriptName) =>
            {
                await ScriptService.Instance.Execute(scriptName);
            });

            UpdateSubtitlesCommand = ReactiveCommand.Create(() =>
            {
                Wrapper.AVManager.UpdateSubtitles(Wrapper.File);
            });

            // TODO: Maybe not do this this way
            Wrapper.PropertyChanged += (o, e) => { this.RaisePropertyChanged(nameof(Display)); };
            Wrapper.SelectedEventsChanged([Wrapper.File.EventManager.Head], Wrapper.File.EventManager.Head);
            SelectedEvent = Wrapper.File.EventManager.Head;
        }
    }
}
