# Đổ bóng và chiều sâu

## 1. Khái niệm `[F2]`

Elevation là khoảng cách cảm nhận giữa vật thể và bề mặt phía sau, thể hiện bằng bóng và ánh sáng.
Hướng bóng nhất quán gợi một nguồn sáng. Bóng sắc nét = gần bề mặt; lớn và mềm = xa hơn.
Mỗi bóng gồm cặp **key** (sắc, có hướng) và **ambient** (khuếch tán). **Windows thay lớp key bằng stroke.**

## 2. Các mức `[F2]`

Sáu mức: `$shadow2, 4, 8, 16, 28, 64` (số là độ blur theo px).

| Dải | Theme | Shadow 1 | Shadow 2 |
|---|---|---|---|
| 2, 4, 8, 16 | Light | Y 0.5n, blur n, **14%** | cùng, 14% |
| 2, 4, 8, 16 | Dark | cùng, **28%** | cùng, 14% |
| 28, 64 | Light | Y 0.5n, blur n, **24%** | blur 8, 20% |
| 28, 64 | Dark | cùng, **28%** | blur **2**, 20% |

Ý nghĩa "n" suy ra là số của mức; trang không định nghĩa rõ `[CHƯA TÌM THẤY]`. Không có màu bóng (hex).

Dùng cho (tóm tắt): 2/4 thẻ không viền; 8 raised card, command bar, tooltip (dark); 16 callout, hover card;
28 bottom sheet, side navigation; 64 dialog pop-up, panel. Trên nền màu (brand) dùng token bóng brand,
điều chỉnh opacity theo độ sáng nền; không dùng thang chính. Công thức:
`luminosity = 0.2126R + 0.7152G + 0.0722B`; Shadow 1 = `Round(42 - 0.116*lum)`; Shadow 2 = `Round(34 - 0.09*lum)`.

## 3. Elevation trên Windows 11 `[WIN]`

| Bề mặt | Giá trị | Viền |
|---|---|---|
| Window | 128 | 1 |
| Dialog | 128 | 1 |
| Flyout | 32 | 1 |
| Tooltip | 16 | 1 |
| Card | 8 | 1 |
| Control | 2 | 1 |
| Layer | 1 | 1 |

Trạng thái control: Rest 2, Hover 2, **Pressed 1**. Cường độ bóng thay đổi theo theme ở cùng giá trị.
Hệ hai lớp: **base** (nền ứng dụng: menu, lệnh, điều hướng) và **content** (nội dung chính, liền khối hoặc chia thẻ).
Bóng + contour truyền đạt chiều sâu; "Overusing shadows ... can diminish their impact and create visual noise."

## 4. Chuyển sang WPF `[PROJECT]`

Các điểm dưới đây là đề xuất, chưa có nguồn Microsoft cho WPF.

- Ưu tiên **viền 1px + khác biệt màu bề mặt** (base/content) thay vì bóng, cho card và panel.
- Chỉ dùng bóng thật cho bề mặt nổi tạm thời: flyout, dropdown, tooltip, dialog.
- `DropShadowEffect` là cách triển khai; chi phí hiệu năng của effect trong WPF cần đo khi làm
  thử nghiệm (Giai đoạn 3-4) `[CHƯA TÌM THẤY]` trong nguồn đã đọc. Phương án nhẹ: thêm một `Border`
  mờ bên dưới thay vì `Effect` trên cây con lớn.
- Tạo token elevation theo tên mức Windows (`Elevation.Flyout`, `Elevation.Dialog`) để đổi một chỗ.
- Light dùng bóng mờ nhẹ; Dark cần opacity cao hơn (28% so với 14% ở dải thấp) để còn thấy.

```xaml
<DropShadowEffect x:Key="Shadow.Flyout" BlurRadius="16" ShadowDepth="4" Direction="270"
                  Opacity="0.14" Color="Black" />
```

Giá trị trong ví dụ là điểm khởi đầu để thử, không phải quy định.

## Lỗi cần tránh

- Bóng đậm hoặc nhiều lớp trên card thường xuyên; mọi phần tử đều có bóng.
- Dùng bóng web (CSS) nguyên bản cho WPF; blur, offset và đơn vị khác.
- Bóng trên màu brand bằng token neutral `[F2]`.
- Dùng bóng làm tín hiệu duy nhất để nhận biết ranh giới (cần viền cho High Contrast).

## Nguồn

- https://fluent2.microsoft.design/elevation
- https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/layering
