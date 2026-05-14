---
name: ags-stub-track
description: "Scan codebase for TODO(epic-...) stub markers and sync .ags/project/stubs.md. Detects newly introduced stubs not registered, stubs whose marker disappeared (closed), and signature drift."
argument-hint: "[scan|close|migrate]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, AskUserQuestion
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `.ags/project/stubs.md` | `/ags-epic-contracts` (first lock) or auto-create here | Auto-create empty registry from `t_stubs.md` if missing — not a STOP. |
| `.ags/project/epics/index.md` | `/ags-create-epics` | STOP. "No epics yet. Run `/ags-create-epics` first — stubs are tied to epics." |

If `stubs.md` is auto-created, log it: "Initialized empty stubs registry at `.ags/project/stubs.md` from template."

---

## Phase 1: Parse Subcommand

- `scan` (default) — auto-reconcile code markers with registry.
- `close [STUB-ID] [story-ref]` — mark a stub as closed by a specific story.
- `migrate [STUB-ID] [new-owner-epic] [reason]` — defer a stub to a different future epic.

If no subcommand, default to `scan`.

---

## Phase 2A: Scan Mode

### 1. Search code markers

Grep the codebase for stub markers. Universal pattern (works for `//`, `#`, `--`, `;`, `/* */` comments):

```
TODO\(epic-[a-z0-9-]+\)
```

Glob: all source files in the engine project root and `tests/`. Skip `.ags/`, `.claude/`, `design/`, `assets/data/`.

For each match capture:
- File path + line
- Full marker text (everything after `TODO(epic-XXX):`)
- Surrounding interface signature (read 5 lines context)
- Epic ID

### 2. Read registry

Read `.ags/project/stubs.md`. Parse the three tables: Open Stubs, Closed Stubs, Migrated Stubs.

### 3. Reconcile

Build three lists:

- **Unregistered** — code marker present, but the surrounding interface signature does not match any STUB-ID row in any table. Propose new STUB-NNN (next free global integer). Ask the user to confirm Stub ID assignment per item.

- **Closed** — STUB-ID in Open Stubs, but no matching marker found in code. Candidate for move to Closed table. Ask the user to provide story reference or confirm "auto-close".

- **Drift** — STUB-ID matches a marker, but the interface signature differs from the registry. Flag for user review. Drift requires either (a) registry update, (b) ADR for contract change, or (c) code revert.

### 4. Present diff

Show three sections to user with proposed changes.

### 5. Approval

Use `AskUserQuestion` with prompt "May I update `stubs.md` with these changes?" and options:
- `Apply all` — write all proposed changes
- `Apply selective` — user picks which categories (Unregistered / Closed / Drift) to apply
- `Cancel`

If declined, stop. Verdict: **BLOCKED**.

### 6. Write

Edit `stubs.md` — append/move rows as approved.

For each Closed move, also append entry to `.ags/project/decisions-log.md` (type: architecture, brief).

For Drift items resolved by registry update, append decision-log entry referencing the change.

Verdict: **COMPLETE — registry synced**.

---

## Phase 2B: Close Mode

Args: `[STUB-ID] [story-ref]`.

1. Read `stubs.md`. Find STUB-ID in Open Stubs. If not present, fail with reason.
2. Verify code marker is gone — grep for `TODO(epic-` near the stub interface. If still present, warn user before closing.
3. Use `AskUserQuestion` with prompt "May I move STUB-[ID] to Closed table with story-ref [ref]?" and options: `Yes — close`, `No — cancel`.
4. Edit `stubs.md` — remove from Open, append to Closed (ID, Closed In, Date=today, Notes).
5. Append decision-log entry.

Verdict: **COMPLETE — stub closed**.

---

## Phase 2C: Migrate Mode

Args: `[STUB-ID] [new-owner-epic] [reason]`.

1. Read `stubs.md`. Find STUB-ID in Open Stubs.
2. Show current owner-epic and proposed new owner.
3. Approval (HUMAN — migration is a scope change): "May I migrate STUB-ID from [old] to [new]? Reason: [reason]"
4. Edit `stubs.md` — keep row in Open Stubs but update Owner Epic; append row to Migrated Stubs (ID, From Epic, To Epic, Reason, Approved By).
5. Append decision-log entry (type: scope).

Verdict: **COMPLETE — stub migrated**.

---

## Phase 3: Next Steps

After scan:
- Continue `/ags-dev-story` if epic in progress.
- `/ags-gate-check epic-done` if all stubs introduced by closing epic are Closed or Migrated.

---

## Rules

- Stub IDs are **global incremental** across the entire project.
- `/ags-gate-check epic-done` blocks epic close if any STUB introduced by the closing epic remains in Open Stubs without a Migration entry.
- Drift detection is best-effort — interface comparison may need user judgment.
- Universal grep pattern catches every comment style; do not specialize per language.
