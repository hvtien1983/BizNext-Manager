using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BizNext.Contracts.Models;
using BizNext.DesignSystem.Utils;
using BizNext.Modules.Notes;
using BizNext.Modules.WeatherCalendar;
using BizNext.Shell;

namespace BizNext.TestHost;

internal sealed record PreviewComponent(
    string Group, string Name, string Description, Func<Window, FrameworkElement> CreatePreview)
{
    public string DisplayName => $"{Group} — {Name}";

    public override string ToString() => DisplayName;
}

internal static class ComponentCatalog
{
    public static IReadOnlyList<PreviewComponent> Create() =>
    [
        new("Module", "Ghi chú",
            "View module Ghi chú hiện tại; chưa có dữ liệu hoặc nghiệp vụ.",
            _ => new NotesModule().CreateView()),
        new("Module", "Lịch & Thời tiết",
            "View module Lịch & Thời tiết hiện tại; chưa có dữ liệu hoặc nghiệp vụ.",
            _ => new WeatherCalendarModule().CreateView()),
        new("Shell", "Cài đặt Dashboard",
            "Mở hộp thoại cài đặt thật, không đọc hoặc lưu cấu hình ứng dụng.",
            owner => CreateSettingsPreview(owner))
    ];

    private static FrameworkElement CreateSettingsPreview(Window owner)
    {
        var button = new Button
        {
            Content = "Mở cài đặt Dashboard",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Thickness(12, 6, 12, 6)
        };
        button.Click += (_, _) =>
        {
            var theme = ThemeManager.CurrentTheme == AppTheme.Dark
                ? ThemePreference.Dark
                : ThemePreference.Light;
            try
            {
                var dialog = new DashboardSettingsWindow(theme) { Owner = owner };
                dialog.ShowDialog();
            }
            catch (XamlParseException exception)
            {
                MessageBox.Show(owner, $"Không thể mở cài đặt Dashboard: {exception.GetBaseException().Message}",
                    "Lỗi tải giao diện", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };
        return button;
    }
}
