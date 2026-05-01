# ADR-[NNNN]: [Title]

## Status

[Proposed | Accepted | Deprecated | Superseded by ADR-XXXX]

## Date

[YYYY-MM-DD]

## Last Verified

[YYYY-MM-DD — last confirmed accurate against current engine + design. Update on re-read confirmation, even if nothing changed.]

## Decision Makers

[Who involved]

## Summary

[2 sentences: problem solved + decision. For tiered context loading — skill scanning 20 ADRs uses this. Be specific: name system, problem, approach.]

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | [Unity 6000.0.30f1 / Unity 6] |
| **Domain** | [Physics / Rendering / UI / Audio / Navigation / Animation / Networking / Core / Input / Scripting] |
| **Knowledge Risk** | [LOW — in training data / MEDIUM — near cutoff, verify / HIGH — post-cutoff, must verify] |
| **References Consulted** | [`.ags/.ags/docs/engine-reference/unity/modules/physics.md`, `breaking-changes.md`] |
| **Post-Cutoff APIs Used** | [Specific post-cutoff APIs depended on, or "None"] |
| **Verification Required** | [Concrete behaviours to test against target engine before ship, or "None"] |

> **Note**: Knowledge Risk MEDIUM/HIGH → re-validate on engine version upgrade. Flag "Superseded", write new ADR.

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | [ADR-NNNN (must be Accepted before implementation), or "None"] |
| **Enables** | [ADR-NNNN (this unlocks that), or "None"] |
| **Blocks** | [Epic/Story name — cannot start until Accepted, or "None"] |
| **Ordering Note** | [Sequencing constraint not captured above] |

## Context

### Problem Statement

[What problem? Why decide now? Cost of not deciding?]

### Current State

[How system works today? What's wrong?]

### Constraints

- [Technical — engine limits, platform reqs]
- [Timeline — deadlines, dependencies]
- [Resource — team size, expertise]
- [Compatibility — must work with existing systems]

### Requirements

- [Functional req 1]
- [Functional req 2]
- [Performance — specific, measurable]
- [Scalability]

## Decision

[Specific technical decision, detailed enough for implementation without further clarification.]

### Architecture

```
[ASCII diagram: components, data flow, key interfaces.]
```

### Key Interfaces

```
[Pseudocode/language interface definitions. Contracts implementers respect.]
```

### Implementation Guidelines

[Specific guidance for implementer.]

## Alternatives Considered

### Alternative 1: [Name]

- **Description**: [How it would work]
- **Pros**: [Good]
- **Cons**: [Bad]
- **Estimated Effort**: [Relative to chosen]
- **Rejection Reason**: [Why not chosen]

### Alternative 2: [Name]

[Same structure]

## Consequences

### Positive

- [Good outcomes]

### Negative

- [Trade-offs accepted]

### Neutral

- [Different, not better/worse]

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|

## Performance Implications

| Metric | Before | Expected After | Budget |
|--------|--------|---------------|--------|
| CPU (frame time) | [X]ms | [Y]ms | [Z]ms |
| Memory | [X]MB | [Y]MB | [Z]MB |
| Load Time | [X]s | [Y]s | [Z]s |
| Network (if applicable) | [X]KB/s | [Y]KB/s | [Z]KB/s |

## Migration Plan

[If changes existing systems, step-by-step.]

1. [Step 1 — what changes, what breaks, how to verify]
2. [Step 2]
3. [Step 3]

**Rollback plan**: [How to revert if wrong]

## Validation Criteria

[How we'll know this was correct after implementation.]

- [ ] [Measurable criterion 1]
- [ ] [Measurable criterion 2]
- [ ] [Performance criterion]

## GDD Requirements Addressed

<!-- MANDATORY. Every ADR traces to ≥1 GDD requirement OR explicitly states foundational with no GDD dependency. Audited by /ags-architecture-review. -->

| GDD Document | System | Requirement | How ADR Satisfies |
|-------------|--------|-------------|-------------------|
| [`design/gdd/combat.md`] | [Combat] | ["Hitbox detection within 1 frame"] | ["Physics raycasts sync in `FixedUpdate`"] |

> Foundational with no GDD dependency: write "Foundational — no GDD requirement. Enables: [GDD systems unlocked or constrained]"

## Related

- [Related ADRs — supersedes, contradicts, depends on]
- [Code files once implemented]
