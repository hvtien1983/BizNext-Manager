using System.Windows;
using ScanApp.ViewModels;
using ScanDevice.Models;
using ScanDevice.Services;

namespace ScanApp;

public partial class MainWindow : Window
{
    private readonly ScannerSettingsViewModel _scannerViewModel;
    private bool _initialized;

    public MainWindow()
    {
        InitializeComponent();

        _scannerViewModel = new ScannerSettingsViewModel(
            new WiaScannerService(),
            new JsonScannerConfigStore());
        _scannerViewModel.ProfileEditRequested += EditScannerProfile;
        _scannerViewModel.ProfileDeleteRequested += DeleteScannerProfile;
        DataContext = _scannerViewModel;
        Loaded += MainWindow_Loaded;
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
}
