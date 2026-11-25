# Shared design tokens

The canonical machine-readable starter palette is
[`../assets/design-tokens.json`](../assets/design-tokens.json). Tokens are
platform-neutral semantic values. Platform projects should map them to their
native resource/theme system and avoid defining a competing palette.

## Initial visual foundation

The starter values are aligned with the current QuickScanManager WPF UI:

| Role | Token | Value |
|---|---|---|
| Primary action | `color.action.primary` | `#356AE6` |
| App canvas | `color.surface.canvas` | `#F5F7FA` |
| Main content surface | `color.surface.default` | `#FFFFFF` |
| Primary text | `color.text.primary` | `#172033` |
| Secondary text | `color.text.muted` | `#748099` |
| Default border | `color.border.default` | `#E5E9F0` |
| Warning surface | `color.surface.warning` | `#FFF7E8` |
| Warning text | `color.text.warning` | `#80530C` |
| Warning border | `color.border.warning` | `#F4D9A6` |

These are a starting point, not permission to assume every color combination
passes accessibility contrast. Check foreground/background pairs in context.

## Usage rules

- Prefer semantic roles (`action.primary`, `text.muted`, `surface.raised`) over
  raw hex values in application components.
- Keep the base palette stable across applications. A platform adapter may
  adjust rendering details, but should not redefine the meaning of a token.
- Use the spacing scale in the JSON file. For layout-specific exceptions, use
  the nearest scale value before introducing a new shared size.
- Use typography roles (`body`, `label`, `heading`) instead of arbitrary sizes.
  Respect platform font fallback and text scaling.
- Use shape tokens for cards, inputs, and pills. Avoid mixing many unrelated
  radii and border widths on one screen.
- Define new tokens only when they represent a recurring semantic need. Update
  this document and the JSON together.
- Use the `density.compact` metrics for data-heavy desktop workflows; retain
  readable text and platform-appropriate hit targets instead of shrinking
  controls indiscriminately.

## Compact desktop density

The WPF theme adapter exposes compact control, button, card, and section sizes.
Use these for dense scanning/library screens while keeping the primary action
visually prominent. The compact settings are semantic density values, not a
second color palette.

## Application theme adapters

Map the tokens into each app's normal theme mechanism, for example WPF
`ResourceDictionary`, web CSS custom properties, or a mobile resource/theme
catalog. The JSON file is the shared source; generated or handwritten platform
resources are adapters and should be traceable to it.

The QuickScanManager WPF adapter lives in
`QuickScanManager/src/ScanApp/Themes/DesignTokens.xaml` and is merged from
`App.xaml`.
