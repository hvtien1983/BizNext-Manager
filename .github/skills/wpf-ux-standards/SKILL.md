---
name: wpf-ux-standards
description: "Use when implementing interaction states, keyboard navigation, accessibility, loading states, and user feedback in WPF. Apply semantic state indicators and ensure WCAG compliance."
compatibility: "Requires a WPF project on .NET or .NET Framework."
---

# WPF UX standards

Implement consistent interaction patterns, accessibility, and user feedback in WPF.
Refer to [`ui-component-patterns`](../ui-component-patterns/SKILL.md) and
[`wpf-ux-guidelines`](./references/wpf-ux-guidelines.md) for detailed patterns.

## Workflow

1. Define the control's interaction states before styling: normal, hover, focused,
   pressed, disabled, busy/loading, and semantic states (success, warning, error).
2. Implement each state with distinct visual feedback using semantic tokens:
   background, border, text, and icon colors from the design system.
3. Ensure focus is always visible via border color, background, or outline change.
4. Preserve keyboard navigation: tab order, arrow keys for lists/grids, Enter/Space
   for activation, Escape to close modals.
5. Set accessible names (`AutomationProperties.Name`) and meaningful automation IDs.
6. Test with keyboard-only navigation and screen reader (Narrator on Windows).
7. Validate at multiple window sizes and both light and dark themes.

## Interaction states

### Control state matrix
Every interactive control must implement:

| State | Visual treatment | Semantic use |
|-------|-----------------|------|
| **Normal** | Base color, standard borders | Default, idle state |
| **Hover** (mouse only) | Subtle background lighten, hover border | User attention cue |
| **Focused** | Bright border (primary color), 2px thickness | Keyboard user navigation |
| **Pressed** | Darker background, pressed border | Active click/tap feedback |
| **Disabled** | Faded/desaturated colors, grayed text | Unavailable action |
| **Loading/Busy** | Spinner or progress indicator, reduced opacity | Async operation in progress |

### Semantic status states
Distinct from interaction states, these communicate operation results:

| Status | Icon | Background | Text | Border |
|--------|------|-----------|------|--------|
| **Success** | ✓ checkmark | `surface.success` | `text.success` | `border.success` |
| **Warning** | ⚠ triangle | `surface.warning` | `text.warning` | `border.warning` |
| **Error** | ✕ circle | `surface.danger` (subtle) | `text.danger` | `border.danger` |
| **Info** | ⓘ circle | `surface.info` (subtle) | `text.info` | `border.info` |
| **Loading** | ◔ spinner | `surface.canvas` | `text.muted` | `border.default` |

### Button state example (ControlTemplate triggers)
```xaml
<!-- Normal state -->
<Border Background="{DynamicResource Brush.Action.Primary}"
        BorderBrush="{DynamicResource Brush.Border.Default}" />

<!-- Hover state -->
<Trigger Property="IsMouseOver" Value="True">
  <Setter Property="Background" Value="{DynamicResource Brush.Action.PrimaryHover}" />
</Trigger>

<!-- Focused state (keyboard) -->
<Trigger Property="IsFocused" Value="True">
  <Setter Property="BorderBrush" Value="{DynamicResource Brush.Border.Focus}" />
  <Setter Property="BorderThickness" Value="2" />
</Trigger>

<!-- Pressed state -->
<Trigger Property="IsPressed" Value="True">
  <Setter Property="Background" Value="{DynamicResource Brush.Action.PrimaryPressed}" />
</Trigger>

<!-- Disabled state -->
<Trigger Property="IsEnabled" Value="False">
  <Setter Property="Background" Value="{DynamicResource Brush.Surface.Disabled}" />
  <Setter Property="Foreground" Value="{DynamicResource Brush.Text.Disabled}" />
  <Setter Property="BorderBrush" Value="{DynamicResource Brush.Border.Disabled}" />
</Trigger>
```

## Keyboard navigation

### Tab order and focus
- Tab moves through interactive controls in logical reading order
- Use `TabIndex` only when default order is incorrect
- Focus must be visible (border, outline, or background change)
- Always use semantic token colors for focus state (never remove focus indicator)

### Common keyboard patterns
```xaml
<!-- Button activation: Space or Enter -->
<Button Click="{Binding ConfirmCommand}" />

<!-- TextBox: Tab to navigate, Enter to submit (if inside form) -->
<TextBox />

<!-- ComboBox: Arrow keys to cycle, Enter to select, Escape to close -->
<ComboBox />

<!-- DataGrid: Arrow keys to navigate cells, Enter to edit, Escape to cancel -->
<DataGrid KeyDown="DataGrid_KeyDown" />

<!-- Dialog: Escape to close, Enter to confirm (if primary button is focused) -->
<Window PreviewKeyDown="Window_PreviewKeyDown" />
```

### Modal dialog focus management
```csharp
// Focus primary action on dialog open
public MyDialog()
{
    InitializeComponent();
    Loaded += (s, e) => { ConfirmButton.Focus(); };
}

// Return focus to opener on dialog close
private void OnDialogClosed()
{
    // Focus returns to the control that opened the dialog
    this.Owner?.Focus();
}
```

## Accessibility (WCAG AA compliance)

### Automation properties
```xaml
<!-- Set accessible names for screen readers -->
<Button Content="Delete" 
        AutomationProperties.Name="Delete document"
        AutomationProperties.HelpText="Permanently remove this document" />

<!-- Provide context for icon-only buttons -->
<Button Content="🔍"
        AutomationProperties.Name="Search"
        ToolTip="Search (Ctrl+F)" />

<!-- Label input fields -->
<Label Target="{Binding ElementName=nameInput}">Name:</Label>
<TextBox x:Name="nameInput"
         AutomationProperties.Name="User name"
         AutomationProperties.LabeledBy="{Binding ElementName=nameLabel}" />
```

### Color contrast
- Foreground/background contrast ≥ 4.5:1 for normal text (WCAG AA)
- Foreground/background contrast ≥ 3:1 for large text (18pt+)
- Do not use color alone to convey status; pair with icon, text, or pattern

### Text scaling
- Respect `SystemFonts` and Windows text scaling preferences
- Use `em` units (or semantic token sizes) instead of fixed pixel sizes
- Test at 100%, 125%, 150%, 200% scaling

### Screen reader support
- Use `AutomationProperties.Name` for all controls
- Label form fields explicitly; do not rely on placeholder text
- Announce dynamic content changes (loading, error messages)
- Provide meaningful button/link labels ("Delete account" not "Delete")

## Loading and async feedback

### Loading state implementation
```xaml
<!-- Show spinner and disable action during async operation -->
<Button x:Name="SubmitButton" Click="Submit_Click">
  <StackPanel Orientation="Horizontal">
    <Image x:Name="Spinner" Source="spinner.png" Visibility="Hidden" />
    <TextBlock Text="Submit" />
  </StackPanel>
</Button>

<!-- Code-behind -->
private async void Submit_Click(object sender, RoutedEventArgs e)
{
    SubmitButton.IsEnabled = false;
    Spinner.Visibility = Visibility.Visible;
    
    try
    {
        await _viewModel.SubmitAsync();
        // Show success state
    }
    catch (Exception ex)
    {
        // Show error state
    }
    finally
    {
        SubmitButton.IsEnabled = true;
        Spinner.Visibility = Visibility.Hidden;
    }
}
```

### Status notifications
```xaml
<!-- Toast notification -->
<Border x:Name="StatusToast"
        Background="{DynamicResource Brush.Surface.Success}"
        BorderBrush="{DynamicResource Brush.Border.Success}"
        Visibility="Hidden">
  <TextBlock Foreground="{DynamicResource Brush.Text.Success}"
             Text="Document saved successfully" />
</Border>

<!-- Inline error message -->
<Border BorderBrush="{DynamicResource Brush.Border.Danger}"
        BorderThickness="1"
        Padding="8"
        Visibility="{Binding ValidationError, Converter=StringVisibilityConverter}">
  <TextBlock Foreground="{DynamicResource Brush.Text.Danger}"
             Text="{Binding ValidationError}" />
</Border>
```

## Best practices

- **State consistency**: The same control type must have identical visual treatment
  across the app (all primary buttons look and behave the same).
- **Focus always visible**: Never remove focus indicators; ensure 2px or color
  change is applied.
- **Semantic use of color**: Use success/warning/error colors only for semantic
  states, not hierarchy.
- **Busy feedback**: Show spinner or progress during async work; disable the
  action to prevent double submission.
- **Error clarity**: Display errors inline and near the control that caused them;
  use semantic token colors + icon + text.
- **Keyboard-first**: Test all workflows using keyboard alone (no mouse).
- **Theme validation**: Verify interaction states and status colors in both light
  and dark themes.