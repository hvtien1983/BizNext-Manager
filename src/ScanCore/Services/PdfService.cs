using System.Text;
using ScanCore.Models;

namespace ScanCore.Services;

public sealed class PdfService
{
    private static readonly byte[] PdfSignature = Encoding.ASCII.GetBytes("%PDF-");

    public async Task<DocumentItem> StoreAsync(
        string sourcePath,
        string storageDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageDirectory);

        await using var source = File.OpenRead(sourcePath);
        var signature = new byte[PdfSignature.Length];
        await source.ReadExactlyAsync(signature, cancellationToken);
        if (!signature.SequenceEqual(PdfSignature))
        {
            throw new InvalidDataException($"The file is not a valid PDF: {sourcePath}");
        }

        Directory.CreateDirectory(storageDirectory);
        var storedPath = Path.Combine(storageDirectory, $"{Guid.NewGuid():N}.pdf");
        source.Position = 0;

        await using (var destination = File.Create(storedPath))
        {
            await source.CopyToAsync(destination, cancellationToken);
        }

        return new DocumentItem
        {
            Name = Path.GetFileNameWithoutExtension(sourcePath),
            FilePath = storedPath
        };
    }
}
