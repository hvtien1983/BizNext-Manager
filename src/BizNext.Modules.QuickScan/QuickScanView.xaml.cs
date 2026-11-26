using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ScanApp.Services;
using ScanApp.ViewModels;
using ScanCore.Services;
using ScanDevice.Models;
using ScanDevice.Services;
using System.IO;
using ScanApp.Models;
using Microsoft.Win32;

namespace ScanApp;

public partial class MainWindow : Window
{
    private const string ScanPageDataFormat = "QuickScanManager.ScanPage";

    private readonly ScannerSettingsViewModel _scannerViewModel;
    private bool _initialized;
    private Point _pageDragStart;
    private ScanPageViewModel? _pageBeingDragged;

    public MainWindow(ApplicationSettings applicationSettings, string? settingsLoadError)
    {
        ArgumentNullException.ThrowIfNull(applicationSettings);
        InitializeComponent();

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
        Loaded += MainWindow_Loaded;
    }

    private async void OpenAppSettings_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AppSettingsWindow(
            _scannerViewModel.GetApplicationSettingsCopy(),
            _scannerViewModel.ConfiguredScanners)
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true || dialog.Settings is null)
        {
            return;
        }

        try
        {
            await _scannerViewModel.SaveApplicationSettingsAsync(dialog.Settings);
            App.ApplyTheme(dialog.Settings.Theme);
        }
        catch (Exception exception) when (
            exception is IOException or
                UnauthorizedAccessException or
                ArgumentException or
                InvalidOperationException or
                System.Security.SecurityException)
        {
            MessageBox.Show(
                this,
                $"Không lưu được cài đặt ứng dụng. {exception.Message}",
                "Lỗi lưu cài đặt",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await _scannerViewModel.InitializeAsync();
    }

    private async void AddFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh hoặc file PDF",
            Multiselect = true,
            Filter = "Ảnh và PDF|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff;*.pdf|Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|PDF|*.pdf"
        };

        if (dialog.ShowDialog(this) != true || dialog.FileNames.Length == 0)
        {
            return;
        }

        await _scannerViewModel.ImportFilesAsync(dialog.FileNames);
    }

    private void CreatePdfFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreatePdfFolderWindow
        {
            Owner = this
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
            Owner = this
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
                this,
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
            this,
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
                this,
                $"Không xóa được bí danh máy scan. {exception.Message}",
                "Lỗi cập nhật cấu hình",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {

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
