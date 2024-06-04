namespace Scribe.Data.Model;

public record AppConfigurations(
    ThemeConfiguration Theme, 
    LanguageConfiguration Language,
    double Scale
);