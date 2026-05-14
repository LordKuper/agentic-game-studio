---
name: ags-milestone-review
description: "Generates a comprehensive milestone progress review including feature completeness, quality metrics, risk assessment, and go/no-go recommendation. Use at milestone checkpoints or when evaluating readiness for a milestone deadline."
argument-hint: "[milestone-name|current]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Task, AskUserQuestion
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

## Phase 0a: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `.ags/project/milestones/` with ≥1 milestone | manual or `t_milestone-definition.md` | STOP. "No milestones defined." |
| `.ags/project/epics/index.md` | `/ags-create-epics` | STOP. "No epics. Run `/ags-create-epics`." |

If STOP triggers, exit verdict **BLOCKED**.

---

## Phase 0: Parse Arguments

Extract the milestone name (`current` or a specific name).

---

## Phase 1: Load Milestone Data

Read the milestone definition from `.ags/project/milestones/`. If the argument is `current`, use the most recently modified milestone file.

Read all sprint reports for sprints within this milestone from `.ags/project/epics/`.

---

## Phase 1b: Document Boundary Check (mandatory — per `.ags/rules/review-workflow.md` § Document Boundary Check)

Run on all design artifacts touched within milestone scope (GDDs, ADRs, UX/HUD specs, art-bible, DESIGN.md). Per `.ags/rules/document-boundaries.md`: front-matter `status:` validity, SSoT zone violations, missing approval markers on cited predecessors.

Delegation: invoke `/ags-consistency-check full` and merge Boundary Violations into milestone risk-assessment + scope-recommendations sections.

---

## Phase 2: Scan Codebase Health

- Scan for `TODO`, `FIXME`, `HACK` markers that indicate incomplete work
- Check the risk register at `.ags/project/risk-register/`

---

## Phase 3: Generate the Milestone Review

```markdown
# Milestone Review: [Milestone Name]

## Overview
- **Target Date**: [Date]
- **Current Date**: [Today]
- **Days Remaining**: [N]
- **Sprints Completed**: [X/Y]

## Feature Completeness

### Fully Complete
| Feature | Acceptance Criteria | Test Status |
|---------|-------------------|-------------|

### Partially Complete
| Feature | % Done | Remaining Work | Risk to Milestone |
|---------|--------|---------------|------------------|

### Not Started
| Feature | Priority | Can Cut? | Impact of Cutting |
|---------|----------|----------|------------------|

## Quality Metrics
- **Open S1 Bugs**: [N] -- [List]
- **Open S2 Bugs**: [N]
- **Open S3 Bugs**: [N]
- **Test Coverage**: [X%]
- **Performance**: [Within budget? Details]

## Code Health
- **TODO count**: [N across codebase]
- **FIXME count**: [N]
- **HACK count**: [N]
- **Technical debt items**: [List critical ones]

## Risk Assessment
| Risk | Status | Impact if Realized | Mitigation Status |
|------|--------|-------------------|------------------|

## Velocity Analysis
- **Planned vs Completed** (across all sprints): [X/Y tasks = Z%]
- **Trend**: [Improving / Stable / Declining]
- **Adjusted estimate for remaining work**: [Days needed at current velocity]

## Scope Recommendations
### Protect (Must ship with milestone)
- [Feature and why]

### At Risk (May need to cut or simplify)
- [Feature and risk]

### Cut Candidates (Can defer without compromising milestone)
- [Feature and impact of cutting]

## Go/No-Go Assessment

**Recommendation**: [GO / CONDITIONAL GO / NO-GO]

**Conditions** (if conditional):
- [Condition 1 that must be met]
- [Condition 2 that must be met]

**Rationale**: [Explanation of the recommendation]

## Action Items
| # | Action | Owner | Deadline |
|---|--------|-------|----------|
```

---

## Phase 3b: Producer Risk Assessment

Before generating the Go/No-Go recommendation, spawn `producer` via Task using gate **PR-MILESTONE** (`.ags/rules/director-gates.md`).

Pass: milestone name and target date, current completion percentage, blocked story count, velocity data from sprint reports (if available), list of cut candidates.

Present the producer's assessment inline within the Go/No-Go section. The producer's verdict (ON TRACK / AT RISK / OFF TRACK) informs the overall recommendation — do not issue a GO against an OFF TRACK producer verdict without explicit user acknowledgement.

---

## Phase 4: Save Review

Present the review to the user.

Ask: "May I write this to `.ags/project/milestones/[milestone-name]-review.md`?"

If yes, write the file, creating the directory if needed. Verdict: **COMPLETE** — milestone review saved.

If no, stop here. Verdict: **BLOCKED** — user declined write.

---

## Phase 5: Next Steps

- Run `/ags-gate-check` for a formal phase gate verdict if this milestone marks a development phase boundary.
- Run `/ags-create-epics` to adjust the next sprint based on the scope recommendations above.

---

## Combined Review Loop (parallel external Codex)

Per `.ags/rules/review-workflow.md`. Milestone-analysis phases run **in parallel** with external Codex inside one loop. Each iteration:

1. Resolve severity floor: iter 1-2 → keep all severities; iter 3-4 → critical/high; iter 5+ → critical only.
2. **Spawn in one message, in parallel**:
   - All internal reviewer Tasks (producer + relevant director gates).
   - For each epic in the milestone scope: `/ags-external-review epic [epic-path] --embedded-parallel --iteration [N] --min-severity [floor]`. Codex unavailable → `skipped: codex-unavailable`; aggregator logs skip in decisions-log and continues with internal pool only.
3. Aggregator (`producer`) merges findings from internal + Codex, drops nitpicks + below-floor.
4. **Loop exit**: filtered set empty → emit final verdict. Non-empty → surface aggregated kept findings, user resolves, N++, repeat.

No iteration cap. No user-confirm gate before external — it runs every iteration automatically. Record final iteration count in the verdict report and decisions-log entry. Codex reviews the source epics, NOT this milestone-review report.
