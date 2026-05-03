---
name: ags-epic-contracts
description: "Lock minimal API contracts for stub-mode systems in the active epic. Writes Contracts section in EPIC.md and pre-registers stubs in stubs.md."
argument-hint: "[epic-slug]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, AskUserQuestion
---

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `.ags/project/stage.md` (Phase = production) | `/ags-gate-check production` | STOP. "Not in production phase. Run `/ags-gate-check production` first." |
| Active `.ags/project/epics/[slug]/EPIC.md` | `/ags-create-epics` | STOP. "No active epic. Run `/ags-create-epics` first." |
| `.ags/project/stubs.md` (creates if missing) | `/ags-stub-track` or this skill | If missing, this skill creates it from `t_stubs.md` template — not a STOP. |

If any STOP triggers, exit with verdict **BLOCKED — missing prerequisite**.

---

## Phase 1: Resolve Active Epic

If `[epic-slug]` argument provided, use it. Otherwise read `.ags/project/stage.md` to find the active epic ID.

Read `.ags/project/epics/[slug]/EPIC.md`. Verify Status is `designing` or `planned`. If status is `done` or `rolled-back`, use `AskUserQuestion` with prompt "Epic status is [X]. Contracts already locked. Re-lock anyway?" options: `Re-lock` / `Cancel`.

If epic file missing or unreadable, stop. Verdict: **FAIL — no active epic. Run `/ags-create-epics` first.**

---

## Phase 2: Identify Stub Systems

Parse the **Systems in Scope** table in `EPIC.md`. Collect rows where `Mode = stub`.

If no stub rows, no contracts needed. Output: "No stub systems in this epic — contracts not required." Verdict: **COMPLETE — nothing to do**.

---

## Phase 3: Read GDD Context

For each stub system:

- Read the linked GDD section (column `GDD Section`).
- Read GDDs of `new` and `revise` systems in the same epic — they are the consumers that need the stub interface.
- Identify what data and methods consumers require from the stub system.

If consumer needs are unclear, ask the user one question per stub system before drafting.

---

## Phase 4: Draft Contracts

For each stub system, draft:

- **Interface signature** — minimal API consumers need (method names, parameters, return types in engine-language syntax).
- **Default behavior** — `NotImplementedException`, empty collection, sane default value, or no-op.
- **Stub ID** — `STUB-NNN` where NNN is the next free integer in `.ags/project/stubs.md` Open + Closed + Migrated tables. Numbering is global across the project.
- **Owner Epic** — which future epic is expected to close this stub. Use TBD if unknown.

Present draft contracts to the user as a structured block per stub system.

---

## Phase 5: User Review and Edits

Use `AskUserQuestion` with prompt "Any edits to interfaces, defaults, or owner-epic assignments?" and options:
- `Approve as drafted` — proceed to internal review
- `Edit interfaces` — user describes changes free-form, redraft
- `Cancel` — stop with verdict BLOCKED

Apply edits, redraft, present again. Loop until user approves.

---

## Phase 5b: Internal Review Loop

Apply review mode:
- `solo` → skip the loop. Proceed to Phase 5c.
- `lean` / `full` → spawn `lead-programmer` (and optionally `technical-director`) via Task to review the drafted contracts against existing ADRs, registry stances, and stubs.md.

**Loop exit condition.** Single iteration where every spawned reviewer returns clean (no critical/high/medium findings). Non-clean → user revises affected interfaces / defaults, re-spawn the reviewers. No iteration cap.

Record iteration count.

## Phase 5c: External Review Gate (user confirm)

After internal loop CLEAN (or skipped), ask via `AskUserQuestion`:

```
Internal review CLEAN ([N] iterations). Run external Codex review on the contracts before writing?
[A] Yes — run /ags-external-review
[B] Skip external (record reason in decisions-log.md)
[C] Stop — review further
```

- **[A]**: persist contracts draft to `.ags/project/reviews/.tmp/[epic-slug]-contracts-draft.md`. Invoke `/ags-external-review contracts [draft-path] --embedded`. Handle verdict line:
  - `BLOCK` → STOP. Surface report path + blockers. User revises, re-run.
  - `CONCERNS` → surface report path. `AskUserQuestion`: accept, or revise.
  - `PASS` → proceed silently.
  - Codex CLI missing → ask user to skip [B-style] or abort [C-style].
- **[B]**: append to `.ags/project/decisions-log.md`:
  ```
  ## [YYYY-MM-DD HH:MM] — External review skipped: contracts [epic-slug]

  **Type**: process
  **Reason**: [user-supplied reason or "user declined"]
  **Decided by**: user
  ```
- **[C]**: halt skill.

---

## Phase 6: Approval

Use `AskUserQuestion` with prompt "May I write contracts to `EPIC.md` and pre-register stubs in `stubs.md`?" and options:
- `Yes — write both files`
- `No — cancel`

If declined, stop. Verdict: **BLOCKED — user declined write**.

---

## Phase 7: Write

1. **EPIC.md** — Edit the `## Contracts (Stub Interfaces)` section. Replace placeholder content with one subsection per stub system. Edit the `## Stubs Introduced` table — append rows for each new STUB-ID.

2. **stubs.md** — Append rows to `## Open Stubs` table. One row per new stub. Columns: ID, System, Interface, Introduced (epic), Owner Epic, Reason, Notes.

3. **decisions-log.md** — Append entry:

```
## [YYYY-MM-DD HH:MM] — Lock contracts for epic-[id]

**Type**: architecture
**Context**: Epic [name] introduces stub neighbors.
**Decision**: API contracts locked for [list of stub systems].
**Rationale**: [user-provided one-sentence reason]
**Refs**: .ags/project/epics/[slug]/EPIC.md, .ags/project/stubs.md
**Decided by**: human
```

Verdict: **COMPLETE — contracts locked, stubs registered**.

---

## Phase 8: Next Steps

Suggest:

- `/ags-create-stories [epic-slug]` — break epic into stories.
- `/ags-dev-story` — implement stories with stubs marked `// TODO(epic-[id]):`.

---

## Rules

- Contracts are **stable across epics**. Changing a locked contract requires a new ADR or a new epic.
- Every stub registered here MUST appear in code with a `// TODO(epic-[id]):` marker once implementation starts.
- `/ags-stub-track` reconciles code markers with this registry.
