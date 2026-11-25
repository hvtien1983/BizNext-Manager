using System.Windows.Media.Imaging;

namespace ScanApp.ViewModels;

public sealed record ScanPageViewModel(
    string Name,
    string FilePath,
    BitmapSource Preview);
