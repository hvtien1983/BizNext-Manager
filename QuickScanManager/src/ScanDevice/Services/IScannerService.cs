using ScanDevice.Models;

namespace ScanDevice.Services;

public interface IScannerService
{
    Task<IReadOnlyList<ScannerDevice>> GetAvailableScannersAsync(
        CancellationToken cancellationToken = default);

    Task<string> ScanToImageAsync(
        string deviceId,
        string outputDirectory,
        CancellationToken cancellationToken = default);
}
