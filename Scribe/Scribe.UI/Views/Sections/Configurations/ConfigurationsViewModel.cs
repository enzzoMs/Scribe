using System.Windows;
using System.Windows.Input;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.UI.Command;

namespace Scribe.UI.Views.Sections.Configurations;

public class ConfigurationsViewModel : BaseViewModel
{
    private AppConfigurations _currentConfiguration;
    private double _currentScale;
    
    public AppConfigurations CurrentConfiguration
    {
        get => _currentConfiguration;
        private set
        {
            _currentConfiguration = value;
            RaisePropertyChanged();
        }
    }

    public double CurrentScale
    {
        get => _currentScale;
        set
        {
            _currentScale = value;
            ScaleApp();
        }
    }

    public ICommand SelectLanguageCommand { get; private set; }
    public ICommand SelectThemeCommand { get; private set; }

    public ConfigurationsViewModel()
    {
        _currentConfiguration = ConfigurationsManager.GetAllConfigurations();
        _currentScale = _currentConfiguration.Scale;
        
        SelectLanguageCommand = new DelegateCommand(SelectLanguage);
        SelectThemeCommand = new DelegateCommand(SelectTheme);
    }

    private void SelectLanguage(object? parameter)
    {
        if (parameter is not LanguageConfiguration langConfig) return;
        
        var newConfiguration = ConfigurationsManager.SaveConfiguration(langConfig);

        if (newConfiguration != CurrentConfiguration)
        {
            var langDictionaryPath = $"/Resources/Strings/Strings.{langConfig}.xaml";
            var langDictionaryIndex = App.Self().StringsDictionaryIndex;

            Application.Current.Resources.MergedDictionaries[langDictionaryIndex] = new ResourceDictionary
            {
                Source = new Uri(langDictionaryPath, UriKind.Relative)
            };
        }

        CurrentConfiguration = newConfiguration;
    }
    
    private void SelectTheme(object? parameter)
    {
        if (parameter is not ThemeConfiguration themeConfig) return;
        
        var newConfiguration = ConfigurationsManager.SaveConfiguration(themeConfig);

        if (newConfiguration != CurrentConfiguration)
        {
            var themeDictionaryPath = $"/Resources/Brushes/Brushes.{themeConfig}.xaml";
            var themeDictionaryIndex = App.Self().ThemeDictionaryIndex;

            Application.Current.Resources.MergedDictionaries[themeDictionaryIndex] = new ResourceDictionary
            {
                Source = new Uri(themeDictionaryPath, UriKind.Relative)
            };
        }

        CurrentConfiguration = newConfiguration;
    }

    private void ScaleApp()
    {
        var newConfiguration = ConfigurationsManager.SaveConfiguration(_currentScale);

        if (newConfiguration == CurrentConfiguration) return;

        CurrentConfiguration = newConfiguration;

        var baseDimensions = new ResourceDictionary { Source = new Uri("Resources/Dimens.xaml", UriKind.Relative) };

        var currentDimensions = Application.Current.Resources.MergedDictionaries[App.Self().DimensDictionaryIndex];
        
        foreach (var dimensionKey in currentDimensions.Keys)
        {
            var dimension = baseDimensions[dimensionKey];

            currentDimensions[dimensionKey] = dimension switch
            {
                double d => d * _currentScale,
                Thickness t => new Thickness(
                    left: t.Left * _currentScale, 
                    right: t.Right * _currentScale,
                    top: t.Top * _currentScale, 
                    bottom: t.Bottom * _currentScale
                ),
                CornerRadius r => new CornerRadius(
                    topLeft: r.TopLeft * _currentScale,
                    topRight: r.TopRight * _currentScale, 
                    bottomLeft: r.BottomLeft * _currentScale,
                    bottomRight: r.BottomRight * _currentScale
                ),
                GridLength g => new GridLength(g.Value * _currentScale),
                _ => dimension
            };
        }
    }
}