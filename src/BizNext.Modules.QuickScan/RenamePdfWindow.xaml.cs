using System.IO;
using System.Windows;

namespace BizNext.Modules.QuickScan;

public partial class RenamePdfWindow : Window
{
    public RenamePdfWindow(string currentName)
    {
        InitializeComponent();
        FileNameTextBox.Text = Path.GetFileNameWithoutExtension(currentName);
        Loaded += (_, _) =>
        {
            FileNameTextBox.Focus();
            FileNameTextBox.SelectAll();
        };
    }

    public string NewName { get; private set; } = string.Empty;

    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        var newName = FileNameTextBox.Text.Trim();
        if (newName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            newName = Path.GetFileNameWithoutExtension(newName);
        }

        if (string.IsNullOrWhiteSpace(newName) ||
            newName is "." or ".." ||
            newName.EndsWith('.') ||
            newName.EndsWith(' ') ||
            newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            MessageBox.Show(
                this,
                "Nhập tên file hợp lệ, không chứa ký tự cấm.",
                "Tên file không hợp lệ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            FileNameTextBox.Focus();
            FileNameTextBox.SelectAll();
            return;
        }

        NewName = newName;
        DialogResult = true;
    }
}
