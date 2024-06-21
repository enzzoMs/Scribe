using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Scribe.UI.Views.Components;

public partial class IconButton : Button
{
    public IconButton() => InitializeComponent();

    public Geometry? IconGeometry
    {
        get => GetValue(IconGeometryProperty) as Geometry;
        set => SetValue(IconGeometryProperty, value);
    }
    
    public double IconPadding
    {
        get => GetValue(IconPaddingProperty) as double? ?? 0;
        set => SetValue(IconPaddingProperty, value);
    }
    
    public Brush? IconBrush
    {
        get => GetValue(IconBrushProperty) as Brush;
        set => SetValue(IconBrushProperty, value);
    }
    
    public static readonly DependencyProperty IconGeometryProperty = DependencyProperty.Register(
        name: nameof(IconGeometry),
        propertyType: typeof(Geometry),
        ownerType: typeof(IconButton)
    );
    
    public static readonly DependencyProperty IconPaddingProperty = DependencyProperty.Register(
        name: nameof(IconPadding),
        propertyType: typeof(double),
        ownerType: typeof(IconButton)
    );
    
    public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
        name: nameof(IconBrush),
        propertyType: typeof(Brush),
        ownerType: typeof(IconButton)
    );
}