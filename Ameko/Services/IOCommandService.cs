using Ameko.DataModels;
using Ameko.ViewModels;
using AssCS;
using AssCS.IO;
using Holo;
using MsBox.Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ameko.Services
{
    public class IOCommandService
    {
        /// <summary>
        /// Display the About Ameko dialog box
        /// </summary>
        /// <param name="interaction"></param>
        public static async void DisplayAboutBox(Interaction<AboutWindowViewModel, AboutWindowViewModel?> interaction)
        {
            var about = new AboutWindowViewModel();
            await interaction.Handle(about);
        }

        /// <summary>
        /// Display the Styles Manager
        /// </summary>
        /// <param name="interaction"></param>
        public static async void DisplayStylesManager(Interaction<StylesManagerViewModel, StylesManagerViewModel?> interaction, MainViewModel vm)
        {
            var manager = new StylesManagerViewModel();
            await interaction.Handle(manager);
        }

        /// <summary>
        /// Display the Open Subtitle file dialog
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="vm"></param>
        public static async void DisplayOpenSubtitleFileDialog(Interaction<MainViewModel, Uri?> interaction, MainViewModel vm)
        {
            var uri = await interaction.Handle(vm);
            if (uri == null) return;
            HoloContext.Instance.Workspace.AddFileToWorkspace(uri);
        }

        /// <summary>
        /// Save the file, or display the Save As dialog
        /// </summary>
        /// <param name="interaction"></param>
        public static async Task<bool> SaveOrDisplaySaveAsDialog(Interaction<FileWrapper, Uri?> interaction, FileWrapper? specifiedFile = null)
        {
            FileWrapper workingFile;
            if (specifiedFile == null) workingFile = HoloContext.Instance.Workspace.WorkingFile;
            else workingFile = specifiedFile;
            if (workingFile == null) return false;

            Uri uri;
            if (workingFile.FilePath == null)
            {
                uri = await interaction.Handle(workingFile);
                if (uri == null) return false;
                HoloContext.Instance.Workspace.ReferencedFiles.Where(f => f.Id == workingFile.ID).Single().Path = uri.LocalPath;
            }
            else
            {
                uri = workingFile.FilePath;
            }
            var writer = new AssWriter(workingFile.File, uri.LocalPath, AmekoInfo.Instance);
            writer.Write(false);
            workingFile.UpToDate = true;
            return true;
        }

        /// <summary>
        /// Display the Save As dialog
        /// </summary>
        /// <param name="interaction"></param>
        public static async void DisplaySaveAsDialog(Interaction<FileWrapper, Uri?> interaction)
        {
            var workingFile = HoloContext.Instance.Workspace.WorkingFile;
            if (workingFile == null) return;

            var uri = await interaction.Handle(workingFile);
            if (uri == null) return;

            var writer = new AssWriter(workingFile.File, uri.LocalPath, AmekoInfo.Instance);
            writer.Write(false);
            workingFile.UpToDate = true;

            var reffile = HoloContext.Instance.Workspace.ReferencedFiles.Where(f => f.Id == workingFile.ID).Single();
            reffile.Path = uri.LocalPath;
        }

        /// <summary>
        /// Display the Export Save As dialog
        /// </summary>
        /// <param name="interaction"></param>
        public static async void DisplayExportDialog(Interaction<FileWrapper, Uri?> interaction)
        {
            var workingFile = HoloContext.Instance.Workspace.WorkingFile;
            if (workingFile == null) return;

            var uri = await interaction.Handle(workingFile);
            if (uri == null) return;

            var writer = new TextWriter(workingFile.File, uri.LocalPath, AmekoInfo.Instance);
            writer.Write(false);
        }

        /// <summary>
        /// Display the Open File dialog for Workspaces
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="vm"></param>
        public static async void DisplayWorkspaceOpenDialog(Interaction<MainViewModel, Uri?> interaction, MainViewModel vm)
        {
            var uri = await interaction.Handle(vm);
            if (uri == null) return;

            // TODO: save prompts and stuff!
            HoloContext.Instance.Workspace.OpenWorkspaceFile(uri);
        }

        /// <summary>
        /// Save the workspace or display the Save As dialog for it
        /// </summary>
        /// <param name="interaction"></param>
        public static async void WorkspaceSaveOrDisplaySaveAsDialog(Interaction<Workspace, Uri?> interaction)
        {
            var workspace = HoloContext.Instance.Workspace;
            Uri uri;
            if (workspace.FilePath == null)
            {
                uri = await interaction.Handle(workspace);
                if (uri == null) return;
            }
            else
            {
                uri = workspace.FilePath;
            }
            workspace.WriteWorkspaceFile(uri);
        }

        /// <summary>
        /// Display the Open File dialog for Workspaces
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="vm"></param>
        public static async void DisplayVideoOpenDialog(Interaction<MainViewModel, Uri?> interaction, MainViewModel vm)
        {
            var uri = await interaction.Handle(vm);
            if (uri == null) return;

            HoloContext.Instance.Workspace.WorkingFile.AVManager.LoadVideo(uri.LocalPath);
        }

        public static async void PasteLines(Interaction<TabItemViewModel, string[]?> interaction, TabItemViewModel vm)
        {
            string[] lines = await interaction.Handle(vm);
            var file = vm.Wrapper.File;
            var selectedId = vm.Wrapper.SelectedEvent?.Id ?? -1;
            if (selectedId == -1) return;

            var events = new List<Event>();

            foreach (var linedata in lines)
            {
                if (linedata.Trim().Equals(string.Empty)) continue;

                Event line;
                if (linedata.StartsWith("Dialogue:") || linedata.StartsWith("Comment:"))
                {
                    line = new Event(file.EventManager.NextId, linedata.Trim());
                }
                else
                {
                    line = new Event(file.EventManager.NextId)
                    {
                        Text = linedata
                    };
                }
                events.Add(line);
                selectedId = file.EventManager.AddAfter(selectedId, line);
            }
            if (events.Count > 0)
                vm.Wrapper.Add(events, events.First(), false, true);
        }

        public static async void PasteOverLines(Interaction<TabItemViewModel, string[]?> pasteInteraction,
                                                Interaction<PasteOverWindowViewModel, PasteOverField> fieldsInteraction, TabItemViewModel vm)
        {
            var powvm = new PasteOverWindowViewModel();
            PasteOverField fields = await fieldsInteraction.Handle(powvm);
            if (fields == PasteOverField.None) return;

            string[] lines = await pasteInteraction.Handle(vm);
            var file = vm.Wrapper.File;
            var selectedId = vm.Wrapper.SelectedEvent?.Id ?? -1;
            if (selectedId == -1) return;

            var editedEvents = new List<Event>();
            var newEvents = new List<Event>();

            Event? oldLine = file.EventManager.Get(selectedId);

            foreach (var linedata in lines)
            {
                if (linedata.Trim().Equals(string.Empty)) continue;
                if (!linedata.StartsWith("Comment:") && !linedata.StartsWith("Dialogue:")) return;
                var newLine = new Event(-1, linedata.Trim());

                var shin = false;
                if (file.EventManager.Has(selectedId))
                {
                    oldLine = file.EventManager.Get(selectedId);
                    editedEvents.Add(oldLine.Clone());
                }
                else
                {
                    var cleanEvent = new Event(file.EventManager.NextId);
                    file.EventManager.AddAfter(oldLine.Id, cleanEvent);
                    oldLine = cleanEvent;
                    shin = true;
                }

                foreach (PasteOverField field in Helpers.GetFlags(fields))
                {
                    switch (field)
                    {
                        case PasteOverField.Comment:
                            oldLine.Comment = newLine.Comment;
                            break;
                        case PasteOverField.Layer:
                            oldLine.Layer = newLine.Layer;
                            break;
                        case PasteOverField.StartTime:
                            oldLine.Start = newLine.Start;
                            break;
                        case PasteOverField.EndTime:
                            oldLine.End = newLine.End;
                            break;
                        case PasteOverField.Actor:
                            oldLine.Actor = newLine.Actor;
                            break;
                        case PasteOverField.Style:
                            oldLine.Style = newLine.Style;
                            break;
                        case PasteOverField.MarginLeft:
                            oldLine.Margins.Left = newLine.Margins.Left;
                            break;
                        case PasteOverField.MarginRight:
                            oldLine.Margins.Right = newLine.Margins.Right;
                            break;
                        case PasteOverField.MarginVertical:
                            oldLine.Margins.Vertical = newLine.Margins.Vertical;
                            break;
                        case PasteOverField.Effect:
                            oldLine.Effect = newLine.Effect;
                            break;
                        case PasteOverField.Text:
                            oldLine.Text = newLine.Text;
                            break;
                        default:
                            break;
                    }
                }

                if (shin)
                {
                    newEvents.Add(oldLine.Clone());
                }

                var next = file.EventManager.GetAfter(oldLine.Id);
                if (next != null) selectedId = next.Id;
                else selectedId = -1;
            }

            var editedSnap = new Snapshot<Event>(editedEvents.Select(e => new SnapPosition<Event>(e, file.EventManager.GetBefore(e.Id)?.Id)).ToList(), AssCS.Action.EDIT);
            var newSnap = new Snapshot<Event>(newEvents.Select(e => new SnapPosition<Event>(e, file.EventManager.GetBefore(e.Id)?.Id)).ToList(), AssCS.Action.INSERT);
            file.HistoryManager.Commit(new Commit<Event>( new List<Snapshot<Event>> { editedSnap, newSnap }));
        }

        /// <summary>
        /// Safely close a tab
        /// </summary>
        /// <param name="fileId">ID to close</param>
        /// <param name="interaction">SaveAsFileDialog interaction</param>
        public static async Task<bool> CloseTab(int fileId, Interaction<FileWrapper, Uri?> interaction)
        {
            var file = HoloContext.Instance.Workspace.GetFile(fileId);
            if (file.UpToDate)
            {
                HoloContext.Instance.Workspace.CloseFileInWorkspace(fileId);
                return true;
            }

            var box = MessageBoxManager.GetMessageBoxStandard(
                "Save work?",
                $"{file.Title} contains unsaved work. Do you want to save?",
                MsBox.Avalonia.Enums.ButtonEnum.YesNoCancel
            );
            var result = await box.ShowAsync();

            switch (result)
            {
                case MsBox.Avalonia.Enums.ButtonResult.Yes:
                    var saved = await SaveOrDisplaySaveAsDialog(interaction, file);
                    if (!saved) return false;

                    HoloContext.Instance.Workspace.CloseFileInWorkspace(fileId);
                    return true;
                case MsBox.Avalonia.Enums.ButtonResult.No:
                    HoloContext.Instance.Workspace.CloseFileInWorkspace(fileId);
                    return true;
                default:  // Cancel
                    return false;
            }
        }

        /// <summary>
        /// Try to close all the tabs
        /// </summary>
        /// <param name="interaction">SaveAsFileDialog interaction</param>
        /// <param name="vm">View Model</param>
        /// <returns>True if the closing can continue, False if canceled</returns>
        public static async Task<bool> CloseWindow(Interaction<FileWrapper, Uri?> interaction, MainViewModel vm)
        {
            var ids = vm.Tabs.Select(t => t.ID).ToList();

            foreach (var id in ids)
            {
                var closed = await CloseTab(id, interaction);
                if (!closed) return false;
            }

            return true;
        }
    }
}
