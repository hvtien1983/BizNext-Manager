# Typography

## 1. Font theo nền tảng `[F2]`

Windows: **Segoe UI Variable**. Web: Segoe UI. macOS/iOS: San Francisco Pro. Android: Roboto.
Hệ thống mặc định dùng font gốc của nền tảng.

## 2. Type ramp Windows `[F2]` và `[WIN]` (hai nguồn khớp nhau)

| Kiểu | Trọng lượng | Cỡ / line height |
|---|---|---|
| Caption | Regular | 12 / 16 |
| Body | Regular | 14 / 20 |
| Body Strong | Semibold | 14 / 20 |
| Body Large | Regular | 18 / 24 |
| Body Large Strong | Semibold | 18 / 24 (chỉ trang `[WIN]`) |
| Subtitle | Semibold (Display) | 20 / 28 |
| Title | Semibold (Display) | 28 / 36 |
| Title Large | Semibold (Display) | 40 / 52 |
| Display | Semibold (Display) | 68 / 92 |

Đơn vị là "px" ở `[F2]` và "epx" ở `[WIN]`. Ramp Web có thêm Caption 2 (10/14) và các biến thể
"Stronger"; Windows **không** có Caption 2.

## 3. Quy tắc Windows 11 `[WIN]`

| Thuộc tính | Giá trị |
|---|---|
| Trọng lượng | Regular cho phần lớn chữ, Semibold cho tiêu đề |
| Cỡ tối thiểu | **14px Semibold, 12px Regular** (nhỏ hơn "illegible in some languages") |
| Canh lề | Trái mặc định; giữa chỉ hiếm (chữ dưới icon) |
| Hoa/thường | Sentence case cho mọi chữ UI, kể cả tiêu đề |
| Cắt chữ | Ưu tiên ellipsis; clipping chỉ hiếm |
| Độ dài dòng | 50-60 ký tự; tránh <20 hoặc >60 |
| Nhấn mạnh | Bold và Italic **không** thuộc type ramp; dùng Semibold thay Bold |

Segoe UI Variable có hai trục: weight (100-700) và optical size (tự động, 8pt-36pt).
Trọng lượng: Light 300, Semilight 350, Regular 400, Semibold 600, Bold 700.
Dùng một font duy nhất trong UI.

## 4. Chuyển sang WPF `[PROJECT]`

- Chuỗi font: `Segoe UI Variable, Segoe UI`. Trang Windows nói Segoe UI Variable là font hệ thống
  "mới của Windows 11"; việc nó có trên Windows 10 hay không **không được nêu**
  `[CHƯA TÌM THẤY]` nên cần fallback `Segoe UI`. Selawik dành cho nền tảng không phải Windows, không dùng.
- Hỗ trợ trục optical size của font biến đổi trong WPF `[CHƯA TÌM THẤY]`; kiểm thử trực quan,
  đừng giả định.
- Coi giá trị px/epx của Windows là DIP của WPF; đây là giả định chưa có nguồn xác nhận.
- Tạo style `TextBlock` theo tên ramp (`FluentCaption`, `FluentBody`, `FluentBodyStrong`,
  `FluentSubtitle`, `FluentTitle`...) thay vì đặt `FontSize` rời rạc ở view.

```xaml
<Style x:Key="FluentBodyStrong" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="{DynamicResource FontFamilyBase}" />
    <Setter Property="FontSize" Value="14" />
    <Setter Property="FontWeight" Value="SemiBold" />
    <Setter Property="LineHeight" Value="20" />
    <Setter Property="LineStackingStrategy" Value="BlockLineHeight" />
</Style>
```

## Xung đột với mã nguồn hiện tại

`design-tokens.json` dùng caption 10, small 11, body 12, label 11 semibold; mật độ compact dùng
label 10 và control 11. Tất cả thấp hơn mức tối thiểu Windows 12 Regular / 14 Semibold,
và dưới Body 14 của ramp. Cần quyết định ở Giai đoạn 3: nâng cỡ chữ hay ghi nhận "mật độ compact" là
ngoại lệ có chủ đích `[PROJECT]`.

## Lỗi cần tránh

- Dùng chữ hoa toàn bộ ("difficult to read" `[F2]`).
- Dùng Bold/Italic để nhấn mạnh trong UI Windows.
- Trộn nhiều font; đặt `FontSize` tùy ý ngoài ramp.
- Dùng bảng ramp của web (Subtitle 1 = 20/26) cho Windows; hai nền tảng khác nhau.

## Nguồn

- https://fluent2.microsoft.design/typography
- https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography
