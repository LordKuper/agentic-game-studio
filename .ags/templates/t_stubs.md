# Stubs Registry

Append-only log of TODO stubs introduced by epics. Code marker: `// TODO(epic-[id]): [reason]`.

Source of truth for unfinished stub interfaces. Synced by `/ags-stub-track` from code markers and by `/ags-create-epics` when an epic introduces a new stub.

## Open Stubs

| ID | System | Interface | Introduced (epic) | Owner Epic | Reason | Notes |
|----|--------|-----------|-------------------|------------|--------|-------|
| STUB-001 | [inventory] | [IItemStore.Get] | epic-002 | epic-005 | Need impl after combat slice | |

## Closed Stubs

| ID | Closed In | Date | Notes |
|----|-----------|------|-------|
| STUB-000 | epic-003 / story S04 | [YYYY-MM-DD] | |

## Migrated Stubs

| ID | From Epic | To Epic | Reason | Approved By |
|----|-----------|---------|--------|-------------|

## Rules

- Every stub has a code marker: `// TODO(epic-[id]): [reason]`.
- Every stub is registered in **Open Stubs** when introduced.
- Gate `/ags-gate-check epic-done` verifies that stubs owned by the closing epic are moved to **Closed** (impl) or **Migrated** (deferred with approval).
- Stub interfaces are stable: changes require a new ADR or epic.
