using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BizNext.Modules.Notes.ViewModels;

namespace BizNext.Modules.Notes;

public partial class NotesView : UserControl
{
    private readonly NotesViewModel _viewModel;
    private bool _initialized;

    public NotesView(NotesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await _viewModel.LoadAsync();
    }

    private async void OnUnloaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.FlushAsync();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NotesViewModel.IsEditingNotebook) && _viewModel.IsEditingNotebook)
        {
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    NotebookEditBox.Focus();
                    NotebookEditBox.SelectAll();
                }),
                DispatcherPriority.Input);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            Window.GetWindow(this),
            "Xóa ghi chú này?",
            "Ghi chú",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.DeleteNoteCommand.Execute(null);
        }
    }

    private void DeleteNotebookButton_Click(object sender, RoutedEventArgs e)
    {
        var notebook = _viewModel.SelectedNotebookFilter;
        if (!notebook.IsNotebook)
        {
            return;
        }

        var message = notebook.NoteCount > 0
            ? $"Xóa notebook \"{notebook.Name}\"?\n\n{notebook.NoteCount} ghi chú trong notebook sẽ được chuyển về \"Chưa phân loại\" (không bị xóa)."
            : $"Xóa notebook \"{notebook.Name}\"?";

        var result = MessageBox.Show(
            Window.GetWindow(this),
            message,
            "Notebook",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.DeleteNotebookCommand.Execute(null);
        }
    }
}