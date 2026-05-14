---
name: ags-adopt
description: "Brownfield onboarding — audits existing project artifacts for template format compliance (not just existence), classifies gaps by impact, and produces a numbered migration plan. Run this when joining an in-progress project or upgrading from an older template version. Distinct from /ags-project-stage-detect (which checks what exists) — this checks whether what exists will actually work with the template's skills."
argument-hint: "[focus: full | gdds | adrs | stories | infra]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, AskUserQuestion
agent: technical-director
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

# Adopt — Brownfield Template Adoption

Audit existing artifacts for **format compliance** with template skill pipeline. Produce prioritised migration plan.

**Not `/ags-project-stage-detect`.** That skill: *what exists?* This skill: *will it work with template skills?*

GDDs/ADRs/stories may exist but format-sensitive skills fail silently if internals wrong.

**Output:** `docs/adoption-plan-[date].md` — persistent checkable plan.

**Argument modes:** `$ARGUMENTS[0]` (blank = `full`)

- `full` — all artifact types
- `gdds` — GDD format only
- `adrs` — ADR format only
- `stories` — story format only
- `infra` — infrastructure gaps (registry, manifest, epics/index.md, stage.md, stubs.md, decisions-log.md)

---

## Phase 1: Detect Project State

Emit one line: `"Scanning project artifacts..."`. Read silently.

### Existence check
- `.ags/project/stage.md` — read if present (authoritative phase)
- `design/gdd/game-concept.md`
- `design/gdd/systems-index.md`
- Count GDDs: `design/gdd/*.md` (excl. game-concept, systems-index)
- Count ADRs: `design/architecture/adr-*.md`
- Count stories: `.ags/project/epics/**/*.md` (excl. EPIC.md)
- `.ags/rules/technical-preferences.md` — engine configured?
- `.ags/docs/engine-reference/` — present?
- Glob `docs/adoption-plan-*.md` — note most recent prior plan

### Infer phase (if no stage.md)
Same heuristic as `/ags-project-stage-detect`:
- 10+ source files in `Assets/Scripts/` → Production
- Stories in `.ags/project/epics/` → Pre-Production
- ADRs exist → Technical Setup
- systems-index.md exists → Systems Design
- game-concept.md exists → Concept
- Nothing → Fresh (suggest `/ags-start`)

If fresh, `AskUserQuestion`:
- "Fresh project — no artifacts. `/ags-adopt` is for projects with work to migrate. What now?"
  - "Run `/ags-start` — guided onboarding"
  - "My artifacts are in non-standard location — help find them"
  - "Cancel"

Stop after any choice — each leads elsewhere.

Report: "Detected phase: [phase]. Found: [N] GDDs, [M] ADRs, [P] stories."

---

## Phase 2: Format Audit

For each artifact in scope, check internal structure not just existence.

### 2a: GDD Format Audit

Check 8 required sections per GDD by heading scan:

| Required Section | Heading pattern |
|---|---|
| Overview | `## Overview` |
| Player Fantasy | `## Player Fantasy` |
| Detailed Rules / Design | `## Detailed` or `## Core Rules` or `## Detailed Design` |
| Formulas | `## Formulas` or `## Formula` |
| Edge Cases | `## Edge Cases` |
| Dependencies | `## Dependencies` or `## Depends` |
| Tuning Knobs | `## Tuning` |
| Acceptance Criteria | `## Acceptance` |

Per GDD record: sections present, missing, has content vs placeholder (`[To be designed]`).

Also check `**Status**:` field. Valid: `In Design`, `Designed`, `In Review`, `Approved`, `Needs Revision`.

### 2b: ADR Format Audit

| Section | Impact if missing |
|---|---|
| `## Status` | **BLOCKING** — `/ags-story-readiness` ADR check silently passes everything |
| `## ADR Dependencies` | HIGH — `/ags-architecture-review` dependency ordering breaks |
| `## Engine Compatibility` | HIGH — post-cutoff API risk unknown |
| `## GDD Requirements Addressed` | MEDIUM — traceability matrix loses coverage |
| `## Performance Implications` | LOW — not pipeline-critical |

Record per ADR: sections present, missing, current Status value.

### 2c: systems-index.md Format Audit

If exists:

1. **Parenthetical status values** — Grep Status cells for `"Needs Revision ("`, `"In Progress ("`, etc. Break exact-string matching in `/ags-gate-check`, `/ags-create-stories`, `/ags-architecture-review`. **BLOCKING.**
2. **Valid status values** — must be only: `Not Started`, `In Progress`, `In Review`, `Designed`, `Approved`, `Needs Revision`. Flag others.
3. **Column structure** — minimum: System, Layer, Priority, Status. Missing degrades skills.

### 2d: Story Format Audit

Per story:
- **`Manifest Version:` field** — present? (LOW — auto-passes if absent)
- **TR-ID reference** — `TR-[a-z]+-[0-9]+` pattern? (MEDIUM — no staleness tracking)
- **ADR reference** — `ADR-` pattern present?
- **Status field** — present, readable?
- **Acceptance criteria** — checkbox list (`- [ ]`)?

### 2e: Infrastructure Audit

| Artifact | Path | Impact if missing |
|---|---|---|
| TR registry | `design/architecture/tr-registry.yaml` | HIGH — no stable requirement IDs |
| Control manifest | `design/architecture/control-manifest.md` | HIGH — no layer rules for stories |
| Manifest version stamp | manifest header `Manifest Version:` | MEDIUM — staleness blind |
| Sprint status | `.ags/project/epics/index.md` | MEDIUM — `/ags-help` falls back to markdown |
| Stage file | `.ags/project/stage.md` | MEDIUM — phase auto-detect unreliable |
| Engine reference | `.ags/docs/engine-reference/[engine]/VERSION.md` | HIGH — ADR engine checks blind |
| Architecture traceability | `design/architecture/architecture-traceability.md` | MEDIUM — no persistent matrix |

### 2f: Technical Preferences Audit

Read `.ags/rules/technical-preferences.md`. Check each field for `[TO BE CONFIGURED]`:
- Engine, Language, Rendering, Physics → HIGH if unconfigured (ADR skills fail)
- Naming conventions → MEDIUM
- Performance budgets → MEDIUM
- Forbidden Patterns, Allowed Libraries → LOW (empty by design)

---

## Phase 3: Classify and Prioritise Gaps

Four severity tiers:

**BLOCKING** — skills silently produce wrong results now. Examples: ADR missing Status, systems-index parentheticals, engine unconfigured with ADRs present.

**HIGH** — stories missing safety checks; infra bootstrap fails. Examples: ADRs missing Engine Compatibility, GDDs missing Acceptance Criteria, tr-registry.yaml missing.

**MEDIUM** — quality/tracking degradation, not broken. Examples: GDDs missing Tuning/Formulas, stories missing TR-IDs, `.ags/project/epics/index.md` missing.

**LOW** — nice-to-have. Examples: stories missing Manifest Version, GDDs missing Open Questions.

Count totals per tier. Zero BLOCKING + zero HIGH → template-compatible, advisory only.

---

## Phase 4: Build the Migration Plan

Numbered ordered plan. Ordering:
1. BLOCKING first (must fix before pipeline runs reliably)
2. HIGH next; infrastructure before GDD/ADR content (bootstrapping needs correct formats)
3. MEDIUM ordered: GDD → ADR → story (stories depend on GDDs+ADRs)
4. LOW last

Per gap entry:
- Problem statement (one sentence, no jargon)
- Exact fix command if skill handles it
- Manual steps if direct edit
- Time estimate (5 min / 30 min / 1 session)
- Checkbox `- [ ]`

**Special case — systems-index parentheticals:** always first. Show exact values + replacement text. Offer immediate fix before writing plan.

**Special case — ADRs missing Status:** fix is `/ags-architecture-decision retrofit design/architecture/adr-[NNNN]-[slug].md`. List each ADR as separate item.

**Special case — GDDs missing sections:** list missing sections. Fix: `/ags-design-system retrofit design/gdd/[filename].md`.

**Infrastructure bootstrap order — always this sequence:**
1. Fix ADR formats first (registry depends on ADR Status)
2. Run `/ags-architecture-review` → bootstraps `tr-registry.yaml`
3. Run `/ags-create-control-manifest` → manifest with version stamp
4. Run `/ags-create-epics` → creates `.ags/project/epics/index.md` and first epic
5. Run `/ags-gate-check [phase]` → writes `stage.md`

**Existing stories** — note explicitly:
> "Existing stories continue to work with all template skills — new format checks auto-pass when fields absent. They won't get TR-ID staleness tracking or manifest version checks until regenerated. Intentional: do not regenerate stories in progress."

---

## Phase 5: Present Summary and Ask to Write

Compact summary before writing:

```
## Adoption Audit Summary
Phase detected: [phase]
Engine: [configured / NOT CONFIGURED]
GDDs audited: [N] ([X] fully compliant, [Y] with gaps)
ADRs audited: [N] ([X] fully compliant, [Y] with gaps)
Stories audited: [N]

Gap counts:
  BLOCKING: [N] — template skills will malfunction without these fixes
  HIGH:     [N] — unsafe to run /ags-create-stories or /ags-story-readiness
  MEDIUM:   [N] — quality degradation
  LOW:      [N] — optional improvements

Estimated remediation: [X blocking items × ~Y min each = roughly Z hours]
```

Show **Gap Preview** before asking:
- BLOCKING: each gap one-line bullet with actual problem (e.g. `systems-index.md: 3 rows have parenthetical status values`, `adr-0002.md: missing ## Status section`). No counts — actual items.
- HIGH/MEDIUM/LOW: counts only (e.g. `HIGH: 4, MEDIUM: 2, LOW: 1`).

If prior plan detected:
> "Previous plan at `docs/adoption-plan-[prior-date].md`. New plan reflects current state — does not diff."

`AskUserQuestion`:
- "Ready to write the migration plan?"
  - "Yes — write `docs/adoption-plan-[date].md`"
  - "Show full plan preview first (don't write yet)"
  - "Cancel — handle migration manually"

If "Show full plan preview", output complete plan as fenced markdown. Re-ask same three options.

---

## Phase 6: Write the Adoption Plan

If approved, write `docs/adoption-plan-[date].md`:

```markdown
# Adoption Plan

> **Generated**: [date]
> **Project phase**: [phase]
> **Engine**: [name + version, or "Not configured"]
> **Template version**: v1.0+

Work through steps in order. Check off each item. Re-run `/ags-adopt` to check remaining gaps.

---

## Step 1: Fix Blocking Gaps

[One sub-section per blocking gap with problem, fix command, time estimate, checkbox]

---

## Step 2: Fix High-Priority Gaps

[One sub-section per high gap]

---

## Step 3: Bootstrap Infrastructure

### 3a. Register existing requirements (creates tr-registry.yaml)
Run `/ags-architecture-review` — bootstraps TR registry from existing GDDs and ADRs.
**Time**: 1 session
- [ ] tr-registry.yaml created

### 3b. Create control manifest
Run `/ags-create-control-manifest`
**Time**: 30 min
- [ ] design/architecture/control-manifest.md created

### 3c. Create sprint tracking file
Run `/ags-create-epics update`
**Time**: 5 min
- [ ] .ags/project/epics/index.md created

### 3d. Set authoritative project stage
Run `/ags-gate-check [current-phase]`
**Time**: 5 min
- [ ] .ags/project/stage.md written

---

## Step 4: Medium-Priority Gaps

[One sub-section per medium gap]

---

## Step 5: Optional Improvements

[One sub-section per low gap]

---

## What to Expect from Existing Stories

Existing stories work with all template skills. New format checks (TR-ID validation, manifest version staleness) auto-pass when fields absent — nothing breaks. No staleness tracking until regenerated. Do not regenerate stories in progress or done.

---

## Re-run

Run `/ags-adopt` after Step 3 to verify all blocking and high gaps resolved.
```

---

## Phase 7: Offer First Action

Pick single highest-priority gap. Offer immediate fix via `AskUserQuestion`. First branch that applies:

**Parenthetical status values in systems-index.md:**
- "Most urgent: `systems-index.md` — [N] rows have parenthetical status values (e.g. `Needs Revision (see notes)`) breaking /ags-gate-check, /ags-create-stories, /ags-architecture-review now. Fix in-place?"
  - "Fix it now — edit systems-index.md"
  - "I'll fix it myself"
  - "Done — leave me with the plan"

**ADRs missing `## Status` (no parenthetical issue):**
- "Most urgent: add `## Status` to [N] ADR(s): [list]. Without it, /ags-story-readiness silently passes all ADR checks. Start with [first filename]?"
  - "Yes — retrofit [first filename] now"
  - "Retrofit all [N] ADRs one by one"
  - "I'll handle ADRs myself"

**GDDs missing Acceptance Criteria (no blocking above):**
- "Most urgent: missing Acceptance Criteria in [N] GDD(s): [list]. Without them /ags-create-stories can't generate stories. Start with [highest-priority filename]?"
  - "Yes — add Acceptance Criteria to [GDD filename] now"
  - "Do all [N] GDDs one by one"
  - "I'll handle GDDs myself"

**No BLOCKING or HIGH gaps:**
- "No blocking gaps — template-compatible. What next?"
  - "Walk me through medium-priority improvements"
  - "Run /ags-project-stage-detect for broader health check"
  - "Done — work through plan at my own pace"

---

## Collaborative Protocol

1. **Read silently** — full audit before presenting
2. **Show summary first** — let user see scope before writing
3. **Ask before writing** — confirm before creating plan file
4. **Offer, don't force** — advisory; user decides what to fix and when
5. **One action at a time** — one specific next step, not six
6. **Never regenerate existing artifacts** — fill gaps only; do not rewrite GDDs/ADRs/stories
