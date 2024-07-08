using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scribe.UI.Views.Sections.Tags;

public partial class Tags : UserControl
{
    public Tags() => InitializeComponent();

    private void OnTagItemClicked(object sender, MouseButtonEventArgs e)
    {
        var tagItem = ((FrameworkElement) sender).DataContext;
        var tagsViewModel = (TagsViewModel) DataContext;

        if (tagsViewModel.ToggleTagSelectionCommand.CanExecute(tagItem))
        {
            tagsViewModel.ToggleTagSelectionCommand.Execute(tagItem);
        }
    }
}