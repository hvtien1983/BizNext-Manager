namespace ScanApp.ViewModels;

public sealed record ScannerOption(
    string DeviceId,
    string DeviceName,
    string DisplayName,
    string? Alias);
