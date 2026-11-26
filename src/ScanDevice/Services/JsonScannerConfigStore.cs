using System.Text.Json;
using ScanDevice.Models;

namespace ScanDevice.Services;

public sealed class JsonScannerConfigStore(string? configPath = null) : IScannerConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _configPath = configPath
        ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuickScanManager",
            "scanner-config.json");

    public async Task<ScannerConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_configPath))
        {
            return new ScannerConfig();
        }

        try
        {
            await using var stream = File.OpenRead(_configPath);
            var config = await JsonSerializer.DeserializeAsync<ScannerConfig>(
                stream,
                JsonOptions,
                cancellationToken);
            if (config is null)
            {
                throw new InvalidDataException("Scanner configuration is empty.");
            }

            if (config.Version != 1)
            {
                throw new InvalidDataException(
                    $"Scanner configuration version {config.Version} is not supported.");
            }

            if (config.Profiles is null
                || config.Profiles.Any(profile =>
                    profile is null
                    || string.IsNullOrWhiteSpace(profile.DeviceId)
                    || string.IsNullOrWhiteSpace(profile.Alias)))
            {
                throw new InvalidDataException(
                    "Scanner configuration contains a profile without a device ID or alias.");
            }

            return config;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Scanner configuration contains invalid JSON: {_configPath}",
                exception);
        }
    }

    public async Task SaveAsync(
        ScannerConfig config,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        var fullPath = Path.GetFullPath(_configPath);
        var directory = Path.GetDirectoryName(fullPath)
                        ?? throw new InvalidOperationException("The scanner config directory is invalid.");
        Directory.CreateDirectory(directory);

        var temporaryPath = $"{fullPath}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var stream = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             4096,
                             FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    config,
                    JsonOptions,
                    cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            if (File.Exists(fullPath))
            {
                File.Replace(temporaryPath, fullPath, null);
            }
            else
            {
                File.Move(temporaryPath, fullPath);
            }
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
