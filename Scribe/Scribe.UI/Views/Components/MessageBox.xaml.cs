using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Scribe.UI.Views.Components;

public record MessageBoxOption(string OptionText, ICommand? Command = null, object? CommandParameter = null);

public partial class MessageBox : Window
{
    public MessageBox()
    {
        InitializeComponent();
        SetValue(OptionsProperty, new List<MessageBoxOption>());
    }

    public string Message
    {
        get => (string) GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    
    public List<MessageBoxOption> Options
    {
        get => (List<MessageBoxOption>) GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }
    
    public Geometry? MessageIconPath
    {
        get => (Geometry?) GetValue(MessageIconPathProperty);
        set => SetValue(MessageIconPathProperty, value);
    }
    
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        name: nameof(Message),
        propertyType: typeof(string),
        ownerType: typeof(MessageBox)
    );
    
    public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
        name: nameof(Options),
        propertyType: typeof(List<MessageBoxOption>),
        ownerType: typeof(MessageBox),
        new FrameworkPropertyMetadata(propertyChangedCallback: OnOptionsChanged)
    );
    
    public static readonly DependencyProperty MessageIconPathProperty = DependencyProperty.Register(
        name: nameof(MessageIconPath),
        propertyType: typeof(Geometry),
        ownerType: typeof(MessageBox)
    );

    private static void OnOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var messageBox = (MessageBox) d;

        foreach (var option in messageBox.Options)
        {
            var optionButton = new Button
            {
                Content = option.OptionText,
                Command = option.Command,
                CommandParameter = option.CommandParameter
            };
            optionButton.Click += messageBox.OnOptionClicked;

            messageBox.OptionsGrid.Columns += 1;
            messageBox.OptionsGrid.Children.Add(optionButton);
        }
        
        messageBox.OptionsGrid.Visibility = messageBox.Options.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        messageBox.MessageDivider.Visibility = messageBox.Options.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        Grid.SetRowSpan(messageBox.MessageBlock,  messageBox.Options.Count == 0 ? 3 : 1);
    }

    private void OnOptionClicked(object sender, RoutedEventArgs e)
    {
        // Closing dialog
        DialogResult = false;   
    }
}