# Mẫu điều hướng

## 1. Fluent 2 `[F2]`

- **Nav** trong roadmap: chưa bắt đầu, ETA TBD. Không có spec điều hướng chính thức trên trang Fluent 2
  (`[CHƯA TÌM THẤY]` mẫu sidebar, độ rộng, trạng thái).
- **Tabs**: Stable (web). Thanh điều hướng/tab bar dùng bo góc 0 `[F2]` shapes.
- Chuyển trang cấp cao: dùng **fade nhanh**, không trượt phần tử lớn `[F2]` motion.
- Thứ tự focus trái→phải, trên→dưới (đường chữ Z); focus không được mất khi UI tạm thời đóng `[F2]` accessibility.

## 2. Windows: NavigationView `[WIN]`

Nguồn cho điều hướng trên Windows. Control này là của WinUI, không có trong WPF; chỉ lấy quy tắc.

- Hai kiểu: pane **trái** và pane **trên**.
- Chế độ hiển thị theo bề rộng cửa sổ (mặc định): **mở rộng ≥ 1008px**, **gọn chỉ icon (LeftCompact) 641-1007px**,
  `CompactModeThresholdWidth` mặc định 640. Chế độ dưới 641 (minimal) chưa đọc kỹ trong phiên này.
- Khi dùng pane trên và cửa sổ hẹp, nên chuyển sang pane trái (minimal) thay vì gom vào menu tràn, nếu các mục ngang hàng quan trọng.
- Thành phần pane trái: nút menu, mục điều hướng, dải phân cách, header, AutoSuggestBox (tùy chọn), nút Settings (tùy chọn);
  mục đặt cuối pane qua `FooterMenuItems`.
- Chỉ **một** chỉ báo chọn: vẽ dọc **cạnh trái** (pane trái) hoặc **cạnh dưới** (pane trên). Nếu con trong cây thu gọn được chọn,
  tổ tiên hiển thị là mục chọn.
- Nút Back ở góc trên trái pane, nhưng không tự xử lý back stack.
- Hệ hai lớp: base layer chứa menu/lệnh/điều hướng; content layer chứa nội dung `[WIN]` layering.
- `[CHƯA TÌM THẤY]`: độ rộng pane mở/đóng bằng số (không trích được từ trang), chiều cao mục.

## 3. Hiện trạng BizNext

- Shell: sidebar trái rộng 236px, nhóm `RadioButton` (GroupName="Sidebar"), nút hành động ở cuối.
  Khi chọn: nền `SurfaceSelected` + chữ màu brand; không có chỉ báo cạnh.
- Mỗi module (QuickScan) có thanh tab riêng bằng `RadioButton` (style `MainNavigationTab`).
- Cửa sổ `MinWidth=1200` nên luôn ở chế độ mở rộng theo ngưỡng Windows; sidebar không cần chế độ minimal.

## 4. Quy tắc cho BizNext `[PROJECT]`

- Một sidebar duy nhất ở Shell (điều hướng giữa module); trong module dùng **Tabs** cho các khu vực ngang hàng.
  Không lồng thêm cấp điều hướng thứ ba nếu không có lý do.
- Chỉ báo chọn dạng thanh dọc ở cạnh trái mục (theo `[WIN]`), giữ nền nhẹ; icon chuyển Regular → Filled khi chọn.
- Nhóm điều hướng chính ở trên, mục phụ (Cài đặt) ở cuối (tương ứng FooterMenuItems).
- Hỗ trợ: Tab vào nhóm rồi mũi tên chuyển mục (`KeyboardNavigation.DirectionalNavigation`), Enter/Space kích hoạt;
  `AutomationProperties.Name` cho từng mục (kể cả khi chỉ còn icon).
- Chuyển giữa module: không animation trượt; fade ngắn hoặc không có (xem animation).
- Giữ nguyên trạng thái chọn khi đổi theme/kích thước.

## Lỗi cần tránh

- Bê hamburger menu kiểu web/di động làm điều hướng chính trên màn hình rộng.
- Hai thanh điều hướng cùng kiểu chồng nhau (sidebar + tab) với cùng một ý nghĩa.
- Báo mục đang chọn chỉ bằng màu chữ.
- Mất focus về đầu cây sau khi đổi trang.

## Nguồn

- https://fluent2.microsoft.design/component-roadmap/
- https://fluent2.microsoft.design/motion
- https://fluent2.microsoft.design/accessibility
- https://learn.microsoft.com/en-us/windows/apps/design/controls/navigationview
- https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/layering
