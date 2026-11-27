# Khoảng cách và bố cục

## 1. Thang khoảng cách `[F2]`

Đơn vị cơ sở 4px ("4x system"). Đo từ bounding box của phần tử.

| Token | px | Token | px |
|---|---|---|---|
| sizeNone | 0 | size200 | 20 |
| size20 | 2 | size240 | 24 |
| size40 | 4 | size280 | 28 |
| size60 | 6 | size320 | 32 |
| size80 | 8 | size360 | 36 |
| size100 | 10 | size400 | 40 |
| size120 | 12 | size480 | 48 |
| size160 | 16 | size520 / 560 | 52 / 56 |

2, 6, 10 nằm ngoài lưới 4px, tồn tại để bù padding bên trong icon. Spacer nhỏ đặt bên trong
component.

Mã web `[TOKENS]` dùng tên khác cho cùng thang: `none 0, xxs 2, xs 4, sNudge 6, s 8, mNudge 10,
m 12, l 16, xl 20, xxl 24, xxxl 32` (`spacingHorizontal*`, `spacingVertical*`).

## 2. Lưới và breakpoint `[F2]`

Lưới gồm columns, gutters, margins. Khung 12 cột phổ biến (chia 2, 3, 4, 6). Gutter là bội của đơn vị cơ sở.

| Size class | Khoảng |
|---|---|
| small | 320-479 |
| medium | 480-639 |
| large | 640-1023 |
| x-large | 1024-1365 |
| xx-large | 1366-1919 |
| xxx-large | 1920+ |

Kỹ thuật responsive: reposition, resize, reflow, show/hide, re-architect.

## 3. Mục tiêu chạm tối thiểu `[F2]`

iOS và Web 44x44, Android 48x48. **Windows không được nêu** `[CHƯA TÌM THẤY]`.

## 4. Không có trong nguồn `[CHƯA TÌM THẤY]`

Chế độ mật độ (compact/comfortable), giá trị gutter/margin theo breakpoint, số cột theo
breakpoint, bảng ánh xạ token → component, chiều rộng nội dung tối đa, row height của baseline grid.
Trang chỉ cảnh báo thông tin quá dày có thể làm choáng.

## 5. Áp dụng cho BizNext `[PROJECT]`

- Dùng đúng các giá trị trong thang trên (4, 8, 12, 16, 20, 24, 32...) làm `Thickness`/`double` token;
  đừng thêm 5, 7, 9, 14.
- Cửa sổ chính `MinWidth=1200` nằm ở size class x-large; thiết kế cho 1024-1365 trước, rồi nới ra.
- Mật độ compact (control cao 36px, nút 34/38px) là **quy ước BizNext**, Fluent 2 không định nghĩa. Giữ,
  nhưng ghi rõ là ngoại lệ và đặt tên token theo mật độ, không theo màn hình.
- Hiện có `CompactCardPadding=10`, `CompactFieldGap=12`; 10 là giá trị off-grid của thang
  (dùng cho padding trong icon), nên cân nhắc đổi thành 8 hoặc 12.
- Margin trang 16-24; khoảng cách giữa các nhóm 16-24; giữa nhãn và ô nhập 4-8.

## Ví dụ

```xaml
<Thickness x:Key="Spacing.L">16</Thickness>
<sys:Double x:Key="Spacing.M">12</sys:Double>
<Grid Margin="{StaticResource Spacing.L}" />
```

## Lỗi cần tránh

- Khoảng cách tùy ý (13px, 18px) ngoài thang.
- Gán token "size" của Fluent cho chiều cao control (size là khoảng cách, không phải chiều cao control).
- Thu nhỏ control và chữ cùng lúc để nhét thêm dữ liệu; mất khả năng đọc.
- Dùng breakpoint di động (320) cho ứng dụng desktop có `MinWidth` 1200 mà không có ca kiểm thử.

## Nguồn

- https://fluent2.microsoft.design/layout
- https://github.com/microsoft/fluentui/blob/master/packages/tokens/src/global/spacings.ts
