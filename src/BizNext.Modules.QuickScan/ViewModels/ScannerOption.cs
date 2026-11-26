namespace BizNext.Modules.QuickScan.ViewModels;

public sealed record ScannerOption(
    string DeviceId,
    string DeviceName,
    string DisplayName,
    string? Alias,
    bool IsAddNew = false);
