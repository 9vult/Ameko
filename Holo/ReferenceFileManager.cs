// SPDX-License-Identifier: MPL-2.0

using AssCS;

namespace Holo;

public class ReferenceFileManager : BindableBase
{
    private Document? _reference;
    private string _currentLines = string.Empty;
    private readonly SelectionManager _selectionManager;

    public Document? Reference
    {
        get => _reference;
        set
        {
            SetProperty(ref _reference, value);
            ReferenceLoaded = value is not null;
            RaisePropertyChanged(nameof(ReferenceLoaded));
            GetCorrespondingLines();
        }
    }

    public bool ReferenceLoaded { get; private set; }

    public string CurrentLines
    {
        get => _currentLines;
        set => SetProperty(ref _currentLines, value);
    }

    private void GetCorrespondingLines()
    {
        if (Reference is null)
            return;
        var hits = Reference
            .EventManager.Events.Where(e => e.CollidesWith(_selectionManager.ActiveEvent))
            .Select(e => e.Text);
        CurrentLines = string.Join(Environment.NewLine, hits);
    }

    public ReferenceFileManager(SelectionManager selectionManager)
    {
        _selectionManager = selectionManager;
        _selectionManager.PropertyChanged += (_, _) => GetCorrespondingLines();
    }
}
