---
name: ui-platform-adaptation
description: "Use when porting, packaging, or synchronizing an app UI across WPF, WinForms, web, mobile, or other desktop platforms. Map shared design tokens to native themes without duplicating design decisions."
---

# UI platform adaptation

Keep the design language shared while implementing UI in platform-appropriate
ways. Read [platform mapping](./references/platform-mapping.md) and use
[`ui-design-system`](../ui-design-system/SKILL.md) as the token authority.

## Workflow

1. Locate the shared token JSON and any existing theme adapters in the target
   app.
2. Reuse an existing adapter or add a small platform-specific adapter that
   maps semantic roles to native resources.
3. Keep business behavior and domain models independent from visual toolkit
   types.
4. Use native controls, focus behavior, accessibility APIs, text scaling, and
   navigation conventions when they improve the platform experience.
5. Keep semantic token names and meanings aligned. Do not fork the palette per
   platform without an approved design-system change.
6. Build/package the target platform and check minimum supported display size,
   scaling, and resource inclusion.

Platform theme resources are adapters, not alternative sources of truth. If
multiple app teams need the same new visual concept, add it to the shared token
source first and then update each adapter.
