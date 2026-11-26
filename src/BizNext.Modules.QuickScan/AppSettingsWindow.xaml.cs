using System.IO;
using System.Windows;
using Microsoft.Win32;
using ScanApp.Models;
using ScanApp.ViewModels;

namespace ScanApp;

public partial class AppSettingsWindow : Window
{
    private readonly IReadOnlyList<ScannerOption> _configuredScanners;

    public AppSettingsWindow(
        ApplicationSettings settings,
        IReadOnlyList<ScannerOption> configuredScanners)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(configuredScanners);
        InitializeComponent();

        _configuredScanners = configuredScanners;
        PdfDirectoryTextBox.Text = settings.DefaultPdfDirectory;
        DefaultScannerComboBox.ItemsSource = _configuredScanners;
        DefaultScannerComboBox.SelectedValue = settings.DefaultScannerDeviceId;
        AppendTimestampCheckBox.IsChecked = settings.AppendTimestampToPdfName;
        LightThemeRadioButton.IsChecked = settings.Theme == ThemePreference.Light;
        DarkThemeRadioButton.IsChecked = settings.Theme == ThemePreference.Dark;
        SystemThemeRadioButton.IsChecked = settings.Theme == ThemePreference.System;

        if (_configuredScanners.Count == 0)
        {
            ScannerHelpText.Text = "Chưa có máy scan được cấu hình. Hãy thêm bí danh cho máy scan trước.";
            DefaultScannerComboBox.IsEnabled = false;
        }
    }

    public ApplicationSettings? Settings { get; private set; }

    private void BrowsePdfDirectory_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Chọn thư mục lưu PDF",
            InitialDirectory = Directory.Exists(PdfDirectoryTextBox.Text)
                ? PdfDirectoryTextBox.Text
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == true)
        {
            PdfDirectoryTextBox.Text = dialog.FolderName;
        }
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        var directory = PdfDirectoryTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(directory))
        {
            MessageBox.Show(
                this,
                "Chọn thư mục để lưu các file PDF.",
                "Thiếu thư mục lưu",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            directory = Path.GetFullPath(directory);
            Directory.CreateDirectory(directory);
        }
        catch (Exception exception) when (
            exception is ArgumentException or
                NotSupportedException or
                PathTooLongException or
                IOException or
                UnauthorizedAccessException)
        {
            MessageBox.Show(
                this,
                $"Không thể sử dụng thư mục đã chọn. {exception.Message}",
                "Thư mục không hợp lệ",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var selectedDeviceId = DefaultScannerComboBox.SelectedValue as string;
        if (selectedDeviceId is not null &&
            !_configuredScanners.Any(scanner =>
                string.Equals(scanner.DeviceId, selectedDeviceId, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(
                this,
                "Máy scan mặc định không còn trong danh sách máy đã cấu hình.",
                "Máy scan không hợp lệ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Settings = new ApplicationSettings
        {
            DefaultPdfDirectory = directory,
            DefaultScannerDeviceId = selectedDeviceId,
            AppendTimestampToPdfName = AppendTimestampCheckBox.IsChecked == true,
            Theme = DarkThemeRadioButton.IsChecked == true
                ? ThemePreference.Dark
                : SystemThemeRadioButton.IsChecked == true
                    ? ThemePreference.System
                    : ThemePreference.Light
        };
        DialogResult = true;
    }
}
