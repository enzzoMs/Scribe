using System.Windows.Input;

namespace Scribe.UI.Commands;

public class DelegateCommand(
    Action<object?> execute, Func<object?, bool>? canExecute = null
) : ICommand
{
    private readonly Action<object?> _execute = execute;
    private readonly Func<object?, bool>? _canExecute = canExecute;
    
    public event EventHandler? CanExecuteChanged;
    
    public void Execute(object? parameter) => _execute(parameter);
    
    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}