// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Windows.Input;
using Ameko.Messages;
using Holo.Models;
using Holo.Providers;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class InstallDictionaryDialogViewModel : ViewModelBase
{
    private bool _areButtonsEnabled = true;
    public bool AreButtonsEnabled
    {
        get => _areButtonsEnabled;
        set => this.RaiseAndSetIfChanged(ref _areButtonsEnabled, value);
    }

    public string Header { get; }

    public ReactiveCommand<Unit, EmptyMessage> DownloadCommand { get; }
    public ReactiveCommand<Unit, EmptyMessage> IgnoreCommand { get; }

    public InstallDictionaryDialogViewModel(
        IDictionaryService dictionaryService,
        SpellcheckLanguage language,
        bool isProjectRequest
    )
    {
        Header = string.Format(
            isProjectRequest
                ? I18N.Spellcheck.Spellcheck_Install_ProjectHeader
                : I18N.Spellcheck.Spellcheck_Install_ConfigHeader,
            language.Name
        );

        IgnoreCommand = ReactiveCommand.Create(() => new EmptyMessage());

        DownloadCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            AreButtonsEnabled = false;
            await dictionaryService.DownloadDictionary(language);
            return new EmptyMessage();
        });
    }
}
