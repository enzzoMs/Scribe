using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Scribe.UI.Views.Sections.Tags.State;

public class TagViewState(string name) : INotifyPropertyChanged
{
    private bool _isSelected;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name { get; } = name;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            RaisePropertyChanged();
        }
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
};