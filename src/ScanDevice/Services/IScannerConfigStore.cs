using ScanDevice.Models;

namespace ScanDevice.Services;

public interface IScannerConfigStore
{
    Task<ScannerConfig> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(ScannerConfig config, CancellationToken cancellationToken = default);
}
