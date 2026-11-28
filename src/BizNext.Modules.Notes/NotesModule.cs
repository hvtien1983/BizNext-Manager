using System.IO;
using System.Windows;
using BizNext.Contracts;
using BizNext.Contracts.Models;
using BizNext.Infrastructure.Storage;
using BizNext.Modules.Notes.Services;
using BizNext.Modules.Notes.ViewModels;

namespace BizNext.Modules.Notes;

public sealed class NotesModule : IBizNextModule
{
    private readonly string _dataFilePath;

    public NotesModule() : this(GetDefaultDataFilePath())
    {
    }

    public NotesModule(string dataFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataFilePath);
        _dataFilePath = dataFilePath;
    }

    public BizNextModuleMetadata Metadata { get; } = new(
        "biznext.module.notes",
        "Ghi chú",
        "Ghi lại ý tưởng và công việc cần nhớ.",
        "Năng suất");

    public FrameworkElement CreateView()
    {
        var repository = new JsonNoteRepository(new JsonFileStorage(), _dataFilePath);
        return new NotesView(new NotesViewModel(repository));
    }

    private static string GetDefaultDataFilePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BizNext-Manager",
        "Notes",
        "notes.json");
}