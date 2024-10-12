namespace Scribe.UI.Helpers;

public interface IPdfHelper
{
    void ExportImageAsPdf(string directoryPath, string fileName, byte[] imageBytes);
}