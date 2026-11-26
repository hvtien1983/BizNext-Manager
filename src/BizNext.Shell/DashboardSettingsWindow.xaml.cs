using System.Windows;
using System.Windows.Controls;
using BizNext.Contracts.Models;

namespace BizNext.Shell;

public partial class DashboardSettingsWindow : Window
{
    public DashboardSettingsWindow(ThemePreference theme)
    {
        InitializeComponent();

        LightThemeOption.IsChecked = theme == ThemePreference.Light;
        DarkThemeOption.IsChecked = theme == ThemePreference.Dark;
        SystemThemeOption.IsChecked = theme == ThemePreference.System;
    }

    public ThemePreference? SelectedTheme { get; private set; }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        var selectedOption = new[] { LightThemeOption, DarkThemeOption, SystemThemeOption }
            .FirstOrDefault(option => option.IsChecked == true);
        if (selectedOption?.Tag is not ThemePreference theme)
        {
            MessageBox.Show(
                this,
                "Chọn một chủ đề giao diện trước khi lưu.",
                "Chưa chọn chủ đề",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        SelectedTheme = theme;
        DialogResult = true;
    }
}
