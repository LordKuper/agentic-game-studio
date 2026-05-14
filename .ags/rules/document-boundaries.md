# Document Boundaries

Single source of truth (SSoT). Every fact lives in exactly one document type. Duplication = contract violation.

## 1. SSoT Matrix

| Decision type | Single source | Forbidden in |
|---|---|---|
| Concept, mechanic, intent, balance-intent, acceptance | GDD (`design/gdd/<system>.md`) | ADR, UX-spec, HUD-spec, art-bible |
| Technical realization, contract, code pattern, data schema, library/API choice, perf budget | ADR (`design/architecture/adr-NNNN-*.md`) | GDD, UX-spec, HUD-spec |
| User flow, controls, screen states, navigation | UX-spec (`design/ux/<screen>.md`) | GDD, HUD-spec, ADR |
| HUD widget, layout, visual binding, in-game overlay | HUD-spec (`design/ux/hud.md`) | UX-spec, GDD, art-bible |
| Color, typography, spacing, radii, component tokens | DESIGN.md (`design/art/DESIGN.md`) | art-bible, UX, HUD, code (raw literals) |
| Numeric balance values | data-config (engine-side asset) | GDD describes intent, cites file path |
| Canonical entity ids (item, enemy, skill, faction…) | `design/registry/entities.yaml` | every other doc cites by id |
| Runtime game-state ownership (which system owns which mutable fact at runtime — health, inventory, player position, save data) | ADR declaring authority (`design/architecture/adr-NNNN-*.md`) + implementing system | other systems read via interface / event, never duplicate authoritative copy |

## 2. GDD = concept-only

GDD describes **what** + **why**, never **how**.

**Forbidden in GDD:**
- Class names, method names, namespaces, package paths.
- Code patterns (Observer, ECS, Singleton, etc.).
- Library / framework / engine-API choices.
- Data-schema definitions (JSON/YAML/SQL shapes).
- Perf numbers in ms/MB/FPS — tech budget belongs to ADR. (Acceptance like "feels instant" or "no perceptible drop" stays.)
- Links to ADR. **GDD does not cite ADR.** Concept is independent of realization.

**Allowed in GDD:**
- Mechanic rules, formulas, state machines (conceptual).
- High-level flow diagrams using concept-nodes (no class/library names).
- Pseudocode for rule-clarity only — no real identifiers.
- Acceptance criteria in player-observable terms.
- Tuning knobs (parameter intent + ranges; concrete numbers in data-config).

## 3. ADR = realization-only

- Cites ≥1 GDD section it serves (`gdd_source`).
- ADR may cite other ADR (dependency chain).
- ADR must NOT redefine concept rules — copies from GDD = drift risk; reference instead.

## 4. UX-spec / HUD-spec

- UX-spec cites GDD section it serves.
- HUD-spec cites UX-spec (if cross-referenced) + DESIGN.md tokens.
- No duplication of mechanic rules from GDD — only player-facing flow / widget surface.
- No raw color/font/spacing literals — use `{colors.x}` / `{typography.y}` / `{spacing.z}` / `{components.w}`.

## 5. Approval marker (front-matter)

Every document under boundary control carries YAML front-matter at file top:

```yaml
---
status: draft | approved
approved_at: YYYY-MM-DD   # required when status: approved
---
```

Applies to: GDD, ADR, UX-spec, HUD-spec, art-bible, DESIGN.md, game-concept, engine doc.

`status: draft` is default. `status: approved` set only on explicit user approval. `approved_at` mandatory when approved.

## 6. Precondition chain

Skills enforce **automatically** by reading front-matter. No user prompt — fail = abort with explicit reason.

| Doc to create / work to start | Required predecessor (`status: approved`) |
|---|---|
| ADR | cited GDD section (`gdd_source`) |
| UX-spec | cited GDD |
| HUD-spec | cited UX-spec **and** `design/art/DESIGN.md` |
| art-bible (Section 6 UI) | `design/art/DESIGN.md` |
| anything citing entity id | `design/registry/entities.yaml` entry exists |
| Story implementation touching player flow / screens / controls | UX-spec (`design/ux/<screen>.md`) cited |
| Story implementation adding/modifying in-game HUD widget | HUD-spec (`design/ux/hud.md`) cited |
| Story implementation rendering UI | `design/art/DESIGN.md` token cited (no raw hex/px/pt) |

Failure form:
```
ABORT — precondition not met.
Required: design/gdd/combat.md status=approved.
Found: status=draft (or missing front-matter).
Fix: get GDD approved (set status: approved + approved_at), then retry.
```

## 7. ADR `gdd_source` field

Mandatory line in every ADR, placed inside the `## Preconditions` section (first section after the title):

```markdown
**GDD source**: `design/gdd/<system>.md#<section-anchor>` (status: approved at YYYY-MM-DD)
```

Multiple sources = multiple lines. Foundational ADR with no GDD: write `**GDD source**: Foundational — no GDD requirement. Enables: <list>`.

## 8. Enforcement (`/ags-consistency-check` rules)

In addition to existing entity/value checks, scan for:

| Rule | Detection |
|---|---|
| Tech-leak in GDD | grep GDD for class/namespace patterns: `class\s+\w+`, `namespace\s+`, `using\s+`, `import\s+`, library names from engine-reference, ms/MB/FPS literals outside player-observable acceptance |
| Concept-leak in ADR | ADR sections contain mechanic rules without `gdd_source` cite for that rule |
| Raw visual literal | hex `#[0-9a-fA-F]{3,8}`, `\d+px`, `\d+pt`, `\d+rem` outside `design/art/DESIGN.md` (allowed in DESIGN.md `colors:` / dimension fields only) |
| GDD→ADR cite | grep GDD for `adr-\d+` or `ADR-\d+` references |
| Duplicated content | same block appears in GDD + ADR/UX/HUD (fingerprint by ≥3 consecutive matching sentences) |
| Missing approval | doc cited as predecessor lacks `status: approved` front-matter |

Hits → conflict report with file + line + rule violated. PASS only when zero.

## 9. Migration policy

- Existing docs without front-matter → treated as `status: draft`.
- New ADR / UX-spec / HUD-spec on top of legacy unmarked GDD → fails precondition. Fix: approve the GDD (add front-matter) first.
- Run `/ags-consistency-check` after these rules land — surface violations, fix incrementally. No bulk rewrite.

## 10. Cross-references

- `.ags/rules/coding.md` — data-driven design (numbers in data-config).
- `.ags/rules/design-system.md` — DESIGN.md tokens authority.
- `.ags/rules/coordination.md` — who approves what.
- `.ags/rules/review-workflow.md` — review loops feeding approval.
