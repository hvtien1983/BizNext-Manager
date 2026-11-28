# Baseline trước tái cấu trúc ba tầng

Ghi nhận ngày 2026-11-27, trên `main` tại commit `42860dc`, trước các thay đổi
của bước 0. Phương án B là tái cấu trúc từng bước, offline-first, tiếp tục dùng
BizNext.DesignSystem và giữ nguyên dữ liệu, hành vi QuickScan.

## Build và test

- `dotnet build .\BizNext-Manager.sln -c Debug --no-restore`: thành công,
  0 cảnh báo, 0 lỗi.
- `dotnet build .\BizNext-Manager.sln -c Release --no-restore`: thành công,
  0 cảnh báo, 0 lỗi.
- `dotnet test .\tests\BizNext.Tests\BizNext.Tests.csproj -c Debug --no-restore`:
  13/13 test đạt tại baseline trên.
- Sau khi bổ sung test ghi đè JSON: cùng lệnh test ở cả `Debug` và `Release`
  đều đạt 14/14.

Hai lượt build trên là build tăng dần, không phải rebuild sạch; số cảnh báo
chỉ phản ánh những gì các lượt đó phát ra. Không khôi phục package qua mạng.
SDK được dùng là 11.0.100; project test nhắm tới .NET 10 Windows.

## Hành vi đã được kiểm tra tự động

- JSON: file chưa có trả giá trị mặc định; cấu hình ghi/đọc lại được; JSON
  sai cú pháp và `null` được báo lỗi; lỗi tuần tự hóa không làm hỏng file cũ
  và file tạm được dọn. Test bổ sung trong bước 0 kiểm tra ghi đè thành công.
- Catalog: ID rỗng hoặc trùng không phân biệt hoa/thường bị từ chối; tìm ID
  không phân biệt hoa/thường; thứ tự là thứ tự đăng ký, không phải sắp xếp
  theo metadata.
- Điều hướng: sự kiện chỉ phát khi lựa chọn thay đổi; ID không tồn tại báo
  lỗi và giữ lựa chọn trước đó; có thể bỏ lựa chọn.
- QuickScan: scanner giả tạo ảnh trong thư mục tạm; browser thư viện và
  metadata dùng dữ liệu tạm. Test WPF dựng view và kiểm tra layout/theme,
  nhưng không kích hoạt `Loaded` của QuickScan và không kiểm tra luồng quét.

Không suy ra tính nguyên tử trong mọi sự cố hệ thống từ test lỗi tuần tự hóa:
test chỉ mô phỏng lỗi trước khi thay thế file đích.

## Smoke test và giới hạn

Người dùng xác nhận đã chạy đầy đủ [checklist smoke test](./smoke-test.md) ngày
2026-11-27 và cả 15 mục đều đạt trong môi trường thử độc lập. Kết quả này do
người dùng báo, không phải xác nhận độc lập bằng công cụ tự động.

Fake scanner chỉ thay thiết bị và một số file cấu hình; QuickScan vẫn dùng
thư viện/PDF và các đường dẫn người dùng thông thường. Không chạy smoke
QuickScan trên dữ liệu thật. Kiểm thử GUI cần môi trường Windows có hồ sơ và
dữ liệu thử độc lập, được xác minh trước khi chạy; không tự di chuyển, xóa
hoặc sửa dữ liệu hiện có.

Ở baseline, `QuickScanManager` tại gốc repo không tồn tại và không có file nào
thuộc thư mục đó được Git theo dõi. Đây không phải thư mục dữ liệu người dùng
`%LocalAppData%\QuickScanManager`.
