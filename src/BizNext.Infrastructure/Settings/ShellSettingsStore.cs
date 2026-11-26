using System.Text.Json;
using System.Text.Json.Serialization;
using BizNext.Infrastructure.Storage;

namespace BizNext.Infrastructure.Settings;

public sealed class ShellSettingsStore(string settingsPath, JsonFileStorage storage)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<ShellSettings> LoadAsync(CancellationToken cancellationToken = default) =>
        storage.ReadAsync(settingsPath, new ShellSettings(), JsonOptions, cancellationToken);

    public Task SaveAsync(ShellSettings settings, CancellationToken cancellationToken = default) =>
        storage.WriteAtomicallyAsync(settingsPath, settings, JsonOptions, cancellationToken);
}
