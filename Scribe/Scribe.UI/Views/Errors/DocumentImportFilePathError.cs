namespace Scribe.UI.Views.Errors;

public record DocumentImportFilePathError(string FilePath) : IViewModelError;