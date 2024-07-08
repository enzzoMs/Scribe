using System.Windows;
using System.Windows.Controls;

namespace Scribe.UI.Views.Components;

public partial class ClosableTabControl : TabControl
{
    public ClosableTabControl() => InitializeComponent();

    public event EventHandler<object>? CloseTabClick;

    public void CloseAllTabs()
    {
        var itemsSource = ItemsSource.Cast<object>().ToList();
        
        foreach (var item in itemsSource)
        {
            var tabItem = (TabItem) ItemContainerGenerator.ContainerFromItem(item);
            CloseTabClick?.Invoke(this, tabItem.Content);   
        }
    }
    
    private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;
        
        // Updating the tab header content just in case the data context was updated
        
        var tabControl = (TabControl) sender;

        var selectedItem = e.AddedItems[0]!;
        
        var selectedTabItem = (TabItem) tabControl.ItemContainerGenerator.ContainerFromItem(selectedItem);
        
        if (selectedTabItem.Template.FindName("TabHeader", selectedTabItem) is ContentControl tabTemplate)
        {
            tabTemplate.Content = selectedTabItem.DataContext.ToString();
        }
    }

    private void OnCloseTabButtonClicked(object sender, RoutedEventArgs e)
    {
        var tabItem = (TabItem) ((FrameworkElement) sender).TemplatedParent;
        CloseTabClick?.Invoke(this, tabItem.Content);   
    }
}