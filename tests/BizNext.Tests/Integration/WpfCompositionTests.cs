using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BizNext.Contracts;
using BizNext.DesignSystem.Utils;
using BizNext.Modules.Notes;
using BizNext.Modules.QuickScan;
using BizNext.Modules.QuickScan.Models;
using BizNext.Modules.WeatherCalendar;

namespace BizNext.Tests.Integration;

public sealed class WpfCompositionTests
{
    [Fact]
    public void RealModuleViewsUseSharedResourcesAndThemesOnStaThread()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                app.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/BizNext.DesignSystem;component/Themes/DesignTokens.xaml")
                });
                app.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/BizNext.DesignSystem;component/Themes/App.xaml")
                });
                app.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/BizNext.Shell;component/Themes/ShellSpecific.xaml")
                });
                app.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/BizNext.Shell;component/Themes/ControlStyles.xaml")
                });

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
                var quickScan = Assert.IsType<QuickScanView>(modules[0].CreateView());
                Assert.NotNull(quickScan.DataContext);
                Assert.IsType<TabControl>(quickScan.FindName("MainTabControl"));
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
