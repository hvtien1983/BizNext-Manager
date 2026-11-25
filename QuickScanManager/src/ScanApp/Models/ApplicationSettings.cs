namespace ScanApp.Models;

public sealed class ApplicationSettings
{
    public string DefaultPdfDirectory { get; set; } = string.Empty;

    public string? DefaultScannerDeviceId { get; set; }

    public bool AppendTimestampToPdfName { get; set; } = true;
}
