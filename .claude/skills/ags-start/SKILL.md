---
name: ags-start
description: "Entry point for every Agentic Game Studio session. On first run — guided onboarding (user-interaction, engine, concept, review mode). On returning runs — checks `.ags/project/state.md` and offers to continue or reset. Single active session per project: starting a new task overwrites `state.md`. Run on first session, when engine not configured, when no game concept exists, when starting a new working session, or on explicit /ags-start invocation."
argument-hint: "[no arguments]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, AskUserQuestion, Task
metadata:
  author: agentic-game-studio
  version: "0.3"
---

# Agentic Game Studio — Session Entry Point

Entry point for every working session:

- **First run** — guided onboarding (user-interaction, engine, concept, review mode), then create empty `state.md`.
- **Returning run** — read `.ags/project/state.md`. Unfinished work present → offer continue or reset (overwrite). Absent → fresh skeleton.

One active session at a time. No slugs, no archive, no pointer files. New task overwrites `.ags/project/state.md`. History in git.

Direct-write files (no extra "May I write?"):

- `.ags/project/p_user-interaction.md` — only if placeholders remain
- `design/gdd/engine.md` — from `.ags/templates/t_engine.md`
- `design/gdd/game-concept.md` — from `.ags/templates/t_concept.md` (skeleton only)
- `.ags/project/review-mode.md`
- `.ags/project/state.md` — skeleton on new session, or overwritten on reset

---

## Phase 1: Silent state detection

No output. Gather context.

Check:

- **User-interaction configured?** Read `.ags/project/p_user-interaction.md`. `{{...}}` placeholders → not configured.
- **Engine configured?** `design/gdd/engine.md` exists, no `{{...}}`.
- **Concept exists?** `design/gdd/game-concept.md` exists, no `{{pitch}}`.
- **State present?** Read `.ags/project/state.md`. Note current task, count of `- [ ]`, files in progress, open questions.
- **Source code?** Glob `assets/scripts/**/*.{cs,gd,cpp,h,rs,py,js,ts}`.
- **Design docs?** Markdown under `design/gdd/` or other `design/`.
- **Production artifacts?** `.ags/project/sprints/`, `.ags/project/milestones/`.

Store internally. Use Phase 4 to validate user self-assessment. Do NOT show unprompted.

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

Read `.ags/project/state.md`. Unfinished signals:

- `## Current task` non-empty AND not `(none)` / `Done`
- Any `- [ ]` items
- `## Files in progress` lists any file
- `## Open questions` lists any question

### No state.md OR all sections clean

- Engine + concept configured → write fresh `state.md` skeleton, hand off. Skip Phases 3–9. Verdict: **NEW SESSION**.
- Onboarding incomplete → Phase 3.

### state.md has unfinished work

`AskUserQuestion`:

- **Prompt**: "Active state: `[Current task]` ({{N}} unchecked items, {{M}} files in progress). Continue or reset?"
- **Options**:
  - `Continue` — keep `state.md`, resume.
  - `Reset` — overwrite `state.md` fresh, new task.
  - `Show details` — print full `state.md`, re-ask.

### Routing

- **Continue** — "Resuming active state from `.ags/project/state.md`." Skip Phases 3–9. Verdict: **CONTINUE**.
- **Reset** — write fresh skeleton. "State reset. New session active." Skip Phases 3–9. Verdict: **NEW SESSION**.
- **Show details** — print, re-ask.

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
   - **Design**: game-designer (GDD skeleton) → systems-designer → narrative-director / art-director / audio-director
   - **Architecture**: technical-director → lead-programmer → engine specialist
   - **Production**: producer (sprints) → specialists pick stories

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

One short line:

> "Onboarding complete. State live at `.ags/project/state.md`. Proceed with the next task."

No re-explanation. No encouragement. No auto-invocation.

Verdict: **COMPLETE** — user oriented, `state.md` live.

---

## Edge cases

- **User picks D, project empty**: redirect — "Looks like fresh template. A or B might fit?"
- **User picks A but source code exists**: surface findings — "I see code in `assets/scripts/`. Mean D?"
- **No option fits**: let user describe. Adapt.
- **User-interaction file partially filled**: ask only missing fields. Do not overwrite answered.
- **state.md missing but engine + concept set**: write fresh skeleton, **NEW SESSION**.
- **state.md malformed**: show contents, offer Continue / Reset / Edit.

---

## Collaborative protocol

From `CLAUDE.md` — **Question → Options → Decision → Draft → Approval**:

1. **Ask first** — never assume user state or intent.
2. **Give options** — paths, not mandates.
3. **User decides** — they pick.
4. **No auto-execution** — recommend next skill or agent. Never run without explicit "yes".
5. **Adapt** — if situation does not match template, listen and adjust.
6. **Write approval** — except direct-consequence writes listed at top, every write needs "May I write to [path]?" approval.
