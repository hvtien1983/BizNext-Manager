# Control và component

## 1. Nguồn dữ liệu và giới hạn

Trang roadmap `[F2]` chỉ có các cột Component, Area, Phase, Status, ETA. **Không có phân tách theo nền tảng**
và không nhắc WPF; trang nhắc "components for v9" nên thực chất là thư viện web (React). Không dùng nó
để kết luận một control "đã có Fluent cho WPF".

Fluent theme của WPF `[WPF]`: có sẵn từ .NET 9; .NET 10 thêm style cho DatePicker, GridSplitter, GridView,
GroupBox, Hyperlink, Label, NavigationWindow, RichTextBox, TextBox và ghi rõ
"Fluent UI style support is still in progress". Danh sách đầy đủ control được style **chưa tìm thấy**
trong tài liệu `[CHƯA TÌM THẤY]`; cần chạy ứng dụng WPF Gallery hoặc thử nghiệm (Giai đoạn 3).

## 2. Danh sách control cần cho BizNext

| Control yêu cầu | Trạng thái Fluent 2 (web roadmap) | Thành phần Fluent 2 gần nhất | Hướng cho WPF `[PROJECT]` |
|---|---|---|---|
| Button | Stable | Button, CompoundButton, SplitButton | Template dựng lại theo token; 3 biến thể: primary, secondary (mặc định), subtle |
| TextBox | Stable | Input, TextArea | Chuyển hóa, template theo token |
| PasswordBox | Không có trên roadmap | không | Dựng lại, dùng chung khung với TextBox |
| ComboBox | Unstable | Dropdown/ComboBox, Select | Dựng lại popup; kế thừa điểm tốt từ template hiện có |
| CheckBox | Stable | Checkbox | Chuyển hóa |
| RadioButton | Stable | Radio | Chuyển hóa |
| ToggleButton | Stable | ToggleButton, Switch | Chuyển hóa; thêm `ToggleSwitch` dạng style nếu cần |
| TabControl | Stable | Tabs | Chuyển hóa |
| Navigation Menu | Nav: chưa bắt đầu, ETA TBD | không có spec | Theo WinUI NavigationView (xem navigation-patterns) |
| Card | Unstable | Card | Dựng lại, tiết chế (xem elevation) |
| Dialog | Stable | Dialog | Dựng lại; kèm quản lý focus |
| Tooltip | Stable | Tooltip | Chuyển hóa |
| ProgressBar | Unstable | ProgressIndicator, Spinner | Chuyển hóa |
| DataGrid | Unstable | Table | Dựng lại; mật độ dày là quy ước BizNext |
| Status Indicator | không có | Badge (Stable), Persona (Unstable) | Dựng lại; kết hợp icon + chữ + màu |

Các thành phần Stable khác trong roadmap: Avatar, Divider, Link, Menu, MenuButton, Popover, Slider, Label.
Unstable (target Q2): Select, Dropdown/ComboBox, Toolbar, AvatarGroup, Card, ProgressIndicator, Table,
Alert, Field, Persona; Toast unstable (target Q3). Ngày ước tính "approximate and may change".

## 3. Ma trận trạng thái bắt buộc cho mọi control `[F2]` + `[PROJECT]`

| Trạng thái | Quy tắc |
|---|---|
| Rest | Nền neutral; không bóng thừa |
| Hover | Đổi nền/viền nhẹ (chiều đổi: xem color-system, chưa chốt cho WPF) |
| Pressed | Khác hover rõ ràng; với elevation Windows giảm xuống mức 1 `[WIN]` |
| Selected/Checked | Dùng brand + tín hiệu khác ngoài màu |
| Focus (bàn phím) | Container có stroke dày hơn `[F2]`; luôn nhìn thấy, tách khỏi hover chuột |
| Disabled | Foreground/stroke token disabled; High Contrast dùng GrayText cho UI vô hiệu `[WIN]`, nên không chỉ dựa vào `Opacity` `[PROJECT]` |
| Error/Warning/Success | Màu semantic + icon + chữ; không chỉ màu |
| Busy | Hiển thị đang xử lý, chặn hành động lặp |

## 4. Quy tắc triển khai chung

- Mỗi control **một** style gốc + một vài biến thể có tên; không tạo control mới trùng chức năng.
- Template bám `TemplateBinding` cho thuộc tính cho phép tuỳ biến; màu qua `DynamicResource`.
- Giữ `ContentPresenter RecognizesAccessKey`, `FocusVisualStyle`, automation peer mặc định.
- Style theo `x:Key` rõ nghĩa theo vai trò (`Fluent.Button.Primary`), không theo màn hình.

```xaml
<Style x:Key="Fluent.Button.Primary" TargetType="Button" BasedOn="{StaticResource Fluent.Button}">
    <Setter Property="Background" Value="{DynamicResource BrandBackgroundBrush}" />
    <Setter Property="Foreground" Value="{DynamicResource OnBrandForegroundBrush}" />
</Style>
```

Tên token trong ví dụ là gợi ý, sẽ chốt ở Giai đoạn 3.

## 5. Phân loại

- **Chuyển hóa được** (ý tưởng/quy tắc dùng lại, XAML viết mới): hình dạng, spacing, typography, trạng thái.
- **Phải dựng lại bằng XAML + C#**: Card, Dialog (quản lý focus), Navigation, Status Indicator,
  ComboBox popup, DataGrid (mật độ và trạng thái).
- Không sao chép mã React/CSS/Web Components; chỉ dùng giá trị token làm tham chiếu.

## Lỗi cần tránh

- Style từng instance bằng setter tại view; hard-code màu/kích thước.
- Chỉ dùng `Opacity` cho disabled; chỉ dùng màu cho trạng thái.
- Gọi `FocusVisualStyle="{x:Null}"` rồi hiển thị focus bằng `IsKeyboardFocused`: viền focus sẽ hiện cả khi click chuột
  (xem accessibility).
- Dựng lại control mà WPF đã style tốt, làm mất hành vi bàn phím/automation.

## Nguồn

- https://fluent2.microsoft.design/component-roadmap/
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net100
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net90
