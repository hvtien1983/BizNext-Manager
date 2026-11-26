using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScanApp.Commands;
using ScanApp.Models;
using ScanApp.Services;
using ScanCore.Models;
using ScanCore.Services;
using ScanDevice.Models;
using ScanDevice.Services;
using PDFtoImage;
using PDFtoImage.Exceptions;

namespace ScanApp.ViewModels;

internal static class PdfPageSizeOptions
{
    public static IReadOnlyList<PdfPageSize> All { get; } =
    [
        new("A4", 595.28d, 841.89d),
        new("A5", 419.53d, 595.28d),
        new("A3", 841.89d, 1190.55d),
        new("Letter", 612d, 792d),
        new("Legal", 612d, 1008d),
        new("B5", 498.90d, 708.66d),
        new("Tabloid", 792d, 1224d)
    ];
}

public sealed class ScannerSettingsViewModel : INotifyPropertyChanged
{
    private const string AddNewScannerDeviceId = "__quickscan_add_new_scanner__";

    private readonly IScannerService _scannerService;
    private readonly IScannerConfigStore _configStore;
    private readonly JsonApplicationSettingsStore _applicationSettingsStore;
    private readonly ScannedPdfExportService _pdfExportService;
    private readonly JsonLibraryService _libraryService;
    private readonly SemaphoreSlim _configLock = new(1, 1);
    private readonly ManualResetEventSlim _batchContinueSignal = new(initialState: true);
    private readonly string _scanOutputDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QuickScanManager",
        "scans");
    private string _pdfStorageDirectory;
    private ApplicationSettings _applicationSettings;
    private readonly string? _settingsLoadError;
    private ScannerConfig _config = new();
    private ScannerOption? _selectedScanner;
    private string? _selectedScannerId;
    private string _statusMessage = "Đang tìm máy scan...";
    private StatusMessageType _statusMessageType = StatusMessageType.Warning;
    private string _selectedColorMode = "Color";
    private int _selectedResolutionDpi = 300;
    private string _selectedSource = "Flatbed";
    private ScanPageViewModel? _selectedPage;
    private bool _isScanning;
    private bool _isBatchScanning;
    private bool _isBatchPaused;
    private bool _isCheckingAutomaticFeeder;
    private bool _hasAutomaticFeeder;
    private bool _isSavingPdf;
    private bool _isImportingFiles;
    private string _pdfFileName = string.Empty;
    private string _pdfLocation = "Thư mục mặc định";
    private PdfPageSize _selectedPdfPageSize = PdfPageSizeOptions.All[0];
    private bool _isLoadingScanners;
    private bool _hasLoadedConfig;
    private bool _configCanBeSaved = true;

    public ScannerSettingsViewModel(
        IScannerService scannerService,
        IScannerConfigStore configStore,
        JsonApplicationSettingsStore applicationSettingsStore,
        ScannedPdfExportService pdfExportService,
        JsonLibraryService libraryService,
        string pdfStorageDirectory,
        ApplicationSettings applicationSettings,
        string? settingsLoadError)
    {
        ArgumentNullException.ThrowIfNull(applicationSettings);
        _scannerService = scannerService;
        _configStore = configStore;
        _applicationSettingsStore = applicationSettingsStore;
        _pdfExportService = pdfExportService;
        _libraryService = libraryService;
        _pdfStorageDirectory = Path.GetFullPath(pdfStorageDirectory);
        _applicationSettings = applicationSettings;
        _settingsLoadError = settingsLoadError;

        ScanPageCommand = new AsyncRelayCommand(
            ScanPageAsync,
            () => SelectedScanner is not null && !IsScanning && !IsSavingPdf && !IsImportingFiles);
        BatchScanCommand = new RelayCommand(
            OnBatchScanButtonClicked,
            () => CanBatchScan);
        SavePdfCommand = new AsyncRelayCommand(SavePdfAsync, CanSavePdf);
        DeleteScanPageCommand = new RelayCommand<ScanPageViewModel>(
            DeleteScanPage,
            _ => !IsSavingPdf);
        RotateSelectedPageLeftCommand = new RelayCommand(
            () => TransformSelectedPage(-90, flipHorizontal: false, flipVertical: false),
            CanTransformSelectedPage);
        RotateSelectedPageRightCommand = new RelayCommand(
            () => TransformSelectedPage(90, flipHorizontal: false, flipVertical: false),
            CanTransformSelectedPage);
        FlipSelectedPageVerticalCommand = new RelayCommand(
            () => TransformSelectedPage(0, flipHorizontal: false, flipVertical: true),
            CanTransformSelectedPage);
        FlipSelectedPageHorizontalCommand = new RelayCommand(
            () => TransformSelectedPage(0, flipHorizontal: true, flipVertical: false),
            CanTransformSelectedPage);
        OpenWindowsScannerSettingsCommand = new RelayCommand(OpenWindowsScannerSettings);
        AddOrEditProfileCommand = new RelayCommand(
            () => ProfileEditRequested?.Invoke(CreateProfileCopy(SelectedProfile)),
            () => SelectedScanner is not null);
        DeleteProfileCommand = new RelayCommand(
            () => ProfileDeleteRequested?.Invoke(),
            () => SelectedProfile is not null && _configCanBeSaved);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event Action<ScannerProfile?>? ProfileEditRequested;

    public event Action? ProfileDeleteRequested;

    public ObservableCollection<ScannerOption> Scanners { get; } = [];

    public IReadOnlyList<ScannerOption> ConfiguredScanners =>
        _config.Profiles.Select(profile =>
        {
            var availableScanner = Scanners.FirstOrDefault(scanner =>
                !scanner.IsAddNew &&
                string.Equals(scanner.DeviceId, profile.DeviceId, StringComparison.OrdinalIgnoreCase));
            return availableScanner ?? new ScannerOption(
                profile.DeviceId,
                profile.Alias,
                $"{profile.Alias} (không kết nối)",
                profile.Alias);
        }).ToArray();

    public ObservableCollection<ScanPageViewModel> ScanPages { get; } = [];

    public ObservableCollection<string> PdfLocations { get; } = [];

    public IReadOnlyList<PdfPageSize> PaperSizes { get; } = PdfPageSizeOptions.All;

    public int AvailableScannerCount => Scanners.Count(scanner => !scanner.IsAddNew);

    public int ScanPageCount => ScanPages.Count;

    public AsyncRelayCommand ScanPageCommand { get; }

    public RelayCommand BatchScanCommand { get; }

    public bool CanBatchScan =>
        IsBatchScanning ||
        (!IsScanning && !IsSavingPdf && !IsImportingFiles && SelectedScanner is not null && _hasAutomaticFeeder);

    public bool CanChangeScanConfiguration => !IsScanning && !IsSavingPdf && !IsImportingFiles;

    public string BatchScanButtonText =>
        !IsBatchScanning ? "Quét hàng loạt" : IsBatchPaused ? "Tiếp tục" : "Tạm dừng";

    public string BatchScanAvailabilityMessage =>
        IsBatchScanning
            ? IsBatchPaused
                ? "Đã tạm dừng. Chọn Tiếp tục để quét các trang còn lại."
                : "Tạm dừng sẽ hoàn tất trang đang quét rồi dừng."
            : _isCheckingAutomaticFeeder
                ? "Đang kiểm tra khả năng nạp giấy tự động..."
                : _hasAutomaticFeeder
                    ? "Máy scan đã chọn hỗ trợ khay nạp ADF."
                    : "Quét hàng loạt cần máy scan có khay nạp ADF.";

    public RelayCommand<ScanPageViewModel> DeleteScanPageCommand { get; }

    public RelayCommand RotateSelectedPageLeftCommand { get; }

    public RelayCommand RotateSelectedPageRightCommand { get; }

    public RelayCommand FlipSelectedPageVerticalCommand { get; }

    public RelayCommand FlipSelectedPageHorizontalCommand { get; }

    public RelayCommand OpenWindowsScannerSettingsCommand { get; }

    public RelayCommand AddOrEditProfileCommand { get; }

    public RelayCommand DeleteProfileCommand { get; }

    public async Task ImportFilesAsync(IReadOnlyList<string> filePaths)
    {
        ArgumentNullException.ThrowIfNull(filePaths);
        if (filePaths.Count == 0 || IsScanning || IsSavingPdf || IsImportingFiles)
        {
            return;
        }

        IsImportingFiles = true;
        try
        {
            Directory.CreateDirectory(_scanOutputDirectory);
            var importedCount = 0;
            var failedFiles = new List<string>();

            foreach (var sourcePath in filePaths)
            {
                try
                {
                    var extension = Path.GetExtension(sourcePath);
                    if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        importedCount += await ImportPdfAsync(sourcePath);
                    }
                    else if (IsSupportedImageExtension(extension))
                    {
                        var destinationPath = Path.Combine(
                            _scanOutputDirectory,
                            $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}");
                        await using (var source = File.OpenRead(sourcePath))
                        await using (var destination = File.Create(destinationPath))
                        {
                            await source.CopyToAsync(destination);
                        }

                        AddScannedPage(destinationPath);
                        importedCount++;
                    }
                    else
                    {
                        failedFiles.Add($"{Path.GetFileName(sourcePath)} (định dạng không được hỗ trợ)");
                    }
                }
                catch (Exception exception) when (
                    exception is IOException or
                        UnauthorizedAccessException or
                        ArgumentException or
                        InvalidDataException or
                        NotSupportedException or
                        FileFormatException or
                        System.Security.SecurityException or
                        PdfException)
                {
                    failedFiles.Add($"{Path.GetFileName(sourcePath)} ({exception.Message})");
                }
            }

            OnPropertyChanged(nameof(ScanPageCount));
            SavePdfCommand.RaiseCanExecuteChanged();

            if (failedFiles.Count == 0)
            {
                SetStatusMessage(
                    $"Đã nạp {importedCount} trang từ file đã chọn.",
                    StatusMessageType.Success);
            }
            else
            {
                SetStatusMessage(
                    $"Đã nạp {importedCount} trang. Không nhập được: {string.Join("; ", failedFiles)}",
                    StatusMessageType.Warning);
            }
        }
        finally
        {
            IsImportingFiles = false;
        }
    }

    private async Task<int> ImportPdfAsync(string pdfPath)
    {
        await using var pdfStream = File.OpenRead(pdfPath);
        var pageCount = await Task.Run(() =>
            Conversion.GetPageCount(pdfStream, leaveOpen: true, password: null));
        if (pageCount == 0)
        {
            throw new InvalidDataException("File PDF không có trang.");
        }

        var importedCount = 0;
        var renderOptions = new PDFtoImage.RenderOptions
        {
            Dpi = 300,
            WithAspectRatio = true,
            BackgroundColor = SkiaSharp.SKColors.White
        };

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            var destinationPath = Path.Combine(
                _scanOutputDirectory,
                $"{Guid.NewGuid():N}.png");

            await Task.Run(() =>
            {
                pdfStream.Position = 0;
                using var output = File.Create(destinationPath);
                Conversion.SavePng(
                    imageStream: output,
                    pdfStream: pdfStream,
                    page: pageIndex,
                    leaveOpen: true,
                    password: null,
                    options: renderOptions);
            });

            AddScannedPage(destinationPath);
            importedCount++;
        }

        return importedCount;
    }

    private static bool IsSupportedImageExtension(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".tif", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase);

    public ScannerOption? SelectedScanner
    {
        get => _selectedScanner;
        set
        {
            if (ReferenceEquals(_selectedScanner, value))
            {
                return;
            }

            _selectedScanner = value;
            _selectedScannerId = value?.DeviceId;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedScannerId));
            OnPropertyChanged(nameof(SelectedProfile));
            OnPropertyChanged(nameof(ProfileButtonText));
            ApplyProfileSettings(value);
            ScanPageCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(CanBatchScan));
            BatchScanCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(CanChangeScanConfiguration));
            AddOrEditProfileCommand.RaiseCanExecuteChanged();
            DeleteProfileCommand.RaiseCanExecuteChanged();

            if (!_isLoadingScanners)
            {
                _ = RefreshAutomaticFeederSupportAsync(value?.DeviceId);
                SetStatusMessage(
                    value is null ? "Chưa chọn máy scan." : $"Đã chọn {value.DisplayName}.",
                    value is null ? StatusMessageType.Warning : StatusMessageType.Success);
            }

            if (value is not null && !_isLoadingScanners && _configCanBeSaved)
            {
                _ = SaveLastUsedScannerAsync(value.DeviceId);
            }
        }
    }

    public string? SelectedScannerId
    {
        get => _selectedScannerId;
        set
        {
            if (string.Equals(value, AddNewScannerDeviceId, StringComparison.Ordinal))
            {
                OnPropertyChanged();
                OpenWindowsScannerSettingsCommand.Execute(null);
                return;
            }

            var scanner = Scanners.FirstOrDefault(item =>
                string.Equals(item.DeviceId, value, StringComparison.OrdinalIgnoreCase));
            SelectedScanner = scanner;
        }
    }

    public ScannerProfile? SelectedProfile =>
        SelectedScanner is null
            ? null
            : _config.Profiles.FirstOrDefault(profile =>
                string.Equals(
                    profile.DeviceId,
                    SelectedScanner.DeviceId,
                    StringComparison.OrdinalIgnoreCase));

    public string ProfileButtonText => SelectedProfile is null ? "Thêm bí danh" : "Sửa bí danh";

    public string SelectedColorMode
    {
        get => _selectedColorMode;
        set => SetField(ref _selectedColorMode, value);
    }

    public int SelectedResolutionDpi
    {
        get => _selectedResolutionDpi;
        set
        {
            if (_selectedResolutionDpi == value)
            {
                return;
            }

            _selectedResolutionDpi = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedResolutionIndex));
        }
    }

    public int SelectedResolutionIndex
    {
        get => SelectedResolutionDpi switch
        {
            150 => 0,
            200 => 1,
            _ => 2
        };
        set => SelectedResolutionDpi = value switch
        {
            0 => 150,
            1 => 200,
            _ => 300
        };
    }

    public string SelectedSource
    {
        get => _selectedSource;
        set => SetField(ref _selectedSource, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value)
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public StatusMessageType StatusSeverity => _statusMessageType;

    public string PdfFileName
    {
        get => _pdfFileName;
        set
        {
            if (string.Equals(_pdfFileName, value, StringComparison.Ordinal))
            {
                return;
            }

            _pdfFileName = value;
            OnPropertyChanged();
            SavePdfCommand.RaiseCanExecuteChanged();
        }
    }

    public string PdfLocation
    {
        get => _pdfLocation;
        set
        {
            if (string.Equals(_pdfLocation, value, StringComparison.Ordinal))
            {
                return;
            }

            _pdfLocation = value ?? "Thư mục mặc định";
            OnPropertyChanged();
        }
    }

    public PdfPageSize SelectedPdfPageSize
    {
        get => _selectedPdfPageSize;
        set
        {
            if (ReferenceEquals(_selectedPdfPageSize, value))
            {
                return;
            }

            _selectedPdfPageSize = value ?? PdfPageSizeOptions.All[0];
            OnPropertyChanged();
        }
    }

    public ScanPageViewModel? SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (ReferenceEquals(_selectedPage, value))
            {
                return;
            }

            _selectedPage = value;
            OnPropertyChanged();
            RaisePageTransformCommands();
        }
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (_isScanning == value)
            {
                return;
            }

            _isScanning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanBatchScan));
            OnPropertyChanged(nameof(CanChangeScanConfiguration));
            BatchScanCommand.RaiseCanExecuteChanged();
            ScanPageCommand.RaiseCanExecuteChanged();
            SavePdfCommand.RaiseCanExecuteChanged();
            RaisePageTransformCommands();
        }
    }

    public bool IsBatchScanning
    {
        get => _isBatchScanning;
        private set
        {
            if (_isBatchScanning == value)
            {
                return;
            }

            _isBatchScanning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BatchScanButtonText));
            OnPropertyChanged(nameof(BatchScanAvailabilityMessage));
            OnPropertyChanged(nameof(CanBatchScan));
            BatchScanCommand.RaiseCanExecuteChanged();
            RaisePageTransformCommands();
        }
    }

    public bool IsBatchPaused
    {
        get => _isBatchPaused;
        private set
        {
            if (_isBatchPaused == value)
            {
                return;
            }

            _isBatchPaused = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BatchScanButtonText));
            OnPropertyChanged(nameof(BatchScanAvailabilityMessage));
        }
    }

    public bool IsSavingPdf
    {
        get => _isSavingPdf;
        private set
        {
            if (_isSavingPdf == value)
            {
                return;
            }

            _isSavingPdf = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanBatchScan));
            OnPropertyChanged(nameof(CanChangeScanConfiguration));
            BatchScanCommand.RaiseCanExecuteChanged();
            SavePdfCommand.RaiseCanExecuteChanged();
            ScanPageCommand.RaiseCanExecuteChanged();
            DeleteScanPageCommand.RaiseCanExecuteChanged();
            RaisePageTransformCommands();
        }
    }

    public bool IsImportingFiles
    {
        get => _isImportingFiles;
        private set
        {
            if (_isImportingFiles == value)
            {
                return;
            }

            _isImportingFiles = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanBatchScan));
            OnPropertyChanged(nameof(CanChangeScanConfiguration));
            BatchScanCommand.RaiseCanExecuteChanged();
            SavePdfCommand.RaiseCanExecuteChanged();
            ScanPageCommand.RaiseCanExecuteChanged();
            RaisePageTransformCommands();
        }
    }

    public AsyncRelayCommand SavePdfCommand { get; }

    public async Task InitializeAsync()
    {
        var settingsLoadError = _settingsLoadError;
        try
        {
            if (string.IsNullOrWhiteSpace(_applicationSettings.DefaultPdfDirectory))
            {
                _applicationSettings.DefaultPdfDirectory = _pdfStorageDirectory;
            }

            _applicationSettings.DefaultPdfDirectory =
                Path.GetFullPath(_applicationSettings.DefaultPdfDirectory);
            _pdfStorageDirectory = _applicationSettings.DefaultPdfDirectory;
            RefreshPdfLocations();
        }
        catch (Exception exception) when (
            exception is InvalidDataException or
                IOException or
                UnauthorizedAccessException or
                ArgumentException or
                NotSupportedException)
        {
            _applicationSettings = new ApplicationSettings
            {
                DefaultPdfDirectory = _pdfStorageDirectory
            };
            settingsLoadError =
                $"Không đọc được cài đặt ứng dụng. Đang dùng giá trị mặc định; hãy kiểm tra app-settings.json. {exception.Message}";
        }

        await LoadScannersAsync();
        if (settingsLoadError is not null)
        {
            SetStatusMessage(settingsLoadError, StatusMessageType.Error);
        }
    }

    public ApplicationSettings GetApplicationSettingsCopy() => new()
    {
        DefaultPdfDirectory = _applicationSettings.DefaultPdfDirectory,
        DefaultScannerDeviceId = _applicationSettings.DefaultScannerDeviceId ??
            ConfiguredScanners.FirstOrDefault(scanner =>
                string.Equals(
                    scanner.DeviceId,
                    SelectedScanner?.DeviceId,
                    StringComparison.OrdinalIgnoreCase))?.DeviceId,
        AppendTimestampToPdfName = _applicationSettings.AppendTimestampToPdfName,
        Theme = _applicationSettings.Theme
    };

    public async Task SaveApplicationSettingsAsync(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var directory = Path.GetFullPath(settings.DefaultPdfDirectory);
        Directory.CreateDirectory(directory);

        var savedSettings = new ApplicationSettings
        {
            DefaultPdfDirectory = directory,
            DefaultScannerDeviceId = settings.DefaultScannerDeviceId,
            AppendTimestampToPdfName = settings.AppendTimestampToPdfName,
            Theme = settings.Theme
        };
        if (settings.Theme is not (
                ThemePreference.Light or
                ThemePreference.Dark or
                ThemePreference.System))
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "The selected theme is invalid.");
        }
        if (savedSettings.DefaultScannerDeviceId is not null &&
            !ConfiguredScanners.Any(scanner =>
                string.Equals(
                    scanner.DeviceId,
                    savedSettings.DefaultScannerDeviceId,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException(
                "The default scanner must be selected from a saved scanner profile.",
                nameof(settings));
        }

        await _applicationSettingsStore.SaveAsync(savedSettings);
        _applicationSettings = savedSettings;
        _pdfStorageDirectory = savedSettings.DefaultPdfDirectory;
        RefreshPdfLocations();

        var selectedDefaultScanner = savedSettings.DefaultScannerDeviceId is null
            ? null
            : Scanners.FirstOrDefault(scanner =>
                !scanner.IsAddNew &&
                string.Equals(
                    scanner.DeviceId,
                    savedSettings.DefaultScannerDeviceId,
                    StringComparison.OrdinalIgnoreCase));
        if (selectedDefaultScanner is not null)
        {
            SelectedScanner = selectedDefaultScanner;
        }

        SetStatusMessage("Đã lưu cài đặt ứng dụng.", StatusMessageType.Success);
    }

    public void CreatePdfLocation(string folderName)
    {
        var name = folderName?.Trim();
        if (string.IsNullOrWhiteSpace(name) ||
            name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            !string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal) ||
            name.EndsWith('.') ||
            IsReservedFileName(name))
        {
            SetStatusMessage(
                "Tên thư mục không hợp lệ.",
                StatusMessageType.Warning);
            return;
        }

        try
        {
            var newDirectory = Path.Combine(_pdfStorageDirectory, name);
            Directory.CreateDirectory(newDirectory);
            RefreshPdfLocations();
            PdfLocation = name;
            SetStatusMessage($"Đã tạo thư mục “{name}” trong thư mục mặc định.", StatusMessageType.Success);
        }
        catch (Exception exception) when (
            exception is IOException or
                UnauthorizedAccessException or
                ArgumentException or
                NotSupportedException or
                System.Security.SecurityException)
        {
            SetStatusMessage($"Không tạo được thư mục: {exception.Message}", StatusMessageType.Error);
        }
    }

    private void RefreshPdfLocations()
    {
        var selectedLocation = PdfLocation;
        PdfLocations.Clear();
        PdfLocations.Add("Thư mục mặc định");

        if (!Directory.Exists(_pdfStorageDirectory))
        {
            PdfLocation = "Thư mục mặc định";
            return;
        }

        foreach (var directory in Directory.GetDirectories(_pdfStorageDirectory))
        {
            PdfLocations.Add(Path.GetFileName(directory));
        }

        PdfLocation = PdfLocations.Any(location =>
            string.Equals(location, selectedLocation, StringComparison.Ordinal))
            ? selectedLocation
            : "Thư mục mặc định";
    }

    public void MoveScanPage(ScanPageViewModel page, int targetIndex, bool insertAfter)
    {
        var sourceIndex = ScanPages.IndexOf(page);
        if (sourceIndex < 0 || targetIndex < 0 || targetIndex >= ScanPages.Count)
        {
            return;
        }

        if (IsSavingPdf)
        {
            return;
        }

        var destinationIndex = targetIndex + (insertAfter ? 1 : 0);
        if (sourceIndex < destinationIndex)
        {
            destinationIndex--;
        }

        destinationIndex = Math.Clamp(destinationIndex, 0, ScanPages.Count - 1);
        if (sourceIndex == destinationIndex)
        {
            return;
        }

        var selectedFilePath = SelectedPage?.FilePath;
        ScanPages.Move(sourceIndex, destinationIndex);
        RefreshPageNames();
        SelectedPage = ScanPages.FirstOrDefault(item =>
            string.Equals(item.FilePath, selectedFilePath, StringComparison.Ordinal));
        SetStatusMessage(
            $"Đã cập nhật thứ tự {ScanPages[destinationIndex].Name}.",
            StatusMessageType.Success);
    }

    private bool CanTransformSelectedPage() =>
        SelectedPage is not null &&
        !IsScanning &&
        !IsBatchScanning &&
        !IsSavingPdf &&
        !IsImportingFiles;

    private void RaisePageTransformCommands()
    {
        RotateSelectedPageLeftCommand.RaiseCanExecuteChanged();
        RotateSelectedPageRightCommand.RaiseCanExecuteChanged();
        FlipSelectedPageVerticalCommand.RaiseCanExecuteChanged();
        FlipSelectedPageHorizontalCommand.RaiseCanExecuteChanged();
    }

    private void TransformSelectedPage(double angle, bool flipHorizontal, bool flipVertical)
    {
        var page = SelectedPage;
        if (page is null || !CanTransformSelectedPage())
        {
            return;
        }

        try
        {
            var transformedPreview = TransformPreview(page.Preview, angle, flipHorizontal, flipVertical);
            var transformedPage = page with { Preview = transformedPreview };
            var index = ScanPages.IndexOf(page);
            if (index < 0)
            {
                return;
            }

            ScanPages[index] = transformedPage;
            SelectedPage = transformedPage;
            SetStatusMessage(
                angle != 0
                    ? $"Đã xoay {page.Name}."
                    : flipHorizontal
                        ? $"Đã lật trái/phải {page.Name}."
                        : $"Đã lật lên/xuống {page.Name}.",
                StatusMessageType.Success);
        }
        catch (Exception exception) when (
            exception is ArgumentException or
                InvalidOperationException or
                NotSupportedException)
        {
            SetStatusMessage(
                $"Không thể chỉnh sửa {page.Name}: {exception.Message}",
                StatusMessageType.Error);
        }
    }

    private static BitmapSource TransformPreview(
        BitmapSource source,
        double angle,
        bool flipHorizontal,
        bool flipVertical)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var centerX = width / 2d;
        var centerY = height / 2d;

        var matrix = Matrix.Identity;
        matrix.RotateAt(angle, centerX, centerY);
        matrix.ScaleAt(
            flipHorizontal ? -1 : 1,
            flipVertical ? -1 : 1,
            centerX,
            centerY);

        var bounds = new Rect(0, 0, width, height);
        bounds.Transform(matrix);
        matrix.Translate(-bounds.X, -bounds.Y);

        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.PushTransform(new MatrixTransform(matrix));
            context.DrawImage(source, new Rect(0, 0, width, height));
            context.Pop();
        }

        var transformed = new RenderTargetBitmap(
            Math.Max(1, (int)Math.Round(bounds.Width)),
            Math.Max(1, (int)Math.Round(bounds.Height)),
            96,
            96,
            PixelFormats.Pbgra32);
        transformed.Render(visual);
        transformed.Freeze();
        return transformed;
    }

    private bool CanSavePdf() =>
        ScanPages.Count > 0 &&
        !IsScanning &&
        !IsSavingPdf &&
        !IsImportingFiles &&
        !string.IsNullOrWhiteSpace(PdfFileName);

    private async Task SavePdfAsync()
    {
        var fileName = PdfFileName.Trim();
        if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            fileName = fileName[..^4].TrimEnd();
        }

        if (!string.Equals(PdfFileName, fileName, StringComparison.Ordinal))
        {
            PdfFileName = fileName;
        }

        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            fileName.EndsWith('.') ||
            IsReservedFileName(fileName))
        {
            SetStatusMessage(
                "Tên file không hợp lệ. Hãy nhập tên file, không kèm đường dẫn.",
                StatusMessageType.Warning);
            return;
        }

        var outputDirectory = string.Equals(PdfLocation, "Thư mục mặc định", StringComparison.Ordinal)
            ? _pdfStorageDirectory
            : Path.Combine(_pdfStorageDirectory, PdfLocation);
        Directory.CreateDirectory(outputDirectory);

        var storedFileName = fileName;
        if (_applicationSettings.AppendTimestampToPdfName)
        {
            var timestamp = DateTime.Now.ToString(
                "yyyy-MM-dd_hh-mm-ss",
                CultureInfo.InvariantCulture);
            storedFileName = $"{fileName}_{timestamp}";
            var collision = 2;
            while (File.Exists(Path.Combine(outputDirectory, $"{storedFileName}.pdf")))
            {
                storedFileName = $"{fileName}_{timestamp}_{collision++}";
            }
        }

        var outputPath = Path.Combine(outputDirectory, $"{storedFileName}.pdf");
        if (File.Exists(outputPath))
        {
            SetStatusMessage(
                $"File “{storedFileName}.pdf” đã tồn tại. Hãy chọn tên khác để tránh ghi đè.",
                StatusMessageType.Warning);
            return;
        }

        var pages = ScanPages.ToArray();
        var pageCount = pages.Length;
        IsSavingPdf = true;
        string? createdPdfPath = null;
        try
        {
            SetStatusMessage("Đang tạo file PDF từ các trang đã quét...", StatusMessageType.Warning);
            var documents = (await _libraryService.LoadAsync()).ToList();
            await _pdfExportService.CreateAsync(
                pages.Select(page => page.Preview).ToArray(),
                outputPath,
                SelectedPdfPageSize);
            createdPdfPath = outputPath;

            documents.Add(new DocumentItem
            {
                Name = storedFileName,
                FilePath = outputPath,
                Category = null,
                PageCount = pageCount,
                Tags = null
            });
            await _libraryService.SaveAsync(documents);
            SetStatusMessage(
                $"Đã lưu {storedFileName}.pdf ({pageCount} trang) và cập nhật library.json.",
                StatusMessageType.Success);
            PdfFileName = string.Empty;
        }
        catch (Exception exception) when (
            exception is IOException or
                UnauthorizedAccessException or
                InvalidDataException or
                System.Text.Json.JsonException or
                ArgumentException or
                InvalidOperationException or
                NotSupportedException or
                FormatException or
                FileFormatException or
                System.Security.SecurityException)
        {
            var message = createdPdfPath is null
                ? $"Không tạo được file PDF: {exception.Message}"
                : $"PDF đã tạo tại “{createdPdfPath}” nhưng không cập nhật được thư viện: {exception.Message}";
            SetStatusMessage(message, StatusMessageType.Error);
        }
        finally
        {
            IsSavingPdf = false;
        }
    }

    private static bool IsReservedFileName(string fileName)
    {
        var baseName = fileName.Split('.')[0];
        return baseName.Equals("CON", StringComparison.OrdinalIgnoreCase) ||
               baseName.Equals("PRN", StringComparison.OrdinalIgnoreCase) ||
               baseName.Equals("AUX", StringComparison.OrdinalIgnoreCase) ||
               baseName.Equals("NUL", StringComparison.OrdinalIgnoreCase) ||
               (baseName.Length == 4 &&
                (baseName.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ||
                 baseName.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) &&
                baseName[3] is >= '1' and <= '9');
    }

    private void DeleteScanPage(ScanPageViewModel page)
    {
        if (IsSavingPdf)
        {
            return;
        }

        var index = ScanPages.IndexOf(page);
        if (index < 0)
        {
            return;
        }

        try
        {
            File.Delete(page.FilePath);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            SetStatusMessage(
                $"Không xóa được {page.Name}: {exception.Message}",
                StatusMessageType.Error);
            return;
        }

        var wasSelected = ReferenceEquals(SelectedPage, page);
        var selectedFilePath = wasSelected ? null : SelectedPage?.FilePath;
        ScanPages.RemoveAt(index);
        RefreshPageNames();

        if (ScanPages.Count == 0)
        {
            SelectedPage = null;
        }
        else if (wasSelected)
        {
            SelectedPage = ScanPages[Math.Min(index, ScanPages.Count - 1)];
        }
        else
        {
            SelectedPage = ScanPages.FirstOrDefault(item =>
                string.Equals(item.FilePath, selectedFilePath, StringComparison.Ordinal));
        }

        OnPropertyChanged(nameof(ScanPageCount));
        SavePdfCommand.RaiseCanExecuteChanged();
        SetStatusMessage($"Đã xóa {page.Name} khỏi danh sách.", StatusMessageType.Success);
    }

    private void RefreshPageNames()
    {
        for (var index = 0; index < ScanPages.Count; index++)
        {
            var page = ScanPages[index];
            var name = $"Trang {index + 1}";
            if (!string.Equals(page.Name, name, StringComparison.Ordinal))
            {
                ScanPages[index] = page with { Name = name };
            }
        }
    }

    private async Task ScanPageAsync()
    {
        var scanner = SelectedScanner
            ?? throw new InvalidOperationException("Select a scanner before scanning a page.");

        IsScanning = true;
        SetStatusMessage($"Đang quét bằng {scanner.DisplayName}...", StatusMessageType.Warning);
        string? scannedImagePath = null;
        try
        {
            scannedImagePath = await _scannerService.ScanToImageAsync(
                scanner.DeviceId,
                _scanOutputDirectory);

            var page = AddScannedPage(scannedImagePath);
            SelectedPage = page;
            SetStatusMessage($"Đã quét xong {page.Name}.", StatusMessageType.Success);
        }
        catch (Exception exception)
        {
            if (scannedImagePath is not null)
            {
                try
                {
                    File.Delete(scannedImagePath);
                }
                catch (Exception cleanupException) when (
                    cleanupException is IOException or UnauthorizedAccessException)
                {
                    SetStatusMessage(
                        $"Quét được ảnh nhưng không tải được bản xem trước. Không xóa được ảnh tạm tại “{scannedImagePath}”. {exception.Message} {cleanupException.Message}",
                        StatusMessageType.Error);
                    return;
                }
            }

            SetStatusMessage($"Không quét được trang: {exception.Message}", StatusMessageType.Error);
        }
        finally
        {
            IsScanning = false;
        }
    }

    private ScanPageViewModel AddScannedPage(string imagePath)
    {
        var preview = new BitmapImage();
        preview.BeginInit();
        preview.CacheOption = BitmapCacheOption.OnLoad;
        preview.UriSource = new Uri(Path.GetFullPath(imagePath));
        preview.EndInit();
        preview.Freeze();

        var page = new ScanPageViewModel($"Trang {ScanPages.Count + 1}", imagePath, preview);
        ScanPages.Add(page);
        OnPropertyChanged(nameof(ScanPageCount));
        SavePdfCommand.RaiseCanExecuteChanged();
        SelectedPage = page;
        return page;
    }

    private void OnBatchScanButtonClicked()
    {
        if (IsBatchScanning)
        {
            if (IsBatchPaused)
            {
                _batchContinueSignal.Set();
                IsBatchPaused = false;
                SetStatusMessage("Đang tiếp tục quét hàng loạt...", StatusMessageType.Warning);
            }
            else
            {
                _batchContinueSignal.Reset();
                IsBatchPaused = true;
                SetStatusMessage(
                    "Sẽ tạm dừng sau khi quét xong trang hiện tại.",
                    StatusMessageType.Warning);
            }

            return;
        }

        if (CanBatchScan)
        {
            _ = ScanBatchAsync();
        }
    }

    private async Task ScanBatchAsync()
    {
        var scanner = SelectedScanner;
        if (scanner is null)
        {
            return;
        }

        IsBatchScanning = true;
        IsBatchPaused = false;
        _batchContinueSignal.Set();
        IsScanning = true;
        var scannedCount = 0;
        try
        {
            SetStatusMessage(
                $"Đang kiểm tra khay ADF của {scanner.DisplayName}...",
                StatusMessageType.Warning);
            var supportsFeeder =
                await _scannerService.SupportsAutomaticDocumentFeederAsync(scanner.DeviceId);
            if (!supportsFeeder)
            {
                _hasAutomaticFeeder = false;
                OnPropertyChanged(nameof(CanBatchScan));
                BatchScanCommand.RaiseCanExecuteChanged();
                SetStatusMessage(
                    "Máy scan hiện tại không hỗ trợ khay nạp tài liệu tự động (ADF).",
                    StatusMessageType.Warning);
                return;
            }

            _hasAutomaticFeeder = true;
            OnPropertyChanged(nameof(CanBatchScan));
            BatchScanCommand.RaiseCanExecuteChanged();
            SetStatusMessage(
                $"Đang quét hàng loạt bằng {scanner.DisplayName}...",
                StatusMessageType.Warning);

            var pageCount = await _scannerService.ScanBatchToImagesAsync(
                scanner.DeviceId,
                _scanOutputDirectory,
                _batchContinueSignal,
                imagePath => Application.Current.Dispatcher.Invoke(() =>
                {
                    var page = AddScannedPage(imagePath);
                    scannedCount++;
                    SetStatusMessage(
                        $"Đã quét và nạp {page.Name} vào danh sách ({scannedCount} trang).",
                        StatusMessageType.Success);
                }));

            SetStatusMessage(
                pageCount == 0
                    ? "Khay ADF không có giấy để quét."
                    : $"Đã quét xong {pageCount} trang từ khay ADF.",
                pageCount == 0 ? StatusMessageType.Warning : StatusMessageType.Success);
        }
        catch (Exception exception)
        {
            SetStatusMessage(
                scannedCount == 0
                    ? $"Không quét được hàng loạt: {exception.Message}"
                    : $"Đã nạp {scannedCount} trang trước khi xảy ra lỗi: {exception.Message}",
                StatusMessageType.Error);
        }
        finally
        {
            _batchContinueSignal.Set();
            IsBatchPaused = false;
            IsBatchScanning = false;
            IsScanning = false;
        }
    }

    private async Task RefreshAutomaticFeederSupportAsync(string? deviceId)
    {
        _hasAutomaticFeeder = false;
        _isCheckingAutomaticFeeder = !string.IsNullOrWhiteSpace(deviceId);
        OnPropertyChanged(nameof(CanBatchScan));
        OnPropertyChanged(nameof(BatchScanAvailabilityMessage));
        BatchScanCommand.RaiseCanExecuteChanged();
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return;
        }

        try
        {
            var supportsFeeder =
                await _scannerService.SupportsAutomaticDocumentFeederAsync(deviceId);
            if (string.Equals(
                    SelectedScanner?.DeviceId,
                    deviceId,
                    StringComparison.OrdinalIgnoreCase))
            {
                _hasAutomaticFeeder = supportsFeeder;
            }
        }
        catch (Exception exception)
        {
            if (string.Equals(
                    SelectedScanner?.DeviceId,
                    deviceId,
                    StringComparison.OrdinalIgnoreCase))
            {
                SetStatusMessage(
                    $"Không kiểm tra được khả năng nạp giấy ADF của máy scan: {exception.Message}",
                    StatusMessageType.Error);
            }
        }
        finally
        {
            if (string.Equals(
                    SelectedScanner?.DeviceId,
                    deviceId,
                    StringComparison.OrdinalIgnoreCase))
            {
                _isCheckingAutomaticFeeder = false;
                OnPropertyChanged(nameof(CanBatchScan));
                OnPropertyChanged(nameof(BatchScanAvailabilityMessage));
                BatchScanCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public async Task SaveProfileAsync(ScannerProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (SelectedScanner is null)
        {
            throw new InvalidOperationException("Select a scanner before saving its profile.");
        }

        if (!_configCanBeSaved)
        {
            throw new InvalidOperationException(
                "The scanner configuration could not be loaded. Fix or remove scanner-config.json before saving changes.");
        }

        if (string.IsNullOrWhiteSpace(profile.Alias))
        {
            throw new ArgumentException("Enter a name for the scanner profile.", nameof(profile));
        }

        var savedProfile = new ScannerProfile
        {
            DeviceId = SelectedScanner.DeviceId,
            Alias = profile.Alias.Trim(),
            ColorMode = profile.ColorMode,
            ResolutionDpi = profile.ResolutionDpi,
            Source = profile.Source
        };

        await UpdateConfigAsync(config =>
        {
            var existing = config.Profiles.FirstOrDefault(item =>
                string.Equals(item.DeviceId, savedProfile.DeviceId, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                config.Profiles.Add(savedProfile);
            }
            else
            {
                existing.Alias = savedProfile.Alias;
                existing.ColorMode = savedProfile.ColorMode;
                existing.ResolutionDpi = savedProfile.ResolutionDpi;
                existing.Source = savedProfile.Source;
            }

            return config;
        });
        RebuildScannerOptions(SelectedScanner.DeviceId);
        OnPropertyChanged(nameof(ConfiguredScanners));
        SetStatusMessage($"Đã lưu bí danh “{savedProfile.Alias}”.", StatusMessageType.Success);
    }

    public async Task DeleteSelectedProfileAsync()
    {
        var profile = SelectedProfile;
        if (profile is null)
        {
            return;
        }

        await UpdateConfigAsync(config =>
        {
            config.Profiles.RemoveAll(item =>
                string.Equals(item.DeviceId, profile.DeviceId, StringComparison.OrdinalIgnoreCase));
            return config;
        });
        RebuildScannerOptions(SelectedScanner?.DeviceId);
        OnPropertyChanged(nameof(ConfiguredScanners));
        SetStatusMessage(
            "Đã xóa bí danh. Thiết bị vẫn được cài trong Windows.",
            StatusMessageType.Success);
    }

    private async Task LoadScannersAsync()
    {
        _isLoadingScanners = true;
        var requestedDeviceId = SelectedScannerId;
        try
        {
            if (!_hasLoadedConfig)
            {
                try
                {
                    _config = await _configStore.LoadAsync();
                    _hasLoadedConfig = true;
                }
                catch (Exception exception) when (
                    exception is InvalidDataException or IOException or UnauthorizedAccessException)
                {
                    _config = new ScannerConfig();
                    _configCanBeSaved = false;
                    _hasLoadedConfig = true;
                    SetStatusMessage(
                        $"Không đọc được cấu hình máy scan. Danh sách vẫn được tải; cấu hình không được ghi đè. {exception.Message}",
                        StatusMessageType.Error);
                }
            }

            var devices = await _scannerService.GetAvailableScannersAsync();
            var defaultScannerId = _applicationSettings.DefaultScannerDeviceId;
            var defaultScannerAvailable = !string.IsNullOrWhiteSpace(defaultScannerId) &&
                devices.Any(device => string.Equals(
                    device.Id,
                    defaultScannerId,
                    StringComparison.OrdinalIgnoreCase));
            requestedDeviceId ??= defaultScannerAvailable
                ? defaultScannerId
                : _config.LastUsedDeviceId;
            RebuildScannerOptions(requestedDeviceId, devices);

            if (devices.Count == 0)
            {
                SetStatusMessage(
                    "Không tìm thấy máy scan WIA. Kết nối thiết bị hoặc cài driver, sau đó đóng và mở lại ứng dụng.",
                    StatusMessageType.Warning);
            }
            else if (!string.IsNullOrWhiteSpace(requestedDeviceId) && SelectedScanner is null)
            {
                SetStatusMessage(
                    "Máy scan đã dùng gần nhất không còn kết nối. Hãy chọn thiết bị khác.",
                    StatusMessageType.Warning);
            }
            else if (!string.IsNullOrWhiteSpace(defaultScannerId) && !defaultScannerAvailable)
            {
                SetStatusMessage(
                    "Máy scan mặc định không khả dụng. Đã khôi phục máy scan gần nhất; kiểm tra kết nối hoặc chọn lại trong Cài đặt.",
                    StatusMessageType.Warning);
            }
            else if (StatusMessage.StartsWith("Không đọc được cấu hình", StringComparison.Ordinal))
            {
                return;
            }
            else
            {
                SetStatusMessage(
                    SelectedScanner is null
                        ? $"Tìm thấy {devices.Count} máy scan. Hãy chọn một thiết bị."
                        : $"Đã chọn {SelectedScanner.DisplayName}.",
                    SelectedScanner is null ? StatusMessageType.Warning : StatusMessageType.Success);
            }
        }
        catch (Exception exception)
        {
            RebuildScannerOptions(null, []);
            SetStatusMessage(DescribeScannerError(exception), StatusMessageType.Error);
        }
        finally
        {
            _isLoadingScanners = false;
            _ = RefreshAutomaticFeederSupportAsync(SelectedScanner?.DeviceId);
        }
    }

    private void RebuildScannerOptions(
        string? preferredDeviceId,
        IReadOnlyList<ScannerDevice>? devices = null)
    {
        devices ??= Scanners
            .Where(option => !option.IsAddNew)
            .Select(option => new ScannerDevice(option.DeviceId, option.DeviceName))
            .ToList();

        var duplicateNames = devices
            .GroupBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var options = devices.Select(device =>
        {
            var profile = _config.Profiles.FirstOrDefault(item =>
                string.Equals(item.DeviceId, device.Id, StringComparison.OrdinalIgnoreCase));
            var baseName = string.IsNullOrWhiteSpace(profile?.Alias)
                ? device.Name
                : $"{profile.Alias} ({device.Name})";
            var displayName = duplicateNames.Contains(device.Name)
                ? $"{baseName} · {GetDeviceIdSuffix(device.Id)}"
                : baseName;
            return new ScannerOption(device.Id, device.Name, displayName, profile?.Alias);
        }).ToList();
        options.Add(new ScannerOption(
            AddNewScannerDeviceId,
            string.Empty,
            "＋  Thêm máy mới",
            null,
            IsAddNew: true));

        var restoredScanner = options.FirstOrDefault(option =>
            !string.IsNullOrWhiteSpace(preferredDeviceId)
            && string.Equals(
                option.DeviceId,
                preferredDeviceId,
                StringComparison.OrdinalIgnoreCase));

        var wasLoadingScanners = _isLoadingScanners;
        _isLoadingScanners = true;
        try
        {
            Scanners.Clear();
            foreach (var option in options)
            {
                Scanners.Add(option);
            }

            OnPropertyChanged(nameof(AvailableScannerCount));
            OnPropertyChanged(nameof(ConfiguredScanners));
            SelectedScanner = restoredScanner;
            OnPropertyChanged(nameof(SelectedScannerId));
        }
        finally
        {
            _isLoadingScanners = wasLoadingScanners;
        }
    }

    private void ApplyProfileSettings(ScannerOption? scanner)
    {
        var profile = scanner is null
            ? null
            : _config.Profiles.FirstOrDefault(item =>
                string.Equals(item.DeviceId, scanner.DeviceId, StringComparison.OrdinalIgnoreCase));

        SelectedColorMode = profile?.ColorMode ?? "Color";
        SelectedResolutionDpi = profile?.ResolutionDpi ?? 300;
        SelectedSource = profile?.Source ?? "Flatbed";
    }

    private async Task SaveLastUsedScannerAsync(string deviceId)
    {
        if (!_configCanBeSaved)
        {
            return;
        }

        try
        {
            await UpdateConfigAsync(config =>
            {
                config.LastUsedDeviceId = deviceId;
                return config;
            });
        }
        catch (Exception exception)
        {
            SetStatusMessage(
                $"Không lưu được máy scan đã chọn: {exception.Message}",
                StatusMessageType.Error);
        }
    }

    private async Task UpdateConfigAsync(Func<ScannerConfig, ScannerConfig> update)
    {
        await _configLock.WaitAsync();
        try
        {
            var updatedConfig = CopyConfig(_config);
            updatedConfig = update(updatedConfig);
            await _configStore.SaveAsync(updatedConfig);
            _config = updatedConfig;
            OnPropertyChanged(nameof(SelectedProfile));
            OnPropertyChanged(nameof(ProfileButtonText));
            DeleteProfileCommand.RaiseCanExecuteChanged();
        }
        finally
        {
            _configLock.Release();
        }
    }

    private static ScannerConfig CopyConfig(ScannerConfig config) =>
        new()
        {
            Version = config.Version,
            LastUsedDeviceId = config.LastUsedDeviceId,
            Profiles = config.Profiles.Select(profile => new ScannerProfile
            {
                DeviceId = profile.DeviceId,
                Alias = profile.Alias,
                ColorMode = profile.ColorMode,
                ResolutionDpi = profile.ResolutionDpi,
                Source = profile.Source
            }).ToList()
        };

    private void OpenWindowsScannerSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo("ms-settings:printers")
            {
                UseShellExecute = true
            });
            SetStatusMessage(
                "Cài đặt Máy in & máy quét đã mở. Sau khi thêm thiết bị, hãy mở lại ứng dụng để nạp danh sách mới.",
                StatusMessageType.Success);
        }
        catch (Exception exception)
        {
            SetStatusMessage(
                $"Không mở được cài đặt Windows. Hãy mở Settings > Bluetooth & devices > Printers & scanners. {exception.Message}",
                StatusMessageType.Error);
        }
    }

    private ScannerProfile? CreateProfileCopy(ScannerProfile? profile) =>
        profile is null
            ? SelectedScanner is null
                ? null
                : new ScannerProfile
                {
                    DeviceId = SelectedScanner.DeviceId,
                    Alias = SelectedScanner.DeviceName,
                    ColorMode = SelectedColorMode,
                    ResolutionDpi = SelectedResolutionDpi,
                    Source = SelectedSource
                }
            : new ScannerProfile
            {
                DeviceId = profile.DeviceId,
                Alias = profile.Alias,
                ColorMode = SelectedColorMode,
                ResolutionDpi = SelectedResolutionDpi,
                Source = SelectedSource
            };

    private static string GetDeviceIdSuffix(string deviceId) =>
        deviceId.Length > 8 ? deviceId[^8..] : deviceId;

    private void SetStatusMessage(string message, StatusMessageType severity)
    {
        StatusMessage = message;
        if (_statusMessageType == severity)
        {
            return;
        }

        _statusMessageType = severity;
        OnPropertyChanged(nameof(StatusSeverity));
    }

    private static string DescribeScannerError(Exception exception)
    {
        if (exception is COMException comException)
        {
            return $"Lỗi COM/driver máy scan: {comException.Message} (0x{comException.HResult:X8}). Kiểm tra thiết bị và driver WIA, rồi đóng và mở lại ứng dụng.";
        }

        if (exception is PlatformNotSupportedException)
        {
            return exception.Message;
        }

        return $"Không thể tải danh sách máy scan: {exception.Message}";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }
}
