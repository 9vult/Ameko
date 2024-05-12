using Ameko.DataModels;
using AssCS;
using Holo;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class StyleEditorViewModel : ViewModelBase
    {
        private StyleLocation _loc;
        private string _name;
        private string _originName;
        private bool _invalidName;
        
        public Style Style { get; set; }

        public bool InvalidName
        {
            get => _invalidName;
            set => this.RaiseAndSetIfChanged(ref _invalidName, value);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_originName != value)
                {
                    InvalidName = IsNameInUse(value);
                }
                else
                {
                    InvalidName = value.Trim() == string.Empty;
                }
                this.RaiseAndSetIfChanged(ref _name, value);
            }
        }

        public Interaction<ColorWindowViewModel, Color?> ShowDialog { get; }

        public ICommand EditPrimaryCommand { get; }
        public ICommand EditSecondaryCommand { get; }
        public ICommand EditOutlineCommand { get; }
        public ICommand EditShadowCommand { get; }

        public bool CommitNameChange()
        {
            if (_originName == _name) return true;
            if (IsNameInUse(Name)) return false;

            Style.Name = _name;
            if (_loc == StyleLocation.File)
            {
                HoloContext.Instance.Workspace.WorkingFile.File.EventManager.ChangeStyle(_originName, _name);
            }
            return true;
        }

        private bool IsNameInUse(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            switch (_loc)
            {
                case StyleLocation.File:
                    if (HoloContext.Instance.Workspace.WorkingFile.File.StyleManager.Get(name) != null)
                        return true;
                    return false;
                case StyleLocation.Workspace:
                    if (HoloContext.Instance.Workspace.GetStyle(name) != null)
                        return true;
                    return false;
                case StyleLocation.Global:
                    if (HoloContext.Instance.GlobalsManager.GetStyle(name) != null)
                        return true;
                    return false;
                default:
                    return true;
            }
        }

        public StyleEditorViewModel(Style style, StyleLocation loc)
        {
            this.Style = style;
            this._loc = loc;
            this._name = style.Name;
            this._originName = style.Name;
            InvalidName = _name.Trim() == string.Empty;

            ShowDialog = new Interaction<ColorWindowViewModel, Color?>();

            EditPrimaryCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var input = new ColorWindowViewModel(Style.Primary);
                var result = await ShowDialog.Handle(input);
                if (result != null)
                {
                    Style.Primary = result;
                }
            });
            EditSecondaryCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var input = new ColorWindowViewModel(Style.Secondary);
                var result = await ShowDialog.Handle(input);
                if (result != null)
                {
                    Style.Secondary = result;
                }
            });
            EditOutlineCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var input = new ColorWindowViewModel(Style.Outline);
                var result = await ShowDialog.Handle(input);
                if (result != null)
                {
                    Style.Outline = result;
                }
            });
            EditShadowCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var input = new ColorWindowViewModel(Style.Shadow);
                var result = await ShowDialog.Handle(input);
                if (result != null)
                {
                    Style.Shadow = result;
                }
            });
        }
    }
}
