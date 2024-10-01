namespace Scribe.UI.Views.Errors;

public record DocumentImportFileFormatError(string FilePath) : IViewModelError;