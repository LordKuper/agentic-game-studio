# External Review — {{TYPE}} {{SLUG}}

| Field | Value |
|---|---|
| **Target** | {{TARGET}} |
| **Type** | {{TYPE}} |
| **Created** | {{DATE}} |
| **Latest iteration** | {{ITERATION}} |
| **Latest mode** | {{MODE}} (standalone / embedded / embedded-parallel) |
| **Latest verdict** | {{VERDICT}} (n/a in embedded-parallel) |
| **Codex version** | {{CODEX_VERSION}} |

## Summary (latest iteration)

| Severity | Kept | Dropped (below floor) | Dropped (nitpick) |
|---|---|---|---|
| Critical | {{N_CRIT_KEPT}} | {{N_CRIT_FLOOR}} | {{N_CRIT_NIT}} |
| High | {{N_HIGH_KEPT}} | {{N_HIGH_FLOOR}} | {{N_HIGH_NIT}} |
| Medium | {{N_MED_KEPT}} | {{N_MED_FLOOR}} | {{N_MED_NIT}} |
| Low | {{N_LOW_KEPT}} | {{N_LOW_FLOOR}} | {{N_LOW_NIT}} |

| Field | Value |
|---|---|
| Severity floor (this iteration) | {{SEVERITY_FLOOR}} |
| Iteration policy | iter 1-2 → all; iter 3-4 → critical+high; iter 5+ → critical |

**Verdict logic** (standalone / embedded only): any kept critical or high → BLOCK. Else any kept medium → CONCERNS. Else PASS.

In `embedded-parallel`, no verdict — caller's aggregator combines kept findings with internal-reviewer findings and decides loop exit.

---

## Iteration {{ITERATION}}{{PARALLEL_TAG}}

**Date**: {{DATE}}
**Mode**: {{MODE}}
**Severity floor applied**: {{SEVERITY_FLOOR}}
**Prompt**: `.ags/project/reviews/.tmp/{{DATE}}-{{TYPE}}-{{SLUG}}-iter{{ITERATION}}-prompt.md`
**Raw output**: `.ags/project/reviews/.tmp/{{DATE}}-{{TYPE}}-{{SLUG}}-iter{{ITERATION}}-raw.json`
**Verdict**: {{VERDICT}}

### Kept findings (substantive, at or above floor)

| # | Severity (Claude) | Codex label | Location | Title | Verified | Action |
|---|---|---|---|---|---|---|
| 1 | {{sev}} | {{codex_sev}} | {{file}}:{{line}} | {{title}} | yes/no | block / fix / accept / dismiss |

### Dropped findings

| # | Severity (Claude) | Title | Drop reason | Notes |
|---|---|---|---|---|
| 1 | {{sev}} | {{title}} | nitpick / below-floor | {{one-line reason cite}} |

### Detail (kept findings)

#### Finding 1 — {{title}}

- **Location**: {{file:line}}
- **Codex severity**: {{codex_sev}}
- **Claude severity**: {{sev}} — *Reason*: {{reclassification reason}}
- **Description**: {{description}}
- **Suggested fix (Codex)**: {{fix}}
- **Verified**: yes / no — {{evidence or "unable to confirm by reading target"}}

(repeat per kept finding)

### Blockers (kept critical + high only)

1. **[Finding #N]** {{title}} — must fix before {{calling-gate-name or "next iteration"}}.

### User actions taken

- [ ] Fixes applied — list commits / file edits
- [ ] CONCERNS accepted — reason: {{reason}}
- [ ] Dismissed (low/unverified) — reason: {{reason}}

### Outcome

- BLOCK: re-run scheduled as iteration {{ITERATION+1}}.
- CONCERNS-ACCEPTED: caller proceeds, decisions-log updated.
- PASS: caller proceeds, decisions-log updated.
- embedded-parallel: caller's aggregator owns outcome; report records kept/dropped only.

---

<!-- Append next ## Iteration N+1 below on re-run. Do not edit past iterations. -->
