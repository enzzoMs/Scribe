using System.Windows;
using System.Windows.Controls;

namespace Scribe.UI.Views.Components;

public partial class PagedList : UserControl
{
    private int _currentPageIndex;
    private int _maxPageIndex;
    private const int SkipSizeInPages = 5;

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        name: nameof(ItemsProperty),
        propertyType: typeof(ICollection<object>),
        ownerType: typeof(UserControl),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnItemsSourceChanged)    
    );
    
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        name: nameof(ItemTemplate),
        propertyType: typeof(ControlTemplate),
        ownerType: typeof(UserControl),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnItemsSourceChanged)
    );
    
    public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(
        name: nameof(PageSize),
        propertyType: typeof(int),
        ownerType: typeof(UserControl),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnItemsSourceChanged)
    );
    
    public ICollection<object> Items
    {
        get => GetValue(ItemsProperty) as ICollection<object> ?? [];
        set => SetValue(ItemsProperty, value);
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

        pagedList.ItemList.ItemsSource = pagedList.Items.Take(pagedList.PageSize);
        
        pagedList.CurrentPageText.Text = (pagedList._currentPageIndex + 1).ToString();
        
        pagedList._maxPageIndex = 
            pagedList.PageSize == 0 || pagedList.Items.Count == 0 ? 
                0 : (int) Math.Ceiling((double) pagedList.Items.Count / pagedList.PageSize) - 1;

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
        if ((_currentPageIndex + 1) * PageSize >= Items.Count()) return;
        
        _currentPageIndex++;
        UpdatePageItems();
    }

    private void OnSkipBackwardsClicked(object sender, RoutedEventArgs e)
    {
        if (_currentPageIndex - SkipSizeInPages < 0) return;

        _currentPageIndex -= SkipSizeInPages;
        UpdatePageItems();
    }
    
    private void OnSkipForwardClicked(object sender, RoutedEventArgs e)
    {
        if (_currentPageIndex + SkipSizeInPages > _maxPageIndex) return;

        _currentPageIndex += SkipSizeInPages;
        UpdatePageItems();
    }

    private void UpdatePageItems()
    {
        CurrentPageText.Text = (_currentPageIndex + 1).ToString();
        ItemList.ItemsSource = Items.Skip(PageSize * _currentPageIndex).Take(PageSize);
    }
}