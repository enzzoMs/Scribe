using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Scribe.UI.Views.Components;

public partial class PagedList : UserControl
{
    private int _currentPageIndex;
    private int _maxPageIndex;
    private const int SkipSizeInPages = 5;

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        name: nameof(ItemsSource),
        propertyType: typeof(IEnumerable),
        ownerType: typeof(UserControl),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnItemsSourceChanged)    
    );

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        name: nameof(ItemTemplate),
        propertyType: typeof(ControlTemplate),
        ownerType: typeof(UserControl)
    );
    
    public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(
        name: nameof(PageSize),
        propertyType: typeof(int),
        ownerType: typeof(UserControl)
    );
    
    public IEnumerable ItemsSource
    {
        get => (IEnumerable) GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    public ControlTemplate ItemTemplate
    {
        get => (ControlTemplate) GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    public int PageSize
    {
        get => (int) GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }
    
    public PagedList() => InitializeComponent();

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var pagedList = (PagedList) d;
        NotifyCollectionChangedEventHandler onItemChanged = (_, _) => InitializePagedList(pagedList);

        if (e.NewValue is INotifyCollectionChanged newObservableCollection)
        {
            newObservableCollection.CollectionChanged += onItemChanged;
        }
        
        if (e.OldValue is INotifyCollectionChanged oldObservableCollection)
        {
            oldObservableCollection.CollectionChanged -= onItemChanged;
        }
        
        InitializePagedList(pagedList);
    }

    private static void InitializePagedList(PagedList pagedList)
    {
        var itemsSource = pagedList.ItemsSource.Cast<object>().ToList();
        
        pagedList.ItemList.ItemsSource = itemsSource.Take(pagedList.PageSize);
        
        pagedList.CurrentPageText.Text = (pagedList._currentPageIndex + 1).ToString();
        
        pagedList._maxPageIndex = 
            pagedList.PageSize == 0 || itemsSource.Count == 0 ? 
                0 : (int) Math.Ceiling((double) itemsSource.Count / pagedList.PageSize) - 1;

        pagedList.MaxPagesText.Text = (pagedList._maxPageIndex + 1).ToString();
    }

    private void OnPreviousPageClicked(object sender, RoutedEventArgs e)
    {
        if (_currentPageIndex == 0) return;
    
        _currentPageIndex--;
        UpdatePageItems();
    }

    private void OnNextPageClicked(object sender, RoutedEventArgs e)
    {
        var itemsSource = ItemsSource.Cast<object>();
        if ((_currentPageIndex + 1) * PageSize >= itemsSource.Count()) return;
        
        _currentPageIndex++;
        UpdatePageItems();
    }

    private void OnSkipBackwardsClicked(object sender, RoutedEventArgs e)
    {
        var skipSize = _currentPageIndex - SkipSizeInPages < 0 ? _currentPageIndex : SkipSizeInPages; 

        _currentPageIndex -= skipSize;
        UpdatePageItems();
    }
    
    private void OnSkipForwardClicked(object sender, RoutedEventArgs e)
    {
        var skipSize = _currentPageIndex + SkipSizeInPages > _maxPageIndex ? 
            _maxPageIndex - _currentPageIndex : SkipSizeInPages; 
        
        _currentPageIndex += skipSize;
        UpdatePageItems();
    }

    private void UpdatePageItems()
    {
        var itemsSource = ItemsSource.Cast<object>();

        CurrentPageText.Text = (_currentPageIndex + 1).ToString();
        ItemList.ItemsSource = itemsSource.Skip(PageSize * _currentPageIndex).Take(PageSize);
    }
}