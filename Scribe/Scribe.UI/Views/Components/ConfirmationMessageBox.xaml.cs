using System.Windows;
using System.Windows.Input;

namespace Scribe.UI.Views.Components;

public partial class ConfirmationMessageBox : Window
{
    public ConfirmationMessageBox() => InitializeComponent();

    public string WindowTitle
    {
        get => (string) GetValue(WindowTitleProperty);
        set => SetValue(WindowTitleProperty, value);
    }
    
    public string ConfirmationMessage
    {
        get => (string) GetValue(ConfirmationMessageProperty);
        set => SetValue(ConfirmationMessageProperty, value);
    }
    
    public ICommand OnConfirm
    {
        get => (ICommand) GetValue(OnConfirmProperty);
        set => SetValue(OnConfirmProperty, value);
    }
    
    public static readonly DependencyProperty WindowTitleProperty = DependencyProperty.Register(
        name: nameof(WindowTitle),
        propertyType: typeof(string),
        ownerType: typeof(ConfirmationMessageBox)
    );
    
    public static readonly DependencyProperty ConfirmationMessageProperty = DependencyProperty.Register(
        name: nameof(ConfirmationMessage),
        propertyType: typeof(string),
        ownerType: typeof(ConfirmationMessageBox)
    );
    
    public static readonly DependencyProperty OnConfirmProperty = DependencyProperty.Register(
        name: nameof(OnConfirm),
        propertyType: typeof(ICommand),
        ownerType: typeof(ConfirmationMessageBox)
    );
    
    private void OnOptionButtonClicked(object sender, RoutedEventArgs e) => DialogResult = false;
}