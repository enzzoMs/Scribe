using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scribe.Data.Model;

namespace Scribe.Data.Configurations;

public class ConfigurationsRepository : IConfigurationsRepository
{
    private const string ConfigFilePath = "./Configurations/appsettings.json";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    { 
        WriteIndented = true, Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    public AppConfigurations GetAllConfigurations() => JsonSerializer.Deserialize<AppConfigurations>(
        File.ReadAllText(ConfigFilePath), options: SerializerOptions
    );
    
    /// <returns>The updated app configuration.</returns>
    public AppConfigurations SaveConfiguration(LanguageConfiguration config)
    {
        var currentAppConfigurations = GetAllConfigurations();
        
        var newAppConfiguration = currentAppConfigurations with { Language = config };

        if (currentAppConfigurations != newAppConfiguration)
        {
            File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(newAppConfiguration, SerializerOptions));
        }

        return newAppConfiguration;
    }
    
    /// <returns>The updated app configuration.</returns>
    public AppConfigurations SaveConfiguration(ThemeConfiguration config)
    {
        var currentAppConfigurations = GetAllConfigurations();
        
        var newAppConfiguration = currentAppConfigurations with { Theme = config };

        if (currentAppConfigurations != newAppConfiguration)
        {
            File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(newAppConfiguration, SerializerOptions));
        }

        return newAppConfiguration;
    }
    
    public AppConfigurations SaveConfiguration(double scale)
    {
        var currentAppConfigurations = GetAllConfigurations();
        
        var newAppConfiguration = currentAppConfigurations with { Scale = scale };

        if (currentAppConfigurations != newAppConfiguration)
        {
            File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(newAppConfiguration, SerializerOptions));
        }

        return newAppConfiguration;
    }
}