# BizNext Manager - Project Summary

**Last Updated:** 2026-11-27  
**Project Status:** Active Development  
**Primary Language:** C# (.NET 10, WPF)  
**Platform:** Windows (net10.0-windows)  
**Build Tool:** .NET SDK 10

---

## 📋 Project Overview

**BizNext Manager** là một ứng dụng Dashboard WPF trên Windows được xây dựng trên nền tảng .NET 10. Ứng dụng hoạt động như một host cho các tiện ích (modules) độc lập, với **QuickScan** là tiện ích đầu tiên được phát triển hoàn chỉnh. Các tiện ích khác như Notes (Ghi chú) và Weather/Calendar (Lịch & Thời tiết) đã có khung module sẵn sàng để phát triển tiếp.

**Mục đích:** Tạo một nền tảng Dashboard tập trung để quản lý nhiều tiện ích kinh doanh, bắt đầu với quét tài liệu, tạo PDF và quản lý thư viện.

---

## 🏗️ Kiến Trúc Tổng Quan

### Mô Hình Kiến Trúc: Modular Host Pattern

Dự án sử dụng kiến trúc modular, nơi:
- **BizNext.Shell** là ứng dụng chính (entry point) - WinExe (Windows Executable)
- Mỗi tiện ích là một **WPF Module** độc lập thoả mãn hợp đồng `IBizNextModule`
- **Module Catalog** quản lý đăng ký và điều hướng tuyến tính (static composition - không phải dynamic plugin loader)
- Trạng thái ứng dụng được lưu giữ qua `JsonFileStorage` (atomic file writes)
- Giao diện được quản lý tập trung qua hệ thống **Theme** dùng chung

### Luồng Module Lifecycle

```
1. App.xaml.cs khởi động
   ↓
2. Tải Shell Settings (%LocalAppData%\BizNext-Manager\shell-settings.json)
   ↓
3. Tải cấu hình QuickScan cũ (%LocalAppData%\QuickScanManager\)
   ↓
4. Áp dụng Theme (Light/Dark/System)
   ↓
5. Tạo Module Instances (static registration)
   ↓
6. ModuleCatalog xác thực module IDs
   ↓
7. ModuleNavigationService quản lý selection state
   ↓
8. Shell lười tạo View khi module được chọn lần đầu (lazy load)
   ↓
9. Cache View để giữ in-memory state khi điều hướng
   ↓
10. Lưu selected module ID vào shell-settings.json (atomic)
```

---

## 📁 Cấu Trúc Thư Mục và Projects

### Solution Structure
```
BizNext-Manager.sln
│
├─ src/
│  │
│  ├─ BizNext.Contracts/                    [Class Library, net10.0-windows]
│  │  ├─ IBizNextModule.cs                  Interface chính cho all modules
│  │  ├─ Models/
│  │  │  ├─ BizNextModuleMetadata.cs       Record: Id, Name, Description, Category
│  │  │  └─ ThemePreference.cs             Enum: Light, Dark, System
│  │  └─ BizNext.Contracts.csproj
│  │
│  ├─ BizNext.Infrastructure/              [Class Library, net10.0-windows]
│  │  ├─ Modules/
│  │  │  ├─ ModuleCatalog.cs               Quản lý registry modules
│  │  │  └─ ModuleNavigationService.cs     Quản lý selected module + publish changes
│  │  ├─ Settings/
│  │  │  └─ [Shell settings management]
│  │  ├─ Storage/
│  │  │  └─ JsonFileStorage.cs             Lưu JSON atomically
│  │  └─ BizNext.Infrastructure.csproj
│  │
│  ├─ BizNext.Shell/                       [WinExe (WPF), net10.0-windows]
│  │  ├─ App.xaml(.cs)                     Entry point, theme loading, module registration
│  │  ├─ MainWindow.xaml(.cs)              Dashboard UI - sidebar + module host area
│  │  ├─ DashboardSettingsWindow.xaml(.cs) Settings dialog cho theme selection
│  │  ├─ Themes/
│  │  │  └─ ControlStyles.xaml             Dashboard-only theme option style
│  │  ├─ Converters/
│  │  │  └─ [Value converters for XAML bindings]
│  │  └─ BizNext.Shell.csproj
│  │
│  ├─ BizNext.DesignSystem/                [WPF shared UI library]
│  │  ├─ Themes/DesignTokens.xaml          Shared semantic tokens
│  │  ├─ Themes/DarkTheme.xaml             Dark theme overrides
│  │  ├─ Themes/Shared/                    Reusable styles and templates
│  │  ├─ Components/                       Reusable WPF controls
│  │  └─ Converters/                       Shared UI converters
│  │
│  ├─ BizNext.Modules.QuickScan/           [Class Library WPF, net10.0-windows] ⭐ ACTIVE
│  │  ├─ QuickScanView.xaml(.cs)           Main UserControl - module UI root
│  │  ├─ AppSettingsWindow.xaml(.cs)       QuickScan settings dialog
│  │  ├─ ScannerProfileWindow.xaml(.cs)    Scanner profile editor
│  │  ├─ CreatePdfFolderWindow.xaml(.cs)   Dialog tạo folder output PDF
│  │  ├─ RenamePdfWindow.xaml(.cs)         Dialog đổi tên PDF
│  │  ├─ PdfViewerWindow.xaml(.cs)         Viewer PDF in-app (page-by-page)
│  │  ├─ Commands/
│  │  │  ├─ RelayCommand.cs                ICommand impl không tham số
│  │  │  ├─ RelayCommandOfT.cs             ICommand impl với generic type
│  │  │  └─ AsyncRelayCommand.cs           Async ICommand impl
│  │  ├─ Models/
│  │  │  ├─ ApplicationSettings.cs         Settings (PDF dir, scanner, timestamp)
│  │  │  ├─ PdfLibraryEntry.cs             Đại diện PDF/folder trong library
│  │  │  ├─ PdfLibraryFolder.cs            Folder metadata
│  │  │  └─ PdfLibraryLoadResult.cs        Result type cho library load
│  │  ├─ Services/
│  │  │  ├─ PdfLibraryBrowserService.cs    Load PDFs from folders + metadata
│  │  │  ├─ ScannedPdfExportService.cs     Export scanned images to PDF
│  │  │  └─ JsonApplicationSettingsStore.cs JSON storage for app settings
│  │  ├─ ViewModels/
│  │  │  ├─ ScanPageViewModel.cs           Main ViewModel cho scan workflow
│  │  │  ├─ ScannerSettingsViewModel.cs    Settings screen ViewModel
│  │  │  ├─ ScannerOption.cs               Dropdown option cho scanner selection
│  │  │  └─ StatusMessageType.cs           Enum: Info, Success, Warning, Error
│  │  ├─ Assets/
│  │  │  ├─ icon.ico                       App icon
│  │  │  ├─ icon.png
│  │  │  └─ *.svg                          Vector icons
│  │  ├─ storage/                          Temp storage directory
│  │  └─ BizNext.Modules.QuickScan.csproj
│  │     Dependencies: BizNext.Contracts, BizNext.Infrastructure,
│  │                   BizNext.DesignSystem, ScanCore, ScanDevice, PDFtoImage
│  │
│  ├─ BizNext.Modules.Notes/               [Class Library WPF, net10.0-windows] 📝 Placeholder
│  │  └─ [Khung cơ bản - sẵn sàng để phát triển]
│  │
│  ├─ BizNext.Modules.WeatherCalendar/     [Class Library WPF, net10.0-windows] 📅 Placeholder
│  │  └─ [Khung cơ bản - sẵn sàng để phát triển]
│  │
│  ├─ ScanCore/                            [Class Library, net10.0-windows] 🔧 Support Lib
│  │  ├─ Models/
│  │  │  ├─ DocumentItem.cs                Record: Guid Id, Name, FilePath, Category, PageCount, Tags, CreatedAtUtc
│  │  │  └─ [Scan metadata models]
│  │  ├─ Services/
│  │  │  ├─ PdfService.cs                  PDF manipulation (render, merge...)
│  │  │  ├─ JsonLibraryService.cs          Load/save library.json (metadata)
│  │  │  └─ [Core PDF operations]
│  │  └─ ScanCore.csproj
│  │     Dependencies: PDFtoImage (NuGet)
│  │
│  └─ ScanDevice/                          [Class Library, net10.0-windows] 🖥️ Support Lib
│     ├─ Models/
│     │  ├─ ScannerDevice.cs               Record: Id (string), Name (string)
│     │  ├─ ScannerProfile.cs              DeviceId, Alias, ColorMode, ResolutionDpi, Source
│     │  ├─ ScannerConfig.cs               [Scanner configuration]
│     │  └─ [WIA scanner models]
│     ├─ Services/
│     │  ├─ IScannerService.cs             Interface chính cho scanner operations
│     │  ├─ WiaScannerService.cs           Impl real WIA scanners
│     │  ├─ FakeScannerService.cs          Impl fake scanner (dev/testing)
│     │  ├─ IScannerConfigStore.cs         Interface for config storage
│     │  ├─ JsonScannerConfigStore.cs      JSON storage for scanner configs
│     │  └─ [Scanner-specific services]
│     └─ ScanDevice.csproj
│
├─ docs/
│  ├─ architecture.md                       Detailed architecture documentation
│  └─ [Other documentation]
│
├─ QuickScanManager/                       Legacy support (bin, Release builds)
│  └─ [Build output directory - có thể xóa trong clean]
│
└─ [Root level configs]
   ├─ BizNext-Manager.sln
   ├─ README.md                            Vietnamese project intro
   ├─ PROJECT_CONTEXT.md                   Living handoff document
   └─ .gitignore

```

### Key Directories
- **src/**: Tất cả source code
- **docs/**: Documentation (architecture guide)
- **.github/skills/**: UI design system, WPF workflow guide
- **.vscode/**: VS Code settings, launch profiles
- **.git/**: Version control
- **QuickScanManager/**: Legacy build outputs (có thể là từ migration cũ)

---

## 📦 Dependencies & Libraries

### NuGet Packages
| Package | Version | Purpose |
|---------|---------|---------|
| **PDFtoImage** | 5.4.0 | PDF rendering, page extraction, thumbnail generation |
| **.NET Runtime** | 10.0 | Windows target framework (net10.0-windows) |

### Project References (Internal)
- **BizNext.Contracts** ← Referenced by: Infrastructure, Shell, QuickScan, Notes, WeatherCalendar
- **BizNext.Infrastructure** ← Referenced by: Shell, QuickScan
- **ScanCore** ← Referenced by: QuickScan
- **ScanDevice** ← Referenced by: QuickScan

### WPF Framework
- Uses: `System.Windows` (WPF)
- Nullable: Enabled globally (`<Nullable>enable</Nullable>`)
- Implicit Usings: Enabled globally

---

## 🎯 Core Interfaces & Base Classes

### IBizNextModule (BizNext.Contracts)
```csharp
public interface IBizNextModule
{
    BizNextModuleMetadata Metadata { get; }
    FrameworkElement CreateView();  // Returns WPF UserControl or Window root
}
```

**Implementations:**
- `QuickScanModule` (BizNext.Modules.QuickScan)
- `NotesModule` (BizNext.Modules.Notes) - Placeholder
- `WeatherCalendarModule` (BizNext.Modules.WeatherCalendar) - Placeholder

### BizNextModuleMetadata (sealed record)
```csharp
public sealed record BizNextModuleMetadata(
    string Id,                          // e.g., "biznext.module.quickscan"
    string Name,                        // Display name
    string Description,                 // User-visible description
    string Category                     // Grouping (e.g., "Productivity")
);
```

### Module IDs (Constants)
```
biznext.module.quickscan           → QuickScan (Document Scanning)
biznext.module.notes               → Notes (Ghi chú)
biznext.module.weathercalendar     → Weather & Calendar (Lịch & Thời tiết)
```

### Theme System (ThemePreference enum)
```
Light       → Light theme
Dark        → Dark theme
System      → Follow Windows system theme
```

---

## 🎨 User Interface Architecture

### MainWindow (BizNext.Shell)
**Layout:** Sidebar + Content Area
- **Sidebar (Left):**
  - Module navigation buttons (QuickScan, Notes, Calendar icons)
  - Bottom: "Cài đặt" (Settings) button for Dashboard settings
  - Responsive to theme changes

- **Header (Top):**
  - Current module name and icon
  - Module description

- **Content Area (Center-Right):**
  - Hosts selected module's view (UserControl)
  - Lazy-loaded on first selection
  - Cached between navigations (preserves state)

- **Themes Path:**
  - `src/BizNext.DesignSystem/Themes/` - Shared tokens, themes, and control styles
  - `src/BizNext.Shell/Themes/` - Dashboard-only styles and theme application
  - Source tokens: `.github/skills/ui-design-system/` (source-of-truth)

### QuickScanView (BizNext.Modules.QuickScan)
**Main Interface Components:**
1. **Scan Tab:**
   - Scanner selection dropdown
   - Scan mode options (Single page / ADF batch)
   - Scan button (triggers IScannerService)
   - Page editing area (reorder, delete, rotate, flip)
   - Settings button (bottom-right of status bar)

2. **Library Tab:**
   - Folder navigation (tree/breadcrumb)
   - PDF list (DataGrid) with columns:
     - File name (sortable)
     - Page count
     - File size (formatted KB/MB/GB)
     - Scan date/time (formatted dd/MM/yyyy HH:mm)
   - Search box (filters by filename)
   - Right-click context menu: View, Reveal in Explorer, Rename, Delete
   - Parent folder navigation link: "Quay lại thư mục cha"

3. **Settings Dialog:**
   - PDF output directory selector
   - Scanner profile selector
   - Timestamp appending toggle
   - **NO** theme option (moved to Dashboard settings)

4. **Other Dialogs:**
   - `PdfViewerWindow` - Standalone PDF viewer (page-by-page navigation)
   - `AppSettingsWindow` - QuickScan-specific settings
   - `ScannerProfileWindow` - Profile editor
   - `RenamePdfWindow` - Rename dialog
   - `CreatePdfFolderWindow` - Create new folder

### Dashboard Settings (DashboardSettingsWindow)
- **Theme Selector:** Radio buttons (Light / Dark / System)
- Owns all Shell-level settings
- Stored in: `%LocalAppData%\BizNext-Manager\shell-settings.json`

### Design System Integration
- **Tokens Source:** `.github/skills/ui-design-system/` (semantic design tokens)
- **WPF Adapters:** `src/BizNext.DesignSystem/Themes/DesignTokens.xaml` + `DarkTheme.xaml`
- **Shared Styles:** `src/BizNext.DesignSystem/Themes/Shared/`
- **Reusable Controls:** `src/BizNext.DesignSystem/Components/`
- **Dynamic Resources:** Modules use `DynamicResource` for brushes (follow theme changes)
- **Module UI:** Modules compose feature views and behavior but do not define local styles,
  reusable controls, components, or theme dictionaries.

---

## 💾 Data Models & Structures

### ApplicationSettings (QuickScan)
```csharp
public sealed class ApplicationSettings
{
    public string DefaultPdfDirectory { get; set; }      // Output folder path
    public string? DefaultScannerDeviceId { get; set; }  // Last selected scanner
    public bool AppendTimestampToPdfName { get; set; }   // Auto-timestamp naming
    public ThemePreference? Theme { get; set; }          // Legacy - Shell owns now
}
```
**Stored:** `%LocalAppData%\QuickScanManager\app-settings.json`

### PdfLibraryEntry (QuickScan)
```csharp
public sealed class PdfLibraryEntry
{
    public required string Name { get; init; }           // File or folder name
    public required bool IsFolder { get; init; }         // Type discriminator
    public bool IsParentNavigation { get; init; }        // "Go back" link
    public string? FullPath { get; init; }               // Full file path
    public int? PageCount { get; init; }                 // PDF pages (from metadata)
    public long? SizeBytes { get; init; }                // File size in bytes
    public DateTimeOffset? ScannedAt { get; init; }      // Scan/creation timestamp
    public string? ErrorMessage { get; init; }           // Load error details
    
    // Display helpers:
    public string SizeDisplay { get; }                   // "1.23 MB"
    public string ScannedAtDisplay { get; }              // "27/11/2026 14:30"
}
```

### PdfLibraryFolder
```csharp
public sealed class PdfLibraryFolder
{
    // Metadata for a PDF library folder
}
```

### DocumentItem (ScanCore)
```csharp
public sealed record DocumentItem
{
    public Guid Id { get; init; }                        // Unique identifier
    public required string Name { get; init; }           // Document name
    public required string FilePath { get; init; }       // Full path to file
    public string? Category { get; init; }               // Classification
    public int PageCount { get; init; }                  // Total pages
    public string? Tags { get; init; }                   // Searchable tags
    public DateTimeOffset CreatedAtUtc { get; init; }    // Creation UTC time
}
```
**Stored:** `library.json` in QuickScan data folder

### ScannerDevice (ScanDevice)
```csharp
public sealed record ScannerDevice(
    string Id,          // WIA device ID or fake ID
    string Name         // User-friendly name
);
```

### ScannerProfile (ScanDevice)
```csharp
public sealed class ScannerProfile
{
    public required string DeviceId { get; init; }       // Link to device
    public required string Alias { get; set; }           // Custom profile name
    public string ColorMode { get; set; }                // "Color", "Grayscale", "BlackAndWhite"
    public int ResolutionDpi { get; set; }               // 150, 200, 300, 600...
    public string Source { get; set; }                   // "Flatbed", "ADF Front", "ADF Back"
}
```
**Stored:** `%LocalAppData%\QuickScanManager\scanner-profiles.json`

### ScannerConfig
```csharp
public sealed class ScannerConfig
{
    // Container for active scanner configuration
}
```

---

## 🔧 Core Services & Methods

### ModuleCatalog (BizNext.Infrastructure)
```csharp
// Validates and manages registered modules
public class ModuleCatalog
{
    public void Register(IBizNextModule module);         // Register a module
    public IBizNextModule? GetModule(string moduleId);   // Get by ID
    public IReadOnlyList<IBizNextModule> GetAll();       // List all modules
    // Rejects duplicate/missing IDs
}
```

### ModuleNavigationService (BizNext.Infrastructure)
```csharp
public class ModuleNavigationService
{
    public string? CurrentModuleId { get; }              // Selected module ID
    public event EventHandler<string>? ModuleChanged;    // Navigation event
    public bool Navigate(string moduleId);               // Switch to module
}
```

### JsonFileStorage (BizNext.Infrastructure)
```csharp
public class JsonFileStorage
{
    public T? Load<T>(string filePath);                  // Deserialize JSON
    public void Save<T>(string filePath, T data);        // Atomic JSON write
    // Uses System.Text.Json with safe atomic writes
}
```

### PdfLibraryBrowserService (QuickScan)
```csharp
public class PdfLibraryBrowserService
{
    public async Task<PdfLibraryLoadResult> LoadLibraryAsync(
        string folderPath, 
        string? searchFilter = null
    );
    // Returns list of PdfLibraryEntry items
    // Loads metadata from library.json
    // Inspects PDFs with PDFtoImage if metadata missing
}
```

### ScannedPdfExportService (QuickScan)
```csharp
public class ScannedPdfExportService
{
    public async Task ExportToPdfAsync(
        List<string> imagePaths,
        string outputPath,
        string fileName,
        bool appendTimestamp = true
    );
    // Merge scanned images into single PDF
    // Save with optional timestamp naming
    // Update library.json metadata
}
```

### JsonApplicationSettingsStore (QuickScan)
```csharp
public class JsonApplicationSettingsStore
{
    public ApplicationSettings Load();                   // Load from app-settings.json
    public void Save(ApplicationSettings settings);     // Save atomically
}
```

### PdfService (ScanCore)
```csharp
public class PdfService
{
    public async Task<int> GetPageCountAsync(string filePath);
    public async Task RenderPageAsync(string filePath, int pageIndex, string outputPath);
    // Uses PDFtoImage for rendering
}
```

### JsonLibraryService (ScanCore)
```csharp
public class JsonLibraryService
{
    public List<DocumentItem> LoadLibrary(string filePath);
    public void SaveLibrary(string filePath, List<DocumentItem> items);
    // Manages library.json (PDF metadata)
}
```

### IScannerService Interface (ScanDevice)
```csharp
public interface IScannerService
{
    Task<List<ScannerDevice>> EnumerateDevicesAsync();
    Task<string> ScanAsync(ScannerProfile profile);     // Returns image file path
    Task<List<string>> ScanBatchAsync(ScannerProfile profile);
    // Abstraction for WIA / Fake implementations
}
```

### WiaScannerService (ScanDevice)
```csharp
public class WiaScannerService : IScannerService
{
    // Implements real WIA scanner access
    // Uses Windows Imaging Acquisition API
}
```

### FakeScannerService (ScanDevice)
```csharp
public class FakeScannerService : IScannerService
{
    // Generates fake scan images for development
    // Activated by QUICKSCAN_USE_FAKE_SCANNER=1
}
```

### JsonScannerConfigStore (ScanDevice)
```csharp
public class JsonScannerConfigStore : IScannerConfigStore
{
    public List<ScannerProfile> LoadProfiles();
    public void SaveProfiles(List<ScannerProfile> profiles);
    // Manages scanner-profiles.json
}
```

---

## 📂 Data Storage Locations

### Shell Settings
**Path:** `%LocalAppData%\BizNext-Manager\shell-settings.json`
```json
{
  "LastSelectedModuleId": "biznext.module.quickscan",
  "Theme": "Dark"
}
```

### QuickScan Settings (Legacy - Maintained)
**Path:** `%LocalAppData%\QuickScanManager\app-settings.json`
```json
{
  "DefaultPdfDirectory": "C:\\Users\\User\\Documents\\Scans",
  "DefaultScannerDeviceId": "scanner-id-123",
  "AppendTimestampToPdfName": true
}
```

### Scanner Profiles
**Path:** `%LocalAppData%\QuickScanManager\scanner-profiles.json`
```json
[
  {
    "DeviceId": "scanner-id-123",
    "Alias": "Office Scanner",
    "ColorMode": "Color",
    "ResolutionDpi": 300,
    "Source": "Flatbed"
  }
]
```

### PDF Library Metadata
**Path:** `%LocalAppData%\QuickScanManager\library.json`
```json
[
  {
    "Id": "guid-uuid",
    "Name": "Important Document",
    "FilePath": "C:\\Users\\User\\Documents\\Scans\\document.pdf",
    "Category": "Finance",
    "PageCount": 15,
    "Tags": "invoice,2026",
    "CreatedAtUtc": "2026-11-26T10:30:00Z"
  }
]
```

### Temporary Scan Images
**Path:** `%LocalAppData%\QuickScanManager\temp-scans\`
- Stores temporary image files during scan session
- Cleaned up after PDF export

---

## 🎯 Completed Features (Hoạt Động Được)

### QuickScan Module ✅
1. **Scanner Operations**
   - [x] WIA scanner detection and enumeration
   - [x] Single-page scan
   - [x] ADF batch scanning
   - [x] Fake scanner mode (dev testing)
   - [x] Scanner profile management (color mode, resolution, source)

2. **Image Handling**
   - [x] Image import (JPG, PNG, TIFF)
   - [x] Page reordering (drag/drop or list controls)
   - [x] Page deletion
   - [x] Image rotation (left/right/180°)
   - [x] Image flip (horizontal/vertical)
   - [x] Page preview in scan area

3. **PDF Export**
   - [x] Merge multiple images into single PDF
   - [x] Configurable output location
   - [x] Paper size selection
   - [x] Timestamp auto-naming (optional)
   - [x] Library metadata auto-creation

4. **PDF Library**
   - [x] Load PDFs from configured folder and subfolders
   - [x] Search by filename
   - [x] Sort by: Name, Page Count, File Size, Scan Date
   - [x] Display file properties: Name, Pages, Size, Date
   - [x] Folder navigation (tree view with parent link)
   - [x] In-app PDF viewer (page-by-page navigation)
   - [x] Context menu actions: Quick View, Reveal in Explorer
   - [x] Library metadata persistence (library.json)

5. **Settings & Configuration**
   - [x] Application settings storage (JSON)
   - [x] Scanner profile editor (create/edit/delete)
   - [x] Default PDF directory selection
   - [x] Default scanner device selection

6. **User Interface**
   - [x] Tabbed interface (Scan / Library)
   - [x] Status bar with mode indicator
   - [x] Modal dialogs (settings, profile, rename, folder creation)
   - [x] Settings button in status bar (bottom-right)
   - [x] Dialog ownership (uses host window)
   - [x] Error messages and status updates

7. **Theming**
   - [x] Light theme support
   - [x] Dark theme support
   - [x] System theme auto-detection
   - [x] Theme persistence (shell-settings.json)
   - [x] Dynamic resource bindings for theme switching

8. **Data Compatibility**
   - [x] Existing QuickScan data preserved on upgrade
   - [x] Legacy theme preference migration (one-time)
   - [x] Backward-compatible JSON schema

### Shell & Infrastructure ✅
1. **Dashboard Host**
   - [x] Sidebar navigation
   - [x] Module switching (lazy load + cache)
   - [x] Settings button (Dashboard settings)

2. **Module System**
   - [x] Static module registration
   - [x] Module catalog with validation
   - [x] Module navigation service
   - [x] Shared theme system

3. **Data Persistence**
   - [x] Atomic JSON file storage
   - [x] Shell settings persistence
   - [x] Application startup state restoration

### Module Placeholders ⚠️ (Framework Ready, No Logic)
- [x] **Notes Module** - Frame created, awaiting implementation
- [x] **Weather/Calendar Module** - Frame created, awaiting implementation

---

## 🚫 Known Limitations & Future Work

### Not Yet Implemented
- [ ] **PDF Library Management**
  - Rename PDF files (framework exists, not wired)
  - Delete PDF files (framework exists, deletion not implemented)
  - Edit tags on PDFs (UI not built)
  - Multi-select operations

- [ ] **Scanner Features**
  - Duplex scanning (double-sided ADF)
  - Batch job scheduling
  - Automatic document feeder configuration UI

- [ ] **Notes Module**
  - Note creation/editing
  - Note storage/retrieval
  - Note search and tagging

- [ ] **Weather/Calendar Module**
  - Calendar integration
  - Weather data fetching
  - Appointment display

- [ ] **Testing**
  - No automated test project yet
  - Manual smoke testing only

---

## 🏢 Project Statistics

| Metric | Count |
|--------|-------|
| Total C# Source Files | 157 |
| XAML Files | 13 |
| Projects (DLLs + EXE) | 8 |
| Main Modules | 3 (1 active, 2 placeholder) |
| Support Libraries | 2 (ScanCore, ScanDevice) |
| .NET Framework | net10.0-windows |
| Primary Language | C# (Nullable enabled, Implicit usings) |
| External Dependencies | 1 (PDFtoImage v5.4.0) |

---

## 🛠️ Build & Development

### Prerequisites
- Windows 11 Pro or later
- .NET 10 SDK installed
- Visual Studio Code (recommended) or Visual Studio
- WIA-compatible scanner (optional - fake mode available)

### Build Commands
```powershell
# Build solution
dotnet build .\BizNext-Manager.sln

# Run application
dotnet run --project .\src\BizNext.Shell\BizNext.Shell.csproj

# Run with fake scanner (no hardware needed)
$env:QUICKSCAN_USE_FAKE_SCANNER=1
dotnet run --project .\src\BizNext.Shell\BizNext.Shell.csproj

# Clean build
dotnet clean .\BizNext-Manager.sln
```

### VS Code Launch Profiles
- **BizNext Manager (WPF, giả lập scanner)** - Default, with fake scanner
- **BizNext Manager (WPF, real scanner)** - Production mode

### Configuration Files
- `.vscode/launch.json` - Debug profiles
- `.vscode/settings.json` - Editor settings
- `.gitignore` - Source control exclusions
- `.github/skills/` - Project guides and design system

---

## 📚 Documentation References

| Document | Location | Purpose |
|----------|----------|---------|
| Project Context | `PROJECT_CONTEXT.md` | Living handoff for future devs |
| Architecture Guide | `docs/architecture.md` | Detailed architecture patterns |
| UI Design System | `.github/skills/ui-design-system/` | Semantic design tokens |
| WPF Workflow | `.github/skills/wpf/SKILL.md` | WPF development guide |
| This Summary | `PROJECT_SUMMARY.md` | Complete project overview |

---

## 🔄 Development Workflow

### Adding a New Feature to QuickScan
1. Create service/model in `BizNext.Modules.QuickScan/`
2. Update ViewModel if logic layer needed
3. Update XAML view to bind commands/data
4. Store settings in `ApplicationSettings` if persistent
5. Test with fake scanner mode
6. Update `docs/architecture.md` if contracts change

### Adding a New Module
1. Create new project `BizNext.Modules.NewModule` (WPF Class Library)
2. Reference `BizNext.Contracts`
3. Implement `IBizNextModule` interface
4. Create WPF UserControl (root view)
5. Register in `BizNext.Shell/App.xaml.cs`
6. Add navigation entry in `MainWindow.xaml`
7. Add module icon to sidebar
8. Document in `docs/architecture.md`

### Version Control Conventions
- Branch by feature: `feature/module-name` or `feature/scanner-duplex`
- Branch by fix: `fix/issue-name`
- Merge to `main` via pull request
- Atomic commits with descriptive messages
- Reference issue numbers in commit messages

---

## 🔐 Security & Data Privacy

- **No network calls** (all local processing)
- **No telemetry or analytics**
- **Data stored locally** (`%LocalAppData%`)
- **WIA integration** respects Windows security model
- **PDF files** stored in user-designated directories

---

## 💡 Tips for Future Developers

1. **Understand the Module Pattern First**
   - Read `IBizNextModule` and `BizNextModuleMetadata`
   - Study how QuickScan module works
   - Then extend with new modules

2. **Respect Theme System**
   - Use `DynamicResource` for all brushes/colors
   - Don't hardcode colors
   - Reuse styles and controls from `BizNext.DesignSystem/Themes/Shared/` and `Components/`
   - Do not add module-local styles, reusable controls, components, or theme dictionaries

3. **Data Compatibility**
   - QuickScan data lives in separate folder for backward compatibility
   - Don't migrate user data without explicit confirmation
   - Version your JSON schemas

4. **Testing Strategy**
   - Use fake scanner mode for development
   - Test without WIA hardware
   - Smoke test UI changes in Light/Dark/System themes

5. **Performance Considerations**
   - Lazy-load module views to reduce startup time
   - Cache views to preserve state
   - Async IO for large PDF libraries

6. **Error Handling**
   - Display errors in status bar when possible
   - Use `ErrorMessage` field in data models
   - Log to Windows Event Viewer for severe issues

---

## 📞 Contact & Support

- **Git History:** Use `git log` to see development history
- **Active Commits:** Last update 2026-11-26
- **Maintained By:** VibeCoding Projects

---

**End of Project Summary**  
*This document should be updated whenever architecture, features, or project status changes.*
