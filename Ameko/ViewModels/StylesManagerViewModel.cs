using Ameko.DataModels;
using AssCS;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class StylesManagerViewModel : ViewModelBase
    {
        private Workspace _workspace;
        private GlobalsManager _globalsManager;
        public Workspace Workspace
        {
            get => _workspace;
            set => this.RaiseAndSetIfChanged(ref _workspace, value);
        }

        public GlobalsManager GlobalsManager
        {
            get => _globalsManager;
            set => this.RaiseAndSetIfChanged(ref _globalsManager, value);
        }

        public Style? SelectedFileStyle { get; set; }
        public Style? SelectedWorkspaceStyle { get; set; }
        public Style? SelectedGlobalStyle { get; set; }

        public Interaction<StyleEditorViewModel, StyleEditorViewModel?> ShowStyleEditor { get; }

        public ICommand CopyFromFileToWorkspaceCommand { get; }
        public ICommand CopyFromFileToGlobalsCommand { get; }
        public ICommand DeleteFileStyleCommand { get; }
        public ICommand EditFileStyleCommand { get; }
        public ICommand NewFileStyleCommand { get; }
        public ICommand DuplicateFileStyleCommand { get; }

        public ICommand CopyFromWorkspaceToFileCommand { get; }
        public ICommand CopyFromWorkspaceToGlobalsCommand { get; }
        public ICommand DeleteWorkspaceStyleCommand { get; }
        public ICommand EditWorkspaceStyleCommand { get; }
        public ICommand NewWorkspaceStyleCommand { get; }
        public ICommand DuplicateWorkspaceStyleCommand { get; }

        public ICommand CopyFromGlobalsToFileCommand { get; }
        public ICommand CopyFromGlobalsToWorkspaceCommand { get; }
        public ICommand DeleteGlobalsStyleCommand { get; }
        public ICommand EditGlobalsStyleCommand { get; }
        public ICommand NewGlobalsStyleCommand { get; }
        public ICommand DuplicateGlobalsStyleCommand { get; }

        public StylesManagerViewModel()
        {
            _workspace = HoloContext.Instance.Workspace;
            _globalsManager = HoloContext.Instance.GlobalsManager;
            ShowStyleEditor = new Interaction<StyleEditorViewModel, StyleEditorViewModel?>();

            CopyFromFileToWorkspaceCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedFileStyle == null) return;
                Workspace.AddStyle(SelectedFileStyle.Clone());
            });

            CopyFromFileToGlobalsCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedFileStyle == null) return;
                GlobalsManager.AddStyle(SelectedFileStyle.Clone());
            });

            DeleteFileStyleCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedFileStyle == null) return;
                Workspace.WorkingFile.File.StyleManager.Remove(SelectedFileStyle.Name);
            });

            EditFileStyleCommand = ReactiveCommand.Create(async () =>
            {
                if (SelectedFileStyle == null) return;
                var editor = new StyleEditorViewModel(SelectedFileStyle, StyleLocation.File);
                await ShowStyleEditor.Handle(editor);
            });

            NewFileStyleCommand = ReactiveCommand.Create(async () =>
            {
                var style = new Style(Workspace.WorkingFile.File.StyleManager.NextId);
                style.Name = "";
                Workspace.WorkingFile.File.StyleManager.Add(style);
                SelectedFileStyle = style;
                var editor = new StyleEditorViewModel(SelectedFileStyle, StyleLocation.File);
                await ShowStyleEditor.Handle(editor);
            });

            DuplicateFileStyleCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedFileStyle == null) return;
                var style = new Style(Workspace.WorkingFile.File.StyleManager.NextId, SelectedFileStyle);
                var name = $"{SelectedFileStyle.Name} (copy)";
                while (Workspace.WorkingFile.File.StyleManager.Get(name) != null) name = $"{name} (copy)";
                style.Name = name;
                Workspace.WorkingFile.File.StyleManager.Add(style);
            });

            CopyFromWorkspaceToFileCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedWorkspaceStyle == null) return;
                Workspace.WorkingFile.File.StyleManager.AddOrReplace(SelectedWorkspaceStyle.Clone());
            });

            CopyFromWorkspaceToGlobalsCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedWorkspaceStyle == null) return;
                GlobalsManager.AddStyle(SelectedWorkspaceStyle.Clone());
            });

            DeleteWorkspaceStyleCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedWorkspaceStyle == null) return;
                Workspace.RemoveStyle(SelectedWorkspaceStyle.Name);
            });

            EditWorkspaceStyleCommand = ReactiveCommand.Create(async () =>
            {
                if (SelectedWorkspaceStyle == null) return;
                var editor = new StyleEditorViewModel(SelectedWorkspaceStyle, StyleLocation.Workspace);
                await ShowStyleEditor.Handle(editor);
            });

            NewWorkspaceStyleCommand = ReactiveCommand.Create(async () =>
            {
                var style = new Style(Workspace.WorkingFile.File.StyleManager.NextId);
                style.Name = "";
                Workspace.AddStyle(style);
                SelectedWorkspaceStyle = style;
                var editor = new StyleEditorViewModel(SelectedWorkspaceStyle, StyleLocation.Workspace);
                await ShowStyleEditor.Handle(editor);
            });

            DuplicateWorkspaceStyleCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedWorkspaceStyle == null) return;
                var style = new Style(Workspace.WorkingFile.File.StyleManager.NextId, SelectedWorkspaceStyle);
                var name = $"{SelectedWorkspaceStyle.Name} (copy)";
                while (Workspace.GetStyle(name) != null) name = $"{name} (copy)";
                style.Name = name;
                Workspace.AddStyle(style);
            });

            CopyFromGlobalsToFileCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedGlobalStyle == null) return;
                Workspace.WorkingFile.File.StyleManager.AddOrReplace(SelectedGlobalStyle.Clone());
            });

            CopyFromGlobalsToWorkspaceCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedGlobalStyle == null) return;
                Workspace.AddStyle(SelectedGlobalStyle.Clone());
            });

            DeleteGlobalsStyleCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedGlobalStyle == null) return;
                GlobalsManager.RemoveStyle(SelectedGlobalStyle.Name);
            });

            EditGlobalsStyleCommand = ReactiveCommand.Create(async () =>
            {
                if (SelectedGlobalStyle == null) return;
                var editor = new StyleEditorViewModel(SelectedGlobalStyle, StyleLocation.Global);
                await ShowStyleEditor.Handle(editor);
            });

            NewGlobalsStyleCommand = ReactiveCommand.Create(async () =>
            {
                var style = new Style(Workspace.WorkingFile.File.StyleManager.NextId);
                style.Name = "";
                GlobalsManager.AddStyle(style);
                SelectedGlobalStyle = style;
                var editor = new StyleEditorViewModel(SelectedGlobalStyle, StyleLocation.Global);
                await ShowStyleEditor.Handle(editor);
            });

            DuplicateGlobalsStyleCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedGlobalStyle == null) return;
                var style = new Style(Workspace.WorkingFile.File.StyleManager.NextId, SelectedGlobalStyle);
                var name = $"{SelectedGlobalStyle.Name} (copy)";
                while (GlobalsManager.GetStyle(name) != null) name = $"{name} (copy)";
                style.Name = name;
                GlobalsManager.AddStyle(style);
            });
        }
    }
}
