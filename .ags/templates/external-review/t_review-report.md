# External Review — {{TYPE}} {{SLUG}}

| Field | Value |
|---|---|
| **Target** | {{TARGET}} |
| **Type** | {{TYPE}} |
| **Created** | {{DATE}} |
| **Latest iteration** | {{ITERATION}} |
| **Latest verdict** | {{VERDICT}} |
| **Codex version** | {{CODEX_VERSION}} |

## Summary (latest iteration)

| Severity | Count |
|---|---|
| Critical | {{N_CRIT}} |
| High | {{N_HIGH}} |
| Medium | {{N_MED}} |
| Low | {{N_LOW}} |

**Verdict logic**: any critical or high → BLOCK. Else any medium → CONCERNS. Else PASS.

---

## Iteration {{ITERATION}}

**Date**: {{DATE}}
**Prompt**: `.ags/project/reviews/.tmp/{{DATE}}-{{TYPE}}-{{SLUG}}-iter{{ITERATION}}-prompt.md`
**Raw output**: `.ags/project/reviews/.tmp/{{DATE}}-{{TYPE}}-{{SLUG}}-iter{{ITERATION}}-raw.json`
**Verdict**: {{VERDICT}}

### Findings

| # | Severity (Claude) | Codex label | Location | Title | Verified | Action |
|---|---|---|---|---|---|---|
| 1 | {{sev}} | {{codex_sev}} | {{file}}:{{line}} | {{title}} | yes/no | block / fix / accept / dismiss |

### Detail

#### Finding 1 — {{title}}

- **Location**: {{file:line}}
- **Codex severity**: {{codex_sev}}
- **Claude severity**: {{sev}} — *Reason*: {{reclassification reason}}
- **Description**: {{description}}
- **Suggested fix (Codex)**: {{fix}}
- **Verified**: yes / no — {{evidence or "unable to confirm by reading target"}}

(repeat per finding)

### Blockers (critical + high only)

1. **[Finding #N]** {{title}} — must fix before {{calling-gate-name or "next iteration"}}.

### User actions taken

- [ ] Fixes applied — list commits / file edits
- [ ] CONCERNS accepted — reason: {{reason}}
- [ ] Dismissed (low/unverified) — reason: {{reason}}

### Outcome

- BLOCK: re-run scheduled as iteration {{ITERATION+1}}.
- CONCERNS-ACCEPTED: caller proceeds, decisions-log updated.
- PASS: caller proceeds, decisions-log updated.

---

<!-- Append next ## Iteration N+1 below on re-run. Do not edit past iterations. -->
