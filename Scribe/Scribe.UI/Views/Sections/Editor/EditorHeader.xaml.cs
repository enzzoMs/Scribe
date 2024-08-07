using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Scribe.UI.Views.Components;

namespace Scribe.UI.Views.Sections.Editor;

public partial class EditorHeader : UserControl
{
    public EditorHeader() => InitializeComponent();
    
    private void OnAddTagButtonClicked(object sender, RoutedEventArgs e)
    {
        var button = (Button) sender;
        var contextMenu = button.ContextMenu;
        
        if (contextMenu != null)
        {
            contextMenu.PlacementTarget = button;
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }
    }

    private void OnConfirmTagNameClicked(object sender, RoutedEventArgs e)
    {
        var iconButton = (IconButton) sender;
        var stackPanel = (StackPanel) iconButton.Parent;
        var menuItem = (MenuItem) stackPanel.TemplatedParent;
        var contextMenu = (ContextMenu) menuItem.Parent;
        contextMenu.IsOpen = false;
    }
}