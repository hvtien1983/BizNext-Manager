using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;
using BizNext.Contracts.Models;
using BizNext.DesignSystem.Utils;
using BizNext.Infrastructure.Settings;
using BizNext.Infrastructure.Storage;
using BizNext.Modules.QuickScan.Models;
using BizNext.Modules.QuickScan.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BizNext.Shell;

public partial class App : Application
{
    private const string PersonalizationRegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private ServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var useFakeScanner = string.Equals(
            Environment.GetEnvironmentVariable("QUICKSCAN_USE_FAKE_SCANNER"),
            "1",
            StringComparison.Ordinal);
        var settingsPath = Path.Combine(
            localAppData,
            "QuickScanManager",
            useFakeScanner ? "app-settings.fake.json" : "app-settings.json");

        ApplicationSettings settings;
        string? settingsLoadError = null;
        try
        {
            settings = await new JsonApplicationSettingsStore(settingsPath).LoadAsync();
        }
        catch (Exception exception) when (
            exception is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            settings = new ApplicationSettings();
            settingsLoadError =
                $"Không đọc được cài đặt ứng dụng. Đang dùng giá trị mặc định; hãy kiểm tra app-settings.json. {exception.Message}";
        }

        var shellSettingsStore = new ShellSettingsStore(
            Path.Combine(localAppData, "BizNext-Manager", "shell-settings.json"),
            new JsonFileStorage());
        var shellSettings = new ShellSettings();
        string? shellSettingsLoadError = null;
        try
        {
            shellSettings = await shellSettingsStore.LoadAsync();
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidDataException or IOException or UnauthorizedAccessException)
        {
            shellSettingsLoadError =
                $"Không đọc được cài đặt BizNext Manager. Dashboard sẽ dùng cài đặt mặc định. {exception.Message}";
        }

        // The Shell owns theming; adopt a theme saved by an earlier QuickScan-only version once.
        if (shellSettings.Theme is null)
        {
            shellSettings.Theme = settings.Theme ?? ThemePreference.Light;
            if (shellSettingsLoadError is null)
            {
                try
                {
                    await shellSettingsStore.SaveAsync(shellSettings);
                }
                catch (Exception exception) when (
                    exception is IOException or UnauthorizedAccessException)
                {
                    Trace.TraceWarning($"Could not persist the migrated theme preference. {exception}");
                }
            }
        }

        ApplyTheme(shellSettings.Theme.Value);

        _services = ShellComposition.Create(
            settings, settingsLoadError, shellSettingsStore, shellSettings, shellSettingsLoadError);
        var mainWindow = _services.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }

    public static void ApplyTheme(ThemePreference preference)
    {
        var useDarkTheme = preference switch
        {
            ThemePreference.Light => false,
            ThemePreference.Dark => true,
            ThemePreference.System => IsSystemUsingDarkTheme(),
            _ => throw new ArgumentOutOfRangeException(nameof(preference), preference, null)
        };

        ThemeManager.SetTheme(useDarkTheme ? AppTheme.Dark : AppTheme.Light);
    }

    private static bool IsSystemUsingDarkTheme()
    {
        try
        {
            using var personalizationKey =
                Registry.CurrentUser.OpenSubKey(PersonalizationRegistryPath, writable: false);
            if (personalizationKey?.GetValue("AppsUseLightTheme") is int appsUseLightTheme)
            {
                return appsUseLightTheme == 0;
            }

            Trace.TraceWarning(
                "The Windows AppsUseLightTheme preference is unavailable. Using the light theme.");
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or SecurityException)
        {
            Trace.TraceError(
                $"Could not read the Windows app theme preference. Using the light theme. {exception}");
        }

        return false;
    }
}
