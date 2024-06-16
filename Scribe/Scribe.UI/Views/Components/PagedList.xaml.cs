﻿using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Scribe.UI.Views.Components;

public partial class PagedList : UserControl
{
    private int _currentPageIndex;
    private int _maxPageIndex;
    private int _itemsPerPage = 1;
    private const int SkipSizeInPages = 5;

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        name: nameof(ItemsSource),
        propertyType: typeof(IEnumerable),
        ownerType: typeof(PagedList),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnItemsSourceChanged)    
    );

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        name: nameof(ItemTemplate),
        propertyType: typeof(ControlTemplate),
        ownerType: typeof(PagedList)
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
    
    public PagedList() => InitializeComponent();

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var pagedList = (PagedList) d;
        
        NotifyCollectionChangedEventHandler onItemChanged = (_, collectionEventArgs) =>
        {
            var maxPageIndex = pagedList.RecalculateMaxPageIndex();
            
            // If an item was added
            if (collectionEventArgs.NewItems != null)
                pagedList._currentPageIndex = maxPageIndex;
            // If an item was deleted
            else if (collectionEventArgs.OldItems != null && pagedList._currentPageIndex > maxPageIndex)
                pagedList._currentPageIndex = maxPageIndex;
            
            UpdatePageItems(pagedList);
        };

        if (e.NewValue is INotifyCollectionChanged newObservableCollection)
        {
            newObservableCollection.CollectionChanged += onItemChanged;
        }
        
        if (e.OldValue is INotifyCollectionChanged oldObservableCollection)
        {
            oldObservableCollection.CollectionChanged -= onItemChanged;
        }
        
        UpdatePageItems(pagedList);
    }

    private static void UpdatePageItems(PagedList pagedList)
    {
        var itemsSource = pagedList.ItemsSource.Cast<object>();

        pagedList.CurrentPageText.Text = (pagedList._currentPageIndex + 1).ToString();
        pagedList.MaxPagesText.Text = (pagedList.RecalculateMaxPageIndex() + 1).ToString();

        pagedList.ItemListView.ItemsSource = itemsSource.Skip(
            pagedList._itemsPerPage * pagedList._currentPageIndex
        ).Take(pagedList._itemsPerPage);
    }

    private int RecalculateMaxPageIndex()
    {
        var itemsSource = ItemsSource.Cast<object>().ToList();

        _maxPageIndex = _itemsPerPage == 0 || itemsSource.Count == 0 ? 
            0 : (int) Math.Ceiling((double) itemsSource.Count / _itemsPerPage) - 1;

        return _maxPageIndex;
    }
    
    private void OnPreviousPageClicked(object sender, RoutedEventArgs e)
    {
        if (_currentPageIndex == 0) return;
    
        _currentPageIndex--;
        UpdatePageItems(this);
    }

    private void OnNextPageClicked(object sender, RoutedEventArgs e)
    {
        var itemsSource = ItemsSource.Cast<object>();
        if ((_currentPageIndex + 1) * _itemsPerPage >= itemsSource.Count()) return;
        
        _currentPageIndex++;
        UpdatePageItems(this);
    }

    private void OnSkipBackwardsClicked(object sender, RoutedEventArgs e)
    {
        var skipSize = _currentPageIndex - SkipSizeInPages < 0 ? _currentPageIndex : SkipSizeInPages; 

        _currentPageIndex -= skipSize;
        UpdatePageItems(this);
    }
    
    private void OnSkipForwardClicked(object sender, RoutedEventArgs e)
    {
        var skipSize = _currentPageIndex + SkipSizeInPages > _maxPageIndex ? 
            _maxPageIndex - _currentPageIndex : SkipSizeInPages; 
        
        _currentPageIndex += skipSize;
        UpdatePageItems(this);
    }

    private void OnHeightChanged(object sender, SizeChangedEventArgs e)
    {
        var pageList = (PagedList) sender;
        var itemListView = pageList.ItemListView;

        if (itemListView.Items.Count == 0) return;

        var sampleItem = (ListViewItem) itemListView.ItemContainerGenerator.ContainerFromItem(itemListView.Items[0]);
        var itemHeight = sampleItem.ActualHeight;
        
        _itemsPerPage = (int) Math.Floor(itemListView.ActualHeight / itemHeight);
        UpdatePageItems(pageList);
    }
}