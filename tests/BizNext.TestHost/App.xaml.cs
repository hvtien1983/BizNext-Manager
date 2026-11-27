using System.Windows;

namespace BizNext.TestHost;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/BizNext.DesignSystem;component/Themes/DesignTokens.xaml")
        });
        Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/BizNext.DesignSystem;component/Themes/App.xaml")
        });
        Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/BizNext.Shell;component/Themes/ShellSpecific.xaml")
        });
        Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/BizNext.Shell;component/Themes/ControlStyles.xaml")
        });
        base.OnStartup(e);
    }
}
