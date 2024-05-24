using System.Windows;
using System.Windows.Controls;

namespace Scribe.UI.Views.Sections.Navigation.Expanded;

public partial class ExpandedNavSection : UserControl
{
    public static readonly DependencyProperty CondensedWidthProperty = DependencyProperty.Register(
        name: nameof(CondensedWidth),
        propertyType: typeof(double),
        ownerType: typeof(UserControl)
    );
    
    public double CondensedWidth
    {
        get => (double) GetValue(CondensedWidthProperty);
        set => SetValue(CondensedWidthProperty, value);
    }
    
    public ExpandedNavSection() => InitializeComponent();
}