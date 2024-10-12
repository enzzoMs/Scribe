using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scribe.Data.Model;

namespace Scribe.Data.Configurations;

public class ConfigurationsRepository : IConfigurationsRepository
{
    private const string ConfigFileDirectoryPath = "./Configurations";
    private const string ConfigFilePath = "./Configurations/appsettings.json";
    private readonly AppConfigurations _defaultConfiguration = new(
        ThemeConfiguration.Light, LanguageConfiguration.EnUs, 1.0
    );
    private static readonly JsonSerializerOptions SerializerOptions = new()
    { 
        WriteIndented = true, Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public AppConfigurations GetAllConfigurations()
    {
        try
        {
            var configurations = JsonSerializer.Deserialize<AppConfigurations>(
                File.ReadAllText(ConfigFilePath), options: SerializerOptions
            )!;
            return configurations;
        }
        catch (Exception e) when (
            e is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or JsonException
        )
        {
            return _defaultConfiguration;
        }
    }
    
    public void SaveConfiguration(AppConfigurations newAppConfiguration)
    {
        try
        {
            Directory.CreateDirectory(ConfigFileDirectoryPath);

            using var fileStream = File.Open(ConfigFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fileStream.SetLength(0);
            
            var serializedConfiguration = JsonSerializer.Serialize(newAppConfiguration, SerializerOptions);
            var configurationBytes = Encoding.UTF8.GetBytes(serializedConfiguration);
            
            fileStream.Write((ReadOnlySpan<byte>) configurationBytes);
        }
        catch (Exception e) when (e is UnauthorizedAccessException) {}
    }
}