using System.Windows;
using System.Windows.Controls;

namespace Scribe.UI.Views.Components;

public partial class SearchBox : UserControl
{
    public SearchBox() => InitializeComponent();

    public string SearchText
    {
        get => GetValue(SearchTextProperty) as string ?? "";
        set => SetValue(SearchTextProperty, value);
    }
    
    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        name: nameof(SearchText),
        propertyType: typeof(string),
        ownerType: typeof(SearchBox)
    );
}