using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Resources;
using Scribe.UI.Views.Screens.Main;
using Scribe.UI.Views.Screens.Splash;
using Scribe.UI.Views.Screens.Window;
using Scribe.UI.Views.Sections.Configurations;
using Scribe.UI.Views.Sections.Documents;
using Scribe.UI.Views.Sections.Editor;
using Scribe.UI.Views.Sections.FolderDetails;
using Scribe.UI.Views.Sections.Navigation;
using Scribe.UI.Views.Sections.Tags;
using Window = Scribe.UI.Views.Screens.Window.Window;

namespace Scribe.UI;

public partial class App : Application
{
    public const int ThemeDictionaryIndex = 6;
    public const int DimensDictionaryIndex = 7;
    public const int StringsDictionaryIndex = 8;
    
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
        serviceProvider.GetService<Window>()?.Show();
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventAggregator, EventAggregator>();
        
        services.AddTransient<Window>();
        services.AddTransient<WindowViewModel>();
        services.AddTransient<SplashViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<NavigationViewModel>();
        services.AddTransient<ConfigurationsViewModel>();
        services.AddTransient<FolderDetailsViewModel>();
        services.AddTransient<DocumentsViewModel>();
        services.AddTransient<TagsViewModel>();
        services.AddTransient<EditorViewModel>();

        services.AddTransient<IRepository<Folder>, FolderRepository>();
        services.AddTransient<IRepository<Document>, DocumentRepository>();
        services.AddTransient<IRepository<Tag>, TagRepository>();

        services.AddTransient<IConfigurationsRepository, ConfigurationsRepository>();
        
        services.AddTransient<IResourceManager, ResourceManager>();
    }
}