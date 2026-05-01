---
name: ags-estimate
description: "Estimates task effort by analyzing complexity, dependencies, historical velocity, and risk factors. Produces a structured estimate with confidence levels."
argument-hint: "[task-description]"
user-invocable: true
allowed-tools: Read, Glob, Grep, AskUserQuestion
---

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| Task description (argument) | user | STOP. "Usage: `/ags-estimate <task-description>` or path to story file." |
| `.ags/project/epics/` for velocity baseline | `/ags-create-epics` | WARN: estimate without historical baseline is rougher. |

If STOP triggers, exit verdict **BLOCKED**.

---

## Phase 1: Understand the Task

Read task description from argument. If too vague to estimate, ask clarification first.

Read CLAUDE.md for project context: tech stack, coding standards, architectural patterns, estimation guidelines.

Read relevant docs from `design/gdd/` if task relates to documented feature.

---

## Phase 2: Scan Affected Code

Identify files/modules to change:

- Assess complexity (size, dependency count, cyclomatic)
- Identify integration points
- Check existing test coverage
- Read past sprint data from `.ags/project/epics/` for similar tasks and historical velocity

---

## Phase 3: Analyze Complexity Factors

**Code Complexity:**
- LOC in affected files
- Dependencies and coupling
- Core/engine vs leaf/feature
- Existing patterns vs new patterns

**Scope:**
- Systems touched
- New code vs modification
- New test coverage required
- Data/config migrations

**Risk:**
- New tech or unfamiliar libs
- Ambiguous requirements
- Dependencies on unfinished work
- Cross-system integration
- Performance sensitivity

---

## Phase 4: Generate the Estimate

```markdown
## Task Estimate: [Task Name]
Generated: [Date]

### Task Description
[Restate clearly in 1-2 sentences]

### Complexity Assessment

| Factor | Assessment | Notes |
|--------|-----------|-------|
| Systems affected | [List] | [Core, gameplay, UI, etc.] |
| Files likely modified | [Count] | [Key files below] |
| New code vs modification | [Ratio] | |
| Integration points | [Count] | [Which systems] |
| Test coverage needed | [Low / Medium / High] | |
| Existing patterns available | [Yes / Partial / No] | |

**Key files likely affected:**
- `[path/to/file1]` -- [what changes]

### Effort Estimate

| Scenario | Days | Assumption |
|----------|------|------------|
| Optimistic | [X] | No surprises |
| Expected | [Y] | Normal pace, minor issues, one review |
| Pessimistic | [Z] | Unknowns surface, blocked for a day |

**Recommended budget: [Y days]**

### Confidence: [High / Medium / Low]

[Explain factors driving confidence.]

### Risk Factors

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|

### Dependencies

| Dependency | Status | Impact if Delayed |
|-----------|--------|-------------------|

### Suggested Breakdown

| # | Sub-task | Estimate | Notes |
|---|----------|----------|-------|
| 1 | [Research / spike] | [X days] | |
| 2 | [Core implementation] | [X days] | |
| 3 | [Testing and validation] | [X days] | |
| | **Total** | **[Y days]** | |

### Notes and Assumptions
- [Key assumption affecting estimate]
- [Caveats about scope]
```

Output estimate with summary: recommended budget, confidence, biggest risk.

Read-only — no files written. Verdict: **COMPLETE** — estimate generated.

---

## Phase 5: Next Steps

- Confidence Low → recommend time-boxed spike before commit.
- Task > 10 days → break into smaller stories via `/ags-create-stories`.
- To schedule: run `/ags-create-epics update`.

### Guidelines

- Always range (optimistic/expected/pessimistic), never single number
- Recommended budget = expected, not optimistic
- Round to half-day increments — hours imply false precision past one day
- No silent padding — call out risk explicitly
