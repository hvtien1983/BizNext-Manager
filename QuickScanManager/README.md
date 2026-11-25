# QuickScanManager

Ứng dụng WPF trên Windows để scan tài liệu và quản lý thư viện PDF.

## Cấu trúc

- `src/ScanCore`: mô hình tài liệu và dịch vụ lưu thư viện JSON/PDF.
- `src/ScanDevice`: giao tiếp máy scan bằng Windows Image Acquisition (WIA).
- `src/ScanApp`: giao diện WPF.
- `storage/pdfs`: nơi lưu PDF; `storage/library.json`: danh mục tài liệu.

## Build và chạy

Yêu cầu .NET 8 SDK trên Windows:

```powershell
dotnet build .\QuickScanManager.sln
dotnet run --project .\src\ScanApp\ScanApp.csproj
```

WIA yêu cầu Windows và máy scan có driver WIA tương thích.

## Chọn và cấu hình máy scan

Danh sách máy scan được dò tự động qua WIA khi mở ứng dụng. Ứng dụng giữ
`DeviceId` làm định danh, khôi phục máy được chọn gần nhất nếu thiết bị còn
kết nối, và phân biệt các máy trùng tên bằng một phần ID.

- **Làm mới**: dò lại thiết bị WIA.
- **Thêm thiết bị Windows**: mở trang Printers & scanners của Windows; sau khi
  cài driver/kết nối thiết bị, quay lại ứng dụng và làm mới danh sách.
- **Thêm/Sửa bí danh**: lưu tên gợi nhớ và mặc định màu, DPI, nguồn nạp cho
  thiết bị đang chọn.
- **Xóa bí danh**: chỉ xóa profile trong ứng dụng, không gỡ thiết bị khỏi
  Windows.

Cấu hình được lưu tại `%LocalAppData%\QuickScanManager\scanner-config.json`.
Không cần cài NuGet package hoặc thêm COM reference: `ScanDevice` gọi WIA COM
trực tiếp và thực hiện các thao tác WIA trên STA thread. Nếu tệp cấu hình JSON
bị lỗi, danh sách thiết bị vẫn được dò nhưng ứng dụng sẽ không ghi đè cấu hình
đó; hãy sửa hoặc xóa tệp cấu hình rồi mở lại ứng dụng để lưu thay đổi.
