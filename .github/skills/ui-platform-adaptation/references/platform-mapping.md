# Platform mapping

| Shared concept | WPF | Web | Mobile / other |
|---|---|---|---|
| Semantic colors | `ResourceDictionary` brushes | CSS custom properties | Native theme/resource palette |
| Typography roles | Styles and text resources | CSS typography variables | Text styles / typography scale |
| Spacing and shape | Styles, control templates | Component styles / CSS variables | Layout and shape resources |
| Component behavior | Commands, bindings, routed events | Components, state, events | Native controls, view models |
| Accessibility | UI Automation, keyboard focus | ARIA, keyboard, semantics | Platform accessibility APIs |

## Porting rules

- Keep token names stable even when native property names differ.
- Preserve meaning and hierarchy rather than copying pixel values blindly;
  account for device density, font rendering, text scaling, and platform
  conventions.
- Put platform-specific mappings in the app's theme/resource layer, not in
  shared business logic.
- Avoid introducing a cross-platform UI framework solely to share colors or
  spacing; token resources are sufficient for visual consistency.
- Ensure theme files and token assets are included in build/package output when
  required by the target framework.
- When platform limitations require a visual deviation, document the reason
  next to the adapter or in the app's UI documentation.
