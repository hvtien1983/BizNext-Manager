# Desktop UI patterns for BizNext

Professional Windows desktop application UI patterns. Use these for dashboard
layouts, sidebars, data grids, forms, and dialogs.

## Dashboard and main window layout

### Standard desktop application shell

```
┌─────────────────────────────────────────────────────┐
│ [File] [Edit] [View] [Tools] [Help]      [🔍] [👤] │  Menu bar
├─────────────────────────────────────────────────────┤
│ ⟨ ⟩  Home  |  Dashboard                              │  Breadcrumb or tab bar
├────────────────┬─────────────────────────────────────┤
│                │                                     │
│  Navigation    │        Main Content Area            │
│  Sidebar       │                                     │
│                │  - Cards / Panels                   │
│  • Dashboard   │  - Lists / Grids                    │
│  • Reports     │  - Forms                            │
│  • Settings    │                                     │
│                │                                     │
├────────────────┴─────────────────────────────────────┤
│ Status bar: Ready | Last updated: Nov 27, 2026       │
└─────────────────────────────────────────────────────┘
```

### Component breakdown

#### Menu bar (if used)
- Traditional `File`, `Edit`, `View`, `Tools`, `Help` menus
- Keep to standard conventions (File → Exit, Edit → Copy/Paste, etc.)
- Avoid mobile-style hamburger menus for primary navigation (use sidebar instead)

#### Breadcrumb or tab navigation
- Show current location: Home > Dashboard
- Optional breadcrumb trail for drill-down workflows
- Alternatively: Tab bar for primary sections (Dashboard, Reports, Settings)

#### Sidebar / main navigation
- **Fixed or collapsible**: Collapsible preferred for compact screens
- **Width**: 250–280px normal, 60px when collapsed (icon-only)
- **Active indicator**: Bright left border or background highlight (use `action.primary`)
- **Icons + text**: 24px icons + semantic body text
- **Spacing**: `spacing.4` (16px) between items
- **Hover**: `surface.hover` background change

#### Main content area
- **Margins**: `spacing.6` (24px) or more
- **Max width**: Consider content readability; not all content needs full width
- **Column grid**: Use CSS Grid or WPF Grid with proportional columns

#### Status bar (optional)
- Right-aligned timestamp (muted text)
- Left-aligned status message or mode indicator
- Useful for file editors, viewers, batch operations

## Dashboard design

Dashboards present key metrics, charts, lists, and actions in one view.

### Card-based layout (restrained)

```
┌─────────────────────┬─────────────────────┐
│ KPI: 1,234 Users    │ KPI: $45.6K Revenue │
├─────────────────────┼─────────────────────┤
│ Activity Chart (3mo)│ Recent Documents    │
│ [Chart area]        │ • Doc A      Today  │
│                     │ • Doc B      Yest.  │
│                     │ • Doc C      2d ago │
└─────────────────────┴─────────────────────┘
```

**Rules**:
- **Minimal cards**: Use cards only to group related content (not everywhere)
- **1–2px border**, `surface.default` background, light padding (`spacing.4`)
- **No excessive shadows** (prefer border)
- **Clear hierarchy**: Heading + metric/content + optional footer action

### KPI tiles

```xaml
<Border BorderBrush="{DynamicResource Brush.Border.Default}"
        BorderThickness="1"
        Padding="16"
        Background="{DynamicResource Brush.Surface.Default}">
  <StackPanel Spacing="8">
    <TextBlock Text="Active Users"
               Foreground="{DynamicResource Brush.Text.Muted}"
               FontSize="{DynamicResource Typography.Size.Label}" />
    <TextBlock Text="1,234"
               Foreground="{DynamicResource Brush.Text.Primary}"
               FontSize="{DynamicResource Typography.Size.Title}"
               FontWeight="Bold" />
    <TextBlock Text="↑ 12% vs last month"
               Foreground="{DynamicResource Brush.Text.Success}"
               FontSize="{DynamicResource Typography.Size.Caption}" />
  </StackPanel>
</Border>
```

### Chart integration

- **Title**: Above chart, body or label size, semibold
- **Legend**: Below or to the right; muted text, semantic colors only
- **Data labels**: Small, low-emphasis; avoid cluttering the chart
- **Responsive**: Resize gracefully at different window widths
- **Colors**: Use design token colors (don't hardcode chart library colors)

## Navigation sidebar

### Sidebar structure (expandable)

```
[≡]                          (Collapsed icon, click to expand)

[≡] BizNext Manager          (Expanded header)
━━━━━━━━━━━━━━━━━━━━━━━━━━

[🏠] Dashboard

[📊] Reports
  └─ Sales Report
  └─ User Activity

[⚙️] Settings

                             (Space)

[👤] Account
[🚪] Logout
```

### Interaction patterns
- **Hover**: Background lightens (`surface.hover`)
- **Active**: Left border (3px) in `action.primary` + background tint (`surface.selected`)
- **Submenu**: Indented, gray text, expandable arrow indicator
- **Collapse**: Icon-only view (60px wide) shows only icons; hover reveals labels

### Sidebar width and responsive
- **Desktop (≥1200px)**: 280px fixed
- **Tablet (800–1199px)**: 250px or collapsible
- **Compact (≥600px)**: Collapsible by default, 60px collapsed

## Data grid and table design

### Column layout
```
┌──────────┬─────────────┬──────────┬────────────┐
│ Name     │ Email       │ Status   │ Actions    │
├──────────┼─────────────┼──────────┼────────────┤
│ John D.  │ john@ex.com │ ✓ Active │ [Edit] […] │
│ Jane S.  │ jane@ex.com │ ✓ Active │ [Edit] […] │
│ Alex M.  │ alex@ex.com │ ✗ Paused │ [Edit] […] │
└──────────┴─────────────┴──────────┴────────────┘
```

### Header row
- **Font**: Semibold, `typography.role.label`
- **Background**: `surface.canvas` (optional, light shade)
- **Border**: 1px bottom, `border.default`
- **Sortable**: Add up/down arrow indicator on click
- **Frozen**: Consider freezing first column on horizontal scroll

### Data rows
- **Height**: 36–40px normal, 32px compact
- **Padding**: `spacing.3` (12px) horizontal
- **Striping** (optional): Alternate row backgrounds (canvas/default) every other row
- **Hover**: `surface.hover` background, 2px left indicator optional
- **Selection**: Row checkbox on left; `surface.selected` background on selected rows

### Empty state
```xaml
<StackPanel HorizontalAlignment="Center"
            VerticalAlignment="Center"
            Spacing="12">
  <TextBlock Text="No data available"
             Foreground="{DynamicResource Brush.Text.Muted}"
             FontSize="{DynamicResource Typography.Size.BodyLarge}" />
  <TextBlock Text="Try adjusting your filters"
             Foreground="{DynamicResource Brush.Text.Muted}"
             FontSize="{DynamicResource Typography.Size.Caption}" />
  <Button Content="Clear filters" Margin="0,8,0,0" />
</StackPanel>
```

### Error state
```xaml
<Border BorderBrush="{DynamicResource Brush.Border.Danger}"
        BorderThickness="1"
        Padding="12"
        Background="{DynamicResource Brush.Surface.Danger}">
  <StackPanel Spacing="4">
    <TextBlock Text="✕ Failed to load data"
               Foreground="{DynamicResource Brush.Text.Danger}"
               FontWeight="Bold" />
    <TextBlock Text="Please check your connection and try again."
               Foreground="{DynamicResource Brush.Text.Danger}" />
    <Button Content="Retry" Margin="0,8,0,0" />
  </StackPanel>
</Border>
```

## Form design

### Form layout
```
Layout: Labels above inputs (desktop standard)

┌─────────────────────┐
│ Project Name *      │
│ [________________]  │
│                     │
│ Description         │
│ [________________]  │
│ [________________]  │
│                     │
│ Status              │
│ [   Dropdown ▼]    │
│                     │
│          [Save] [Cancel]
└─────────────────────┘
```

### Input field patterns

#### Text input
```xaml
<StackPanel Spacing="4">
  <TextBlock Text="Project Name"
             Foreground="{DynamicResource Brush.Text.Primary}"
             FontWeight="SemiBold" />
  <TextBox Padding="12,8,12,8"
           BorderBrush="{DynamicResource Brush.Border.Default}"
           BorderThickness="1" />
</StackPanel>
```

#### Required field indicator
```xaml
<TextBlock>
  <Run Text="Project Name " />
  <Run Text="*" Foreground="{DynamicResource Brush.Text.Danger}" />
</TextBlock>
```

#### Validation error
```xaml
<Border BorderBrush="{DynamicResource Brush.Border.Danger}"
        BorderThickness="1"
        Padding="4"
        Visibility="{Binding ValidationError, Converter=StringVisibilityConverter}">
  <TextBlock Text="{Binding ValidationError}"
             Foreground="{DynamicResource Brush.Text.Danger}"
             FontSize="{DynamicResource Typography.Size.Caption}" />
</Border>
```

### Form spacing
- **Label to input**: `spacing.2` (8px)
- **Input to next label**: `spacing.4` (16px)
- **Sections**: `spacing.6` (24px)
- **Button area**: `spacing.4` (16px) margin-top

### Button placement
- **Primary button**: Left side (Save, Submit, Apply)
- **Secondary button**: Right of primary (Cancel, Reset)
- **Danger button**: Far right (Delete, Discard)

## Dialog and modal design

### Dialog structure
```
┌─────────────────────────────────────┐
│ Delete Document?               [✕]  │  Title bar
├─────────────────────────────────────┤
│ This action cannot be undone.       │
│                                     │  Content area
│ ⚠ Are you sure?                    │
│                                     │
│         [Cancel]  [Delete]  [Help]  │  Action buttons
└─────────────────────────────────────┘
```

### Dialog patterns
- **Title**: Top-left, bold, action-oriented ("Delete Document?" not "Warning")
- **Content**: Standard body text with status indicators
- **Close button**: Top-right [✕] only if cancelable (destructive dialogs should require explicit action)
- **Action buttons**: Primary action on right (highest attention), secondary/cancel on left

### Modal backdrop
- Semi-transparent overlay, `surface.canvas` or black at 40% opacity
- Prevents interaction with content behind the dialog
- Click outside dialog to close (if cancelable)

## Notification and feedback patterns

### Toast notification (temporary)
```
┌──────────────────────────────────┐
│ ✓ Document saved successfully    │  Auto-dismiss in 4-6s
└──────────────────────────────────┘
```

**Position**: Bottom-right corner, 16px margin
**Duration**: 4–6 seconds (longer for errors/warnings, shorter for success)
**Colors**: `surface.success` + `text.success` (or status color)

### Inline notification (persistent)
```
┌──────────────────────────────────┐
│ ⚠ Unsaved changes in this form   │  Visible until action taken
│ [Save] [Discard]                 │
└──────────────────────────────────┘
```

**Position**: Near the form or at the top of the view
**Dismiss**: Action buttons or manual close (X)

### Loading indicator
```
  ◔ Loading data...
  
(spinner animation, 3–4 second cycle)
```

**Position**: Near the control or center of area being loaded
**Text**: Brief, present tense ("Loading", "Saving", "Generating report")
**Disable action**: Button/controls should be disabled during async work

## Keyboard shortcuts and affordance

### Common shortcuts
- **Ctrl+S**: Save
- **Ctrl+N**: New
- **Ctrl+O**: Open
- **Ctrl+P**: Print
- **Ctrl+Z**: Undo
- **Ctrl+Y**: Redo
- **Escape**: Close dialog / Cancel operation
- **Enter**: Confirm primary action (if focused on dialog button)
- **Tab / Shift+Tab**: Navigate between controls
- **Alt+key**: Access menu (e.g., Alt+F for File menu)

### Shortcut display
- **Menu items**: "Save (Ctrl+S)" displayed next to menu text
- **Button tooltips**: Show shortcut hint on hover
- **Status bar**: Optional display of active shortcuts during operation

## Responsive and density considerations

### Normal density (default)
- Button height: 38px
- Input height: 36px
- Row height: 36–40px
- Padding/margin: Generous spacing per design token scale

### Compact density (data-heavy views)
- Button height: 34px
- Input height: 32px
- Row height: 32px
- Font size: 1–2px smaller (caption/small instead of body)
- Padding/margin: Use compact token values (`spacing.Compact`)

**When to use compact**:
- DataGrids with 50+ rows
- Dense lists (file explorers, library views)
- Not recommended for forms or primary actions (too cramped)

### Minimum window sizes
- **Desktop**: 1024×768 (old standard), better 1200×800+
- **Tablet / compact**: 800×600+
- **Mobile**: Not supported (this is desktop-only design)