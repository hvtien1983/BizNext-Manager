using System.Windows.Media.Imaging;

namespace BizNext.Modules.QuickScan.ViewModels;

public sealed record ScanPageViewModel(
    string Name,
    string FilePath,
    BitmapSource Preview);
