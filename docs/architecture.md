# BizNext Manager architecture

## Overview

BizNext Manager is a Windows WPF desktop application. `BizNext.Shell` is the
single executable and owns the Dashboard, application theme, and navigation.
Each utility is presented as a WPF view implementing the shared module
contract; the shell replaces the hosted view without starting another window.

## Projects and responsibilities

```text
BizNext.Shell
├── BizNext.Contracts
├── BizNext.Infrastructure
├── BizNext.Modules.QuickScan
│   ├── ScanCore
│   └── ScanDevice
├── BizNext.Modules.Notes
└── BizNext.Modules.WeatherCalendar
```

- **BizNext.Contracts** defines `IBizNextModule` and `BizNextModuleMetadata`.
  Module IDs are stable identifiers such as `biznext.module.quickscan`.
- **BizNext.Infrastructure** validates and catalogs registered modules,
  coordinates selected-module navigation, stores the last module selection,
  and provides atomic JSON file persistence.
- **BizNext.Shell** is the application entry point. It registers built-in
  modules, owns the shared theme resources, remembers navigation state, and
  hosts each module view.
- **BizNext.Modules.QuickScan** adapts the existing scan workflow into
  `QuickScanView`, a `UserControl`. `ScanCore` and `ScanDevice` remain separate
  supporting libraries for PDF/library and scanner responsibilities.
- **BizNext.Modules.Notes** and **BizNext.Modules.WeatherCalendar** are
  registered placeholder modules; they do not yet implement note storage,
  calendar, or weather services.

The current module catalog is statically composed by the Shell. This keeps
module ownership and project references explicit; it is not a dynamic
assembly/plugin loader.

## Module lifecycle and navigation

1. `App` loads the Shell settings and the existing QuickScan settings,
   resolves and applies the theme (see *UI and theming*), and creates the
   built-in module instances.
2. `ModuleCatalog` rejects missing or duplicate module IDs.
3. `ModuleNavigationService` validates a requested ID and publishes selection
   changes.
4. The Shell creates a module view the first time it is selected, then caches
   that view so its in-memory state survives navigation.
5. The selected ID is written atomically to the Shell settings file. The
   Dashboard is shown when there is no saved module selection.

To add a module, create a WPF class library referencing `BizNext.Contracts`,
implement `IBizNextModule`, register it in `BizNext.Shell`, add a navigation
entry, and add its project to `BizNext-Manager.sln`.

## Data ownership and compatibility

- Shell settings (last module and theme): `%LocalAppData%\BizNext-Manager\shell-settings.json`.
- QuickScan settings, scanner profiles, PDF library, exports, and temporary
  scan images remain under `%LocalAppData%\QuickScanManager\`.
- Fake-scanner mode remains opt-in via `QUICKSCAN_USE_FAKE_SCANNER=1`, and uses
  its existing separate settings/profile files.

Keeping QuickScan's established data root avoids a one-time migration and
prevents the new host from hiding or overwriting existing user PDFs or scanner
configuration.

## QuickScan PDF library

- The library reads PDF files from the currently configured default output
  folder and its direct child folders. The default folder is always the first
  folder choice; child folders are listed above files when it is selected.
- Selecting a folder or refreshing enumerates its PDFs and displays filename,
  page count, file size, and scan/creation date. Filename search filters the
  current folder, and column headers sort the results without moving child
  folders below PDF rows in the default folder.
- The initial order is scan date descending. In a child folder, a pinned
  “Quay lại thư mục cha” row returns to the default folder. Right-clicking a
  PDF row opens its actions: quick view, reveal in Explorer, rename, and
  delete. Rename preserves matching library metadata; deletion asks for
  confirmation and removes the corresponding metadata entry.
- Existing `library.json` metadata supplies saved page counts and scan dates.
  PDFs without metadata are inspected with the existing `PDFtoImage` renderer;
  unreadable metadata/page counts are surfaced in the library status tooltip.
- Double-clicking a child folder selects it in the folder picker. Double-
  clicking a PDF opens `PdfViewerWindow`, which renders one page at a time and
  supports previous/next navigation.
- Tag editing is not yet implemented.

## UI and theming

The Shell owns the application's look and feel and provides it to every module
through the application resources:

- **Tokens and themes**: `src/BizNext.Shell/Themes/DesignTokens.xaml` (light)
  and `DarkTheme.xaml`. The source design tokens remain under
  `.github/skills/ui-design-system/`; these WPF dictionaries are adapters, not
  a second token authority.
- **Shared controls and styles**: `Themes/ControlStyles.xaml` (cards, labels,
  inputs, buttons, navigation tabs, status-bar button, data grid, ...). Modules
  should reuse these keys instead of redefining them.
- **Theme choice**: Dashboard settings (opened from the bottom of the sidebar)
  has the only theme selector (Light, Dark, System). The choice is stored in
  `shell-settings.json` and applied by `App.ApplyTheme` at startup and on save.
  Modules must not expose their own theme setting and must reference brushes
  with `DynamicResource` so they follow theme changes.
- **Module-specific styles** stay in the module. QuickScan keeps its scan-only
  styles in `Themes/QuickScanStyles.xaml`, merged into `QuickScanView`. Shared
  Shell dictionaries must not reference module types.
- A module's root view should not repeat the Dashboard title; the Shell header
  shows the module name and description. Module-level settings are opened from
  the module's own status bar (QuickScan: **Cài đặt**, bottom-right).
- Note: a `TabItem` sets its own foreground, which breaks inheritance of the
  theme text color for its content; set `Foreground` on `TabItem` explicitly.

`ThemePreference` lives in `BizNext.Contracts`. A theme previously saved in the
QuickScan `app-settings.json` is adopted once as the Shell's theme when the Shell
has none, and is no longer written back.

QuickScan dialogs obtain their owner from the host window.

## Build and run

From the repository root on Windows with .NET 10 SDK:

```powershell
dotnet build .\BizNext-Manager.sln
dotnet run --project .\src\BizNext.Shell\BizNext.Shell.csproj
```

Set `QUICKSCAN_USE_FAKE_SCANNER=1` to exercise the scan workflow without WIA
hardware.
