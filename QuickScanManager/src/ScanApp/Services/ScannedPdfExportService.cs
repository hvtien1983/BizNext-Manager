using System.IO;
using System.Globalization;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScanApp.Services;

public sealed class ScannedPdfExportService
{
    private static readonly byte[] PdfHeader =
        [0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A, 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A];

    public Task CreateAsync(
        IReadOnlyList<string> imagePaths,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imagePaths);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        if (imagePaths.Count == 0)
        {
            throw new ArgumentException("At least one scanned page is required.", nameof(imagePaths));
        }

        var pagePaths = imagePaths.ToArray();
        return Task.Run(
            () => Create(pagePaths, destinationPath, cancellationToken),
            cancellationToken);
    }

    private static void Create(
        IReadOnlyList<string> imagePaths,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        var fullDestinationPath = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullDestinationPath)
            ?? throw new ArgumentException("The destination must include a directory.", nameof(destinationPath));
        Directory.CreateDirectory(directory);

        var temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(fullDestinationPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            using (var output = new FileStream(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None))
            {
                output.Write(PdfHeader);
                var objectOffsets = new List<long> { 0 };
                WriteObject(output, objectOffsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");

                var pageReferences = Enumerable.Range(0, imagePaths.Count)
                    .Select(index => $"{3 + (index * 3)} 0 R");
                WriteObject(
                    output,
                    objectOffsets,
                    2,
                    $"<< /Type /Pages /Kids [{string.Join(' ', pageReferences)}] /Count {imagePaths.Count} >>");

                for (var index = 0; index < imagePaths.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var (jpegBytes, pixelWidth, pixelHeight, dpiX, dpiY) = EncodePage(imagePaths[index]);
                    var pageWidth = pixelWidth * 72d / dpiX;
                    var pageHeight = pixelHeight * 72d / dpiY;
                    var pageObject = 3 + (index * 3);
                    var contentObject = pageObject + 1;
                    var imageObject = pageObject + 2;
                    var content = Encoding.ASCII.GetBytes(
                        $"q\n{PdfNumber(pageWidth)} 0 0 {PdfNumber(pageHeight)} 0 0 cm\n/Im0 Do\nQ\n");

                    WriteObject(
                        output,
                        objectOffsets,
                        pageObject,
                        $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PdfNumber(pageWidth)} {PdfNumber(pageHeight)}] " +
                        $"/Resources << /ProcSet [/PDF /ImageC] /XObject << /Im0 {imageObject} 0 R >> >> " +
                        $"/Contents {contentObject} 0 R >>");
                    WriteStreamObject(output, objectOffsets, contentObject, string.Empty, content);
                    WriteStreamObject(
                        output,
                        objectOffsets,
                        imageObject,
                        $"/Type /XObject /Subtype /Image /Width {pixelWidth} /Height {pixelHeight} " +
                        "/ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode ",
                        jpegBytes);
                }

                WriteCrossReference(output, objectOffsets);
                output.Flush();
            }

            File.Move(temporaryPath, fullDestinationPath);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static (byte[] Bytes, int Width, int Height, double DpiX, double DpiY) EncodePage(string imagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imagePath);
        using var input = File.OpenRead(imagePath);
        var decoder = BitmapDecoder.Create(
            input,
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);
        if (decoder.Frames.Count == 0)
        {
            throw new InvalidDataException($"The scanned page has no image frames: {imagePath}");
        }

        var source = decoder.Frames[0];
        var rgbImage = new FormatConvertedBitmap(source, PixelFormats.Bgr24, null, 0);
        rgbImage.Freeze();

        var encoder = new JpegBitmapEncoder { QualityLevel = 90 };
        encoder.Frames.Add(BitmapFrame.Create(rgbImage));
        using var jpeg = new MemoryStream();
        encoder.Save(jpeg);

        return (
            jpeg.ToArray(),
            rgbImage.PixelWidth,
            rgbImage.PixelHeight,
            NormalizeDpi(rgbImage.DpiX),
            NormalizeDpi(rgbImage.DpiY));
    }

    private static double NormalizeDpi(double dpi) => double.IsFinite(dpi) && dpi > 0 ? dpi : 96;

    private static string PdfNumber(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);

    private static void WriteObject(
        Stream output,
        ICollection<long> objectOffsets,
        int objectNumber,
        string content)
    {
        objectOffsets.Add(output.Position);
        WriteAscii(output, $"{objectNumber} 0 obj\n{content}\nendobj\n");
    }

    private static void WriteStreamObject(
        Stream output,
        ICollection<long> objectOffsets,
        int objectNumber,
        string dictionary,
        byte[] content)
    {
        objectOffsets.Add(output.Position);
        WriteAscii(
            output,
            $"{objectNumber} 0 obj\n<< {dictionary}/Length {content.Length} >>\nstream\n");
        output.Write(content);
        WriteAscii(output, "\nendstream\nendobj\n");
    }

    private static void WriteCrossReference(Stream output, IReadOnlyList<long> objectOffsets)
    {
        var crossReferenceOffset = output.Position;
        WriteAscii(output, $"xref\n0 {objectOffsets.Count}\n0000000000 65535 f \n");
        for (var index = 1; index < objectOffsets.Count; index++)
        {
            WriteAscii(
                output,
                $"{objectOffsets[index].ToString("D10", CultureInfo.InvariantCulture)} 00000 n \n");
        }

        WriteAscii(
            output,
            $"trailer\n<< /Size {objectOffsets.Count} /Root 1 0 R >>\nstartxref\n" +
            $"{crossReferenceOffset.ToString(CultureInfo.InvariantCulture)}\n%%EOF\n");
    }

    private static void WriteAscii(Stream output, string text) =>
        output.Write(Encoding.ASCII.GetBytes(text));
}
