using System.IO;
using System.Windows;
using System.Windows.Controls;
using BizNext.Contracts;
using BizNext.Contracts.Models;
using BizNext.Infrastructure.Modules;
using BizNext.Infrastructure.Settings;

namespace BizNext.Shell;

public partial class MainWindow : Window
{
    private readonly ModuleCatalog _moduleCatalog;
    private readonly ModuleNavigationService _navigation;
    private readonly ShellSettingsStore _settingsStore;
    private readonly ShellSettings _settings;
    private readonly string? _settingsLoadError;
    private readonly Dictionary<string, FrameworkElement> _viewCache =
        new(StringComparer.OrdinalIgnoreCase);
    public MainWindow(
        IEnumerable<IBizNextModule> modules,
        ShellSettingsStore settingsStore,
        ShellSettings settings,
        string? settingsLoadError)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(settingsStore);
        ArgumentNullException.ThrowIfNull(settings);
        InitializeComponent();

        _moduleCatalog = new ModuleCatalog(modules);
        _navigation = new ModuleNavigationService(_moduleCatalog);
        _navigation.SelectedModuleChanged += ShowModule;
        _settingsStore = settingsStore;
        _settings = settings;
        _settingsLoadError = settingsLoadError;

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (_settings.LastSelectedModuleId is { } moduleId &&
            _moduleCatalog.TryGetModule(moduleId, out _))
        {
            NavigateToModule(moduleId);
        }

        if (_settingsLoadError is not null)
        {
            MessageBox.Show(
                this,
                _settingsLoadError,
                "Không đọc được cài đặt",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private async void OpenDashboardSettings_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new DashboardSettingsWindow(_settings.Theme ?? ThemePreference.Light)
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true || dialog.SelectedTheme is not { } theme)
        {
            return;
        }

        var previousTheme = _settings.Theme;
        _settings.Theme = theme;
        try
        {
            await _settingsStore.SaveAsync(_settings);
            App.ApplyTheme(theme);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            _settings.Theme = previousTheme;
            ShowSettingsSaveError(exception);
        }
    }

    private async void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string target })
        {
            return;
        }

        if (string.Equals(target, "dashboard", StringComparison.OrdinalIgnoreCase))
        {
            ShowDashboard();
            _settings.LastSelectedModuleId = null;
        }
        else
        {
            NavigateToModule(target);
            _settings.LastSelectedModuleId = target;
        }

        await SaveSettingsAsync();
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            await _settingsStore.SaveAsync(_settings);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            ShowSettingsSaveError(exception);
        }
    }

    private void NavigateToModule(string moduleId)
    {
        _ = _moduleCatalog.GetRequiredModule(moduleId);
        _navigation.SelectModule(moduleId);
        SetNavigationSelection(moduleId);
    }

    private void ShowModule(string? moduleId)
    {
        if (moduleId is null)
        {
            SetNavigationSelection(null);
            ContentHost.Content = DashboardHome;
            ShellPageTitle.Text = "Dashboard";
            ShellPageSubtitle.Text = "Tập trung các tiện ích bạn cần mỗi ngày.";
            return;
        }

        var module = _moduleCatalog.GetRequiredModule(moduleId);
        if (!_viewCache.TryGetValue(moduleId, out var view))
        {
            view = module.CreateView()
                ?? throw new InvalidOperationException($"Module '{moduleId}' returned no view.");
            _viewCache.Add(moduleId, view);
        }

        ContentHost.Content = view;
        ShellPageTitle.Text = module.Metadata.Name;
        ShellPageSubtitle.Text = module.Metadata.Description;
    }

    private void ShowDashboard()
    {
        _navigation.SelectModule(null);
        if (_navigation.SelectedModuleId is null)
        {
            SetNavigationSelection(null);
            ContentHost.Content = DashboardHome;
            ShellPageTitle.Text = "Dashboard";
            ShellPageSubtitle.Text = "Tập trung các tiện ích bạn cần mỗi ngày.";
        }
    }

    private void SetNavigationSelection(string? moduleId)
    {
        DashboardNavigation.IsChecked = moduleId is null;
        QuickScanNavigation.IsChecked =
            string.Equals(moduleId, "biznext.module.quickscan", StringComparison.OrdinalIgnoreCase);
        NotesNavigation.IsChecked =
            string.Equals(moduleId, "biznext.module.notes", StringComparison.OrdinalIgnoreCase);
        WeatherNavigation.IsChecked =
            string.Equals(moduleId, "biznext.module.weathercalendar", StringComparison.OrdinalIgnoreCase);
    }

    private void ShowSettingsSaveError(Exception exception)
    {
        MessageBox.Show(
            this,
            $"Không lưu được cài đặt BizNext Manager. {exception.Message}",
            "Lỗi lưu cài đặt",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
