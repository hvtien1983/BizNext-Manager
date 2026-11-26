using System.Text.Json;
using ScanCore.Models;

namespace ScanCore.Services;

public sealed class JsonLibraryService(string libraryPath)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task<IReadOnlyList<DocumentItem>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(libraryPath))
        {
            return [];
        }

        await using var stream = File.OpenRead(libraryPath);
        return await JsonSerializer.DeserializeAsync<List<DocumentItem>>(
                   stream,
                   JsonOptions,
                   cancellationToken)
               ?? [];
    }

    public async Task SaveAsync(
        IEnumerable<DocumentItem> documents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var fullPath = Path.GetFullPath(libraryPath);
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new ArgumentException("The library path must include a directory.", nameof(libraryPath));
        Directory.CreateDirectory(directory);

        var temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 4096,
                             useAsync: true))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    documents,
                    JsonOptions,
                    cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
