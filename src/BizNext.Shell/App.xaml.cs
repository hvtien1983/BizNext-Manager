using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows;
using Microsoft.Win32;
using ScanApp.Models;
using ScanApp.Services;

namespace ScanApp;

public partial class App : Application
{
    private const string PersonalizationRegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

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

        ApplyTheme(settings.Theme);
        var mainWindow = new MainWindow(settings, settingsLoadError);
        MainWindow = mainWindow;
        mainWindow.Show();
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
        var themeDictionary = new ResourceDictionary
        {
            Source = new Uri(
                useDarkTheme
                    ? "/ScanApp;component/Themes/DarkTheme.xaml"
                    : "/ScanApp;component/Themes/DesignTokens.xaml",
                UriKind.Relative)
        };

        Current.Resources.MergedDictionaries[0] = themeDictionary;
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
