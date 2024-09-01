using System.Windows;
using Scribe.Data.Model;

namespace Scribe.UI.Resources;

public class ResourceManager : IResourceManager
{
    public void UpdateLanguageResource(LanguageConfiguration langConfig)
    {
        var langDictionaryPath = $"/Resources/Strings/Strings.{langConfig}.xaml";

        Application.Current.Resources.MergedDictionaries[App.StringsDictionaryIndex] = new ResourceDictionary
        {
            Source = new Uri(langDictionaryPath, UriKind.Relative)
        };
    }

    public void UpdateThemeResource(ThemeConfiguration themeConfig)
    {
        var themeDictionaryPath = $"/Resources/Brushes/Brushes.{themeConfig}.xaml";

        Application.Current.Resources.MergedDictionaries[App.ThemeDictionaryIndex] = new ResourceDictionary
        {
            Source = new Uri(themeDictionaryPath, UriKind.Relative)
        };
    }

    public void UpdateDimensionsResource(double scale)
    {
        var currentDimensions = 
            Application.Current.Resources.MergedDictionaries[App.DimensDictionaryIndex];
        
        currentDimensions["Dimen.App.ScaleX"] = scale;
        currentDimensions["Dimen.App.ScaleY"] = scale;
    }
}