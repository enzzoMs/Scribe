using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scribe.UI.Views.Components;

public partial class ClosableTabControl : UserControl
{
    public static readonly DependencyProperty TabItemsProperty = DependencyProperty.Register(
        name: nameof(TabItems),
        propertyType: typeof(IEnumerable),
        ownerType: typeof(UserControl),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnTabItemsChanged)
    );
    
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        name: nameof(SelectedItem),
        propertyType: typeof(object),
        ownerType: typeof(UserControl)
    );
    
    public static readonly DependencyProperty CloseTabCommandProperty = DependencyProperty.Register(
        name: nameof(CloseTabCommand),
        propertyType: typeof(ICommand),
        ownerType: typeof(UserControl)
    );

    public IEnumerable TabItems
    {
        get => (IEnumerable) GetValue(TabItemsProperty);
        set => SetValue(TabItemsProperty, value);
    }
    
    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
    
    public ICommand CloseTabCommand
    {
        get => (ICommand) GetValue(CloseTabCommandProperty);
        set => SetValue(CloseTabCommandProperty, value);
    }
    
    public ClosableTabControl() => InitializeComponent();
    
    
    private static void OnTabItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var closableTabControl = (ClosableTabControl) d;
        
        NotifyCollectionChangedEventHandler onItemChanged = (_, collectionEventArgs) =>
        {
            if (collectionEventArgs.NewItems != null)
                AddTabs(closableTabControl.MainTabControl, collectionEventArgs.NewItems);
            else if (collectionEventArgs.OldItems != null)
                CloseTabs(closableTabControl.MainTabControl, collectionEventArgs.OldItems);
        };

        if (e.NewValue is INotifyCollectionChanged newObservableCollection)
        {
            newObservableCollection.CollectionChanged += onItemChanged;
        }
        
        if (e.OldValue is INotifyCollectionChanged oldObservableCollection)
        {
            oldObservableCollection.CollectionChanged -= onItemChanged;
        }
        
        AddTabs(closableTabControl.MainTabControl, closableTabControl.TabItems);
    }

    private static void AddTabs(TabControl tabControl, IEnumerable tabItems)
    {
        foreach (var item in tabItems)
        {
            tabControl.Items.Add(item);
        }
    }
    
    private static void CloseTabs(TabControl tabControl, IEnumerable tabItems)
    {
        foreach (var item in tabItems)
        {
            tabControl.Items.Remove(item);
        }
    }

    private void MainTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;
        
        var tabControl = (TabControl) sender;

        var selectedItem = e.AddedItems[0]!;
        
        var selectedTabItem = (TabItem) tabControl.ItemContainerGenerator.ContainerFromItem(selectedItem);
        selectedTabItem.Focus();
    }
}