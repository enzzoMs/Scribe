using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Scribe.UI.Views.Components;

public partial class IconButton : UserControl
{
    public static readonly DependencyProperty IconGeometryProperty = DependencyProperty.Register(
        name: nameof(IconGeometry),
        propertyType: typeof(Geometry),
        ownerType: typeof(UserControl)
    );
    
    public static readonly DependencyProperty IconPaddingProperty = DependencyProperty.Register(
        name: nameof(IconPadding),
        propertyType: typeof(double),
        ownerType: typeof(UserControl)
    );
    
    public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
        name: nameof(IconBrush),
        propertyType: typeof(Brush),
        ownerType: typeof(UserControl)
    );

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        name: nameof(Command),
        propertyType: typeof(ICommand),
        ownerType: typeof(UserControl)
    );
    
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        name: nameof(CommandParameter),
        propertyType: typeof(object),
        ownerType: typeof(UserControl)
    );
    
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
    
    public ICommand? Command
    {
        get => GetValue(CommandProperty) as ICommand;
        set => SetValue(CommandProperty, value);
    }
    
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public IconButton() => InitializeComponent();
}