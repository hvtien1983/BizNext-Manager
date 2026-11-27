---
name: wpf-visual-design
description: "Use when implementing visual design standards in WPF: ResourceDictionary structure, semantic tokens, control templates, light/dark theme switching, and design token mapping to XAML."
compatibility: "Requires a WPF project on .NET or .NET Framework."
---

# WPF visual design

Implement shared design tokens and visual standards in WPF using XAML resources,
ControlTemplates, and DesignTokens mappings. Refer to
[`ui-design-system`](../ui-design-system/SKILL.md) for the shared design language
and [`wpf-design-implementation`](./references/wpf-design-implementation.md) for
WPF-specific patterns.

## Workflow

1. Read the [shared design tokens](../ui-design-system/assets/design-tokens.json)
   and [visual design standards](../ui-design-system/references/visual-design-standards.md).
2. Inspect the target app's existing ResourceDictionary and theme adapter
   (typically `DesignTokens.xaml`, `LightTheme.xaml`, `DarkTheme.xaml` in the Shell).
3. Map semantic token values to XAML `SolidColorBrush` and `Color` resources
   using the naming convention `{Namespace}.{Role}.{Property}`.
4. Use `DynamicResource` in styles and templates so controls respect theme changes.
5. Implement control templates with semantic tokens: replace hardcoded colors,
   sizes, and spacing with token references.
6. Test theme switching at runtime: toggle light/dark theme and verify all
   surfaces, text, borders, and state indicators update correctly.
7. Validate compact and normal density layouts; ensure semantic density metrics
   scale correctly without breaking visual hierarchy.

## Design token implementation

### ResourceDictionary structure
```
App.xaml (merged resources)
├── Shell/Themes/DesignTokens.xaml     (shared token mappings)
├── Shell/Themes/LightTheme.xaml       (light-only token overrides)
├── Shell/Themes/DarkTheme.xaml        (dark-only token overrides)
└── [Module]/Themes/[Feature].xaml     (module-specific styles)
```

### Semantic token naming in XAML
```xaml
<!-- Color palette foundation (rarely used directly) -->
<Color x:Key="Palette.Blue.600">#356AE6</Color>
<Color x:Key="Palette.Slate.950">#172033</Color>

<!-- Semantic tokens (used in controls and styles) -->
<Color x:Key="Text.Primary">#172033</Color>
<Color x:Key="Text.Muted">#748099</Color>
<Color x:Key="Surface.Canvas">#F5F7FA</Color>
<Color x:Key="Surface.Default">#FFFFFF</Color>
<Color x:Key="Border.Default">#E5E9F0</Color>
<Color x:Key="Action.Primary">#356AE6</Color>

<!-- Brush resources (use in bindings and templates) -->
<SolidColorBrush x:Key="Brush.Text.Primary" Color="{StaticResource Text.Primary}" />
<SolidColorBrush x:Key="Brush.Surface.Canvas" Color="{StaticResource Surface.Canvas}" />
<SolidColorBrush x:Key="Brush.Border.Default" Color="{StaticResource Border.Default}" />
<SolidColorBrush x:Key="Brush.Action.Primary" Color="{StaticResource Action.Primary}" />
```

### Theme switching
Light theme is the app default; dark theme overrides tokens in `DarkTheme.xaml`:
```xaml
<!-- DarkTheme.xaml (merged after LightTheme) -->
<Color x:Key="Text.Primary">#F3F5FA</Color>
<Color x:Key="Text.Muted">#A0AABD</Color>
<Color x:Key="Surface.Canvas">#11151D</Color>
<Color x:Key="Surface.Default">#1B2230</Color>
<Color x:Key="Border.Default">#384254</Color>
<Color x:Key="Action.Primary">#7EA0FF</Color>

<!-- Brush resources update automatically via DynamicResource -->
```

## ControlTemplate patterns

Apply semantic tokens consistently to all interaction states:

### Button template example
```xaml
<ControlTemplate x:Key="PrimaryButtonTemplate" TargetType="Button">
  <Border x:Name="border"
          Background="{DynamicResource Brush.Action.Primary}"
          BorderBrush="{DynamicResource Brush.Border.Default}"
          BorderThickness="1"
          Padding="{DynamicResource Button.Padding}"
          CornerRadius="8">
    <ContentPresenter HorizontalAlignment="Center"
                      VerticalAlignment="Center"
                      Foreground="{DynamicResource Brush.Text.OnPrimary}" />
  </Border>
  <ControlTemplate.Triggers>
    <Trigger Property="IsMouseOver" Value="True">
      <Setter TargetName="border" Property="Background"
              Value="{DynamicResource Brush.Action.PrimaryHover}" />
    </Trigger>
    <Trigger Property="IsPressed" Value="True">
      <Setter TargetName="border" Property="Background"
              Value="{DynamicResource Brush.Action.PrimaryPressed}" />
    </Trigger>
    <Trigger Property="IsEnabled" Value="False">
      <Setter TargetName="border" Property="Background"
              Value="{DynamicResource Brush.Surface.Disabled}" />
      <Setter TargetName="border" Property="BorderBrush"
              Value="{DynamicResource Brush.Border.Disabled}" />
    </Trigger>
    <Trigger Property="IsFocused" Value="True">
      <Setter TargetName="border" Property="BorderBrush"
              Value="{DynamicResource Brush.Border.Focus}" />
      <Setter TargetName="border" Property="BorderThickness" Value="2" />
    </Trigger>
  </ControlTemplate.Triggers>
</ControlTemplate>
```

## Density tokens

Compact layouts use reduced padding and margins. Store as shared resources:
```xaml
<!-- Normal density (default) -->
<Thickness x:Key="Spacing.Section">16</Thickness>
<Thickness x:Key="Spacing.Field">12</Thickness>
<Thickness x:Key="Button.Padding">16,10,16,10</Thickness>

<!-- Compact density (opt-in for data-heavy views) -->
<Thickness x:Key="Spacing.Section.Compact">12</Thickness>
<Thickness x:Key="Spacing.Field.Compact">8</Thickness>
<Thickness x:Key="Button.Padding.Compact">12,8,12,8</Thickness>
```

Apply compact density via style:
```xaml
<Style x:Key="CompactDataGridStyle" TargetType="DataGrid">
  <Setter Property="Margin" Value="{DynamicResource Spacing.Section.Compact}" />
  <Setter Property="RowHeight" Value="32" />
</Style>
```

## Best practices

- **Never hardcode colors**: Use semantic token keys and `DynamicResource`.
- **Control templates**: Replace all brush/color hardcoding with token references.
- **Test theme switching**: Runtime toggle should affect all surfaces instantly.
- **Minimal rounded corners**: Use `CornerRadius="8"` for controls, `CornerRadius="0"` for most content.
- **Avoid decorative shadows**: Prefer borders for visual separation.
- **Keep brush counts low**: Reuse primary/secondary/muted brushes instead of creating one per control.