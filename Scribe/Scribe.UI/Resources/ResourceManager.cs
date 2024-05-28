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
        var baseDimensions = new ResourceDictionary
        {
            Source = new Uri("Resources/Dimens/Dimens.xaml", UriKind.Relative)
        };

        var currentDimensions = 
            Application.Current.Resources.MergedDictionaries[App.DimensDictionaryIndex];
        
        foreach (var dimensionKey in currentDimensions.Keys)
        {
            var dimension = baseDimensions[dimensionKey];

            currentDimensions[dimensionKey] = dimension switch
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
    }
}