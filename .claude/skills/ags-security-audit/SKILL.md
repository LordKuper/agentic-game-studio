---
name: ags-security-audit
description: "Audit the game for security vulnerabilities: save tampering, cheat vectors, network exploits, data exposure, and input validation gaps. Produces a prioritised security report with remediation guidance. Run before any public release or multiplayer launch."
argument-hint: "[full | network | save | input | quick]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Task, AskUserQuestion
agent: lead-programmer
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

# Security Audit

Security is not optional. Single-player games have save tampering vectors.
Multiplayer games have cheat surfaces, data exposure risks, and DoS potential.
Systematically audits codebase for common game security failures and produces
prioritised remediation plan.

**Run this skill:**
- Before any public release (required for the Polish → Release gate)
- Before enabling any online/multiplayer feature
- After implementing any system that reads from disk or network
- When a security-related bug is reported

**Output:** `.ags/project/security/security-audit-[date].md`

---

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| Engine source code | engine init | STOP. "No engine source — nothing to audit." |
| `CLAUDE.md` (project context) | template | STOP. "Project not initialized." |

If STOP triggers, exit verdict **BLOCKED**.

---

## Phase 1: Parse Arguments and Scope

**Modes:**
- `full` — all categories (recommended before release)
- `network` — network/multiplayer only
- `save` — save file and serialization only
- `input` — input validation and injection only
- `quick` — high-severity checks only (fastest, for iterative use)
- No argument — run `full`

Read `.ags/rules/technical-preferences.md` to determine:
- Engine and language (affects which patterns to search for)
- Target platforms (affects which attack surfaces apply)
- Whether multiplayer/networking is in scope

---

## Phase 2: Spawn Security Engineer

Spawn `lead-programmer` via Task. Pass:
- The audit scope/mode
- Engine and language from technical preferences
- A manifest of all source directories: `Assets/Scripts/`, `assets/data/`, any config files

The lead-programmer runs the audit across 6 categories (see Phase 3). Collect their full findings before proceeding.

---

## Phase 3: Audit Categories

The lead-programmer evaluates each of the following. Skip categories not applicable to the project scope.

### Category 1: Save File and Serialization Security
- Are save files validated before loading? (no blind deserialization)
- Are save file paths constructed from user input? (path traversal risk)
- Are save files checksummed or signed? (tamper detection)
- Does the game trust numeric values from save files without bounds checking?
- Are there any eval() or dynamic code execution calls near save loading?

Grep patterns: `File.open`, `load`, `deserialize`, `JSON.parse`, `from_json`, `read_file` — check each for validation.

### Category 2: Network and Multiplayer Security (skip if single-player only)
- Is game state authoritative on the server, or does the client dictate outcomes?
- Are incoming network packets validated for size, type, and value range?
- Are player positions and state changes validated server-side?
- Is there rate limiting on any network calls?
- Are authentication tokens handled correctly (never sent in plaintext)?
- Does the game expose any debug endpoints in release builds?

Grep for: `recv`, `receive`, `PacketPeer`, `socket`, `NetworkedMultiplayerPeer`, `rpc`, `rpc_id` — check each call site for validation.

### Category 3: Input Validation
- Are any player-supplied strings used in file paths? (path traversal)
- Are any player-supplied strings logged without sanitization? (log injection)
- Are numeric inputs (e.g., item quantities, character stats) bounds-checked before use?
- Are achievement/stat values checked before being written to any backend?

Grep for: `get_input`, `Input.get_`, `input_map`, user-facing text fields — check validation.

### Category 4: Data Exposure
- Are any API keys, credentials, or secrets hardcoded in `Assets/Scripts/` or `assets/`?
- Are debug symbols or verbose error messages included in release builds?
- Does the game log sensitive player data to disk or console?
- Are any internal file paths or system information exposed to players?

Grep for: `api_key`, `secret`, `password`, `token`, `private_key`, `DEBUG`, `print(` in release-facing code.

### Category 5: Cheat and Anti-Tamper Vectors
- Are gameplay-critical values stored only in memory, not in easily-editable files?
- Are any critical game progression flags (e.g., "has paid for DLC") validated server-side?
- Is there any protection against memory editing tools (Cheat Engine, etc.) for multiplayer?
- Are leaderboard/score submissions validated before acceptance?

Note: Client-side anti-cheat is largely unenforceable. Focus on server-side validation for anything competitive or monetised.

### Category 6: Dependency and Supply Chain
- Are any third-party plugins or libraries used? List them.
- Do any plugins have known CVEs in the version being used?
- Are plugin sources verified (official marketplace, reviewed repository)?

Glob for: `addons/`, `plugins/`, `third_party/`, `vendor/` — list all external dependencies.

---

## Phase 4: Classify Findings

For each finding, assign:

**Severity:**
| Level | Definition |
|-------|-----------|
| **CRITICAL** | Remote code execution, data breach, or trivially-exploitable cheat that breaks multiplayer integrity |
| **HIGH** | Save tampering that bypasses progression, credential exposure, or server-side authority bypass |
| **MEDIUM** | Client-side cheat enablement, information disclosure, or input validation gap with limited impact |
| **LOW** | Defence-in-depth improvement — hardening that reduces attack surface but no direct exploit exists |

**Status:** Open / Accepted Risk / Out of Scope

---

## Phase 5: Generate Report

```markdown
# Security Audit Report

**Date**: [date]
**Scope**: [full | network | save | input | quick]
**Engine**: [engine + version]
**Audited by**: lead-programmer via /ags-security-audit
**Files scanned**: [N source files, N config files]

---

## Executive Summary

| Severity | Count | Must Fix Before Release |
|----------|-------|------------------------|
| CRITICAL | [N] | Yes — all |
| HIGH | [N] | Yes — all |
| MEDIUM | [N] | Recommended |
| LOW | [N] | Optional |

**Release recommendation**: [CLEAR TO SHIP / FIX CRITICALS FIRST / DO NOT SHIP]

---

## CRITICAL Findings

### SEC-001: [Title]
**Category**: [Save / Network / Input / Data / Cheat / Dependency]
**File**: `[path]` line [N]
**Description**: [What the vulnerability is]
**Attack scenario**: [How a malicious user would exploit it]
**Remediation**: [Specific code change or pattern to apply]
**Effort**: [Low / Medium / High]

[repeat per finding]

---

## HIGH Findings

[same format]

---

## MEDIUM Findings

[same format]

---

## LOW Findings

[same format]

---

## Accepted Risk

[Any findings explicitly accepted by the team with rationale]

---

## Dependency Inventory

| Plugin / Library | Version | Source | Known CVEs |
|-----------------|---------|--------|------------|
| [name] | [version] | [source] | [none / CVE-XXXX-NNNN] |

---

## Remediation Priority Order

1. [SEC-NNN] — [1-line description] — Est. effort: [Low/Medium/High]
2. ...

---

## Re-Audit Trigger

Run `/ags-security-audit` again after remediating any CRITICAL or HIGH findings.
The Polish → Release gate requires this report with no open CRITICAL or HIGH items.
```

---

## Phase 6: Write Report

Present the report summary (executive summary + CRITICAL/HIGH findings only) in conversation.

Ask: "May I write the full security audit report to `.ags/project/security/security-audit-[date].md`?"

Write only after approval.

---

## Phase 7: Gate Integration

This report is a required artifact for the **Polish → Release gate**.

After remediating findings, re-run: `/ags-security-audit quick` to confirm CRITICAL/HIGH items are resolved before running `/ags-gate-check release`.

If CRITICAL findings exist:
> "⛔ CRITICAL security findings must be resolved before any public release. Do not proceed to `/ags-launch-checklist` until these are addressed."

If no CRITICAL/HIGH findings:
> "✅ No blocking security findings. Report written to `.ags/project/security/`. Include this path when running `/ags-gate-check release`."

---

## Collaborative Protocol

- **Never assume a pattern is safe** — flag it and let the user decide
- **Accepted risk is a valid outcome** — some LOW findings are acceptable trade-offs for a solo team; document the decision
- **Multiplayer games have a higher bar** — any HIGH finding in a multiplayer context should be treated as CRITICAL
- **This is not a penetration test** — this audit covers common patterns; a real pentest by a human security professional is recommended before any competitive or monetised multiplayer launch

---

## Combined Review Loop (parallel external Codex)

Per `.ags/rules/review-workflow.md`. Audit phases run **in parallel** with external Codex inside one loop. Each iteration:

1. Resolve severity floor: iter 1-2 → keep all severities; iter 3-4 → critical/high; iter 5+ → critical only.
2. **Spawn in one message, in parallel**:
   - All internal reviewer Tasks (technical-director + lead-programmer).
   - `/ags-external-review security [release-branch-or-version-tag] --embedded-parallel --iteration [N] --min-severity [floor]` — Codex scans the release branch / dependency manifest / threat-model notes. Codex unavailable → `skipped: codex-unavailable`; aggregator logs skip in decisions-log and continues with internal pool only.
3. Aggregator (`technical-director`) merges findings from internal + Codex, drops nitpicks + below-floor.
4. **Loop exit**: filtered set empty → emit final verdict. Non-empty → surface aggregated kept findings, user resolves, N++, repeat.

No iteration cap. No user-confirm gate before external — it runs every iteration automatically. Record final iteration count in the verdict report and decisions-log entry. Codex reviews the source code / release artifacts, NOT this audit report.
