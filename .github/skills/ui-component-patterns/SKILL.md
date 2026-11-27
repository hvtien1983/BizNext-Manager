---
name: ui-component-patterns
description: "Use when creating or updating reusable UI components, buttons, inputs, text boxes, cards, dialogs, lists, navigation, or control states. Apply the shared design system and accessibility patterns."
---

# UI component patterns

Design reusable controls from semantic tokens, then adapt them to the host
framework. Consult [component guidelines](./references/component-guidelines.md),
[desktop UI patterns](./references/desktop-ui-patterns.md), and the shared
[UI design system](../ui-design-system/SKILL.md).

## Workflow

1. Identify whether the control already exists in the app or shared component
   library. Extend/reuse it before creating another variant.
2. Define its purpose, content, and interaction states before styling.
3. Apply semantic color, typography, spacing, and shape tokens from the design system.
4. Preserve keyboard navigation, focus visibility, accessible names, disabled
   behavior, and sensible target sizes.
5. Keep component API and visual behavior consistent across repeated uses.
6. Test at compact and normal layouts, in light and dark themes, and validate
   the affected app.

## Common component patterns

### Buttons
- **Primary button**: Main action on a screen, prominent and highest contrast
- **Secondary button**: Supporting actions, secondary priority
- **Danger button**: Destructive actions only (delete, discard, etc.)
- **State variants**: Normal, hover, pressed, focused, disabled, loading/busy
- **Size variants**: Standard (38px) and compact (34px) for dense layouts

### Input fields & forms
- **Text input**: Persistent label, helper/error text, focus/validation states
- **Combo box / Picker**: Support keyboard selection, show loading/empty/error states
- **Checkbox / Radio**: Semantic selection controls with clear disabled behavior
- **Text area**: Multi-line input with resize affordance and character limits where needed

### Data presentation
- **DataGrid / Table**: Column headers, sorting, selection, loading/empty/error states
- **List items**: Consistent height, spacing, hover/selected states, truncation handling
- **Status badges**: Visual indicator + text (success, warning, error, info, loading)

### Surfaces & containers
- **Card / Panel**: Border + subtle background, consistent padding, clear content hierarchy
- **Dialog / Modal**: Title, content, action buttons, focus management on open/close
- **Toast / Notification**: Temporary feedback at screen edge, dismissible, semantic color

### Navigation
- **Sidebar / Main navigation**: Clear active state, keyboard focus, collapse/expand affordance
- **Breadcrumb**: Sequential path clarity, linking to parent contexts
- **Tab navigation**: Active tab visual prominence, keyboard arrow key support

## Implementation guidelines

Do not encode business behavior in a visual component. Expose a clear command,
event, or binding contract and let the owning view/model handle the workflow.

Ensure each component:
- Respects semantic tokens for all visual properties
- Implements visible focus states for keyboard users
- Provides accessible names and automation identifiers
- Scales correctly in compact and normal density modes
- Works identically in light and dark themes
- Handles empty, loading, error, and success states distinctly
