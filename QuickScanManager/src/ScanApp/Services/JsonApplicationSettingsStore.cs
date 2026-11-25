using System.IO;
using System.Text.Json;
using ScanApp.Models;

namespace ScanApp.Services;

public sealed class JsonApplicationSettingsStore(string settingsPath)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<ApplicationSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(settingsPath))
        {
            return new ApplicationSettings();
        }

        try
        {
            await using var stream = File.OpenRead(settingsPath);
            return await JsonSerializer.DeserializeAsync<ApplicationSettings>(
                       stream,
                       JsonOptions,
                       cancellationToken)
                   ?? throw new InvalidDataException("Application settings are empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Application settings contain invalid JSON: {settingsPath}",
                exception);
        }
    }

    public async Task SaveAsync(
        ApplicationSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var fullPath = Path.GetFullPath(settingsPath);
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new ArgumentException("The settings path must include a directory.", nameof(settingsPath));
        Directory.CreateDirectory(directory);

        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
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
                await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
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
