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
    
    private void OnCloseTabButtonClicked(object sender, RoutedEventArgs e)
    {
        var tabItem = (TabItem) ((FrameworkElement) sender).TemplatedParent;
        CloseTabClick?.Invoke(this, tabItem.Content);   
    }
}