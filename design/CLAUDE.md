# Design Directory

Rules for `design/`. Two subdirs: `architecture/` (technical), `gdd/` (game design).

## Architecture (`design/architecture/`)

ADR template: `.ags/templates/architecture-decision-record.md`

**Required sections:** Title, Status, Context, Decision, Consequences, ADR Dependencies, Engine Compatibility, GDD Requirements Addressed.

**Status lifecycle:** `Proposed` → `Accepted` → `Superseded`. Never skip `Accepted` — stories on `Proposed` ADRs auto-block. Use `/architecture-decision`.

**TR Registry** (`tr-registry.yaml`): stable IDs (e.g. `TR-MOV-001`) link GDD requirements to stories. Append-only, never renumber. Updated by `/architecture-review` Phase 8.

**Control Manifest** (`control-manifest.md`): flat Required / Forbidden / Guardrails per layer. Header has date-stamped `Manifest Version:`. Stories embed version; `/story-done` checks staleness.

**Validation:** `/architecture-review` after ADR set complete.

## GDD (`design/gdd/`)

**8 required sections, in order:**
1. Overview — one-paragraph summary
2. Player Fantasy — intended feeling
3. Detailed Rules — unambiguous mechanics
4. Formulas — math with variables defined
5. Edge Cases — unusual situations
6. Dependencies — other systems
7. Tuning Knobs — configurable values
8. Acceptance Criteria — testable success

**File naming:** `[system-slug].md` (e.g. `movement-system.md`).

**Index:** update `gdd/systems-index.md` when adding GDD.

**Length cap:** 5000 words / 10000 tokens soft cap. Shorten wording, keep meaning.

**Style:** short, clear, unambiguous. Tech detail only when needed for correct implementation.

**Single source of truth:** each rule in one best section. Other sections reference, never duplicate.

**Design order:** Foundation → Core → Feature → Presentation → Polish.

**Validation:** `/design-review [path]` after authoring. `/review-all-gdds` after related set.

## UX Specs (`design/ux/`)

- Per-screen: `design/ux/[screen-name].md`
- HUD: `design/ux/hud.md`
- Patterns: `design/ux/interaction-patterns.md`

Accessibility: `design/accessibility-requirements.md` (project root, not `ux/`).

Author via `/ux-design`. Validate `/ux-review` before `/team-ui`.

## Engine Reference

API snapshots in `.ags/docs/engine-reference/`. Always check before using engine API — LLM training predates pinned version.
