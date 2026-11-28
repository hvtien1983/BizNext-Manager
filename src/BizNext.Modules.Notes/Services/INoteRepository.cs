using System.Threading;
using System.Threading.Tasks;
using BizNext.Modules.Notes.Models;

namespace BizNext.Modules.Notes.Services;

public interface INoteRepository
{
    Task<NotesData> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(NotesData data, CancellationToken cancellationToken = default);
}