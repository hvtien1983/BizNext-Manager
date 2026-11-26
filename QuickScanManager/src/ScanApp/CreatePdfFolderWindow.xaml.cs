using System.Windows;

namespace ScanApp;

public partial class CreatePdfFolderWindow : Window
{
    public CreatePdfFolderWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => FolderNameTextBox.Focus();
    }

    public string FolderName { get; private set; } = string.Empty;

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        FolderName = FolderNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(FolderName))
        {
            FolderNameTextBox.Focus();
            return;
        }

        DialogResult = true;
    }
}
