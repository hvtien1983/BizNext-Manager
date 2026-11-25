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

        var directory = Path.GetDirectoryName(Path.GetFullPath(libraryPath));
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(libraryPath);
        await JsonSerializer.SerializeAsync(
            stream,
            documents,
            JsonOptions,
            cancellationToken);
    }
}
