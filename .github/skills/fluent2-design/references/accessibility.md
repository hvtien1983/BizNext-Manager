# Accessibility

## 1. Chuẩn chính thức `[F2]`

Component Fluent "meet or surpass WCAG 2.1 AA". Không gắn với một nền tảng cụ thể.

| Chủ đề | Quy định |
|---|---|
| Tương phản chữ thường | ≥ 4.5:1 |
| Chữ lớn | ≥ 3:1 (trên 18.5px đậm hoặc 24px thường) |
| Control tương tác, icon, phi văn bản | ≥ 3:1 với màu kề bên |
| Focus | Nhìn thấy được; thứ tự trái→phải, trên→dưới (đường Z); không mất focus khi UI tạm thời (dialog) đóng |
| Màu | Không dùng màu làm tín hiệu duy nhất (thêm chữ, đồ họa) `[F2]` color |
| Zoom | Nội dung reflow đến 400% không cuộn ngang (thiết kế xuống breakpoint 320); chữ phóng 200% không bị cắt |
| Trình đọc màn hình | Heading đúng thứ tự; alt text; mã ngữ nghĩa theo chuẩn |
| Tài liệu thiết kế | Ghi thứ tự focus, chú thích screen reader, cấu trúc ngữ nghĩa |

`[CHƯA TÌM THẤY]` trên trang: độ dày/màu vòng focus, kích thước mục tiêu chạm, High Contrast, reduced motion
(phần này có ở trang motion), quy định về "không chỉ dùng màu" (có ở trang color).

Focus `[F2]` color: màu control giữ nguyên, container có **stroke dày hơn** để phân biệt bàn phím và chuột.
Token tham chiếu web `[TOKENS]`: `colorStrokeFocus1` #FFFFFF, `colorStrokeFocus2` #000000 (vòng đôi: sáng + tối).

## 2. Windows và High Contrast `[WIN]`

- Contrast theme (≥ 7:1 thường) khác theme sáng/tối; thử cả 4 theme Aquatic, Desert, Dusk, Night sky
  (Settings > Accessibility > Contrast themes; phím tắt Left Alt + Left Shift + PrtScn).
- Dùng cặp màu hệ thống, không hard-code; GrayText chỉ cho vô hiệu; Hotlight chỉ cho hyperlink.
- Item danh sách: đặt `Foreground` cố định trong DataTemplate làm hỏng đảo màu khi chọn.
- Đây là hướng dẫn WinUI; cách làm trong WPF xem [wpf-adaptation](./wpf-adaptation.md).

## 3. WPF

`[WPF]` .NET 10: Fluent theme đã "Fixed crashes related to HighContrast" và thêm `RecognizesAccessKey` còn thiếu.

`[PROJECT]` (kỹ thuật WPF, cần kiểm chứng khi triển khai):

| Việc | Cách làm |
|---|---|
| Tên truy cập | `AutomationProperties.Name` cho control không có nhãn chữ (nút icon, ô tìm); `LabeledBy` cho input |
| Nhãn ô nhập | Nhãn cố định, không dùng placeholder làm nhãn |
| Thứ tự Tab | Theo thứ tự logic trong cây; chỉ dùng `TabIndex` khi bắt buộc |
| Focus ring | Dùng `FocusVisualStyle` (chỉ hiện khi dùng bàn phím); xem lưu ý dưới |
| Dialog | Mở: focus vào control đầu tiên/nút chính; đóng: trả focus về control mở dialog |
| Phím | Enter = hành động chính (`IsDefault`), Esc = hủy (`IsCancel`) |
| Thông báo động | Báo trạng thái (lỗi, hoàn tất) qua vùng có `AutomationProperties.LiveSetting` |
| Giảm chuyển động | `SystemParameters.ClientAreaAnimation` cho biết tính năng animation client area bật hay tắt `[WPF]`; tài liệu không nói nó khớp cài đặt "Animation effects" của Windows, cần kiểm chứng trước khi dựa vào |
| Văn bản phóng | Kiểm tra chữ 200% không bị cắt; DPI/thang chữ trong WPF chưa có nguồn đã đọc |

### Lưu ý về vòng focus (phát hiện trong mã hiện tại)

Template hiện có đặt `FocusVisualStyle="{x:Null}"` và hiển thị viền bằng trigger `IsKeyboardFocused`. Trong WPF
`IsKeyboardFocused` cũng đúng sau khi **click chuột** vào control, nên viền focus xuất hiện cả khi dùng chuột.
Điều này đi ngược mục tiêu Fluent "tách bàn phím khỏi chuột" `[F2]`. Hành vi WPF này đến từ kiến thức nền, chưa
có nguồn đã đọc; cần kiểm tra trên app thật ở Giai đoạn 4.

## 4. Kiểm tra trước khi xem là xong

- Duyệt toàn bộ luồng chỉ bằng bàn phím (Tab, Shift+Tab, mũi tên, Enter, Esc, Space).
- Narrator đọc đúng tên, vai trò, trạng thái.
- Light, Dark, và ít nhất một contrast theme.
- Đo lại tương phản các cặp token mới (bảng cặp màu hiện có ở [color-system](./color-system.md)).

## Lỗi cần tránh

- Bỏ vòng focus để "đẹp"; dùng Opacity làm tín hiệu vô hiệu duy nhất.
- Icon-only mà không có tên truy cập.
- Màu lỗi đỏ mà không có icon hoặc câu chữ.
- Thông tin chỉ có trong tooltip.
- Chữ phụ 3.97:1 (xem color-system) cho nội dung cần đọc.

## Nguồn

- https://fluent2.microsoft.design/accessibility
- https://fluent2.microsoft.design/color
- https://learn.microsoft.com/en-us/windows/apps/design/accessibility/high-contrast-themes
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net100
- https://learn.microsoft.com/en-us/dotnet/api/system.windows.systemparameters.clientareaanimation
