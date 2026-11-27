# Fluent 2 Component Library & Design System Instructions

**Last Updated:** 2026-11-27  
**Status:** Phase 7/7 Complete (Knowledge Base + Design Tokens + Component Library + Showcase)  
**Architecture:** A (Built-in Fluent theme base + BizNext token layer + custom templates)

---

## Quick Start

### When to Use This Guide
- **Styling UI** in BizNext Manager (WPF .NET 10)
- **Adding new views, controls, or screens**
- **Ensuring Light/Dark/High Contrast consistency**
- **Applying Fluent 2 design principles**

### Component Usage Pattern
```xaml
<!-- Import DesignSystem resources (auto-loaded via App.xaml) -->
<Button Style="{StaticResource Fluent.Button.Primary}" Content="Save" />
<CheckBox Style="{StaticResource Fluent.CheckBox}" Content="Agree" />
<TextBox Style="{StaticResource InputTextBox}" Text="Enter text" />
```

---

## Available Components (14+)

| Category | Components | Style Key |
|----------|-----------|-----------|
| **Input** | CheckBox, RadioButton, ComboBox, ToggleButton, TextBox, PasswordBox | `Fluent.CheckBox`, `Fluent.RadioButton`, `Fluent.ComboBox`, `Fluent.ToggleButton`, `InputTextBox`, `Fluent.PasswordBox` |
| **Buttons** | Primary, Secondary, Subtle | `Fluent.Button.Primary`, `Fluent.Button.Secondary`, `Fluent.Button.Subtle` |
| **Containers** | TabControl, TabItem | `Fluent.TabControl`, `Fluent.TabItem` |
| **Feedback** | ProgressBar, Tooltip, Status Indicator | `Fluent.ProgressBar`, `Fluent.ToolTip`, `Fluent.StatusIndicator.Success/Warning/Error` |
| **Navigation** | NavigationMenu Item | `Fluent.NavigationMenu.Item` |
| **Layout** | Card, Dialog | `SectionCard`, `Fluent.Dialog` |

---

## Design Tokens (Semantic)

### Colors
- **Action (Brand):** `ActionPrimaryBrush` (#0078D4 light, #60B0FF dark) = Windows accent
- **Text:** `TextPrimaryBrush`, `TextSecondaryBrush`, `TextMutedBrush` (≥4.5:1 contrast)
- **Surface:** `SurfaceDefaultBrush`, `SurfaceHoverBrush`, `SurfaceSelectedBrush`
- **Status:** `TextSuccessBrush`, `TextWarningBrush`, `TextDangerBrush`
- **Border:** `BorderDefaultBrush`, `BorderFocusBrush` (action primary when focused)

### Spacing (4px scale)
- Tokens: `Spacing2XSmall` (4px), `SpacingXSmall` (8px), `SpacingSmall` (12px), `SpacingMedium` (16px), `SpacingLarge` (20px), `SpacingXLarge` (24px)

### Shape
- **Control Corner:** `ControlCornerRadius` = 4px
- **Overlay Corner:** `OverlayCornerRadius` = 8px
- **Pill (full round):** `PillCornerRadius` = 999px

### Typography
- **Font:** Segoe UI Variable, fallback Segoe UI
- **Minimum size:** 12px Regular / 14px Semibold
- **Tokens:** `CompactControlFontSize` (12px), `CompactLabelFontSize` (12px), etc.

### Motion
- **Durations:** `MotionDurationEnter` (83ms), `MotionDurationMedium` (167ms), `MotionDurationExit` (250ms)
- **Easing:** KeySpline `(0,0,0,1)` for enter, `(1,0,1,1)` for exit

---

## State Matrix (All Components)

| State | Visual Treatment |
|-------|-----------------|
| **Rest** | Default colors, no border |
| **Hover** | `SurfaceHoverBrush` background (or border lighten) |
| **Pressed/Active** | Opacity 0.86 (or darker shade) |
| **Selected/Checked** | `SurfaceSelectedBrush` background + accent indicator |
| **Focused (Keyboard)** | `BorderFocusBrush` stroke 2px (thicker than normal 1px) |
| **Disabled** | Opacity 0.48, foreground `TextMutedBrush` |
| **Error/Warning/Success** | Semantic color (red/orange/green) + icon + text |
| **Busy** | Pulsing or rotating indicator (animation TBD) |

---

## Light/Dark/High Contrast

### Auto-Switching
All colors use `DynamicResource`:
```xaml
<SolidColorBrush x:Key="TextPrimaryBrush" Color="#172033" />  <!-- Light -->
<!-- Dark theme override in DarkTheme.xaml -->
<SolidColorBrush x:Key="TextPrimaryBrush" Color="#F3F5FA" />  <!-- Dark -->
```

**Result:** No code changes needed. Light/Dark swap automatically.

### High Contrast
Windows OS setting (Settings → Ease of Access → Display → High Contrast):
- `SystemColors` override `DynamicResource` brushes automatically
- Ensure all UI elements use `DynamicResource` (not hard-coded hex)
- Text contrast ≥ 4.5:1 on white backgrounds (verified in token design)

### Testing Theme Modes
```csharp
// Light
(Application.Current)?.Resources.SetValue(FrameworkElement.ThemeModeProperty, ThemeMode.Light);
// Dark
(Application.Current)?.Resources.SetValue(FrameworkElement.ThemeModeProperty, ThemeMode.Dark);
// System (follows Windows OS setting)
(Application.Current)?.Resources.SetValue(FrameworkElement.ThemeModeProperty, ThemeMode.System);
```

**Note:** Settingg `ThemeMode` in code is experimental API (WPF0001 warning). Prefer XAML-only or external theme manager.

---

## Accessibility Checklist

✓ **Keyboard Navigation**
- Tab through controls: `IsKeyboardFocused` triggers 2px focus border
- Buttons: Enter/Space to activate
- ComboBox: Arrow keys to navigate items
- RadioButton: Arrow keys to cycle options
- Tab order follows visual order (use `TabIndex` to override if needed)

✓ **Focus Indicators**
- All controls show `BorderFocusBrush` (blue accent) when keyboard-focused
- Focus ring 2px thick (thicker than default 1px for visibility)
- Never set `FocusVisualStyle="{x:Null}"` without custom focus logic

✓ **Color Contrast**
- `TextPrimaryBrush` on `SurfaceDefaultBrush`: ≥7:1 (black/white)
- `TextSecondaryBrush` on `SurfaceDefaultBrush`: ≥4.5:1
- `TextMutedBrush` on `SurfaceDefaultBrush`: ≥4.5:1 (verified)
- Never use color alone to convey information (combine with icon/text)

✓ **Screen Reader**
- Use `AutomationProperties.Name` for icon-only buttons
- Buttons have `RecognizesAccessKey` (supports `_` underline)
- Labels with `TargetUpdated` binding for dynamic content
- DataGrid has accessible cell templates

✓ **High Contrast**
- All brush references use `DynamicResource` (auto high-contrast)
- No hard-coded hex colors in control templates
- Text always contrast ≥4.5:1

---

## Common Patterns

### Custom Button (Primary + Secondary)
```xaml
<StackPanel Orientation="Horizontal" Gap="8">
    <Button Style="{StaticResource Fluent.Button.Primary}" Content="Save" />
    <Button Style="{StaticResource Fluent.Button.Secondary}" Content="Cancel" />
</StackPanel>
```

### Form with Labels & Input
```xaml
<StackPanel Gap="8">
    <TextBlock Style="{StaticResource FieldLabel}" Text="Email:" />
    <TextBox Style="{StaticResource InputTextBox}" Text="{Binding Email}" />
    
    <TextBlock Style="{StaticResource FieldLabel}" Text="Agree:" />
    <CheckBox Style="{StaticResource Fluent.CheckBox}" Content="I accept the terms" />
</StackPanel>
```

### Status Message (Success/Error)
```xaml
<!-- Success -->
<Border Style="{StaticResource Fluent.StatusIndicator.Success}">
    <StackPanel Orientation="Horizontal" Gap="8">
        <TextBlock Text="✓" />
        <TextBlock Style="{StaticResource Fluent.StatusText.Success}" Text="Operation completed" />
    </StackPanel>
</Border>

<!-- Error -->
<Border Style="{StaticResource Fluent.StatusIndicator.Error}">
    <StackPanel Orientation="Horizontal" Gap="8">
        <TextBlock Text="✕" />
        <TextBlock Style="{StaticResource Fluent.StatusText.Error}" Text="An error occurred" />
    </StackPanel>
</Border>
```

### Navigation Sidebar
```xaml
<StackPanel Orientation="Vertical" Gap="4">
    <RadioButton Style="{StaticResource Fluent.NavigationMenu.Item}"
                 Content="Home"
                 IsChecked="True" />
    <RadioButton Style="{StaticResource Fluent.NavigationMenu.Item}"
                 Content="Settings" />
    <RadioButton Style="{StaticResource Fluent.NavigationMenu.Item}"
                 Content="Help" />
</StackPanel>
```

---

## Rules & Constraints

### DO
- ✓ Use `DynamicResource` for all brushes (Light/Dark consistency)
- ✓ Use token keys from `DesignTokens.xaml` (never hard-code colors)
- ✓ Test keyboard navigation (Tab, Enter, Arrow keys)
- ✓ Provide `AutomationProperties.Name` for icon-only buttons
- ✓ Verify contrast ≥4.5:1 for text on backgrounds
- ✓ Use Fluent style keys: `Fluent.Button.Primary`, `Fluent.CheckBox`, etc.
- ✓ Combine color + icon + text for status/error messages
- ✓ Use `CornerRadius` token (4px for controls, 8px for overlays)

### DON'T
- ✗ Hard-code hex colors (use token brush instead)
- ✗ Set `FocusVisualStyle="{x:Null}"` without custom focus logic
- ✗ Use `Opacity` alone for disabled state (use `TextMuted` foreground)
- ✗ Rely on color alone for accessibility (add icon/text)
- ✗ Create new control styles without reference to Fluent pattern
- ✗ Mix gradient, shadow, blur unless explicitly required (keep minimal)
- ✗ Use font sizes <12px for primary text (Windows minimum)
- ✗ Disable keyboard focus ring (accessibility violation)

---

## Troubleshooting

### Component Not Styling
**Problem:** Button/CheckBox not styled despite `Style` attribute  
**Solution:** Ensure `BizNext.DesignSystem` assembly is loaded. Check App.xaml merges all theme files.

### Light/Dark Not Switching
**Problem:** Theme toggle button doesn't update colors  
**Solution:**  
1. Verify all brushes use `{DynamicResource BrushName}` (not `{StaticResource}`)
2. Check `ThemeMode` is set on Application or window
3. Test in ShowcaseWindow first (known working)

### Keyboard Focus Not Visible
**Problem:** Tab through controls but no focus indicator  
**Solution:**  
1. Verify `FocusVisualStyle != "{x:Null}"` in template
2. Check `BorderFocusBrush` token is defined
3. Ensure trigger `IsKeyboardFocused="True"` sets `BorderBrush` + `BorderThickness="2"`

### Text Not Readable (Contrast Issue)
**Problem:** Text too faint on background  
**Solution:**  
1. Check contrast ratio in design token (target ≥4.5:1)
2. Verify `TextMutedBrush` is #5B6B7F (not old #748099)
3. Use `TextPrimaryBrush` or `TextSecondaryBrush` for headings

---

## File Locations

| File | Purpose |
|------|---------|
| `src/BizNext.DesignSystem/Themes/DesignTokens.xaml` | Semantic color/shape/typography tokens (Light theme) |
| `src/BizNext.DesignSystem/Themes/DarkTheme.xaml` | Dark theme color overrides |
| `src/BizNext.DesignSystem/Themes/Shared/CheckBoxRadioStyles.xaml` | CheckBox, RadioButton styles |
| `src/BizNext.DesignSystem/Themes/Shared/SelectionStyles.xaml` | ComboBox, ComboBoxItem styles |
| `src/BizNext.DesignSystem/Themes/Shared/ToggleSwitchStyles.xaml` | ToggleButton style |
| `src/BizNext.DesignSystem/Themes/Shared/TabControlStyles.xaml` | TabControl, TabItem styles |
| `src/BizNext.DesignSystem/Themes/Shared/DialogStyles.xaml` | Dialog, DialogButtonPanel styles |
| `src/BizNext.DesignSystem/Themes/Shared/TooltipProgressStyles.xaml` | Tooltip, ProgressBar styles |
| `src/BizNext.DesignSystem/Themes/Shared/StatusIndicatorStyles.xaml` | Status indicator styles (Success/Warning/Error) |
| `src/BizNext.DesignSystem/Themes/Shared/NavigationMenuStyles.xaml` | NavigationMenu.Item style |
| `src/BizNext.DesignSystem/Themes/Shared/PasswordBoxStyles.xaml` | PasswordBox style |
| `src/BizNext.DesignSystem/Themes/Shared/ButtonVariantStyles.xaml` | Button variants (Primary/Secondary/Subtle) |
| `src/BizNext.DesignSystem/Themes/Shared/InputStyles.xaml` | TextBox, FieldLabel styles |
| `src/BizNext.DesignSystem/Themes/Shared/CardStyles.xaml` | Card, SectionCard styles |
| `src/BizNext.DesignSystem/Themes/Shared/DataGridStyles.xaml` | DataGrid, DataGridRow styles |
| `src/BizNext.DesignSystem/ShowcaseWindow.xaml` | Interactive demo of all components |
| `.github/skills/fluent2-design/` | Knowledge base (12 references + design principles) |

---

## Resources

- **Fluent 2 Design:** https://fluent2.microsoft.design
- **WPF Fluent Theme:** https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net90
- **Windows Design:** https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/
- **WCAG Accessibility:** https://www.w3.org/WAI/WCAG21/quickref/
- **ShowcaseWindow:** Launch from BizNext.DesignSystem to see all components live

---

## Questions?

Refer to `.github/skills/fluent2-design/references/` for detailed design principles, token specifications, and WPF adaptation guidance.
