using System.Windows;

namespace BizNext.DesignSystem.Utils;

/// <summary>
/// Manages application theme switching (Light/Dark mode).
/// </summary>
public static class ThemeManager
{
    private const string DarkThemeUri = "pack://application:,,,/BizNext.DesignSystem;component/Themes/DarkTheme.xaml";
    private const string LightThemeUri = "pack://application:,,,/BizNext.DesignSystem;component/Themes/LightTheme.xaml";

    /// <summary>
    /// Gets or sets the current application theme.
    /// </summary>
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    /// <summary>
    /// Applies the specified theme to the application.
    /// </summary>
    public static void SetTheme(AppTheme theme)
    {
        if (CurrentTheme == theme)
            return;

        try
        {
            // Remove existing dark/light theme override if present
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            var existingTheme = mergedDictionaries
                .FirstOrDefault(d =>
                    (d.Source?.OriginalString?.Contains("DarkTheme") ?? false) ||
                    (d.Source?.OriginalString?.Contains("LightTheme") ?? false));

            if (existingTheme != null)
                mergedDictionaries.Remove(existingTheme);

            // Apply new theme
            if (theme == AppTheme.Dark)
            {
                var darkTheme = new ResourceDictionary { Source = new Uri(DarkThemeUri) };
                mergedDictionaries.Add(darkTheme);
            }
            else if (theme == AppTheme.Light)
            {
                var lightTheme = new ResourceDictionary { Source = new Uri(LightThemeUri) };
                mergedDictionaries.Add(lightTheme);
            }

            CurrentTheme = theme;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggles between light and dark themes.
    /// </summary>
    public static void ToggleTheme()
    {
        SetTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
    }
}

/// <summary>
/// Enum for available application themes.
/// </summary>
public enum AppTheme
{
    Light,
    Dark
}
