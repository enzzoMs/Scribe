namespace Scribe.Data.Model;

public record struct AppConfigurations(
    ThemeConfiguration Theme, 
    LanguageConfiguration Language,
    double Scale
);