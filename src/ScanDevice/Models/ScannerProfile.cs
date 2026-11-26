namespace ScanDevice.Models;

public sealed class ScannerProfile
{
    public required string DeviceId { get; init; }

    public required string Alias { get; set; }

    public string ColorMode { get; set; } = "Color";

    public int ResolutionDpi { get; set; } = 300;

    public string Source { get; set; } = "Flatbed";
}
