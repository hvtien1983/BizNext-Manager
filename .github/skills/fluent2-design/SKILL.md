---
name: fluent2-design
description: "Use when designing or restyling any BizNext Manager WPF UI (themes, tokens, controls, dialogs, navigation, icons, motion, accessibility). Fluent 2 is the project's chosen design language; this skill separates official Fluent 2 rules from Windows/WPF conventions and BizNext project decisions."
compatibility: "WPF on net10.0-windows. Fluent 2 has no official WPF kit; everything WPF-specific here is an adaptation."
---

# Fluent 2 for BizNext Manager (WPF)

Fluent 2 (Microsoft) is the chosen design language. Read the reference that matches
the task before changing any XAML. Knowledge fetched 2026-11-27; Microsoft pages can
change, so re-check a value before treating it as final.

## Trạng thái

Giai đoạn 2/7 (Knowledge Base). Chưa có Design Tokens/Style Fluent trong mã nguồn.
Cho tới khi Giai đoạn 3 được duyệt và hoàn thành, **các token đang chạy thật vẫn là
`BizNext.DesignSystem/Themes/*`** và skill `ui-design-system`. Không sửa XAML theo
Fluent 2 khi chưa có token tương ứng.

## Ba tầng quy tắc (không được trộn lẫn)

| Nhãn | Ý nghĩa | Được gọi là "quy định Microsoft"? |
|---|---|---|
| `[F2]` | Trang fluent2.microsoft.design | Có |
| `[WIN]` | learn.microsoft.com, cách Fluent áp dụng cho Windows/WinUI | Có, cho Windows |
| `[WPF]` | learn.microsoft.com, tính năng/API của WPF | Có, cho WPF |
| `[TOKENS]` | Mã nguồn `microsoft/fluentui` `packages/tokens` (web, MIT) | Giá trị tham chiếu của web, không phải quy định cho WPF |
| `[PROJECT]` | Quyết định/suy luận của BizNext | **Không** |
| `[CHƯA TÌM THẤY]` | Không có bằng chứng trong nguồn đã đọc | Không được áp dụng như quy định |

Khi viết code hoặc tài liệu, giữ nguyên nhãn. Không nâng một `[PROJECT]` thành `[F2]`.

## Chọn tài liệu

| Việc cần làm | Đọc |
|---|---|
| Hiểu triết lý, quyết định thiết kế | [design-principles](./references/design-principles.md) |
| Màu, theme sáng/tối, High Contrast | [color-system](./references/color-system.md) |
| Font, cỡ chữ, type ramp | [typography](./references/typography.md) |
| Khoảng cách, lưới, mật độ | [spacing-and-layout](./references/spacing-and-layout.md) |
| Bo góc, viền | [shapes-and-radius](./references/shapes-and-radius.md) |
| Đổ bóng, lớp nền | [elevation-and-depth](./references/elevation-and-depth.md) |
| Icon | [iconography](./references/iconography.md) |
| Từng control | [controls-and-components](./references/controls-and-components.md) |
| Sidebar, tab, điều hướng | [navigation-patterns](./references/navigation-patterns.md) |
| Bàn phím, tương phản, trình đọc màn hình | [accessibility](./references/accessibility.md) |
| Animation, hiệu ứng tương tác | [animation-and-interaction](./references/animation-and-interaction.md) |
| Cách chuyển sang WPF, quyết định kiến trúc | [wpf-adaptation](./references/wpf-adaptation.md) |

## Quy tắc làm việc

1. Đọc reference liên quan trước khi sửa giao diện; ghi nhãn nguồn khi trích dẫn.
2. Dùng token đã có; không tự đặt màu, kích thước, bo góc mới. Cần giá trị mới thì
   thêm vào token (xem `ui-design-system`) và ghi lý do.
3. Giữ MVVM/Binding; chỉ sửa tầng View khi được giao nhiệm vụ UI.
4. Fluent 2 không có bộ WPF chính thức: mọi control WPF là bản chuyển thể, kiểm tra
   thực tế (chạy app, Light/Dark, High Contrast, bàn phím).
5. Tiết chế gradient, blur, transparency, shadow; chỉ dùng khi có mục đích.

## Xung đột đã biết với hệ thống hiện tại (chờ quyết định)

Chi tiết và số liệu ở [wpf-adaptation](./references/wpf-adaptation.md#5-xung-đột-với-mã-nguồn-hiện-tại).

- Bo góc 8/12 so với Windows 4/8.
- Chữ 10-12px so với tối thiểu Windows 12 Regular / 14 Semibold.
- `TextMuted` (#748099) trên nền trắng đạt khoảng 3.97:1, dưới 4.5:1.
- WPF (.NET 9+) đã có sẵn Fluent theme tích hợp; cần quyết định dùng làm nền hay tự dựng toàn bộ.
