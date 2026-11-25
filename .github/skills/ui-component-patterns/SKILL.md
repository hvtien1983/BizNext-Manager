---
name: ui-component-patterns
description: "Use when creating or updating reusable UI components, buttons, inputs, text boxes, cards, dialogs, lists, navigation, or control states. Apply the shared design system and accessibility patterns."
---

# UI component patterns

Design reusable controls from semantic tokens, then adapt them to the host
framework. Consult [component guidelines](./references/component-guidelines.md)
and use the shared [UI design system](../ui-design-system/SKILL.md).

## Workflow

1. Identify whether the control already exists in the app or shared component
   library. Extend/reuse it before creating another variant.
2. Define its purpose, content, and interaction states before styling.
3. Apply semantic color, typography, spacing, and shape tokens.
4. Preserve keyboard navigation, focus visibility, accessible names, disabled
   behavior, and sensible target sizes.
5. Keep component API and visual behavior consistent across repeated uses.
6. Validate at compact and normal layouts, and build/test the affected app.

Do not encode business behavior in a visual component. Expose a clear command,
event, or binding contract and let the owning view/model handle the workflow.
