using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scribe.UI.Views.Components;

public partial class ClosableTabControl : TabControl
{
    public ClosableTabControl() => InitializeComponent();

    public DataTemplate TabContentTemplate
    {
        get => (DataTemplate) GetValue(TabContentTemplateProperty);
        set => SetValue(TabContentTemplateProperty, value);
    }
    
    public ICommand CloseTabCommand
    {
        get => (ICommand) GetValue(CloseTabCommandProperty);
        set => SetValue(CloseTabCommandProperty, value);
    }
    
    public static readonly DependencyProperty TabContentTemplateProperty = DependencyProperty.Register(
        name: nameof(TabContentTemplate),
        propertyType: typeof(DataTemplate),
        ownerType: typeof(ClosableTabControl)
    );
    
    public static readonly DependencyProperty CloseTabCommandProperty = DependencyProperty.Register(
        name: nameof(CloseTabCommand),
        propertyType: typeof(ICommand),
        ownerType: typeof(ClosableTabControl)
    );
    
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
}