using Scribe.Data.Model;

namespace Scribe.UI.Resources;

public interface IResourceManager
{
    void UpdateLanguageResource(LanguageConfiguration langConfig);
    void UpdateThemeResource(ThemeConfiguration themeConfig);
    void UpdateDimensionsResource(double scale);
}