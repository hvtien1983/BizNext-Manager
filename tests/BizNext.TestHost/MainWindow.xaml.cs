using System.Windows;
using System.Windows.Controls;
using BizNext.DesignSystem.Utils;

namespace BizNext.TestHost;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ComponentList.ItemsSource = ComponentCatalog.Create();
        ComponentList.SelectedIndex = 0;
    }

    private void ComponentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComponentList.SelectedItem is not PreviewComponent component)
        {
            return;
        }

        ComponentTitle.Text = component.Name;
        ComponentDescription.Text = component.Description;
        PreviewContent.Content = component.CreatePreview(this);
    }

    private void LightTheme_Click(object sender, RoutedEventArgs e) => ApplyTheme(AppTheme.Light);

    private void DarkTheme_Click(object sender, RoutedEventArgs e) => ApplyTheme(AppTheme.Dark);

    private void ApplyTheme(AppTheme theme)
    {
        ThemeManager.SetTheme(theme);
        if (ThemeManager.CurrentTheme != theme)
        {
            MessageBox.Show(this, "Không thể đổi giao diện. Kiểm tra đầu ra gỡ lỗi.",
                "Lỗi giao diện", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
