# WPF UX guidelines

Detailed implementation patterns for interaction states, keyboard navigation,
accessibility, and user feedback in WPF applications.

## Interaction state implementation

### Button state implementation patterns

#### StateGroup approach (clean, maintainable)
```xaml
<ControlTemplate TargetType="Button">
  <Border x:Name="border"
          Background="{DynamicResource Brush.Action.Primary}"
          BorderBrush="{DynamicResource Brush.Border.Default}"
          Padding="16,10,16,10">
    <ContentPresenter Foreground="{DynamicResource Brush.Text.OnPrimary}" />
  </Border>
  
  <ControlTemplate.Triggers>
    <!-- Group 1: Normal/Hover/Pressed -->
    <Trigger Property="IsMouseOver" Value="True">
      <Setter TargetName="border" Property="Background" 
              Value="{DynamicResource Brush.Action.PrimaryHover}" />
    </Trigger>
    
    <Trigger Property="IsPressed" Value="True">
      <Setter TargetName="border" Property="Background" 
              Value="{DynamicResource Brush.Action.PrimaryPressed}" />
    </Trigger>
    
    <!-- Group 2: Disabled -->
    <Trigger Property="IsEnabled" Value="False">
      <Setter TargetName="border" Property="Background" 
              Value="{DynamicResource Brush.Surface.Subtle}" />
      <Setter TargetName="border" Property="Opacity" Value="0.6" />
    </Trigger>
    
    <!-- Group 3: Focused (keyboard) -->
    <Trigger Property="IsFocused" Value="True">
      <Setter TargetName="border" Property="BorderThickness" Value="2" />
      <Setter TargetName="border" Property="BorderBrush" 
              Value="{DynamicResource Brush.Border.Focus}" />
    </Trigger>
  </ControlTemplate.Triggers>
</ControlTemplate>
```

### Hover state considerations

Hover should only apply on mouse-over (not keyboard):
```xaml
<!-- Desktop only: show hover effect on mouse over -->
<Trigger Property="IsMouseOver" Value="True">
  <Setter TargetName="border" Property="Background" 
          Value="{DynamicResource Brush.Surface.Hover}" />
</Trigger>

<!-- Keyboard users get focus indicator instead (see FocusVisualStyle) -->
```

### Focus state (keyboard navigation)

Focus must be **always visible** and **distinct**:
```xaml
<FocusVisualStyle>
  <Setter Property="Control.Template">
    <Setter.Value>
      <ControlTemplate>
        <Border BorderThickness="2"
                BorderBrush="{DynamicResource Brush.Border.Focus}"
                CornerRadius="8"
                IsHitTestVisible="False" />
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</FocusVisualStyle>
```

### Disabled state

Provide clear visual feedback:
```xaml
<Trigger Property="IsEnabled" Value="False">
  <!-- Option 1: Fade appearance -->
  <Setter Property="Opacity" Value="0.6" />
  
  <!-- Option 2: Use disabled token colors -->
  <Setter TargetName="border" Property="Background" 
          Value="{DynamicResource Brush.Surface.Disabled}" />
  <Setter TargetName="border" Property="BorderBrush" 
          Value="{DynamicResource Brush.Border.Disabled}" />
  <Setter TargetName="text" Property="Foreground" 
          Value="{DynamicResource Brush.Text.Disabled}" />
</Trigger>
```

### Loading/Busy state
```xaml
<ControlTemplate TargetType="Button">
  <Border x:Name="border" ...>
    <Grid>
      <!-- Normal content -->
      <ContentPresenter x:Name="content" />
      
      <!-- Spinner (animated, hidden normally) -->
      <Image x:Name="spinner" Source="spinner.gif" Visibility="Hidden" />
    </Grid>
  </Border>
  
  <ControlTemplate.Triggers>
    <!-- Show spinner when IsBusy is true -->
    <DataTrigger Binding="{Binding IsBusy, RelativeSource={RelativeSource TemplatedParent}}" 
                 Value="True">
      <Setter TargetName="content" Property="Visibility" Value="Hidden" />
      <Setter TargetName="spinner" Property="Visibility" Value="Visible" />
      <Setter Property="IsEnabled" Value="False" />
    </DataTrigger>
  </ControlTemplate.Triggers>
</ControlTemplate>
```

## Semantic status states

### Success state
```xaml
<Border Background="{DynamicResource Brush.Surface.Success}"
        BorderBrush="{DynamicResource Brush.Border.Success}"
        BorderThickness="1"
        Padding="12">
  <StackPanel Spacing="8">
    <TextBlock Text="✓ Save successful" 
               Foreground="{DynamicResource Brush.Text.Success}"
               FontWeight="SemiBold" />
  </StackPanel>
</Border>
```

### Warning state
```xaml
<Border Background="{DynamicResource Brush.Surface.Warning}"
        BorderBrush="{DynamicResource Brush.Border.Warning}"
        BorderThickness="1"
        Padding="12">
  <StackPanel Spacing="8">
    <TextBlock Text="⚠ Unsaved changes" 
               Foreground="{DynamicResource Brush.Text.Warning}"
               FontWeight="SemiBold" />
    <TextBlock Text="Click Save to keep changes"
               Foreground="{DynamicResource Brush.Text.Muted}" />
  </StackPanel>
</Border>
```

### Error state
```xaml
<Border Background="{DynamicResource Brush.Surface.Danger}"
        BorderBrush="{DynamicResource Brush.Border.Danger}"
        BorderThickness="1"
        Padding="12">
  <StackPanel Spacing="8">
    <TextBlock Text="✕ Operation failed" 
               Foreground="{DynamicResource Brush.Text.Danger}"
               FontWeight="SemiBold" />
    <TextBlock Text="{Binding ErrorMessage}"
               Foreground="{DynamicResource Brush.Text.Danger}" />
  </StackPanel>
</Border>
```

## Keyboard navigation implementation

### Tab order
Explicitly set `TabIndex` when default order is incorrect:
```xaml
<StackPanel>
  <TextBox x:Name="FirstName" TabIndex="1" />
  <TextBox x:Name="LastName" TabIndex="2" />
  <Button x:Name="Submit" TabIndex="3" />
  <Button x:Name="Cancel" TabIndex="4" />
</StackPanel>
```

### KeyDown event handling
```csharp
private void DataGrid_KeyDown(object sender, KeyEventArgs e)
{
    if (e.Key == Key.Return)
    {
        // User pressed Enter while focused on grid
        EditSelectedRow();
        e.Handled = true;
    }
    else if (e.Key == Key.Escape)
    {
        // User pressed Escape to cancel edit
        CancelEdit();
        e.Handled = true;
    }
}
```

### Modal dialog keyboard handling
```csharp
public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
        this.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Return && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                // Enter key while no modifier → confirm primary action
                this.DialogResult = true;
                e.Handled = true;
            }
        };
        
        // Focus primary action on open
        this.Loaded += (s, e) => OkButton.Focus();
    }
}
```

### ComboBox keyboard patterns
```xaml
<ComboBox KeyDown="ComboBox_KeyDown">
  <!-- Arrow Up/Down: cycle items -->
  <!-- Enter: select current item -->
  <!-- Escape: close dropdown -->
</ComboBox>
```

```csharp
private void ComboBox_KeyDown(object sender, KeyEventArgs e)
{
    var comboBox = (ComboBox)sender;
    
    if (e.Key == Key.Return)
    {
        // Select and close
        comboBox.IsDropDownOpen = false;
        e.Handled = true;
    }
    else if (e.Key == Key.Escape)
    {
        comboBox.IsDropDownOpen = false;
        e.Handled = true;
    }
}
```

## Accessibility implementation

### AutomationProperties for screen readers
```xaml
<!-- Button with accessible name and description -->
<Button Content="Delete"
        AutomationProperties.Name="Delete document"
        AutomationProperties.HelpText="Permanently remove this file from the system" />

<!-- Icon-only button -->
<Button Content="🔍"
        AutomationProperties.Name="Search"
        ToolTip="Search documents (Ctrl+F)" />

<!-- Form label -->
<Label Target="{Binding ElementName=nameInput}">
  <Run Text="Project Name " />
  <Run Text="*" Foreground="{DynamicResource Brush.Text.Danger}" />
</Label>
<TextBox x:Name="nameInput"
         AutomationProperties.Name="Project name"
         AutomationProperties.LabeledBy="{Binding ElementName=projectLabel}" />
```

### Contrast validation
```csharp
// Helper to check contrast ratio
private bool IsContrastSufficient(Color foreground, Color background)
{
    // WCAG AA: ≥4.5:1 for normal text, ≥3:1 for large text (18pt+)
    // Calculate relative luminance and contrast ratio
    var fg = GetRelativeLuminance(foreground);
    var bg = GetRelativeLuminance(background);
    var lighter = Math.Max(fg, bg);
    var darker = Math.Min(fg, bg);
    var ratio = (lighter + 0.05) / (darker + 0.05);
    return ratio >= 4.5;
}
```

### Text scaling support
Respect system text scaling:
```xaml
<!-- Use em-based sizing or semantic token sizes (not hardcoded px) -->
<TextBlock Text="Label"
           FontSize="{DynamicResource FontSize.Label}" />

<!-- Or use relative sizing -->
<TextBlock Text="Large"
           FontSize="{Binding RelativeSource={RelativeSource AncestorType=Control}, 
                              Path=FontSize, 
                              Converter=MultiplierConverter, 
                              ConverterParameter=1.5}" />
```

### ARIA-equivalent labeling in WPF
```csharp
// Set automation IDs for testing and accessibility
AutomationProperties.SetAutomationId(submitButton, "submit-btn");
AutomationProperties.SetName(submitButton, "Save changes");

// Announce dynamic content changes
var peer = UIElementAutomationPeer.FromElement(notificationPanel);
if (peer != null)
{
    peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
}
```

## Loading and async feedback patterns

### Loading indicator with timeout
```csharp
private async void LoadData_Click(object sender, RoutedEventArgs e)
{
    LoadButton.IsEnabled = false;
    LoadingSpinner.Visibility = Visibility.Visible;
    StatusText.Text = "Loading data...";
    
    try
    {
        var result = await Task.Run(() => _service.FetchDataAsync());
        DataGrid.ItemsSource = result;
        StatusText.Text = "Data loaded successfully";
        ShowStatusNotification("✓ Loaded", "success");
    }
    catch (Exception ex)
    {
        StatusText.Text = $"✕ Error: {ex.Message}";
        ShowStatusNotification($"✕ Failed: {ex.Message}", "error");
    }
    finally
    {
        LoadButton.IsEnabled = true;
        LoadingSpinner.Visibility = Visibility.Hidden;
    }
}

private void ShowStatusNotification(string message, string type)
{
    var notification = new Border
    {
        Background = type == "success" 
            ? (Brush)this.FindResource("Brush.Surface.Success")
            : (Brush)this.FindResource("Brush.Surface.Danger"),
        Child = new TextBlock { Text = message }
    };
    
    NotificationArea.Children.Add(notification);
    
    // Auto-dismiss after 4 seconds
    Task.Delay(4000).ContinueWith(_ => 
        Dispatcher.Invoke(() => NotificationArea.Children.Remove(notification)));
}
```

### Progress indicator for long operations
```xaml
<ProgressBar x:Name="ProgressBar"
             Minimum="0"
             Maximum="100"
             Value="{Binding ProgressPercentage}"
             Foreground="{DynamicResource Brush.Action.Primary}"
             Height="4"
             Margin="0,0,0,8" />
```

```csharp
private async Task ProcessFilesAsync(IEnumerable<string> files)
{
    int total = files.Count();
    int processed = 0;
    
    foreach (var file in files)
    {
        await ProcessFileAsync(file);
        processed++;
        ProgressPercentage = (processed * 100) / total;
        StatusText = $"Processing {processed}/{total}";
    }
    
    StatusText = "✓ All files processed";
}
```

## Validation and error messages

### Inline validation
```xaml
<StackPanel Spacing="4">
  <TextBlock Text="Email"
             FontWeight="SemiBold" />
  <TextBox x:Name="EmailInput"
           Validation.ErrorTemplate="{StaticResource ErrorTemplate}" />
  <TextBlock Text="{Binding ElementName=EmailInput, Path=(Validation.Errors)/0/ErrorContent}"
             Foreground="{DynamicResource Brush.Text.Danger}"
             FontSize="{DynamicResource FontSize.Caption}"
             Visibility="{Binding ElementName=EmailInput, Path=(Validation.HasError), 
                                   Converter=BoolToVisibilityConverter}" />
</StackPanel>
```

### Form validation on submit
```csharp
private void Submit_Click(object sender, RoutedEventArgs e)
{
    var errors = new List<string>();
    
    if (string.IsNullOrWhiteSpace(NameInput.Text))
        errors.Add("Name is required");
    
    if (!IsValidEmail(EmailInput.Text))
        errors.Add("Email format is invalid");
    
    if (errors.Any())
    {
        ShowErrorDialog("Validation Failed", string.Join("\n", errors));
        return;
    }
    
    _viewModel.SubmitAsync();
}

private void ShowErrorDialog(string title, string message)
{
    var dialog = new Border
    {
        Background = (Brush)this.FindResource("Brush.Surface.Danger"),
        BorderBrush = (Brush)this.FindResource("Brush.Border.Danger"),
        BorderThickness = new Thickness(1),
        Padding = new Thickness(12),
        Child = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = "✕ " + title, FontWeight = FontWeights.Bold },
                new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap }
            }
        }
    };
    
    ErrorArea.Children.Add(dialog);
}
```

## Best practices checklist

- [ ] All interaction states visually distinct: normal, hover, pressed, focused, disabled
- [ ] Focus always visible (2px border or color change)
- [ ] Disabled controls: faded/grayed appearance + disabled cursor
- [ ] Status colors paired with icons + text (never color-only)
- [ ] Tab order logical and explicitly set when needed
- [ ] Keyboard shortcuts documented and consistent (Ctrl+S, etc.)
- [ ] AutomationProperties set for all interactive controls
- [ ] Contrast ratios meet WCAG AA (4.5:1 normal, 3:1 large)
- [ ] Text scaling respected (use dynamic tokens, not hardcoded sizes)
- [ ] Loading states: spinner + disabled action + status message
- [ ] Error messages: inline, semantic color, actionable guidance
- [ ] Theme switching: test all states in light and dark modes
- [ ] Keyboard-only navigation: test workflow without mouse