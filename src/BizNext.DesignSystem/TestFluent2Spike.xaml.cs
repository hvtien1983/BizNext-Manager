using System.Windows;

namespace BizNext.DesignSystem
{
    /// <summary>
    /// Architecture A Spike: Verify built-in Fluent theme + BizNext token override.
    ///
    /// Test checklist:
    /// 1. Window loads with Fluent theme (Windows 11 look).
    /// 2. ControlCornerRadius visible as 4px (not old 8px).
    /// 3. Brand button color is Windows accent #0078D4.
    /// 4. TextMuted text readable (contrast ≥ 4.5:1).
    /// 5. Light button: set ThemeMode.Light, all colors update.
    /// 6. Dark button: set ThemeMode.Dark, all colors invert, light blue tones apply.
    /// 7. High Contrast: Windows OS setting, SystemColors apply automatically.
    ///
    /// If all 7 pass: Architecture A approved. Proceed to full component library.
    /// If any fail: Document and decide between Option A fallback or Option B (custom theme).
    /// </summary>
    public partial class TestFluent2Spike : Window
    {
        public TestFluent2Spike()
        {
            InitializeComponent();
            LightButton.Click += (s, e) => SetThemeMode(ThemeMode.Light);
            DarkButton.Click += (s, e) => SetThemeMode(ThemeMode.Dark);
        }

        private void SetThemeMode(ThemeMode mode)
        {
            // Note: ThemeMode in code is experimental API (WPF0001 warning expected).
            // For spike only; production would use App.xaml ThemeMode or external state.
            #pragma warning disable WPF0001
            (Application.Current as Application)?.Resources.SetValue(FrameworkElement.ThemeModeProperty, mode);
            #pragma warning restore WPF0001
        }
    }
}
