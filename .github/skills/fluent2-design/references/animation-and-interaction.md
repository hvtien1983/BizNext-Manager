# Animation và tương tác

## 1. Nguyên tắc `[F2]`

| Nguyên tắc | Ý nghĩa |
|---|---|
| Functional | Chuyển động có mục đích: chỉ bước tiếp theo, báo thay đổi UI, đánh dấu hoàn thành |
| Natural | Mô phỏng vật lý (quán tính, trọng lực, vận tốc) để dễ đoán |
| Consistent | Hành vi chuyển động chung giữa các sản phẩm |
| Appealing | Làm sản phẩm dễ nhớ, thân thiện |

Trang không đưa số mili giây, không đưa token duration/easing, không đưa curve bezier `[CHƯA TÌM THẤY]` ở mức Fluent 2 chung.
Chỉ có định tính: "fast and smooth ... without making people wait"; thời gian tăng theo kích thước và quãng đường.

Easing: linear (tốc độ đều, hợp xoay), ease-in, ease-out, ease-in-out.
Kiểu chuyển: enter/exit, elevation (thay đổi độ sâu: trạng thái nút, kéo thả), top-level (điều hướng giữa trang: fade nhanh thay vì trượt), container transform.
Biên đạo: stagger bằng độ trễ ngắn, thứ bậc quan trọng → chuyển động nổi bật và dài hơn.

## 2. Giá trị cụ thể

### Windows `[WIN]` (dùng làm giá trị tham chiếu cho WPF)

| Tên | Giá trị |
|---|---|
| ControlNormalAnimationDuration | 250ms |
| ControlFastAnimationDuration | 167ms |
| ControlFasterAnimationDuration | 83ms |

| Easing | Bezier | Dùng cho |
|---|---|---|
| Fast Out, Slow In | `cubic-bezier(0, 0, 0, 1)` | Đối tượng/UI đi **vào** cảnh |
| Slow Out, Fast In | `cubic-bezier(1, 0, 1, 1)` | Đối tượng/UI **rời** cảnh |

### Web `[TOKENS]` (không áp dụng nguyên cho WPF)

Duration: ultraFast 50, faster 100, fast 150, normal 200, gentle 250, slow 300, slower 400, ultraSlow 500 (ms).
Curve: accelerateMax `(0.9,0.1,1,0.2)`, accelerateMid `(1,0,1,1)`, accelerateMin `(0.8,0,0.78,1)`, decelerateMax `(0.1,0.9,0.2,1)`,
decelerateMid `(0,0,0,1)`, decelerateMin `(0.33,0,0.1,1)`, easyEaseMax `(0.8,0,0.2,1)`, easyEase `(0.33,0,0.67,1)`, linear `(0,0,1,1)`.
`curveDecelerateMid` và `curveAccelerateMid` trùng hai easing của Windows.

Hai bộ duration khác nhau (83/167/250 so với 100/150/200). Cho WPF chọn bộ Windows `[PROJECT]` theo nguyên tắc
"Natural on every platform".

## 3. Tương tác theo trạng thái `[WIN]`

Elevation theo trạng thái control: Rest 2, Hover 2, Pressed 1. Đổi trạng thái hover/pressed nên gần như tức thì
(≤ 83ms `[PROJECT]`); animation dài hơn dành cho bề mặt xuất hiện/biến mất.

## 4. Giảm chuyển động `[F2]`

Cung cấp thiết lập "no motion" (WCAG); giữ thời lượng ngắn; tránh nháy và chuyển động đột ngột;
giới hạn chuyển động ở phần tử đang tập trung; truyền thông tin động bằng cách khác (vùng live).
Triển khai cụ thể không có trong nguồn `[CHƯA TÌM THẤY]`.

## 5. Triển khai WPF `[PROJECT]`

- Easing bezier trong WPF dùng `KeySpline` của `SplineDoubleKeyFrame`: `(0,0,0,1)` → `KeySpline="0,0 0,1"`,
  `(1,0,1,1)` → `KeySpline="1,0 1,1"`. Phần ánh xạ này là kiến thức WPF, chưa có nguồn đã đọc; kiểm thử.
- Đặt thời lượng và easing thành tài nguyên (`Duration`) trong ResourceDictionary, không viết số rải rác.
- Animation bằng `Storyboard` trong `ControlTemplate.Triggers` (EnterActions/ExitActions) hoặc `VisualStateManager`.
- Kiểm tra `SystemParameters.ClientAreaAnimation` trước khi chạy animation không thiết yếu (cần xác minh ý nghĩa, xem accessibility).
- Chỉ animate `Opacity`, `RenderTransform`; tránh animate `Width/Height` trên cây lớn (hiệu năng; cần đo).

```xaml
<Duration x:Key="Motion.Fast">0:0:0.167</Duration>
<Storyboard x:Key="Flyout.Enter">
  <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty="Opacity">
    <SplineDoubleKeyFrame KeyTime="0:0:0.167" Value="1" KeySpline="0,0 0,1" />
  </DoubleAnimationUsingKeyFrames>
</Storyboard>
```

## Lỗi cần tránh

- Animation trang trí không có mục đích; hiệu ứng bật nảy, nhấp nháy.
- Chuyển trang bằng trượt lớn thay vì fade nhanh.
- Số thời lượng tự đặt (300ms, 350ms) ngoài bộ chuẩn.
- Bỏ qua thiết lập giảm chuyển động.
- Animation chặn thao tác (người dùng phải chờ xong mới tương tác được).

## Nguồn

- https://fluent2.microsoft.design/motion
- https://learn.microsoft.com/en-us/windows/apps/design/motion/timing-and-easing
- https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/layering
- https://github.com/microsoft/fluentui/blob/master/packages/tokens/src/global/durations.ts
- https://github.com/microsoft/fluentui/blob/master/packages/tokens/src/global/curves.ts
