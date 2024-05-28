using NSubstitute;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.UI.Resources;
using Scribe.UI.Views.Sections.Configurations;

namespace Scribe.Tests.UI.Views;

public class ConfigurationsViewModelTests
{
    private readonly IConfigurationsRepository _configurationsRepositoryMock = Substitute.For<IConfigurationsRepository>();
    private readonly IResourceManager _resourceManagerMock = Substitute.For<IResourceManager>();
    private readonly AppConfigurations _initialConfiguration = new(ThemeConfiguration.Light, LanguageConfiguration.EnUs, 1.0);

    public ConfigurationsViewModelTests() 
        => _configurationsRepositoryMock.GetAllConfigurations().Returns(_initialConfiguration);
    
    [Fact]
    public void InitializesWithCurrentConfigurations()
    {
        var configurationsViewModel = new ConfigurationsViewModel(_configurationsRepositoryMock, _resourceManagerMock);

        _configurationsRepositoryMock.Received(1).GetAllConfigurations();
        Assert.Equivalent(_initialConfiguration, configurationsViewModel.CurrentConfiguration);
        Assert.Equal(_initialConfiguration.Scale, configurationsViewModel.CurrentScale);
    }

    [Theory]
    [InlineData(LanguageConfiguration.EnUs)]
    [InlineData(LanguageConfiguration.PtBr)]
    public void SelectLangCommand_UpdatesConfiguration(LanguageConfiguration newLangConfig)
    {
        foreach (var config in Enum.GetValues<LanguageConfiguration>())
        {
            _configurationsRepositoryMock.SaveConfiguration(config).Returns( _initialConfiguration with { Language = config });
        }
        
        var configurationsViewModel = new ConfigurationsViewModel(_configurationsRepositoryMock, _resourceManagerMock);
        
        configurationsViewModel.SelectLanguageCommand.Execute(newLangConfig);

        Assert.Equal(configurationsViewModel.CurrentConfiguration.Language, newLangConfig);
    }
    
    [Theory]
    [InlineData(ThemeConfiguration.Light)]
    [InlineData(ThemeConfiguration.Dark)]
    public void SelectThemeCommand_UpdatesConfiguration(ThemeConfiguration newThemeConfig)
    {
        foreach (var config in Enum.GetValues<ThemeConfiguration>())
        {
            _configurationsRepositoryMock.SaveConfiguration(config).Returns( _initialConfiguration with { Theme = config });
        }
        
        var configurationsViewModel = new ConfigurationsViewModel(_configurationsRepositoryMock, _resourceManagerMock);
        
        configurationsViewModel.SelectThemeCommand.Execute(newThemeConfig);

        Assert.Equal(configurationsViewModel.CurrentConfiguration.Theme, newThemeConfig);
    }
    
    [Theory]
    [InlineData(1.5)]
    [InlineData(1.05)]
    [InlineData(0.99999995)]
    [InlineData(0.05)]
    public void ScaleSetter_UpdatesConfiguration(double newScale)
    {
        _configurationsRepositoryMock.SaveConfiguration(newScale).Returns( _initialConfiguration with { Scale = newScale });
        
        var configurationsViewModel = new ConfigurationsViewModel(_configurationsRepositoryMock, _resourceManagerMock)
        {
            CurrentScale = newScale
        };

        Assert.Equal(configurationsViewModel.CurrentScale, newScale);
    }

    [Fact]
    public void SelectLangCommand_Calls_UpdateResource()
    {
        const LanguageConfiguration newLangConfig = LanguageConfiguration.PtBr;
        
        var configurationsViewModel = new ConfigurationsViewModel(_configurationsRepositoryMock, _resourceManagerMock);

        configurationsViewModel.SelectLanguageCommand.Execute(newLangConfig);
        
        _resourceManagerMock.Received(1).UpdateLanguageResource(newLangConfig);
    }
    
    [Fact]
    public void SelectThemeCommand_Calls_UpdateResource()
    {
        const ThemeConfiguration newThemeConfig = ThemeConfiguration.Dark;
        
        var configurationsViewModel = new ConfigurationsViewModel(_configurationsRepositoryMock, _resourceManagerMock);

        configurationsViewModel.SelectThemeCommand.Execute(newThemeConfig);
        
        _resourceManagerMock.Received(1).UpdateThemeResource(newThemeConfig);
    }
    
    [Fact]
    public void ScaleSetter_Calls_UpdateResource()
    {
        const double newScale = 1.5;
        
        new ConfigurationsViewModel(_configurationsRepositoryMock, _resourceManagerMock)
        {
            CurrentScale = newScale
        };

        _resourceManagerMock.Received(1).UpdateDimensionsResource(newScale);
    }
}