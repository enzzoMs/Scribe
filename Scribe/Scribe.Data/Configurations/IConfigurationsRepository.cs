using Scribe.Data.Model;

namespace Scribe.Data.Configurations;

public interface IConfigurationsRepository
{
    AppConfigurations GetAllConfigurations();
    
    /// <returns>The updated app configuration.</returns>
    AppConfigurations SaveConfiguration(LanguageConfiguration config);
    
    /// <returns>The updated app configuration.</returns>
    AppConfigurations SaveConfiguration(ThemeConfiguration config);
    
    /// <returns>The updated app configuration.</returns>
    AppConfigurations SaveConfiguration(double scale);
}