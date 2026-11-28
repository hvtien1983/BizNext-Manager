# Checklist smoke test bước 0

**Kết quả ngày 2026-11-27:** Người dùng xác nhận đã chạy thủ công và tất cả
15 mục bên dưới đều đạt. Môi trường là phiên Windows được người dùng xác nhận
chỉ dùng dữ liệu thử, với cấu hình scanner giả và thư mục PDF thử
`C:\Users\Admin\Desktop\TestScanFiles`. Kết quả này do người dùng cung cấp,
không phải xác nhận độc lập bằng công cụ tự động.

**Kiểm tra lại sau bước 1A (2026-11-27):** Người dùng xác nhận chạy lại
checklist trong môi trường thử và tất cả các mục đều đạt.

Không dùng scanner thật hoặc thư viện
QuickScan thật. Fake scanner không cô lập thư viện: ứng dụng vẫn trỏ tới
`%LocalAppData%\QuickScanManager\storage` và có thể dùng thư mục PDF đã cấu
hình. Shell cũng dùng `%LocalAppData%\BizNext-Manager`.

Chỉ chạy checklist sau khi xác minh ứng dụng đang hoạt động trong một hồ sơ
Windows thử độc lập, với các đường dẫn cấu hình, thư viện, PDF và dữ liệu
Shell đều thuộc môi trường thử; không dùng lại các file/thư mục của người dùng.
Chọn cấu hình fake bằng `QUICKSCAN_USE_FAKE_SCANNER=1`, xác nhận thiết bị giả
trước khi quét. Nếu không bảo đảm được cô lập, dừng và ghi **chưa chạy**;
không đổi tên, sao chép đè, di chuyển hay xóa dữ liệu người dùng để thử.

Ghi ngày, môi trường, kết quả và lỗi thực tế khi chạy; không đánh dấu đạt chỉ
vì build hoặc test tự động thành công.

- [x] Khởi động ứng dụng, không lỗi hoặc cửa sổ trống.
- [x] Mở Ghi chú, Lịch & Thời tiết và QuickScan; điều hướng hiển thị đúng.
- [x] Chuyển module, đóng và mở lại; module cuối được khôi phục.
- [x] Mở cài đặt Dashboard, chọn Sáng và lưu; giao diện cập nhật.
- [x] Chọn Tối và lưu; Shell và QuickScan hiển thị đúng.
- [x] Chọn Theo Windows và lưu; theme phù hợp cài đặt Windows hiện tại.
- [x] Dùng Tab và bàn phím điều hướng; focus hiển thị rõ.
- [x] Kiểm tra bố cục ở kích thước cửa sổ nhỏ nhất cho phép.
- [x] Mở cài đặt QuickScan, xác nhận thư mục PDF là thư mục thử độc lập.
- [x] Chọn scanner giả và quét một trang; ảnh xem trước hiển thị.
- [x] Lưu trang quét thành PDF trong thư mục thử; kết quả xuất hiện.
- [x] Mở Thư viện PDF, làm mới và tìm PDF vừa tạo.
- [x] Đổi tên PDF thử; thư viện phản ánh tên mới.
- [x] Xem nhanh PDF thử và xác nhận nội dung mở được.
- [x] Chuyển module rồi quay lại QuickScan; kiểm tra trạng thái đang làm việc.
