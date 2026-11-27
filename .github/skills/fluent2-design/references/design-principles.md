# Nguyên tắc thiết kế Fluent 2

## 1. Nguyên tắc chính thức `[F2]`

Bốn nguyên tắc (trang `/design-principles`). Mỗi nguyên tắc còn có phần giải thích
"functional" và "emotional" mà tài liệu này **chưa ghi lại**.

| Nguyên tắc | Ý chính (trích) |
|---|---|
| Natural on every platform | Trải nghiệm "should adapt to the device you're on and should build off the familiar" |
| Built for focus | Giữ người dùng trong luồng làm việc, "inspire action, drawing you forward, simply and seamlessly" |
| One for all, all for one | Bao gồm: phản ánh "a range of perspectives and abilities" |
| Unmistakably Microsoft | Sản phẩm "should feel like one Microsoft" |

Trang chủ Fluent 2 **không liệt kê nguyên tắc nào**; chỉ trang `/design-principles` có.

## 2. Token: hai tầng `[F2]`

Trang `/design-tokens` mô tả **hai** tầng:

- **Global token**: giá trị thô, không gắn ngữ cảnh (hex màu, và dữ liệu cho typography,
  border radius, stroke width, animation).
- **Alias token**: thêm ý nghĩa ngữ nghĩa, tên "immediately recognizable" theo chức năng.
  Với shadow/typography, alias gộp nhiều giá trị thành một mục.

`[CHƯA TÌM THẤY]` Tầng **component token** không được nhắc trên trang. Nếu BizNext cần
token theo từng control, đó là quyết định `[PROJECT]`.

Token hỗ trợ theme sáng, tối, high-contrast và brand `[F2]`.

## 3. Cách áp dụng vào BizNext `[PROJECT]`

Phần này là diễn giải của dự án, không phải quy định Microsoft.

| Nguyên tắc | Quy tắc cho màn hình desktop |
|---|---|
| Natural on every platform | Theo quy ước Windows (4/8px bo góc, Segoe UI Variable, NavigationView), không bê giao diện web. |
| Built for focus | Một hành động chính nổi bật mỗi màn hình; ít trang trí; trạng thái rõ ràng. |
| One for all | Bàn phím đầy đủ, tương phản đạt chuẩn, High Contrast, văn bản đọc được khi phóng to. |
| Unmistakably Microsoft | Dùng chung token và hình dạng thay vì style riêng từng module. |

## 4. Phân biệt ba tầng

1. **Nguyên tắc thiết kế**: bốn nguyên tắc trên, độc lập nền tảng.
2. **Quy chuẩn từng nền tảng**: Fluent 2 có bộ UI kit/thư viện cho Web (React, Web
   Components), iOS, Android; Windows trỏ tới WinUI `[F2]`. WPF **không** được nhắc.
3. **Chuyển hóa sang WPF**: xem [wpf-adaptation](./wpf-adaptation.md).

## Lỗi cần tránh

- Gọi một con số của web (ví dụ `borderRadiusLarge` 6px) là "chuẩn Fluent cho WPF".
- Dùng "Fluent" như nhãn thẩm mỹ chung chung (acrylic, blur, gradient) rồi bỏ qua nguyên tắc focus.
- Tự đặt thêm tầng token rồi gọi là chuẩn Microsoft.

## Nguồn

- https://fluent2.microsoft.design/design-principles
- https://fluent2.microsoft.design/design-tokens/
- https://fluent2.microsoft.design/get-started/develop/
- https://fluent2.microsoft.design/ (không liệt kê nguyên tắc)
