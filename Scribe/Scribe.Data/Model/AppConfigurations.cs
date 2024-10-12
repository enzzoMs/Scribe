namespace Scribe.Data.Model;

public class AppConfigurations
{
    public const double MinScale = 0.7;  
    public const double MaxScale = 1.5;

    private double _scale;

    public AppConfigurations(ThemeConfiguration theme, LanguageConfiguration language, double scale)
    {
        Theme = theme;
        Language = language;
        Scale = scale;
    }
    
    public ThemeConfiguration Theme { get; set; }

    public LanguageConfiguration Language { get; set; }

    public double Scale
    {
        get => _scale;
        set
        {
            _scale = value switch
            {
                > MaxScale => MaxScale,
                < MinScale => MinScale,
                _ => value
            };
        }
    }
}