using System.Windows;
using System.Windows.Controls;
using ScanDevice.Models;

namespace BizNext.Modules.QuickScan;

public partial class ScannerProfileWindow : Window
{
    private readonly string _deviceId;

    public ScannerProfileWindow(
        string deviceId,
        string deviceName,
        ScannerProfile? existingProfile)
    {
        InitializeComponent();
        _deviceId = deviceId;
        DeviceNameText.Text = $"Thiết bị: {deviceName}";

        if (existingProfile is null)
        {
            AliasTextBox.Text = deviceName;
            SelectByTag(ColorModeComboBox, "Color");
            SelectByTag(ResolutionComboBox, "300");
            SelectByTag(SourceComboBox, "Flatbed");
            return;
        }

        AliasTextBox.Text = existingProfile.Alias;
        SelectByTag(ColorModeComboBox, existingProfile.ColorMode);
        SelectByTag(ResolutionComboBox, existingProfile.ResolutionDpi.ToString());
        SelectByTag(SourceComboBox, existingProfile.Source);
    }

    public ScannerProfile CreateProfile()
    {
        var colorMode = GetSelectedTag(ColorModeComboBox);
        var resolution = GetSelectedTag(ResolutionComboBox);
        var source = GetSelectedTag(SourceComboBox);
        if (!int.TryParse(resolution, out var resolutionDpi))
        {
            throw new InvalidOperationException("The selected scan resolution is invalid.");
        }

        return new ScannerProfile
        {
            DeviceId = _deviceId,
            Alias = AliasTextBox.Text.Trim(),
            ColorMode = colorMode,
            ResolutionDpi = resolutionDpi,
            Source = source
        };
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AliasTextBox.Text))
        {
            MessageBox.Show(
                this,
                "Nhập tên gợi nhớ cho máy scan.",
                "Thiếu tên bí danh",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            AliasTextBox.Focus();
            return;
        }

        DialogResult = true;
    }

    private static void SelectByTag(ComboBox comboBox, string tag)
    {
        comboBox.SelectedItem = comboBox.Items
            .OfType<ComboBoxItem>()
            .FirstOrDefault(item => string.Equals(
                item.Tag?.ToString(),
                tag,
                StringComparison.OrdinalIgnoreCase))
            ?? comboBox.Items[0];
    }

    private static string GetSelectedTag(ComboBox comboBox) =>
        (comboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString()
        ?? throw new InvalidOperationException("Select a value for every scanner profile setting.");
}
