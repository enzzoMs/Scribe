using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Resources;
using Scribe.UI.Views.Screens.Editor;
using Scribe.UI.Views.Screens.Main;
using Scribe.UI.Views.Screens.Splash;
using Scribe.UI.Views.Sections.Configurations;
using Scribe.UI.Views.Sections.FolderDetails;
using Scribe.UI.Views.Sections.Navigation;
using MainWindow = Scribe.UI.Views.Screens.Main.MainWindow;

namespace Scribe.UI;

public partial class App : Application
{
    public const int ThemeDictionaryIndex = 4;
    public const int DimensDictionaryIndex = 5;
    public const int StringsDictionaryIndex = 6;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var currentConfigurations = new ConfigurationsRepository().GetAllConfigurations();
        var resourceManager = new ResourceManager();
        
        resourceManager.UpdateLanguageResource(currentConfigurations.Language);
        resourceManager.UpdateThemeResource(currentConfigurations.Theme);
        resourceManager.UpdateDimensionsResource(currentConfigurations.Scale);
        
        var services = new ServiceCollection();
        ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<MainWindow>()?.Show();
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventAggregator, EventAggregator>();
        
        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<SplashViewModel>();
        services.AddTransient<EditorViewModel>();
        services.AddTransient<NavigationViewModel>();
        services.AddTransient<ConfigurationsViewModel>();
        services.AddTransient<FolderDetailsViewModel>();

        services.AddTransient<IRepository<Folder>, FolderRepository>();
        
        services.AddTransient<IConfigurationsRepository, ConfigurationsRepository>();
        
        services.AddTransient<IResourceManager, ResourceManager>();
    }
}