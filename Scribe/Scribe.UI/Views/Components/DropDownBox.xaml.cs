using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Scribe.UI.Views.Components;

public partial class DropDownBox : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        name: nameof(Text),
        propertyType: typeof(string),
        ownerType: typeof(DropDownBox)
    );

    public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register(
        name: nameof(MenuItems),
        propertyType: typeof(List<MenuItem>),
        ownerType: typeof(DropDownBox)
    );

    public static readonly DependencyProperty MenuItemsIconSourceProperty = DependencyProperty.Register(
        name: nameof(MenuItemsIconSource),
        propertyType: typeof(ImageSource),
        ownerType: typeof(DropDownBox)
    );
    
    public ImageSource MenuItemsIconSource
    {
        get => (ImageSource) GetValue(MenuItemsIconSourceProperty);
        set => SetValue(MenuItemsIconSourceProperty, value);
    }
    
    public string Text
    {
        get => (string) GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public List<MenuItem> MenuItems
    {
        get => (List<MenuItem>) GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    public DropDownBox()
    {
        InitializeComponent();
        SetValue(MenuItemsProperty, new List<MenuItem>());
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var contextMenu = DropDownButton.ContextMenu;
        if (contextMenu != null)
        {
            contextMenu.PlacementTarget = DropDownButton;
            contextMenu.IsOpen = true;
        }
    }

    private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu contextMenu)
        {
            contextMenu.ItemsSource = MenuItems;
            contextMenu.DataContext = Root.DataContext;
        }
    }
}