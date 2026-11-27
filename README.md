# BizNext Manager

BizNext Manager là ứng dụng Dashboard WPF đa tiện ích trên Windows. Các tiện
ích được đóng gói thành module riêng; hiện có **QuickScan** để quét tài liệu,
tạo PDF và quản lý thư viện. **Ghi chú** và **Lịch & Thời tiết** đã có module
khung để phát triển tiếp.

## Build và chạy

Yêu cầu .NET 10 SDK trên Windows:

```powershell
dotnet build .\BizNext-Manager.sln
dotnet run --project .\src\BizNext.Shell\BizNext.Shell.csproj
```

Trong VS Code, nhấn `F5` và chọn **BizNext Manager (WPF, giả lập scanner)**
để chạy QuickScan mà không cần máy scan thật. Chọn cấu hình scanner thật khi
có thiết bị WIA tương thích.

QuickScan giữ dữ liệu người dùng hiện có tại
`%LocalAppData%\QuickScanManager\`; việc nâng cấp không di chuyển/xóa dữ liệu
PDF, cấu hình máy scan hay thiết lập cũ.

Đổi giao diện Sáng/Tối/Theo Windows từ nút **Cài đặt** ở cuối sidebar.
Thư viện PDF hỗ trợ tìm theo tên file, duyệt thư mục con, sắp xếp thông tin
PDF và mở tài liệu trong cửa sổ xem tích hợp.

## Kiểm thử

Trên Windows với .NET 10 SDK hoặc SDK mới hơn có runtime Windows Desktop 10:

```powershell
dotnet test .\tests\BizNext.Tests\BizNext.Tests.csproj -c Release
dotnet build .\BizNext-Manager.sln -c Release
dotnet run --project .\tests\BizNext.TestHost\BizNext.TestHost.csproj -c Release
```

`BizNext.Tests` dùng xUnit/VSTest, chia test theo Shell, Infrastructure,
Modules và Integration. Có thể chạy riêng một nhóm, ví dụ:

```powershell
dotnet test .\tests\BizNext.Tests\BizNext.Tests.csproj -c Release --filter "FullyQualifiedName~BizNext.Tests.Modules"
```

Test ghi file vào thư mục tạm riêng, dùng fake scanner, không dùng máy scan
hoặc dữ liệu QuickScan thật. Test WPF chạy trên STA và kiểm tra tài nguyên,
theme và layout TestHost. Các test hiện chưa bao phủ toàn bộ thao tác GUI.

`BizNext.TestHost` là ứng dụng độc lập, dùng tài nguyên DesignSystem để xem
view hiện có của Ghi chú và Lịch & Thời tiết, cùng nút thử cửa sổ cài đặt
Dashboard. Có thể chuyển sáng/tối; thay đổi không lưu vào cài đặt ứng dụng.
Hiện cửa sổ cài đặt Shell báo lỗi không tìm thấy `assets/icon.ico` khi mở
từ TestHost; TestHost hiển thị lỗi thay vì đóng đột ngột. QuickScan chưa được
preview vì view hiện truy cập trực tiếp các đường dẫn người dùng. Chưa có
widget thật trong solution để preview hoặc kiểm thử.

Nếu ứng dụng đang chạy giữ DLL của cấu hình Debug, hãy dừng phiên debug
trước khi build Debug hoặc dùng cấu hình Release như trên.

## Kiến trúc

- `BizNext.Contracts`: hợp đồng và metadata chung của module.
- `BizNext.Infrastructure`: danh mục/điều hướng module, cài đặt Shell và lưu
  JSON nguyên tử.
- `BizNext.Shell`: entry point, Dashboard, theme và host các module.
- `BizNext.Modules.QuickScan`: giao diện UserControl và nghiệp vụ QuickScan.
- `BizNext.Modules.Notes`, `BizNext.Modules.WeatherCalendar`: khung module mở
  rộng trong tương lai.
- `ScanCore`, `ScanDevice`: thư viện hỗ trợ hiện tại của QuickScan, tách PDF/
  metadata khỏi truy cập WIA.

Xem [docs/architecture.md](./docs/architecture.md) để biết luồng module và
quy ước mở rộng. Hướng dẫn UI dùng chung nằm tại
[.github/skills](./.github/skills/README.md), bao gồm hướng dẫn WPF tại
[.github/skills/wpf/SKILL.md](./.github/skills/wpf/SKILL.md).
