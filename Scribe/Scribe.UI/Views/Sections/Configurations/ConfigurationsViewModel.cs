using System.Windows.Input;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.UI.Commands;
using Scribe.UI.Resources;

namespace Scribe.UI.Views.Sections.Configurations;

public class ConfigurationsViewModel : BaseViewModel
{
    private readonly IConfigurationsRepository _configurationsRepository;
    private readonly IResourceManager _resourceManager;
    
    private AppConfigurations _currentConfiguration;
    private double _currentScale;
    
    public ConfigurationsViewModel(IConfigurationsRepository configurationsRepository, IResourceManager resourceManager)
    {
        _configurationsRepository = configurationsRepository;
        _resourceManager = resourceManager;
        
        _currentConfiguration = configurationsRepository.GetAllConfigurations();
        _currentScale = _currentConfiguration.Scale;

        SelectLanguageCommand = new DelegateCommand(parameter =>
        {
            if (parameter is LanguageConfiguration langConfig)
            {
                SelectLanguage(langConfig);
            }
        });
        SelectThemeCommand = new DelegateCommand(parameter =>
        {
            if (parameter is ThemeConfiguration themeConfig)
            {
                SelectTheme(themeConfig);
            }
        });
    }
    
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

    public ICommand SelectLanguageCommand { get; }
    
    public ICommand SelectThemeCommand { get; }
    
    private void SelectLanguage(LanguageConfiguration langConfig)
    {
        if (CurrentConfiguration.Language == langConfig) return;
        
        CurrentConfiguration = _configurationsRepository.SaveConfiguration(langConfig);
        _resourceManager.UpdateLanguageResource(langConfig);
    }
    
    private void SelectTheme(ThemeConfiguration themeConfig)
    {
        if (CurrentConfiguration.Theme == themeConfig) return;
        
        CurrentConfiguration = _configurationsRepository.SaveConfiguration(themeConfig);
        _resourceManager.UpdateThemeResource(themeConfig);
    }

    private void ScaleApp()
    {
        const double scaleTolerance = 0.001;
        if (Math.Abs(CurrentConfiguration.Scale - _currentScale) < scaleTolerance) return;
        
        CurrentConfiguration = _configurationsRepository.SaveConfiguration(_currentScale);
        _resourceManager.UpdateDimensionsResource(_currentScale);
    }
}