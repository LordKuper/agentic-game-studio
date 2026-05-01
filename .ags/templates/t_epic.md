# Epic: [Name]

## Metadata

| Field | Value |
|-------|-------|
| ID | epic-[NNN]-[slug] |
| Status | planned \| designing \| implementing \| playtesting \| done \| rolled-back |
| Created | [YYYY-MM-DD] |
| Closed | [YYYY-MM-DD or —] |

## Rationale

[2-3 sentences: why this epic now, what risk it burns down, what playable state it produces.]

## Systems in Scope

Mode values:
- `new` — system implemented for the first time in this epic
- `revise` — existing system extended, refactored, or rewired (see Existing System Changes below)
- `stub` — interface only, real impl deferred to a future epic

Epic may be all-`new`, all-`revise`, or any mix.

| System | Mode | GDD Section |
|--------|------|------------|
| [system-a] | new | [link] |
| [system-b] | revise | [link] |
| [system-c] | stub | [link or —] |

## Existing System Changes

[Fill this section only if at least one system is `revise`. For each revised system: what changes, why, breaking-change impact on other systems, GDD diff summary, ADR if architectural.]

### [system-b]
- **Change**: [what is being modified]
- **Reason**: [why now]
- **Impact**: [other systems affected]
- **GDD diff**: [section/subsection updated]
- **ADR**: [link if architectural change]

## Contracts (Stub Interfaces)

[Minimal API of stub neighbors. One subsection per stub system. Stable across epics — changes here propagate to all consumers.]

### [system-b]

- Interface: `IFoo { Bar Get(int id); }`
- Default behavior: returns empty / NotImplementedException
- Stub ID: STUB-[NNN]

## Acceptance Criteria

- [ ] Playable end-to-end through scoped systems
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
