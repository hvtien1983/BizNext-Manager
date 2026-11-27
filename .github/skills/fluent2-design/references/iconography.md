# Iconography

## 1. Nguyên tắc `[F2]`

- Hai kiểu: **Regular** cho wayfinding (nhận biết hành động như tải xuống, mua); **Filled** cho trạng thái
  đã chọn hoặc khoảnh khắc cần nặng hơn. Modifier (hành động đi kèm) luôn Filled, ở góc dưới phải; bỏ nếu làm icon phức tạp.
- Chọn kích thước theo công cụ nhập: chuột chính xác cho phép icon nhỏ hơn.
- Icon 12px chỉ để cung cấp thông tin, thường quá nhỏ để tương tác.
- Icon hệ thống dùng một màu đặc, chú ý tương phản. Không đổi màu icon khởi chạy sản phẩm.
- Đặt tên theo hình dạng/vật thể, không theo chức năng ("Shield, not security").
- Kiểm tra biểu tượng phù hợp văn hóa.
- Thư viện: `microsoft/fluentui-system-icons` (GitHub), giấy phép MIT.

`[CHƯA TÌM THẤY]` trên trang: bảng cỡ icon hệ thống ngoài 12px, lưới/keyline, độ dày nét, bo góc nét,
hướng dẫn riêng cho Windows, cách cài đặt thư viện.

## 2. Icon trên Windows `[WIN]` (Segoe Fluent Icons)

- Windows 11: có sẵn. Windows 10: **không** kèm theo; tải từ trang Design resources, và trang nói
  rõ không được mang font sang nền tảng khác.
- Cỡ khuyến nghị: **16, 20, 24, 32, 40, 48, 64**; lệch cỡ có thể mờ.
- Không dùng trong dòng chữ; glyph nằm ở vùng Private Use Area (rủi ro nếu font thiếu); mọi glyph cùng
  độ rộng cố định nên xếp chồng để tô nhiều màu được; có dạng đối xứng cho ngôn ngữ RTL.
- Segoe MDL2 Assets bị thay thế bởi Segoe Fluent Icons ở Windows 11, "still available". Trang **không**
  nói đó là phương án dự phòng cho Windows 10 `[CHƯA TÌM THẤY]`.

## 3. Phương án cho BizNext `[PROJECT]`

Hiện trạng: module QuickScan có control `SvgIcon` và các tệp `.svg` (Assets/*.svg), được dùng cho tab;
sidebar Shell dùng ký tự chữ (ví dụ "⌂") làm icon.

| Phương án | Ưu | Nhược |
|---|---|---|
| A. Font Segoe Fluent Icons | Đúng Windows, nhẹ, theo màu chữ | Thiếu trên Windows 10; không có nguồn nói cách fallback; glyph PUA |
| B. SVG từ fluentui-system-icons (MIT) qua `SvgIcon` | Nhất quán mọi máy; không cần font; có Regular/Filled | Cần kiểm tra `SvgIcon` hỗ trợ tô màu theo theme và các cỡ 16/20/24 |
| C. Giữ ký tự Unicode | Không cần gì thêm | Không nhất quán, phụ thuộc font, không Regular/Filled |

Đề xuất tạm: B, nếu `SvgIcon` đáp ứng được; cần kiểm thử, chưa chốt. Quyết định cuối chờ Giai đoạn 3.

Quy tắc dùng chung:
- Cỡ 16 (inline/hàng), 20 (nút lệnh), 24 (điều hướng); chọn từ danh sách Windows trên.
- Tô màu bằng token (`TextSecondary` → `TextPrimary`/`Accent` khi chọn); không hard-code.
- Regular mặc định, Filled cho trạng thái đã chọn (item đang chọn ở sidebar).
- Icon-only button: bắt buộc `AutomationProperties.Name` và ToolTip.
- Không thêm package icon mới khi chưa phân tích.

## Lỗi cần tránh

- Trộn icon từ nhiều bộ khác độ dày nét.
- Dùng icon 12px cho nút bấm.
- Dùng emoji/Unicode làm icon chức năng.
- Đóng gói font Segoe Fluent Icons tải từ trang Design resources cùng ứng dụng khi chưa đọc kỹ điều khoản (trang chỉ nêu
  không được mang font sang nền tảng khác; việc phân phối kèm trên Windows 10 chưa rõ `[CHƯA TÌM THẤY]`).
- Icon một mình truyền đạt ý nghĩa mà không có nhãn hoặc tooltip.

## Nguồn

- https://fluent2.microsoft.design/iconography
- https://learn.microsoft.com/en-us/windows/apps/design/iconography/segoe-fluent-icons-font
- https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography (mục Symbols and icons)
