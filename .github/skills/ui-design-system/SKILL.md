---
name: ui-design-system
description: "Use when designing or implementing application UI, themes, colors, typography, spacing, surfaces, borders, boxes, or controls. Keep the visual language consistent across apps and platforms."
---

# UI design system

Use the shared design system before introducing new visual values. The intent is
to give all applications in this repository a coherent visual language while
allowing each platform to use its native rendering and controls.

## Workflow

1. Read [design tokens](./references/design-tokens.md) and
   [design-tokens.json](./assets/design-tokens.json).
2. Inspect the target app's existing theme/resources. Reuse shared semantic
   tokens; do not copy palette literals into individual controls.
3. Use semantic roles such as `text.primary`, `surface.canvas`,
   `border.default`, and `action.primary` rather than names tied to one view.
4. Preserve hierarchy, readable contrast, predictable spacing, and clear
   interaction states.
5. If a token needs to change or a new shared token is needed, update the JSON
   source and its reference documentation together, then propagate it through
   the platform theme adapters.
6. Validate the target app build and inspect the UI at its minimum supported
   window size and normal usage size.

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
