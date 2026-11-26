# QuickScanManager

Ứng dụng WPF trên Windows để scan tài liệu và quản lý thư viện PDF.

## Cấu trúc

- `src/ScanCore`: mô hình tài liệu và dịch vụ lưu thư viện JSON/PDF.
- `src/ScanDevice`: giao tiếp máy scan bằng Windows Image Acquisition (WIA).
- `src/ScanApp`: giao diện WPF.
- `%LocalAppData%/QuickScanManager/storage/pdfs`: nơi lưu PDF đã xuất;
  `%LocalAppData%/QuickScanManager/storage/library.json`: danh mục tài liệu.

## Build và chạy

Yêu cầu .NET 10 SDK trên Windows:

```powershell
dotnet build .\QuickScanManager.sln
dotnet run --project .\src\ScanApp\ScanApp.csproj
```

WIA yêu cầu Windows và máy scan có driver WIA tương thích.

## Chạy với máy scan giả lập

Để kiểm tra danh sách máy scan và luồng quét trang khi không có phần cứng, mở
terminal tại thư mục `QuickScanManager` và chạy:

```powershell
$env:QUICKSCAN_USE_FAKE_SCANNER = "1"
dotnet run --project .\src\ScanApp\ScanApp.csproj
```

Chế độ giả lập hiển thị hai thiết bị tên giống nhau để kiểm tra cách phân biệt
thiết bị bằng `DeviceId`. Chọn một máy ảo rồi bấm **Quét trang** để tạo ảnh BMP
tài liệu mẫu; ảnh được thêm vào danh sách trang và hiển thị trong vùng xem trước.
Nút **Quét hàng loạt** khả dụng khi máy scan hỗ trợ ADF; trong lúc quét, nút
chuyển thành **Tạm dừng** và các trang được thêm vào danh sách ngay khi quét
xong. Chế độ giả lập quét ba trang để thử luồng này.
Ảnh tạm nằm trong `%LocalAppData%\QuickScanManager\scans`. Dùng trường **Tên
file PDF** để nhập tên (phần mở rộng `.pdf` được gắn cố định), sau đó bấm
**Lưu thành PDF** để ghép các trang theo thứ tự trong danh sách. File được lưu
trong `%LocalAppData%\QuickScanManager\storage\pdfs` và đăng ký trong
`library.json`. Thông báo thao tác hiển thị ở thanh cuối cửa sổ, phân biệt
thành công, chú ý và lỗi bằng màu sắc. Dịch vụ WIA thật không bị gọi khi biến
môi trường được bật.

Chế độ này dùng cấu hình riêng
`%LocalAppData%\QuickScanManager\scanner-config.fake.json` và
`app-settings.fake.json`. Tắt giả lập bằng cách đóng terminal hoặc chạy:

```powershell
Remove-Item Env:QUICKSCAN_USE_FAKE_SCANNER
```

Giả lập hoạt động ở tầng ứng dụng, không cài thiết bị/driver WIA vào Windows.

## Chọn và cấu hình máy scan

Danh sách máy scan được dò tự động qua WIA khi mở ứng dụng. Ứng dụng giữ
`DeviceId` làm định danh, khôi phục máy được chọn gần nhất nếu thiết bị còn
kết nối, và phân biệt các máy trùng tên bằng một phần ID.

- **Thêm máy mới**: mục cuối trong danh sách mở trang Printers & scanners của
  Windows. Sau khi cài driver/kết nối thiết bị, mở lại ứng dụng để nạp danh sách.
- **Thêm/Sửa bí danh**: lưu tên gợi nhớ và mặc định màu, DPI, nguồn nạp cho
  thiết bị đang chọn.
- **Xóa bí danh**: chỉ xóa profile trong ứng dụng, không gỡ thiết bị khỏi
  Windows.

Cấu hình được lưu tại `%LocalAppData%\QuickScanManager\scanner-config.json`.
Các cài đặt ứng dụng được lưu tại
`%LocalAppData%\QuickScanManager\app-settings.json` (chế độ giả lập dùng
`app-settings.fake.json`). Nút **Cài đặt** cho phép đổi thư mục PDF mặc định,
chọn máy scan mặc định trong các máy đã có bí danh/cấu hình, và bật/tắt hậu tố
thời gian `yyyy-MM-dd_hh-mm-ss` trước `.pdf`, và chọn chủ đề **Màu Sáng**,
**Màu Tối** hoặc **Theo hệ thống**. Chủ đề hệ thống đọc cài đặt giao diện ứng
dụng của Windows mỗi khi mở app. Khi hậu tố bật, tên trùng trong cùng một giây
sẽ nhận thêm số thứ tự để không ghi đè file đã có.
Không cần cài NuGet package hoặc thêm COM reference: `ScanDevice` gọi WIA COM
trực tiếp và thực hiện các thao tác WIA trên STA thread. Nếu tệp cấu hình JSON
bị lỗi, danh sách thiết bị vẫn được dò nhưng ứng dụng sẽ không ghi đè cấu hình
đó; hãy sửa hoặc xóa tệp cấu hình rồi mở lại ứng dụng để lưu thay đổi.
