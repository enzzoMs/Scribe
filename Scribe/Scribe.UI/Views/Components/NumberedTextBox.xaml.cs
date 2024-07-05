using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Scribe.UI.Views.Components;

/// <summary>
/// A custom UserControl that provides a TextBox with line numbers.
/// </summary>
public partial class NumberedTextBox : UserControl
{
    private const int LineStartToleranceInChars = 5;
    private int _lineCount = 1;
    
    public NumberedTextBox() => InitializeComponent();

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (MainTextBox.LineCount < _lineCount)
        {
            _lineCount = MainTextBox.LineCount;
                
            // Remove the extra line numbers
            LineNumbersTextBlock.Text = LineNumbersTextBlock.Text[
                ..LineNumbersTextBlock.Text.IndexOf($"\n{_lineCount + 1}", StringComparison.Ordinal)
            ];
        }
        else if (MainTextBox.LineCount > _lineCount)
        {
            var lineNumbersText = new StringBuilder(LineNumbersTextBlock.Text);
                
            for (var lineNumber = _lineCount + 1; lineNumber <= MainTextBox.LineCount; lineNumber++)
            {
                lineNumbersText.Append('\n');
                lineNumbersText.Append(lineNumber);
            }

            LineNumbersTextBlock.Text = lineNumbersText.ToString();
            _lineCount = MainTextBox.LineCount;

            MainTextBox.ScrollToLine(_lineCount - 1);
        }
            
        var largestLineLength = MainTextBox.Text.Split("\n").Max(line => line.Length);
        var currentLine = MainTextBox.GetLineIndexFromCharacterIndex(MainTextBox.CaretIndex);
        var currentLineStartIndex = MainTextBox.GetCharacterIndexFromLineIndex(currentLine);

        // Scroll to the right end if the caret is at the end of the longest line.
        // This ensures that the text does not remain hidden behind the vertical scrollbar.
        if (MainTextBox.CaretIndex - currentLineStartIndex >= largestLineLength - 1)
        {
            MainScrollViewer.ScrollToRightEnd();
        }
    }

    private void OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        var currentLine = MainTextBox.GetLineIndexFromCharacterIndex(MainTextBox.CaretIndex);
        var currentLineStartIndex = MainTextBox.GetCharacterIndexFromLineIndex(currentLine);

        // Scroll to the left end if the caret is near the start of the line
        if (MainTextBox.CaretIndex < currentLineStartIndex + LineStartToleranceInChars)
        {
            MainScrollViewer.ScrollToLeftEnd();
        }
    }
}