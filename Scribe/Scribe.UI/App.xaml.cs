using System.Windows;
using System.Windows.Media;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Commands;
using Scribe.UI.Events;
using Scribe.UI.Helpers;
using Scribe.UI.Resources;
using Scribe.UI.Views.Components;
using Scribe.UI.Views.Screens.Main;
using Scribe.UI.Views.Screens.Splash;
using Scribe.UI.Views.Screens.Window;
using Scribe.UI.Views.Sections.Configurations;
using Scribe.UI.Views.Sections.Documents;
using Scribe.UI.Views.Sections.Editor;
using Scribe.UI.Views.Sections.FolderDetails;
using Scribe.UI.Views.Sections.Navigation;
using Scribe.UI.Views.Sections.Tags;
using MessageBox = Scribe.UI.Views.Components.MessageBox;
using Window = Scribe.UI.Views.Screens.Window.Window;

namespace Scribe.UI;

public partial class App : Application
{
    public const int ThemeDictionaryIndex = 0;
    public const int DimensDictionaryIndex = 1;
    public const int StringsDictionaryIndex = 2;

    private const int CantOpenDatabaseErrorCode = 14;
    
    protected override void OnStartup(StartupEventArgs eventArgs)
    {
        base.OnStartup(eventArgs);
        
        var currentConfigurations = new ConfigurationsRepository().GetAllConfigurations();
        var resourceManager = new ResourceManager();
        
        resourceManager.UpdateLanguageResource(currentConfigurations.Language);
        resourceManager.UpdateThemeResource(currentConfigurations.Theme);
        resourceManager.UpdateDimensionsResource(currentConfigurations.Scale);
        
        var services = new ServiceCollection();
        ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();
        var window = serviceProvider.GetService<Window>();

        Current.DispatcherUnhandledException += (_, args) =>
        {
            if (args.Exception is not SqliteException { SqliteErrorCode: CantOpenDatabaseErrorCode }) return;
            
            new MessageBox
            {
                Owner = Current.MainWindow,
                Title = Resources["String.Error"] as string,
                MessageIconPath = Resources["Drawing.Exclamation"] as Geometry,
                Message = Resources["String.Error.Database"] as string ?? "",
                Options = [new MessageBoxOption(
                    Resources["String.Button.Understood"] as string ?? "",
                    new DelegateCommand(_ => Current.MainWindow?.Close())
                )]
            }.ShowDialog();
            
            args.Handled = true;
        };
        
        window?.Show();
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
        services.AddTransient<IPdfHelper, PdfHelper>();
        services.AddTransient<IResourceManager, ResourceManager>();
    }
}