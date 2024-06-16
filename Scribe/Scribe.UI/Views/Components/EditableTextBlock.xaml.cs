using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scribe.UI.Views.Components;

public partial class EditableTextBlock : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        name: nameof(Text),
        propertyType: typeof(string),
        ownerType: typeof(EditableTextBlock)
    );
    
    public static readonly DependencyProperty OnEditModeProperty = DependencyProperty.Register(
        name: nameof(OnEditMode),
        propertyType: typeof(bool),
        ownerType: typeof(EditableTextBlock)
    );
        
    public static readonly DependencyProperty ConfirmChangesCommandProperty = DependencyProperty.Register(
        name: nameof(ConfirmChangesCommand),
        propertyType: typeof(ICommand),
        ownerType: typeof(EditableTextBlock)
    );
    
    public string Text
    {
        get => (string) GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public bool OnEditMode
    {
        get => (bool) GetValue(OnEditModeProperty);
        set => SetValue(OnEditModeProperty, value);
    }
    
    public ICommand ConfirmChangesCommand
    {
        get => (ICommand) GetValue(ConfirmChangesCommandProperty);
        set => SetValue(ConfirmChangesCommandProperty, value);
    }
    
    public EditableTextBlock() => InitializeComponent();

    private void OnCancelEditClicked(object sender, MouseButtonEventArgs e) => OnEditMode = false;
}