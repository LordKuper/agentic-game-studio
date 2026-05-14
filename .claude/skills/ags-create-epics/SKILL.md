---
name: ags-create-epics
description: "Create one vertical-slice epic covering 1-3 systems (modes: new / revise / stub). Writes EPIC.md from t_epic.md, updates epics/index.md, sets active epic in stage.md."
argument-hint: "[name]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task, AskUserQuestion
agent: producer
---

# Create Epic (Vertical Slice)

Epic = 1-3 systems designed and implemented together with TODO stubs for unscoped neighbors. Each system is in mode:

- `new` — implemented for the first time
- `revise` — existing system extended, refactored, or rewired
- `stub` — interface only, real impl deferred to a future epic

**Output:** `.ags/project/epics/[slug]/scope.html`, `.ags/project/epics/[slug]/EPIC.md`, updated `.ags/project/epics/index.md`, updated `.ags/project/stage.md`, append to `.ags/project/decisions-log.md`.

**Next step after epic creation:** `/ags-epic-contracts [slug]` (if stubs), then `/ags-design-system`, `/ags-architecture-decision`, `/ags-create-stories [slug]`.

---

## 0. Prerequisites

Verify required artifacts exist with meaningful content. STOP on first missing item with redirect.

| Artifact | Created by | If missing |
|---|---|---|
| `design/gdd/game-concept.md` (no `{{...}}` placeholders) | `/ags-brainstorm` | STOP. "No game concept. Run `/ags-brainstorm` first." |
| `design/gdd/systems-index.md` | `/ags-map-systems` | STOP. "No systems map. Run `/ags-map-systems` first — epics need a system catalog." |
| `design/architecture/architecture.md` | `/ags-create-architecture` | STOP. "Architecture skeleton missing. Run `/ags-create-architecture` (Foundation phase)." |
| `design/architecture/control-manifest.md` | `/ags-create-control-manifest seed` | STOP. "Control manifest missing. Run `/ags-create-control-manifest seed`." |
| `design/accessibility-requirements.md` | `/ags-gate-check foundation` flow or manual | STOP. "Accessibility tier not committed. Run `/ags-gate-check foundation` to bootstrap." |
| `.ags/project/stage.md` Phase = `production` | `/ags-gate-check production` | WARN (not STOP). Use `AskUserQuestion` to confirm continue: phases other than production are unusual for epic creation. |

If any STOP triggers, exit with verdict **BLOCKED — missing prerequisite** and surface the redirect command. Do not write anything.

---

## 1. Parse Arguments

- `[name]` — optional slug hint. If omitted, derive slug from first `new` system or ask user.

---

## 2. Phase Check

Read `.ags/project/stage.md`. If phase is not `production`, warn:

> "Current phase is [X]. Epic creation is intended for production phase. Continue anyway?"

If user declines, stop. Verdict: **BLOCKED — wrong phase**.

---

## 3. Load Context

Read in parallel:

- `design/gdd/systems-index.md` — available systems with status and dependencies
- `.ags/project/epics/index.md` — existing epics (compute next ID = max + 1; create file if missing)
- `.ags/project/stubs.md` — Open Stubs (candidates for closure in revise/new epic)
- `.ags/rules/technical-preferences.md` — engine context

Report: "Available systems: [N]. Existing epics: [M]. Open stubs: [K]."

---

## 4. Free-Form Description

Ask the user, in their chosen chat language (per `user-interaction.md`):

> "Describe what this epic should do. Free form — new features, mechanic changes, data specs, refactor, fix, anything. Don't worry about format yet."

Capture the response verbatim into working memory (not a file yet). No structuring, no questions yet.

---

## 5. Scope Clarification

Goal: turn the free-form description into a clear, formalized scope statement that the user explicitly approves before anything is written to disk.

Stay above implementation details — describe **what the epic delivers and why**, not how systems are coded, what data fields exist, or which algorithms are used.

1. Re-read the free-form description. Identify ambiguities, missing acceptance signals, scope boundaries.
2. Draft a formalized scope statement (in user's chat language) covering:
   - **Goal** — 2-4 sentences on what the epic delivers and why now.
   - **In scope** — bullet list of user-facing outcomes / capabilities.
   - **Not in scope** (only if helpful for disambiguation) — what the epic explicitly does *not* deliver.
3. List clarification questions for any ambiguity. Ask ONE question per ambiguity, batched in a single turn via `AskUserQuestion`. Do not invent answers.
4. Iterate: refine the formalized statement based on answers, present updated draft, ask "Approved?" Repeat until user explicitly approves.
5. **Output** of this step: an approved formalized scope statement held in working memory. This becomes the Goal block of `scope.html` (translated to English at write time per `.ags/rules/user-interaction.md`).

Hard rules for this step:
- No implementation details (no field names, no algorithms, no code structure).
- No system list yet — that comes in step 6 from the approved scope.
- Nothing written to disk before user approval.

---

## 6. Affected Systems Analysis

With the approved scope statement in hand, derive the affected-systems list from `design/gdd/systems-index.md`. Classify each touched system:

| Action | Meaning |
|--------|---------|
| **Create** | System does not exist in `systems-index.md`. Will be added. |
| **Modify** | System exists. GDD and/or code changes. |
| **Delete** | System exists. Removed by this epic (with migration). |
| **Touch** | System read-only: epic calls existing API, no GDD or code change. |

**Scope count** = `Create + Modify + Delete`. **Touch is not counted.**

Present the matrix to user (System | Action | Reason | GDD link | Risk low/med/high) and ask for confirmation or adjustment.

---

## 7. Soft Scope Limit

If `Scope count > 3`:

1. Warn the user: "Scope count is N — exceeds soft limit of 3. Recommended actions: (a) cut systems out of scope, (b) split into 2-3 sequential epics, (c) override and proceed."
2. If (a) cut — drop systems from the matrix, return to step 5 to revise the formalized scope statement.
3. If (b) split — produce a proposed split with ordering and dependencies between sub-epics. Stop this skill invocation; user re-runs `/ags-create-epics` per sub-epic.
4. If (c) override — capture the override reason and record an entry in `.ags/project/decisions-log.md` (type=`scope-override`, includes scope count and reason). Proceed.

This gate is soft: user can always override, but the override is logged.

---

## 8. Validate

Per system in the matrix:

- **Create** — verify the system is not already in `systems-index.md` and not declared `new` in any past EPIC.md with Status=done.
- **Modify** — verify the system exists in `systems-index.md`. Warn the user about consumer systems likely affected (from systems-index dependencies); the affected consumers should be either in the matrix as `Touch`/`Modify` or explicitly out of scope.
- **Delete** — verify the system exists. Require a migration note (covered by Reason column).
- **Touch** — verify the system exists.

If validation fails, surface to user and let them adjust.

---

## 9. Compute IDs

- **Epic ID**: `epic-[NNN]-[slug]`. NNN = max existing + 1, zero-padded to 3 digits.
- **Slug**: from `[name]` arg, or derived from first `Create` system, or user-confirmed.
- **Created**: today (YYYY-MM-DD).

Confirm IDs with user before writing.

---

## 10. Draft scope.html (Live Preview)

Write the scope file to its real path **immediately**, so the user can review the live rendered HTML in a browser. No `.tmp/` staging — `scope.html` is the working artifact for review and iteration.

Path: `.ags/project/epics/[slug]/scope.html`

Source: `.ags/templates/t_epic-scope.html`. Substitutions:

- `{{epic-id}}`, `{{epic-title}}`, `{{slug}}`, `{{owner}}`, `{{YYYY-MM-DD}}` — header values; Status = `draft`
- **Goal block** — formalized scope statement from step 5 (translated to English at write time per `.ags/rules/user-interaction.md`)
- **Affected Systems table** — one `<tr>` per matrix entry; pick correct badge class (`b-create` / `b-modify` / `b-delete` / `b-touch`) and risk class
- **Scope count** sentence — fill `{{count}}`; when override applied, append "Override recorded in decisions-log on {{date}}."
- **Component diagram** — inline SVG. Author chooses the layout that best communicates the epic's component relations for this specific case. Hard rules only:
  1. Each affected system = labeled `<rect>` node; node color follows Action (green=Create, yellow=Modify, red=Delete, gray dashed=Touch).
  2. Edges = directed `<line>` with `marker-end="url(#arr)"`; read-only edges use `stroke-dasharray="4,3"`.
  3. Edge labels short (≤20 chars); drop labels when no relation is meaningful.
  4. Diagram fits inside the template's `viewBox` or a reasonably sized replacement.
- **Dependencies** list — fill from working memory; use literal "none" when empty
- **Acceptance Criteria** — user-facing outcomes from step 5 + always-include lines for doc-comments/unit tests and no-regressions
- **Open Questions** — any ambiguity left after step 5 (rare; should mostly be resolved by approval)

Validate the resulting HTML opens in a browser (no JS errors expected; SVG renders inline).

After write, surface the file path to the user: "scope.html written. Open in browser to review the live render. Iterate by telling me what to change."

---

## 11. Combined Review Loop (Producer Gate PR-EPIC + External Codex, parallel)

Canonical contract: `.ags/rules/review-workflow.md`. Aggregator: `producer`. The reviewed artifact is the live `scope.html` written in step 10 — iterations edit that file in place.

**Each iteration N (start N=1):**

1. Resolve severity floor: N≤2 → `low`; N=3..4 → `high`; N≥5 → `critical`.
2. Spawn in parallel (single message, multiple Task calls):
   - `producer` via Task with gate **PR-EPIC** (`.ags/rules/director-gates.md`). Pass: path to live `scope.html`, scope count, override flag, current epic count, open stubs count, iteration N, severity floor.
   - `/ags-external-review epic [path-to-scope.html] --embedded-parallel --iteration [N] --min-severity [floor]`. Codex unavailable → returns `skipped: codex-unavailable`; producer aggregator logs skip in decisions-log and continues with internal pool only.
3. **Aggregator (producer)** collects findings from both reviewers; drops nitpicks + below-floor per `.ags/rules/review-workflow.md`.
4. **Loop exit**: filtered set is empty AND user has approved the live file → proceed to Phase 12. Otherwise: surface aggregated kept findings + any user-requested changes, **edit `scope.html` in place** (Edit tool, not rewrite), N++, repeat.

No iteration cap. Record final iteration count for the decisions-log entry.

User may approve at any iteration even if reviewers still have findings — those are logged as accepted-with-known-issues in `decisions-log.md`.

---

## 12. Final Approval (remaining files)

`scope.html` is already on disk. Ask:

> "scope.html is approved and live. May I now write:
> - `.ags/project/epics/[slug]/EPIC.md` (from `.ags/templates/t_epic.md`)
> - `.ags/project/epics/[slug]/stories/` (empty folder)
> - update `.ags/project/epics/index.md`
> - set active epic in `.ags/project/stage.md`
> - append entry to `.ags/project/decisions-log.md`
> - flip Status in scope.html from `draft` to `planned`?"

If declined, stop. Verdict: **BLOCKED — user declined write**. `scope.html` stays on disk in `draft` status; user can re-run the skill to resume.

---

## 13. Write Remaining Files

### 13a. scope.html status flip

Edit the existing `scope.html`: change Status `draft` → `planned`. No other edits.

### 13b. EPIC.md

Read `.ags/templates/t_epic.md`. Write to `.ags/project/epics/[slug]/EPIC.md` with substitutions:

- Title: `# Epic: [name]`
- Metadata: ID, Status=`planned`, Created=today, Closed=—
- Scope section: keep the link to `./scope.html` from the template (no field duplication — scope.html is the source of truth for goal, affected systems, acceptance criteria)
- Other sections (Contracts, Stories, Stubs, Playtest, Retrospective, Gate Verdict) remain as template placeholders, filled by downstream skills

### 13c. Stories folder

Create `.ags/project/epics/[slug]/stories/` (write a single `.gitkeep` empty file to ensure directory exists).

### 13d. epics/index.md

If file does not exist, create with header:

```markdown
# Epics Index

Registry of all epics. Status values: planned, designing, implementing, playtesting, done, rolled-back.

| ID | Name | Systems | Modes | Status | Created | Closed |
|----|------|---------|-------|--------|---------|--------|

## Backlog

[Follow-up epic candidates surfaced by retros. One line each.]
```

Append row for new epic to the table.

### 13e. stage.md

If file does not exist, create with skeleton:

```markdown
# Stage

| Field | Value |
|-------|-------|
| Phase | production |
| Active Epic | epic-[NNN]-[slug] |
| Updated | YYYY-MM-DD HH:MM |

## Transition History

| Date | Phase | Active Epic | Note |
|------|-------|-------------|------|
| YYYY-MM-DD HH:MM | production | epic-[NNN]-[slug] | Created |
```

If exists, edit: update Active Epic, Updated; append row to Transition History.

### 13f. decisions-log.md

If file does not exist, copy from `.ags/templates/t_decisions-log.md` first.

Append entry:

```
## [YYYY-MM-DD HH:MM] — Create epic-[NNN]-[slug]

**Type**: scope
**Context**: New vertical slice planned.
**Decision**: Epic [name] — scope statement + affected systems matrix (Create/Modify/Delete/Touch) authored in scope.html.
**Rationale**: [condensed approved Goal text]
**Refs**: .ags/project/epics/[slug]/scope.html, .ags/project/epics/[slug]/EPIC.md
**Decided by**: human
```

If a soft-limit override (step 7c) was applied, append a second entry of `Type: scope-override` with scope count, override reason, and link to `scope.html`.

Verdict: **COMPLETE — epic created**.

---

## 14. Next Steps

Suggest in order:

1. `/ags-epic-contracts [slug]` — required if any consumer/neighbor system needs a stub interface. Locks contracts and pre-registers stubs.
2. `/ags-design-system` — author or extend GDD sections for `Create` and `Modify` systems.
3. `/ags-architecture-decision` — add ADRs for this epic's architectural choices (required for any `Modify` that changes architecture).
4. `/ux-design` — only if epic has UI/UX work.
5. `/ags-create-stories [slug]` — break epic into implementable stories once design + ADRs are stable.

---

## Rules

- One epic per skill invocation.
- Soft limit: scope count (Create + Modify + Delete) ≤ 3. Touch is unbounded.
- Mix of Create / Modify / Delete is allowed.
- Free-form description and formalized scope statement come from the user — skill never invents intent. Skill may **propose** a formalized rewording for approval; it does not commit it without explicit user approval.
- Scope discussion stays above implementation details (no field names, no algorithms, no code structure).
- `scope.html` is written **first**, at its real path, in `draft` status, so the user can review the live render in a browser. It is then iterated in place via Edit through the review loop. The remaining files (EPIC.md, index.md, stage.md, decisions-log.md) are written atomically at final approval, and at the same time `scope.html` is flipped to `planned` status.
- If the user declines final approval, `scope.html` stays on disk in `draft` status. Re-running the skill resumes from that draft.
- `scope.html` is the source of truth for goal, affected systems, acceptance criteria. EPIC.md never duplicates those fields.
- `epics/index.md` is the source of truth for epic count and status. `stage.md` points at the active one.
