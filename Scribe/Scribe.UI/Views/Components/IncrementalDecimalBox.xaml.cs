using System.Windows;
using System.Windows.Controls;
using Scribe.UI.Command;

namespace Scribe.UI.Views.Components;

public partial class IncrementalDecimalBox : UserControl
{
    public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register(
        name: nameof(CurrentValue),
        propertyType: typeof(double),
        ownerType: typeof(UserControl)
    );

    public double CurrentValue
    {
        get => (double) GetValue(CurrentValueProperty);
        set => SetValue(CurrentValueProperty, value);
    }
    
    public IncrementalDecimalBox()
    {
        InitializeComponent();
        IncreaseButton.Command = new DelegateCommand(_ => IncreaseValue());
        DecreaseButton.Command = new DelegateCommand(_ => DecreaseValue());
    }

    private void IncreaseValue() => CurrentValue += 0.05;
    
    private void DecreaseValue()
    {
        var newValue = CurrentValue - 0.05;
        CurrentValue = newValue < 0 ? 0 : newValue;
    }
}