---
name: ags-start
description: "Entry point for every Agentic Game Studio session. On first run — guided onboarding (user-interaction, engine, concept, review mode). On returning runs — scans `.ags/project/sessions/*.md` for unfinished sessions and offers to continue an existing one or start a new one. Run on first session, when engine not configured, when no game concept exists, when starting a new working session, or on explicit /ags-start invocation. Detects project state, asks where the user is, routes to the right agent or skill. No assumptions."
argument-hint: "[no arguments]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, AskUserQuestion, Task
metadata:
  author: agentic-game-studio
  version: "0.2"
---

# Agentic Game Studio — Session Entry Point

Skill is the entry point for **every working session**, not only first onboarding:

- **First run** — guided onboarding (user-interaction, engine, concept, review mode), then hand off to `/ags-session` to start the first working session.
- **Returning run** — scan `.ags/project/sessions/*.md` for unfinished sessions, offer continue or start fresh, hand off to `/ags-session`.

Files this skill writes (direct consequences of user choices — no extra "May I write?" needed):

- `.ags/project/p_user-interaction.md` — only if placeholders remain
- `design/gdd/engine.md` — from `.ags/templates/t_engine.md`
- `design/gdd/concept.md` — from `.ags/templates/t_concept.md` (skeleton only; sections filled later by other skills/agents)
- `.ags/project/review-mode.md`

Session files (`.ags/project/sessions/{name}.md`) are owned by `/ags-session`, NOT by this skill.

Does NOT assume idea, engine, or experience. Asks first, routes second.

---

## Phase 1: Silent state detection

No output yet. Gather context to calibrate later recommendations.

Check:

- **User-interaction configured?** Read `.ags/project/p_user-interaction.md`. If `{{...}}` placeholders remain — not configured.
- **Engine configured?** File `design/gdd/engine.md` exists and `{{...}}` placeholders are gone.
- **Concept exists?** File `design/gdd/concept.md` exists and `{{pitch}}` placeholder is gone.
- **Sessions present?** Glob `.ags/project/sessions/*.md`. List file names + `## Current task` + count of unchecked items per file.
- **Source code?** Glob `assets/scripts/**/*.{cs,gd,cpp,h,rs,py,js,ts}`.
- **Design docs?** Markdown files under `design/gdd/` or other `design/` subdirs.
- **Production artifacts?** Files under `production/sprints/` or `production/milestones/`.

Store findings internally. Use them in Phase 4 to validate the user's self-assessment and tailor advice. Do NOT show findings unprompted.

---

## Phase 2: User-interaction bootstrap

If `.ags/project/p_user-interaction.md` still contains `{{...}}` placeholders, follow `.ags/rules/user-interaction.md`:

1. Read `.ags/templates/t_user-interaction.md`.
2. One question per template field via `AskUserQuestion`.
3. Write filled file to `.ags/project/p_user-interaction.md`. No separate "May I write?" — bootstrap rule is a direct directive.
4. From this point, follow rules from the created file (communication language, etc.) for the rest of the session.

If no placeholders — skip to Phase 2.5.

---

## Phase 2.5: Returning session check

Glob `.ags/project/sessions/*.md`. For each file detect **unfinished signals**:

- `## Current task` is non-empty AND not literally "(none)" / "Done"
- `## Progress checklist` has any `- [ ]` items
- `## Files in progress` lists any file
- `## Open questions` lists any question

Branch by count of unfinished sessions:

### 0 unfinished

If no `sessions/` directory, no session files, or all sessions complete:

- Engine + concept already configured → hand off via `/ags-session` (no parameter) to start a fresh session. Skip Phases 3–9. Verdict: **NEW SESSION**.
- Onboarding incomplete (no engine / no concept) → proceed to Phase 3.

### 1 unfinished

Use `AskUserQuestion`:

- **Prompt**: "Unfinished session `[name]`: `[Current task]` ({{N}} unchecked items, {{M}} files in progress). Continue or start a new session?"
- **Options**:
  - `Continue [name]` — resume that session.
  - `New session` — leave existing session as-is, start a new one.
  - `Show details` — print full session file, then re-ask.

### 2+ unfinished

Use `AskUserQuestion`:

- **Prompt**: "{{N}} unfinished sessions. Pick one to continue or start a new session."
- **Options**:
  - One option per session: `Continue [name] — [Current task] ({{N}} pending)`.
  - `New session` — start a fresh one.
  - `Show details` — print session list with full bodies, then re-ask.

### Routing per choice

- **Continue [name]** — hand off via `/ags-session [name]`. One-liner: "Resume `[name]` via `/ags-session [name]`." Skip Phases 3–9. Verdict: **CONTINUE**.
- **New session** — hand off via `/ags-session` (no parameter). One-liner: "Start fresh via `/ags-session`." Skip Phases 3–9. Verdict: **NEW SESSION**.
- **Show details** — print and re-ask the same question.

---

## Phase 3: Ask where the user is

Reached only when onboarding is incomplete (no engine and/or no concept). First visible step. Use `AskUserQuestion`:

- **Prompt**: "Welcome to Agentic Game Studio. Before I suggest anything — where are you with your game idea right now?"
- **Options**:
  - `A) No idea yet` — no concept at all. Want to explore.
  - `B) Vague idea` — rough theme, genre, or feeling ("something with space", "cozy farming"). Nothing concrete.
  - `C) Clear concept` — know the genre + core mechanic + pitch sentence. Documents not written yet.
  - `D) Existing work` — design docs, prototypes, code, or planning already done. Want to organize or continue.

Wait for selection. Do not proceed without it.

---

## Phase 4: Route based on answer

### A) No idea yet

1. Acknowledge — starting from zero is fine.
2. Delegate `creative-director` via Task: ideation session (vision, genre, audience, hook).
3. Show the path:
   - **Concept phase**: creative-director (idea) → game-designer (concept doc) → engine pick (Phase 5)
   - **Design phase**: game-designer (GDD skeleton) → systems-designer (mechanics) → narrative-director / art-director / audio-director (as needed)
   - **Architecture phase**: technical-director → lead-programmer → engine specialist
   - **Prototype phase**: gameplay-programmer
   - **Production phase**: producer (sprints) → specialists pick up stories

### B) Vague idea

1. Ask the user to share the idea — even a few words.
2. Accept as starting point. No judgement.
3. Delegate `creative-director` to develop the idea into a concept.
4. Path same as (A).

### C) Clear concept

1. Ask for one sentence: genre + core mechanic. Free-form input, not `AskUserQuestion`.
2. Use `AskUserQuestion`:
   - **Prompt**: "How do you want to proceed?"
   - **Options**:
     - `Formalize first` — `creative-director` structures it into a concept document.
     - `Engine first` — go to Phase 5 now, write GDD afterward.
3. Show the path from (A), starting at the chosen step.

### D) Existing work

1. Surface findings from Phase 1: "I see [X source files / Y design docs / Z prototypes]. Engine is [configured as X / not configured]."
2. Sub-cases:
   - **D1 early stage** (only concept exists, engine not picked): Phase 5 → delegate `producer` for gap inventory.
   - **D2 artifacts present** (GDD / ADR / code): "Files existing ≠ skill templates can use them." Delegate `producer` + `qa-lead` for format audit + migration plan.
3. Path for D2:
   - producer (gap detect) → qa-lead (format audit) → Phase 5 (if needed) → specialists retrofit missing sections.

---

## Phase 5: Engine selection

If `design/gdd/engine.md` does not exist or still has `{{...}}` placeholders, run engine pick:

1. Read template `.ags/templates/t_engine.md`.
2. Use `AskUserQuestion`:
   - **Prompt**: "Which engine?"
   - **Options**:
     - `Unity` — C#, broad ecosystem. `unity-specialist` + `unity-dots-specialist` available.
     - `Godot` — GDScript / C#, open source, lightweight.
     - `Unreal` — C++ / Blueprints, AAA visuals.
     - `Other` — free-form, user-supplied.
3. For version + rationale: free-form follow-up questions.
4. Write filled template to `design/gdd/engine.md`. Direct consequence of selection — no separate approval needed.
5. If Unity — mention `unity-specialist` as the engine entry point for later phases.

---

## Phase 6: Review mode

Check `.ags/project/review-mode.md`.

**Exists**: read, show "Review mode: `[current]`" → Phase 7.

**Missing**: use `AskUserQuestion`:

- **Prompt**: "How much director review do you want as you work?"
- **Options**:
  - `Full` — directors review at every key step. Best for teams, learning the workflow, thorough feedback.
  - `Lean (recommended)` — directors only at phase gate transitions. Solo / small teams.
  - `Solo` — no director reviews. Maximum speed. Jams, prototypes.

Write choice to `.ags/project/review-mode.md` immediately: `full` / `lean` / `solo`.

---

## Phase 7: Concept skeleton (if applicable)

If route is A / B / C and `design/gdd/concept.md` does not exist:

1. Read `.ags/templates/t_concept.md`.
2. Write skeleton to `design/gdd/concept.md` with all section headers + placeholders intact.
3. Sections fill incrementally — `creative-director` and `game-designer` drive that during the working session, not this skill. Per `.ags/rules/context-management.md`: one section at a time, write on approval, compact in between.

If route is D — skip. Audit handles existing concept.

---

## Phase 8: Confirm next step

After showing the path, use `AskUserQuestion`:

- **Prompt**: "Start the first working session via `/ags-session`?"
- **Options**:
  - `Yes`
  - `Different step` (let them name it)

Never auto-run the next skill or agent.

---

## Phase 9: Hand off

After onboarding, the first working session begins via `ags-session`. One short line:

> "Onboarding complete. Start your first session via `/ags-session`."

No re-explanation. No encouragement. No auto-invocation. `/ags-start` job done.

Verdict: **COMPLETE** — user oriented, ready for `/ags-session`.

---

## Edge cases

- **User picks D, project is empty**: gently redirect — "Project looks like a fresh template. A or B might fit better?"
- **User picks A but source code exists**: surface findings — "I see code in `assets/scripts/`. Did you mean D?"
- **No option fits**: let the user describe in their own words. Adapt.
- **User-interaction file partially filled**: ask only for missing fields. Do not overwrite answered ones.
- **`sessions/` directory missing but engine + concept set**: treat as 0 unfinished — hand off to `/ags-session` to start the first session.
- **Session file exists but is malformed** (no `## Current task` etc.): list it under "needs review", offer to skip or open in editor.

---

## Collaborative protocol

From `CLAUDE.md` — **Question → Options → Decision → Draft → Approval**:

1. **Ask first** — never assume the user's state or intent.
2. **Give options** — clear paths, not mandates.
3. **User decides** — they pick the direction.
4. **No auto-execution** — recommend the next skill or agent. Never run it without explicit "yes".
5. **Adapt** — if the user's situation does not match a template, listen and adjust.
6. **Write approval** — except for direct-consequence writes listed at the top of this skill, every file write requires "May I write to [path]?" approval first.
