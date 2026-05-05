# Epics Index

<!-- Registry of all epics (vertical slices). One row per epic. Source of truth for epic status. -->

## Active

| ID | Slug | Scope (systems) | Branch | PR | Started | Status |
|----|------|-----------------|--------|-----|---------|--------|
| E-001 | <slug> | <sys-a, sys-b> | epic/<slug> | #<n> | YYYY-MM-DD | active |

## Planned

| ID | Slug | Scope (systems) | Depends on | Priority | Status |
|----|------|-----------------|------------|----------|--------|
| E-00N | <slug> | <systems> | <epic-id or "-"> | <P0/P1/P2> | planned |

## Done

| ID | Slug | Scope (systems) | Branch | PR | Closed | Retro |
|----|------|-----------------|--------|-----|--------|-------|
| E-000 | <slug> | <systems> | epic/<slug> | #<n> | YYYY-MM-DD | EPIC.md#retrospective |

## Status values

- `planned` — defined, not started
- `active` — branch cut, in progress
- `blocked` — paused, see `state.md` / `decisions-log.md`
- `done` — merged to `main`, gate passed, retro written
- `cancelled` — abandoned, reason in `decisions-log.md`

## Rules

- ID monotonic: `E-NNN`. Never reuse.
- One `active` epic at a time (matches `stage.md` active epic).
- Epic file: `.ags/project/epics/<slug>/EPIC.md` (from `t_epic.md`).
- Close epic = merge PR + gate PASS + row moved to Done + retro appended.
