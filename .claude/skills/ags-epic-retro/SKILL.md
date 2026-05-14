---
name: ags-epic-retro
description: "Run a retrospective for the closing epic. Combines standard questions with scope-dependent ones derived from epic content. Writes Retrospective section in EPIC.md and appends to decisions-log.md."
argument-hint: "[epic-slug]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, AskUserQuestion
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `.ags/project/stage.md` (Phase = production) | `/ags-gate-check production` | STOP. "Not in production phase." |
| Active `.ags/project/epics/[slug]/EPIC.md` | `/ags-create-epics` | STOP. "No active epic. Run `/ags-create-epics` first." |
| `.ags/project/decisions-log.md` | this skill or other writers | Auto-create from `t_decisions-log.md` if missing — not a STOP. |
| At least one playtest report under `.ags/project/playtests/epic-[slug]-*.md` | `/ags-playtest-report` | WARN (not STOP) — retro can run, but flag missing in signal summary. |

If any STOP triggers, exit with verdict **BLOCKED — missing prerequisite**.

---

## Phase 1: Resolve Active Epic

If `[epic-slug]` provided, use it. Otherwise read `.ags/project/stage.md` for active epic.

Read `.ags/project/epics/[slug]/EPIC.md`. Status should be `playtesting` or `implementing` (entering retro before gate). If Status is already `done`, use `AskUserQuestion` with prompt "Epic already done — retro will overwrite existing entry. Continue?" options: `Yes — overwrite` / `Cancel`.

If epic file missing, stop. Verdict: **FAIL — no active epic. Run `/ags-create-epics` or specify [epic-slug] argument.**

---

## Phase 2: Gather Signals

Auto-extract from artifacts:

1. **Acceptance status** — count checked vs unchecked items in `## Acceptance Criteria`.
2. **Stories** — read `stories/` subfolder. Count: completed, in progress, carried-over.
3. **Stubs** — read `.ags/project/stubs.md`. Count: introduced by this epic, closed by this epic, migrated.
4. **Bugs** — read `.ags/project/bugs/` for reports tagged with epic ID. Count by severity.
5. **Playtests** — list linked playtest reports under `.ags/project/playtests/epic-[slug]-*.md`.
6. **Scope delta** — compare current `Systems in Scope` table to original Rationale. Note any system added or removed mid-epic.
7. **Modes used** — count systems by `new` / `revise` / `stub`.

Present this signal summary to the user before asking questions.

---

## Phase 3: Standard Questions

Ask the user, one at a time:

1. **What worked well in this epic?**
2. **What should change in the next epic?**
3. **Was there scope creep? How was it handled?**
4. **What surprises (technical or design) came up?**
5. **What follow-up epics does this epic suggest?**

---

## Phase 4: Scope-Dependent Questions

Generate additional questions based on signals from Phase 2. Skip a category if not applicable.

- **If `revise` mode used** — ask: "Did the revision break or destabilize any consumer system? How well did the change propagate?"
- **If new stubs introduced** — ask: "Are stub interfaces stable enough to survive multiple consuming epics? Any signs of contract drift?"
- **If stubs were migrated** — ask: "What drove migration? Is the new owner-epic realistic?"
- **If carried-over stories > 0** — ask: "Why did stories carry over? Estimation, dependency, or scope?"
- **If S1/S2 bugs > 0 during epic** — ask: "Were any bugs caused by stub-default behavior? Any mitigations needed in `t_epic.md` template?"
- **If playtests revealed UX issues** — ask: "Did UX specs catch these issues, or did they only surface in play? Should `/ux-design` happen earlier?"
- **If scope delta non-zero** — ask: "Why did scope shift? Was the original epic too ambitious, or was the discovery valuable?"
- **If multiple `new` systems in one epic** — ask: "Was the multi-system slice manageable, or should future epics narrow to fewer systems?"
- **If architecture-review or cross-gdd-review surfaced issues** — ask: "What did the review catch that we missed during design? How to surface earlier?"

---

## Phase 5: Draft Retrospective

Compose the `## Retrospective` section in `EPIC.md`:

```markdown
## Retrospective

### Signal Summary
[counts from Phase 2]

### What Worked
[Q1 answer]

### What to Change
[Q2 answer]

### Scope Creep
[Q3 answer]

### Surprises
[Q4 answer]

### Follow-up Epic Candidates
[Q5 answer]

### Scope-Specific Notes
[Phase 4 Q&A pairs, only those asked]
```

Show the draft to the user.

---

## Phase 6: Approval

Use `AskUserQuestion` with prompt "May I write retrospective to `EPIC.md` and append to `decisions-log.md`?" and options:
- `Yes — write both files`
- `Edit retro first` — show draft, accept user edits, re-confirm
- `Cancel`

If declined, stop. Verdict: **BLOCKED — user declined write**.

---

## Phase 7: Write

1. Edit `.ags/project/epics/[slug]/EPIC.md` — replace `## Retrospective` placeholder with drafted content.
2. Append to `.ags/project/decisions-log.md`:

```
## [YYYY-MM-DD HH:MM] — Retro for epic-[id] [name]

**Type**: process
**Context**: Epic closing. Retrospective complete.
**Decision**: [one-sentence summary of biggest takeaway from "What to Change"]
**Rationale**: [optional context]
**Refs**: .ags/project/epics/[slug]/EPIC.md
**Decided by**: human
```

3. If user named follow-up epic candidates in Q5, list them in `.ags/project/epics/index.md` under a `## Backlog` section (create if missing). One line per candidate: name + one-sentence rationale.

Verdict: **COMPLETE — retro written**.

---

## Phase 8: Next Steps

Suggest:

- `/ags-gate-check epic-done` — final gate to close the epic.
- `/ags-create-epics` for next iteration if MVP not yet feature-complete.

---

## Rules

- Retro is **required before** `/ags-gate-check epic-done`.
- Never edit past retros — supersede with a new entry referencing the old.
- Standard 5 questions are always asked. Scope-dependent ones are added based on actual epic content.
