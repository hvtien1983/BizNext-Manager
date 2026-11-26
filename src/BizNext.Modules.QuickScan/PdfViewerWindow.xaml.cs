using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PDFtoImage;
using PDFtoImage.Exceptions;

namespace BizNext.Modules.QuickScan;

public partial class PdfViewerWindow : Window
{
    private readonly string _filePath;
    private byte[]? _pdfBytes;
    private int _pageCount;
    private int _currentPageIndex;
    private bool _isRendering;

    public PdfViewerWindow(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = Path.GetFullPath(filePath);
        InitializeComponent();
        FileNameText.Text = Path.GetFileName(_filePath);
        Title = $"{Path.GetFileName(_filePath)} - Trình xem PDF";
    }

    private async void PdfViewerWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _pdfBytes = await File.ReadAllBytesAsync(_filePath);
            _pageCount = await Task.Run(() => Conversion.GetPageCount(_pdfBytes, password: null));
            if (_pageCount < 1)
            {
                throw new InvalidDataException("PDF không có trang để hiển thị.");
            }

            await RenderCurrentPageAsync();
        }
        catch (Exception exception) when (IsPdfReadError(exception))
        {
            MessageBox.Show(
                this,
                $"Không thể mở file PDF. {exception.Message}",
                "Lỗi mở PDF",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Close();
        }
    }

    private async void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPageIndex > 0)
        {
            _currentPageIndex--;
            await RenderCurrentPageAsync();
        }
    }

    private async void NextPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPageIndex < _pageCount - 1)
        {
            _currentPageIndex++;
            await RenderCurrentPageAsync();
        }
    }

    private async void PdfViewerWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.PageUp or Key.Left && _currentPageIndex > 0)
        {
            _currentPageIndex--;
            e.Handled = true;
            await RenderCurrentPageAsync();
        }
        else if (e.Key is Key.PageDown or Key.Right && _currentPageIndex < _pageCount - 1)
        {
            _currentPageIndex++;
            e.Handled = true;
            await RenderCurrentPageAsync();
        }
    }

    private async Task RenderCurrentPageAsync()
    {
        if (_isRendering || _pdfBytes is null)
        {
            return;
        }

        _isRendering = true;
        LoadingText.Visibility = Visibility.Visible;
        PreviousPageButton.IsEnabled = false;
        NextPageButton.IsEnabled = false;
        PageStatusText.Text = $"Đang kết xuất trang {_currentPageIndex + 1}...";

        try
        {
            var pageIndex = _currentPageIndex;
            var pdfBytes = _pdfBytes;
            var pngBytes = await Task.Run(() =>
            {
                using var pdfStream = new MemoryStream(pdfBytes, writable: false);
                using var imageStream = new MemoryStream();
                Conversion.SavePng(
                    imageStream,
                    pdfStream,
                    pageIndex,
                    leaveOpen: true,
                    password: null,
                    options: new RenderOptions());
                return imageStream.ToArray();
            });

            var image = new BitmapImage();
            using (var imageStream = new MemoryStream(pngBytes, writable: false))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = imageStream;
                image.EndInit();
                image.Freeze();
            }

            PageImage.Source = image;
            AutomationProperties.SetName(PageImage, $"Trang {_currentPageIndex + 1}");
            PageStatusText.Text = $"Trang {_currentPageIndex + 1} / {_pageCount}";
        }
        catch (Exception exception) when (IsPdfReadError(exception))
        {
            PageStatusText.Text = $"Không thể kết xuất trang: {exception.Message}";
        }
        finally
        {
            _isRendering = false;
            LoadingText.Visibility = Visibility.Collapsed;
            PreviousPageButton.IsEnabled = _currentPageIndex > 0;
            NextPageButton.IsEnabled = _currentPageIndex < _pageCount - 1;
        }
    }

    private static bool IsPdfReadError(Exception exception) =>
        exception is IOException or
            UnauthorizedAccessException or
            SecurityException or
            ArgumentException or
            InvalidDataException or
            PdfException;
}
