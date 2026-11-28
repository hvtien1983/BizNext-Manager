# BizNext Manager — Project Context

> Living handoff: cập nhật tài liệu này khi kiến trúc, hành vi hoặc trạng thái
> dự án thay đổi. Lần đối chiếu mã nguồn gần nhất: **2026-11-28**.

## Project overview

BizNext Manager là ứng dụng Windows WPF trên .NET 10, với Dashboard host các
module tiện ích. `BizNext.Shell` là executable; QuickScan đang có nghiệp vụ
quét/tạo PDF/thư viện. Notes và Weather/Calendar hiện là module placeholder.
`ScanCore` và `ScanDevice` là project hỗ trợ QuickScan, không phải module
Dashboard. Chưa có widget, account, cloud sync, dynamic plugin loader hay
Generic Host.

## Current architecture

- `BizNext.Contracts`: `IBizNextModule`, `BizNextModuleMetadata`,
  `ThemePreference`.
- `BizNext.Infrastructure`: `ModuleCatalog`, `ModuleNavigationService`,
  `ShellSettingsStore`, `JsonFileStorage`.
- `BizNext.DesignSystem`: WPF semantic tokens, Light/Dark themes, shared styles,
  SVG component và converters.
- `BizNext.Modules.QuickScan`: view, viewmodel, scanner/PDF/library behavior.
- `BizNext.Modules.Notes`, `BizNext.Modules.WeatherCalendar`: placeholder views.
- `ScanCore`: `DocumentItem`, JSON library, PDF storage helper.
- `ScanDevice`: scanner contract, WIA/fake implementation, scanner profile store.
- `BizNext.Shell`: startup, DI composition, Dashboard, navigation, theme.

Shell composition hiện dùng `Microsoft.Extensions.DependencyInjection` trong
[`ShellComposition.cs`](./src/BizNext.Shell/ShellComposition.cs). Module được
đăng ký tĩnh. `ModuleCatalog` nhận danh sách module qua constructor và cung cấp
`Modules`, `TryGetModule`, `GetRequiredModule`; navigation state là
`SelectedModuleId` trên `ModuleNavigationService`. MainWindow giữ cache view
để trạng thái module không mất khi điều hướng trong cùng phiên.

Đây chưa phải kiến trúc ba tầng đích: QuickScan vẫn có một số phụ thuộc trực
tiếp tới WPF và các đường dẫn local. Chưa có `IAppPaths`, module manifest hoặc
module lifecycle mới. Xem [`docs/architecture.md`](./docs/architecture.md) và
[`PROJECT_TAICAUTRUC.md`](./PROJECT_TAICAUTRUC.md).

## Module and UI ownership

- BizNext.DesignSystem sở hữu reusable WPF styles, themes, components và
  tokens. Module compose views và consume shared resources; module không tạo
  local UI infrastructure dùng chung.
- Shell sở hữu Dashboard layout, navigation và lựa chọn theme. Theme setting
  Light/Dark/System nằm trong Dashboard settings và được lưu ở Shell settings.
- Module sở hữu view composition, feature behavior và feature-specific assets.
  QuickScan settings nằm trong QuickScan.
- Không lặp lại title Dashboard trong module root view; Shell header hiển thị
  tên/mô tả module.
- Mọi WPF Window mới dùng `Icon="Assets/icon.ico"` và bảo đảm icon được đưa
  vào project. Dialog của module cần set host window làm owner.
- Brush theo theme phải dùng `DynamicResource`. Tham chiếu
  [`DesignSystem`](./src/BizNext.DesignSystem/) trước khi thêm shared style.
- Shell hiện merge `BizNext.DesignSystem/Themes/DesignTokens.xaml`,
  `BizNext.DesignSystem/Themes/App.xaml`, `ShellSpecific.xaml` và
  `ControlStyles.xaml`. Hai file Shell
  `Themes/DesignTokens.xaml`, `Themes/DarkTheme.xaml` còn tồn tại nhưng không
  được merge bởi `App.xaml`; DesignSystem là nguồn token/theme dùng chung.
- Có Light/Dark theme; System được Shell ánh xạ sang Light/Dark theo Windows
  preference. Không có dictionary High Contrast độc lập được merge hiện tại.

## QuickScan behavior and data safety

QuickScan hiện hỗ trợ WIA scanner/fake scanner; quét trang đơn và ADF batch;
import ảnh/PDF; reorder, delete, rotate và flip trang; export PDF với khổ giấy,
đường dẫn và timestamp; thư viện PDF có folder navigation, search, sort, open,
rename, delete và PDF viewer. Rename/delete đã có xử lý đồng bộ metadata và
rollback best-effort khi có lỗi. Không thấy chức năng chỉnh sửa tags/category
trong thư viện.

Local paths hiện tại:

- Shell settings: `%LocalAppData%\BizNext-Manager\shell-settings.json`.
- QuickScan settings: `%LocalAppData%\QuickScanManager\app-settings.json`;
  fake mode dùng `app-settings.fake.json`.
- Scanner profiles: `%LocalAppData%\QuickScanManager\scanner-config.json`;
  fake mode dùng `scanner-config.fake.json`.
- Scan images tạm: `%LocalAppData%\QuickScanManager\scans`.
- Library metadata/PDF mặc định:
  `%LocalAppData%\QuickScanManager\storage\library.json` và thư mục `pdfs`.
- PDF có thể được cấu hình lưu ở thư mục khác.

`QUICKSCAN_USE_FAKE_SCANNER=1` chỉ tách scanner và một số settings/config;
không tự cô lập library, PDF directory hay toàn bộ QuickScan data. Không test
bằng scanner thật hoặc dữ liệu QuickScan thật; chỉ chạy GUI test sau khi xác
minh settings, library, PDF và Shell paths đều thuộc môi trường độc lập.
Không tự di chuyển, xóa hoặc sửa dữ liệu người dùng để thử.

Shell có migration theme một lần từ `ApplicationSettings.Theme` cũ sang
`ShellSettings.Theme` khi Shell chưa có theme. Các file PDF, thư viện và
scanner profiles không được migrate/xóa bởi thay đổi này.

## Design system reference

Các style XAML được merge trong DesignSystem bao gồm buttons, input controls,
cards, DataGrid, dialog, navigation, PasswordBox, ComboBox/ListBox/MenuItem,
status/tooltip/progress, tabs, visibility helpers. `SvgIcon` là component WPF;
converter hiện có là `ComboBoxSelectedItemTextConverter` và
`EnumToStringConverter`.

Các key phổ biến: `Fluent.Button.Primary`, `Fluent.Button.Secondary`,
`Fluent.Button.Subtle`, `Fluent.CheckBox`, `Fluent.RadioButton`,
`Fluent.ComboBox`, `Fluent.ToggleButton`, `InputTextBox`, `SectionCard`,
`FieldLabel`, `Fluent.TabControl`, `Fluent.Dialog`,
`Fluent.NavigationMenu.Item`, `Fluent.StatusIndicator.Success/Warning/Error`,
`Fluent.ProgressBar`, `Fluent.ToolTip`, `LibraryDataGrid`.
Shell-only keys: `SidebarNavigationButton`, `SidebarActionButton`,
`DashboardModuleCard`, `SettingsThemeOption`.

Full file inventory, resource keys, model/API summary và controls có tên được
liệt kê trong [`PROJECT_SUMMARY.md`](./PROJECT_SUMMARY.md).

## Test infrastructure and verification

`tests/BizNext.Tests` là xUnit/VSTest với 14 test hiện có: JSON storage (4),
module catalog/navigation (3), QuickScan service/command (5), WPF composition
(2). Test dùng temporary directories và fake scanner; WPF tests dùng STA.
QuickScan view-construction test không kích hoạt `Loaded` hoặc quét thật.
`tests/BizNext.TestHost` là preview app cho một số component/module placeholder,
không phải QuickScan GUI harness.

Baseline lịch sử: 13/13 test trên commit gốc `42860dc`, tăng thành 14/14 sau
test overwrite; bước 1A có Release build 0 warning/0 error được ghi nhận.
Người dùng xác nhận 15/15 mục checklist smoke đạt ngày 2026-11-27, bao gồm
lượt sau bước 1A. Đây là kết quả người dùng báo từ môi trường thử, không phải
kết quả tự động xác minh mới nhất. Chi tiết ở
[`docs/baseline.md`](./docs/baseline.md) và
[`docs/smoke-test.md`](./docs/smoke-test.md).

Commands từ repo root:

```powershell
dotnet build .\BizNext-Manager.sln -c Release
dotnet test .\tests\BizNext.Tests\BizNext.Tests.csproj -c Release
dotnet run --project .\src\BizNext.Shell\BizNext.Shell.csproj
```

## Refactor option B — approved scope and current status

Đã duyệt phương án B: tái cấu trúc từng bước, offline-first, bảo toàn dữ liệu
và hành vi QuickScan, tái sử dụng BizNext.DesignSystem. Không tự chuyển sang
giai đoạn sau nếu chưa được duyệt.

- **Step 0 — baseline/smoke:** đã thêm test overwrite JSON và tài liệu
  `docs/baseline.md`, `docs/smoke-test.md`; người dùng xác nhận checklist đạt.
- **Step 1A — Shell DI composition:** đã thêm composition root DI và cập nhật
  startup/test; giữ module contract, user paths, navigation và view caching.
  Build Release và 14 test đạt theo ghi nhận trước; người dùng xác nhận smoke
  checklist đạt lại.
- **Not yet authorized:** data-driven navigation/module lifecycle và các bước
  tái cấu trúc tiếp theo vẫn là roadmap, không nằm trong thay đổi tài liệu này.

### Git handoff

Lần kiểm tra ngày 2026-11-28: branch `refactor/three-tier`, HEAD
`42860dc`. Thay đổi bước 0/1A và tài liệu đang nằm trong working tree, chưa
commit. Các file đã có thay đổi gồm `PROJECT_CONTEXT.md`, `README.md`,
`docs/architecture.md`, `src/BizNext.Shell/App.xaml.cs`,
`src/BizNext.Shell/BizNext.Shell.csproj`,
`src/BizNext.Shell/MainWindow.xaml.cs`,
`tests/BizNext.Tests/Infrastructure/JsonFileStorageTests.cs`,
`tests/BizNext.Tests/Integration/WpfCompositionTests.cs`, cùng các file mới
`PROJECT_TAICAUTRUC.md`, `docs/baseline.md`, `docs/smoke-test.md`,
`src/BizNext.Shell/ShellComposition.cs`. Giữ nguyên các thay đổi này; không
reset/checkout hoặc ghi đè nội dung chưa commit.

## Current documentation map

- [`PROJECT_SUMMARY.md`](./PROJECT_SUMMARY.md): inventory chi tiết files,
  API/state, models/services, controls/styles và test limitations.
- [`PROJECT_TAICAUTRUC.md`](./PROJECT_TAICAUTRUC.md): roadmap và quyết định
  tái cấu trúc.
- [`docs/architecture.md`](./docs/architecture.md): module lifecycle,
  project boundaries và quy ước mở rộng.
- [`README.md`](./README.md): build/run/test quick start.
