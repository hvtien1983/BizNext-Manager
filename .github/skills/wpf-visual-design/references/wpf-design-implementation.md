# WPF design implementation patterns

Practical XAML patterns for implementing shared design tokens and visual
standards in WPF. Use this alongside [`wpf-visual-design`](../SKILL.md).

## ResourceDictionary structure and merging

### Recommended folder structure
```
src/YourApp/
├── Themes/
│   ├── App.xaml                  (Main app resources, merge order here)
│   ├── DesignTokens.xaml         (Semantic token definitions)
│   ├── LightTheme.xaml           (Light theme token overrides)
│   ├── DarkTheme.xaml            (Dark theme token overrides)
│   ├── Colors.xaml               (Optional: palette foundation)
│   └── Shared/
│       ├── ButtonStyles.xaml
│       ├── InputStyles.xaml
│       ├── DataGridStyles.xaml
│       └── ...
```

### Merge order in App.xaml
Order matters: later dictionaries override earlier ones.

```xaml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <!-- 1. Semantic tokens (foundation) -->
      <ResourceDictionary Source="pack://application:,,,/Themes/DesignTokens.xaml" />
      
      <!-- 2. Light theme (default) -->
      <ResourceDictionary Source="pack://application:,,,/Themes/LightTheme.xaml" />
      
      <!-- 3. Dark theme (selected at runtime) -->
      <!-- Added programmatically when user switches theme -->
      
      <!-- 4. Shared control styles (use tokens from above) -->
      <ResourceDictionary Source="pack://application:,,,/Themes/Shared/ButtonStyles.xaml" />
      <ResourceDictionary Source="pack://application:,,,/Themes/Shared/InputStyles.xaml" />
      <ResourceDictionary Source="pack://application:,,,/Themes/Shared/DataGridStyles.xaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

## Semantic token definitions

### DesignTokens.xaml structure

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
  
  <!-- PALETTE FOUNDATION (rarely used directly) -->
  <Color x:Key="Palette.Blue.600">#356AE6</Color>
  <Color x:Key="Palette.Blue.700">#2858C7</Color>
  <Color x:Key="Palette.Slate.950">#172033</Color>
  <Color x:Key="Palette.Slate.700">#536078</Color>
  <Color x:Key="Palette.Slate.500">#748099</Color>
  
  <!-- SEMANTIC COLOR TOKENS (compose from palette) -->
  <!-- Text colors -->
  <Color x:Key="Color.Text.Primary">#172033</Color>
  <Color x:Key="Color.Text.Secondary">#536078</Color>
  <Color x:Key="Color.Text.Muted">#748099</Color>
  <Color x:Key="Color.Text.OnPrimary">#FFFFFF</Color>
  <Color x:Key="Color.Text.Success">#278354</Color>
  <Color x:Key="Color.Text.Danger">#C44747</Color>
  <Color x:Key="Color.Text.Warning">#80530C</Color>
  
  <!-- Surface colors (backgrounds) -->
  <Color x:Key="Color.Surface.Canvas">#F5F7FA</Color>
  <Color x:Key="Color.Surface.Default">#FFFFFF</Color>
  <Color x:Key="Color.Surface.Subtle">#F8FAFD</Color>
  <Color x:Key="Color.Surface.Selected">#EAF0FF</Color>
  <Color x:Key="Color.Surface.Hover">#F8FAFD</Color>
  <Color x:Key="Color.Surface.Success">#F0F7F2</Color>
  <Color x:Key="Color.Surface.Warning">#FFF7E8</Color>
  <Color x:Key="Color.Surface.Danger">#FFEFEF</Color>
  
  <!-- Border colors -->
  <Color x:Key="Color.Border.Default">#E5E9F0</Color>
  <Color x:Key="Color.Border.Subtle">#EEF0F4</Color>
  <Color x:Key="Color.Border.Focus">#356AE6</Color>
  <Color x:Key="Color.Border.Success">#35A66F</Color>
  <Color x:Key="Color.Border.Warning">#F4D9A6</Color>
  <Color x:Key="Color.Border.Danger">#E8B5B5</Color>
  
  <!-- Action colors (interactive) -->
  <Color x:Key="Color.Action.Primary">#356AE6</Color>
  <Color x:Key="Color.Action.PrimaryHover">#2858C7</Color>
  <Color x:Key="Color.Action.PrimaryPressed">#1F3D8A</Color>
  <Color x:Key="Color.Action.PrimaryDisabled">#CAD5F0</Color>
  <Color x:Key="Color.Action.Secondary">#536078</Color>
  <Color x:Key="Color.Action.Danger">#C44747</Color>
  
  <!-- BRUSH RESOURCES (use in styles/templates) -->
  <SolidColorBrush x:Key="Brush.Text.Primary" Color="{StaticResource Color.Text.Primary}" />
  <SolidColorBrush x:Key="Brush.Text.Secondary" Color="{StaticResource Color.Text.Secondary}" />
  <SolidColorBrush x:Key="Brush.Text.Muted" Color="{StaticResource Color.Text.Muted}" />
  <SolidColorBrush x:Key="Brush.Text.OnPrimary" Color="{StaticResource Color.Text.OnPrimary}" />
  
  <SolidColorBrush x:Key="Brush.Surface.Canvas" Color="{StaticResource Color.Surface.Canvas}" />
  <SolidColorBrush x:Key="Brush.Surface.Default" Color="{StaticResource Color.Surface.Default}" />
  <SolidColorBrush x:Key="Brush.Surface.Subtle" Color="{StaticResource Color.Surface.Subtle}" />
  <SolidColorBrush x:Key="Brush.Surface.Selected" Color="{StaticResource Color.Surface.Selected}" />
  <SolidColorBrush x:Key="Brush.Surface.Hover" Color="{StaticResource Color.Surface.Hover}" />
  
  <SolidColorBrush x:Key="Brush.Border.Default" Color="{StaticResource Color.Border.Default}" />
  <SolidColorBrush x:Key="Brush.Border.Focus" Color="{StaticResource Color.Border.Focus}" />
  
  <SolidColorBrush x:Key="Brush.Action.Primary" Color="{StaticResource Color.Action.Primary}" />
  <SolidColorBrush x:Key="Brush.Action.Secondary" Color="{StaticResource Color.Action.Secondary}" />
  <SolidColorBrush x:Key="Brush.Action.Danger" Color="{StaticResource Color.Action.Danger}" />
  
  <!-- TYPOGRAPHY TOKENS -->
  <FontFamily x:Key="FontFamily.Default">Segoe UI, Arial, sans-serif</FontFamily>
  <System:Double x:Key="FontSize.Caption">10</System:Double>
  <System:Double x:Key="FontSize.Small">11</System:Double>
  <System:Double x:Key="FontSize.Body">12</System:Double>
  <System:Double x:Key="FontSize.BodyLarge">14</System:Double>
  <System:Double x:Key="FontSize.Title">22</System:Double>
  
  <FontWeight x:Key="FontWeight.Regular">Regular</FontWeight>
  <FontWeight x:Key="FontWeight.SemiBold">SemiBold</FontWeight>
  <FontWeight x:Key="FontWeight.Bold">Bold</FontWeight>
  
  <!-- SPACING TOKENS -->
  <Thickness x:Key="Spacing.1">4</Thickness>
  <Thickness x:Key="Spacing.2">8</Thickness>
  <Thickness x:Key="Spacing.3">12</Thickness>
  <Thickness x:Key="Spacing.4">16</Thickness>
  <Thickness x:Key="Spacing.5">20</Thickness>
  <Thickness x:Key="Spacing.6">24</Thickness>
  <Thickness x:Key="Spacing.8">32</Thickness>
  <Thickness x:Key="Spacing.10">40</Thickness>
  
  <!-- DENSITY TOKENS (compact for data-heavy views) -->
  <System:Double x:Key="Density.Compact.ControlHeight">36</System:Double>
  <System:Double x:Key="Density.Compact.ButtonHeight">34</System:Double>
  <System:Double x:Key="Density.Normal.ControlHeight">38</System:Double>
  <System:Double x:Key="Density.Normal.ButtonHeight">38</System:Double>
  
  <!-- CORNER RADIUS TOKENS -->
  <CornerRadius x:Key="CornerRadius.Control">8</CornerRadius>
  <CornerRadius x:Key="CornerRadius.Card">12</CornerRadius>
  
</ResourceDictionary>
```

## Theme switching implementation

### LightTheme.xaml (default overrides, mostly empty or matches defaults)
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
  <!-- Light theme is the default; this file is minimal or empty -->
  <!-- Included for consistency and future customization -->
</ResourceDictionary>
```

### DarkTheme.xaml (dark mode overrides)
```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
  
  <!-- Override semantic tokens for dark theme -->
  <Color x:Key="Color.Text.Primary">#F3F5FA</Color>
  <Color x:Key="Color.Text.Secondary">#C2CAD8</Color>
  <Color x:Key="Color.Text.Muted">#A0AABD</Color>
  
  <Color x:Key="Color.Surface.Canvas">#11151D</Color>
  <Color x:Key="Color.Surface.Default">#1B2230</Color>
  <Color x:Key="Color.Surface.Subtle">#202938</Color>
  <Color x:Key="Color.Surface.Selected">#263858</Color>
  <Color x:Key="Color.Surface.Hover">#273246</Color>
  
  <Color x:Key="Color.Border.Default">#384254</Color>
  <Color x:Key="Color.Border.Focus">#7EA0FF</Color>
  
  <Color x:Key="Color.Action.Primary">#7EA0FF</Color>
  <Color x:Key="Color.Action.PrimaryHover">#9AB4FF</Color>
  <Color x:Key="Color.Action.PrimaryPressed">#6080D0</Color>
  
  <!-- Brush resources automatically update via DynamicResource -->
  
</ResourceDictionary>
```

### C# code for theme switching
```csharp
public static class ThemeManager
{
    private static const string DarkThemeUri = 
        "pack://application:,,,/Themes/DarkTheme.xaml";
    
    public static void SetTheme(bool isDarkTheme)
    {
        var darkThemeDict = new ResourceDictionary 
        { 
            Source = new Uri(DarkThemeUri) 
        };
        
        // Remove old dark theme if exists
        Application.Current.Resources.MergedDictionaries
            .Where(d => d.Source?.OriginalString?.Contains("DarkTheme") ?? false)
            .ForEach(d => Application.Current.Resources.MergedDictionaries.Remove(d));
        
        // Add dark theme at the end (highest priority)
        if (isDarkTheme)
        {
            Application.Current.Resources.MergedDictionaries.Add(darkThemeDict);
        }
    }
}
```

## Control template patterns

### Primary button template (complete example)
```xaml
<ControlTemplate x:Key="PrimaryButtonTemplate" TargetType="Button">
  <Border x:Name="border"
          Background="{DynamicResource Brush.Action.Primary}"
          BorderBrush="{DynamicResource Brush.Border.Default}"
          BorderThickness="1"
          Padding="{DynamicResource Spacing.4}"
          CornerRadius="{DynamicResource CornerRadius.Control}">
    <Grid>
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
      </Grid.ColumnDefinitions>
      
      <!-- Icon (optional, left) -->
      <ContentPresenter x:Name="icon"
                        Grid.Column="0"
                        Margin="0,0,8,0"
                        Content="{TemplateBinding Tag}" />
      
      <!-- Text (center) -->
      <ContentPresenter Grid.Column="1"
                        HorizontalAlignment="Center"
                        VerticalAlignment="Center"
                        Foreground="{DynamicResource Brush.Text.OnPrimary}" />
      
      <!-- Busy spinner (optional, right) -->
      <Image x:Name="spinner"
             Grid.Column="2"
             Source="spinner.png"
             Width="16"
             Height="16"
             Margin="8,0,0,0"
             Visibility="Hidden" />
    </Grid>
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
              Value="{DynamicResource Brush.Action.PrimaryDisabled}" />
      <Setter TargetName="border" Property="Foreground" 
              Value="{DynamicResource Brush.Text.Disabled}" />
    </Trigger>
    
    <Trigger Property="IsFocused" Value="True">
      <Setter TargetName="border" Property="BorderBrush" 
              Value="{DynamicResource Brush.Border.Focus}" />
      <Setter TargetName="border" Property="BorderThickness" Value="2" />
    </Trigger>
  </ControlTemplate.Triggers>
</ControlTemplate>

<!-- Apply the template via style -->
<Style x:Key="PrimaryButtonStyle" TargetType="Button">
  <Setter Property="Template" Value="{StaticResource PrimaryButtonTemplate}" />
  <Setter Property="Foreground" Value="{DynamicResource Brush.Text.OnPrimary}" />
  <Setter Property="Cursor" Value="Hand" />
</Style>
```

### Text input template
```xaml
<ControlTemplate x:Key="TextBoxTemplate" TargetType="TextBox">
  <Border x:Name="border"
          Background="{DynamicResource Brush.Surface.Default}"
          BorderBrush="{DynamicResource Brush.Border.Default}"
          BorderThickness="1"
          CornerRadius="{DynamicResource CornerRadius.Control}">
    <ScrollViewer Padding="{DynamicResource Spacing.3}"
                  VerticalScrollBarVisibility="Hidden"
                  HorizontalScrollBarVisibility="Hidden">
      <TextBlock Text="{TemplateBinding Text}"
                 Foreground="{DynamicResource Brush.Text.Primary}" />
    </ScrollViewer>
  </Border>
  
  <ControlTemplate.Triggers>
    <Trigger Property="IsFocused" Value="True">
      <Setter TargetName="border" Property="BorderBrush" 
              Value="{DynamicResource Brush.Border.Focus}" />
      <Setter TargetName="border" Property="BorderThickness" Value="2" />
    </Trigger>
    
    <Trigger Property="IsEnabled" Value="False">
      <Setter TargetName="border" Property="Background" 
              Value="{DynamicResource Brush.Surface.Subtle}" />
      <Setter Property="Foreground" Value="{DynamicResource Brush.Text.Muted}" />
    </Trigger>
  </ControlTemplate.Triggers>
</ControlTemplate>
```

## DataTemplate for status indicators

```xaml
<!-- Status badge template -->
<DataTemplate x:Key="StatusBadgeTemplate" DataType="local:StatusViewModel">
  <Border BorderBrush="{Binding Status, Converter=StatusBorderConverter}" 
          BorderThickness="1"
          Background="{Binding Status, Converter=StatusBackgroundConverter}"
          Padding="8,4,8,4"
          CornerRadius="8">
    <StackPanel Orientation="Horizontal" Spacing="4">
      <Image Source="{Binding Status, Converter=StatusIconConverter}" 
             Width="16" Height="16" />
      <TextBlock Text="{Binding Status}"
                 Foreground="{Binding Status, Converter=StatusTextConverter}" />
    </StackPanel>
  </Border>
</DataTemplate>
```

## Best practices checklist

- [ ] All colors use `DynamicResource` (not `StaticResource`)
- [ ] ControlTemplates reference tokens, not hardcoded hex values
- [ ] Palette colors defined once; semantic tokens compose them
- [ ] Focus state: 2px border or color change, always visible
- [ ] Disabled state: faded/desaturated appearance + disabled text color
- [ ] Hover state: `surface.hover` background or color change
- [ ] DarkTheme.xaml contains only overrides, not duplicated definitions
- [ ] Test theme switching at runtime for instant visual updates
- [ ] No `OnLoaded` hacks to set brushes; use template binding
- [ ] Semantic token names stable across light/dark (meaning consistent)