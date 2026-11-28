# Kế hoạch tái cấu trúc BizNext Manager theo phương án B

> Tài liệu tổng hợp để dùng trong các phiên làm việc sau. Cập nhật lần đầu
> ngày 2026-11-27 theo mã nguồn và kết quả kiểm thử tại thời điểm đó.
> Đây là lộ trình, **không phải lệnh tự động triển khai tất cả các bước**.

## 1. Nguồn và cách sử dụng

- [Đề xuất tái cấu trúc Dashboard, Modules, Widgets](<../Prompt/Đề xuất tái cấu trúc BizNext Manager Dashboard, Modules, Widgets.md>):
  kiến trúc, các cập nhật v1.1–v1.4 và tổng hợp v2.0.
- [Bộ prompt Chặng A: bản Free ba tầng](<../Prompt/Bộ prompt Chặng A BizNext Manager bản Free ba tầng.md>):
  công việc và tiêu chí kiểm thử chi tiết cho bản ngoại tuyến.
- [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md), [kiến trúc hiện tại](./docs/architecture.md),
  [baseline](./docs/baseline.md) và [smoke test](./docs/smoke-test.md):
  hiện trạng và bằng chứng trong repository.

Phương án B đã được chọn: **tái cấu trúc từng bước, ưu tiên offline-first,
giữ dữ liệu và hành vi QuickScan, tái sử dụng BizNext.DesignSystem**. Kế hoạch
chi tiết bước 1 đã được duyệt để làm **1A trước**; các nhịp tiếp theo phải
được nghiệm thu và cho phép thực hiện riêng. Các bước còn lại trong tài liệu
này là **kế hoạch tham khảo**, không mặc nhiên là phê duyệt triển khai hoặc
phê duyệt mọi quyết định chi tiết.

Trước mỗi nhịp: đọc tài liệu này và trạng thái Git, đối chiếu mã thực tế, lập
kế hoạch file sẽ thay đổi, xác nhận các lựa chọn còn mở, rồi mới triển khai.
Sau mỗi nhịp: build, test, smoke test phù hợp và cập nhật tài liệu; báo rõ
đạt, không đạt hoặc chưa kiểm chứng. Không tự chuyển nhịp, commit, tag, push
hay di trú dữ liệu nếu chưa được yêu cầu hoặc phê duyệt.

## 2. Hiện trạng và tiến độ

Ảnh chụp ngày 2026-11-27:

| Hạng mục | Trạng thái |
| --- | --- |
| Mã nền | Nhánh `refactor/three-tier`, bắt đầu từ commit `42860dc`; thay đổi bước 0 và 1A đang ở working tree, **chưa commit**. |
| Bước 0 | Build Debug/Release baseline không lỗi; test baseline 13/13, sau bổ sung test JSON 14/14. Người dùng báo 15/15 mục smoke đạt trên môi trường thử. Chi tiết và giới hạn xem [baseline](./docs/baseline.md). |
| Bước 1A theo phương án B | Đã thêm `Microsoft.Extensions.DependencyInjection` và composition root `ShellComposition`; đăng ký module, catalog, navigation, settings và cửa sổ. Build Release và 14/14 test đạt; người dùng báo smoke đạt lại. Chưa có Generic Host hoặc `IAppPaths`. |
| Bước 1B, 1C và sau đó | Chưa triển khai. Không nhầm 1B/1C trong tài liệu này với 1B trong bộ prompt gốc. |
| Git baseline | Nhánh đã tạo; tag `baseline-before-refactor` chưa có. Không có `QuickScanManager/` ở gốc repo được Git theo dõi; quy tắc ignore riêng cho thư mục đó chưa được bổ sung. Đây là các mục của prompt gốc, không tự coi là đã hoàn thành. |

Build baseline là build tăng dần; số cảnh báo chỉ phản ánh lượt build đó.
Smoke test là kết quả người dùng cung cấp, không phải toàn bộ đều được xác minh
độc lập bằng tự động hóa. Sau thời điểm ghi nhận phải kiểm tra lại trạng thái,
không xem số test hay nhánh ở bảng này là luôn đúng.

Hiện nay Shell có Home/Dashboard tĩnh và đăng ký ba module tĩnh; sidebar và
thẻ Home còn khai báo cứng. `IBizNextModule` cung cấp metadata và `CreateView`
WPF; view được tạo khi cần và cache. QuickScan là module hoạt động, Notes và
WeatherCalendar mới là placeholder. Test nằm trong `tests/BizNext.Tests` và
`tests/BizNext.TestHost`, không cần dựng lại một bộ project test theo tên cũ.

## 3. Mục tiêu kiến trúc

### 3.1 Ba tầng

1. **Main Dashboard / Shell:** composition root, điều hướng, Home, cài đặt,
   theme, thông báo, vòng đời module/widget và bố cục. Những dịch vụ tài khoản,
   workspace và cloud chỉ xuất hiện ở chặng sau khi được duyệt.
2. **Modules:** QuickScan, Notes, WeatherCalendar và các tiện ích sau này.
   Mỗi module sở hữu chức năng, view, dịch vụ và widget riêng; không tham chiếu
   Shell hay module khác. Tích hợp chéo dùng contract hoặc thông điệp chung.
3. **Widgets:** widget độc lập và widget thuộc module, chạy trong Home; host
   quản lý vòng đời, kích thước, trạng thái và cô lập lỗi.

Chiều phụ thuộc: Shell lắp ghép module; module phụ thuộc contract, DesignSystem
và thư viện lõi phù hợp, không phụ thuộc Shell. Composition tiếp tục là tĩnh;
chưa thiết kế plugin động hay cho bên thứ ba chạy mã.

### 3.2 Cấu trúc solution đích dự kiến

Tên và vị trí dưới đây là mục tiêu thiết kế, **không khẳng định đã tồn tại**:

```text
BizNext-Manager.sln
src/
  BizNext.Contracts/                 # contract/DTO không WPF, khi được tách
  BizNext.Contracts.Wpf/             # khả năng và view WPF, nếu được duyệt
  BizNext.Infrastructure/            # JSON, catalog, layout, SQLite, paths
  BizNext.DesignSystem/              # token, theme, control và WidgetCard dùng chung
  BizNext.Shell/                     # App, DI, Dashboard, navigation, host, settings
  BizNext.Modules.QuickScan/          # chức năng hiện có; Widgets/ về sau
  BizNext.Modules.Notes/              # Notes offline; Widgets/ về sau
  BizNext.Modules.WeatherCalendar/    # hiện là placeholder
  BizNext.Widgets.Standard/           # widget độc lập, dự kiến
  ScanCore/  ScanDevice/             # thư viện hỗ trợ QuickScan
tests/
  BizNext.Tests/                     # xUnit: unit, integration, WPF STA
  BizNext.TestHost/                  # preview thành phần an toàn
docs/
  architecture.md  baseline.md  smoke-test.md
```

Server/API, kho blob và công cụ admin là **chặng sau**, chưa có trong solution.
Không tạo lại `BizNext.UI`: `BizNext.DesignSystem` đã là nơi sở hữu UI dùng
chung. Chỉ cân nhắc tách `Contracts.Wpf` khi kế hoạch chuyển đổi được duyệt;
contract hiện tại vẫn dùng WPF. Một contract không WPF không được chứa
`FrameworkElement` hay kiểu WPF; DTO về kích thước widget phải đặt ở lớp dùng
chung nếu contract phi WPF cần nó.

### 3.3 Đường dẫn và dữ liệu

| Dữ liệu | Hiện tại / nguyên tắc |
| --- | --- |
| Shell | `%LocalAppData%\BizNext-Manager\shell-settings.json`; giữ vị trí và định dạng cũ. |
| QuickScan | `%LocalAppData%\QuickScanManager\` cho cài đặt, profile, metadata và ảnh tạm; PDF có thể nằm ở thư mục người dùng đã chọn. Giữ nguyên, không di chuyển hay ghi đè. |
| Fake scanner | Chỉ tách thiết bị và một số cấu hình; **không cô lập thư viện/PDF**. Chỉ smoke test trong môi trường thử độc lập đã xác minh. |
| Bố cục/SQLite mới | Đề xuất dưới `%LocalAppData%\BizNext-Manager\users\local\`; chính xác đường dẫn và migration phải được duyệt ở bước tương ứng. |
| Tài nguyên file | Đề xuất root đổi được, ví dụ `Documents\BizNext\Personal\{module}\...`; thư mục QuickScan có sẵn chỉ liên kết, không tự di chuyển. Mặc định và phạm vi áp dụng cần duyệt. |

`IAppPaths` và `IResourceService` là kế hoạch sau, không có nghĩa mọi đường
dẫn QuickScan đã được chuyển. Phân biệt file do người dùng sở hữu với cache
do ứng dụng tạo. Không tự đổi tên, xóa, thay thế hoặc di trú dữ liệu cũ.
Khi dữ liệu JSON/SQLite hỏng: giữ bản gốc và các file liên quan, báo lỗi;
không tự tạo dữ liệu rỗng thành công giả hoặc di chuyển file để “sửa” khi
chưa có quy trình và sự đồng ý.

## 4. Quy tắc xuyên suốt

- Ứng dụng vẫn chạy ở cuối từng nhịp; không đổi logic QuickScan, ScanCore,
  ScanDevice hoặc định dạng dữ liệu ngoài điểm hook đã mô tả và được duyệt.
- Giữ nguyên các ID `biznext.module.quickscan`, `biznext.module.notes`,
  `biznext.module.weathercalendar`, module cuối đã lưu và theme Light/Dark/System.
- Dùng `BizNext.DesignSystem`, semantic token và `DynamicResource`, hỗ trợ
  bàn phím, focus và screen reader. Không đưa style dùng chung mới vào module.
- Chặng A chạy offline: không thêm login, API, cloud giả hoặc lời gọi mạng cho
  tính năng bản Free. Thêm package chỉ sau khi xác định nhu cầu và được duyệt.
- Đăng ký dịch vụ trước khi dựng container; metadata/catalog không tạo view.
  Logic WPF và khởi tạo view chạy trên Dispatcher; lỗi phải được log và thông
  báo, không làm mất dữ liệu hoặc trả kết quả thành công giả.
- Test file thật phải dùng thư mục tạm hoặc hồ sơ thử độc lập; không dùng
  scanner thật hay thư viện QuickScan thật. Không tự đổi tên dữ liệu thật
  để giả lập máy sạch.
- Duy trì compatibility, schema version và test dữ liệu cũ. Lưu nguyên tử
  không đồng nghĩa đã kiểm chứng mọi sự cố hệ thống.
- Cập nhật `PROJECT_CONTEXT.md`, `docs/architecture.md`, README và smoke test
  khi liên quan. Mỗi kết quả ghi rõ môi trường, ngày và giới hạn.

## 5. Chặng A — bản Free ba tầng, ngoại tuyến

Số bước bên dưới theo **phương án B đã trình bày**, không áp dụng máy móc số
bước trong bộ prompt gốc. Mỗi mục nhỏ có thể cần một kế hoạch và lần duyệt riêng.

### Bước 0 — Baseline và lưới an toàn

**Đã thực hiện phần kiểm thử** như mục 2; không tạo lại hạ tầng.

1. Kiểm tra catalog: ID rỗng/trùng, tìm ID, thứ tự đăng ký; navigation: sự
   kiện, ID sai và lựa chọn cũ. Kiểm tra JSON: file thiếu, đọc/ghi/ghi đè,
   lỗi JSON và lỗi trước thay thế file.
2. Ghi build Debug/Release, số test và các giới hạn trong baseline.
3. Chạy checklist QuickScan, theme, module cuối và bàn phím bằng fake scanner
   và dữ liệu thử độc lập; không suy luận GUI đạt chỉ từ build.
4. Kiểm tra Git và ignore. Tag baseline, quy tắc ignore riêng, commit/tag
   nghiệm thu chỉ thực hiện khi có yêu cầu rõ; không xóa dữ liệu ngoài repo.

**Nghiệm thu:** test xanh, hành vi được ghi nhận, smoke test đạt và giới hạn
được công khai. Bằng chứng ban đầu tại [baseline](./docs/baseline.md).

### Bước 1 — Composition và module host

#### 1A. Composition root và DI — đã thực hiện

- Shell đăng ký tĩnh ba module và dịch vụ bằng DI, vẫn đọc settings cũ trước
  khi dựng composition; giải phóng container khi ứng dụng thoát.
- Tiêm cùng catalog/navigation vào MainWindow; giữ ID, thứ tự, đường dẫn và
  cache view. Không áp dụng Generic Host hay `IAppPaths` trong nhịp đã duyệt.
- Test thành phần DI, module cuối, cache QuickScan; build và smoke như mục 2.

#### 1B. Điều hướng và catalog theo dữ liệu — kế tiếp, chưa triển khai

1. Chuyển sidebar và thẻ Home từ danh sách cứng sang dữ liệu catalog; Dashboard
   vẫn là đích riêng. Giữ thứ tự, tên, biểu tượng và hành vi hiện tại.
2. Dùng ID ổn định để chọn, hiển thị và lưu; xử lý module không đăng ký một
   cách rõ ràng. Catalog không tạo module view chỉ để hiển thị metadata.
3. Test danh sách và lựa chọn, ID không hợp lệ, Home, module cuối, truy cập
   bàn phím và mở lại; smoke QuickScan trước khi nghiệm thu.

**Nghiệm thu:** UI và điều hướng hiện hữu không hồi quy, không mất trạng thái
QuickScan, cấu hình cũ vẫn mở đúng module.

#### 1C. Vòng đời và lỗi module — chưa triển khai

1. Thiết kế contract vòng đời **tùy chọn** hoặc adapter cho module cũ; không
   ép viết lại nghiệp vụ QuickScan. Đăng ký dịch vụ trước khi dựng container.
2. Host khởi tạo trễ, tối đa một lần cho cùng yêu cầu hợp lệ; đồng bộ hóa
   yêu cầu đồng thời. View tạo trên Dispatcher và cache đúng chính sách hiện có.
3. Định nghĩa rõ trạng thái, hủy, retry, disposal và xử lý điều hướng cũ kết
   thúc muộn. Lỗi khởi tạo/tạo view được log, thông báo, không lưu lựa chọn
   thất bại hay làm hỏng module khác.
4. Test module giả: đồng thời, lỗi, retry, điều hướng cạnh tranh, lazy-load,
   cache và tắt; kiểm tra QuickScan giữ phiên quét.

**Nghiệm thu:** một module lỗi không làm sập Shell; ID cũ, module cuối và
QuickScan hoạt động. Chọn chính xác contract/adapter, retry và thay đổi
`Contracts`/`Contracts.Wpf` trong kế hoạch riêng trước khi sửa.

#### Công việc nền bổ sung từ prompt gốc — chưa mặc nhiên duyệt

`IAppPaths`, Generic Host, tách contract khỏi WPF và `ModuleManifest` có thể
hữu ích, nhưng chưa thuộc 1A đã làm. Trước khi áp dụng phải thiết kế chuyển
tiếp, test đường dẫn và chỉ thay chỗ cần thiết; không đổi đường dẫn thực tế.
Đặc biệt, yêu cầu “mọi chỗ dùng IAppPaths” sẽ chạm QuickScan/ScanDevice và
cần được duyệt riêng, không thể giả định là không sửa các module đó.

### Bước 2 — Dashboard và khung widget

#### 2A. Nền UI, theme và điều hướng sâu

1. Tái dùng `BizNext.DesignSystem`; nếu cần bổ sung `WidgetCard` tại đây với
   các trạng thái nội dung, đang tải, lỗi và khóa có icon/text, keyboard focus.
2. Đề xuất `IThemeService` và dịch vụ điều hướng có `NavigationRequest`
   (`ModuleId`, route, tham số), bọc điều hướng hiện có; không đổi resource key.
3. Giữ `shell-settings.json` cũ, theme hiện tại, xử lý module/route không có
   bằng thông báo; test chuyển theme và tài nguyên WPF trong cả Shell/QuickScan.

#### 2B. Hợp đồng, registry và lưu bố cục

1. Định nghĩa `WidgetDescriptor` (ID, tiêu đề, loại, chủ module, kích thước
   hỗ trợ), `WidgetInstance` (instance ID, vị trí, size, settings) và
   `DashboardLayout` có schema version. Widget có thể có nhiều instance.
2. Quy định standalone/module-bound, `ActivateAsync`, `RefreshAsync`,
   `Deactivate` và `Dispose`; định nghĩa ownership, hủy tác vụ và xử lý lỗi.
3. Registry kiểm tra ID trùng; layout store đọc/ghi JSON nguyên tử, lưu đúng
   scope. Layout có provider thiếu phải được **giữ lại**, không tự xóa.
4. Test round-trip, schema không hỗ trợ, file hỏng, placement và va chạm;
   không overwrite bố cục không hiểu hoặc dữ liệu hỏng.

#### 2C. Host, Home và widget chuẩn

1. `WidgetHost` hiển thị Loading/Content/Error, retry và cô lập lỗi từng thẻ;
   xử lý activate đang chạy khi Home bị ẩn, dừng timer và dispose đúng lúc.
2. Lưới ô cố định Small 1×1, Wide 2×1, Tall 1×2, Large 2×2; thư viện widget,
   thêm/xóa/đổi cỡ/di chuyển bằng nút, lưu và khôi phục bố cục. Kéo thả tự do
   không cần ở MVP; thao tác xóa widget có xác nhận phù hợp.
3. Widget chuẩn dự kiến: Đồng hồ, Lối tắt module, Thẻ chào/ngày. Đồng hồ dùng
   clock giả trong test và dừng timer khi ẩn; lối tắt dùng navigation chung.
4. Dùng dữ liệu tạm và widget giả kiểm tra lỗi, retry, không chồng lấn, theme,
   bàn phím, layout giữ qua restart và provider vắng mặt.

#### 2D. Cài đặt và cache nâng cao

Đề xuất trang Chung/Giao diện/Module và khả năng cung cấp trang cài đặt. Giữ
cửa sổ QuickScan hiện tại cho đến khi có cổng cài đặt an toàn và test lưu/hủy/
lỗi/cập nhật view; không tạo hai nguồn trạng thái. Xem xét `KeepAlive` và
`DisposeOnLeave` cho module phù hợp, QuickScan tiếp tục giữ view và phiên.
Không chuyển cài đặt QuickScan chỉ vì prompt gốc đề xuất wrapper.

**Nghiệm thu bước 2:** Home chỉnh sửa và lưu được, widget lỗi không ảnh hưởng
widget khác, theme và QuickScan không hồi quy. Không hardcode entitlement theo
tên gói; hạn mức bản offline chỉ xác định sau khi được duyệt.

### Bước 3 — Widget QuickScan

1. Thêm provider module-bound: **Quét nhanh** mở route scan; **Tài liệu gần
   đây** hiển thị tối đa 5 PDF gần nhất và mở đúng tài liệu khi chọn.
2. Tái sử dụng quy tắc duyệt thư mục thực tế và metadata hiện có; không coi
   `library.json` là toàn bộ thư viện. Đọc qua cổng dịch vụ phù hợp, không đổi
   schema hoặc nơi lưu.
3. Thêm hook điều hướng và thông điệp khi xuất/đổi tên/xóa nếu được duyệt;
   kiểm tra file mất, không truy cập được, lỗi metadata và refresh/debounce.
4. Test bằng thư viện giả và dữ liệu tạm; smoke quét, tạo, đổi tên và mở PDF
   chỉ trong môi trường thử, không dùng scanner hoặc thư viện thật.

**Nghiệm thu:** widget cập nhật đúng, điều hướng đúng, QuickScan nguyên trạng
và không mất dữ liệu.

### Bước 4 — SQLite local, Notes và widget Notes

#### 4A. Storage local

1. Đề xuất `Microsoft.Data.Sqlite`, database riêng cho local, migration version,
   WAL và foreign keys; xác định scope/module ID và cách cấu hình đường dẫn.
2. Model `LocalItems` với ID, scope, module, payload/schema version, kích thước
   UTF-8, revision, mutation ID, server version, trạng thái sync, soft-delete
   và timestamp; `SyncCursors` chỉ chuẩn bị cho tương lai nếu có lý do.
3. `IModuleStorage.For(module)` cô lập dữ liệu module; ghi theo transaction,
   `LocalRevision` tăng, `ClientMutationId` mới, trạng thái `LocalOnly`.
   Không có sync hoặc network trong Chặng A.
4. Test CRUD, xóa mềm, scope, ghi đồng thời, migration chạy lại, tiếng Việt
   và lỗi lưu. Không tự đổi tên DB hỏng, WAL/SHM hoặc tạo DB rỗng; báo lỗi và
   chỉ phục hồi theo quy trình được duyệt.

#### 4B. Notes

1. Model note: ID, tiêu đề, nội dung, tag, ghim, ngày tạo/sửa. Service chỉ
   dùng `IModuleStorage`, không truy cập SQLite trực tiếp.
2. Danh sách, soạn, tìm/lọc, ghim, xóa có xác nhận hoặc hoàn tác; tự lưu có
   debounce và flush an toàn khi chuyển note/đóng ứng dụng.
3. Thông điệp thay đổi và route `note/{id}`; test CRUD, tìm kiếm tiếng Việt,
   autosave/đóng ứng dụng, lỗi ghi và độ mượt với dữ liệu thử khoảng 2.000 note.

#### 4C. Widget Notes

Ghi chú ghim tối đa 5 mục, mở đúng note; Ghi nhanh kiểm tra nội dung rỗng,
tạo note qua service và thông báo. Cập nhật khi note thay đổi, widget hoạt
động trước khi mở view Notes, hủy subscription khi deactivate/dispose. Test
refresh, title, route, lỗi lưu và rò rỉ.

**Nghiệm thu bước 4:** Notes dùng được offline, dữ liệu bền vững qua restart,
widget cập nhật, dữ liệu module cô lập, không có mạng.

### Bước 5 — Tài nguyên và liên kết thư mục

1. Thiết kế `ResourceLayout` theo module và `IResourceService`, root đổi được,
   quản lý Personal/module và liên kết thư mục có sẵn; chỉ tạo khi thực sự cần.
2. Kiểm tra path traversal, đường dẫn tuyệt đối, junction/symlink, quyền truy
   cập và đường dẫn bị mất. Không tự di chuyển hay xóa file liên kết.
3. Trang Tài nguyên hiển thị root, thư mục và dung lượng tính bất đồng bộ;
   đổi root chỉ áp dụng theo chính sách người dùng đã duyệt.
4. Với QuickScan đã có `DefaultPdfDirectory`, giữ nguyên. Việc đổi mặc định
   cho người dùng mới là thay đổi hành vi và cần duyệt riêng trước khi làm.
5. Test liên kết/đổi root, path an toàn, lỗi truy cập và tính tương thích.

**Nghiệm thu:** dữ liệu cũ giữ nguyên, không có file bị tự di chuyển/xóa;
đường dẫn và quyền truy cập được xử lý rõ ràng.

### Nghiệm thu cuối Chặng A

- Build Release và test toàn solution; đối chiếu cảnh báo với baseline và ghi
  rõ nếu là build tăng dần hay rebuild.
- Chạy smoke đầy đủ trên môi trường thử sạch **cô lập**, rồi kiểm tra tương
  thích dữ liệu cũ theo quy trình được người dùng cho phép. Không tự đổi tên
  `%LocalAppData%` thật để tạo máy sạch.
- Kiểm tra QuickScan, theme, layout, Notes, dữ liệu qua restart, accessibility,
  không có network trong chức năng offline; publish `win-x64` self-contained
  và thử chạy bản publish khi được yêu cầu.
- Cập nhật tài liệu kiến trúc, context, README; nghiệm thu từng nhịp. Prompt
  gốc đề xuất dùng thử 3–5 ngày trước chặng server; đây là khuyến nghị.

## 6. Chặng B–F — định hướng sau bản Free

Các chặng này **không triển khai trong Chặng A**. Cần đặc tả, đánh giá bảo
mật, chính sách và phê duyệt riêng trước từng chặng. “Đã chốt trong tài liệu
nguồn” dưới đây là lịch sử của tài liệu đó, không tự mở rộng phạm vi phương
án B đã được phép triển khai.

### B. Tài khoản và quyền lợi

- Server auth, đăng ký/đăng nhập, refresh token, quản lý thiết bị và preferences;
  client có session, workspace và cache quyền lợi offline.
- `EntitlementDefinitions`, `PlanVersions`, `PlanVersionEntitlements`,
  overrides và resolver có hiệu lực theo thời gian; client hiển thị cache,
  **server luôn kiểm quyền**. Giá trị là cấu hình, không hardcode theo tên gói;
  `IsUnlimited` phải tường minh.
- `ModuleAccessState`/màn hình khóa chỉ là UX; người dùng vẫn được truy cập
  dữ liệu của mình theo chính sách được duyệt. Cần test hết hạn cache và
  chuyển tài khoản/workspace không trộn dữ liệu.

### C. Đồng bộ item và vòng đời gói

- Push/pull có cursor, phân trang, idempotency, `LocalRevision`, xung đột,
  retry và khôi phục khi mất mạng; kiểm quyền/quota nguyên tử ở server.
- Theo nguồn v1.3: nâng gói có hiệu lực ngay; hủy dùng đến hết kỳ rồi về Free;
  gia hạn lỗi có ân hạn. Chuyển đổi `LocalOnly` sang chờ sync chỉ sau khi chốt
  cách ánh xạ user/scope và chính sách dữ liệu.
- Test đồng thời, thay đổi local lúc push, cursor, xử lý xung đột và hết hạn.

### D. File cloud và lưu lượng

- File local là bản gốc; upload **thủ công**, không tự đồng bộ hai chiều.
  `LocalFiles`, metadata server, `IBlobStore` và `ITransferService` quản lý
  hàng đợi, upload theo đoạn, resume, download range, kiểm hash.
- Server kiểm quyền, file size, quota và đặt chỗ nguyên tử; chunk phải
  idempotent. Hash trùng không được làm mất danh tính, đường dẫn hay quyền
  của file. Xóa local không mặc định xóa cloud.
- Hai bộ đếm độc lập tải lên/tải về theo tháng; nguồn v1.4 nêu múi giờ
  `Asia/Ho_Chi_Minh` có thể cấu hình. Test đồng thời, quota, resume, reset,
  file thay đổi giữa upload và dọn đặt chỗ hết hạn.

### E. Hạ gói và giữ dữ liệu

- Tính quyền lợi Free tại thời điểm hiện tại; chỉ phần vượt quyền lợi hoặc
  module không được cấp bị giới hạn. Tăng quyền lợi có thể hiệu lực ngay;
  giảm cần chính sách thông báo và xem trước tác động.
- Thiết kế `OverQuota`, ân hạn, quyền xem/tải/xuất và thông báo trước thời
  hạn. **Chưa tự động xóa cloud hay cache**: cần người dùng duyệt chính sách
  chính xác, phạm vi, thời gian, quyền và cơ chế phục hồi. Không xóa file local
  do người dùng sở hữu.

### F. Admin và tổ chức

- Admin là ứng dụng riêng, phân quyền và audit ở server; nguồn đề xuất ba nhóm
  `PlatformOwner`, `PlatformAdmin`, `PlatformStaff`, MFA, ghi vết và nhân viên
  chỉ xem/đề xuất. Tên, phạm vi và quy trình duyệt phải được xác nhận.
- `PlanVersions.AppliesTo`: áp dụng cho tất cả hoặc chỉ subscription phát sinh;
  chính sách gia hạn liên tục và khôi phục gói cần kiểm thử theo quyết định
  cuối cùng. Không dùng đồng thời chính sách `PricingPolicy` cũ.
- Tổ chức: owner/admin/member/viewer, lời mời, workspace, quota chung và giới
  hạn thành viên. Tách quyền tổ chức khỏi quyền admin nền tảng, không suy ra
  quyền xóa cache hoặc dữ liệu local khi mất tư cách thành viên.
- Test chống tự nâng quyền, chủ nền tảng cuối cùng, proposal/approval, phân
  vùng dữ liệu, quota, thu hồi quyền, backup và khôi phục.

Weather/Calendar có chức năng thật, API thời tiết và widget tương ứng là công
việc sau riêng; hiện vẫn chỉ là placeholder.

## 7. Khác biệt giữa hai nguồn và quyết định còn mở

| Chủ đề | Cách xử lý trong kế hoạch này |
| --- | --- |
| Số bước | Bước 1B ở đây là điều hướng theo dữ liệu; prompt gốc 1B là tách contract/lifecycle. Các bước 2–5 ở đây nhóm lại nội dung prompt 2A–4E. Không áp dụng nhầm checklist theo tên. |
| Design system | Nguồn cũ đề xuất tạo `BizNext.UI`; repo đã có `BizNext.DesignSystem`, nên tái sử dụng, không sao chép/tách lại. |
| Host và paths | Prompt yêu cầu Generic Host và `IAppPaths` ngay; 1A đã duyệt chỉ dùng DI, đường dẫn giữ nguyên. Host/paths là quyết định tiếp theo. |
| Contract module | Prompt gốc thay interface cả ba module; đề xuất khác dùng adapter. Phương án B ưu tiên chuyển tiếp và vòng đời tùy chọn; lựa chọn cụ thể cần duyệt khi làm. |
| Dữ liệu lỗi | Prompt gốc đề xuất sao lưu layout hỏng rồi trả rỗng, đổi tên DB hỏng rồi tạo DB mới; không tự làm vì có rủi ro mất/ghi đè dữ liệu. Báo lỗi, giữ nguyên và xin duyệt quy trình phục hồi. |
| Layout widget | Giữ instance/provider thiếu và phiên bản không hiểu, không silently bỏ rồi ghi đè. Cần đặc tả hiển thị/khôi phục trước khi triển khai. |
| QuickScan settings | Wrapper trong prompt không chứng minh an toàn; giữ cửa sổ cũ đến khi có cổng dữ liệu, test và nghiệm thu riêng. |
| Thư viện PDF | Không chỉ đọc `library.json`; cần tôn trọng thư mục PDF cấu hình, file trên đĩa và metadata thực tế. |
| File local/cache | Nguồn vừa nói không xóa local vừa nói xóa cache khi mất quyền tổ chức. Cần phân loại và chốt chính sách trước hành động có thể xóa. |
| Phiên bản quyền lợi | v1.4 thay `FollowLatest`/`Pinned` bằng `AppliesTo`; v1.3 thay meter ngày/gộp bằng hai meter tháng. Không triển khai đồng thời các bản cũ. |

Các câu hỏi cần chốt ở giai đoạn liên quan:

1. Chọn adapter hay thay interface, có tách `Contracts.Wpf`, dùng Generic Host
   và `IAppPaths` ở thời điểm nào; giới hạn thay đổi QuickScan.
2. Chính sách widget mặc định, kích thước lưới, số widget, persistence,
   lỗi và retry; chính sách cache/dispose từng module.
3. Resource root mặc định, thư mục mới so với liên kết, quyền truy cập và
   hành vi khi thư mục không còn tồn tại.
4. Cách phân vùng local khi có user/workspace, di trú có xác nhận, phục hồi
   DB và layout hỏng, sao lưu và xử lý xung đột.
5. Giá trị quyền lợi/Free ban đầu, thời gian ân hạn, quota, ngưỡng cảnh báo,
   xử lý dữ liệu cloud quá hạn, tải lại dữ liệu, phân biệt cache/file sở hữu.
6. Gia hạn liên tục có giữ điều khoản cũ, nâng cấp/đăng ký lại có được tính
   là subscription mới, tên nhóm admin và quyền ghi chú nội bộ của nhân viên.
   Nguồn v2.0 có các mặc định đề xuất, **không coi mặc định là phê duyệt**.
7. Điều kiện upload lớn, quota reservation, idempotency, bảo mật, backup,
   xóa cloud có xác nhận, retention, sự cố và quyền tổ chức.

## 8. Bàn giao cho phiên sau

1. Đọc [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md), tài liệu này, Git status và
   code liên quan; xác định nhịp được giao và các thay đổi chưa commit.
2. Không lặp lại bước đã hoàn thành hoặc xóa/sửa thay đổi của người khác.
   Đối chiếu trạng thái thực tế thay vì giả định snapshot còn đúng.
3. Đưa kế hoạch chi tiết gồm file, phạm vi, rủi ro, cách test; hỏi khi thay đổi
   hành vi, cấu trúc dữ liệu hoặc gặp quyết định chưa rõ.
4. Chạy kiểm thử phù hợp và smoke trên dữ liệu thử độc lập; ghi bằng chứng,
   kết quả do người dùng xác nhận và phần chưa kiểm chứng.
5. Dừng ở ranh giới đã duyệt. Chỉ commit, tag, push hoặc sang bước khác khi
   người dùng cho phép.
