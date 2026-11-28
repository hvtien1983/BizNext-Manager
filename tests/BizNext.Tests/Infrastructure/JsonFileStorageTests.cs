using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BizNext.Infrastructure.Settings;
using BizNext.Infrastructure.Storage;
using BizNext.Contracts.Models;
using BizNext.Tests.TestSupport;

namespace BizNext.Tests.Infrastructure;

public sealed class JsonFileStorageTests
{
    [Fact]
    public async Task MissingFileReturnsDefaultAndSettingsRoundTrip()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "nested", "settings.json");
        var store = new ShellSettingsStore(path, new JsonFileStorage());

        Assert.Null((await store.LoadAsync()).Theme);
        await store.SaveAsync(new ShellSettings { Theme = ThemePreference.Dark, LastSelectedModuleId = "notes" });
        var loaded = await store.LoadAsync();

        Assert.Equal(ThemePreference.Dark, loaded.Theme);
        Assert.Equal("notes", loaded.LastSelectedModuleId);
        Assert.Contains("\"Dark\"", await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task CorruptAndNullJsonAreReported()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var storage = new JsonFileStorage();
        await File.WriteAllTextAsync(path, "{");
        await Assert.ThrowsAsync<JsonException>(() => storage.ReadAsync(path, new ShellSettings()));
        await File.WriteAllTextAsync(path, "null");
        await Assert.ThrowsAsync<InvalidDataException>(() => storage.ReadAsync(path, new ShellSettings()));
    }

    [Fact]
    public async Task AtomicWriteReplacesExistingFile()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var storage = new JsonFileStorage();
        await storage.WriteAtomicallyAsync(path, new ShellSettings { LastSelectedModuleId = "notes" });

        await storage.WriteAtomicallyAsync(path, new ShellSettings { LastSelectedModuleId = "scan" });

        var loaded = await storage.ReadAsync(path, new ShellSettings());
        Assert.Equal("scan", loaded.LastSelectedModuleId);
        Assert.Equal(new[] { path }, Directory.GetFiles(temp.Path));
    }

    [Fact]
    public async Task FailedSerializationLeavesExistingFileAndCleansTemporaryFile()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        await File.WriteAllTextAsync(path, "original");
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ThrowingConverter());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new JsonFileStorage().WriteAtomicallyAsync(path, new SampleValue(), options));

        Assert.Equal("original", await File.ReadAllTextAsync(path));
        Assert.Equal(new[] { path }, Directory.GetFiles(temp.Path));
    }

    private sealed class SampleValue;

    private sealed class ThrowingConverter : JsonConverter<SampleValue>
    {
        public override SampleValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException();

        public override void Write(Utf8JsonWriter writer, SampleValue value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            throw new InvalidOperationException("Simulated serialization failure");
        }
    }
}
