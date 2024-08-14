using System.ComponentModel;
using System.Runtime.CompilerServices;
using Scribe.Data.Model;

namespace Scribe.UI.Views.Sections.Editor.State;

public class DocumentViewState(Document document) : INotifyPropertyChanged
{
    private bool _hasUnsavedChanges;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public Document Document { get; } = document;

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set
        {
            _hasUnsavedChanges = value;
            RaisePropertyChanged();
        }
    }

    public string EditedContent { get; set; } = document.Content;

    public override string ToString() => Document.Name;

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}