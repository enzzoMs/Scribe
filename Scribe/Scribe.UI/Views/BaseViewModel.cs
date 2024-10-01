using System.ComponentModel;
using System.Runtime.CompilerServices;
using Scribe.UI.Views.Errors;

namespace Scribe.UI.Views;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event EventHandler<IViewModelError>? ViewModelError;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void RaiseViewModelError(IViewModelError error)
    {
        ViewModelError?.Invoke(this, error);
    }

    public virtual Task Load() => Task.CompletedTask;
}