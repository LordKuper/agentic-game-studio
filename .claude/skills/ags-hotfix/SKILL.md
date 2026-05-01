---
name: ags-hotfix
description: "Emergency fix workflow that bypasses normal sprint processes with a full audit trail. Creates hotfix branch, tracks approvals, and ensures the fix is backported correctly."
argument-hint: "[bug-id or description]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, Task, AskUserQuestion
---

> **Explicit invocation only**: Run only on `/ags-hotfix`. No auto-invoke.

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| Git repository with main branch | dev work | STOP. "No git repo." |
| Bug report describing the issue | `/ags-bug-report` | STOP. "No bug report. Run `/ags-bug-report` first to log the issue." |
| Released build to patch (release tag or branch) | release pipeline | WARN: hotfix without released build is just a normal fix — consider `/ags-dev-story`. |

If STOP triggers, exit verdict **BLOCKED**.

---

## Phase 1: Assess Severity

Read bug description/ID. Determine severity:

- **S1 (Critical)**: Game unplayable, data loss, security vulnerability — hotfix immediately
- **S2 (Major)**: Significant feature broken, workaround exists — hotfix within 24 hours
- S3 or lower → recommend normal bug fix workflow, stop.

---

## Phase 2: Create Hotfix Record

Draft record:

```markdown
## Hotfix: [Short Description]
Date: [Date]
Severity: [S1/S2]
Reporter: [Who found it]
Status: IN PROGRESS

### Problem
[What is broken, player impact]

### Root Cause
[Filled during investigation]

### Fix
[Filled during implementation]

### Testing
[What tested, how]

### Approvals
- [ ] Fix reviewed by lead-programmer
- [ ] Regression test passed (qa-lead)
- [ ] Release approved (producer)

### Rollback Plan
[How to revert if fix causes issues]
```

Ask: "May I write this to `.ags/project/hotfixes/hotfix-[date]-[short-name].md`?"

If yes, write file, create directory if needed.

---

## Phase 3: Create Hotfix Branch

If git initialized:

```
git checkout -b hotfix/[short-name] [release-tag-or-main]
```

---

## Phase 4: Investigate and Implement

Minimal change to resolve issue. NO refactoring, cleanup, features.

Run targeted tests for affected system. Check regressions in adjacent systems.

Update hotfix record: root cause, fix details, test results.

---

## Phase 5: Collect Approvals

Request sign-off in parallel via Task:

- `subagent_type: lead-programmer` — Review fix for correctness, side effects
- `subagent_type: qa-lead` — Run targeted regression tests on affected system
- `subagent_type: producer` — Approve deployment timing, communication plan

All three must APPROVE. Any CONCERNS or REJECT → do not deploy, surface and resolve.

---

## Phase 5b: QA Re-Entry Gate

After approvals, determine QA scope before deploy. Spawn `qa-lead` via Task with:
- Hotfix description, affected system
- Phase 5 regression test results
- All systems touching changed files (Grep callers)

Ask qa-lead: **Smoke check sufficient, or targeted team-qa pass required?**

Apply verdict:
- **Smoke check sufficient** — `/ags-smoke-check` against hotfix build. PASS → Phase 6.
- **Targeted QA required** — `/ags-team-qa [affected-system]` scoped to changed system. APPROVED or APPROVED WITH CONDITIONS → Phase 6.
- **Full QA required** — S1 fixes touching core systems may need full `/ags-team-qa sprint`. Delays deploy, prevents bad patch.

Do not skip. Hotfix breaking something else is worse than original bug.

---

## Phase 6: Update Bug Status and Deploy

Update original bug file if exists:

```markdown
## Fix Record
**Fixed in**: hotfix/[branch-name] — [commit hash or description]
**Fixed date**: [date]
**Status**: Fixed — Pending Verification
```

Set `**Status**: Fixed — Pending Verification` in bug header.

Output deployment summary:

```
## Hotfix Ready to Deploy: [short-name]

**Severity**: [S1/S2]
**Root cause**: [one line]
**Fix**: [one line]
**QA gate**: [Smoke check PASS / Team-QA APPROVED]
**Approvals**: lead-programmer ✓ / qa-lead ✓ / producer ✓
**Rollback plan**: [from Phase 2 record]

Merge to: release branch AND development branch
Next: /ags-bug-report verify [BUG-ID] after deploy to confirm resolution
```

### Rules
- Hotfixes = MINIMUM change. No cleanup, no refactoring.
- Every hotfix needs rollback plan before deploy.
- Hotfix branches merge to BOTH release AND development branches.
- All hotfixes need post-incident review within 48 hours.
- Fix > 4 hours → escalate to `technical-director`.

---

## Phase 7: Post-Deploy Verification

After deploy, run `/ags-bug-report verify [BUG-ID]` to confirm fix resolved issue in deployed build.

VERIFIED FIXED → `/ags-bug-report close [BUG-ID]`.
STILL PRESENT → hotfix failed; re-open, assess rollback, escalate.

Schedule post-incident review within 48 hours: `/ags-epic-retro hotfix`.
