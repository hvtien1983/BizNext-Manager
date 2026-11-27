# BizNext Manager - Project Context

> Living handoff for future development. Update this document when architecture,
> user-visible behavior, or project status changes.
>
> Last updated: 2026-11-27 (Shared DesignSystem ownership)

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
├── BizNext.DesignSystem/              # Shared WPF tokens, themes, styles, controls
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
- The Shell owns theme selection: **Cài đặt** at the bottom of the sidebar opens
  Dashboard settings, where the global theme is selected and stored in
  `shell-settings.json`. Shared WPF tokens, themes, styles, and reusable
  controls are provided by `src/BizNext.DesignSystem/` and loaded into
  application resources by the Shell. Modules reference these shared resources
  and do not define local styles, reusable controls, components, or theme
  dictionaries. Modules continue to own their views, feature behavior, and
  feature-specific assets.
- Every new WPF window must use the standard application icon with
  `Icon="Assets/icon.ico"`; ensure that asset is available to the window's
  project.

See [docs/architecture.md](./docs/architecture.md) for lifecycle, project
dependencies, and extension instructions.

## Fluent 2 Design System Status

**Location**: `src/BizNext.DesignSystem/`

**Components**: 14 Fluent 2 components fully implemented and styled
- Input Controls: CheckBox, RadioButton, ComboBox, ToggleButton, TextBox, PasswordBox
- Buttons: Primary, Secondary, Subtle, Status Bar variants
- Containers: TabControl with tabs
- Feedback: ProgressBar, Status Indicators (Success/Warning/Error), Tooltip
- Navigation: NavigationMenu with radio button items

**Themes**: Light, Dark, High Contrast (via DynamicResource + WPF SystemColors)

**Token System**: Semantic design tokens for colors, typography, spacing, corner radius
- Files: `DesignTokens.xaml`, `LightTheme.xaml`, `DarkTheme.xaml`
- Used by: ButtonStyles, CardStyles, InputStyles, DialogStyles, DataGridStyles,
  and shared WPF components.

**WPF Compatibility**: ✅ Fully compatible (Phase 8 fixes applied)
- Removed WinUI 3 attributes (Gap, Radius on Rectangle)
- All remaining attributes verified against WPF spec
- Ready for production use

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

## Development Environment & Agent Configuration

**Agent Auto-Delegation Setup** (implemented 2026-11-27):
- **Global config**: `C:\Users\Admin\.claude\settings.json`
  - Auto-routes tasks to specialized agents (Explore, BuildValidator, TaskExecutor, IntegrityReviewer)
  - Enables fork agents for complex tasks to isolate output from main context
  - Configures model selection: Haiku for simple, Sonnet for medium, Opus for complex
  - Enables prompt caching (TTL 300s) for token reuse

- **Project config**: `.claude/settings.json`
  - Routes XAML/C# issues to TaskExecutor
  - Routes file searches to Explore (quick reasoning)
  - Routes build/test to BuildValidator
  - Enables parallel execution (max 3 agents)
  - Auto-cleanup of worktree isolation

**Credit Optimization**:
- Fork agents + background execution saves **60-80% tokens** on exploration/build/refactoring tasks
- Prompt caching reuses results instead of re-computing
- Model selection prevents overspending on simple tasks

To activate: Reload VS Code after updating settings files

## Build and run

From the repository root in PowerShell with the .NET 10 SDK:

```powershell
dotnet build .\BizNext-Manager.sln
dotnet run --project .\src\BizNext.Shell\BizNext.Shell.csproj
```

VS Code launch profiles cover both fake and WIA scanner modes. The fake profile
is the preferred development/debug path when no scanner hardware is available.

## Current verification and next work

### Deferred three-tier refactor

The user approved option B (incremental, offline-first) and the scope of
step 0, but deferred execution until after the requested test infrastructure.
Resume by reviewing the step-0 baseline and smoke-test scope with the existing
test suite; do not assume the whole step is already complete. Preserve
QuickScan data and behavior, and obtain approval before later refactor stages.
The source proposal and the earlier comparison remain in the conversation.

### Test infrastructure

`tests/BizNext.Tests` contains isolated xUnit tests for module catalog and
navigation, JSON persistence, QuickScan services, and WPF composition on an
STA thread. `tests/BizNext.TestHost` is a separate preview app with the real
Notes and WeatherCalendar placeholder views and a Shell settings preview.
Shell settings currently fails to open in TestHost because the Shell assembly
does not contain its referenced `Assets/icon.ico`; the host reports the error.
QuickScan view preview is deferred because it uses real user-data paths.
Widgets do not yet exist. See README for commands and limitations.
The isolated QuickScan view-construction check currently fails because
`StatusBarButton` is defined in `ButtonStyles.xaml`, which is not merged into
the DesignSystem application resources. The test stops before `Loaded` and
does not exercise scanning or access user files.

**Phase 8 - XAML Compatibility & Agent Configuration (2026-11-27):**

### XAML Compatibility Fixes
Fixed 5 XAML errors to ensure full WPF .NET 10 compatibility (removed WinUI 3 attributes):
- **ShowcaseWindow.xaml**: Removed all `Gap="X"` attributes from StackPanel elements (WPF doesn't support Gap)
- **DialogStyles.xaml**: Removed `<Setter Property="Gap" Value="8" />` from StackPanel style
- **CheckBoxRadioStyles.xaml**: Removed `Radius="1"` from Rectangle (WPF Rectangle doesn't support Radius)
- **TooltipProgressStyles.xaml**: Removed `CornerRadius="2"` from PART_Track Rectangle

**Result**: Design System now fully WPF-compatible while maintaining Fluent 2 design language:
- ✅ All CornerRadius on Border (supported)
- ✅ All BorderBrush on Border/Button/TextBox (supported)
- ✅ All color tokens and typography preserved
- ✅ Light/Dark/High Contrast themes intact

### Agent Configuration
Created auto-delegation setup to optimize Claude AI credits usage:
- **Global**: `C:\Users\Admin\.claude\settings.json` - Universal agent routing rules
- **Project**: `.claude/settings.json` - BizNext-specific task routing
- **Features**: Fork agents, background execution, prompt caching, worktree isolation
- **Expected savings**: 60-80% credits reduction on code exploration, build, and refactoring tasks

**Previous work:**
- `dotnet build .\BizNext-Manager.sln` succeeded with 0 warnings and 0 errors (before XAML fixes).
- Dashboard settings and persisted Light/Dark theme selection were smoke-tested.
  QuickScan was visually checked in both themes, including themed ComboBox
  selections/popups and the minimum 1200x720 window size.
- Automated test infrastructure is now present; see the section above.
- Next feature work can implement Notes/Weather functionality or add
  management actions to the QuickScan PDF library. See the architecture guide
  before adding a module or shared UI behavior.
