using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Scribe.Markup.Nodes;
using Scribe.Markup.Nodes.Leafs;
using Scribe.UI.Views.Components;

namespace Scribe.UI.Converters;

public class MarkupConverter : IMultiValueConverter
{
    public object? Convert(object[]? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value?[0] is not MarkupEditor markupEditor || value[1] is not IMarkupNode markupNode) return null;

        var markupView = markupEditor.GetViewFromMarkupNode(markupNode);
        
        if (markupNode is not LabelNode)
        {
            markupView.Margin = new Thickness(
                left: 0, 
                top: MarkupEditor.MarkupViewMargin,
                right: 0,
                bottom: markupEditor.MarkupNodes?.Last() == markupNode ? MarkupEditor.MarkupViewMargin : 0
            );
        }

        return markupView;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}