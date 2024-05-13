using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Scribe.Data.Database;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Views.Main;
using Scribe.UI.Views.Splash;

namespace Scribe.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<MainWindow>()?.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<SplashViewModel>();

        services.AddTransient<ScribeContext>();
        services.AddTransient<IRepository<Folder>, FolderRepository>();
    }
}