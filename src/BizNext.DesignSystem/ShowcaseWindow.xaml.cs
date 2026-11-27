using System.Windows;

namespace BizNext.DesignSystem
{
    /// <summary>
    /// ShowcaseWindow: Interactive demo of all 14 Fluent 2 components.
    /// Tests: state transitions (hover, focus, disabled), Light/Dark/High Contrast themes,
    /// keyboard navigation, and accessibility.
    /// </summary>
    public partial class ShowcaseWindow : Window
    {
        public ShowcaseWindow()
        {
            InitializeComponent();
            LightThemeButton.Click += (s, e) => SetThemeMode(ThemeMode.Light);
            DarkThemeButton.Click += (s, e) => SetThemeMode(ThemeMode.Dark);
            HighContrastButton.Click += (s, e) => SetHighContrast();
        }

        private void SetThemeMode(ThemeMode mode)
        {
            // Note: ThemeMode in code is experimental API (WPF0001 warning expected).
            // For production, prefer XAML-only or external theme preference service.
            #pragma warning disable WPF0001
            (Application.Current as Application)?.Resources.SetValue(FrameworkElement.ThemeModeProperty, mode);
            #pragma warning restore WPF0001
        }

        private void SetHighContrast()
        {
            // High Contrast uses Windows OS SystemColors settings.
            // This button is a placeholder; true effect requires Windows OS setting change.
            MessageBox.Show(
                "High Contrast mode is controlled by Windows OS Settings:\n" +
                "Settings → Ease of Access → Display → High Contrast\n\n" +
                "When enabled, SystemColors override app DynamicResource brushes automatically.",
                "High Contrast Mode");
        }
    }
}
