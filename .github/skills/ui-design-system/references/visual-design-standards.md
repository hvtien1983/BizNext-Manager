# Visual design standards for BizNext UI

A comprehensive guide to applying visual design principles across all desktop
applications. These standards build on the shared semantic tokens and ensure
consistency in hierarchy, rhythm, and professional appearance.

## Visual hierarchy

### Primary vs. Secondary vs. Tertiary weight

Establish clear visual priority using **size + color contrast + position**.

#### Primary elements (highest emphasis)
- Main action buttons (`action.primary` color, bold weight)
- Primary headings (`heading` size, 22px, bold)
- Key numeric values in dashboards (larger size, darker color)
- **Position**: Top-left, center, or leading edge

**Example**: "GENERATE REPORT" primary button on a report builder screen

#### Secondary elements (supporting emphasis)
- Secondary buttons (`action.secondary`, normal weight, secondary text color)
- Section headings (`bodyLarge` size, 14px, semibold)
- Input field labels (`label` size, semibold)
- Selected state indicators

**Example**: "Cancel" button next to primary button, clearly subordinate

#### Tertiary elements (low emphasis)
- Body text (`body` size, 12px, regular, `text.muted`)
- Helper text below inputs
- Disabled controls (faded appearance)
- Timestamps, metadata

**Example**: "Last updated: Nov 27, 2026" in muted gray

### Visual rhythm and spacing

Use the spacing scale to create predictable visual rhythm:

```
Components (micro):    4px, 8px
Groups (macro):        12px, 16px, 20px, 24px
Sections:              32px, 40px
Page margins:          16px minimum
```

**Rule**: Double-check spacing is consistent within a view—no random gaps.

## Color theory and application

### Semantic roles, not hex values

Do not say "use #356AE6" in a design spec. Say "use `action.primary` for
the call-to-action button" so the dark theme updates automatically.

### Primary color: action.primary
- **Light theme**: `#356AE6` (Blue 600)
- **Dark theme**: `#7EA0FF` (Blue lighter)
- **Use**: Main buttons, active tabs, focus states, primary links
- **Avoid**: Large backgrounds (too dominant)

### Neutral palette: slate shades
- **Slate 950** (`text.primary`): Main text, highest contrast
- **Slate 700** (`text.secondary`): Supporting text, secondary labels
- **Slate 500** (`text.muted`): Hints, metadata, disabled text
- **Slate 100** (`surface.canvas`): App background (safe for large areas)
- **White** (`surface.default`): Content surfaces, cards, modals

**Gradient principle**: Use only adjacent shades (950 → 700 → 500) for text hierarchy.
Avoid skipping levels or using palette.blue in text.

### Semantic status colors

| Status | Light theme | Dark theme | Use |
|--------|---|---|---|
| **Success** | Green 600 / Green 100 | Green 50s | Save, upload, validation pass |
| **Warning** | Amber 800 / Amber 50 | Amber lighter shades | Unsaved changes, confirmations |
| **Error** | Red 600 / subtle bg | Red lighter | Validation fail, destructive actions |
| **Info** | Blue 600 (primary) / Blue 50 | Blue lighter | Informational messages |

**Rule**: Status color = text + background + border. Never color-only.

### Contrast and accessibility

- **Text on light backgrounds**: Use `text.primary` (slate 950) for ≥4.5:1 contrast
- **Text on colored backgrounds**: Use `text.onPrimary` (white) for primary actions
- **Text on muted backgrounds**: Use `text.secondary` or `text.muted` (slate 700/500)
- **Check tools**: Use WebAIM contrast checker for edge cases

## Typography

### Font family
- **System font**: Segoe UI (Windows native)
- **Fallbacks**: Arial, sans-serif
- **Rationale**: Native rendering, consistent with OS

### Type scale (semantic roles, not raw sizes)

| Role | Size | Weight | Use | Example |
|------|------|--------|-----|---------|
| `heading` | 22px | bold | Page titles, section headers | "Dashboard" |
| `title` | 18px | bold | Subsection titles | "Recent Activity" |
| `bodyLarge` | 14px | regular | Prominent body text, form labels | List item text |
| `body` | 12px | regular | Standard body text | Main content, descriptions |
| `label` | 11px | semibold | Input labels, button text | "Name:", "Save" |
| `small` | 11px | regular | Supporting text | Timestamps |
| `caption` | 10px | regular | Minimal text, tooltips | Counts, metadata |

### Line height and spacing
- **Headings**: 1.2–1.3× font size (tighter, more authoritative)
- **Body text**: 1.5–1.6× font size (readable in long passages)
- **Input labels**: 1.0–1.1× (compact, single-line)

**Example for body (12px)**:
- Line height: 18–20px (12 × 1.5–1.67)
- Paragraph spacing: 12px (use `spacing.3`)

## Spacing & layout system

### 8px modular grid

All spacing uses multiples of 4px or 8px. The scale is 1, 2, 3, 4, 5, 6, 8, 10, 12
(= 4px, 8px, 12px, 16px, 20px, 24px, 32px, 40px, 48px).

#### Micro spacing (component internals)
- Button padding: `spacing.4` horizontal (16px) × `spacing.2` vertical (8px)
- Input field padding: `spacing.3` (12px) all sides
- Icon-button spacing: `spacing.2` (8px) between icon and text

#### Macro spacing (between components/groups)
- Form field gap: `spacing.3` (12px) between label and input
- Section gap: `spacing.4` (16px) or `spacing.6` (24px) between sections
- Modal padding: `spacing.6` (24px) around content

#### Page/view margins
- Minimum safe margin: `spacing.4` (16px)
- Large screens: `spacing.6` (24px) or more
- Compact/mobile: `spacing.3` (12px)

### Content alignment

**Align left**: Text, labels, lists (Western reading order)
**Center**: Modals, error states (draws attention)
**Right**: Numbers in tables (compare magnitudes), timestamps (trailing detail)

## Shapes and borders

### Border radius (minimal use)
- **Controls** (buttons, inputs, chips): `shape.radius.control` = 8px
- **Cards / panels**: `shape.radius.card` = 12px
- **Pills** (badges, pills): `shape.radius.pill` = 999px
- **Most content**: 0px (rectilinear, clean desktop aesthetic)

**Rationale**: Rounded corners have visual cost; use sparingly and consistently.

### Border width
- **Default**: `shape.borderWidth.default` = 1px (subtle separation)
- **Emphasis**: `shape.borderWidth.emphasis` = 2px (focus, active, strong states)
- **Avoid**: 3px+ borders (too heavy for desktop UIs)

### Border colors
- **Default**: `border.default` (slate 200 light / 384254 dark)
- **Hover/emphasis**: `border.subtle` (slightly lighter/darker)
- **Focus**: `border.focus` (primary blue)
- **Status**: `border.success`, `border.warning`, `border.danger`

## Shadows and elevation (restrained)

Shadows are **rarely used** in modern desktop UX. Prefer borders.

### When to use elevation
- **Card separation**: Only if the card is floating over content
- **Modal overlay**: Subtle shadow behind the modal dialog (restrained)
- **Hover**: Very subtle lift (optional, borders often sufficient)

### Shadow implementation in WPF
Avoid complex `DropShadowEffect` by default. Use:
```xaml
<!-- Subtle card border instead of shadow -->
<Border BorderBrush="{DynamicResource Brush.Border.Default}"
        BorderThickness="1"
        Background="{DynamicResource Brush.Surface.Default}" />
```

If shadow is necessary (rare):
```xaml
<!-- Card with minimal shadow -->
<Grid Effect="{StaticResource SubtleDropShadow}">
  <!-- Content -->
</Grid>
<!-- SubtleDropShadow: BlurRadius=4, ShadowDepth=2, Opacity=0.1 -->
```

## Iconography

### Icon sizing
- **Small icons** (inline): 16px (in text, labels)
- **Standard icons** (buttons, sidebars): 24px (most common)
- **Large icons** (empty states, illustrations): 48px+ (draws attention)

### Status icon set (consistent across app)
- **Success**: Green checkmark ✓
- **Error**: Red X or circle ✕
- **Warning**: Amber triangle ⚠
- **Info**: Blue circle-i ⓘ
- **Loading**: Gray spinner ◔

### Icon-text pairing
- Icon + text together (not icon-only on first use)
- Icon on left, text on right (Western convention)
- Consistent spacing between icon and text: `spacing.2` (8px)

## Visual consistency and patterns

### Button family
- **Primary button**: Prominent, highest contrast, main action
- **Secondary button**: Lower emphasis, supporting action (outline or muted)
- **Danger button**: Red, destructive action only (delete, discard)
- **State variants**: Hover, pressed, focused, disabled, loading/busy

**Rule**: One primary button per screen/dialog (highest action priority).

### Form layout
- **Labels above inputs** (desktop standard)
- **Helper text below inputs** (not inside placeholder)
- **Error messages inline** (near the invalid field)
- **Field spacing**: `spacing.3` (12px) between label and input, `spacing.4` (16px) between fields

### DataGrid/Table
- **Column headers**: Semibold, sortable indicators
- **Row height**: Normal (36–40px), compact (32px for dense data)
- **Striped rows** (optional): Alternate `surface.canvas` / `surface.default` for readability
- **Hover state**: `surface.hover` (subtle background lighten)
- **Selected rows**: `surface.selected` (blue tint)

### Dialog and modals
- **Title**: Primary heading, top-left
- **Content**: Standard body text with semantic tokens
- **Action buttons**: Primary (confirm) + secondary/danger (cancel/destructive)
- **Focus**: Primary action receives focus on open; focus returns to opener on close

### Sidebar navigation
- **Active indicator**: `action.primary` border or background on current page
- **Hover**: `surface.hover` background
- **Icons**: 24px, `text.muted` → `text.primary` on hover/active
- **Text**: `body` size, `text.primary`, left-aligned

## Light and dark theme application

### Theme switching philosophy
- **Same semantic meaning**: Success is green, warning is amber, in both themes
- **Different execution**: Dark theme uses lighter shades of the same colors
- **Equal quality**: Both themes are fully polished (not "dark mode" as an afterthought)
- **Automatic updates**: Use `DynamicResource` in XAML so theme changes apply instantly

### Light theme (default)
- Background: `surface.canvas` (slate 100, light gray)
- Content surface: `surface.default` (white)
- Text: `text.primary` (slate 950, dark)
- Accent: `action.primary` (blue 600)

### Dark theme
- Background: `surface.canvas` (slate 950 dark, almost black)
- Content surface: `surface.default` (slate 950 dark)
- Text: `text.primary` (slate 50, light gray)
- Accent: `action.primary` (blue lighter, `#7EA0FF`)

### Validation checklist
- [ ] Switch theme and inspect all surfaces
- [ ] Text remains readable (contrast ≥4.5:1)
- [ ] Status colors (success/warning/error) are distinct in both themes
- [ ] Focus indicators visible in both themes
- [ ] No hardcoded colors (all via tokens/`DynamicResource`)