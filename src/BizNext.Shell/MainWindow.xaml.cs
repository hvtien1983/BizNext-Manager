using System.IO;
using System.Linq;
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
    private bool _suppressSelection;

    public MainWindow(
        ModuleCatalog moduleCatalog,
        ModuleNavigationService navigation,
        ShellSettingsStore settingsStore,
        ShellSettings settings,
        string? settingsLoadError)
    {
        ArgumentNullException.ThrowIfNull(moduleCatalog);
        ArgumentNullException.ThrowIfNull(navigation);
        ArgumentNullException.ThrowIfNull(settingsStore);
        ArgumentNullException.ThrowIfNull(settings);
        InitializeComponent();

        _moduleCatalog = moduleCatalog;
        _navigation = navigation;
        _navigation.SelectedModuleChanged += ShowModule;
        _settingsStore = settingsStore;
        _settings = settings;
        _settingsLoadError = settingsLoadError;

        // Data-driven navigation: sidebar và Dashboard sinh từ catalog.
        SidebarModuleList.ItemsSource = _moduleCatalog.Modules;
        DashboardCards.ItemsSource = _moduleCatalog.Modules;

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
        _suppressSelection = true;
        DashboardNavigation.IsChecked = moduleId is null;
        SidebarModuleList.SelectedItem = moduleId is null
            ? null
            : _moduleCatalog.Modules.FirstOrDefault(module =>
                string.Equals(module.Metadata.Id, moduleId, StringComparison.OrdinalIgnoreCase));
        _suppressSelection = false;
    }

    private void SidebarModuleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelection)
        {
            return;
        }

        if (SidebarModuleList.SelectedItem is IBizNextModule module)
        {
            NavigateToModule(module.Metadata.Id);
            _settings.LastSelectedModuleId = module.Metadata.Id;
            _ = SaveSettingsAsync();
        }
    }

    private void ModuleCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IBizNextModule module })
        {
            NavigateToModule(module.Metadata.Id);
            _settings.LastSelectedModuleId = module.Metadata.Id;
            _ = SaveSettingsAsync();
        }
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
