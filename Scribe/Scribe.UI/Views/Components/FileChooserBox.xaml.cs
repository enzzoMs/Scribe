using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Scribe.UI.Views.Components;

public partial class FileChooserBox : Window
{
    private string[]? _chosenFilePaths;
    
    public FileChooserBox() => InitializeComponent();

    public string? FileFilter
    {
        get => (string?) GetValue(FileFilterProperty);
        set => SetValue(FileFilterProperty, value);
    }
    
    /// <summary>
    /// An ICommand that is executed when the "Confirm" button is clicked.
    /// </summary>
    /// <remarks>
    /// It is passed an string array containing the selected file paths.
    /// </remarks>
    public ICommand? ConfirmActionCommand
    {
        get => (ICommand?) GetValue(ConfirmActionCommandProperty);
        set => SetValue(ConfirmActionCommandProperty, value);
    }
    
    private static readonly DependencyProperty FileFilterProperty = DependencyProperty.Register(
        name: nameof(FileFilter),
        propertyType: typeof(string),
        ownerType: typeof(FileChooserBox)
    );
    
    private static readonly DependencyProperty ConfirmActionCommandProperty = DependencyProperty.Register(
        name: nameof(ConfirmActionCommand),
        propertyType: typeof(ICommand),
        ownerType: typeof(FileChooserBox)
    );
    
    private void OnChooseFilesClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = FileFilter,
            Multiselect = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _chosenFilePaths = dialog.FileNames;
            ChosenFilePathsList.ItemsSource = _chosenFilePaths;
        }
    }
    
    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        _chosenFilePaths = [];
        ChosenFilePathsList.ItemsSource = _chosenFilePaths;
    }

    private void OnConfirmActionClicked(object sender, RoutedEventArgs e)
    {
        ConfirmActionCommand?.Execute(_chosenFilePaths!);
        DialogResult = true;
    }
}