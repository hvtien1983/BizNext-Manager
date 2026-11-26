# QuickScanManager - Project Context

> Living handoff document for future development sessions. Update this file when
> architecture, user-visible behavior, or project status changes.
>
> Last updated: 2026-11-26

## Project overview

QuickScanManager is a Windows desktop application built with WPF and .NET 10.
Its purpose is to scan paper documents, assemble scanned pages into PDFs, and
maintain a library of saved documents. The application can use Windows Image
Acquisition (WIA) hardware or an in-process fake scanner for development on
machines without scanner hardware.

The solution is inside `QuickScanManager/`; shared UI-agent skills and VS Code
launch/build configuration are at the repository root.

## Current solution structure

Generated `bin/`, `obj/`, and `.vs/` contents are intentionally omitted.

```text
BizNext-PdfScanManager/
├── PROJECT_CONTEXT.md
├── .vscode/
│   ├── launch.json                     # Debug with fake or real scanner
│   └── tasks.json                      # Build solution before debugging
├── .github/skills/
│   ├── README.md
│   ├── ui-design-system/
│   │   ├── SKILL.md
│   │   ├── assets/design-tokens.json
│   │   └── references/design-tokens.md
│   ├── ui-component-patterns/
│   │   ├── SKILL.md
│   │   └── references/component-guidelines.md
│   └── ui-platform-adaptation/
│       ├── SKILL.md
│       └── references/platform-mapping.md
└── QuickScanManager/
    ├── QuickScanManager.sln
    ├── README.md
    ├── storage/
    │   ├── library.json                # Seed/runtime library metadata
    │   └── pdfs/.gitkeep               # Default PDF output placeholder
    └── src/
        ├── ScanCore/
        │   ├── ScanCore.csproj
        │   ├── Models/DocumentItem.cs
        │   └── Services/
        │       ├── JsonLibraryService.cs
        │       └── PdfService.cs
        ├── ScanDevice/
        │   ├── ScanDevice.csproj
        │   ├── Models/
        │   │   ├── ScannerConfig.cs
        │   │   ├── ScannerDevice.cs
        │   │   └── ScannerProfile.cs
        │   └── Services/
        │       ├── FakeScannerService.cs
        │       ├── IScannerConfigStore.cs
        │       ├── IScannerService.cs
        │       ├── JsonScannerConfigStore.cs
        │       └── WiaScannerService.cs
        └── ScanApp/
            ├── ScanApp.csproj
            ├── App.xaml[.cs]
            ├── MainWindow.xaml[.cs]
            ├── AppSettingsWindow.xaml[.cs]
            ├── ScannerProfileWindow.xaml[.cs]
            ├── Commands/
            │   ├── AsyncRelayCommand.cs
            │   ├── RelayCommand.cs
            │   └── RelayCommandOfT.cs
            ├── Models/ApplicationSettings.cs
            ├── Models/ThemePreference.cs
            ├── Services/
            │   ├── JsonApplicationSettingsStore.cs
            │   └── ScannedPdfExportService.cs
            ├── Themes/
            │   ├── ControlStyles.xaml
            │   ├── DarkTheme.xaml
            │   └── DesignTokens.xaml
            └── ViewModels/
                ├── ScannerOption.cs
                ├── ScannerSettingsViewModel.cs
                ├── ScanPageViewModel.cs
                └── StatusMessageType.cs
```

The paired XAML and code-behind files shown as `*.xaml[.cs]` are separate
`.xaml` and `.xaml.cs` files.

## Architecture and key files

- **ScanCore** contains document metadata and JSON/PDF persistence. `DocumentItem`
  represents a library entry; `JsonLibraryService` stores the library JSON;
  `PdfService` validates/stores an existing PDF.
- **ScanDevice** owns `IScannerService`, the WIA and fake implementations,
  scanner models/profiles, and scanner-configuration JSON persistence. Hardware
  access is isolated from WPF.
- **ScanApp** is the WPF presentation layer. `ScannerSettingsViewModel` owns
  scan-page, scanner-selection, status, profile, settings, and PDF-export state.
  Windows and UI event handlers are in the window code-behind; keep business
  logic in services/ViewModels when extending the app.
- **Theme resources** are loaded through `App.xaml`. Use semantic WPF resources
  in `Themes/DesignTokens.xaml` and `Themes/DarkTheme.xaml`, plus reusable
  control styles in `Themes/ControlStyles.xaml`; shared semantic tokens are
  sourced from `.github/skills/ui-design-system/assets/design-tokens.json`.
- **Agent UI skills** under `.github/skills/` are development guidance only;
  they are not application/runtime dependencies.

## Implemented behavior

### Scanner discovery and profiles

- On startup, the app discovers available WIA devices and presents readable
  names while preserving each device's `DeviceId`.
- Duplicate device names are disambiguated using part of the device ID.
- The scanner picker restores the most recently used scanner when available;
  selecting a scanner updates the footer status. Its final menu option opens
  Windows' scanner setup; restart the app after installing a device so it can be
  discovered.
- Users can add/edit an application-level alias/profile with scanner defaults
  (color mode, DPI, and source), or delete the profile without uninstalling the
  Windows device. Saved profiles remain visible when their device is offline.
- WIA operations use COM on an STA thread and release COM objects. WIA needs
  Windows and a compatible driver; no separate NuGet package/COM reference is
  currently required.

### Fake scanning and scan pages

- Set `QUICKSCAN_USE_FAKE_SCANNER=1` to select `FakeScannerService` instead of
  WIA. It supplies two simulated devices with the same friendly name and can
  generate a sample BMP page through the same `IScannerService` contract.
- **Quét trang** scans to a local temporary image, shows it in the preview and
  page list, and reports success/failure in the footer.
- The **Thêm ảnh / PDF** button sits below the batch-scan action and opens a
  multi-file selection dialog. Supported image files are copied into local scan
  storage; each PDF is rendered into PNG pages and appended to the page list.
- Scan pages can be selected, deleted, and reordered by drag-and-drop before
  export. The page list is designed to scroll within its panel.
- Batch scanning checks the selected device's ADF capability before
  enabling the action, rechecks before starting, appends each completed page to
  the page list, and can pause between pages. The fake scanner simulates a
  three-page ADF batch.
- Selected pages can be rotated left/right and flipped vertically/horizontally.
  The transformed preview is used by both the page thumbnail and final PDF
  export; image previews scale uniformly to fit their display areas.

### PDF export, library data, and app settings

- **Lưu thành PDF** combines the scanned page images in their current order,
  writes the result to the configured output folder, and records document
  metadata (name, category, page count, tags, and path) in the JSON library.
- The `.pdf` suffix is fixed in the filename UI. An optional timestamp suffix
  uses `yyyy-MM-dd_hh-mm-ss`; a numeric suffix avoids collisions within the
  same second. Existing files are not silently overwritten.
- The PDF information panel uses **Nơi lưu** instead of category. It lists
  immediate subfolders of the configured default PDF folder, with **Thư mục
  mặc định** always first. A small `+` button creates and selects a named
  subfolder.
- The PDF information panel provides common paper sizes and exports with fit-to-
  page scaling for A4, A5, A3, Letter, Legal, B5, and Tabloid. A successful
  save clears the filename and disables **Lưu thành PDF** until a new name is
  entered.
- The **Cài đặt** dialog lets the user choose the PDF output folder, choose the
  default scanner from saved profiles, and toggle timestamp suffixing. When no
  default has been saved, it preselects the currently selected scanner if that
  scanner has a saved profile.
- The **Cài đặt** dialog also provides Light, Dark, and System themes. Existing
  settings default to Light; System reads Windows' `AppsUseLightTheme`
  preference when the app starts. Theme colors are mapped through the WPF
  resource dictionaries and shared semantic UI tokens.
- App settings, scanner profiles, and library JSON are written atomically via
  a temporary file followed by replacement.
- The **Thư viện PDF** tab currently has the designed search/filter controls,
  grid columns, and empty state. Search/filter, refresh, open, rename, and
  delete interactions are not yet a complete wired library-management flow.

## Local application data and run modes

The running app stores mutable user data under
`%LocalAppData%\QuickScanManager\`:

| File/folder | Purpose |
| --- | --- |
| `scanner-config.json` | Hardware scanner profiles and last-used device |
| `app-settings.json` | Output folder, default scanner, timestamp option, theme |
| `storage\library.json` | Metadata for exported PDFs |
| `storage\pdfs\` | Default PDF output folder |
| `scans\` | Temporary images produced by scanning |

Fake-scanner mode uses separate `scanner-config.fake.json` and
`app-settings.fake.json` files so tests do not alter hardware-mode settings.

Build and run in PowerShell from `QuickScanManager/`:

```powershell
dotnet build .\QuickScanManager.sln
dotnet run --project .\src\ScanApp\ScanApp.csproj
```

Run with the fake scanner:

```powershell
$env:QUICKSCAN_USE_FAKE_SCANNER = "1"
dotnet run --project .\src\ScanApp\ScanApp.csproj
```

VS Code provides launch configurations for fake and real scanners. Both run
the `build QuickScanManager` task before launch. The app targets
`net10.0-windows` with WPF enabled. `ScanCore` targets `net10.0`.

## Development milestones

This timeline consolidates the work requested and implemented across prior
sessions. It records feature direction and current state rather than every
individual edit.

1. **Created the solution and project boundaries.** Added the
   `QuickScanManager.sln` solution with `ScanCore`, `ScanDevice`, and WPF
   `ScanApp`, plus local storage for PDFs and `library.json`.
2. **Designed the two-tab workflow.** Added the scan/export tab with a
   three-column setup, preview, and page/PDF-information layout, plus a
   scanned-files/library tab.
3. **Added scanner selection and configuration.** Implemented WIA discovery,
   device IDs, duplicate-name disambiguation, last-used-device restoration,
   JSON profiles/aliases, scanner defaults, and Windows scanner setup entry.
4. **Established shared UI design guidance.** Added the repository-local
   design-system, reusable-component, and cross-platform UI-adaptation skills,
   with semantic tokens and a WPF resource adapter.
5. **Refined the WPF presentation and debugging workflow.** Applied a compact,
   modern visual style based on shared tokens/Figma direction. Fixed a WPF
   startup crash caused by supplying a `Double` where `Border.Padding` requires
   a `Thickness`; added VS Code fake/real launch configurations and a solution
   build task.
6. **Added scanner simulation.** Implemented two fake scanner entries and
   sample BMP generation, controlled by `QUICKSCAN_USE_FAKE_SCANNER`, with
   separate fake-mode config files.
7. **Wired single-page scanning.** Connected the scan-page command to the
   shared scanner-service contract and added page preview/list state.
8. **Added page-list interactions.** Added page deletion and drag/drop
   reordering; bounded the page list in its panel rather than letting it push
   the PDF-information area.
9. **Implemented PDF output and footer feedback.** Added success/warning/error
   footer messaging, fixed `.pdf` filename display, PDF generation from the
   ordered scanned images, and library metadata persistence.
10. **Added application settings.** Added settings JSON persistence and a
    dialog for the default output folder, scanner profile, and optional
    timestamp naming. The last-used configured scanner is preselected when no
    default has yet been saved.
11. **Added selectable app themes.** Added Light, Dark, and Windows System
    options to application settings, persisted the choice, and mapped the dark
    palette through shared design tokens and dynamic WPF resources.
12. **Upgraded the solution to .NET 10.** Retargeted all projects, updated the
    VS Code launch paths and setup documentation, and verified the solution
    build with the installed .NET SDK.

## Current verification and known gaps

- Most recent solution build completed successfully with **0 warnings and
  0 errors** after upgrading all projects to .NET 10.
- The settings dialog was opened in the fake-scanner app; both saved fake
  profiles appeared, the last-used profile was selected by default, and saving
  produced the expected fake settings JSON. Temporary test settings and the
  test process were cleaned up.
- There is no separate automated test project in the current solution.
- Theme behavior is startup-loaded from the existing settings JSON. The System
  option follows Windows' app-theme preference on each app launch.
- Before extending an area, inspect the actual command/event bindings and
  current worktree diffs; some library and batch-scan UI is still a visual
  scaffold rather than a finished workflow.
- WPF `Border.Padding` is a `Thickness`, not a numeric `Double`. Keep this type
  distinction in mind when changing shared XAML resources.
- The repository may contain staged/unstaged changes from earlier work. Do not
  discard changes outside the current task.

## Suggested next implementation targets

1. Complete library loading, search/category/date filtering, open/open-location,
   rename/tag editing, and safe deletion while keeping `library.json` in sync.
2. Wire and verify batch scanning for ADF devices.
3. Add focused tests for page transforms, PDF page-size fitting, filename
   validation/collision handling, JSON stores, and fake scanner behavior where
   test infrastructure is introduced.
