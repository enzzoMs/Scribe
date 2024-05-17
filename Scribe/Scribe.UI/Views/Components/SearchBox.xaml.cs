using System.Windows;
using System.Windows.Controls;

namespace Scribe.UI.Views.Components;

public partial class SearchBox : UserControl
{
    public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register(
        name: nameof(HintText),
        propertyType: typeof(string),
        ownerType: typeof(UserControl),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: "")
    );

    public string HintText
    {
        get => GetValue(HintTextProperty) as string ?? "";
        set => SetValue(HintTextProperty, value);
    }

    public SearchBox() => InitializeComponent();
}