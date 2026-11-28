using BizNext.Infrastructure.Storage;
using BizNext.Modules.Notes.Models;

namespace BizNext.Modules.Notes.Services;

public sealed class JsonNoteRepository : INoteRepository
{
    private readonly JsonFileStorage _storage;
    private readonly string _filePath;

    public JsonNoteRepository(JsonFileStorage storage, string filePath)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    public Task<NotesData> LoadAsync(CancellationToken cancellationToken = default)
        => _storage.ReadAsync(_filePath, new NotesData(), null, cancellationToken);

    public Task SaveAsync(NotesData data, CancellationToken cancellationToken = default)
        => _storage.WriteAtomicallyAsync(_filePath, data, null, cancellationToken);
}