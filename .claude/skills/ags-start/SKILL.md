---
name: ags-start
description: "Entry point for every Agentic Game Studio session. First run — guided onboarding (user-interaction, engine, concept, review mode, state.md). Returning run on any phase — surfaces project context (phase, active epic, open stubs, latest decision) and resumes from where work stopped. RESUME branch handles cloned repos where state.md is gone but stage.md / epics survive. Run on first session, on missing engine/concept/state.md, on new task, or on explicit /ags-start invocation."
argument-hint: "[no arguments]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, AskUserQuestion, Task
metadata:
  author: agentic-game-studio
  version: "0.3"
---

# Agentic Game Studio — Session Entry Point

Entry point for every working session. Two modes:

- **First run (greenfield)** — guided onboarding: user-interaction → engine → concept skeleton → review mode → fresh `state.md`. Phases 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9.
- **Returning run** — three sub-paths based on what is on disk:
  - **CONTINUE** — `state.md` has unfinished work → resume in-place (after user confirms).
  - **RESUME** — `state.md` absent or clean, but project mid-flight (`stage.md` or `epics/index.md` populated) → pre-fill new `state.md` with active-epic / current-phase task, surface project context, hand off.
  - **NEW SESSION** — `state.md` absent or clean, no mid-flight signals → fresh skeleton, hand off.

All non-BLOCKED exits run **Phase 9** for a phase-aware next-step suggestion.

One active session at a time. No slugs, no archive, no pointer files. New task overwrites `.ags/project/state.md`. History in git.

Direct-write files (no extra "May I write?"):

- `.ags/project/p_user-interaction.md` — only if placeholders remain
- `design/gdd/engine.md` — from `.ags/templates/t_engine.md`
- `design/gdd/game-concept.md` — from `.ags/templates/t_concept.md` (skeleton only)
- `.ags/project/review-mode.md`
- `.ags/project/state.md` — skeleton on new session, or overwritten on reset

---

## Phase 1: Silent state detection

No output. Gather context. Each check produces a boolean signal that drives routing in Phase 2.5.

`ags-start` is designed to **handle missing artifacts itself** rather than redirect — it bootstraps user-interaction (Phase 2), engine (Phase 5), concept (Phase 7), review-mode (Phase 6), state.md (Phase 8). No STOP-with-redirect pattern. If a returning user has none of these, the skill walks the full onboarding.

Verify each:

- **User-interaction configured?** Read `.ags/project/p_user-interaction.md`. Flag `not configured` if `{{...}}` placeholders remain or file absent.
- **Engine configured?** Flag `not configured` if `design/gdd/engine.md` missing or contains `{{...}}`.
- **Concept exists?** Flag `not configured` if `design/gdd/game-concept.md` missing or contains `{{pitch}}`.
- **state.md status?** Read `.ags/project/state.md`. Record: file absent | clean (all sections empty) | unfinished (current task non-empty, or any `- [ ]`, or files in progress, or open questions).
- **Phase + active epic?** Read `.ags/project/stage.md`. Record Phase value and Active Epic value, or null.
- **Mid-flight signals?** Flag `mid-flight` if `stage.md` Phase filled OR `epics/index.md` has any rows.
- **Source code?** Glob `assets/scripts/**/*.{cs,gd,cpp,h,rs,py,js,ts}` and engine source root. Record file count.
- **Design docs?** Glob `design/**/*.md`. Record count.
- **Production artifacts?** Note presence of `epics/index.md`, active `EPIC.md`, `stubs.md`, `decisions-log.md`, `milestones/`.

Store all signals internally. Do NOT show unprompted.

---

## Phase 2: User-interaction bootstrap

If `.ags/project/p_user-interaction.md` has `{{...}}`, follow `.ags/rules/user-interaction.md`:

1. Read `.ags/templates/t_user-interaction.md`.
2. One question per template field via `AskUserQuestion`.
3. Write filled file to `.ags/project/p_user-interaction.md`. Bootstrap rule = direct directive, no separate approval.
4. From now follow created file rules (language, etc.).

No placeholders → skip to Phase 2.5.

---

## Phase 2.5: Returning state check

Three signals decide routing:

1. **state.md present + unfinished** — `## Current task` non-empty AND not `(none)` / `Done`, OR any `- [ ]`, OR `## Files in progress`/`## Open questions` non-empty.
2. **Project mid-flight** — `.ags/project/stage.md` exists with `Phase` filled, OR `.ags/project/epics/index.md` exists with at least one row.
3. **Onboarding artifacts** — engine + concept configured.

### Phase 2.6: Surface project context (always run if signal 2 true)

Before any routing, if signal 2 is true, print one compact block to user (no questions yet):

```
## Project Context

Phase: [from stage.md] | Active Epic: [from stage.md or —]
Epics: [done count] done, [in-progress count] in progress, [planned count] planned
Open stubs: [count from stubs.md, or 0]
Last decision: [most recent entry header from decisions-log.md, truncated]
```

This is the resume hook — user sees where the project actually is before the skill asks anything.

### Routing matrix

| state.md | mid-flight (signal 2) | onboarding done | Action |
|---|---|---|---|
| unfinished | any | any | **ASK** Continue / Reset / Show details |
| absent or clean | yes | yes | **RESUME**: write fresh state.md skeleton with `## Current task: resume [active epic or current phase]`, jump to Phase 9 |
| absent or clean | no | yes | **NEW SESSION**: write fresh state.md skeleton, jump to Phase 9 |
| absent or clean | any | no | **ONBOARDING**: continue to Phase 3 |

### ASK branch (state.md unfinished)

`AskUserQuestion`:

- **Prompt**: "Active state: `[Current task]` ({{N}} unchecked items, {{M}} files in progress). Continue or reset?"
- **Options**:
  - `Continue` — keep `state.md`, resume.
  - `Reset` — overwrite `state.md` fresh, new task.
  - `Show details` — print full `state.md`, re-ask.

Routing on choice:

- **Continue** — "Resuming active state from `.ags/project/state.md`." Jump to Phase 9. Verdict: **CONTINUE**.
- **Reset** — write fresh skeleton. "State reset. New session active." Jump to Phase 9. Verdict: **NEW SESSION**.
- **Show details** — print full `state.md`, re-ask.

### RESUME branch (mid-flight, state.md absent)

Common case: user cloned repo, gitignored `state.md` is gone, but `stage.md` + `epics/index.md` survive (or were tracked in git).

1. Write fresh `state.md` skeleton (Phase 8 format) with `## Current task` pre-filled as `Resume [Active Epic id or current Phase work]`.
2. Jump to Phase 9.
3. Verdict: **RESUME** — user oriented to active epic / current phase, state.md live.

---

## Phase 3: Ask where the user is

Reached only when onboarding incomplete. First visible step. `AskUserQuestion`:

- **Prompt**: "Welcome to Agentic Game Studio. Before I suggest anything — where are you with your game idea right now?"
- **Options**:
  - `A) No idea yet` — no concept. Want to explore.
  - `B) Vague idea` — rough theme, genre, or feeling. Nothing concrete.
  - `C) Clear concept` — genre + core mechanic + pitch. Docs not written.
  - `D) Existing work` — design docs, code, or planning done.

Wait for selection.

---

## Phase 4: Route based on answer

### A) No idea yet

1. Acknowledge — starting from zero fine.
2. Delegate `creative-director` via Task: ideation (vision, genre, audience, hook).
3. Show path:
   - **Concept**: creative-director → game-designer (concept doc) → engine pick (Phase 5)
   - **Foundation**: technical-director → lead-programmer → architecture skeleton + accessibility tier + control manifest seed
   - **Production**: producer (epic plan via `/ags-create-epics` — vertical slice of 1-3 systems) → game-designer + lead-programmer (GDD + ADR for epic) → specialists (impl with stubs marked `// TODO(epic-[id]):`) → `/ags-epic-retro` + `/ags-gate-check epic-done` → loop next epic

### B) Vague idea

1. Ask user to share idea — even few words.
2. Accept. No judgement.
3. Delegate `creative-director` to develop into concept.
4. Path same as (A).

### C) Clear concept

1. Ask one sentence: genre + core mechanic. Free-form, not `AskUserQuestion`.
2. `AskUserQuestion`:
   - **Prompt**: "How proceed?"
   - **Options**:
     - `Formalize first` — `creative-director` structures into concept doc.
     - `Engine first` — Phase 5 now, GDD after.
3. Show path from (A), starting at chosen step.

### D) Existing work

1. Surface Phase 1 findings: "I see [X source files / Y design docs]. Engine is [configured X / not configured]."
2. Sub-cases:
   - **D1 early** (only concept, no engine): Phase 5 → delegate `producer` for gap inventory.
   - **D2 artifacts present** (GDD/ADR/code): "Files existing ≠ skill templates can use them." Delegate `producer` + `qa-lead` for format audit + migration plan.
3. D2 path: producer (gap detect) → qa-lead (format audit) → Phase 5 (if needed) → specialists retrofit.

---

## Phase 5: Engine selection

If `design/gdd/engine.md` missing or has `{{...}}`:

1. Read `.ags/templates/t_engine.md`.
2. Studio supports Unity only. `AskUserQuestion`:
   - **Prompt**: "Confirm Unity for this project?"
   - **Options**:
     - `Yes — use Unity (Recommended)` — `unity-specialist` + `unity-dots-specialist` available.
     - `Discuss alternatives` — explain Unity-only specialist support; defer engine setup.
3. Ask Unity version (e.g. `6000.0.30f1`) free-form follow-up.
4. Write filled template to `design/gdd/engine.md`. Direct consequence — no approval.
5. Mention `unity-specialist` as engine entry point.

---

## Phase 6: Review mode

Check `.ags/project/review-mode.md`.

**Exists**: read, show "Review mode: `[current]`" → Phase 7.

**Missing**: `AskUserQuestion`:

- **Prompt**: "How much director review during work?"
- **Options**:
  - `Full` — directors review every key step. Teams, learning, thorough feedback.
  - `Lean (recommended)` — directors only at phase gates. Solo / small teams.
  - `Solo` — no director reviews. Maximum speed. Jams.

Write choice to `.ags/project/review-mode.md`: `full` / `lean` / `solo`.

---

## Phase 7: Concept skeleton (if applicable)

Route A/B/C and `design/gdd/game-concept.md` missing:

1. Read `.ags/templates/t_concept.md`.
2. Write skeleton to `design/gdd/game-concept.md` with section headers + placeholders.
3. Sections fill incrementally — `creative-director` and `game-designer` drive during session, not this skill. Per `.ags/rules/context-management.md`: one section, write on approval, compact between.

Route D — skip. Audit handles existing concept.

---

## Phase 8: Initialize state.md

Write fresh `.ags/project/state.md` skeleton:

```markdown
# Project State

## Current task
(define on first action)

## Progress checklist
- [ ]

## Files in progress

## Open questions

## Recent decisions
```

Direct consequence — no approval.

---

## Phase 9: Hand off

**Always runs** for any non-BLOCKED exit (NEW SESSION, CONTINUE, RESUME, onboarding-complete). Tailored line based on `stage.md`:

- No `stage.md` / Concept phase: "Onboarding complete. State live at `.ags/project/state.md`. Continue concept work or run `/ags-gate-check foundation` when concept artifacts ready."
- Foundation phase: "State live. Run `/ags-create-architecture`, `/ags-create-control-manifest`, `/ags-gate-check production` to advance."
- Production phase, no active epic: "State live. Run `/ags-create-epics` to plan next vertical slice."
- Production phase, active epic in progress: "State live. Active epic: `[id]`. Continue stories, run `/ags-stub-track scan` to reconcile stubs, or `/ags-gate-check epic-done` when ready to close."
- Polish phase: "State live. Run `/ags-perf-profile`, `/ags-balance-check`, or `/ags-asset-audit`. `/ags-gate-check release` to advance."
- Release phase: "State live. Run `/ags-release-checklist` or `/ags-launch-checklist`."

For RESUME and CONTINUE branches, **prepend** the Phase 2.6 Project Context block before the hand-off line so the user immediately sees where they are.

No re-explanation. No encouragement. No auto-invocation.

Verdict per branch: **COMPLETE** (onboarding) | **CONTINUE** (resumed in-progress task) | **RESUME** (mid-flight, state.md was absent) | **NEW SESSION** (state.md was clean or reset).

---

## Edge cases

- **User picks D, project empty**: redirect — "Looks like fresh template. A or B might fit?"
- **User picks A but source code exists**: surface findings — "I see code in `assets/scripts/`. Mean D?"
- **No option fits**: let user describe. Adapt.
- **User-interaction file partially filled**: ask only missing fields. Do not overwrite answered.
- **state.md missing, engine + concept set, no stage.md**: fresh skeleton → **NEW SESSION** → Phase 9.
- **state.md missing but stage.md or epics/index.md exists**: **RESUME** branch (Phase 2.5) — pre-fill state.md current task, surface project context, Phase 9.
- **state.md malformed**: show contents, offer Continue / Reset / Edit.
- **stage.md exists but no epics/index.md**: surface phase from stage.md anyway. Phase 9 hand-off picks foundation/concept guidance.
- **Multiple `EPIC.md` files have Status=implementing**: name each in context block; let user pick which to resume.

---

## Collaborative protocol

From `CLAUDE.md` — **Question → Options → Decision → Draft → Approval**:

1. **Ask first** — never assume user state or intent.
2. **Give options** — paths, not mandates.
3. **User decides** — they pick.
4. **No auto-execution** — recommend next skill or agent. Never run without explicit "yes".
5. **Adapt** — if situation does not match template, listen and adjust.
6. **Write approval** — except direct-consequence writes listed at top, every write needs "May I write to [path]?" approval.
