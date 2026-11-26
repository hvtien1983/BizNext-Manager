namespace ScanDevice.Models;

public sealed class ScannerConfig
{
    public int Version { get; set; } = 1;

    public string? LastUsedDeviceId { get; set; }

    public List<ScannerProfile> Profiles { get; set; } = [];
}
