# Design System Rules

Visual identity (color palette, typography, spacing, radii, elevation, component tokens) MUST be authored in **DESIGN.md format** — the spec at https://github.com/google-labs-code/design.md.

DESIGN.md is the **single source of truth** for design tokens. Art bible, UX specs, HUD design, interaction patterns, and UI code all reference it. Numeric/color/font values defined elsewhere are a contract violation — fix the offending doc to point at the token.

## Canonical file

- `design/art/DESIGN.md` — project-wide visual identity. One file per game.
- Optional sibling files for distinct surfaces (e.g. `design/art/DESIGN-marketing.md`) only when a separate visual system is justified by ADR.

## Format

Two-layer file per spec:

1. **YAML front matter** — machine-readable tokens (`---` fenced).
2. **Markdown body** — rationale, organized by `##` sections in canonical order.

### Required front matter keys

```yaml
---
version: alpha
name: <project visual identity name>
description: <one-line>
colors:
  <token>: "#RRGGBB"
typography:
  <token>:
    fontFamily: <family>
    fontSize: <Nrem|Npx>
    fontWeight: <int>           # optional
    lineHeight: <number|Npx>    # optional
    letterSpacing: <Nem>        # optional
rounded:
  <scale>: <Npx>
spacing:
  <scale>: <Npx>
components:
  <component-name>:
    backgroundColor: "{colors.<token>}"
    textColor: "{colors.<token>}"
    typography: "{typography.<token>}"
    rounded: "{rounded.<scale>}"
    padding: <Npx>
---
```

### Required body sections (in this order; omit if empty, never reorder)

1. Overview (alias: Brand & Style)
2. Colors
3. Typography
4. Layout (alias: Layout & Spacing)
5. Elevation & Depth (alias: Elevation)
6. Shapes
7. Components
8. Do's and Don'ts

### Token types

| Type | Format | Example |
|------|--------|---------|
| Color | sRGB hex | `"#1A1C1E"` |
| Dimension | number + unit (`px`, `em`, `rem`) | `8px`, `1rem`, `-0.02em` |
| Token reference | `{path.to.token}` | `{colors.primary}` |
| Typography | object with `fontFamily`, `fontSize`, optional `fontWeight`, `lineHeight`, `letterSpacing`, `fontFeature`, `fontVariation` | see above |

Component property whitelist: `backgroundColor`, `textColor`, `typography`, `rounded`, `padding`, `size`, `height`, `width`. Variants (hover, active, pressed, disabled) = separate component entry with related key name (`button-primary-hover`).

## Validation

- Lint with `npx @google/design.md lint design/art/DESIGN.md` before approving the file or merging changes.
- Diff with `npx @google/design.md diff old new` for token-level regressions during review.
- Lint MUST pass (errors=0) for art bible / GDD UI section / UX spec gates to PASS.

## Authoring rules

- New visual project — author DESIGN.md **before** art bible Section 6 (UI Visual Language) and before any UX spec.
- Art bible Color Palette / Typography / UI Visual Language sections describe **rationale**; numeric values live only in DESIGN.md (referenced by token name).
- UX specs, HUD design, interaction patterns reference tokens by `{colors.x}` / `{typography.y}` / `{spacing.z}`. No raw hex / pt / px in those docs except when documenting a value coming directly from DESIGN.md (cite the token).
- New visual decision — update DESIGN.md first, then propagate via `/ags-propagate-design-change`.
- Token rename = breaking change → ADR required.

## UI implementation rules

- UI code consumes DESIGN.md tokens. No hardcoded color/font/spacing literals in UI code — bind to a token.
- Tokens loaded once at startup (engine-native pipeline: ScriptableObject for Unity, Theme resource for Godot, etc.). Source generator or build step from `design/art/DESIGN.md` recommended; if absent, mirror values into a single `UiTokens` data file referenced by all UI code.
- Mirror data file MUST cite source: `# Generated from design/art/DESIGN.md@<commit>`. Manual mirroring without commit cite = audit fail.

## Cross-references

- Art bible — `t_art-bible.md`
- UX spec — `t_ux-spec.md`
- HUD design — `t_hud-design.md`
- Interaction patterns — `t_interaction-patterns.md`
- Coding — `.ags/rules/coding.md` § Data-driven design
