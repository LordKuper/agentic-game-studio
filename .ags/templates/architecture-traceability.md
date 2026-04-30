# Architecture Traceability Index

<!-- Living document — updated by /architecture-review after each review. Do not edit manually unless correcting an error. -->

## Document Status

- **Last Updated**: [YYYY-MM-DD]
- **Engine**: [Unity 6000.0.30f1]
- **GDDs Indexed**: [N]
- **ADRs Indexed**: [M]
- **Last Review**: [link to design/architecture/architecture-review-[date].md]

## Coverage Summary

| Status | Count | Percentage |
|--------|-------|-----------|
| Covered | [X] | [%] |
| Partial | [Y] | [%] |
| Gap | [Z] | [%] |
| **Total** | **[N]** | |

---

## Traceability Matrix

<!-- One row per technical requirement extracted from a GDD. "Technical requirement" = GDD statement implying specific architectural decision: data structures, performance constraints, engine capabilities, cross-system communication, state persistence. -->

| Req ID | GDD | System | Requirement Summary | ADR(s) | Status | Notes |
|--------|-----|--------|---------------------|--------|--------|-------|
| TR-[gdd]-001 | [filename] | [system] | [one-line] | [ADR-NNNN] | Covered | |
| TR-[gdd]-002 | [filename] | [system] | [one-line] | — | GAP | Needs `/architecture-decision [title]` |

---

## Known Gaps

Requirements with no ADR coverage, prioritised by layer (Foundation first):

### Foundation Layer Gaps (BLOCKING — resolve before coding)
- [ ] TR-[id]: [requirement] — GDD: [file] — Suggested ADR: "[title]"

### Core Layer Gaps (resolve before relevant system built)
- [ ] TR-[id]: [requirement] — GDD: [file] — Suggested ADR: "[title]"

### Feature Layer Gaps (resolve before feature sprint)
- [ ] TR-[id]: [requirement] — GDD: [file] — Suggested ADR: "[title]"

### Presentation Layer Gaps (defer to implementation)
- [ ] TR-[id]: [requirement] — GDD: [file] — Suggested ADR: "[title]"

---

## Cross-ADR Conflicts

<!-- Pairs of ADRs making contradictory claims. Must resolve. -->

| Conflict ID | ADR A | ADR B | Type | Status |
|-------------|-------|-------|------|--------|
| CONFLICT-001 | ADR-NNNN | ADR-MMMM | Data ownership | Unresolved |

---

## ADR → GDD Coverage (Reverse Index)

<!-- Per ADR: which GDD requirements does it address? -->

| ADR | Title | GDD Requirements Addressed | Engine Risk |
|-----|-------|---------------------------|-------------|
| ADR-0001 | [title] | TR-combat-001, TR-combat-002 | HIGH |

---

## Superseded Requirements

<!-- Requirements that existed in GDD when ADR was written, but GDD has changed. ADR may need update. -->

| Req ID | GDD | Change | Affected ADR | Status |
|--------|-----|--------|-------------|--------|
| TR-[id] | [file] | [what changed] | ADR-NNNN | ADR needs update |

---

## How to Use This Document

**New ADR**: Add to "ADR → GDD Coverage" table. Mark satisfied requirements as Covered.

**Approving GDD change**: Scan matrix for that GDD's requirements. Check if change invalidates any ADR. Add to "Superseded" if so.

**Running `/architecture-review`**: Skill updates this doc automatically.

**Gate check**: Pre-Production gate requires this doc to exist with zero Foundation Layer Gaps.
