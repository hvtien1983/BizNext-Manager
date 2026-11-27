# Chuyển Fluent 2 sang WPF

Fluent 2 **không có bộ kit hoặc thư viện WPF**. Trang Develop liệt kê React, Web Components, iOS, Android và
Windows (chỉ nói WinUI); trang Design chỉ có Figma kit Web/iOS/Android; roadmap không nhắc WPF `[F2]`.
Mọi thứ cho WPF là **bản chuyển thể**, không phải quy định Microsoft.

## 1. Bằng chứng về nền tảng WPF `[WPF]`

- **.NET 9** thêm **Fluent theme** tích hợp: giao diện Windows 11, sáng/tối, hỗ trợ màu nhấn hệ thống.
- Cách bật: thuộc tính `ThemeMode` (`Light`, `Dark`, `System`, `None` mặc định = Aero2) trên `Application` hoặc `Window`;
  hoặc nạp `pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml`.
- Đặt `ThemeMode` trong XAML dùng được. Đặt trong **code** là tính năng *thử nghiệm*, phát lỗi **WPF0001**, cần
  `<NoWarn>$(NoWarn);WPF0001</NoWarn>` (hoặc `#pragma`); "subject to change".
- Nếu `ThemeMode` ở `Application` khác `None`, không thể đặt `None` ở cấp `Window`.
- "`ThemeMode` is designed to respect the settings set by a Fluent Dictionary, allowing you to customize the Fluent theme."
- **.NET 10** (phát hành 11/2025): thêm style Fluent cho DatePicker, GridSplitter, GridView, GroupBox, Hyperlink, Label,
  NavigationWindow, RichTextBox, TextBox; sửa animation Expander, lỗi HighContrast, RTL. Ghi rõ "Fluent UI style support is still in progress".
- Màu nhấn: `SystemColors.AccentColor*` (có Light1-3, Dark1-3), dùng được cả khi không bật Fluent.
- Xem thử: ứng dụng **WPF Gallery** (Microsoft Store).

`[CHƯA TÌM THẤY]`: danh sách key tài nguyên của Fluent theme; control nào được style đầy đủ; cách ghi đè từng key
(chỉ có câu nói ý định ở trên); `ControlCornerRadius`/`OverlayCornerRadius` có tồn tại trong WPF hay không.

## 2. Quyết định kiến trúc cần xác nhận (chưa chọn)

| | A. Fluent theme tích hợp làm nền + lớp token BizNext + template riêng cho phần thiếu | B. Tự dựng toàn bộ style cho mọi control |
|---|---|---|
| Khối lượng | Thấp hơn; chỉ viết phần thiếu (Card, Dialog, Status, Nav, DataGrid...) | Cao; ~15 control phải viết và kiểm thử |
| Theo Windows | Tự cập nhật theo bản .NET | Phải tự bảo trì |
| Light/Dark/System | `ThemeMode` ánh xạ trực tiếp `ThemePreference` hiện có | Tự quản lý như `ThemeManager` hiện tại |
| Màu nhấn hệ thống | Có sẵn | Phải làm |
| Rủi ro | `ThemeMode` trong code còn thử nghiệm; độ phủ control chưa đủ; ghi đè key chưa kiểm chứng | Không dính API thử nghiệm; trùng việc Microsoft đang làm |
| Kiểm soát giao diện | Phụ thuộc cấu trúc nội bộ của theme | Toàn quyền |

C. Thư viện bên thứ ba (WPF-UI, MahApps, ModernWpf...): **không đề xuất**; dự án quy định không thêm package khi chưa phân
tích, và đã có theme tích hợp. Nếu cần xét, phải có báo cáo tương thích .NET 10 và giấy phép riêng.

Đề xuất: **A**, sau khi chạy một *spike* ngắn để kiểm chứng ba điều chưa có nguồn: (1) key nào ghi đè được và ghi đè
bằng cách nào; (2) control nào đã có Fluent trong .NET 10; (3) `ThemeMode` ở cấp `Window` đặt từ XAML có đủ dùng không
(tránh cờ thử nghiệm). Nếu spike thất bại, chuyển sang B. Quyết định thuộc về chủ dự án.

## 3. Bản đồ chuyển thể

| Khái niệm Fluent 2 | Tương ứng WPF | Ghi chú |
|---|---|---|
| Global/alias token | `ResourceDictionary` (Color → `SolidColorBrush` → brush ngữ nghĩa) | Hai tầng; không có tầng component token `[F2]` |
| Theme sáng/tối | Từ điển theme hoán đổi, hoặc `ThemeMode` | Dùng `DynamicResource` cho mọi thứ đổi theo theme |
| High Contrast | `SystemColors` / từ điển theme riêng | WinUI dùng `ThemeDictionaries`; WPF không có, cách thay thế cần chốt |
| Type ramp | Style `TextBlock` theo tên | Xem typography |
| Spacing | Token `Thickness`/`double` | Xem spacing-and-layout |
| Corner radius | Token `CornerRadius` | 4 control / 8 overlay |
| Elevation | Viền + màu bề mặt; `DropShadowEffect` hạn chế | Xem elevation |
| Icon | SVG qua `SvgIcon` hoặc font | Xem iconography |
| Motion | `Storyboard`, `KeySpline` | Xem animation |
| Component | `ControlTemplate`, `Style`, `DataTemplate` | Xem controls-and-components |
| Focus | `FocusVisualStyle`, trigger | Xem accessibility |

## 4. Hiện trạng dự án (đo ngày 2026-11-27)

- Đích: `net10.0-windows`, WPF. SDK cài: 8.0.100 và 11.0.100; runtime WindowsDesktop 6.0.25, 8.0.0, 8.0.34, **10.0.15**, 11.0.0.
- Thư viện giao diện: **không có**. Package duy nhất là `PDFtoImage` 5.4.0 (QuickScan).
- MVVM: tự viết (`RelayCommand`, `AsyncRelayCommand`) trong QuickScan; ViewModels: `ScanPageViewModel`,
  `ScannerSettingsViewModel`... Shell dùng code-behind (`MainWindow.xaml.cs`, `DashboardSettingsWindow.xaml.cs`).
  Module Notes và WeatherCalendar mới chỉ có lớp module, chưa có XAML.
- Theme: `ThemePreference` (Light/Dark/System) trong Contracts; `App.ApplyTheme` → `ThemeManager.SetTheme(AppTheme)`
  (BizNext.DesignSystem). Ánh xạ sang `ThemeMode` là 1:1.
- Token: `BizNext.DesignSystem/Themes/DesignTokens.xaml` (brush tên `XxxBrush`), `DarkTheme.xaml`; không còn hex cứng trong XAML
  ngoài tệp token (0 vị trí khi kiểm tra).
- Component dùng lại: SectionCard, FieldLabel, PrimaryButton, SecondaryButton, StatusBarButton, InputTextBox, DataGrid thư viện;
  ở Shell: SidebarNavigationButton, SidebarActionButton, DashboardModuleCard; `SvgIcon` trong QuickScan.

## 5. Xung đột với mã nguồn hiện tại

| Hạng mục | Hiện tại | Fluent/Windows | Mức độ |
|---|---|---|---|
| Bo góc control | 8 | 4 `[WIN]` | Cao, nhìn thấy ở mọi nơi |
| Bo góc card | 12 | overlay 8; card không có giá trị riêng | Trung bình |
| Chữ | 10/11/12px | tối thiểu 12 Regular / 14 Semibold `[WIN]` | Cao |
| `TextMuted` trên trắng | 3.97:1 | ≥ 4.5 `[F2]` | Cao (truy cập) |
| `TextIcon` trên trắng | 2.99:1 | ≥ 3 `[F2]` | Thấp |
| Nền | Xám xanh (#F5F7FA) | Neutral xám trung tính (#FAFAFA/#F5F5F5) `[TOKENS]` | Quyết định thẩm mỹ |
| Màu brand | #356AE6 | Brand tùy sản phẩm `[F2]`; web mặc định #0078D4 | Quyết định chủ dự án |
| Vòng focus | `IsKeyboardFocused` + `FocusVisualStyle=null` | Tách bàn phím/chuột `[F2]` | Trung bình |
| Disabled | `Opacity` 0.5 | GrayText ở High Contrast `[WIN]` | Trung bình |
| Mật độ | compact 36/34/38 | Fluent 2 không có khái niệm mật độ | Ghi nhận là ngoại lệ |

## 6. Điều kiện trước Giai đoạn 3

1. Solution phải **build và chạy** được. Đợt tái cấu trúc DesignSystem trước đó chưa được build/chạy; kiểm tra tĩnh cho thấy
   `InputComboBox`, `MainNavigationTab`, `SettingsThemeOption` (và converter `ComboBoxSelectedItemTextConverter`) chỉ còn
   trong `Shell/Themes/ControlStyles.xaml`, tệp không còn được nạp. Phải sửa trước khi thêm token mới.
2. Môi trường build: `dotnet build` hiện lỗi restore NuGet ở mọi project (xem báo cáo phiên làm việc).
3. Chủ dự án chọn A hay B (mục 2), màu brand, và cách xử lý cỡ chữ/bo góc (mục 5).
4. Spike kiểm chứng ba điều ở mục 2.

## Lỗi cần tránh

- Sao chép mã React/CSS/Web Components; dùng token web như quy định cho WPF.
- Bật cờ thử nghiệm `WPF0001` trên toàn dự án mà không quyết định có chấp nhận rủi ro đổi API.
- Viết lại control WPF vốn đã có Fluent, mất hành vi bàn phím và automation.
- Thêm package giao diện vì tiện.
- Đánh dấu hoàn thành khi chưa chạy ứng dụng ở Light/Dark/High Contrast.

## Nguồn

- https://fluent2.microsoft.design/get-started/develop/
- https://fluent2.microsoft.design/get-started/design
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net90
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net100
- https://github.com/microsoft/fluentui/tree/master/packages/tokens/src
