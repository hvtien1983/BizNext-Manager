using BizNext.Modules.Notes.Models;

namespace BizNext.Modules.Notes.ViewModels;

public enum NotebookItemKind
{
    All,
    Unassigned,
    Notebook
}

/// <summary>Một dòng trong danh sách notebook: "Tất cả", "Chưa phân loại" hoặc một notebook thật.</summary>
public sealed class NotebookItemViewModel : ObservableObject
{
    private string _name;
    private int _noteCount;

    public NotebookItemViewModel(NotebookItemKind kind, string name, Notebook? model = null)
    {
        Kind = kind;
        _name = name;
        Model = model;
    }

    public NotebookItemKind Kind { get; }

    public Notebook? Model { get; }

    public bool IsNotebook => Kind == NotebookItemKind.Notebook;

    /// <summary>Guid.Empty nghĩa là chưa phân loại.</summary>
    public Guid Key => Model?.Id ?? Guid.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value) && Model is not null)
            {
                Model.Name = value;
            }
        }
    }

    public int NoteCount
    {
        get => _noteCount;
        set => SetProperty(ref _noteCount, value);
    }
}