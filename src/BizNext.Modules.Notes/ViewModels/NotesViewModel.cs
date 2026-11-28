using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using BizNext.Modules.Notes.Commands;
using BizNext.Modules.Notes.Models;
using BizNext.Modules.Notes.Services;

namespace BizNext.Modules.Notes.ViewModels;

public sealed class NotesViewModel : ObservableObject
{
    private readonly INoteRepository _repository;
    private readonly DispatcherTimer _saveTimer;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private readonly NotebookItemViewModel _allItem = new(NotebookItemKind.All, "Tất cả ghi chú");
    private readonly NotebookItemViewModel _unassignedItem = new(NotebookItemKind.Unassigned, "Chưa phân loại");

    private NotesData _data = new();
    private NoteViewModel? _selectedNote;
    private NotebookItemViewModel _selectedNotebookFilter;
    private string _searchText = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isLoaded;
    private bool _isEditingNotebook;
    private bool _isRenamingNotebook;
    private bool _isBulkUpdating;
    private string _notebookEditName = string.Empty;

    public NotesViewModel(INoteRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        NotebookFilters.Add(_allItem);
        NotebookFilters.Add(_unassignedItem);
        NotebookOptions.Add(_unassignedItem);
        _selectedNotebookFilter = _allItem;

        VisibleNotes = CollectionViewSource.GetDefaultView(Notes);
        VisibleNotes.Filter = FilterNote;
        VisibleNotes.SortDescriptions.Add(new SortDescription(nameof(NoteViewModel.IsPinned), ListSortDirection.Descending));
        VisibleNotes.SortDescriptions.Add(new SortDescription(nameof(NoteViewModel.UpdatedAtUtc), ListSortDirection.Descending));

        NewNoteCommand = new RelayCommand(AddNote);
        DeleteNoteCommand = new RelayCommand(DeleteSelectedNote, () => SelectedNote is not null);
        TogglePinCommand = new RelayCommand(TogglePin, () => SelectedNote is not null);

        NewNotebookCommand = new RelayCommand(BeginCreateNotebook);
        RenameNotebookCommand = new RelayCommand(BeginRenameNotebook, () => IsNotebookSelected);
        DeleteNotebookCommand = new RelayCommand(DeleteSelectedNotebook, () => IsNotebookSelected);
        ConfirmNotebookEditCommand = new RelayCommand(ConfirmNotebookEdit);
        CancelNotebookEditCommand = new RelayCommand(CancelNotebookEdit);

        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
        _saveTimer.Tick += async (_, _) => await FlushAsync();
    }

    public ObservableCollection<NoteViewModel> Notes { get; } = new();

    /// <summary>Danh sách ghi chú đã lọc (notebook + tìm kiếm) và sắp xếp (ghim lên trước, mới sửa lên trước).</summary>
    public ICollectionView VisibleNotes { get; }

    /// <summary>Danh sách bên trái: Tất cả, Chưa phân loại, rồi các notebook.</summary>
    public ObservableCollection<NotebookItemViewModel> NotebookFilters { get; } = new();

    /// <summary>Danh sách chọn notebook cho một ghi chú: Chưa phân loại rồi các notebook.</summary>
    public ObservableCollection<NotebookItemViewModel> NotebookOptions { get; } = new();

    public RelayCommand NewNoteCommand { get; }
    public RelayCommand DeleteNoteCommand { get; }
    public RelayCommand TogglePinCommand { get; }
    public RelayCommand NewNotebookCommand { get; }
    public RelayCommand RenameNotebookCommand { get; }
    public RelayCommand DeleteNotebookCommand { get; }
    public RelayCommand ConfirmNotebookEditCommand { get; }
    public RelayCommand CancelNotebookEditCommand { get; }

    public NoteViewModel? SelectedNote
    {
        get => _selectedNote;
        set
        {
            if (SetProperty(ref _selectedNote, value))
            {
                OnPropertyChanged(nameof(HasSelection));
                DeleteNoteCommand.RaiseCanExecuteChanged();
                TogglePinCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasSelection => SelectedNote is not null;

    public NotebookItemViewModel SelectedNotebookFilter
    {
        get => _selectedNotebookFilter;
        set
        {
            // ListBox có thể đẩy null khi một dòng bị gỡ; bỏ qua để giữ trạng thái hợp lệ.
            if (value is null)
            {
                return;
            }

            if (SetProperty(ref _selectedNotebookFilter, value))
            {
                OnPropertyChanged(nameof(IsNotebookSelected));
                RenameNotebookCommand.RaiseCanExecuteChanged();
                DeleteNotebookCommand.RaiseCanExecuteChanged();
                CancelNotebookEdit();
                RefreshView();
            }
        }
    }

    public bool IsNotebookSelected => SelectedNotebookFilter.IsNotebook;

    public bool IsEditingNotebook
    {
        get => _isEditingNotebook;
        private set => SetProperty(ref _isEditingNotebook, value);
    }

    public string NotebookEditName
    {
        get => _notebookEditName;
        set => SetProperty(ref _notebookEditName, value);
    }

    public string NotebookEditLabel => _isRenamingNotebook ? "Đổi tên notebook" : "Notebook mới";

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshView();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public async Task LoadAsync()
    {
        try
        {
            _data = await _repository.LoadAsync();
            _isLoaded = true;

            foreach (var old in NotebookFilters.Where(i => i.IsNotebook).ToList())
            {
                NotebookFilters.Remove(old);
                NotebookOptions.Remove(old);
            }

            var activeNotebooks = _data.Notebooks
                .Where(n => !n.IsDeleted)
                .OrderBy(n => n.SortOrder)
                .ThenBy(n => n.Name)
                .ToList();

            foreach (var notebook in activeNotebooks)
            {
                AddNotebookItem(notebook);
            }

            // Ghi chú trỏ tới notebook không còn tồn tại thì đưa về chưa phân loại.
            var activeIds = activeNotebooks.Select(n => n.Id).ToHashSet();
            foreach (var note in _data.Notes.Where(n => n.NotebookId is Guid id && !activeIds.Contains(id)))
            {
                note.NotebookId = null;
            }

            foreach (var old in Notes)
            {
                old.Changed -= OnNoteChanged;
                old.PropertyChanged -= OnNotePropertyChanged;
            }

            Notes.Clear();
            foreach (var note in _data.Notes.Where(n => !n.IsDeleted))
            {
                Notes.Add(CreateItem(note));
            }

            RefreshCounts();
            RefreshView();
            StatusMessage = $"{Notes.Count} ghi chú";
        }
        catch (Exception ex)
        {
            // Không cho tự lưu để tránh ghi đè file dữ liệu cũ bằng dữ liệu rỗng.
            _isLoaded = false;
            StatusMessage = $"Không đọc được dữ liệu ghi chú: {ex.Message}";
        }
    }

    /// <summary>Lưu ngay (gọi khi đóng view hoặc khi hết thời gian chờ tự lưu).</summary>
    public async Task FlushAsync()
    {
        _saveTimer.Stop();
        VisibleNotes.Refresh();

        if (!_isLoaded)
        {
            return;
        }

        await _saveLock.WaitAsync();
        try
        {
            await _repository.SaveAsync(_data);
            StatusMessage = $"Đã lưu lúc {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi lưu ghi chú: {ex.Message}";
        }
        finally
        {
            _saveLock.Release();
        }
    }

    private NoteViewModel CreateItem(Note note)
    {
        var item = new NoteViewModel(note);
        item.Changed += OnNoteChanged;
        item.PropertyChanged += OnNotePropertyChanged;
        return item;
    }

    private NotebookItemViewModel AddNotebookItem(Notebook notebook)
    {
        var item = new NotebookItemViewModel(NotebookItemKind.Notebook, notebook.Name, notebook);
        NotebookFilters.Add(item);
        NotebookOptions.Add(item);
        return item;
    }

    private void OnNoteChanged(object? sender, EventArgs e)
    {
        StatusMessage = "Đang chỉnh sửa…";
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void OnNotePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isBulkUpdating || e.PropertyName != nameof(NoteViewModel.NotebookKey))
        {
            return;
        }

        RefreshCounts();

        // Hoãn một nhịp để không đổi lựa chọn ngay trong lúc ComboBox đang cập nhật.
        _saveTimer.Dispatcher.BeginInvoke(new Action(RefreshView));
    }

    private void RefreshView()
    {
        VisibleNotes.Refresh();

        if (SelectedNote is null || !FilterNote(SelectedNote))
        {
            SelectedNote = VisibleNotes.Cast<NoteViewModel>().FirstOrDefault();
        }
    }

    private void RefreshCounts()
    {
        _allItem.NoteCount = Notes.Count;
        _unassignedItem.NoteCount = Notes.Count(n => n.NotebookKey == Guid.Empty);

        foreach (var notebook in NotebookFilters.Where(i => i.IsNotebook))
        {
            notebook.NoteCount = Notes.Count(n => n.NotebookKey == notebook.Key);
        }
    }

    private bool MatchesNotebook(NoteViewModel note) => SelectedNotebookFilter.Kind switch
    {
        NotebookItemKind.Unassigned => note.NotebookKey == Guid.Empty,
        NotebookItemKind.Notebook => note.NotebookKey == SelectedNotebookFilter.Key,
        _ => true
    };

    private bool FilterNote(object item)
    {
        if (item is not NoteViewModel note || !MatchesNotebook(note))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return note.Title.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase)
            || note.Content.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
    }

    private void AddNote()
    {
        var note = new Note();
        if (SelectedNotebookFilter.IsNotebook)
        {
            note.NotebookId = SelectedNotebookFilter.Key;
        }

        _data.Notes.Add(note);

        var item = CreateItem(note);
        Notes.Add(item);

        SearchText = string.Empty;
        RefreshCounts();
        RefreshView();
        SelectedNote = item;
        _ = FlushAsync();
    }

    private void DeleteSelectedNote()
    {
        if (SelectedNote is null)
        {
            return;
        }

        var item = SelectedNote;
        item.Model.DeletedAtUtc = DateTime.UtcNow;   // xóa mềm: dữ liệu vẫn nằm trong file
        item.Changed -= OnNoteChanged;
        item.PropertyChanged -= OnNotePropertyChanged;
        Notes.Remove(item);

        RefreshCounts();
        SelectedNote = VisibleNotes.Cast<NoteViewModel>().FirstOrDefault();
        _ = FlushAsync();
    }

    private void TogglePin()
    {
        if (SelectedNote is not null)
        {
            SelectedNote.IsPinned = !SelectedNote.IsPinned;
        }
    }

    // ----- Notebook -----

    private void BeginCreateNotebook()
    {
        _isRenamingNotebook = false;
        NotebookEditName = string.Empty;
        OnPropertyChanged(nameof(NotebookEditLabel));
        IsEditingNotebook = true;
    }

    private void BeginRenameNotebook()
    {
        if (!SelectedNotebookFilter.IsNotebook)
        {
            return;
        }

        _isRenamingNotebook = true;
        NotebookEditName = SelectedNotebookFilter.Name;
        OnPropertyChanged(nameof(NotebookEditLabel));
        IsEditingNotebook = true;
    }

    private void CancelNotebookEdit()
    {
        IsEditingNotebook = false;
        NotebookEditName = string.Empty;
    }

    private void ConfirmNotebookEdit()
    {
        var name = NotebookEditName.Trim();
        if (name.Length == 0)
        {
            StatusMessage = "Tên notebook không được để trống.";
            return;
        }

        var current = _isRenamingNotebook ? SelectedNotebookFilter : null;
        var duplicate = NotebookFilters.Any(i =>
            i.IsNotebook
            && !ReferenceEquals(i, current)
            && string.Equals(i.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (duplicate)
        {
            StatusMessage = $"Đã có notebook tên \"{name}\".";
            return;
        }

        if (current is not null)
        {
            current.Name = name;
            IsEditingNotebook = false;
        }
        else
        {
            var notebook = new Notebook
            {
                Name = name,
                SortOrder = _data.Notebooks.Count == 0 ? 1 : _data.Notebooks.Max(n => n.SortOrder) + 1
            };

            _data.Notebooks.Add(notebook);
            var item = AddNotebookItem(notebook);
            RefreshCounts();
            SelectedNotebookFilter = item;   // tự đóng ô nhập và lọc theo notebook mới
        }

        _ = FlushAsync();
    }

    /// <summary>Xóa mềm notebook đang chọn; ghi chú bên trong chuyển về chưa phân loại (phương án A).</summary>
    private void DeleteSelectedNotebook()
    {
        var item = SelectedNotebookFilter;
        if (!item.IsNotebook || item.Model is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var notebookId = item.Model.Id;

        // Gồm cả ghi chú đang ở thùng rác, để sau này khôi phục không trỏ vào notebook đã xóa.
        var affected = _data.Notes.Where(n => n.NotebookId == notebookId).ToList();

        _isBulkUpdating = true;
        try
        {
            foreach (var note in affected)
            {
                note.NotebookId = null;
                note.UpdatedAtUtc = now;
            }

            foreach (var vm in Notes.Where(n => affected.Contains(n.Model)))
            {
                vm.NotifyNotebookChanged();
            }
        }
        finally
        {
            _isBulkUpdating = false;
        }

        item.Model.DeletedAtUtc = now;
        NotebookFilters.Remove(item);
        NotebookOptions.Remove(item);

        SelectedNotebookFilter = _allItem;
        RefreshCounts();
        _ = FlushAsync();
    }
}