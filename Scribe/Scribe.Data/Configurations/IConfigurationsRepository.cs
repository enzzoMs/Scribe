using Scribe.Data.Model;

namespace Scribe.Data.Configurations;

public interface IConfigurationsRepository
{
    AppConfigurations GetAllConfigurations();
    void SaveConfiguration(AppConfigurations newAppConfiguration);
}