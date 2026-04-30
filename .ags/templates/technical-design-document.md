# Technical Design: [System Name]

## Document Status
- **Version**: 1.0
- **Last Updated**: [Date]
- **Author**: [Agent/Person]
- **Reviewer**: lead-programmer
- **Related ADR**: [ADR-XXXX if applicable]
- **Related Design Doc**: [Link to design doc this implements]

## Engine API Surface

| Field | Value |
|-------|-------|
| **Engine** | [Unity 6000.0.30f1 / Unity 6] |
| **APIs Depended On** | [Specific classes/methods, version-pinned — `CharacterController.Move() (Unity 6)`] |
| **References Consulted** | [engine-reference docs read — `.ags/.ags/docs/engine-reference/unity/modules/physics.md`] |
| **Post-Cutoff Features Used** | [Beyond LLM training cutoff, or "None"] |
| **Unverified Assumptions** | [API behaviours assumed but not tested, or "None"] |
| **Engine Upgrade Risk** | [LOW / MEDIUM / HIGH] |

> **Rule**: Unverified Assumptions present → cannot mark Accepted until validated in actual engine.

## Overview
[2-3 sentences: what system does + why exists]

## Requirements
### Functional Requirements
- [FR-1]: [Description]
- [FR-2]: [Description]

### Non-Functional Requirements
- **Performance**: [Budget — "<1ms per frame"]
- **Memory**: [Budget — "<50MB at peak"]
- **Scalability**: [Limits — "Up to 1000 entities"]
- **Thread Safety**: [Requirements]

## Architecture

### System Diagram
```
[ASCII diagram: components + data flow]
```

### Component Breakdown
| Component | Responsibility | Owns |
| --------- | -------------- | ---- |
| [Name] | [What it does] | [Data owned] |

### Public API
```
[Interface/API in pseudocode or target language]
```

### Data Structures
```
[Key structures with field descriptions]
```

### Data Flow
[Step by step: data movement during typical frame]

## Implementation Plan

### Phase 1: [Core Functionality]
- [ ] [Task 1]
- [ ] [Task 2]

### Phase 2: [Extended Features]
- [ ] [Task 3]
- [ ] [Task 4]

### Phase 3: [Optimization/Polish]
- [ ] [Task 5]

## Dependencies
| Depends On | For What |
| ---------- | -------- |
| [System] | [Reason] |

| Depended On By | For What |
| -------------- | -------- |
| [System] | [Reason] |

## Testing Strategy
- **Unit Tests**: [What at unit level]
- **Integration Tests**: [Cross-system needed]
- **Performance Tests**: [Benchmarks]
- **Edge Cases**: [Specific scenarios]

## Known Limitations
[What this design intentionally does NOT support + why]

## Future Considerations
[What might change if requirements evolve — do NOT build now]
