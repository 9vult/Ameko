using Ameko.DataModels;
using Ameko.Views;
using Avalonia.Controls;
using Avalonia.Media;
using Holo;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Tomlet;

namespace Ameko.Services
{
    public class LayoutService : INotifyPropertyChanged
    {
        private static readonly Lazy<LayoutService> _instance = new(() => new LayoutService());
        private readonly string layoutsRoot;
        private readonly Dictionary<string, TabLayout> layouts;

        private TabLayout selectedLayout;

        public TabLayout SelectedLayout
        {
            get => selectedLayout;
            private set
            {
                selectedLayout = value;
                OnPropertyChanged(nameof(SelectedLayout));
            }
        }

        public static LayoutService Instance => _instance.Value;
        public ObservableCollection<string> LoadedLayouts { get; private set; }

        public List<TabLayout> Layouts => new(layouts.Values);

        /// <summary>
        /// Get a layout by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TabLayout? Get(string name)
        {
            if (layouts.TryGetValue(name, out TabLayout? value))
                return value;
            return null;
        }

        /// <summary>
        /// Reload the scripts
        /// </summary>
        /// <param name="manual">Was the reload manually triggered</param>
        public void Reload(bool manual)
        {
            HoloContext.Logger.Info($"Reloading layouts", "LayoutService");
            if (!Directory.Exists(layoutsRoot))
            {
                Directory.CreateDirectory(layoutsRoot);
            }

            var previous = selectedLayout?.Name ?? "";

            layouts.Clear();
            LoadedLayouts.Clear();

            layouts.Add("Default", TabLayout.Default);
            LoadedLayouts.Add("Default");

            foreach (var path in Directory.EnumerateFiles(layoutsRoot))
            {
                try
                {
                    if (!System.IO.Path.GetExtension(path).Equals(".toml")) continue;

                    HoloContext.Logger.Info($"Loading layout {path}", "LayoutService");

                    using var reader = new StreamReader(path);
                    var contents = reader.ReadToEnd();
                    TabLayout layout = TomletMain.To<TabLayout>(contents);

                    if (layout == null) continue;

                    var name = layout.Name;
                    layouts[name] = layout;
                    LoadedLayouts.Add(name);
                }
                catch (Exception e)
                {
                    HoloContext.Logger.Error(e.Message.Trim(), "LayoutService");
                    continue;
                }
            }

            if (Get(previous) != null) OnPropertyChanged(nameof(SelectedLayout));
            else SelectedLayout = TabLayout.Default;

            if (manual)
            {
                DisplayMessageBox("Layouts have been reloaded.");
            }
            HoloContext.Logger.Info($"Reloading scripts complete", "ScriptService");
        }

        /// <summary>
        /// Select a layout
        /// </summary>
        /// <param name="layoutName">Name of the layout</param>
        public void SelectLayout(string layoutName)
        {
            var layout = Get(layoutName);
            if (layout == null) return;
            SelectedLayout = layout;
        }

        /// <summary>
        /// Apply the currently selected layout
        /// </summary>
        /// <param name="grid">Layout grid</param>
        /// <param name="video">Video component</param>
        /// <param name="audio">Audio component</param>
        /// <param name="editor">Editor component</param>
        /// <param name="events">Events component</param>
        public void ApplyLayout(ref Grid grid, ref TabItem_VideoView video, ref TabItem_AudioView audio,
            ref TabItem_EditorView editor, ref TabItem_EventsView events)
        {
            var lt = SelectedLayout;

            grid.Children.RemoveAll(grid.Children.OfType<GridSplitter>());

            grid.ColumnDefinitions = new ColumnDefinitions(lt.Columns);
            grid.RowDefinitions = new RowDefinitions(lt.Rows);

            video.IsVisible = lt.Video;
            video.SetValue(Grid.ColumnProperty, lt.VideoColumn);
            video.SetValue(Grid.RowProperty, lt.VideoRow);
            video.SetValue(Grid.ColumnSpanProperty, lt.VideoColumnSpan);
            video.SetValue(Grid.RowSpanProperty, lt.VideoRowSpan);

            audio.IsVisible = lt.Audio;
            audio.SetValue(Grid.ColumnProperty, lt.AudioColumn);
            audio.SetValue(Grid.RowProperty, lt.AudioRow);
            audio.SetValue(Grid.ColumnSpanProperty, lt.AudioColumnSpan);
            audio.SetValue(Grid.RowSpanProperty, lt.AudioRowSpan);

            editor.IsVisible = lt.Editor;
            editor.SetValue(Grid.ColumnProperty, lt.EditorColumn);
            editor.SetValue(Grid.RowProperty, lt.EditorRow);
            editor.SetValue(Grid.ColumnSpanProperty, lt.EditorColumnSpan);
            editor.SetValue(Grid.RowSpanProperty, lt.EditorRowSpan);

            events.IsVisible = lt.Events;
            events.SetValue(Grid.ColumnProperty, lt.EventsColumn);
            events.SetValue(Grid.RowProperty, lt.EventsRow);
            events.SetValue(Grid.ColumnSpanProperty, lt.EventsColumnSpan);
            events.SetValue(Grid.RowSpanProperty, lt.EventsRowSpan);

            foreach (var split in lt.Splitters)
            {
                GridSplitter splitter = new()
                {
                    ResizeDirection = split.Vertical ? GridResizeDirection.Columns : GridResizeDirection.Rows,
                    Background = Brush.Parse("Black")
                };
                splitter.SetValue(Grid.ColumnProperty, split.Column);
                splitter.SetValue(Grid.RowProperty, split.Row);
                splitter.SetValue(Grid.ColumnSpanProperty, split.ColumnSpan);
                splitter.SetValue(Grid.RowSpanProperty, split.RowSpan);
                grid.Children.Add(splitter);
            }
        }

        private static async void DisplayMessageBox(string message)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ameko Layout Service", message, ButtonEnum.Ok);
            await box.ShowAsync();
        }

        private LayoutService()
        {
            layoutsRoot = System.IO.Path.Combine(HoloContext.Directories.AmekoDataHome, "layouts");
            LoadedLayouts = [];
            layouts = [];
            Reload(false);
            selectedLayout = TabLayout.Default;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
