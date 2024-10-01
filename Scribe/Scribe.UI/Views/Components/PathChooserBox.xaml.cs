using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace Scribe.UI.Views.Components;

public partial class PathChooserBox : Window
{
    public PathChooserBox()
    {
        InitializeComponent();
        SetValue(OptionsProperty, new List<object>());
    }
    
    public string? ChosenPath
    {
        get => (string?) GetValue(ChosenPathProperty);
        private set => SetValue(ChosenPathProperty, value);
    }
    
    public List<object> Options
    {
        get => (List<object>) GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }
    
    /// <summary>
    /// An ICommand that is executed when the "Confirm" button is clicked.
    /// </summary>
    /// <remarks>
    /// It is passed a tuple containing two values: <br/>
    /// 1. Option: A string representing the selected option from the OptionsBox. If no option was specified, it passes null.<br/>
    /// 2. Path: The chosen directory path. It cannot be null.
    /// </remarks>
    public ICommand? ConfirmActionCommand
    {
        get => (ICommand?) GetValue(ConfirmActionCommandProperty);
        set => SetValue(ConfirmActionCommandProperty, value);
    }
    
    public string? ConfirmActionMessage
    {
        get => (string?) GetValue(ConfirmActionMessageProperty);
        set => SetValue(ConfirmActionMessageProperty, value);
    }
    
    private static readonly DependencyProperty ChosenPathProperty = DependencyProperty.Register(
        name: nameof(ChosenPath),
        propertyType: typeof(string),
        ownerType: typeof(PathChooserBox)
    );
    
    private static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
        name: nameof(Options),
        propertyType: typeof(List<object>),
        ownerType: typeof(PathChooserBox),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnOptionsChanged)
    );

    private static readonly DependencyProperty ConfirmActionCommandProperty = DependencyProperty.Register(
        name: nameof(ConfirmActionCommand),
        propertyType: typeof(ICommand),
        ownerType: typeof(PathChooserBox)
    );
    
    private static readonly DependencyProperty ConfirmActionMessageProperty = DependencyProperty.Register(
        name: nameof(ConfirmActionMessage),
        propertyType: typeof(string),
        ownerType: typeof(PathChooserBox)
    );

    private static void OnOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var chooserBox = (PathChooserBox) d;

        if (chooserBox.Options.Count == 0)
        {
            chooserBox.OptionsBox.Visibility = Visibility.Collapsed;
            return;
        }

        chooserBox.OptionsBox.Visibility = Visibility.Visible;
        
        foreach (var option in chooserBox.Options)
        {
            var optionMenuItem = new MenuItem { Header = option };
            optionMenuItem.PreviewMouseLeftButtonDown += OnOptionClicked;
            chooserBox.OptionsBox.MenuItems.Add(optionMenuItem);
        }

        chooserBox.OptionsBox.Text = chooserBox.Options[0].ToString() ?? "";
        return;
        
        void OnOptionClicked(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var menuItem = (MenuItem) sender;
            chooserBox.OptionsBox.Text = menuItem.Header.ToString() ?? "";
        }
    }
    
    private void OnChoosePathClicked(object sender, RoutedEventArgs e)
    {
        var folderBrowserDialog = new FolderBrowserDialog();

        if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ChosenPath = folderBrowserDialog.SelectedPath;
        }
    }

    private void OnConfirmActionClicked(object sender, RoutedEventArgs e)
    {
        ConfirmActionCommand?.Execute(
            (Option: string.IsNullOrEmpty(OptionsBox.Text) ? null : OptionsBox.Text, Path: ChosenPath)
        );
        DialogResult = true;
    }
}