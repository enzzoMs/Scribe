using NSubstitute;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.UI.Resources;
using Scribe.UI.Views.Sections.Configurations;

namespace Scribe.Tests.UI.Views;

public class ConfigurationsViewModelTests
{
    private readonly ConfigurationsViewModel _configurationsViewModel;
    private readonly IConfigurationsRepository _configurationsRepositoryMock = Substitute.For<IConfigurationsRepository>();
    private readonly IResourceManager _resourceManagerMock = Substitute.For<IResourceManager>();
    private readonly AppConfigurations _defaultConfiguration = new(ThemeConfiguration.Light, LanguageConfiguration.EnUs, 1.0);

    public ConfigurationsViewModelTests()
    {
        _configurationsRepositoryMock.GetAllConfigurations().Returns(_defaultConfiguration);

        _configurationsViewModel = new ConfigurationsViewModel(_configurationsRepositoryMock, _resourceManagerMock);
    }
    
    [Fact]
    public void InitializesWithCurrentConfigurations()
    {
        _configurationsRepositoryMock.Received(1).GetAllConfigurations();
        Assert.Equivalent(_defaultConfiguration, _configurationsViewModel.CurrentConfiguration);
        Assert.Equal(_defaultConfiguration.Scale, _configurationsViewModel.CurrentScale);
    }
    
    [Fact]
    public void SelectLangCommand_UpdatesConfiguration()
    {
        var langConfigs = new[] { LanguageConfiguration.PtBr, LanguageConfiguration.EnUs };

        foreach (var newLangConfig in langConfigs)
        {
            _configurationsViewModel.SelectLanguageCommand.Execute(newLangConfig);

            Assert.Equal(_configurationsViewModel.CurrentConfiguration.Language, newLangConfig);
        }
        
        _configurationsRepositoryMock.Received(2).SaveConfiguration(Arg.Any<AppConfigurations>());
    }
    
    [Fact]
    public void SelectThemeCommand_UpdatesConfiguration()
    {
        var themeConfigs = new[] { ThemeConfiguration.Dark, ThemeConfiguration.Light };

        foreach (var newThemeConfig in themeConfigs)
        {
            _configurationsViewModel.SelectThemeCommand.Execute(newThemeConfig);

            Assert.Equal(_configurationsViewModel.CurrentConfiguration.Theme, newThemeConfig);
        }
        
        _configurationsRepositoryMock.Received(2).SaveConfiguration(Arg.Any<AppConfigurations>());
    }
    
    [Theory]
    [InlineData(1.5)]
    [InlineData(1.05)]
    [InlineData(0.95)]
    [InlineData(0.7)]
    public void ScaleSetter_UpdatesConfiguration(double newScale)
    {
        _configurationsViewModel.CurrentScale = newScale;
            
        _configurationsRepositoryMock.Received(1).SaveConfiguration(Arg.Any<AppConfigurations>());
        Assert.Equal(_configurationsViewModel.CurrentScale, newScale);
    }

    [Fact]
    public void SelectLangCommand_UpdatesResource()
    {
        var langConfigs = new[] { LanguageConfiguration.PtBr, LanguageConfiguration.EnUs };

        foreach (var newLangConfig in langConfigs)
        {
            _configurationsViewModel.SelectLanguageCommand.Execute(newLangConfig);
            _resourceManagerMock.Received(1).UpdateLanguageResource(newLangConfig);
        }
    }
    
    [Fact]
    public void SelectThemeCommand_UpdatesResource()
    {
        var themeConfigs = new[] { ThemeConfiguration.Dark, ThemeConfiguration.Light };

        foreach (var newThemeConfig in themeConfigs)
        {
            _configurationsViewModel.SelectThemeCommand.Execute(newThemeConfig);
        
            _resourceManagerMock.Received(1).UpdateThemeResource(newThemeConfig);
        }
    }
    
    [Theory]
    [InlineData(1.5)]
    [InlineData(1.05)]
    [InlineData(0.95)]
    [InlineData(0.7)]
    public void ScaleSetter_UpdatesResource(double newScale)
    {
        _configurationsViewModel.CurrentScale = newScale;

        _resourceManagerMock.Received(1).UpdateDimensionsResource(newScale);
    }

    [Fact]
    public void UpdatingAnyConfig_RaisesPropertyChanged_ForCurrentConfig()
    {
        var propertyChangedCount = 0;

        _configurationsViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(_configurationsViewModel.CurrentConfiguration))
            {
                propertyChangedCount++;
            }
        };

        _configurationsViewModel.SelectLanguageCommand.Execute(LanguageConfiguration.PtBr);
        _configurationsViewModel.SelectThemeCommand.Execute(ThemeConfiguration.Dark);
        _configurationsViewModel.CurrentScale = 1.5;
        
        Assert.Equal(3, propertyChangedCount);
    }
}