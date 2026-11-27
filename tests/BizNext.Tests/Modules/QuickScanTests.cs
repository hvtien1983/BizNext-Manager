using System.IO;
using BizNext.Modules.QuickScan.Commands;
using BizNext.Modules.QuickScan.Models;
using BizNext.Modules.QuickScan.Services;
using BizNext.Tests.TestSupport;
using ScanCore.Models;
using ScanCore.Services;
using ScanDevice.Services;

namespace BizNext.Tests.Modules;

public sealed class QuickScanTests
{
    [Fact]
    public void RelayCommandUsesPredicateAndRaisesChangeNotification()
    {
        var enabled = false;
        var calls = 0;
        var notifications = 0;
        var command = new RelayCommand(() => calls++, () => enabled);
        command.CanExecuteChanged += (_, _) => notifications++;

        Assert.False(command.CanExecute(null));
        enabled = true;
        command.RaiseCanExecuteChanged();
        Assert.True(command.CanExecute(null));
        command.Execute(null);

        Assert.Equal(1, calls);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public async Task FakeScannerCreatesBitmapOnlyInTemporaryDirectory()
    {
        using var temp = new TemporaryDirectory();
        var scanner = new FakeScannerService();
        var device = (await scanner.GetAvailableScannersAsync()).First();
        var path = await scanner.ScanToImageAsync(device.Id, temp.Path);

        Assert.Equal(temp.Path, Path.GetDirectoryName(path));
        var bytes = await File.ReadAllBytesAsync(path);
        Assert.True(bytes.Length > 54);
        Assert.Equal((byte)'B', bytes[0]);
        Assert.Equal((byte)'M', bytes[1]);
        await Assert.ThrowsAsync<InvalidOperationException>(() => scanner.ScanToImageAsync("missing", temp.Path));
    }

    [Fact]
    public async Task LibraryBrowserCombinesActualFilesWithMetadata()
    {
        using var temp = new TemporaryDirectory();
        var pdfFolder = Path.Combine(temp.Path, "pdfs");
        Directory.CreateDirectory(Path.Combine(pdfFolder, "subfolder"));
        var pdfPath = Path.Combine(pdfFolder, "sample.pdf");
        await File.WriteAllTextAsync(pdfPath, "placeholder");
        var library = new JsonLibraryService(Path.Combine(temp.Path, "library.json"));
        await library.SaveAsync([new DocumentItem { Name = "sample", FilePath = pdfPath, PageCount = 3 }]);
        var browser = new PdfLibraryBrowserService(library, pdfFolder);

        var folders = await browser.GetFoldersAsync();
        var result = await browser.LoadFolderAsync(folders[0]);

        Assert.True(folders[0].IsDefault);
        Assert.Contains(result.Entries, entry => entry.IsFolder && entry.Name == "subfolder");
        var pdf = Assert.Single(result.Entries, entry => !entry.IsFolder);
        Assert.Equal("sample.pdf", pdf.Name);
        Assert.Equal(3, pdf.PageCount);
        Assert.Null(result.MetadataWarning);
    }

    [Fact]
    public async Task CorruptLibraryMetadataProducesWarningWithoutHidingFiles()
    {
        using var temp = new TemporaryDirectory();
        var pdfFolder = Path.Combine(temp.Path, "pdfs");
        Directory.CreateDirectory(pdfFolder);
        var libraryPath = Path.Combine(temp.Path, "library.json");
        await File.WriteAllTextAsync(libraryPath, "{");
        Directory.CreateDirectory(Path.Combine(pdfFolder, "documents"));
        var browser = new PdfLibraryBrowserService(new JsonLibraryService(libraryPath), pdfFolder);
        var folder = Assert.Single(await browser.GetFoldersAsync(), folder => folder.IsDefault);

        var result = await browser.LoadFolderAsync(folder);

        Assert.NotNull(result.MetadataWarning);
        Assert.Contains(result.Entries, entry => entry.Name == "documents" && entry.IsFolder);
    }

    [Fact]
    public async Task QuickScanSettingsRoundTripKeepsUserSelectedDirectory()
    {
        using var temp = new TemporaryDirectory();
        var store = new JsonApplicationSettingsStore(Path.Combine(temp.Path, "settings.json"));
        var settings = new ApplicationSettings { DefaultPdfDirectory = Path.Combine(temp.Path, "chosen") };

        await store.SaveAsync(settings);

        Assert.Equal(settings.DefaultPdfDirectory, (await store.LoadAsync()).DefaultPdfDirectory);
    }
}
