# BizNext Manager - Project Context

> Living handoff for future development. Update this document when architecture,
> user-visible behavior, or project status changes.
>
> Last updated: 2026-11-26

## Project overview

BizNext Manager is a Windows WPF/.NET 10 dashboard that hosts focused business
utilities as modules. `BizNext.Shell` is the executable; `QuickScan` is the
first complete module, while Notes and Weather/Calendar are registered
placeholders.

## Solution structure

```text
BizNext-Manager.sln
src/
├── BizNext.Contracts/                 # IBizNextModule and shared metadata
├── BizNext.Infrastructure/            # Module catalog/navigation, JSON storage
├── BizNext.Modules.QuickScan/         # QuickScan UserControl and workflows
├── BizNext.Modules.Notes/              # Future Notes utility
├── BizNext.Modules.WeatherCalendar/    # Future calendar/weather utility
├── BizNext.Shell/                      # WPF entry point and dashboard host
├── ScanCore/                           # QuickScan PDF/library support
└── ScanDevice/                         # WIA/fake scanner support
docs/architecture.md
```

`ScanCore` and `ScanDevice` remain independent support projects for the
QuickScan module; they are not dashboard modules.

## Architecture and module contract

- `IBizNextModule` supplies `BizNextModuleMetadata` and creates a WPF view.
- Current modules are statically registered by the Shell; there is no dynamic
  assembly discovery/plugin loader.
- `ModuleCatalog` validates module IDs and `ModuleNavigationService` owns
  selection state.
- The Shell lazily creates and caches module views to preserve their in-memory
  state while navigating.
- Shell navigation state is persisted atomically by shared
  `JsonFileStorage`.
- QuickScan's user control retains the scanner, page editing, PDF export,
  settings, and existing in-module scan/library tabs. It has no title header
  (the Shell header names the module); its **Cài đặt** button sits at the right
  of its status bar, and its settings dialog no longer contains a theme option.
  Its modal dialogs use the host window as owner.
- The Shell owns theming: **Cài đặt** at the bottom of the sidebar opens
  Dashboard settings, where the global theme is selected and stored in
  `shell-settings.json`. Shared tokens/themes/control styles in
  `src/BizNext.Shell/Themes/` are provided to all modules (adapting the
  semantic source tokens in `.github/skills/ui-design-system/`). QuickScan-only
  styles live in the module (`Themes/QuickScanStyles.xaml`).

See [docs/architecture.md](./docs/architecture.md) for lifecycle, project
dependencies, and extension instructions.

## Existing capabilities

QuickScan continues to support:

- WIA scanner discovery and configuration, plus a fake scanner selected with
  `QUICKSCAN_USE_FAKE_SCANNER=1`.
- Single-page and ADF batch scanning, image/PDF import, page reorder/delete,
  rotation and flips.
- PDF creation with configurable output location, paper size, timestamp naming,
  and library metadata persistence.
- PDF library browsing by filename and selected storage folder, including
  child-folder navigation, sortable file details, and an in-app PDF viewer.
- Light, Dark, and Windows System themes, selected from Dashboard settings.
- The project-specific WPF workflow is documented in
  `.github/skills/wpf/SKILL.md`.

PDF library rename, tag editing, and deletion are still not implemented.

## User data and compatibility

- Shell state (last module and theme): `%LocalAppData%\BizNext-Manager\shell-settings.json`.
- Existing QuickScan settings, scanner profiles, library, PDFs, and temporary
  scans remain under `%LocalAppData%\QuickScanManager\`.
- Fake mode continues to use its existing separate settings/configuration
  files. No user-data migration or deletion is performed by the host upgrade.

## Build and run

From the repository root in PowerShell with the .NET 10 SDK:

```powershell
dotnet build .\BizNext-Manager.sln
dotnet run --project .\src\BizNext.Shell\BizNext.Shell.csproj
```

VS Code launch profiles cover both fake and WIA scanner modes. The fake profile
is the preferred development/debug path when no scanner hardware is available.

## Current verification and next work

- `dotnet build .\BizNext-Manager.sln` succeeded with 0 warnings and 0 errors.
- Dashboard settings and persisted Light/Dark theme selection were smoke-tested.
  QuickScan was visually checked in both themes, including themed ComboBox
  selections/popups and the minimum 1200x720 window size.
- There is no separate automated test project currently.
- Next feature work can implement Notes/Weather functionality or add
  management actions to the QuickScan PDF library. See the architecture guide
  before adding a module or shared UI behavior.
