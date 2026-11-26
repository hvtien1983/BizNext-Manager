using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BizNext.Infrastructure.Storage;
using BizNext.Modules.QuickScan.Models;

namespace BizNext.Modules.QuickScan.Services;

public sealed class JsonApplicationSettingsStore(string settingsPath)
{
    private readonly JsonFileStorage _storage = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<ApplicationSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _storage.ReadAsync(
                settingsPath,
                new ApplicationSettings(),
                JsonOptions,
                cancellationToken);
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
        await _storage.WriteAtomicallyAsync(settingsPath, settings, JsonOptions, cancellationToken);
    }
}
