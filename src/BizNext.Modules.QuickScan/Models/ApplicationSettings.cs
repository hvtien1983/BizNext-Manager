using System.Text.Json.Serialization;
using BizNext.Contracts.Models;

namespace BizNext.Modules.QuickScan.Models;

public sealed class ApplicationSettings
{
    public string DefaultPdfDirectory { get; set; } = string.Empty;

    public string? DefaultScannerDeviceId { get; set; }

    public bool AppendTimestampToPdfName { get; set; } = true;

    // Legacy value kept only so the Shell can migrate an existing theme choice once.
    // The Shell owns theming now; this property is never written back.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ThemePreference? Theme { get; set; }
}
