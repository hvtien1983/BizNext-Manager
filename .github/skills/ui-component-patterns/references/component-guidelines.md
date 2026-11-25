# Component guidelines

## Components

- **Button:** use primary for the main action, secondary for supporting
  actions, and danger only for destructive actions. Keep labels verb-led and
  concise. Include normal, hover, pressed, focused, disabled, and busy states
  where applicable.
- **Text input:** provide a persistent label, useful hint/error text, keyboard
  and focus behavior, and a visible invalid state. Placeholder text is not a
  replacement for a label.
- **Combo box / picker:** label the choice, show the selected value clearly,
  support keyboard selection, and provide an explicit empty/loading/error
  state when data is dynamic.
- **Card / box / panel:** use a surface token, consistent padding, and a
  restrained border/radius. Use elevation only to communicate hierarchy.
- **List / table:** define empty, loading, error, selected, and populated
  states. Keep column labels readable and preserve useful detail at narrow
  sizes.
- **Dialog:** title the task, keep the primary action predictable, make cancel
  explicit, and return focus to the control that opened it.

## States and accessibility

- Never communicate status using color alone; pair it with text, iconography,
  or another programmatic cue.
- Ensure foreground/background contrast is appropriate for the applicable
  accessibility target.
- Keep keyboard focus visible and logical. Interactive controls need accessible
  names and meaningful automation/accessibility identifiers where supported.
- Support system text scaling and platform accessibility APIs.
- Do not make disabled controls the only way to explain why an action is
  unavailable; provide nearby context where useful.

## Reuse and API

- Share a component when the same behavior and visual contract recur.
- Prefer a small number of documented variants over one-off styling props.
- Separate presentation from business logic; use bindings/commands/events.
- Do not force identical control visuals across platforms when native behavior
  improves usability. Preserve the shared semantic role and state model.
