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

**Output:** `.ags/project/epics/[slug]/EPIC.md`, updated `.ags/project/epics/index.md`, updated `.ags/project/stage.md`, append to `.ags/project/decisions-log.md`.

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

## 4. Suggest Scope Candidates

Generate 2-3 epic candidates, each with 1-3 systems, mode assignment, and rationale. Sources for candidates:

- **Next-in-dependency-order** — first systems-index entry not yet covered by `new` mode in any past epic.
- **Stub-closure** — systems in `Open Stubs` whose owner-epic is TBD or matches next slot.
- **Revise** — systems flagged in any prior epic's Retrospective as needing rework.

Present candidates as numbered list:

```
1. epic-[NNN]-[slug-A]
   Systems: [system-a:new, system-b:stub]
   Rationale: [one sentence]

2. epic-[NNN]-[slug-B]
   Systems: [system-c:revise, system-d:new]
   Rationale: [one sentence]
```

Ask: "Pick a candidate, or describe your own."

---

## 5. User Defines Epic

If user picks a candidate, confirm and proceed. If user describes own epic, ask:

1. Which 1-3 systems are in scope? (validate against systems-index)
2. Mode per system? (new / revise / stub)
3. One-sentence epic name + rationale (2-3 sentences why now, what risk it burns down, what playable state it produces)

---

## 6. Validate

Per system in scope:

- **`new`** — verify system not already implemented (not in any past EPIC.md as `new` with Status=done).
- **`revise`** — read GDD; warn user about consumer systems likely affected. Ask user to list affected consumers in Existing System Changes.
- **`stub`** — verify system exists in systems-index. If not, fail with reason.

If validation fails, surface to user and let them adjust.

---

## 7. Compute IDs

- **Epic ID**: `epic-[NNN]-[slug]`. NNN = max existing + 1, zero-padded to 3 digits.
- **Slug**: from `[name]` arg, or derived from first `new` system, or user-confirmed.
- **Created**: today (YYYY-MM-DD).

Confirm IDs with user before writing.

---

## 8. Combined Review Loop (Producer Gate PR-EPIC + External Codex, parallel)

Canonical contract: `.ags/rules/review-workflow.md`. Aggregator: `producer`.

**Each iteration N (start N=1):**

1. Resolve severity floor: N≤2 → `low`; N=3..4 → `high`; N≥5 → `critical`.
2. Persist epic plan summary to `.ags/project/reviews/.tmp/epic-[slug]-iter[N]-draft.md`.
3. Spawn in parallel (single message, multiple Task calls):
   - `producer` via Task with gate **PR-EPIC** (`.ags/rules/director-gates.md`). Pass: epic name, systems with modes, rationale, current epic count, open stubs count, iteration N, severity floor.
   - `/ags-external-review epic [draft-path] --embedded-parallel --iteration [N] --min-severity [floor]`. Codex unavailable → returns `skipped: codex-unavailable`; producer aggregator logs skip in decisions-log and continues with internal pool only.
4. **Aggregator (producer)** collects findings from both reviewers; drops nitpicks + below-floor per `.ags/rules/review-workflow.md`.
5. **Loop exit**: filtered set is empty → proceed to Phase 9. Non-empty → surface aggregated kept findings to user, user revises epic scope/rationale/systems-in-scope, N++, repeat.

No iteration cap. Record final iteration count for the decisions-log entry.

---

## 9. Approval

Ask: "May I write:
- `.ags/project/epics/[slug]/EPIC.md` (from `.ags/templates/t_epic.md`)
- `.ags/project/epics/[slug]/stories/` (empty folder)
- update `.ags/project/epics/index.md`
- set active epic in `.ags/project/stage.md`
- append entry to `.ags/project/decisions-log.md`?"

If declined, stop. Verdict: **BLOCKED — user declined write**.

---

## 10. Write Files

### 10a. EPIC.md

Read `.ags/templates/t_epic.md`. Write to `.ags/project/epics/[slug]/EPIC.md` with substitutions:

- Title: `# Epic: [name]`
- Metadata: ID, Status=`planned`, Created=today, Closed=—
- Rationale: user-provided
- Systems in Scope: filled table with mode per system, GDD links
- Existing System Changes: filled if any `revise`; otherwise leave placeholder
- Other sections remain as template placeholders (filled by downstream skills)

### 10b. Stories folder

Create `.ags/project/epics/[slug]/stories/` (write a single `.gitkeep` empty file to ensure directory exists).

### 10c. epics/index.md

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

### 10d. stage.md

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

### 10e. decisions-log.md

If file does not exist, copy from `.ags/templates/t_decisions-log.md` first.

Append entry:

```
## [YYYY-MM-DD HH:MM] — Create epic-[NNN]-[slug]

**Type**: scope
**Context**: New vertical slice planned.
**Decision**: Epic [name] covers [system list with modes].
**Rationale**: [user-provided rationale, condensed]
**Refs**: .ags/project/epics/[slug]/EPIC.md
**Decided by**: human
```

Verdict: **COMPLETE — epic created**.

---

## 11. Next Steps

Suggest in order:

1. `/ags-epic-contracts [slug]` — required if any system is `stub` mode. Locks contracts and pre-registers stubs.
2. `/ags-design-system` — author or extend GDD sections for `new` and `revise` systems.
3. `/ags-architecture-decision` — add ADRs for this epic's architectural choices (required for `revise` epics that change architecture).
4. `/ux-design` — only if epic has UI/UX work.
5. `/ags-create-stories [slug]` — break epic into implementable stories once design + ADRs are stable.

---

## Rules

- One epic per skill invocation.
- 1-3 systems per epic. More = scope too big, split into multiple epics.
- All-`new`, all-`revise`, all-`stub`, or any mix is allowed.
- Epic name + rationale come from the user — skill never invents them.
- File writes are atomic per phase: EPIC.md + index.md + stage.md + decisions-log.md all written or none.
- `epics/index.md` is the source of truth for epic count and status. `stage.md` points at the active one.
