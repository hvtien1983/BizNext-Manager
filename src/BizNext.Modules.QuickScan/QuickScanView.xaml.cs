using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BizNext.Modules.QuickScan.Models;
using BizNext.Modules.QuickScan.Services;
using BizNext.Modules.QuickScan.ViewModels;
using ScanCore.Models;
using ScanCore.Services;
using ScanDevice.Models;
using ScanDevice.Services;
using System.IO;
using Microsoft.Win32;

namespace BizNext.Modules.QuickScan;

public partial class QuickScanView : UserControl
{
    private const string ScanPageDataFormat = "BizNext.Modules.QuickScan.ScanPage";

    private readonly ScannerSettingsViewModel _scannerViewModel;
    private readonly ObservableCollection<PdfLibraryFolder> _libraryFolders = [];
    private readonly ObservableCollection<PdfLibraryEntry> _visibleLibraryEntries = [];
    private IReadOnlyList<PdfLibraryEntry> _libraryEntries = [];
    private string? _libraryMetadataWarning;
    private CancellationTokenSource? _libraryRefreshCancellation;
    private string _librarySortMember = nameof(PdfLibraryEntry.ScannedAt);
    private ListSortDirection _librarySortDirection = ListSortDirection.Descending;
    private bool _initialized;
    private bool _libraryReady;
    private bool _isUpdatingLibraryFolders;
    private Point _pageDragStart;
    private ScanPageViewModel? _pageBeingDragged;

    public QuickScanView(ApplicationSettings applicationSettings, string? settingsLoadError)
    {
        ArgumentNullException.ThrowIfNull(applicationSettings);
        InitializeComponent();
        LibraryFolderComboBox.ItemsSource = _libraryFolders;
        LibraryGrid.ItemsSource = _visibleLibraryEntries;
        UpdateLibrarySortIndicators();

        var useFakeScanner = string.Equals(
            Environment.GetEnvironmentVariable("QUICKSCAN_USE_FAKE_SCANNER"),
            "1",
            StringComparison.Ordinal);
        IScannerService scannerService = useFakeScanner
            ? new FakeScannerService()
            : new WiaScannerService();
        var configPath = useFakeScanner
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "QuickScanManager",
                "scanner-config.fake.json")
            : null;

        var storageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuickScanManager",
            "storage");
        var applicationSettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuickScanManager",
            useFakeScanner ? "app-settings.fake.json" : "app-settings.json");
        _scannerViewModel = new ScannerSettingsViewModel(
            scannerService,
            new JsonScannerConfigStore(configPath),
            new JsonApplicationSettingsStore(applicationSettingsPath),
            new ScannedPdfExportService(),
            new JsonLibraryService(Path.Combine(storageDirectory, "library.json")),
            Path.Combine(storageDirectory, "pdfs"),
            applicationSettings,
            settingsLoadError);
        _scannerViewModel.ProfileEditRequested += EditScannerProfile;
        _scannerViewModel.ProfileDeleteRequested += DeleteScannerProfile;
        DataContext = _scannerViewModel;
        Loaded += QuickScanView_Loaded;
    }

    private Window OwnerWindow =>
        Window.GetWindow(this)
        ?? throw new InvalidOperationException("The QuickScan module is not hosted in a window.");

    private async void OpenAppSettings_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AppSettingsWindow(
            _scannerViewModel.GetApplicationSettingsCopy(),
            _scannerViewModel.ConfiguredScanners)
        {
            Owner = OwnerWindow
        };
        if (dialog.ShowDialog() != true || dialog.Settings is null)
        {
            return;
        }

        try
        {
            await _scannerViewModel.SaveApplicationSettingsAsync(dialog.Settings);
        }
        catch (Exception exception) when (
            exception is IOException or
                UnauthorizedAccessException or
                ArgumentException or
                InvalidOperationException or
                System.Security.SecurityException)
        {
            MessageBox.Show(
                OwnerWindow,
                $"Không lưu được cài đặt ứng dụng. {exception.Message}",
                "Lỗi lưu cài đặt",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void QuickScanView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await _scannerViewModel.InitializeAsync();
        _libraryReady = true;
        await RefreshLibraryAsync(refreshFolders: true);
    }

    private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ReferenceEquals(e.Source, MainTabControl) &&
            _libraryReady &&
            ReferenceEquals(MainTabControl.SelectedItem, LibraryTab))
        {
            await RefreshLibraryAsync(refreshFolders: true);
        }
    }

    private async void LibraryFolderComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_isUpdatingLibraryFolders || !_libraryReady)
        {
            return;
        }

        await RefreshLibraryAsync(refreshFolders: false);
    }

    private async void RefreshLibrary_Click(object sender, RoutedEventArgs e) =>
        await RefreshLibraryAsync(refreshFolders: true);

    private async Task RefreshLibraryAsync(bool refreshFolders)
    {
        var cancellation = new CancellationTokenSource();
        var previousCancellation = _libraryRefreshCancellation;
        _libraryRefreshCancellation = cancellation;
        previousCancellation?.Cancel();

        try
        {
            var browser = CreateLibraryBrowserService();
            if (refreshFolders)
            {
                var previousFolderPath = (LibraryFolderComboBox.SelectedItem as PdfLibraryFolder)?.FullPath;
                var folders = await browser.GetFoldersAsync(cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();

                _isUpdatingLibraryFolders = true;
                try
                {
                    _libraryFolders.Clear();
                    foreach (var folder in folders)
                    {
                        _libraryFolders.Add(folder);
                    }

                    LibraryFolderComboBox.SelectedItem = _libraryFolders.FirstOrDefault(folder =>
                        previousFolderPath is not null &&
                        string.Equals(
                            folder.FullPath,
                            previousFolderPath,
                            StringComparison.OrdinalIgnoreCase))
                        ?? _libraryFolders.FirstOrDefault(folder => folder.IsDefault);
                }
                finally
                {
                    _isUpdatingLibraryFolders = false;
                }
            }

            if (LibraryFolderComboBox.SelectedItem is not PdfLibraryFolder selectedFolder)
            {
                _libraryEntries = [];
                _libraryMetadataWarning = null;
                ApplyLibraryFilter();
                LibraryStatusText.Text = "Không có thư mục để hiển thị.";
                return;
            }

            LibraryStatusText.Text = $"Đang quét “{selectedFolder.Name}”...";
            LibraryStatusText.ToolTip = null;
            LibraryRefreshButton.IsEnabled = false;
            var result = await browser.LoadFolderAsync(selectedFolder, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();

            _libraryEntries = result.Entries;
            _libraryMetadataWarning = result.MetadataWarning;
            ApplyLibraryFilter();
            UpdateLibraryStatus();
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception) when (
            exception is IOException or
                UnauthorizedAccessException or
                System.Security.SecurityException or
                ArgumentException or
                NotSupportedException or
                InvalidDataException or
                System.Text.Json.JsonException)
        {
            if (ReferenceEquals(_libraryRefreshCancellation, cancellation))
            {
                _libraryEntries = [];
                _libraryMetadataWarning = null;
                ApplyLibraryFilter();
                LibraryStatusText.Text = "Không tải được thư viện PDF.";
                LibraryStatusText.ToolTip = exception.Message;
                MessageBox.Show(
                    OwnerWindow,
                    $"Không thể tải danh sách file PDF. {exception.Message}",
                    "Lỗi tải thư viện PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        finally
        {
            if (ReferenceEquals(_libraryRefreshCancellation, cancellation))
            {
                _libraryRefreshCancellation = null;
                LibraryRefreshButton.IsEnabled = true;
            }

            cancellation.Dispose();
        }
    }

    private PdfLibraryBrowserService CreateLibraryBrowserService()
        => new(CreateLibraryMetadataService(), _scannerViewModel.PdfStorageDirectory);

    private static JsonLibraryService CreateLibraryMetadataService()
    {
        var libraryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuickScanManager",
            "storage",
            "library.json");
        return new JsonLibraryService(libraryPath);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) =>
        ApplyLibraryFilter();

    private void ApplyLibraryFilter()
    {
        var searchText = SearchBox?.Text.Trim();
        var visibleEntries = _libraryEntries
            .Where(entry =>
                entry.IsFolder ||
                string.IsNullOrEmpty(searchText) ||
                entry.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
            .ToList();
        visibleEntries.Sort(CompareLibraryEntries);

        _visibleLibraryEntries.Clear();
        foreach (var entry in visibleEntries)
        {
            _visibleLibraryEntries.Add(entry);
        }

        var visibleFileCount = visibleEntries.Count(entry => !entry.IsFolder);
        LibraryEmptyState.Visibility = visibleFileCount == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        LibraryEmptyTitle.Text = string.IsNullOrEmpty(searchText)
            ? "Thư mục này chưa có file PDF"
            : "Không tìm thấy file phù hợp";
        LibraryEmptyDescription.Text = string.IsNullOrEmpty(searchText)
            ? "Các file PDF trong thư mục đã chọn sẽ xuất hiện tại đây."
            : "Thử một từ khóa khác để tìm theo tên tập tin.";
        UpdateLibrarySortIndicators();
    }

    private void UpdateLibraryStatus()
    {
        var fileEntries = _libraryEntries.Where(entry => !entry.IsFolder).ToArray();
        var unreadableEntries = fileEntries
            .Where(entry => entry.ErrorMessage is not null)
            .ToArray();
        var statusDetails = new List<string>();
        if (_libraryMetadataWarning is not null)
        {
            statusDetails.Add(_libraryMetadataWarning);
        }

        if (unreadableEntries.Length > 0)
        {
            statusDetails.AddRange(
                unreadableEntries.Select(entry => $"{entry.Name}: {entry.ErrorMessage}"));
        }

        LibraryStatusText.Text = statusDetails.Count == 0
            ? $"{fileEntries.Length} tài liệu"
            : $"{fileEntries.Length} tài liệu · Có lỗi khi đọc thông tin";
        LibraryStatusText.ToolTip = statusDetails.Count == 0
            ? null
            : string.Join(Environment.NewLine, statusDetails);
    }

    private void LibraryGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Column.SortMemberPath))
        {
            return;
        }

        _librarySortDirection = e.Column.SortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;
        _librarySortMember = e.Column.SortMemberPath;
        e.Column.SortDirection = _librarySortDirection;
        e.Handled = true;
        ApplyLibraryFilter();
    }

    private int CompareLibraryEntries(PdfLibraryEntry left, PdfLibraryEntry right)
    {
        if (left.IsParentNavigation != right.IsParentNavigation)
        {
            return left.IsParentNavigation ? -1 : 1;
        }

        if ((LibraryFolderComboBox.SelectedItem as PdfLibraryFolder)?.IsDefault == true &&
            left.IsFolder != right.IsFolder)
        {
            return left.IsFolder ? -1 : 1;
        }

        var comparison = _librarySortMember switch
        {
            nameof(PdfLibraryEntry.PageCount) =>
                Nullable.Compare(left.PageCount, right.PageCount),
            nameof(PdfLibraryEntry.SizeBytes) =>
                Nullable.Compare(left.SizeBytes, right.SizeBytes),
            nameof(PdfLibraryEntry.ScannedAt) =>
                Nullable.Compare(left.ScannedAt, right.ScannedAt),
            _ => StringComparer.CurrentCultureIgnoreCase.Compare(left.Name, right.Name)
        };
        if (comparison == 0)
        {
            comparison = StringComparer.CurrentCultureIgnoreCase.Compare(left.Name, right.Name);
        }

        return _librarySortDirection == ListSortDirection.Ascending
            ? comparison
            : -comparison;
    }

    private void UpdateLibrarySortIndicators()
    {
        foreach (var column in LibraryGrid.Columns)
        {
            column.SortDirection = string.Equals(
                column.SortMemberPath,
                _librarySortMember,
                StringComparison.Ordinal)
                ? _librarySortDirection
                : null;
        }
    }

    private void LibraryGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source ||
            FindVisualAncestor<DataGridRow>(source)?.DataContext is not PdfLibraryEntry entry)
        {
            return;
        }

        if (entry.IsFolder)
        {
            LibraryFolderComboBox.SelectedItem = _libraryFolders.FirstOrDefault(folder =>
                string.Equals(
                    folder.FullPath,
                    entry.FullPath,
                    StringComparison.OrdinalIgnoreCase));
            return;
        }

        OpenPdfViewer(entry);
    }

    private void LibraryGridRow_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is not DataGridRow row ||
            row.DataContext is not PdfLibraryEntry { IsFolder: false })
        {
            e.Handled = true;
            return;
        }

        row.IsSelected = true;
        LibraryGrid.SelectedItem = row.Item;
    }

    private void LibraryGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        e.Row.ContextMenu = e.Row.Item is PdfLibraryEntry { IsFolder: false }
            ? (ContextMenu)FindResource("PdfLibraryContextMenu")
            : null;
        e.Row.ContextMenuOpening -= LibraryGridRow_ContextMenuOpening;
        e.Row.ContextMenuOpening += LibraryGridRow_ContextMenuOpening;
    }

    private void QuickViewPdf_Click(object sender, RoutedEventArgs e)
    {
        if (GetContextMenuEntry(sender) is { } entry)
        {
            OpenPdfViewer(entry);
        }
    }

    private void OpenPdfViewer(PdfLibraryEntry entry)
    {
        if (entry.FullPath is not { } filePath || !File.Exists(filePath))
        {
            MessageBox.Show(
                OwnerWindow,
                "File PDF không còn tồn tại trong thư mục đã chọn.",
                "Không tìm thấy file PDF",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        new PdfViewerWindow(filePath)
        {
            Owner = OwnerWindow
        }.Show();
    }

    private void OpenPdfLocation_Click(object sender, RoutedEventArgs e)
    {
        if (GetContextMenuEntry(sender) is not { FullPath: { } filePath } ||
            !File.Exists(filePath))
        {
            MessageBox.Show(
                OwnerWindow,
                "File PDF không còn tồn tại trong thư mục đã chọn.",
                "Không tìm thấy file PDF",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{filePath}\"")
            {
                UseShellExecute = true
            });
        }
        catch (Exception exception) when (
            exception is IOException or
                UnauthorizedAccessException or
                System.ComponentModel.Win32Exception or
                System.Security.SecurityException or
                ArgumentException or
                InvalidOperationException or
                NotSupportedException)
        {
            MessageBox.Show(
                OwnerWindow,
                $"Không thể mở vị trí file PDF. {exception.Message}",
                "Lỗi mở vị trí file",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void RenamePdf_Click(object sender, RoutedEventArgs e)
    {
        var entry = GetContextMenuEntry(sender);
        if (entry?.FullPath is not { } oldPath || !File.Exists(oldPath))
        {
            MessageBox.Show(
                OwnerWindow,
                "File PDF không còn tồn tại trong thư mục đã chọn.",
                "Không tìm thấy file PDF",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var dialog = new RenamePdfWindow(entry.Name)
        {
            Owner = OwnerWindow
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var newPath = Path.Combine(
            Path.GetDirectoryName(oldPath)!,
            $"{dialog.NewName}.pdf");
        if (string.Equals(oldPath, newPath, StringComparison.Ordinal))
        {
            return;
        }

        if (File.Exists(newPath) &&
            !string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                OwnerWindow,
                "Đã có file khác cùng tên trong thư mục này.",
                "Không thể đổi tên",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            MovePdfFile(oldPath, newPath);
        }
        catch (Exception exception) when (IsLibraryFileOperationError(exception))
        {
            ShowLibraryFileOperationError(
                "Không thể đổi tên file PDF.",
                "Lỗi đổi tên file",
                exception);
            return;
        }

        try
        {
            await UpdatePdfMetadataPathAsync(oldPath, newPath);
        }
        catch (Exception exception) when (IsLibraryFileOperationError(exception))
        {
            try
            {
                MovePdfFile(newPath, oldPath);
            }
            catch (Exception rollbackException) when (IsLibraryFileOperationError(rollbackException))
            {
                MessageBox.Show(
                    OwnerWindow,
                    $"Không cập nhật được metadata sau khi đổi tên. File mới vẫn ở “{newPath}”. " +
                    $"Không thể khôi phục tên cũ: {rollbackException.Message}",
                    "Lỗi cập nhật thư viện",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                await RefreshLibraryAsync(refreshFolders: false);
                return;
            }

            ShowLibraryFileOperationError(
                "Không cập nhật được metadata; đã khôi phục tên file ban đầu.",
                "Lỗi cập nhật thư viện",
                exception);
            return;
        }

        await RefreshLibraryAsync(refreshFolders: false);
    }

    private async void DeletePdf_Click(object sender, RoutedEventArgs e)
    {
        var entry = GetContextMenuEntry(sender);
        if (entry?.FullPath is not { } filePath || !File.Exists(filePath))
        {
            MessageBox.Show(
                OwnerWindow,
                "File PDF không còn tồn tại trong thư mục đã chọn.",
                "Không tìm thấy file PDF",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var confirmation = MessageBox.Show(
            OwnerWindow,
            $"Bạn có chắc muốn xóa vĩnh viễn file “{entry.Name}”?",
            "Xác nhận xóa file PDF",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        var metadataService = CreateLibraryMetadataService();
        IReadOnlyList<DocumentItem> originalDocuments;
        var metadataUpdated = false;
        try
        {
            originalDocuments = await metadataService.LoadAsync();
            var retainedDocuments = originalDocuments
                .Where(document => !PathsEqual(document.FilePath, filePath))
                .ToArray();
            if (retainedDocuments.Length != originalDocuments.Count)
            {
                await metadataService.SaveAsync(retainedDocuments);
                metadataUpdated = true;
            }
        }
        catch (Exception exception) when (IsLibraryFileOperationError(exception))
        {
            ShowLibraryFileOperationError(
                "Không cập nhật được metadata; file chưa bị xóa.",
                "Lỗi xóa file",
                exception);
            return;
        }

        try
        {
            File.Delete(filePath);
        }
        catch (Exception exception) when (IsLibraryFileOperationError(exception))
        {
            if (metadataUpdated)
            {
                try
                {
                    await metadataService.SaveAsync(originalDocuments);
                }
                catch (Exception rollbackException) when (IsLibraryFileOperationError(rollbackException))
                {
                    MessageBox.Show(
                        OwnerWindow,
                        $"Không thể xóa file. File vẫn còn, nhưng không khôi phục được metadata: " +
                        $"{rollbackException.Message}",
                        "Lỗi xóa file",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }

            ShowLibraryFileOperationError(
                metadataUpdated
                    ? "Không thể xóa file PDF; metadata đã được khôi phục."
                    : "Không thể xóa file PDF.",
                "Lỗi xóa file",
                exception);
            return;
        }

        await RefreshLibraryAsync(refreshFolders: false);
    }

    private static PdfLibraryEntry? GetContextMenuEntry(object sender) =>
        (sender as MenuItem)?.DataContext as PdfLibraryEntry;

    private async Task UpdatePdfMetadataPathAsync(string oldPath, string newPath)
    {
        var metadataService = CreateLibraryMetadataService();
        var documents = await metadataService.LoadAsync();
        var hasChanges = false;
        var updatedDocuments = documents
            .Select(document =>
            {
                if (!PathsEqual(document.FilePath, oldPath))
                {
                    return document;
                }

                hasChanges = true;
                return document with
                {
                    Name = Path.GetFileName(newPath),
                    FilePath = newPath
                };
            })
            .ToArray();

        if (hasChanges)
        {
            await metadataService.SaveAsync(updatedDocuments);
        }
    }

    private static bool PathsEqual(string left, string right)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (
            exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static void MovePdfFile(string sourcePath, string destinationPath)
    {
        if (!string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourcePath, destinationPath, StringComparison.Ordinal))
        {
            File.Move(sourcePath, destinationPath);
            return;
        }

        var temporaryPath = Path.Combine(
            Path.GetDirectoryName(sourcePath)!,
            $".{Guid.NewGuid():N}.rename");
        File.Move(sourcePath, temporaryPath);
        try
        {
            File.Move(temporaryPath, destinationPath);
        }
        catch (Exception exception) when (IsLibraryFileOperationError(exception))
        {
            File.Move(temporaryPath, sourcePath);
            throw;
        }
    }

    private static bool IsLibraryFileOperationError(Exception exception) =>
        exception is IOException or
            UnauthorizedAccessException or
            System.Security.SecurityException or
            ArgumentException or
            NotSupportedException or
            InvalidOperationException or
            System.Text.Json.JsonException;

    private void ShowLibraryFileOperationError(string message, string title, Exception exception) =>
        MessageBox.Show(
            OwnerWindow,
            $"{message} {exception.Message}",
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Error);

    private void OpenLibraryFolder_Click(object sender, RoutedEventArgs e)
    {
        if (LibraryFolderComboBox.SelectedItem is not PdfLibraryFolder folder)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(folder.FullPath)
            {
                UseShellExecute = true
            });
        }
        catch (Exception exception) when (
            exception is IOException or
                UnauthorizedAccessException or
                System.ComponentModel.Win32Exception or
                System.Security.SecurityException or
                ArgumentException or
                InvalidOperationException or
                NotSupportedException)
        {
            MessageBox.Show(
                OwnerWindow,
                $"Không thể mở thư mục đã chọn. {exception.Message}",
                "Lỗi mở thư mục",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void AddFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh hoặc file PDF",
            Multiselect = true,
            Filter = "Ảnh và PDF|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff;*.pdf|Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|PDF|*.pdf"
        };

        if (dialog.ShowDialog(OwnerWindow) != true || dialog.FileNames.Length == 0)
        {
            return;
        }

        await _scannerViewModel.ImportFilesAsync(dialog.FileNames);
    }

    private void CreatePdfFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreatePdfFolderWindow
        {
            Owner = OwnerWindow
        };
        if (dialog.ShowDialog() == true)
        {
            _scannerViewModel.CreatePdfLocation(dialog.FolderName);
        }
    }

    private async void EditScannerProfile(ScannerProfile? existingProfile)
    {
        var scanner = _scannerViewModel.SelectedScanner;
        if (scanner is null)
        {
            return;
        }

        var dialog = new ScannerProfileWindow(
            scanner.DeviceId,
            scanner.DeviceName,
            existingProfile)
        {
            Owner = OwnerWindow
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            await _scannerViewModel.SaveProfileAsync(dialog.CreateProfile());
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                OwnerWindow,
                $"Không lưu được cấu hình máy scan. {exception.Message}",
                "Lỗi lưu cấu hình",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void DeleteScannerProfile()
    {
        var profile = _scannerViewModel.SelectedProfile;
        if (profile is null)
        {
            return;
        }

        var result = MessageBox.Show(
            OwnerWindow,
            $"Xóa bí danh “{profile.Alias}” khỏi ứng dụng? Thiết bị vẫn được cài trong Windows.",
            "Xóa bí danh máy scan",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _scannerViewModel.DeleteSelectedProfileAsync();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                OwnerWindow,
                $"Không xóa được bí danh máy scan. {exception.Message}",
                "Lỗi cập nhật cấu hình",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ScanPageList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox ||
            e.OriginalSource is not DependencyObject source ||
            FindVisualAncestor<Button>(source) is not null)
        {
            _pageBeingDragged = null;
            return;
        }

        var item = ItemsControl.ContainerFromElement(listBox, source) as ListBoxItem;
        _pageBeingDragged = item?.DataContext as ScanPageViewModel;
        _pageDragStart = e.GetPosition(listBox);
    }

    private void ScanPageList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not ListBox listBox ||
            _pageBeingDragged is not { } page ||
            e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var currentPosition = e.GetPosition(listBox);
        if (Math.Abs(currentPosition.X - _pageDragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(currentPosition.Y - _pageDragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        _pageBeingDragged = null;
        var dragData = new DataObject(ScanPageDataFormat, page.FilePath);
        DragDrop.DoDragDrop(listBox, dragData, DragDropEffects.Move);
    }

    private void ScanPageList_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(ScanPageDataFormat)
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void ScanPageList_Drop(object sender, DragEventArgs e)
    {
        if (sender is not ListBox listBox ||
            e.Data.GetData(ScanPageDataFormat) is not string filePath ||
            listBox.Items.Count == 0)
        {
            return;
        }

        var page = _scannerViewModel.ScanPages.FirstOrDefault(item =>
            string.Equals(item.FilePath, filePath, StringComparison.Ordinal));
        if (page is null)
        {
            return;
        }

        var target = e.OriginalSource is DependencyObject source
            ? ItemsControl.ContainerFromElement(listBox, source) as ListBoxItem
            : null;
        var targetIndex = target is null
            ? listBox.Items.Count - 1
            : listBox.ItemContainerGenerator.IndexFromContainer(target);
        var insertAfter = target is null ||
            e.GetPosition(target).Y >= target.ActualHeight / 2;

        _scannerViewModel.MoveScanPage(page, targetIndex, insertAfter);
        e.Handled = true;
    }

    private static T? FindVisualAncestor<T>(DependencyObject element)
        where T : DependencyObject
    {
        for (var current = element; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is T ancestor)
            {
                return ancestor;
            }
        }

        return null;
    }
}
