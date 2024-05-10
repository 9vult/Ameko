using Ameko.Services;
using AvaloniaEdit.Document;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class FreeformWindowViewModel : ViewModelBase
    {
        public ICommand ExecuteCommand { get; private set; }
        public TextDocument Document { get; private set; }

        private string _status;
        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public FreeformWindowViewModel()
        {
            Document = new TextDocument(HoloContext.Instance.GlobalsManager.FreeformDocument);
            _status = "Ready";

            ExecuteCommand = ReactiveCommand.Create(() =>
            {
                HoloContext.Instance.GlobalsManager.FreeformDocument = Document.Text;
                Status = "Running...";
                var result = ScriptService.Instance.ExecuteFreeform(Document.Text);
                Status = result;
            });

        }
    }
}
