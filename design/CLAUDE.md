# Design Directory

Rules for files in `design/`. Two subdirs: `architecture/` (technical decisions), `gdd/` (game design docs).

## Architecture (`design/architecture/`)

ADR template: `.ags/templates/architecture-decision-record.md`

**Required sections:** Title, Status, Context, Decision, Consequences, ADR Dependencies, Engine Compatibility, GDD Requirements Addressed.

**Status lifecycle:** `Proposed` в†’ `Accepted` в†’ `Superseded`. Never skip `Accepted` вЂ” stories on `Proposed` ADRs auto-block. Use `/architecture-decision`.

**TR Registry** (`tr-registry.yaml`): stable IDs (e.g. `TR-MOV-001`) link GDD requirements to stories. Append-only вЂ” never renumber. Updated by `/architecture-review` Phase 8.

**Control Manifest** (`control-manifest.md`): flat Required / Forbidden / Guardrails per layer. Header has date-stamped `Manifest Version:`. Stories embed version; `/story-done` checks staleness.

**Validation:** `/architecture-review` after ADR set complete.

## GDD (`design/gdd/`)

**8 required sections, in order:**
1. Overview вЂ” one-paragraph summary
2. Player Fantasy вЂ” intended feeling
3. Detailed Rules вЂ” unambiguous mechanics
4. Formulas вЂ” math with variables defined
5. Edge Cases вЂ” unusual situations
6. Dependencies вЂ” other systems
7. Tuning Knobs вЂ” configurable values
8. Acceptance Criteria вЂ” testable success

**File naming:** `[system-slug].md` (e.g. `movement-system.md`).

**Index:** update `gdd/systems-index.md` when adding GDD.

**Length cap:** 5000 words / 10000 tokens soft cap per file. Shorten wording, keep meaning.

**Style:** short, clear, unambiguous. Tech detail only when needed for correct implementation.

**Single source of truth:** each rule described once in best section. Other sections reference, never duplicate.

**Design order:** Foundation в†’ Core в†’ Feature в†’ Presentation в†’ Polish.

**Validation:** `/design-review [path]` after authoring. `/review-all-gdds` after related set.

## Quick Specs (`design/quick-specs/`)

Lightweight specs: tuning, minor mechanics, balance. Author via `/quick-design`.

## UX Specs (`design/ux/`)

- Per-screen: `design/ux/[screen-name].md`
- HUD: `design/ux/hud.md`
- Patterns: `design/ux/interaction-patterns.md`
- Accessibility: `design/ux/accessibility-requirements.md`

Author via `/ux-design`. Validate `/ux-review` before `/team-ui`.

## Engine Reference

Engine API snapshots live in `.ags/docs/engine-reference/` (not here). Always check before using engine API вЂ” LLM training predates pinned version.
