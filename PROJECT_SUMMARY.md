# BizNext Manager — Project Summary

> **Kiểm kê mã nguồn ngày 2026-11-28 10:09.** Đây là mô tả implementation đang có
> trong repo, không phải thiết kế đích. Các mục “planned/deferred” được tách
> khỏi phần đã triển khai. Bỏ qua `bin/`, `obj/` và file build sinh tự động.

## 1. Tổng quan

BizNext Manager là ứng dụng Windows WPF, các project giao diện nhắm
`net10.0-windows`; `ScanCore` nhắm `net10.0`. `BizNext.Shell` là executable
host Dashboard và các module. Hiện QuickScan là module có nghiệp vụ hoàn chỉnh; Notes đã có giai đoạn 1 (ghi chú, notebook, tìm kiếm, ghim, tự lưu; xem mục 6A); Weather/Calendar mới là module placeholder. Chưa có widget, tài khoản,
đồng bộ cloud, plugin loader hay Generic Host.

Kiến trúc hiện tại là modular host với composition root dùng
`Microsoft.Extensions.DependencyInjection`. Đây chưa phải kiến trúc ba tầng
mới hoàn chỉnh: QuickScan vẫn có một số phụ thuộc trực tiếp tới đường dẫn,
WPF và local storage. Lộ trình phương án B, quyết định và phần việc chưa được
duyệt nằm trong [PROJECT_TAICAUTRUC.md](./PROJECT_TAICAUTRUC.md).

## 2. Cây project và file nguồn

### Root và tài liệu

```text
BizNext-Manager.sln
README.md
PROJECT_CONTEXT.md
PROJECT_SUMMARY.md
PROJECT_TAICAUTRUC.md
docs/
├── architecture.md
├── baseline.md
└── smoke-test.md
```

### `src/`

```text
src/
├── BizNext.Contracts/
│   ├── IBizNextModule.cs
│   └── Models/
│       ├── BizNextModuleMetadata.cs
│       └── ThemePreference.cs
├── BizNext.DesignSystem/
│   ├── Components/SvgIcon.cs
│   ├── Converters/
│   │   ├── ComboBoxSelectedItemTextConverter.cs
│   │   └── EnumToStringConverter.cs
│   ├── Utils/ThemeManager.cs
│   └── Themes/
│       ├── App.xaml
│       ├── DesignTokens.xaml
│       ├── LightTheme.xaml
│       ├── DarkTheme.xaml
│       └── Shared/
│           ├── ButtonStyles.xaml
│           ├── ButtonVariantStyles.xaml
│           ├── CardStyles.xaml
│           ├── CheckBoxRadioStyles.xaml
│           ├── DataGridStyles.xaml
│           ├── DialogStyles.xaml
│           ├── InputStyles.xaml
│           ├── NavigationMenuStyles.xaml
│           ├── PasswordBoxStyles.xaml
│           ├── SelectionStyles.xaml
│           ├── StatusIndicatorStyles.xaml
│           ├── TabControlStyles.xaml
│           ├── ToggleSwitchStyles.xaml
│           ├── TooltipProgressStyles.xaml
│           └── VisibilityStyles.xaml
├── BizNext.Infrastructure/
│   ├── Modules/ModuleCatalog.cs
│   ├── Modules/ModuleNavigationService.cs
│   ├── Settings/ShellSettings.cs
│   ├── Settings/ShellSettingsStore.cs
│   └── Storage/JsonFileStorage.cs
├── BizNext.Modules.Notes/
│   ├── NotesModule.cs
│   ├── NotesView.xaml(.cs)
│   ├── Commands/
│   │   └── RelayCommand.cs
│   ├── Models/
│   │   ├── Note.cs
│   │   ├── Notebook.cs
│   │   └── NotesData.cs
│   ├── Services/
│   │   ├── INoteRepository.cs
│   │   └── JsonNoteRepository.cs
│   └── ViewModels/
│       ├── ObservableObject.cs
│       ├── NoteViewModel.cs
│       ├── NotebookItemViewModel.cs
│       └── NotesViewModel.cs
├── BizNext.Modules.QuickScan/
│   ├── AppSettingsWindow.xaml(.cs)
│   ├── CreatePdfFolderWindow.xaml(.cs)
│   ├── PdfViewerWindow.xaml(.cs)
│   ├── QuickScanModule.cs
│   ├── QuickScanView.xaml(.cs)
│   ├── RenamePdfWindow.xaml(.cs)
│   ├── ScannerProfileWindow.xaml(.cs)
│   ├── AssemblyInfo.cs
│   ├── Commands/
│   │   ├── AsyncRelayCommand.cs
│   │   ├── RelayCommand.cs
│   │   └── RelayCommandOfT.cs
│   ├── Models/
│   │   ├── ApplicationSettings.cs
│   │   ├── PdfLibraryEntry.cs
│   │   ├── PdfLibraryFolder.cs
│   │   └── PdfLibraryLoadResult.cs
│   ├── Services/
│   │   ├── JsonApplicationSettingsStore.cs
│   │   ├── PdfLibraryBrowserService.cs
│   │   └── ScannedPdfExportService.cs
│   ├── ViewModels/
│   │   ├── ScannerOption.cs
│   │   ├── ScannerSettingsViewModel.cs
│   │   ├── ScanPageViewModel.cs
│   │   └── StatusMessageType.cs
│   ├── Assets/
│   │   ├── app-icon.ico
│   │   ├── icon.ico
│   │   ├── cartoon-file-scan.svg
│   │   ├── files.svg
│   │   ├── geometric-reading-scan.svg
│   │   ├── geometric-settings.svg
│   │   ├── image-up.svg
│   │   └── sequential-scan.svg
│   └── storage/library.json
├── BizNext.Modules.WeatherCalendar/
│   └── WeatherCalendarModule.cs
├── BizNext.Shell/
│   ├── App.xaml(.cs)
│   ├── MainWindow.xaml(.cs)
│   ├── DashboardSettingsWindow.xaml(.cs)
│   ├── ShellComposition.cs
│   └── Themes/
│       ├── ControlStyles.xaml
│       ├── DarkTheme.xaml
│       ├── DesignTokens.xaml
│       └── ShellSpecific.xaml
├── ScanCore/
│   ├── Models/DocumentItem.cs
│   └── Services/
│       ├── JsonLibraryService.cs
│       └── PdfService.cs
└── ScanDevice/
    ├── Models/
    │   ├── ScannerConfig.cs
    │   ├── ScannerDevice.cs
    │   └── ScannerProfile.cs
    └── Services/
        ├── FakeScannerService.cs
        ├── IScannerConfigStore.cs
        ├── IScannerService.cs
        ├── JsonScannerConfigStore.cs
        └── WiaScannerService.cs
```

### `tests/`

```text
tests/
├── BizNext.Tests/
│   ├── Infrastructure/JsonFileStorageTests.cs
│   ├── Integration/WpfCompositionTests.cs
│   ├── Modules/QuickScanTests.cs
│   ├── Shell/ModuleNavigationTests.cs
│   └── TestSupport/
│       ├── TemporaryDirectory.cs
│       └── TestModule.cs
└── BizNext.TestHost/
    ├── App.xaml(.cs)
    ├── MainWindow.xaml(.cs)
    ├── ComponentCatalog.cs
    └── AssemblyInfo.cs
```

Tại thời điểm kiểm kê, dưới `src/` và `tests/` có 11 project `.csproj`, 61 file
C#, 34 file XAML, 6 SVG, 2 ICO, 1 PNG và 1 JSON theo inventory file. Đây là
thống kê cấu trúc, không bao gồm output build và không phải API cố định.

11 project files: `src/BizNext.Contracts/BizNext.Contracts.csproj`,
`src/BizNext.DesignSystem/BizNext.DesignSystem.csproj`,
`src/BizNext.Infrastructure/BizNext.Infrastructure.csproj`,
`src/BizNext.Modules.Notes/BizNext.Modules.Notes.csproj`,
`src/BizNext.Modules.QuickScan/BizNext.Modules.QuickScan.csproj`,
`src/BizNext.Modules.WeatherCalendar/BizNext.Modules.WeatherCalendar.csproj`,
`src/BizNext.Shell/BizNext.Shell.csproj`, `src/ScanCore/ScanCore.csproj`,
`src/ScanDevice/ScanDevice.csproj`,
`tests/BizNext.TestHost/BizNext.TestHost.csproj` và
`tests/BizNext.Tests/BizNext.Tests.csproj`.

## 3. Project references, framework và packages

| Project | Target | Vai trò / dependency đáng chú ý |
|---|---|---|
| `BizNext.Contracts` | `net10.0-windows` | Module contract và metadata |
| `BizNext.DesignSystem` | `net10.0-windows` | Resource dictionaries, style, component WPF |
| `BizNext.Infrastructure` | `net10.0-windows` | Tham chiếu Contracts; catalog, settings, JSON storage |
| `BizNext.Modules.QuickScan` | `net10.0-windows` | Tham chiếu Contracts, DesignSystem, Infrastructure, ScanCore, ScanDevice; `PDFtoImage` 5.4.0 |
| `BizNext.Modules.Notes`           | `net10.0-windows`         | Tham chiếu Contracts, DesignSystem và Infrastructure (dùng `JsonFileStorage`)                |
| `BizNext.Modules.WeatherCalendar` | `net10.0-windows` | Tham chiếu Contracts và DesignSystem |
| `BizNext.Shell` | `net10.0-windows`, WinExe | Tham chiếu UI/modules và Infrastructure; `Microsoft.Extensions.DependencyInjection` 10.0.0 |
| `ScanCore` | `net10.0` | PDF và metadata tài liệu |
| `ScanDevice` | `net10.0-windows` | Thiết bị WIA/fake; tham chiếu ScanCore |
| `BizNext.Tests` | `net10.0-windows` | xUnit 2.9.3, VSTest SDK 17.14.1, xUnit VS runner 3.1.4 |
| `BizNext.TestHost` | `net10.0-windows`, WinExe | Ứng dụng preview độc lập cho một số view/resources |

`BizNext.Shell` đóng vai trò composition root và đăng ký module tĩnh. Không có
assembly scanning hoặc plugin discovery.

## 4. Module contract, catalog, điều hướng và DI

### Contracts

- [`IBizNextModule`](./src/BizNext.Contracts/IBizNextModule.cs): `Metadata` và
  `FrameworkElement CreateView()`.
- [`BizNextModuleMetadata`](./src/BizNext.Contracts/Models/BizNextModuleMetadata.cs):
  record gồm `Id`, `Name`, `Description`, `Category`.
- [`ThemePreference`](./src/BizNext.Contracts/Models/ThemePreference.cs):
  `Light`, `Dark`, `System`.

### Infrastructure

- [`ModuleCatalog`](./src/BizNext.Infrastructure/Modules/ModuleCatalog.cs)
  nhận `IEnumerable<IBizNextModule>` trong constructor, xác thực module,
  từ chối ID rỗng/trùng (không phân biệt hoa thường), giữ thứ tự đăng ký.
  API: `Modules`, `TryGetModule(string, out ...)`,
  `GetRequiredModule(string)`. Không có `Register`.
- [`ModuleNavigationService`](./src/BizNext.Infrastructure/Modules/ModuleNavigationService.cs)
  có `SelectedModuleId`, event `SelectedModuleChanged` và
  `SelectModule(string?)`. ID không hợp lệ bị từ chối; event chỉ phát khi
  lựa chọn thay đổi.
- [`ShellComposition`](./src/BizNext.Shell/ShellComposition.cs) đăng ký
  `ShellSettingsStore`, `ShellSettings`, QuickScan/Notes/Weather modules,
  catalog, navigation và `MainWindow` singleton; bật `ValidateOnBuild` và
  `ValidateScopes`.
- [`App`](./src/BizNext.Shell/App.xaml.cs) đọc settings khi startup, chọn
  scanner thật/giả theo `QUICKSCAN_USE_FAKE_SCANNER=1`, áp dụng theme, tạo
  ServiceProvider và hiển thị MainWindow; Dispose provider khi thoát.
- [`MainWindow`](./src/BizNext.Shell/MainWindow.xaml.cs) điều hướng đến module,
  khôi phục module cuối, cache view theo ID (không phân biệt hoa thường), giữ
  view cùng trạng thái trong phiên chạy và lưu lựa chọn vào Shell settings.
- Các method chính ở Shell: `App.OnStartup`, `App.OnExit`, `App.ApplyTheme`,
  `App.IsSystemUsingDarkTheme`; `MainWindow.MainWindow_Loaded`,
  `OpenDashboardSettings_Click`, `NavigationButton_Click`, `SaveSettingsAsync`,
  `NavigateToModule`, `ShowModule`, `ShowDashboard`,
  `SetNavigationSelection`, `ShowSettingsSaveError`.

State fields chính của Shell: `App._services`; `MainWindow._moduleCatalog`,
`_navigation`, `_settingsStore`, `_settings`, `_settingsLoadError`,
`_viewCache`. MainWindow subscribe `SelectedModuleChanged` và lưu các view đã
tạo trong dictionary theo module ID.

Chưa dùng Generic Host, `IAppPaths`, module manifest, lifecycle interface mới
hoặc data-driven dashboard.

## 5. Settings và lưu trữ dữ liệu

### API và định dạng

- [`JsonFileStorage`](./src/BizNext.Infrastructure/Storage/JsonFileStorage.cs)
  cung cấp `ReadAsync<T>(path, defaultValue, options, cancellationToken)` và
  `WriteAtomicallyAsync<T>(path, value, options, cancellationToken)`. File
  không tồn tại trả default; JSON rỗng/null bị báo `InvalidDataException`;
  ghi dùng file tạm cùng thư mục rồi thay thế file đích, và dọn file tạm.
- [`ShellSettings`](./src/BizNext.Infrastructure/Settings/ShellSettings.cs):
  `LastSelectedModuleId`, `Theme`.
- [`ShellSettingsStore`](./src/BizNext.Infrastructure/Settings/ShellSettingsStore.cs):
  `LoadAsync`, `SaveAsync`, dùng `JsonFileStorage`.
- [`ApplicationSettings`](./src/BizNext.Modules.QuickScan/Models/ApplicationSettings.cs):
  `DefaultPdfDirectory`, `DefaultScannerDeviceId`,
  `AppendTimestampToPdfName`, `Theme?` cũ được đọc để migrate một lần và
  không được ghi trở lại.
- [`JsonApplicationSettingsStore`](./src/BizNext.Modules.QuickScan/Services/JsonApplicationSettingsStore.cs):
  `LoadAsync`, `SaveAsync`; dùng chung atomic JSON storage và chuyển JSON sai
  thành lỗi dữ liệu rõ ràng.
- [`ScannerConfig`](./src/ScanDevice/Models/ScannerConfig.cs):
  `Version`, `LastUsedDeviceId`, `Profiles`.
  [`ScannerProfile`](./src/ScanDevice/Models/ScannerProfile.cs):
  `DeviceId`, `Alias`, `ColorMode`, `ResolutionDpi`, `Source`.
  `IScannerConfigStore` có `LoadAsync`, `SaveAsync`; JSON store xác thực
  version/profile và ghi qua file tạm.

### Runtime data locations

- Shell: `%LocalAppData%\BizNext-Manager\shell-settings.json`.
- QuickScan app settings: `%LocalAppData%\QuickScanManager\app-settings.json`;
  fake scanner dùng `app-settings.fake.json`.
- Scanner profiles: `%LocalAppData%\QuickScanManager\scanner-config.json`;
  QuickScan fake mode dùng `scanner-config.fake.json`.
- Scan images tạm: `%LocalAppData%\QuickScanManager\scans`.
- Notes: `%LocalAppData%\BizNext-Manager\Notes\notes.json` (một file JSON chứa notebook và ghi chú; ghi nguyên tử qua `JsonFileStorage`, định dạng indent, tên thuộc tính PascalCase). Đường dẫn mặc định do `NotesModule` đặt; `NotesModule` có constructor nhận đường dẫn tùy ý để test hoặc preview dùng thư mục riêng.
- Metadata thư viện: `%LocalAppData%\QuickScanManager\storage\library.json`.
- PDF mặc định trong thư viện: `%LocalAppData%\QuickScanManager\storage\pdfs`;
  người dùng có thể chọn thư mục PDF khác trong QuickScan settings.

File `src/BizNext.Modules.QuickScan/storage/library.json` có trong source tree;
runtime service đang tạo `library.json` dưới LocalAppData như trên. Không coi
file source đó là thư viện dữ liệu người dùng runtime.

Shell hiện có migration một lần: nếu Shell chưa có theme, đọc lựa chọn theme
cũ từ QuickScan settings và lưu vào Shell settings. Không có migration dữ liệu
PDF, scanner profile hay thư viện.

## 6. QuickScan: models, services và luồng nghiệp vụ

### Models và command helpers

- [`DocumentItem`](./src/ScanCore/Models/DocumentItem.cs): `Id`, `Name`,
  `FilePath`, `Category`, `PageCount`, `Tags`, `CreatedAtUtc`.
- [`PdfLibraryEntry`](./src/BizNext.Modules.QuickScan/Models/PdfLibraryEntry.cs):
  biểu diễn file/folder/parent row; gồm `Name`, `IsFolder`,
  `IsParentNavigation`, `FullPath`, `PageCount`, `SizeBytes`, `ScannedAt`,
  `SizeDisplay`, `ScannedAtDisplay`, `ErrorMessage`.
- [`PdfLibraryFolder`](./src/BizNext.Modules.QuickScan/Models/PdfLibraryFolder.cs):
  `Name`, `FullPath`, `IsDefault`.
- [`PdfLibraryLoadResult`](./src/BizNext.Modules.QuickScan/Models/PdfLibraryLoadResult.cs):
  `Entries`, `MetadataWarning`.
- [`ScannerDevice`](./src/ScanDevice/Models/ScannerDevice.cs): `Id`, `Name`.
- [`ScannerOption`](./src/BizNext.Modules.QuickScan/ViewModels/ScannerOption.cs):
  ID/label/alias và `IsAddNew`.
- [`ScanPageViewModel`](./src/BizNext.Modules.QuickScan/ViewModels/ScanPageViewModel.cs):
  `Name`, `FilePath`, bitmap `Preview`.
- [`StatusMessageType`](./src/BizNext.Modules.QuickScan/ViewModels/StatusMessageType.cs):
  `Success`, `Warning`, `Error`.
- [`RelayCommand`](./src/BizNext.Modules.QuickScan/Commands/RelayCommand.cs),
  `RelayCommand<T>` và `AsyncRelayCommand`: ICommand helpers có predicate,
  `CanExecuteChanged`; async command quản lý trạng thái thực thi.

### Services

- [`IScannerService`](./src/ScanDevice/Services/IScannerService.cs):
  `GetAvailableScannersAsync`, `ScanToImageAsync`,
  `SupportsAutomaticDocumentFeederAsync`, `ScanBatchToImagesAsync`.
- [`WiaScannerService`](./src/ScanDevice/Services/WiaScannerService.cs):
  scanner Windows WIA qua COM; thao tác WIA chạy trên STA, hỗ trợ liệt kê
  thiết bị, quét đơn/ADF, và giải phóng COM resources.
- [`FakeScannerService`](./src/ScanDevice/Services/FakeScannerService.cs):
  hai scanner giả, sinh BMP mẫu, mô phỏng ADF ba trang; dùng cho phát triển/
  kiểm thử, không phải bộ cô lập hoàn chỉnh cho dữ liệu QuickScan.
- [`JsonScannerConfigStore`](./src/ScanDevice/Services/JsonScannerConfigStore.cs):
  load/save cấu hình, kiểm tra version 1 và profile, atomic replacement.
- [`PdfService`](./src/ScanCore/Services/PdfService.cs):
  `StoreAsync` kiểm tra chữ ký `%PDF-`, sao chép vào storage với GUID filename,
  trả `DocumentItem`.
- [`JsonLibraryService`](./src/ScanCore/Services/JsonLibraryService.cs):
  `LoadAsync`, `SaveAsync` cho metadata `DocumentItem`; save qua file tạm.
- [`PdfLibraryBrowserService`](./src/BizNext.Modules.QuickScan/Services/PdfLibraryBrowserService.cs):
  `GetFoldersAsync` và `LoadFolderAsync`; duyệt thư mục, ghép PDF thật với
  metadata, đọc page count từ PDF khi metadata thiếu, trả warning thay vì ẩn
  PDF khi metadata lỗi.
- [`ScannedPdfExportService`](./src/BizNext.Modules.QuickScan/Services/ScannedPdfExportService.cs):
  `CreateAsync` ghép bitmap thành PDF; hỗ trợ khổ giấy định trước hoặc kích
  thước theo ảnh, ghi file tạm rồi chuyển sang đích.

### `ScannerSettingsViewModel`

[`ScannerSettingsViewModel`](./src/BizNext.Modules.QuickScan/ViewModels/ScannerSettingsViewModel.cs)
là state owner cho thiết bị, cấu hình scan, trang ảnh, export và profile.
Public state/commands gồm:

- Collections: `Scanners`, `ConfiguredScanners`, `ScanPages`, `PdfLocations`,
  `PaperSizes`.
- Trạng thái dẫn xuất: `PdfStorageDirectory`, `AvailableScannerCount`,
  `ScanPageCount`, `CanBatchScan`, `CanChangeScanConfiguration`,
  `BatchScanButtonText`, `BatchScanAvailabilityMessage`,
  `SelectedProfile`, `ProfileButtonText`, `StatusSeverity`.
- Commands: `ScanPageCommand`, `BatchScanCommand`, `SavePdfCommand`,
  `DeleteScanPageCommand`, xoay trái/phải, lật ngang/dọc,
  `OpenWindowsScannerSettingsCommand`, `AddOrEditProfileCommand`,
  `DeleteProfileCommand`.
- API: `ImportFilesAsync`, `InitializeAsync`,
  `GetApplicationSettingsCopy`, `SaveApplicationSettingsAsync`,
  `CreatePdfLocation`, `MoveScanPage`, `SaveProfileAsync`,
  `DeleteSelectedProfileAsync`; phát `PropertyChanged`,
  `ProfileEditRequested`, `ProfileDeleteRequested`.
- Nhóm thao tác nội bộ: quét đơn/ADF, pause/resume ADF, refresh scanner và
  feeder support, import PDF/ảnh, sắp xếp/xóa trang, transform preview,
  kiểm tra filename, lưu PDF + metadata, cập nhật profile/config, phát thông
  báo trạng thái và diễn giải lỗi.

Các private state fields chính: `_scannerService`, `_configStore`,
`_applicationSettingsStore`, `_pdfExportService`, `_libraryService`,
`_configLock`, `_batchContinueSignal`, `_scanOutputDirectory`,
`_pdfStorageDirectory`, `_applicationSettings`, `_settingsLoadError`, `_config`,
`_selectedScanner`, `_selectedScannerId`, `_statusMessage`,
`_statusMessageType`, `_selectedColorMode`, `_selectedResolutionDpi`,
`_selectedSource`, `_selectedPage`, các cờ `_isScanning`,
`_isBatchScanning`, `_isBatchPaused`, `_isCheckingAutomaticFeeder`,
`_hasAutomaticFeeder`, `_isSavingPdf`, `_isImportingFiles`,
`_isLoadingScanners`, `_hasLoadedConfig`, `_configCanBeSaved`, `_pdfFileName`,
`_pdfLocation`, `_selectedPdfPageSize`. Không liệt kê biến cục bộ từng hàm như
API ổn định.

### QuickScan view và dialogs

- [`QuickScanView`](./src/BizNext.Modules.QuickScan/QuickScanView.xaml):
  hai tab **Quét PDF** và **Thư viện PDF**; khu vực thiết lập scanner và
  scan, import file, preview/reorder/delete/rotate/flip trang, chọn nơi lưu,
  status bar và settings. Thư viện có tìm theo tên, chọn folder, refresh,
  open folder, sort, context menu, empty state và DataGrid.
- [`QuickScanView.xaml.cs`](./src/BizNext.Modules.QuickScan/QuickScanView.xaml.cs):
  khởi tạo settings/config/library services; khởi tạo/refresh library, filter,
  sort, điều hướng folder, mở PDF/location, rename/delete, import file,
  tạo folder, edit/delete scanner profile, drag/drop sắp thứ tự trang.
- Các handler/helper quan trọng của view: `OpenAppSettings_Click`,
  `QuickScanView_Loaded`, `MainTabControl_SelectionChanged`,
  `LibraryFolderComboBox_SelectionChanged`, `RefreshLibrary_Click`,
  `RefreshLibraryAsync`, `CreateLibraryBrowserService`,
  `CreateLibraryMetadataService`, `SearchBox_TextChanged`,
  `ApplyLibraryFilter`, `UpdateLibraryStatus`, `LibraryGrid_Sorting`,
  `CompareLibraryEntries`, `UpdateLibrarySortIndicators`,
  `LibraryGrid_MouseDoubleClick`, `LibraryGridRow_ContextMenuOpening`,
  `LibraryGrid_LoadingRow`, `QuickViewPdf_Click`, `OpenPdfViewer`,
  `OpenPdfLocation_Click`, `RenamePdf_Click`, `DeletePdf_Click`,
  `UpdatePdfMetadataPathAsync`, `PathsEqual`, `MovePdfFile`,
  `IsLibraryFileOperationError`, `ShowLibraryFileOperationError`,
  `OpenLibraryFolder_Click`, `AddFiles_Click`, `CreatePdfFolder_Click`,
  `EditScannerProfile`, `DeleteScannerProfile`,
  `ScanPageList_PreviewMouseLeftButtonDown`,
  `ScanPageList_PreviewMouseMove`, `ScanPageList_DragOver`,
  `ScanPageList_Drop`.
- [`AppSettingsWindow`](./src/BizNext.Modules.QuickScan/AppSettingsWindow.xaml):
  sửa thư mục PDF mặc định, scanner mặc định và tùy chọn timestamp.
- [`CreatePdfFolderWindow`](./src/BizNext.Modules.QuickScan/CreatePdfFolderWindow.xaml):
  nhập tên thư mục PDF.
- [`RenamePdfWindow`](./src/BizNext.Modules.QuickScan/RenamePdfWindow.xaml):
  nhập tên PDF mới.
- [`ScannerProfileWindow`](./src/BizNext.Modules.QuickScan/ScannerProfileWindow.xaml):
  alias scanner, color mode, DPI và source.
- [`PdfViewerWindow`](./src/BizNext.Modules.QuickScan/PdfViewerWindow.xaml):
  xem trang PDF, chuyển trang bằng nút/bàn phím và render theo trang hiện tại.
- Code-behind methods chính: `AppSettingsWindow.BrowsePdfDirectory_Click`,
  `AppSettingsWindow.SaveSettings_Click`, `CreatePdfFolderWindow.Create_Click`,
  `RenamePdfWindow.Rename_Click`, `ScannerProfileWindow.Save_Click`,
  `DashboardSettingsWindow.SaveSettings_Click`,
  `PdfViewerWindow_Loaded`, `PreviousPage_Click`, `NextPage_Click`,
  `PdfViewerWindow_PreviewKeyDown`, `RenderCurrentPageAsync`.

**Đổi tên và xóa PDF đã được triển khai.** Rename chuyển file và cập nhật
metadata; nếu cập nhật metadata lỗi thì thử rollback file rename và báo lỗi.
Delete hỏi xác nhận, loại metadata trước, xóa file, khôi phục metadata nếu
không xóa được file. Thư viện hỗ trợ mở file, xem PDF, folder navigation,
sort/search; không thấy UI/API chỉnh sửa tags/category trong thư viện.

Dialogs lấy owner từ cửa sổ host. Window WPF mới phải dùng application icon
`Assets/icon.ico` và bảo đảm asset có trong project; Shell và QuickScan có
assets tương ứng.

## 6A. Notes: models, services, ViewModel và giao diện

Trạng thái: giai đoạn 1 đã được viết trong code (ghi chú, notebook, tìm kiếm, ghim, xóa mềm, tự lưu). Các mục "planned/deferred" nằm ở cuối mục này.

### Models

- `Note`: `Id` (Guid), `Title`, `Content` (Markdown), `NotebookId` (Guid?), `Tags` (`List<string>`, chưa có UI), `IsPinned`, `CreatedAtUtc`, `UpdatedAtUtc`, `DeletedAtUtc` (Guid? xóa mềm), `IsDeleted` (`[JsonIgnore]`).
- `Notebook`: `Id`, `Name`, `SortOrder`, `DeletedAtUtc` (xóa mềm), `IsDeleted` (`[JsonIgnore]`).
- `NotesData`: `Version` (=1), `Notebooks`, `Notes`; là toàn bộ nội dung `notes.json`.

### Services và lưu trữ

- `INoteRepository`: `LoadAsync`, `SaveAsync` (nạp và lưu toàn bộ `NotesData`).
- `JsonNoteRepository(JsonFileStorage storage, string filePath)`: file chưa có thì trả `NotesData` rỗng; file hỏng thì ném lỗi từ `JsonFileStorage`.
- Chỉ có một implementation JSON. Khi làm đồng bộ cloud (gói Pro) dự kiến bổ sung phương thức theo từng note hoặc đổi sang SQLite; vì vậy mô hình dùng Guid, thời gian UTC và xóa mềm.

### Commands và ViewModel

- `RelayCommand` riêng trong `BizNext.Modules.Notes.Commands` (cùng hành vi với bản của QuickScan, chưa có `AsyncRelayCommand`); Notes không tham chiếu QuickScan. Có thể gom về một nơi dùng chung sau.
- `ObservableObject`: `SetProperty`, `OnPropertyChanged`.
- `NoteViewModel` (bọc `Note`): `Title`, `Content`, `IsPinned`, `NotebookKey` (Guid.Empty = chưa phân loại), `UpdatedAtUtc`, `UpdatedAtDisplay`, `DisplayTitle`, `Preview` (dòng đầu có chữ, tối đa 80 ký tự), sự kiện `Changed`, `NotifyNotebookChanged()`. Mỗi lần sửa cập nhật `UpdatedAtUtc`.
- `NotebookItemViewModel` + `NotebookItemKind` (`All`, `Unassigned`, `Notebook`): một dòng trong danh sách notebook; có `Kind`, `Model`, `IsNotebook`, `Key`, `Name` (ghi ngược vào `Notebook.Name`), `NoteCount`.
- `NotesViewModel`: state `Notes`, `VisibleNotes` (ICollectionView: lọc theo notebook + tìm kiếm trong tiêu đề/nội dung; sắp xếp ghim trước, sửa gần nhất trước), `NotebookFilters` (Tất cả, Chưa phân loại, các notebook), `NotebookOptions` (cho ComboBox của ghi chú), `SelectedNote`, `SelectedNotebookFilter`, `SearchText`, `StatusMessage`, `IsEditingNotebook`, `NotebookEditName`, `NotebookEditLabel`, `IsNotebookSelected`, `HasSelection`.
- Commands: `NewNoteCommand`, `DeleteNoteCommand`, `TogglePinCommand`, `NewNotebookCommand`, `RenameNotebookCommand`, `DeleteNotebookCommand`, `ConfirmNotebookEditCommand`, `CancelNotebookEditCommand`. API: `LoadAsync`, `FlushAsync`.
- Quy tắc nghiệp vụ:
  - Tự lưu sau 1,5 giây kể từ lần sửa cuối (`DispatcherTimer`); ghi qua `SemaphoreSlim` để không ghi chồng.
  - Nếu `LoadAsync` lỗi thì cờ `_isLoaded=false` và không lưu, để không ghi đè dữ liệu cũ bằng dữ liệu rỗng.
  - Xóa ghi chú là xóa mềm (`DeletedAtUtc`), ghi chú biến mất khỏi danh sách nhưng còn trong file.
  - Xóa notebook (phương án A): notebook được xóa mềm, mọi ghi chú thuộc notebook đó (kể cả ghi chú đã ở thùng rác) chuyển về chưa phân loại, không bị xóa.
  - Ghi chú mới kế thừa notebook đang chọn.
  - Tên notebook: không rỗng, không trùng (không phân biệt hoa thường) trong các notebook còn hoạt động; tên được cắt khoảng trắng hai đầu.
  - Khi nạp, ghi chú trỏ tới notebook không tồn tại hoặc đã xóa được đưa về chưa phân loại.
  - Số lượng ghi chú mỗi notebook (`NoteCount`) chỉ đếm ghi chú chưa xóa.

### Giao diện

- `NotesView.xaml` (UserControl) bố cục 3 cột (220 | 300 | còn lại): (1) Notebook: nút `+ Notebook`, ô nhập tên tại chỗ khi tạo mới/đổi tên (Enter = lưu, Esc = hủy), danh sách có số lượng, nút Đổi tên/Xóa; (2) nút `+ Ghi chú mới`, tìm kiếm, danh sách ghi chú (biểu tượng ghim, xem trước, giờ sửa); (3) soạn thảo: nút ghim/xóa, ComboBox chọn notebook, ô tiêu đề, ô nội dung nhiều dòng (Markdown thuần, chưa có xem trước), thanh trạng thái.
- `NotesView.xaml.cs`: nạp dữ liệu một lần ở lần `Loaded` đầu; `FlushAsync` khi `Unloaded`; xác nhận bằng `MessageBox` khi xóa ghi chú và khi xóa notebook (nêu số ghi chú sẽ chuyển về chưa phân loại); đưa con trỏ vào `NotebookEditBox` khi ô nhập hiện ra.
- Style dùng từ DesignSystem: `SectionCard`, `FieldLabel`, `InputTextBox`, `PrimaryButton`, `SecondaryButton`, `Fluent.ListBox`, `Fluent.ListBox.Item`, `Fluent.ComboBox`.
- `NotesModule`: ID `biznext.module.notes` không đổi; có hai constructor (mặc định dùng đường dẫn LocalAppData ở mục 5; một constructor nhận đường dẫn). `CreateView()` tạo `JsonNoteRepository`, `NotesViewModel` và `NotesView`. Dịch vụ Notes chưa đăng ký trong DI của Shell; chúng được tạo bên trong `CreateView()`.

### Planned / deferred (chưa triển khai)

- Thùng rác: xem, khôi phục, xóa hẳn ghi chú và notebook (hiện mới có dữ liệu xóa mềm, chưa có UI).
- Tag (có sẵn trường dữ liệu), xem trước Markdown, checklist, ảnh đính kèm, liên kết giữa các ghi chú.
- Widget Dashboard (ghi chú ghim, ghi nhanh), khóa theo gói, đồng bộ cloud.
- Lưu khi đóng cửa sổ chính hoặc thoát app (hiện chỉ có tự lưu 1,5 giây và lưu khi `Unloaded`; tắt app trong vòng 1,5 giây sau lần gõ cuối có thể mất ký tự cuối).
- Test riêng cho Notes; sắp xếp thứ tự notebook; thông báo trạng thái khi xóa notebook.

### Điểm cần xác minh (xóa mục này sau khi kiểm tra)

- Build Release, `dotnet test` và checklist thử tay Notes (tạo/xóa/đổi tên notebook, đổi notebook của ghi chú, tắt/mở lại app).
- Cách `ShellComposition` khởi tạo `NotesModule` (có hai constructor).
- Hiển thị của `Fluent.ComboBox` với `DisplayMemberPath="Name"`; sau khi đổi tên notebook, ô ComboBox của ghi chú đang mở có thể hiện tên cũ.
- Độ rộng 3 cột trên cửa sổ nhỏ.

## 7. BizNext.DesignSystem và giao diện

### Nguồn resource đang hoạt động

[`BizNext.DesignSystem/Themes/App.xaml`](./src/BizNext.DesignSystem/Themes/App.xaml)
merge token, theme mặc định và các shared style dictionaries.
[`BizNext.Shell/App.xaml`](./src/BizNext.Shell/App.xaml) nạp DesignSystem
`DesignTokens.xaml`, `Themes/App.xaml`, rồi Shell-specific styles.
Shell chọn Light/Dark; `ThemePreference.System` được Shell ánh xạ sang
Light/Dark bằng Windows preference. `ThemeManager` của DesignSystem hiện có
`AppTheme.Light/Dark` và `SetTheme`/`ToggleTheme`.

Shell cũng còn `src/BizNext.Shell/Themes/DesignTokens.xaml` và `DarkTheme.xaml`
với các key trùng DesignSystem, nhưng hai dictionary này **không được merge
bởi App.xaml hiện tại**. Nguồn tokens/styles dùng chung đang hoạt động là
`BizNext.DesignSystem`; Shell giữ một số style bố cục/điều hướng riêng.
Không có resource dictionary High Contrast độc lập được liệt kê trong
application merge hiện tại.

### Shared style keys đã có

- Buttons: `PrimaryButton`, `SecondaryButton`, `StatusBarButton`,
  `Fluent.Button`, `Fluent.Button.Primary`, `.Secondary`, `.Subtle`,
  `.ListItemReveal`.
- Inputs: `InputTextBox`, `Fluent.CheckBox`, `Fluent.RadioButton`,
  `Fluent.ComboBox`, `Fluent.ComboBoxItem`, `Fluent.ComboBoxItem.AddNew`,
  `Fluent.PasswordBox`, `Fluent.ToggleButton`.
- Selection/navigation: `Fluent.ListBox`, `Fluent.ListBox.Item`,
  `Fluent.MenuItem`, `Fluent.NavigationMenu.Item`, `Fluent.Navigation.Tab`.
- Containers/layout: `SectionCard`, `FieldLabel`, `Fluent.TabControl`,
  `Fluent.TabControl.ContentHost`, `Fluent.TabItem`, `Fluent.Dialog`,
  `Fluent.DialogButtonPanel`.
- Data/feedback: `LibraryDataGrid`, `LibraryDataGridRow`, `Fluent.ToolTip`,
  `Fluent.ProgressBar`, `Fluent.StatusIndicator.Success/Warning/Error`,
  `Fluent.StatusText`, `.Success`, `.Warning`, `.Error`, `.Dynamic`.
- Visibility helpers: `Fluent.Visibility.VisibleWhenNull`,
  `.VisibleWhenNotNull`, `.VisibleWhenZero`.
- Shell-only styles: `SidebarNavigationButton`, `SidebarActionButton`,
  `DashboardModuleCard`, `SettingsThemeOption`.

Các resource được định nghĩa trong 15 file `Themes/Shared/`; danh sách chính
xác file nằm trong cây source ở trên. `SvgIcon` là component WPF custom;
converter gồm `ComboBoxSelectedItemTextConverter` và `EnumToStringConverter`.

### Tokens, controls và màn hình

`DesignTokens.xaml` định nghĩa semantic brushes: action, text, surface,
status và border; cùng `Control/Card/Overlay/PillCornerRadius`, compact
heights/paddings/font sizes, spacing 4–24px, motion duration và focus-ring
thickness. Các key cụ thể gồm:

`ActionPrimaryBrush`, `ActionPrimaryHoverBrush`, `ActionPrimarySubtleBrush`,
`ActionDangerBrush`; `TextPrimaryBrush`, `TextSecondaryBrush`,
`TextMutedBrush`, `TextOnPrimaryBrush`, `TextSuccessBrush`,
`TextDangerBrush`, `TextWarningBrush`, `TextPlaceholderBrush`,
`TextIconBrush`, `TextIllustrationBrush`; `SurfaceCanvasBrush`,
`SurfaceDefaultBrush`, `SurfaceSubtleBrush`, `SurfaceSuccessBrush`,
`SurfaceHeaderBrush`, `SurfacePreviewBrush`, `SurfaceHoverBrush`,
`SurfaceSelectedBrush`, `SurfaceEmptyBrush`, `SurfaceWarningBrush`,
`StatusDotBrush`; `BorderDefaultBrush`, `BorderSubtleBrush`,
`BorderHeaderBrush`, `BorderPreviewBrush`, `BorderFocusBrush`,
`BorderWarningBrush`; `ControlCornerRadius`, `CardCornerRadius`,
`OverlayCornerRadius`, `PillCornerRadius`; `CompactControlHeight`,
`CompactSecondaryButtonHeight`, `CompactPrimaryButtonHeight`,
`CompactCardPadding`, `CompactSectionGap`, `CompactFieldGap`,
`CompactLabelFontSize`, `CompactControlFontSize`, `CompactCaptionFontSize`;
`Spacing2XSmall`, `SpacingXSmall`, `SpacingSmall`, `SpacingMedium`,
`SpacingLarge`, `SpacingXLarge`; `MotionDurationEnter`,
`MotionDurationMedium`, `MotionDurationExit`, `FocusRingStrokeThickness`.

Các view dùng `DynamicResource` cho brush theo theme; module compose view và
consume shared components/styles, không tạo thư viện UI riêng. Shell Dashboard
có sidebar điều hướng, header nội dung, Dashboard cards và cài đặt theme.
Dashboard settings có ba lựa chọn Sáng/Tối/Theo Windows. QuickScan có tabs,
DataGrid, scanner controls, page list và dialogs. Weather/Calendar view hiện chỉ là card placeholder; Notes có giao diện 3 cột thật (mục 6A).

Các control names có ý nghĩa khi kiểm thử/automation:

- Shell: `DashboardNavigation`, `QuickScanNavigation`, `NotesNavigation`,
  `WeatherNavigation`, `ShellPageTitle`, `ShellPageSubtitle`, `ContentHost`,
  `DashboardHome`; theme options `LightThemeOption`, `DarkThemeOption`,
  `SystemThemeOption`.
- QuickScan: `MainTabControl`, `ScanTab`, `LibraryTab`, `ScanPageList`,
  `SearchBox`, `LibraryFolderComboBox`, `LibraryRefreshButton`,
  `OpenLibraryFolderButton`, `LibraryGrid`, `LibraryEmptyState`,
  `LibraryStatusText`; PDF viewer `FileNameText`, `PageStatusText`,
  `PreviousPageButton`, `NextPageButton`, `PageScrollViewer`, `PageImage`,
  `LoadingText`.
- QuickScan settings/dialogs: `PdfDirectoryTextBox`,
  `DefaultScannerComboBox`, `ScannerHelpText`, `AppendTimestampCheckBox`,
  `FolderNameTextBox`, `FileNameTextBox`, `DeviceNameText`, `AliasTextBox`,
  `ColorModeComboBox`, `ResolutionComboBox`, `SourceComboBox`.
- Notes: `NotebookEditBox`.
- TestHost: `ComponentList`, `ComponentTitle`, `ComponentDescription`,
  `PreviewContent`.

## 8. Test infrastructure và giới hạn

`BizNext.Tests` dùng xUnit/VSTest, gồm **14 test** theo source hiện tại:

- `JsonFileStorageTests` (4): missing/default + round-trip, JSON corrupt/null,
  atomic overwrite, lỗi serialization giữ file cũ và dọn file tạm.
- `ModuleNavigationTests` (3): ID invalid/duplicate, lookup/order, selection
  event và lựa chọn invalid.
- `QuickScanTests` (5): command predicate/notification, fake scanner ghi trong
  temporary directory, browser ghép file với metadata, metadata corrupt vẫn
  hiển thị file kèm warning, QuickScan settings round-trip giữ thư mục user set.
- `WpfCompositionTests` (2): icon resource của Shell; khởi tạo module views,
  shared resources/theme trên STA thread.; kiểm tra tên kiểu view Notes là NotesView(đã đổi từ khi thay view placeholder).
- Notes chưa có test riêng; số test vẫn là 14.

`TemporaryDirectory` hỗ trợ test storage cô lập. Test scanner dùng
`FakeScannerService`; test không gọi thiết bị WIA thật. WPF QuickScan view test
dừng trước `Loaded`, không chạy scan workflow và không đọc dữ liệu user. Không
khẳng định test suite bao phủ toàn bộ thao tác GUI.

`BizNext.TestHost` là preview app riêng, hiện có danh mục thành phần, theme
Sáng/Tối, view placeholder Notes/Weather và preview Shell settings; không phải
QuickScan GUI test harness. QuickScan preview chưa an toàn vì view dùng các
đường dẫn LocalAppData hiện hành.

Baseline lịch sử ghi nhận 13/13 test ở commit `42860dc`, sau đó 14/14 khi thêm
test overwrite; Release build 0 warning/0 error được ghi nhận trong lượt
1A. Đây là kết quả lần xác minh trước, không đại diện cho lần build/test mới
nhất. Xem [docs/baseline.md](./docs/baseline.md).

Người dùng xác nhận toàn bộ 15 mục smoke checklist đạt ngày 2026-11-27, kể cả
lượt sau 1A. Đây là kết quả người dùng báo trong môi trường thử; không phải
kết quả tự động do tài liệu này xác minh. Xem
[docs/smoke-test.md](./docs/smoke-test.md).

View Notes trong TestHost dùng NotesModule; cần xác nhận TestHost không tạo NotesModule với đường dẫn mặc định (sẽ đọc/ghi notes.json thật) mà dùng thư mục tạm.

**Cảnh báo dữ liệu:** `QUICKSCAN_USE_FAKE_SCANNER=1` chỉ thay scanner và một
số settings/config file. Nó không tự cô lập PDF directory, `storage/library.json`
hoặc mọi dữ liệu QuickScan. Không chạy smoke GUI trên dữ liệu thật, không dùng
scanner thật khi test; chỉ chạy khi xác minh cấu hình và toàn bộ thư mục test
độc lập trước.

## 9. Build và test commands

Trên Windows với .NET SDK và Windows Desktop targeting pack phù hợp:

```powershell
dotnet build .\BizNext-Manager.sln -c Release
dotnet test .\tests\BizNext.Tests\BizNext.Tests.csproj -c Release
dotnet run --project .\src\BizNext.Shell\BizNext.Shell.csproj
dotnet run --project .\tests\BizNext.TestHost\BizNext.TestHost.csproj
```

Test theo namespace:

```powershell
dotnet test .\tests\BizNext.Tests\BizNext.Tests.csproj -c Release --filter "FullyQualifiedName~BizNext.Tests.Modules"
```

Chạy fake scanner không đồng nghĩa đã cô lập dữ liệu:

```powershell
$env:QUICKSCAN_USE_FAKE_SCANNER = "1"
dotnet run --project .\src\BizNext.Shell\BizNext.Shell.csproj
```

Chỉ chạy ứng dụng ở trên trong môi trường có dữ liệu test độc lập và sau khi
xác minh đường dẫn settings/library/PDF.

## 10. Trạng thái công việc và ranh giới

- Working branch được kiểm tra: `refactor/three-tier`; HEAD vẫn là
  `42860dc`. Thay đổi bước 0/1A và các tài liệu liên quan đang ở working tree,
  chưa commit tại thời điểm kiểm kê 2026-11-28.
- Bước 0: baseline test bổ sung, `docs/baseline.md`,
  `docs/smoke-test.md`; người dùng xác nhận checklist đạt.
- Bước 1A: Shell DI composition root; giữ nguyên module contract, đường dẫn
  dữ liệu, điều hướng và view cache. User cũng xác nhận checklist sau 1A đạt.
- Các thay đổi đang có trong working tree phải được giữ nguyên; không checkout,
  reset hoặc ghi đè chúng khi làm việc tiếp.
- Bước tiếp theo trong roadmap không tự động được cấp phép bởi việc cập nhật
  summary. Chỉ thực hiện theo phạm vi đã được người dùng duyệt; xem
  [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md) và
  [PROJECT_TAICAUTRUC.md](./PROJECT_TAICAUTRUC.md).
- Notes giai đoạn 1 (mục 6A) được thêm trong nhánh `feature/notes` (tạo từ `refactor/three-tier`), chưa commit tại thời điểm viết. Ngoài module Notes chỉ sửa một test (`WpfCompositionTests`) và thêm tham chiếu Infrastructure vào `BizNext.Modules.Notes.csproj`; Shell, module contract và đường dẫn dữ liệu của QuickScan giữ nguyên. Phạm vi Notes do người dùng duyệt trực tiếp trong quá trình làm việc, không thuộc các bước phương án B trong `PROJECT_TAICAUTRUC.md`.

## 11. Tài liệu liên quan

- [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md): handoff ngắn, hiện trạng và
  quy ước quan trọng.
- [PROJECT_TAICAUTRUC.md](./PROJECT_TAICAUTRUC.md): phương án B, cấu trúc đích
  và tiến độ theo từng bước.
- [docs/architecture.md](./docs/architecture.md): dependency/lifecycle module
  và quy ước mở rộng.
- [README.md](./README.md): build, chạy, test và giới hạn test host.
- [docs/baseline.md](./docs/baseline.md): baseline tự động và kết quả có nguồn.
- [docs/smoke-test.md](./docs/smoke-test.md): checklist thủ công cùng điều kiện
  an toàn dữ liệu.
