using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ScanApp.Commands;
using ScanDevice.Models;
using ScanDevice.Services;

namespace ScanApp.ViewModels;

public sealed class ScannerSettingsViewModel : INotifyPropertyChanged
{
    private readonly IScannerService _scannerService;
    private readonly IScannerConfigStore _configStore;
    private readonly SemaphoreSlim _configLock = new(1, 1);
    private ScannerConfig _config = new();
    private ScannerOption? _selectedScanner;
    private string? _selectedScannerId;
    private string _statusMessage = "Đang tìm máy scan...";
    private string _selectedColorMode = "Color";
    private int _selectedResolutionDpi = 300;
    private string _selectedSource = "Flatbed";
    private bool _isRefreshing;
    private bool _hasLoadedConfig;
    private bool _configCanBeSaved = true;

    public ScannerSettingsViewModel(
        IScannerService scannerService,
        IScannerConfigStore configStore)
    {
        _scannerService = scannerService;
        _configStore = configStore;

        RefreshScannersCommand = new AsyncRelayCommand(
            RefreshScannersAsync,
            () => !IsRefreshing);
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

    public AsyncRelayCommand RefreshScannersCommand { get; }

    public RelayCommand OpenWindowsScannerSettingsCommand { get; }

    public RelayCommand AddOrEditProfileCommand { get; }

    public RelayCommand DeleteProfileCommand { get; }

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
            AddOrEditProfileCommand.RaiseCanExecuteChanged();
            DeleteProfileCommand.RaiseCanExecuteChanged();

            if (value is not null && !_isRefreshing && _configCanBeSaved)
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

    public bool IsRefreshing
    {
        get => _isRefreshing;
        private set
        {
            if (_isRefreshing == value)
            {
                return;
            }

            _isRefreshing = value;
            OnPropertyChanged();
            RefreshScannersCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task InitializeAsync() => await RefreshScannersAsync();

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
        StatusMessage = $"Đã lưu bí danh “{savedProfile.Alias}”.";
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
        StatusMessage = "Đã xóa bí danh. Thiết bị vẫn được cài trong Windows.";
    }

    private async Task RefreshScannersAsync()
    {
        IsRefreshing = true;
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
                    StatusMessage = $"Không đọc được cấu hình máy scan. Danh sách vẫn được tải; cấu hình không được ghi đè. {exception.Message}";
                }
            }

            requestedDeviceId ??= _config.LastUsedDeviceId;
            var devices = await _scannerService.GetAvailableScannersAsync();
            RebuildScannerOptions(requestedDeviceId, devices);

            if (devices.Count == 0)
            {
                StatusMessage = "Không tìm thấy máy scan WIA. Kết nối thiết bị hoặc cài driver, rồi làm mới.";
            }
            else if (!string.IsNullOrWhiteSpace(requestedDeviceId) && SelectedScanner is null)
            {
                StatusMessage = "Máy scan đã dùng gần nhất không còn kết nối. Hãy chọn thiết bị khác.";
            }
            else if (StatusMessage.StartsWith("Không đọc được cấu hình", StringComparison.Ordinal))
            {
                return;
            }
            else
            {
                StatusMessage = SelectedScanner is null
                    ? $"Tìm thấy {devices.Count} máy scan. Hãy chọn một thiết bị."
                    : $"Đã chọn {SelectedScanner.DisplayName}.";
            }
        }
        catch (Exception exception)
        {
            RebuildScannerOptions(null, []);
            StatusMessage = DescribeScannerError(exception);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void RebuildScannerOptions(
        string? preferredDeviceId,
        IReadOnlyList<ScannerDevice>? devices = null)
    {
        devices ??= Scanners
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

        var restoredScanner = options.FirstOrDefault(option =>
            !string.IsNullOrWhiteSpace(preferredDeviceId)
            && string.Equals(
                option.DeviceId,
                preferredDeviceId,
                StringComparison.OrdinalIgnoreCase));

        var wasRefreshing = _isRefreshing;
        _isRefreshing = true;
        try
        {
            Scanners.Clear();
            foreach (var option in options)
            {
                Scanners.Add(option);
            }

            SelectedScanner = restoredScanner;
            OnPropertyChanged(nameof(SelectedScannerId));
        }
        finally
        {
            _isRefreshing = wasRefreshing;
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
            StatusMessage = $"Không lưu được máy scan đã chọn: {exception.Message}";
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
            StatusMessage = "Cài đặt Máy in & máy quét đã mở. Hoàn tất cài đặt Windows, rồi bấm Làm mới.";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Không mở được cài đặt Windows. Hãy mở Settings > Bluetooth & devices > Printers & scanners. {exception.Message}";
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

    private static string DescribeScannerError(Exception exception)
    {
        if (exception is COMException comException)
        {
            return $"Lỗi COM/driver máy scan: {comException.Message} (0x{comException.HResult:X8}). Kiểm tra thiết bị và driver WIA, rồi thử làm mới.";
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
