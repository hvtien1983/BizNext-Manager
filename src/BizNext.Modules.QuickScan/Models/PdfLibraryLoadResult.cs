namespace BizNext.Modules.QuickScan.Models;

public sealed record PdfLibraryLoadResult(
    IReadOnlyList<PdfLibraryEntry> Entries,
    string? MetadataWarning);
