# Hệ thống màu

## 1. Vai trò màu `[F2]`

| Nhóm | Mô tả | Quy tắc |
|---|---|---|
| Neutral | Đen, trắng, xám nền tảng cho bề mặt, chữ, bố cục | Neutral sáng hơn đặt trên bề mặt để kéo sự chú ý vào vùng chính |
| Shared | Màu dùng chung các app M365 (avatar, badge...) | "Use shared colors sparingly" |
| Semantic | Tập con của Shared: đỏ nguy hiểm, vàng thận trọng, xanh tích cực | Phải mang ý nghĩa thật, đi kèm tín hiệu khác |
| Brand | Mỗi sản phẩm có một màu | Dùng cho nút, CTA, trạng thái chọn. Không dùng trên bề mặt lớn |

Tương tác `[F2]`: thành phần thường tối dần theo tương tác (nghỉ nhạt nhất, hover tối hơn,
chọn tối nhất). Focus: màu giữ nguyên, **container có stroke dày hơn**. Trang ghi
"Windows reverses this, so controls get lighter with interaction". Giá trị web trong
`[TOKENS]` bên dưới (hover của nền sáng tối hơn nền nghỉ) **không** theo chiều Windows;
phải đối chiếu với Fluent theme của WPF trước khi chốt chiều đổi màu.

Theme tối `[F2]`: shared color đổi độ bão hòa/độ sáng để giảm mỏi mắt.
Không dựa vào màu đơn thuần; thêm chữ, icon hoặc tín hiệu khác.

## 2. Quy ước đặt tên token `[TOKENS]` (web)

`color{Neutral|Brand|CompoundBrand}{Foreground|Background|Stroke}{số}{Hover|Pressed|Selected|Disabled}`
Ví dụ: `colorNeutralForeground1`, `colorNeutralBackground1Hover`, `colorNeutralStroke1`,
`colorCompoundBrandBackgroundPressed`. Trang `/design-tokens` chỉ mô tả nguyên tắc
đặt tên bằng hình, nên quy ước trên suy ra từ mã nguồn, không phải văn bản Microsoft.

## 3. Giá trị tham chiếu `[TOKENS]` (web, MIT, không phải quy định cho WPF)

| Token | Light | Dark |
|---|---|---|
| NeutralForeground1 | #242424 | #FFFFFF |
| NeutralForeground2 | #424242 | #D6D6D6 |
| NeutralForeground3 | #616161 | #ADADAD |
| NeutralBackground1 | #FFFFFF | #292929 |
| NeutralBackground2 | #FAFAFA | #1F1F1F |
| NeutralBackground3 | #F5F5F5 | #141414 |
| NeutralStroke1 | #D1D1D1 | #666666 |
| NeutralStroke2 | #E0E0E0 | #525252 |
| NeutralStrokeAccessible | chưa đọc | #ADADAD |
| CompoundBrandBackground | #0078D4 | #2899F5 |
| CompoundBrandBackgroundHover | #106EBE | #3AA0F3 |
| CompoundBrandBackgroundPressed | #005A9E | #1890F1 |
| BrandForegroundLink | #106EBE | #2899F5 |
| StrokeFocus1 / 2 | #FFFFFF / #000000 | chưa đọc |

Ghi chú: tối không phải đảo màu. Ví dụ `colorBrandBackground` là Brand.80 (#0078D4) ở
Light nhưng Brand.70 (#106EBE) ở Dark, còn foreground brand sáng lên ở Dark. Màu trạng thái
(`colorStatus*`) **không** nằm trong `lightColor.ts`; nằm ở `statusColorMapping.ts`/`sharedColorNames.ts`
(chưa đọc) `[CHƯA TÌM THẤY]`.

`#0078D4` là brand ramp của theme web mặc định, không phải màu bắt buộc của BizNext.
Brand là thứ có thể thay theo sản phẩm `[F2]`.

## 4. Tương tác với WPF `[WPF]`

WPF .NET 9+ hỗ trợ màu nhấn người dùng chọn qua `SystemColors.AccentColor*` (Accent,
Light1-3, Dark1-3; có cả Color/Brush/ResourceKey), dùng được kể cả khi không bật Fluent theme.
Dùng `DynamicResource` để cập nhật khi người dùng đổi accent:

```xaml
<TextBlock Foreground="{DynamicResource {x:Static SystemColors.AccentColorBrushKey}}" />
```

## 5. Tương phản `[F2]`

Văn bản thường ≥ 4.5:1; chữ lớn (>18.5px đậm hoặc 24px thường) ≥ 3:1; thành phần
tương tác và phi văn bản ≥ 3:1 so với màu kề bên.

### Kiểm tra màu hiện có của BizNext `[PROJECT]` (tính bằng công thức WCAG, nên đo lại bằng công cụ)

| Cặp | Tỷ lệ | Kết luận |
|---|---|---|
| TextMuted #748099 / trắng | 3.97 | **Dưới 4.5**, dùng cho chữ 10-11px |
| TextMuted #748099 / canvas #F5F7FA | 3.70 | **Dưới 4.5** |
| TextSecondary #536078 / trắng | 6.34 | Đạt |
| TextIcon #8A96AA / trắng | 2.99 | **Dưới 3** cho icon |
| Trắng / ActionPrimary #356AE6 | 4.82 | Đạt |
| Dark muted #A0AABD / #1B2230 | 6.81 | Đạt |
| BorderDefault #E5E9F0 / trắng | 1.22 | Chỉ ổn cho viền trang trí; viền ranh giới control cần ≥ 3 |

Tham chiếu: `NeutralStroke1` Fluent #D1D1D1 trên trắng chỉ 1.53, nên Fluent có token riêng
`colorNeutralStrokeAccessible` cho viền cần nhận biết. Không dùng stroke trang trí làm ranh
giới duy nhất của một input.

## 6. High Contrast `[WIN]`

- Contrast theme (tỷ lệ thường ≥ 7:1) khác theme sáng/tối. Người dùng có 4 theme: Aquatic, Desert, Dusk, Night sky.
- Dùng cặp màu hệ thống: Window/WindowText (nền/chữ), ButtonFace/ButtonText (control), Highlight/HighlightText (chọn, hover, pressed), GrayText (chỉ cho vô hiệu), Hotlight (chỉ cho hyperlink).
- Không hard-code màu trong nhánh High Contrast, không ghép cặp không tương thích, không dùng GrayText cho chữ phụ.
- Viền 2px được khuyến nghị cho bề mặt tạm thời (flyout, dialog).
- Ví dụ trên là XAML WinUI (`ThemeDictionaries`, `ThemeResource`). WPF không có hai thứ này;
  cách làm tương đương trong WPF: xem [wpf-adaptation](./wpf-adaptation.md). Các key
  `SystemColors.*` của WPF cần xác minh khi triển khai `[CHƯA TÌM THẤY]` trong nguồn đã đọc.
- .NET 10 ghi nhận đã "Fixed crashes related to HighContrast" trong Fluent theme `[WPF]`.

## Lỗi cần tránh

- Màu brand phủ bề mặt lớn; dùng màu semantic làm trang trí.
- Chỉ đổi màu để báo trạng thái (lỗi/thành công) mà không có chữ/icon.
- Theme tối bằng cách đảo màu sáng; Dark có bảng ánh xạ riêng.
- Hard-code hex trong control; mọi màu đi qua token + `DynamicResource`.
- Cho rằng hover luôn tối đi: chiều đổi màu của Windows ngược với web `[F2]`.

## Nguồn

- https://fluent2.microsoft.design/color
- https://fluent2.microsoft.design/design-tokens/
- https://fluent2.microsoft.design/accessibility
- https://github.com/microsoft/fluentui/tree/master/packages/tokens/src (alias/lightColor.ts, alias/darkColor.ts)
- https://learn.microsoft.com/en-us/windows/apps/design/accessibility/high-contrast-themes
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net90
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net100
