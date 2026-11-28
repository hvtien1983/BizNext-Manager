using System.Windows;
using BizNext.Contracts;
using BizNext.Contracts.Models;
using BizNext.Modules.QuickScan.Models;

namespace BizNext.Modules.QuickScan;

public sealed class QuickScanModule(
    ApplicationSettings applicationSettings,
    string? settingsLoadError) : IBizNextModule
{
    public BizNextModuleMetadata Metadata { get; } = new(
        "biznext.module.quickscan",
        "QuickScan",
        "Quét tài liệu, ghép trang và quản lý PDF.",
        "Tài liệu")
    {
        Icon = "▤",
        Order = 1
    };

    public FrameworkElement CreateView() => new QuickScanView(
        applicationSettings,
        settingsLoadError);
}
