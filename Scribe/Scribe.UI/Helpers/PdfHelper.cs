using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Scribe.UI.Helpers;

public static class PdfHelper
{
    public static void ExportImageAsPdf(string directoryPath, string fileName, byte[] imageBytes)
    {
        var document = new PdfDocument();
        var page = document.AddPage();

        using var outStream = new MemoryStream(imageBytes);
        var image = XImage.FromStream(outStream);

        const int pointsPerInch = 72;
        page.Width = (XUnit) image.PixelWidth * pointsPerInch / image.HorizontalResolution;
        page.Height = (XUnit) image.PixelHeight * pointsPerInch / image.VerticalResolution;
        
        var gfx = XGraphics.FromPdfPage(page);
        gfx.DrawImage(image, 0, 0);
        
        document.Save($"{directoryPath}/{fileName}.pdf");
    }
}