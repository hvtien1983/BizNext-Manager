using System.IO;
using System.Security;
using BizNext.Modules.QuickScan.Models;
using PDFtoImage;
using PDFtoImage.Exceptions;
using ScanCore.Models;
using ScanCore.Services;

namespace BizNext.Modules.QuickScan.Services;

public sealed class PdfLibraryBrowserService(
    JsonLibraryService libraryService,
    string defaultFolderPath)
{
    private readonly string _defaultFolderPath = Path.GetFullPath(defaultFolderPath);

    public async Task<IReadOnlyList<PdfLibraryFolder>> GetFoldersAsync(
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_defaultFolderPath);

        return await Task.Run<IReadOnlyList<PdfLibraryFolder>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var folders = new List<PdfLibraryFolder>
            {
                new("Thư mục mặc định", _defaultFolderPath, IsDefault: true)
            };

            folders.AddRange(
                Directory.EnumerateDirectories(_defaultFolderPath, "*", SearchOption.TopDirectoryOnly)
                    .Select(path => new PdfLibraryFolder(
                        Path.GetFileName(path),
                        Path.GetFullPath(path),
                        IsDefault: false))
                    .OrderBy(folder => folder.Name, StringComparer.CurrentCultureIgnoreCase));

            return folders;
        }, cancellationToken);
    }

    public async Task<PdfLibraryLoadResult> LoadFolderAsync(
        PdfLibraryFolder folder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(folder);
        IReadOnlyList<DocumentItem> documents = [];
        string? metadataWarning = null;
        try
        {
            documents = await libraryService.LoadAsync(cancellationToken);
        }
        catch (Exception exception) when (
            exception is IOException or
                UnauthorizedAccessException or
                SecurityException or
                InvalidDataException or
                System.Text.Json.JsonException)
        {
            metadataWarning = $"Không đọc được metadata thư viện: {exception.Message}";
        }

        var documentLookup = BuildDocumentLookup(documents);

        var entries = await Task.Run<IReadOnlyList<PdfLibraryEntry>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entries = new List<PdfLibraryEntry>();

            if (folder.IsDefault)
            {
                entries.AddRange(
                    Directory.EnumerateDirectories(folder.FullPath, "*", SearchOption.TopDirectoryOnly)
                        .Select(path => new PdfLibraryEntry
                        {
                            Name = Path.GetFileName(path),
                            IsFolder = true,
                            FullPath = Path.GetFullPath(path)
                        }));
            }
            else
            {
                entries.Add(new PdfLibraryEntry
                {
                    Name = "Quay lại thư mục cha",
                    IsFolder = true,
                    IsParentNavigation = true,
                    FullPath = Directory.GetParent(folder.FullPath)?.FullName ?? _defaultFolderPath
                });
            }

            foreach (var filePath in Directory.EnumerateFiles(
                         folder.FullPath,
                         "*.pdf",
                         SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fullPath = Path.GetFullPath(filePath);
                var fileInfo = new FileInfo(fullPath);
                documentLookup.TryGetValue(fullPath, out var document);
                var scannedAt = document?.CreatedAtUtc
                    ?? new DateTimeOffset(fileInfo.CreationTimeUtc);
                int? pageCount = document is { PageCount: > 0 } ? document.PageCount : null;
                string? errorMessage = null;

                if (pageCount is null)
                {
                    try
                    {
                        using var stream = File.OpenRead(fullPath);
                        pageCount = Conversion.GetPageCount(stream, leaveOpen: true, password: null);
                    }
                    catch (Exception exception) when (
                        exception is IOException or
                            UnauthorizedAccessException or
                            SecurityException or
                            ArgumentException or
                            InvalidDataException or
                            PdfException)
                    {
                        errorMessage = $"Không đọc được số trang: {exception.Message}";
                    }
                }

                entries.Add(new PdfLibraryEntry
                {
                    Name = Path.GetFileName(fullPath),
                    IsFolder = false,
                    FullPath = fullPath,
                    PageCount = pageCount,
                    SizeBytes = fileInfo.Length,
                    ScannedAt = scannedAt,
                    ErrorMessage = errorMessage
                });
            }

            return entries;
        }, cancellationToken);
        return new PdfLibraryLoadResult(entries, metadataWarning);
    }

    private static Dictionary<string, DocumentItem> BuildDocumentLookup(
        IEnumerable<DocumentItem> documents)
    {
        var result = new Dictionary<string, DocumentItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var document in documents)
        {
            try
            {
                result[Path.GetFullPath(document.FilePath)] = document;
            }
            catch (Exception exception) when (
                exception is ArgumentException or NotSupportedException or PathTooLongException)
            {
                // Invalid historical paths cannot match a file found on disk.
            }
        }

        return result;
    }
}
