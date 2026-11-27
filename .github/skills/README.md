# Agent UI skills

These workspace skills provide shared UI design guidance for this repository.
They are loaded on demand by compatible coding agents and are not runtime
dependencies of the applications.

```text
.github/skills/
├── ui-design-system/
│   ├── SKILL.md
│   ├── assets/design-tokens.json
│   └── references/ (design-tokens.md, visual-design-standards.md)
├── ui-component-patterns/
│   ├── SKILL.md
│   └── references/ (component-guidelines.md, desktop-ui-patterns.md)
├── ui-platform-adaptation/
│   ├── SKILL.md
│   └── references/platform-mapping.md
├── fluent2-design/
│   ├── SKILL.md
│   └── references/ (12 files: principles, color, typography, spacing, shapes,
│                    elevation, iconography, components, navigation,
│                    accessibility, animation, wpf-adaptation)
├── wpf-visual-design/
│   ├── SKILL.md
│   └── references/wpf-design-implementation.md
├── wpf-ux-standards/
│   ├── SKILL.md
│   └── references/wpf-ux-guidelines.md
└── wpf/
    └── SKILL.md
```

## Skills

- **ui-design-system** — shared color, typography, spacing, shape, and surface
  tokens.
- **ui-component-patterns** — reusable controls, component states, and
  accessibility expectations.
- **ui-platform-adaptation** — applying the shared design language to different
  UI frameworks and packaged applications.
- **fluent2-design** — Fluent 2 as the chosen design language: official rules,
  Windows conventions and BizNext decisions kept separate and labelled by
  source. Knowledge base only until the WPF design system phase is approved.
- **wpf-visual-design** — implementing tokens, themes and templates in XAML.
- **wpf-ux-standards** — interaction states, keyboard, accessibility, feedback.
- **wpf** — WPF/XAML, bindings, MVVM, resource dictionaries, threading, and
  runtime validation for Windows desktop UI.

`ui-design-system/assets/design-tokens.json` is the machine-readable source for
the initial shared tokens. Keep semantic token names stable across platforms;
implementations can map them to native theme resources instead of copying
framework-specific markup.
