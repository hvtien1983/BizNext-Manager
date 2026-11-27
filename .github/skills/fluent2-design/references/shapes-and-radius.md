# Hình dạng, bo góc, viền

## 1. Bo góc `[F2]`

Hình dạng: chữ nhật, tròn, pill, beak (mũi nhọn của callout). Phân biệt bằng fill hoặc viền.

Mặc định: chữ nhật 4px; phần tử dưới 32px giảm còn 2px; component lớn/rất lớn tăng lên 8px/12px.

| Token | px | Dùng cho |
|---|---|---|
| None | 0 | Thanh điều hướng, tab bar |
| Small | 2 | Badge nhỏ |
| Medium | 4 | Button, dropdown |
| Large | 8 | Button lớn |
| X-Large | 12 | Button sheet, popover |
| Circle | 50% | Persona |

Không bo khi: bo sẽ tạo khe thừa (hai nửa SplitButton), hoặc component chạm cạnh màn hình.

## 2. Bo góc trên Windows 11 `[WIN]`

| Bán kính | Dùng cho |
|---|---|
| 8px | Phần tử cấp cao và tạm thời: cửa sổ, flyout, dialog, MenuFlyout, TeachingTip |
| 4px | Control luôn hiện: Button, CheckBox, ComboBox, TextBox, ListView; thanh: ProgressBar, ScrollBar, Slider; **ToolTip** (ngoại lệ vì nhỏ) |
| 0px | Cạnh thẳng chạm cạnh thẳng; cửa sổ đang snap hoặc maximize |

Hai tài nguyên toàn cục điều khiển mặc định: `ControlCornerRadius` (4px) và `OverlayCornerRadius` (8px).
Đây là tài nguyên của WinUI; WPF không có sẵn, cần tạo token cùng tên ý nghĩa `[PROJECT]`.

## 3. Mâu thuẫn giữa các nguồn (ghi lại, chưa tự giải quyết)

| Nguồn | Large | X-Large | Ghi chú |
|---|---|---|---|
| `[F2]` shapes | 8 | 12 | |
| `[TOKENS]` web | **6** | **8** (và 2XLarge = 12) | Cùng tên "Large" nhưng khác giá trị |
| `[WIN]` | 8 (overlay) | n/a | Chỉ có 4 / 8 / 0 |

Với WPF, theo `[WIN]` (4 control, 8 overlay) vì app chạy trên Windows; đây là lựa chọn `[PROJECT]`.
Không lấy `borderRadiusLarge` 6px của web.

## 4. Độ dày viền `[F2]` và `[TOKENS]`

| Tên | Web | Mobile |
|---|---|---|
| Thin | 1px | 1px |
| Thick | 2px | 2px |
| Thicker | 3px | 4px |
| Thickest | 4px | 6px |

Chọn độ dày theo kích thước phần tử. Nét đứt tỷ lệ theo độ dày (con số chỉ có trong hình).
Đầu nét tròn, tránh đầu vuông. `[WIN]`: đường viền (contour) khi biểu diễn elevation rộng 1.

## 5. Áp dụng cho WPF `[PROJECT]`

```xaml
<CornerRadius x:Key="ControlCornerRadius">4</CornerRadius>
<CornerRadius x:Key="OverlayCornerRadius">8</CornerRadius>
<Thickness x:Key="StrokeThin">1</Thickness>
```

- `Border.CornerRadius` cho control; tooltip 4, dialog/flyout 8.
- Chỉ bo khi mục đích rõ. Bề mặt nội dung liền khối (panel, vùng nội dung, hàng DataGrid) giữ 0.
- Bo góc cửa sổ chính trong WPF `[CHƯA TÌM THẤY]`; trang `[WIN]` chỉ nói ứng dụng Win32 có thể bật qua DWM API.

## Xung đột với mã nguồn hiện tại

`ControlCornerRadius`=8, `CardCornerRadius`=12, `PillCornerRadius`=999 (DesignTokens.xaml). Theo Windows
control là 4 và overlay là 8; card 12 và pill chưa có tương ứng trong nguồn Windows. Dùng `Border`
bo 12px cho thẻ ở khắp nơi cũng đi ngược yêu cầu "không lạm dụng card/rounded corner".

## Lỗi cần tránh

- Bo mọi thứ cùng một bán kính; bo vùng nội dung chiếm cả cạnh cửa sổ.
- Dùng bán kính 12 cho control nhỏ.
- Tạo bán kính mới (5px, 10px) ngoài thang.

## Nguồn

- https://fluent2.microsoft.design/shapes
- https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/geometry
- https://github.com/microsoft/fluentui/blob/master/packages/tokens/src/global/borderRadius.ts
- https://github.com/microsoft/fluentui/blob/master/packages/tokens/src/global/strokeWidths.ts
