using System.Windows;
using System.Windows.Controls;
using Scribe.UI.Command;

namespace Scribe.UI.Views.Components;

public partial class IncrementalNumberBox : UserControl
{
    public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register(
        name: nameof(CurrentValue),
        propertyType: typeof(double),
        ownerType: typeof(UserControl)
    );
    
    public static readonly DependencyProperty IncrementStepProperty = DependencyProperty.Register(
        name: nameof(IncrementStep),
        propertyType: typeof(double),
        ownerType: typeof(UserControl)
    );
    
    public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
        name: nameof(MinValue),
        propertyType: typeof(double),
        ownerType: typeof(UserControl)
    );
    
    public static readonly DependencyProperty TextSizeProperty = DependencyProperty.Register(
        name: nameof(TextSize),
        propertyType: typeof(double),
        ownerType: typeof(UserControl)
    );
        
    public static readonly DependencyProperty ShowDecimalPlacesProperty = DependencyProperty.Register(
        name: nameof(ShowDecimalPlaces),
        propertyType: typeof(bool),
        ownerType: typeof(UserControl)
    );

    public double CurrentValue
    {
        get => (double) GetValue(CurrentValueProperty);
        set => SetValue(CurrentValueProperty, value);
    }
    
    public double IncrementStep
    {
        get => (double) GetValue(IncrementStepProperty);
        set => SetValue(IncrementStepProperty, value);
    }
    
    public double MinValue
    {
        get => (double) GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }
    
    public bool ShowDecimalPlaces
    {
        get => (bool) GetValue(ShowDecimalPlacesProperty);
        set => SetValue(ShowDecimalPlacesProperty, value);
    }
    
    public double TextSize
    {
        get => (double) GetValue(TextSizeProperty);
        set => SetValue(TextSizeProperty, value);
    }
    
    public IncrementalNumberBox()
    {
        InitializeComponent();
        IncreaseButton.Command = new DelegateCommand(_ => IncreaseValue());
        DecreaseButton.Command = new DelegateCommand(_ => DecreaseValue());
    }

    private void IncreaseValue() => CurrentValue += IncrementStep;
    
    private void DecreaseValue()
    {
        var newValue = CurrentValue - IncrementStep;
        CurrentValue = newValue < MinValue ? MinValue : newValue;
    }
}