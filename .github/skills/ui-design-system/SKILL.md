---
name: ui-design-system
description: "Use when designing or implementing application UI, themes, colors, typography, spacing, surfaces, borders, boxes, or controls. Keep the visual language consistent across apps and platforms."
---

# UI design system

Use the shared design system before introducing new visual values. The intent is
to give all applications in this repository a coherent visual language while
allowing each platform to use its native rendering and controls.

## Workflow

1. Read [design tokens](./references/design-tokens.md),
   [visual design standards](./references/visual-design-standards.md), and
   [design-tokens.json](./assets/design-tokens.json).
2. Inspect the target app's existing theme/resources. Reuse shared semantic
   tokens; do not copy palette literals into individual controls.
3. Use semantic roles such as `text.primary`, `surface.canvas`,
   `border.default`, and `action.primary` rather than names tied to one view.
4. Preserve visual hierarchy, readable contrast, predictable spacing, and clear
   interaction states across light and dark themes.
5. If a token needs to change or a new shared token is needed, update the JSON
   source and its reference documentation together, then propagate it through
   the platform theme adapters.
6. Validate the target app build and inspect the UI at its minimum supported
   window size and normal usage size, in both light and dark themes.

## Visual Design Standards

The design system is built on these core principles:

### Visual Hierarchy
- **Primary visual weight** is used for the most important content (main actions, primary headings)
- **Secondary visual weight** guides users through secondary actions and supporting content
- **Tertiary visual weight** provides low-emphasis information and hints
- Use size, color, contrast, and spacing to establish hierarchy—not color alone

### Color Theory
- **Primary** (`action.primary`): Core brand color for main actions and focus states
- **Semantic colors** (success, warning, danger): Status and feedback only, never for hierarchy
- **Neutral palette**: Text, borders, surfaces, backgrounds use slate shades
- **Contrast**: Ensure WCAG AA minimum contrast for all foreground/background pairs
- Light and dark themes maintain semantic meaning, not just inverse colors

### Typography
- **Hierarchical scale**: Heading, body, label, caption roles rather than arbitrary sizes
- **Font family**: Segoe UI (Windows system font) for professional desktop UI
- **Weight**: Regular for body, semibold for labels/emphasis, bold for headings
- **Line height**: Consistent spacing for readability, platform-specific text scaling respected
- **Line length**: Body text at 50-80 characters wide; dense lists/data at tighter settings

### Spacing & Layout System
- **8px base unit**: All spacing derives from 4px or 8px increments (1, 2, 3, 4, 5, 6, 8, 10, 12 scale)
- **Consistent gutters**: Sections use predictable spacing; align content across columns
- **Data density**: Desktop workflows use compact density metrics for efficiency without sacrificing readability
- **Breathing room**: Avoid crowding; use white space to separate concerns

### Shapes & Borders
- **Border radius**: Minimal use—control corners (8px) only, no decorative rounding
- **Border width**: 1px default, 2px for emphasis; prefer visual separation over heavy borders
- **Card/surface elevation**: Use subtle borders before visible shadows
- **Desktop aesthetic**: Avoid rounded corners everywhere; keep rectilinear forms dominant

### Shadows & Elevation
- **Minimal elevation**: Prefer borders to communicate hierarchy
- **Card separation**: Only when needed to distinguish layered content
- **No decorative shadows**: Keep shadows purposeful and restrained
- **Platform rendering**: Each platform renders shadows appropriately; avoid precise pixel shadows

### Iconography
- **Semantic meaning**: Each icon has a clear, consistent meaning across the app
- **Size consistency**: Icons scale predictably (16px, 24px common)
- **Status icons**: Success (green checkmark), warning (amber triangle), error (red circle/X), info (blue circle-i)
- **System icons**: Use platform icon sets where available; maintain visual weight across a set

### Visual Consistency
- **Semantic roles over variants**: One button style, applied with semantic purpose (primary, secondary, danger)
- **Reusable patterns**: Form layouts, data grids, sidebars, dialogs follow shared patterns
- **State consistency**: Hover, focus, pressed, disabled states are visually distinct and predictable
- **Cross-theme**: Light and dark themes are equally polished; no "default + dark variant" asymmetry

## Principles

- Prefer existing shared tokens to one-off colors, font sizes, margins, or
  corner radii.
- Use platform-native controls and accessibility behavior; consistency means
  shared semantics, not identical implementation markup.
- Do not reduce contrast or remove focus, hover, disabled, selected, or error
  states to match a static mockup.
- Keep user-facing text concise, action-oriented, and consistent with the
  product's localization.
- Ask before making a broad brand change that would affect multiple apps.
- Apply design standards consistently: do not copy web UI patterns into desktop
  apps without adapting for windows, mouse, and keyboard input.
- Support both light and dark themes equally; theme switching should not expose
  undesigned surfaces.
