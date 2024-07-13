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
    
    private bool _textBoxIsFocused;
    
    public NumberedTextBox()
    {
        InitializeComponent();

        MainTextBox.GotFocus += (_, _) => { _textBoxIsFocused = true; };
        MainTextBox.LostFocus += (_, _) => { _textBoxIsFocused = false; };
    }

    public event EventHandler<string>? TextChanged;
    
    public string Text
    {
        get => (string) GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        name: nameof(Text),
        propertyType: typeof(string),
        ownerType: typeof(NumberedTextBox)
    );
    
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (MainTextBox.LineCount == -1) return;
        
        // If lines were removed
        if (MainTextBox.LineCount < _lineCount)
        {
            _lineCount = MainTextBox.LineCount;

            var lastLineNumberIndex = LineNumbersTextBlock.Text.IndexOf($"\n{_lineCount + 1}", StringComparison.Ordinal);
            
            if (lastLineNumberIndex == -1) return;
            
            // Remove the extra line numbers
            LineNumbersTextBlock.Text = LineNumbersTextBlock.Text[..lastLineNumberIndex];
        }
        // If lines were added
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
        }
            
        var largestLineLength = MainTextBox.Text.Split("\n").Max(line => line.Length);
        var currentLine = MainTextBox.GetLineIndexFromCharacterIndex(MainTextBox.CaretIndex);
        
        if (currentLine == -1) return;
        
        var currentLineStartIndex = MainTextBox.GetCharacterIndexFromLineIndex(currentLine);

        // Scroll to the right end if the caret is at the end of the longest line.
        // This ensures that the text does not remain hidden behind the vertical scrollbar.
        if (MainTextBox.CaretIndex - currentLineStartIndex >= largestLineLength - 1)
        {
            MainScrollViewer.ScrollToRightEnd();
        }
        
        TextChanged?.Invoke(this, MainTextBox.Text);
    }

    private void OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        var currentLine = MainTextBox.GetLineIndexFromCharacterIndex(MainTextBox.CaretIndex);
        
        if (currentLine == -1) return;
        
        var currentLineStartIndex = MainTextBox.GetCharacterIndexFromLineIndex(currentLine);

        // Scroll to the left end if the caret is near the start of the line
        if (MainTextBox.CaretIndex < currentLineStartIndex + LineStartToleranceInChars)
        {
            MainScrollViewer.ScrollToLeftEnd();
        }
    }

    private void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        // Prevents scroll when the 'MainTextBox' is first focused
        if (!_textBoxIsFocused)
        {
            e.Handled = true;
        }
    }
}