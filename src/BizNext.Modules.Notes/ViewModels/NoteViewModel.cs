using System;
using System.Linq;
using BizNext.Modules.Notes.Models;

namespace BizNext.Modules.Notes.ViewModels;

public sealed class NoteViewModel : ObservableObject
{
    private string _title;
    private string _content;
    private bool _isPinned;

    public NoteViewModel(Note model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _title = model.Title;
        _content = model.Content;
        _isPinned = model.IsPinned;
    }

    public Note Model { get; }

    public event EventHandler? Changed;

    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                Model.Title = value;
                OnPropertyChanged(nameof(DisplayTitle));
                Touch();
            }
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            if (SetProperty(ref _content, value))
            {
                Model.Content = value;
                OnPropertyChanged(nameof(Preview));
                Touch();
            }
        }
    }

    public bool IsPinned
    {
        get => _isPinned;
        set
        {
            if (SetProperty(ref _isPinned, value))
            {
                Model.IsPinned = value;
                Touch();
            }
        }
    }
    /// <summary>Id của notebook chứa note; Guid.Empty nghĩa là chưa phân loại.</summary>
    public Guid NotebookKey
    {
        get => Model.NotebookId ?? Guid.Empty;
        set
        {
            var newId = value == Guid.Empty ? (Guid?)null : value;
            if (Model.NotebookId != newId)
            {
                Model.NotebookId = newId;
                OnPropertyChanged();
                Touch();
            }
        }
    }

    /// <summary>Báo giao diện cập nhật khi notebook bị đổi từ bên ngoài (ví dụ khi xóa notebook).</summary>
    public void NotifyNotebookChanged()
    {
        OnPropertyChanged(nameof(NotebookKey));
        OnPropertyChanged(nameof(UpdatedAtUtc));
        OnPropertyChanged(nameof(UpdatedAtDisplay));
    }
    
    public DateTime UpdatedAtUtc => Model.UpdatedAtUtc;

    public string UpdatedAtDisplay => Model.UpdatedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    public string DisplayTitle => string.IsNullOrWhiteSpace(_title) ? "(Chưa có tiêu đề)" : _title;

    /// <summary>Dòng đầu tiên có chữ của nội dung, dùng làm xem trước trong danh sách.</summary>
    public string Preview
    {
        get
        {
            var line = _content
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? string.Empty;
            return line.Length > 80 ? line[..80] + "…" : line;
        }
    }

    private void Touch()
    {
        Model.UpdatedAtUtc = DateTime.UtcNow;
        OnPropertyChanged(nameof(UpdatedAtUtc));
        OnPropertyChanged(nameof(UpdatedAtDisplay));
        Changed?.Invoke(this, EventArgs.Empty);
    }
}