# Agent UI skills

These workspace skills provide shared UI design guidance for this repository.
They are loaded on demand by compatible coding agents and are not runtime
dependencies of the applications.

```text
.github/skills/
├── ui-design-system/
│   ├── SKILL.md
│   ├── assets/design-tokens.json
│   └── references/design-tokens.md
├── ui-component-patterns/
│   ├── SKILL.md
│   └── references/component-guidelines.md
└── ui-platform-adaptation/
    ├── SKILL.md
    └── references/platform-mapping.md
```

## Skills

- **ui-design-system** — shared color, typography, spacing, shape, and surface
  tokens.
- **ui-component-patterns** — reusable controls, component states, and
  accessibility expectations.
- **ui-platform-adaptation** — applying the shared design language to different
  UI frameworks and packaged applications.

`ui-design-system/assets/design-tokens.json` is the machine-readable source for
the initial shared tokens. Keep semantic token names stable across platforms;
implementations can map them to native theme resources instead of copying
framework-specific markup.
