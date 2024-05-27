using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
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
    public int StringsDictionaryIndex { get; private set; }
    public int ThemeDictionaryIndex { get; private set; }
    public int DimensDictionaryIndex { get; private set; }
    
    public static App Self() => (App) Current;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        RemoveDesignTimeResources();
        
        var currentConfigurations = ConfigurationsManager.GetAllConfigurations();
        ApplyLanguageConfig(currentConfigurations.Language);
        ApplyThemeConfig(currentConfigurations.Theme);
        ApplyScaleConfig(currentConfigurations.Scale);

        var services = new ServiceCollection();
        ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<MainWindow>()?.Show();
    }

    private void RemoveDesignTimeResources()
    {
        Resources.MergedDictionaries.RemoveAt(6);
        Resources.MergedDictionaries.RemoveAt(5);
        Resources.MergedDictionaries.RemoveAt(4);
    }
    
    private void ApplyLanguageConfig(LanguageConfiguration languageConfig)
    {
        var langDictionaryPath = $"/Resources/Strings/Strings.{languageConfig}.xaml";

        StringsDictionaryIndex = Resources.MergedDictionaries.Count;

        Resources.MergedDictionaries.Add(new ResourceDictionary { 
            Source = new Uri(langDictionaryPath, UriKind.Relative)
        });
    }

    private void ApplyThemeConfig(ThemeConfiguration themeConfig)
    {
        var themeDictionaryPath = $"/Resources/Brushes/Brushes.{themeConfig}.xaml";
        
        ThemeDictionaryIndex = Resources.MergedDictionaries.Count;
        
        Resources.MergedDictionaries.Add(new ResourceDictionary { 
            Source = new Uri(themeDictionaryPath, UriKind.Relative)
        });

    }

    private void ApplyScaleConfig(double scale)
    {
        DimensDictionaryIndex = Resources.MergedDictionaries.Count;
        
        var baseDimensions = new ResourceDictionary { Source = new Uri("Resources/Dimens.xaml", UriKind.Relative) };
        var newDimensions = new ResourceDictionary { Source = new Uri("/Resources/Dimens.xaml", UriKind.Relative) };
        
        foreach (var dimensionKey in newDimensions.Keys)
        {
            var dimension = baseDimensions[dimensionKey];

            newDimensions[dimensionKey] = dimension switch
            {
                double d => d * scale,
                Thickness t => new Thickness(
                    left: t.Left * scale, 
                    right: t.Right * scale,
                    top: t.Top * scale, 
                    bottom: t.Bottom * scale
                ),
                CornerRadius r => new CornerRadius(
                    topLeft: r.TopLeft * scale,
                    topRight: r.TopRight * scale, 
                    bottomLeft: r.BottomLeft * scale,
                    bottomRight: r.BottomRight * scale
                ),
                _ => dimension
            };
        }
        
        Resources.MergedDictionaries.Add(newDimensions);
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
    }
}