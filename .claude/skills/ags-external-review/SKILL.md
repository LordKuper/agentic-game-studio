---
name: ags-external-review
description: "Run external Codex CLI review on a target artifact (ADR, epic, code diff, GDD, security). Claude re-classifies findings by severity; high+ blocks workflow until fixed and re-reviewed. Reports written to .ags/project/reviews/."
argument-hint: "[type: adr|epic|code|gdd|security|custom] [target-path or identifier] [--iteration N]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Edit, AskUserQuestion
---

# External Review (Codex)

Independent second opinion via the `codex` CLI. Used standalone or embedded in gates (`/ags-gate-check epic-done|release`, `/ags-architecture-decision`, `/ags-create-architecture`).

**Why this exists:** internal review is biased by the authoring context. An external tool with no session history catches issues an in-context reviewer normalises away.

**Severity policy:** Codex returns findings with its own severity labels. Claude **re-classifies** every finding using the rubric in Phase 4 — Codex labels are advisory, not authoritative. Findings re-classified `critical` or `high` are **blocking**: workflow stops, user fixes, the same skill is re-run with `--iteration N+1`.

---

## Phase 0: Prerequisites

| Check | If missing |
|---|---|
| `codex` binary on PATH (run `command -v codex` via Bash) | STOP. "Codex CLI not found on PATH. Install Codex CLI before running external review." |
| Type argument is one of `adr|epic|code|gdd|security|custom` | STOP. "Invalid type. Usage: `/ags-external-review adr design/architecture/adr-0001-event-system.md`." |
| Target path/identifier provided (or resolvable from context for `epic`) | STOP. "No target. Provide path or identifier." |
| Target file/dir exists (where applicable) | STOP. "Target `[X]` not found." |
| Prompt template `.ags/templates/external-review/t_prompt-[type].md` exists (skip for `custom`) | STOP. "Prompt template missing for type `[type]`." |

---

## Phase 1: Resolve Target & Iteration

Parse arguments:

- `$1` = type
- `$2` = target (path or epic-slug)
- `--iteration N` (optional, default 1)

Determine **report path**:

```
.ags/project/reviews/[YYYY-MM-DD]-[type]-[slug].md
```

`slug` derivation:
- `adr` → ADR number from filename (e.g. `adr-0001-event-system` → `adr-0001`)
- `epic` → epic slug from `.ags/project/stage.md` Active Epic, or `$2`
- `code` → branch name or PR number, fallback to short hash of target path
- `gdd` → GDD basename without `.md`
- `security` → `release-[version]` or `security-[short-hash]`
- `custom` → user-supplied via `AskUserQuestion`

If report file already exists, this is a re-review iteration — read previous file, append a new `## Iteration N` section. Do not overwrite past iterations.

---

## Phase 2: Build Codex Prompt

Read `.ags/templates/external-review/t_prompt-[type].md`. Substitute placeholders:

- `{{TARGET}}` — target path or identifier
- `{{TARGET_CONTENT}}` — full content of target file(s) (Read; for dirs Glob + Read each, cap at 50 files; for `code` use `git diff` against base branch)
- `{{PROJECT_CONTEXT}}` — short summary built from: `CLAUDE.md` summary line, `.ags/project/stage.md` (phase + active epic), engine + version from `.ags/rules/technical-preferences.md`
- `{{RELATED_DOCS}}` — type-specific context (e.g. for `adr`: linked GDDs from "GDD Requirements Addressed" table; for `epic`: EPIC.md + stories list; for `gdd`: registry entries)
- `{{ITERATION}}` — iteration number; for N>1 also embed previous iteration's findings + user's fix notes

For `custom`, ask user for prompt body and target context inline via `AskUserQuestion`.

Write rendered prompt to `.ags/project/reviews/.tmp/[date]-[type]-[slug]-iter[N]-prompt.md` (gitignored — outside the published report).

---

## Phase 3: Invoke Codex CLI

Run via Bash:

```
codex review --input <prompt-file> --output <raw-output-file> --format json
```

If the `--format json` flag is not supported by the installed Codex version, fall back to plain text and parse heuristically (see Phase 4).

Raw output to `.ags/project/reviews/.tmp/[date]-[type]-[slug]-iter[N]-raw.json` (or `.txt`).

**Failure handling:**
- Non-zero exit → STOP. Show stderr to user. Do not write report. Ask whether to retry or abort.
- Empty output → STOP. "Codex returned no findings — check CLI auth/quota."
- Timeout (>10 min) → STOP. Suggest splitting target.

---

## Phase 4: Re-classify Findings (Claude, not Codex)

Parse Codex output into a list of findings. For each finding extract: title, location (file:line if present), description, Codex-suggested fix, Codex severity label.

**Discard the Codex severity label.** Re-classify each finding using this rubric:

| Severity | Definition |
|---|---|
| **critical** | Security vulnerability, data loss risk, contract break that ships to users, or violation of a registered architectural stance from `docs/registry/architecture.yaml`. Cannot ship. |
| **high** | Bug visible to player, performance budget violation, ADR/GDD inconsistency that will compound, missing test coverage on a Must-Have story, accessibility tier violation. Must fix before close/merge. |
| **medium** | Code smell with measurable risk, style violation breaking project standards (`.ags/rules/coding.md`), incomplete doc-comment on public API, minor perf concern. Should fix. |
| **low** | Nit, opinion, alternative phrasing, redundant comment. May fix. |

**Re-classification rules:**
- A finding with no concrete failure mode = `low`, regardless of Codex label.
- A finding asserting a violation without citing project rule/ADR/test = downgrade by one level.
- A finding citing a registered architectural stance, security CWE, or accessibility WCAG criterion = at least `high`.
- Unverifiable finding (Claude cannot confirm by reading the target) = `low` + flag "unverified".

Record original Codex label and Claude reclassification in the report.

---

## Phase 5: Write Report

Render `.ags/templates/external-review/t_review-report.md` to the resolved report path.

If iteration N>1, **append** new `## Iteration N` section to existing file (Edit, not Write). Top-level summary table (latest verdict) updated in place.

Ask: "May I write external review report to `[path]`?"

---

## Phase 6: Verdict & Block Decision

Compute verdict from re-classified findings:

| Counts | Verdict |
|---|---|
| any `critical` | **BLOCK** |
| any `high` | **BLOCK** |
| no critical/high, any medium | **CONCERNS** |
| only low, or none | **PASS** |

**On BLOCK:**
1. Print blockers list (critical + high) with location + fix suggestion.
2. STOP the calling workflow. If invoked from `/ags-gate-check`, return verdict=FAIL with reason "external review BLOCK". If invoked from `/ags-architecture-decision`, do not proceed to write approval.
3. Tell user: "Fix the [N] blocking findings, then re-run `/ags-external-review [type] [target] --iteration [N+1]`."
4. Exit.

**On CONCERNS:** show medium findings, ask user via `AskUserQuestion`:
- [A] Accept and proceed (record acceptance in report)
- [B] Fix and re-review
- [C] Stop here

**On PASS:** caller may proceed.

---

## Phase 7: Update decisions-log.md

Append entry to `.ags/project/decisions-log.md` only when verdict is PASS or user accepted CONCERNS:

```
## [YYYY-MM-DD HH:MM] — External review: [type] [slug] — [PASS | CONCERNS-ACCEPTED]

**Type**: process
**Context**: External Codex review iteration [N] on [target].
**Decision**: [verdict] — [N findings: C critical, H high, M medium, L low after Claude re-classification].
**Rationale**: [one-line summary]
**Refs**: [report path]
**Decided by**: [user] (verdict from external review + Claude re-classification)
```

BLOCK verdict: do NOT log to decisions-log (the review is not closed yet — only the resolved iteration is logged).

---

## Phase 8: Cleanup

Delete `.ags/project/reviews/.tmp/` files older than 7 days (Bash). Keep current iteration's tmp files until next iteration starts.

---

## Embedded use (called from other skills)

When invoked as a sub-step from `/ags-gate-check`, `/ags-architecture-decision`, `/ags-create-architecture`:

- Caller passes `type`, `target`, optional `--iteration N`.
- This skill returns a structured verdict line: `EXTERNAL-REVIEW: [PASS | CONCERNS | BLOCK] — [report-path]`.
- Caller treats BLOCK as a hard FAIL of its own gate. CONCERNS surfaces to caller's user prompt. PASS is silent.
- No `AskUserQuestion` prompts inside embedded mode beyond the BLOCK fix-and-retry instruction — the calling skill owns the user dialog.

Detect embedded mode via env or arg flag `--embedded` (caller sets it). Default = standalone mode (full prompts).
