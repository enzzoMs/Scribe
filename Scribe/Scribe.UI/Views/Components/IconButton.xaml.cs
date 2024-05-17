using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Scribe.UI.Views.Components;

public partial class IconButton : UserControl
{
    public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
        name: nameof(IconSource),
        propertyType: typeof(ImageSource),
        ownerType: typeof(UserControl)
    );
    
    public static readonly DependencyProperty IconPaddingProperty = DependencyProperty.Register(
        name: nameof(IconPadding),
        propertyType: typeof(double),
        ownerType: typeof(UserControl)
    );

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        name: nameof(Command),
        propertyType: typeof(ICommand),
        ownerType: typeof(UserControl)
    );
    
    public ImageSource? IconSource
    {
        get => GetValue(IconSourceProperty) as ImageSource;
        set => SetValue(IconSourceProperty, value);
    }
    
    public double IconPadding
    {
        get => GetValue(IconPaddingProperty) as double? ?? 0;
        set => SetValue(IconPaddingProperty, value);
    }
    
    public ICommand? Command
    {
        get => GetValue(CommandProperty) as ICommand;
        set => SetValue(CommandProperty, value);
    }

    public IconButton() => InitializeComponent();

    private void ButtonIcon_OnLoaded(object sender, RoutedEventArgs e)
    {
        var iconWidth = ButtonContainer.ActualWidth - IconPadding;
        var iconHeight = ButtonContainer.ActualHeight - IconPadding;
            
        ButtonIcon.Width = iconWidth < 0 ? 0 : iconWidth;
        ButtonIcon.Height = iconHeight < 0 ? 0 : iconHeight;
    }
}