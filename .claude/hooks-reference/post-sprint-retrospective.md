# Hook: post-sprint-retrospective

## Trigger

Manual at sprint end. Producer agent or human invokes.

## Purpose

Auto-generate retro starting point from sprint data: planned vs done, velocity, bug trends, blockers. Workflow hook, not git hook.

## Implementation

Workflow hook. Invoke via:

```
@producer Generate sprint retrospective for Sprint [N]
```

Producer agent must:

1. **Read sprint plan** from `.ags/project/sprints/sprint-[N].md`
2. **Calc metrics**:
   - Tasks planned vs done
   - Story points planned vs done (if used)
   - Carryover from prior sprint
   - New tasks added mid-sprint
   - Avg task completion time
3. **Find patterns**:
   - Most common blockers
   - Agent/area with most incomplete work
   - Most inaccurate estimates
4. **Generate retro**:

```markdown
# Sprint [N] Retrospective

## Metrics
| Metric | Value |
|--------|-------|
| Tasks Planned | [N] |
| Tasks Completed | [N] |
| Completion Rate | [X%] |
| Carryover from Previous | [N] |
| New Tasks Added | [N] |
| Bugs Found | [N] |
| Bugs Fixed | [N] |

## Velocity Trend
[Sprint N-2]: [X] | [Sprint N-1]: [Y] | [Sprint N]: [Z]
Trend: [Improving / Stable / Declining]

## What Went Well
- [Automatically detected: tasks completed ahead of estimate]
- [Facilitator adds team observations]

## What Went Poorly
- [Automatically detected: tasks that were carried over or cut]
- [Automatically detected: areas with significant estimate overruns]
- [Facilitator adds team observations]

## Blockers
| Blocker | Frequency | Resolution Time | Prevention |
|---------|-----------|----------------|-----------|

## Action Items for Next Sprint
| # | Action | Owner | Priority |
|---|--------|-------|----------|

## Estimation Accuracy
| Area | Avg Planned | Avg Actual | Accuracy |
|------|------------|-----------|----------|
```

5. **Save** to `.ags/project/sprints/sprint-[N]-retro.md`
