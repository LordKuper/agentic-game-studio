---
status: draft        # draft | approved
approved_at:         # YYYY-MM-DD; required when status: approved
---

# Art Bible: [Game Title]

## Document Status
- **Version**: 1.0
- **Last Updated**: [Date]
- **Owned By**: art-director
- **Approval marker**: see YAML front-matter `status` field. See `.ags/rules/document-boundaries.md`.

## Visual Identity Summary
[2-3 sentences: overall visual identity]

## Reference Board
[Reference games, films, art. What specific quality each represents.]

| Reference | Medium | What We're Taking |
| --------- | ------ | ----------------- |
| [Name] | [Game/Film/Art] | [Specific quality] |

## Design Tokens (DESIGN.md)

> **Authoritative tokens** (color hex, typography, spacing, radii, components) live in `design/art/DESIGN.md` per the [google-labs-code DESIGN.md spec](https://github.com/google-labs-code/design.md). This art bible references tokens by name; numeric values are NOT duplicated here.
>
> Lint gate: `npx @google/design.md lint design/art/DESIGN.md` MUST pass (errors=0) before this art bible is approved.

- Token file: `design/art/DESIGN.md`
- Last lint: [date / errors / warnings]

## Color Palette

> Rationale only. Token names + reasoning. Hex values live in DESIGN.md `colors:`.

### Primary Palette
| Token | Role | Usage |
| ----- | ---- | ----- |
| `{colors.primary}` | [Role] | [Where + when] |
| `{colors.secondary}` | [Role] | [Where + when] |
| `{colors.tertiary}` | [Role] | [Where + when] |
| `{colors.neutral}` | [Role] | [Where + when] |

### Emotional Color Mapping
| Game State | Dominant Colors | Mood |
| ---------- | --------------- | ---- |
| Exploration | [Colors] | [Feeling] |
| Combat | [Colors] | [Feeling] |
| Safe zones | [Colors] | [Feeling] |
| Danger | [Colors] | [Feeling] |

## Art Style

### Rendering Style
[Realistic / Stylized / Pixel / Cel-shaded / etc.]

### Proportions
[Character proportions, environment scale, UI scale relationships]

### Level of Detail
[How detailed are characters, environments, UI?]

### Visual Hierarchy
[How guide player's eye? What's always most prominent?]

## Character Art Standards
[Silhouette, color coding, animation style, proportions]

## Environment Art Standards
[Tilesets, modularity, lighting, atmospheric effects, scale]

## UI Art Standards

> All token values (color, typography, spacing, radii, component styles) live in `design/art/DESIGN.md`. This section captures **rationale and visual language** — button styles, icon style, menu layout, HUD density — that explain *why* DESIGN.md tokens are shaped as they are.

- Button family rationale: references `{components.button-primary}`, `{components.button-secondary}`, `{components.button-destructive}`
- Typography rationale: references `{typography.h1}`, `{typography.body-md}`, `{typography.label-caps}`
- Spacing rhythm rationale: references `{spacing.sm}`, `{spacing.md}`, `{spacing.lg}`
- Radii / shape language: references `{rounded.sm}`, `{rounded.md}`
- Iconography style: [stroke weight, fill rules, grid] — NOT in DESIGN.md scope; document here.
- HUD density principle: [from `design/gdd/hud-design.md` § HUD Philosophy]

## VFX Standards
[Particle style, screen effects, impact feedback, color coding]

## Asset Production Standards

### Naming Convention
`[category]_[name]_[variant]_[size].[ext]`

### Texture Standards
| Category | Max Resolution | Format | Color Space |
| -------- | -------------- | ------ | ----------- |
| Characters | [Size] | [Format] | [Space] |
| Environments | [Size] | [Format] | [Space] |
| UI | [Size] | [Format] | [Space] |
| VFX | [Size] | [Format] | [Space] |

### Animation Standards
[Frame rates, blend times, animation graph structure]

## Accessibility
- Colorblind-safe UI required
- Min text size: [X]px at 1080p
- High contrast mode specs
- Icon + color (never color alone) for game state
