---
name: adopt
description: "Brownfield onboarding вЂ” audits existing project artifacts for template format compliance (not just existence), classifies gaps by impact, and produces a numbered migration plan. Run this when joining an in-progress project or upgrading from an older template version. Distinct from /project-stage-detect (which checks what exists) вЂ” this checks whether what exists will actually work with the template's skills."
argument-hint: "[focus: full | gdds | adrs | stories | infra]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, AskUserQuestion
agent: technical-director
---

# Adopt вЂ” Brownfield Template Adoption

This skill audits an existing project's artifacts for **format compliance** with
the template's skill pipeline, then produces a prioritised migration plan.

**This is not `/project-stage-detect`.**
`/project-stage-detect` answers: *what exists?*
`/adopt` answers: *will what exists actually work with the template's skills?*

A project can have GDDs, ADRs, and stories вЂ” and every format-sensitive skill
will still fail silently or produce wrong results if those artifacts are in the
wrong internal format.

**Output:** `docs/adoption-plan-[date].md` вЂ” a persistent, checkable migration plan.

**Argument modes:**

**Audit mode:** `$ARGUMENTS[0]` (blank = `full`)

- **No argument / `full`**: Complete audit вЂ” all artifact types
- **`gdds`**: GDD format compliance only
- **`adrs`**: ADR format compliance only
- **`stories`**: Story format compliance only
- **`infra`**: Infrastructure artifact gaps only (registry, manifest, sprint-status, stage.txt)

---

## Phase 1: Detect Project State

Emit one line before reading: `"Scanning project artifacts..."` вЂ” this confirms the
skill is running during the silent read phase.

Then read silently before presenting anything else.

### Existence check
- `.ags/project/stage.txt` вЂ” if present, read it (authoritative phase)
- `design/gdd/concept.md` вЂ” concept exists?
- `design/gdd/systems-index.md` вЂ” systems index exists?
- Count GDD files: `design/gdd/*.md` (excluding game-concept.md and systems-index.md)
- Count ADR files: `design/architecture/adr-*.md`
- Count story files: `.ags/project/epics/**/*.md` (excluding EPIC.md)
- `.ags/rules/technical-preferences.md` вЂ” engine configured?
- `.ags/docs/engine-reference/` вЂ” engine reference docs present?
- Glob `docs/adoption-plan-*.md` вЂ” note the filename of the most recent prior plan if any exist

### Infer phase (if no stage.txt)
Use the same heuristic as `/project-stage-detect`:
- 10+ source files in `src/` в†’ Production
- Stories in `.ags/project/epics/` в†’ Pre-Production
- ADRs exist в†’ Technical Setup
- systems-index.md exists в†’ Systems Design
- game-concept.md exists в†’ Concept
- Nothing в†’ Fresh (not a brownfield project вЂ” suggest `/ags-start`)

If the project appears fresh (no artifacts at all), use `AskUserQuestion`:
- "This looks like a fresh project вЂ” no existing artifacts found. `/adopt` is for
  projects with work to migrate. What would you like to do?"
  - "Run `/ags-start` вЂ” begin guided first-time onboarding"
  - "My artifacts are in a non-standard location вЂ” help me find them"
  - "Cancel"

Then stop вЂ” do not proceed with the audit regardless of which option the user picks
(each option leads to a different skill or manual investigation).

Report: "Detected phase: [phase]. Found: [N] GDDs, [M] ADRs, [P] stories."

---

## Phase 2: Format Audit

For each artifact type in scope (based on argument mode), check not just that
the file exists but that it contains the internal structure the template requires.

### 2a: GDD Format Audit

For each GDD file found, check for the 8 required sections by scanning headings:

| Required Section | Heading pattern to look for |
|---|---|
| Overview | `## Overview` |
| Player Fantasy | `## Player Fantasy` |
| Detailed Rules / Design | `## Detailed` or `## Core Rules` or `## Detailed Design` |
| Formulas | `## Formulas` or `## Formula` |
| Edge Cases | `## Edge Cases` |
| Dependencies | `## Dependencies` or `## Depends` |
| Tuning Knobs | `## Tuning` |
| Acceptance Criteria | `## Acceptance` |

For each GDD, record:
- Which sections are present
- Which sections are missing
- Whether it has any content in present sections or just placeholder text
  (`[To be designed]` or equivalent)

Also check: does each GDD have a `**Status**:` field in its header block?
Valid values: `In Design`, `Designed`, `In Review`, `Approved`, `Needs Revision`.

### 2b: ADR Format Audit

For each ADR file found, check for these critical sections:

| Section | Impact if missing |
|---|---|
| `## Status` | **BLOCKING** вЂ” `/story-readiness` ADR status check silently passes everything |
| `## ADR Dependencies` | HIGH вЂ” dependency ordering in `/architecture-review` breaks |
| `## Engine Compatibility` | HIGH вЂ” post-cutoff API risk is unknown |
| `## GDD Requirements Addressed` | MEDIUM вЂ” traceability matrix loses coverage |
| `## Performance Implications` | LOW вЂ” not pipeline-critical |

For each ADR, record: which sections present, which missing, current Status value
if the Status section exists.

### 2c: systems-index.md Format Audit

If `design/gdd/systems-index.md` exists:

1. **Parenthetical status values** вЂ” Grep for any Status cell containing
   parentheses: `"Needs Revision ("`, `"In Progress ("`, etc.
   These break exact-string matching in `/gate-check`, `/create-stories`,
   and `/architecture-review`. **BLOCKING.**

2. **Valid status values** вЂ” check that Status column values are only from:
   `Not Started`, `In Progress`, `In Review`, `Designed`, `Approved`, `Needs Revision`
   Flag any unrecognised values.

3. **Column structure** вЂ” check that the table has at minimum: System name,
   Layer, Priority, Status columns. Missing columns degrade skill functionality.

### 2d: Story Format Audit

For each story file found:

- **`Manifest Version:` field** вЂ” present in story header? (LOW вЂ” auto-passes if absent)
- **TR-ID reference** вЂ” does story contain `TR-[a-z]+-[0-9]+` pattern? (MEDIUM вЂ” no staleness tracking)
- **ADR reference** вЂ” does story reference at least one ADR? (check for `ADR-` pattern)
- **Status field** вЂ” present and readable?
- **Acceptance criteria** вЂ” does the story have a checkbox list (`- [ ]`)?

### 2e: Infrastructure Audit

| Artifact | Path | Impact if missing |
|---|---|---|
| TR registry | `design/architecture/tr-registry.yaml` | HIGH вЂ” no stable requirement IDs |
| Control manifest | `design/architecture/control-manifest.md` | HIGH вЂ” no layer rules for stories |
| Manifest version stamp | In manifest header: `Manifest Version:` | MEDIUM вЂ” staleness checks blind |
| Sprint status | `.ags/project/sprint-status.yaml` | MEDIUM вЂ” `/sprint-status` falls back to markdown |
| Stage file | `.ags/project/stage.txt` | MEDIUM вЂ” phase auto-detect unreliable |
| Engine reference | `.ags/docs/engine-reference/[engine]/VERSION.md` | HIGH вЂ” ADR engine checks blind |
| Architecture traceability | `design/architecture/architecture-traceability.md` | MEDIUM вЂ” no persistent matrix |

### 2f: Technical Preferences Audit

Read `.ags/rules/technical-preferences.md`. Check each field for `[TO BE CONFIGURED]`:
- Engine, Language, Rendering, Physics в†’ HIGH if unconfigured (ADR skills fail)
- Naming conventions в†’ MEDIUM
- Performance budgets в†’ MEDIUM
- Forbidden Patterns, Allowed Libraries в†’ LOW (starts empty by design)

---

## Phase 3: Classify and Prioritise Gaps

Organise every gap found across all audits into four severity tiers:

**BLOCKING** вЂ” Will cause template skills to silently produce wrong results *right now*.
Examples: ADR missing Status field, systems-index parenthetical status values,
engine not configured when ADRs exist.

**HIGH** вЂ” Will cause stories to be generated with missing safety checks, or
infrastructure bootstrapping will fail.
Examples: ADRs missing Engine Compatibility, GDDs missing Acceptance Criteria
(stories can't be generated from them), tr-registry.yaml missing.

**MEDIUM** вЂ” Degrades quality and pipeline tracking but does not break functionality.
Examples: GDDs missing Tuning Knobs or Formulas sections, stories missing TR-IDs,
sprint-status.yaml missing.

**LOW** вЂ” Retroactive improvements that are nice-to-have but not urgent.
Examples: Stories missing Manifest Version stamps, GDDs missing Open Questions section.

Count totals per tier. If zero BLOCKING and zero HIGH gaps: report that the project
is template-compatible and only advisory improvements remain.

---

## Phase 4: Build the Migration Plan

Compose a numbered, ordered action plan. Ordering rules:
1. BLOCKING gaps first (must fix before any pipeline skill runs reliably)
2. HIGH gaps next, infrastructure before GDD/ADR content (bootstrapping needs correct formats)
3. MEDIUM gaps ordered: GDD gaps before ADR gaps before story gaps (stories depend on GDDs and ADRs)
4. LOW gaps last

For each gap, produce a plan entry with:
- A clear problem statement (one sentence, no jargon)
- The exact command to fix it, if a skill handles it
- Manual steps if it requires direct editing
- A time estimate (rough: 5 min / 30 min / 1 session)
- A checkbox `- [ ]` for tracking

**Special case вЂ” systems-index parenthetical status values:**
This is always the first item if present. Show the exact values that need changing
and the exact replacement text. Offer to fix this immediately before writing the plan.

**Special case вЂ” ADRs missing Status field:**
For each affected ADR, the fix is:
`/architecture-decision retrofit design/architecture/adr-[NNNN]-[slug].md`
List each ADR as a separate checkable item.

**Special case вЂ” GDDs missing sections:**
For each affected GDD, list which sections are missing and the fix:
`/design-system retrofit design/gdd/[filename].md`

**Infrastructure bootstrap ordering** вЂ” always present in this sequence:
1. Fix ADR formats first (registry depends on reading ADR Status fields)
2. Run `/architecture-review` в†’ bootstraps `tr-registry.yaml`
3. Run `/create-control-manifest` в†’ creates manifest with version stamp
4. Run `/sprint-plan update` в†’ creates `sprint-status.yaml`
5. Run `/gate-check [phase]` в†’ writes `stage.txt` authoritatively

**Existing stories** вЂ” note explicitly:
> "Existing stories continue to work with all template skills вЂ” all new format
> checks auto-pass when the fields are absent. They won't benefit from TR-ID
> staleness tracking or manifest version checks until they're regenerated. This
> is intentional: do not regenerate stories that are already in progress."

---

## Phase 5: Present Summary and Ask to Write

Present a compact summary before writing:

```
## Adoption Audit Summary
Phase detected: [phase]
Engine: [configured / NOT CONFIGURED]
GDDs audited: [N] ([X] fully compliant, [Y] with gaps)
ADRs audited: [N] ([X] fully compliant, [Y] with gaps)
Stories audited: [N]

Gap counts:
  BLOCKING: [N] вЂ” template skills will malfunction without these fixes
  HIGH:     [N] вЂ” unsafe to run /create-stories or /story-readiness
  MEDIUM:   [N] вЂ” quality degradation
  LOW:      [N] вЂ” optional improvements

Estimated remediation: [X blocking items Г— ~Y min each = roughly Z hours]
```

Before asking to write, show a **Gap Preview**:
- List every BLOCKING gap as a one-line bullet describing the actual problem
  (e.g. `systems-index.md: 3 rows have parenthetical status values`,
  `adr-0002.md: missing ## Status section`). No counts вЂ” show the actual items.
- Show HIGH / MEDIUM / LOW as counts only (e.g. `HIGH: 4, MEDIUM: 2, LOW: 1`).

This gives the user enough context to judge scope before committing to writing the file.

If a prior adoption plan was detected in Phase 1, add a note:
> "A previous plan exists at `docs/adoption-plan-[prior-date].md`. The new plan will
> reflect current project state вЂ” it does not diff against the prior run."

Use `AskUserQuestion`:
- "Ready to write the migration plan?"
  - "Yes вЂ” write `docs/adoption-plan-[date].md`"
  - "Show me the full plan preview first (don't write yet)"
  - "Cancel вЂ” I'll handle migration manually"

If the user picks "Show me the full plan preview", output the complete plan as a
fenced markdown block. Then ask again with the same three options.

---

## Phase 6: Write the Adoption Plan

If approved, write `docs/adoption-plan-[date].md` with this structure:

```markdown
# Adoption Plan

> **Generated**: [date]
> **Project phase**: [phase]
> **Engine**: [name + version, or "Not configured"]
> **Template version**: v1.0+

Work through these steps in order. Check off each item as you complete it.
Re-run `/adopt` anytime to check remaining gaps.

---

## Step 1: Fix Blocking Gaps

[One sub-section per blocking gap with problem, fix command, time estimate, checkbox]

---

## Step 2: Fix High-Priority Gaps

[One sub-section per high gap]

---

## Step 3: Bootstrap Infrastructure

### 3a. Register existing requirements (creates tr-registry.yaml)
Run `/architecture-review` вЂ” even if ADRs already exist, this run bootstraps
the TR registry from your existing GDDs and ADRs.
**Time**: 1 session (review can be long for large codebases)
- [ ] tr-registry.yaml created

### 3b. Create control manifest
Run `/create-control-manifest`
**Time**: 30 min
- [ ] design/architecture/control-manifest.md created

### 3c. Create sprint tracking file
Run `/sprint-plan update`
**Time**: 5 min (if sprint plan already exists as markdown)
- [ ] .ags/project/sprint-status.yaml created

### 3d. Set authoritative project stage
Run `/gate-check [current-phase]`
**Time**: 5 min
- [ ] .ags/project/stage.txt written

---

## Step 4: Medium-Priority Gaps

[One sub-section per medium gap]

---

## Step 5: Optional Improvements

[One sub-section per low gap]

---

## What to Expect from Existing Stories

Existing stories continue to work with all template skills. New format checks
(TR-ID validation, manifest version staleness) auto-pass when the fields are
absent вЂ” so nothing breaks. They won't benefit from staleness tracking until
regenerated. Do not regenerate stories that are in progress or done.

---

## Re-run

Run `/adopt` again after completing Step 3 to verify all blocking and high gaps
are resolved. The new run will reflect the current state of the project.
```

---

## Phase 6b: Set Review Mode

After writing the adoption plan (or if the user cancels writing), check whether
`.ags/project/review-mode.txt` exists.

**If it exists**: Read it and note the current mode вЂ” "Review mode is already set to `[current]`." вЂ” skip the prompt.

**If it does not exist**: Use `AskUserQuestion`:

- **Prompt**: "One more setup step: how much design review would you like as you work through the workflow?"
- **Options**:
  - `Full` вЂ” Director specialists review at each key workflow step. Best for teams, learning the workflow, or when you want thorough feedback on every decision.
  - `Lean (recommended)` вЂ” Directors only at phase gate transitions (/gate-check). Skips per-skill reviews. Balanced for solo devs and small teams.
  - `Solo` вЂ” No director reviews at all. Maximum speed. Best for game jams or if reviews feel like overhead.

Write the choice to `.ags/project/review-mode.txt` immediately after selection вЂ” no separate "May I write?" needed:
- `Full` в†’ write `full`
- `Lean (recommended)` в†’ write `lean`
- `Solo` в†’ write `solo`

Create the `.ags/project/` directory if it does not exist.

---

## Phase 7: Offer First Action

After writing the plan, don't stop there. Pick the single highest-priority gap
and offer to handle it immediately using `AskUserQuestion`. Choose the first
branch that applies:

**If there are parenthetical status values in systems-index.md:**
Use `AskUserQuestion`:
- "The most urgent fix is `systems-index.md` вЂ” [N] rows have parenthetical status
  values (e.g. `Needs Revision (see notes)`) that break /gate-check,
  /create-stories, and /architecture-review right now. I can fix these in-place."
  - "Fix it now вЂ” edit systems-index.md"
  - "I'll fix it myself"
  - "Done вЂ” leave me with the plan"

**If ADRs are missing `## Status` (and no parenthetical issue):**
Use `AskUserQuestion`:
- "The most urgent fix is adding `## Status` to [N] ADR(s): [list filenames].
  Without it, /story-readiness silently passes all ADR checks. Start with
  [first affected filename]?"
  - "Yes вЂ” retrofit [first affected filename] now"
  - "Retrofit all [N] ADRs one by one"
  - "I'll handle ADRs myself"

**If GDDs are missing Acceptance Criteria (and no blocking issues above):**
Use `AskUserQuestion`:
- "The most urgent gap is missing Acceptance Criteria in [N] GDD(s):
  [list filenames]. Without them, /create-stories can't generate stories.
  Start with [highest-priority GDD filename]?"
  - "Yes вЂ” add Acceptance Criteria to [GDD filename] now"
  - "Do all [N] GDDs one by one"
  - "I'll handle GDDs myself"

**If no BLOCKING or HIGH gaps exist:**
Use `AskUserQuestion`:
- "No blocking gaps вЂ” this project is template-compatible. What next?"
  - "Walk me through the medium-priority improvements"
  - "Run /project-stage-detect for a broader health check"
  - "Done вЂ” I'll work through the plan at my own pace"

---

## Collaborative Protocol

1. **Read silently** вЂ” complete the full audit before presenting anything
2. **Show the summary first** вЂ” let the user see scope before asking to write
3. **Ask before writing** вЂ” always confirm before creating the adoption plan file
4. **Offer, don't force** вЂ” the plan is advisory; the user decides what to fix and when
5. **One action at a time** вЂ” after handing off the plan, offer one specific next step,
   not a list of six things to do simultaneously
6. **Never regenerate existing artifacts** вЂ” only fill gaps in what exists;
   do not rewrite GDDs, ADRs, or stories that already have content
