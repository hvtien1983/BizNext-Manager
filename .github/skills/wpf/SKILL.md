---
name: wpf
description: "Use when building or modernizing WPF UI, XAML, MVVM, data binding, commands, threading, styles, control templates, or Windows desktop applications."
compatibility: "Requires a WPF project on .NET or .NET Framework."
---

# WPF

Use this skill for WPF UI, MVVM, XAML, data binding, commands, custom styles,
control templates, and Windows desktop modernization.

## Workflow

1. Confirm that the target surface is Windows-only WPF; keep framework-specific
   decisions out of platform-neutral contracts and domain logic.
2. Inspect the existing solution structure, theme resources, bindings, commands,
   and control conventions before changing a view.
3. Keep business behavior in services and ViewModels. Views should bind to
   state and expose clear commands/events; use code-behind for view mechanics
   such as dialog ownership or drag/drop only when that matches the app pattern.
4. Manage bindings explicitly: select the correct binding mode, validate
   two-way inputs, and surface binding/runtime errors rather than silently
   swallowing them.
5. Centralize reusable styles and templates in resource dictionaries. Reuse
   semantic design tokens and `DynamicResource` for values that change with the
   active theme. Keep module-only styles with their owning module.
6. Preserve keyboard navigation, focus visuals, UI Automation names, and
   accessible selected/disabled/error states when customizing controls.
7. Use `async`/`await` for I/O and long-running operations. WPF continuations
   normally return to the dispatcher; explicitly dispatch only when work has
   moved off the UI thread.
8. Validate XAML at runtime as well as building: run the application, visit
   changed views/dialogs, and check light and dark themes at compact and normal
   window sizes.

## Project conventions

- Prefer the repository's existing MVVM implementation over adding a second
  framework or package.
- Keep shared visual resources in the Shell and let hosted modules consume
  those resources; do not make a shared Shell dictionary depend on module
  types.
- Use the native WPF control when it provides appropriate Windows keyboard,
  focus, automation, and text-scaling behavior. Customize its template only
  when the existing visual system requires it, and retain the control's
  interaction states.
- Follow `ui-design-system` for semantic tokens and
  `ui-component-patterns` for reusable control states.

## References

- [WPF Overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/)
- [Data Binding Overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/)
- [Styles and Templates](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/styles-templates-overview)
- [WPF Migration Guide](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/migration/)
- Upstream inspiration: [managedcode/dotnet-skills WPF skill](https://github.com/managedcode/dotnet-skills/blob/main/catalog/Frameworks/WPF/skills/wpf/SKILL.md).

This project skill is an adapted workflow based on the upstream WPF skill,
distributed under the MIT License, Copyright (c) 2026 Managed Code. It is
rewritten for BizNext Manager conventions and is not a verbatim copy.
