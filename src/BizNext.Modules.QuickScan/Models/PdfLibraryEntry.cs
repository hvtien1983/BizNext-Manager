namespace BizNext.Modules.QuickScan.Models;

public sealed class PdfLibraryEntry
{
    public required string Name { get; init; }

    public required bool IsFolder { get; init; }

    public bool IsParentNavigation { get; init; }

    public string? FullPath { get; init; }

    public int? PageCount { get; init; }

    public long? SizeBytes { get; init; }

    public DateTimeOffset? ScannedAt { get; init; }

    public string SizeDisplay => SizeBytes is { } size
        ? FormatFileSize(size)
        : string.Empty;

    public string ScannedAtDisplay =>
        ScannedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? string.Empty;

    public string? ErrorMessage { get; init; }

    private static string FormatFileSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }
}
