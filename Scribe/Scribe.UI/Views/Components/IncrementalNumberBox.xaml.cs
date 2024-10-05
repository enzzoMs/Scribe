using System.Windows;
using System.Windows.Controls;
using Scribe.UI.Commands;

namespace Scribe.UI.Views.Components;

public partial class IncrementalNumberBox : UserControl
{
    public IncrementalNumberBox()
    {
        InitializeComponent();
        IncreaseButton.Command = new DelegateCommand(_ => IncreaseValue());
        DecreaseButton.Command = new DelegateCommand(_ => DecreaseValue());
    }

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
    
    public double? MinValue
    {
        get => (double?) GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }
    
    public double? MaxValue
    {
        get => (double?) GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
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
    
    public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register(
        name: nameof(CurrentValue),
        propertyType: typeof(double),
        ownerType: typeof(IncrementalNumberBox)
    );
    
    public static readonly DependencyProperty IncrementStepProperty = DependencyProperty.Register(
        name: nameof(IncrementStep),
        propertyType: typeof(double),
        ownerType: typeof(IncrementalNumberBox)
    );
    
    public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
        name: nameof(MinValue),
        propertyType: typeof(double?),
        ownerType: typeof(IncrementalNumberBox)
    );
    
    public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
        name: nameof(MaxValue),
        propertyType: typeof(double?),
        ownerType: typeof(IncrementalNumberBox)
    );
    
    public static readonly DependencyProperty TextSizeProperty = DependencyProperty.Register(
        name: nameof(TextSize),
        propertyType: typeof(double),
        ownerType: typeof(IncrementalNumberBox)
    );
        
    public static readonly DependencyProperty ShowDecimalPlacesProperty = DependencyProperty.Register(
        name: nameof(ShowDecimalPlaces),
        propertyType: typeof(bool),
        ownerType: typeof(IncrementalNumberBox)
    );

    private void IncreaseValue()
    {
        var newValue = CurrentValue + IncrementStep;
        if (MaxValue == null)
        {
            CurrentValue = newValue;
            return;
        }

        CurrentValue = newValue > MaxValue.Value ? MaxValue.Value : newValue;
    }
    
    private void DecreaseValue()
    {
        var newValue = CurrentValue - IncrementStep;
        if (MinValue == null)
        {
            CurrentValue = newValue;
            return;
        }
        
        CurrentValue = newValue < MinValue.Value ? MinValue.Value : newValue;
    }
}