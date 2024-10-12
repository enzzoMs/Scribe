using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Scribe.UI.Helpers;

public class PdfHelper : IPdfHelper
{
    public void ExportImageAsPdf(string directoryPath, string fileName, byte[] imageBytes)
    {
        var document = new PdfDocument();
        var page = document.AddPage();

        using var outStream = new MemoryStream(imageBytes);
        var image = XImage.FromStream(outStream);

        const int pointsPerInch = 72;
        page.Width = XUnit.FromPoint(image.PixelWidth * pointsPerInch / image.HorizontalResolution);
        page.Height = XUnit.FromPoint(image.PixelHeight * pointsPerInch / image.VerticalResolution);

        var gfx = XGraphics.FromPdfPage(page);
        gfx.DrawImage(image, 0, 0);

        var filePath = $"{directoryPath}/{fileName}.pdf";
        var fileNumber = 1;

        while (File.Exists(filePath))
        {
            filePath = $"{directoryPath}/{fileName} ({fileNumber}).pdf";
            fileNumber++;
        }

        document.Save(filePath);
    }
}