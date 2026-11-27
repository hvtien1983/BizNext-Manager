using System.Collections;
using System.Runtime.ExceptionServices;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BizNext.Contracts;
using BizNext.Contracts.Models;
using BizNext.DesignSystem.Utils;
using BizNext.Infrastructure.Settings;
using BizNext.Infrastructure.Storage;
using BizNext.Modules.Notes;
using BizNext.Modules.QuickScan;
using BizNext.Modules.QuickScan.Models;
using BizNext.Modules.WeatherCalendar;
using BizNext.Tests.TestSupport;

namespace BizNext.Tests.Integration;

public sealed class WpfCompositionTests
{
    [Fact]
    public void ShellAssemblyContainsWindowIconResource()
    {
        using var resources = typeof(BizNext.Shell.MainWindow).Assembly
            .GetManifestResourceStream("BizNext.Shell.g.resources");
        Assert.NotNull(resources);
        using var reader = new ResourceReader(resources);
        var icon = reader.Cast<DictionaryEntry>()
            .Single(entry => Equals(entry.Key, "assets/icon.ico"));
        using var stream = Assert.IsAssignableFrom<Stream>(icon.Value);
        Assert.Equal(0, stream.ReadByte());
        Assert.Equal(0, stream.ReadByte());
        Assert.Equal(1, stream.ReadByte());
        Assert.Equal(0, stream.ReadByte());
    }

    [Fact]
    public void RealModuleViewsUseSharedResourcesAndThemesOnStaThread()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                var app = new BizNext.Shell.App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                app.InitializeComponent();

                var light = ((SolidColorBrush)app.FindResource("SurfaceCanvasBrush")).Color;
                ThemeManager.SetTheme(AppTheme.Dark);
                var dark = ((SolidColorBrush)app.FindResource("SurfaceCanvasBrush")).Color;
                Assert.NotEqual(light, dark);
                ThemeManager.SetTheme(AppTheme.Light);
                Assert.Equal(light, ((SolidColorBrush)app.FindResource("SurfaceCanvasBrush")).Color);

                IBizNextModule[] modules =
                [
                    new QuickScanModule(new ApplicationSettings(), null),
                    new NotesModule(),
                    new WeatherCalendarModule()
                ];
                using var temp = new TemporaryDirectory();
                var store = new ShellSettingsStore(Path.Combine(temp.Path, "shell.json"), new JsonFileStorage());
                var shell = new BizNext.Shell.MainWindow(
                    modules, store,
                    new ShellSettings { LastSelectedModuleId = "biznext.module.quickscan" }, null);
                var settingsDialog = new BizNext.Shell.DashboardSettingsWindow(ThemePreference.Light);
                Assert.NotNull(shell.Icon);
                Assert.NotNull(settingsDialog.Icon);
                shell.Measure(new Size(1360, 820));
                shell.Arrange(new Rect(0, 0, 1360, 820));
                shell.UpdateLayout();
                shell.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                var quickScan = Assert.IsType<QuickScanView>(
                    Assert.IsType<ContentControl>(shell.FindName("ContentHost")).Content);
                Assert.NotNull(quickScan.DataContext);
                Assert.IsType<TabControl>(quickScan.FindName("MainTabControl"));
                shell.Measure(new Size(1200, 720));
                shell.Arrange(new Rect(0, 0, 1200, 720));
                shell.UpdateLayout();
                quickScan.Measure(new Size(964, 720));
                quickScan.Arrange(new Rect(0, 0, 964, 720));
                quickScan.UpdateLayout();
                ThemeManager.SetTheme(AppTheme.Dark);
                quickScan.Measure(new Size(1124, 820));
                quickScan.Arrange(new Rect(0, 0, 1124, 820));
                quickScan.UpdateLayout();
                ThemeManager.SetTheme(AppTheme.Light);
                settingsDialog.Close();
                shell.Close();
                Assert.Equal("NotesModuleView", Assert.IsAssignableFrom<UserControl>(modules[1].CreateView()).GetType().Name);
                Assert.Equal("WeatherCalendarModuleView", Assert.IsAssignableFrom<UserControl>(modules[2].CreateView()).GetType().Name);
                Assert.Equal("biznext.module.quickscan", modules[0].Metadata.Id);
                var host = new BizNext.TestHost.MainWindow();
                var root = Assert.IsAssignableFrom<UIElement>(host.Content);
                root.Measure(new Size(1000, 650));
                root.Arrange(new Rect(0, 0, 1000, 650));
                root.UpdateLayout();
                var preview = Assert.IsType<ContentControl>(host.FindName("PreviewContent"));
                var notesView = Assert.IsAssignableFrom<UserControl>(preview.Content);
                var initialBackground = ((SolidColorBrush)notesView.Background).Color;
                ThemeManager.SetTheme(AppTheme.Dark);
                Assert.NotEqual(initialBackground, ((SolidColorBrush)notesView.Background).Color);
                ThemeManager.SetTheme(AppTheme.Light);
                var list = Assert.IsType<ListBox>(host.FindName("ComponentList"));
                Assert.Equal(3, list.Items.Count);
                list.SelectedIndex = 1;
                Assert.Equal("Lịch & Thời tiết", Assert.IsType<TextBlock>(host.FindName("ComponentTitle")).Text);
                root.Measure(new Size(1000, 650));
                root.Arrange(new Rect(0, 0, 1000, 650));
                root.UpdateLayout();
                host.Close();
                app.Shutdown();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "WPF test did not finish in time.");
        if (error is not null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
