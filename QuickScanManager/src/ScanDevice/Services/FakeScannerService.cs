using System.Buffers.Binary;
using ScanDevice.Models;

namespace ScanDevice.Services;

public sealed class FakeScannerService : IScannerService
{
    private const int SimulatedBatchPageCount = 3;

    private static readonly IReadOnlyList<ScannerDevice> Devices =
    [
        new("SIMULATED-WIA-001", "Virtual Document Scanner"),
        new("SIMULATED-WIA-002", "Virtual Document Scanner")
    ];

    public Task<IReadOnlyList<ScannerDevice>> GetAvailableScannersAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Devices);
    }

    public async Task<string> ScanToImageAsync(
        string deviceId,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Devices.Any(device =>
                string.Equals(device.Id, deviceId, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Simulated scanner '{deviceId}' is not available.");
        }

        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(
            outputDirectory,
            $"simulated-scan-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.bmp");

        await using var stream = new FileStream(
            outputPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await WriteSampleBitmapAsync(stream, cancellationToken);
        return outputPath;
    }

    public Task<bool> SupportsAutomaticDocumentFeederAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Devices.Any(device =>
                string.Equals(device.Id, deviceId, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Simulated scanner '{deviceId}' is not available.");
        }

        return Task.FromResult(true);
    }

    public async Task<int> ScanBatchToImagesAsync(
        string deviceId,
        string outputDirectory,
        ManualResetEventSlim continueSignal,
        Action<string> onImageScanned,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(continueSignal);
        ArgumentNullException.ThrowIfNull(onImageScanned);
        if (!await SupportsAutomaticDocumentFeederAsync(deviceId, cancellationToken))
        {
            throw new InvalidOperationException(
                "The simulated scanner does not support an automatic document feeder.");
        }

        var scannedCount = 0;
        for (var index = 0; index < SimulatedBatchPageCount; index++)
        {
            await Task.Run(() => continueSignal.Wait(cancellationToken), cancellationToken);
            await Task.Delay(TimeSpan.FromMilliseconds(350), cancellationToken);
            var outputPath = await ScanToImageAsync(deviceId, outputDirectory, cancellationToken);
            try
            {
                onImageScanned(outputPath);
                scannedCount++;
            }
            catch
            {
                File.Delete(outputPath);
                throw;
            }
        }

        return scannedCount;
    }

    private static async Task WriteSampleBitmapAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        const int width = 480;
        const int height = 640;
        const int bytesPerPixel = 3;
        var rowSize = (width * bytesPerPixel + 3) & ~3;
        var imageSize = rowSize * height;
        var header = new byte[54];
        header[0] = (byte)'B';
        header[1] = (byte)'M';
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(2), 54 + imageSize);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(10), 54);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(14), 40);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(18), width);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(22), height);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(26), 1);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(28), 24);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(34), imageSize);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(38), 2835);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(42), 2835);
        await stream.WriteAsync(header, cancellationToken);

        var row = new byte[rowSize];
        for (var y = height - 1; y >= 0; y--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var x = 0; x < width; x++)
            {
                var (red, green, blue) = GetPixelColor(x, y, width, height);
                var offset = x * bytesPerPixel;
                row[offset] = (byte)blue;
                row[offset + 1] = (byte)green;
                row[offset + 2] = (byte)red;
            }

            await stream.WriteAsync(row, cancellationToken);
        }
    }

    private static (int Red, int Green, int Blue) GetPixelColor(
        int x,
        int y,
        int width,
        int height)
    {
        var marginX = width / 8;
        var marginY = height / 10;
        var insidePage = x >= marginX
                         && x < width - marginX
                         && y >= marginY
                         && y < height - marginY;

        if (!insidePage)
        {
            return (226, 231, 239);
        }

        var pageX = x - marginX;
        var pageY = y - marginY;
        var pageWidth = width - marginX * 2;
        var pageHeight = height - marginY * 2;
        var isPageEdge = pageX < 2
                         || pageX >= pageWidth - 2
                         || pageY < 2
                         || pageY >= pageHeight - 2;
        if (isPageEdge)
        {
            return (219, 225, 234);
        }

        var lineStart = pageWidth / 8;
        var lineEnd = pageWidth - lineStart;
        var lineGap = Math.Max(12, pageHeight / 30);
        var firstLineY = pageHeight / 4;
        var isTextLine = pageY >= firstLineY
                         && pageY < pageHeight - pageHeight / 8
                         && (pageY - firstLineY) % lineGap < 2
                         && pageX >= lineStart
                         && pageX < lineEnd;

        if (pageY >= pageHeight / 8
            && pageY < pageHeight / 8 + 5
            && pageX >= lineStart
            && pageX < pageWidth / 2)
        {
            return (53, 106, 230);
        }

        return isTextLine ? (88, 98, 116) : (255, 255, 255);
    }
}
