# Epic: [Name]

## Metadata

| Field | Value |
|-------|-------|
| ID | epic-[NNN]-[slug] |
| Status | planned \| designing \| implementing \| playtesting \| done \| rolled-back |
| Created | [YYYY-MM-DD] |
| Closed | [YYYY-MM-DD or —] |

## Scope

Goal, affected systems (Create / Modify / Delete / Touch), component diagram, dependencies, acceptance criteria and open questions live in the scope file:

- [./scope.html](./scope.html)

EPIC.md does not duplicate those fields. Edit `scope.html` for any change to epic intent, system list, or acceptance criteria.

## Contracts (Stub Interfaces)

[Minimal API of stub neighbors. One subsection per stub system. Stable across epics — changes here propagate to all consumers.]

### [system-b]

- Interface: `IFoo { Bar Get(int id); }`
- Default behavior: returns empty / NotImplementedException
- Stub ID: STUB-[NNN]

## Acceptance Criteria

Functional acceptance criteria are defined in [./scope.html](./scope.html) § Acceptance Criteria.

Process gate criteria (always required, not duplicated in scope):

- [ ] All stories closed
- [ ] Stubs from prior epics closed or migrated (see `.ags/project/stubs.md`)
- [ ] Playtest report filed
- [ ] No new S1/S2 bugs open
- [ ] Code reviewed + merged
- [ ] Design docs updated for spec deviations

## Design Artifacts

- design/gdd/[system].md — sections [X-Y]
- design/architecture/adr-[NNN]-[slug].md

## Stories

| ID | Title | Status |
|----|-------|--------|
| [epic-NNN]-S01 | | Not Started |

(Story files live in `stories/` subdirectory; this table is index.)

## Stubs Introduced

| Stub ID | Interface | Reason | Owner Epic (closes) |
|---------|-----------|--------|---------------------|
| STUB-[NNN] | | | [epic-NNN or TBD] |

## Stubs Closed

| Stub ID | Closed In Story | Notes |
|---------|----------------|-------|

## Playtest

- [link to .ags/project/playtests/epic-[slug]-[date].md]

## Retrospective

[Filled by `/ags-epic-retro` after gate. Capture: what worked, what to change, scope creep, surprises, follow-up epics suggested.]

## Gate Verdict

| Director | Verdict | Notes |
|----------|---------|-------|
| Creative | | |
| Technical | | |
| Producer | | |
| Art | | |

Final: [READY \| CONCERNS \| NOT READY]
Closed in `.ags/project/epics/index.md`: [date]

## Notes

[Optional: owner, context, links, anything that does not fit above sections.]
