---
name: ags-external-review
description: "Run external Codex CLI review on a target artifact (ADR, epic, code diff, GDD, concept, art-bible, design-system, ux, systems-index, control-manifest, story, contracts, qa-plan, asset-spec, localize, release-checklist, security). Claude re-classifies findings by severity; high+ blocks workflow until fixed and re-reviewed. Reports written to .ags/project/reviews/."
argument-hint: "[type: adr|epic|code|gdd|concept|art-bible|design-system|ux|systems-index|control-manifest|story|contracts|qa-plan|asset-spec|localize|release-checklist|security|custom] [target-path or identifier] [--iteration N] [--min-severity critical|high|medium|low] [--embedded | --embedded-parallel]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Edit, AskUserQuestion
---

# External Review (Codex)

Independent second opinion via the `codex` CLI. Three invocation modes:

- **Standalone** — user runs `/ags-external-review`, full verdict + dialog.
- **`--embedded`** — called from a standalone gate (`/ags-gate-check epic-done|release`). Returns `EXTERNAL-REVIEW: PASS|CONCERNS|BLOCK — [report]` verdict line. Caller treats BLOCK as gate FAIL.
- **`--embedded-parallel`** — called by a generator skill from inside its combined review loop, in parallel with internal reviewers. Returns structured findings JSON `{kept: [...], dropped: [...], skipped: null|"codex-unavailable"}`. **No verdict, no block** — the skill's aggregator decides.

**Why this exists:** internal review is biased by the authoring context. An external tool with no session history catches issues an in-context reviewer normalises away.

**Severity policy:** Codex returns findings with its own severity labels. Claude **re-classifies** every finding using the rubric in Phase 4 — Codex labels are advisory, not authoritative. In standalone / `--embedded` modes, findings re-classified `critical` or `high` are **blocking**. In `--embedded-parallel`, this skill returns findings without blocking — caller aggregator applies its own severity floor.

**No-nitpick rule:** This skill always paste the Reviewer Guidance block from `.ags/rules/director-gates.md` § Reviewer Guidance into the Codex prompt. Substantive findings only. See `.ags/rules/review-workflow.md`.

---

## Phase 0: Prerequisites

| Check | If missing |
|---|---|
| `codex` binary on PATH (run `command -v codex` via Bash) | Standalone / `--embedded`: STOP. "Codex CLI not found on PATH. Install Codex CLI before running external review." `--embedded-parallel`: do NOT stop — return immediately with `{kept: [], dropped: [], skipped: "codex-unavailable", report_path: null}` so caller logs skip and continues with internal pool only. |
| Type argument is one of `adr|epic|code|gdd|concept|art-bible|design-system|ux|systems-index|control-manifest|story|contracts|qa-plan|asset-spec|localize|release-checklist|security|custom` | STOP. "Invalid type. Usage: `/ags-external-review adr design/architecture/adr-0001-event-system.md`." |
| Target path/identifier provided (or resolvable from context for `epic`) | STOP. "No target. Provide path or identifier." |
| Target file/dir exists (where applicable) | STOP. "Target `[X]` not found." |
| Prompt template `.ags/templates/external-review/t_prompt-[type].md` exists (skip for `custom`) | STOP. "Prompt template missing for type `[type]`." |

---

## Phase 1: Resolve Target & Iteration

Parse arguments:

- `$1` = type
- `$2` = target (path or epic-slug)
- `--iteration N` (optional, default 1)
- `--min-severity [critical|high|medium|low]` (optional). Default derived from iteration when not passed: N≤2 → `low`; N=3..4 → `high`; N≥5 → `critical`. The Codex prompt is told to omit findings below this floor; Phase 4 still re-classifies and Phase 4b still drops.
- `--embedded` — caller is a standalone gate. Returns verdict line `EXTERNAL-REVIEW: PASS|CONCERNS|BLOCK — [report-path]`.
- `--embedded-parallel` — caller is a generator skill running this in parallel with internal reviewers. **No verdict, no block.** Returns structured JSON `{kept: [...findings...], dropped: [...], skipped: null|"codex-unavailable", report_path: [...]}`. Skip Phases 5-7 (no report write, no decisions-log entry, no verdict dialog) — caller's aggregator owns the dialog. Append the iteration to the report file in compressed form (one block under `## Iteration N (parallel)`) so the report still accumulates history; aggregator may amend or finalise it.

Determine **report path**:

```
.ags/project/reviews/[YYYY-MM-DD]-[type]-[slug].md
```

`slug` derivation:
- `adr` → ADR number from filename (e.g. `adr-0001-event-system` → `adr-0001`)
- `epic` → epic slug from `.ags/project/stage.md` Active Epic, or `$2`
- `code` → branch name or PR number, fallback to short hash of target path
- `gdd` → GDD basename without `.md`
- `concept` → `game-concept` (single artifact)
- `art-bible` → `art-bible` (single artifact)
- `design-system` → DESIGN.md basename without `.md` (e.g. `DESIGN`, `DESIGN-marketing`)
- `ux` → UX doc basename without `.md` (e.g. `hud`, `inventory-flow`)
- `systems-index` → `systems-index` (single artifact)
- `control-manifest` → `control-manifest` (single artifact)
- `story` → story basename without `.md` (e.g. `story-001-spawn-enemy`), prefix with epic slug if available: `[epic-slug]--[story]`
- `contracts` → `[epic-slug]-contracts`
- `qa-plan` → QA plan basename without `.md`
- `asset-spec` → asset-spec basename without `.md`
- `localize` → `localize` or locale-bundle basename
- `release-checklist` → `release-[version]` or checklist basename
- `security` → `release-[version]` or `security-[short-hash]`
- `custom` → user-supplied via `AskUserQuestion`

If report file already exists, this is a re-review iteration — read previous file, append a new `## Iteration N` section. Do not overwrite past iterations.

---

## Phase 2: Build Codex Prompt

Read `.ags/templates/external-review/t_prompt-[type].md`. Substitute placeholders:

- `{{TARGET}}` — target path or identifier
- `{{TARGET_CONTENT}}` — full content of target file(s) (Read; for dirs Glob + Read each, cap at 50 files; for `code` use `git diff` against base branch)
- `{{PROJECT_CONTEXT}}` — short summary built from: `CLAUDE.md` summary line, `.ags/project/stage.md` (phase + active epic), engine + version from `.ags/rules/technical-preferences.md`
- `{{RELATED_DOCS}}` — type-specific context. Per type:
  - `adr` → linked GDDs from "GDD Requirements Addressed" table; existing ADRs in same domain; relevant `docs/registry/architecture.yaml` stances.
  - `epic` → EPIC.md + stories list + epic contracts + linked ADRs.
  - `gdd` → game-concept.md summary, related GDDs cited in Dependencies, registry entries for shared entities.
  - `concept` → pillars file (if separate), narrative direction notes, reference titles.
  - `art-bible` → game-concept Visual Identity Anchor, DESIGN.md token summary, pinned engine renderer.
  - `design-system` → art bible UI Visual Language section, lint output (run `npx @google/design.md lint` and capture), pinned engine UI framework.
  - `ux` → game-concept, control-manifest, DESIGN.md tokens summary, accessibility tier from `design/accessibility-requirements.md`, related GDDs.
  - `systems-index` → game-concept, pillars, milestone scope from producer plan, prior systems-index versions if any.
  - `control-manifest` → game-concept, target platforms, accessibility tier, engine input system, related UX specs.
  - `story` → parent EPIC.md, cited ADRs, cited GDDs, contracts, prior stories in same epic, `.ags/project/stubs.md` rows touched.
  - `contracts` → EPIC.md, related ADRs, `.ags/project/stubs.md`, owner-epics, registry stances.
  - `qa-plan` → game-concept, in-scope GDDs / ADRs, target platforms, accessibility tier, recent bug history from `.ags/project/bugs/`.
  - `asset-spec` → art bible, DESIGN.md tokens, pinned engine renderer, technical-preferences budgets, related GDDs.
  - `localize` → target locale list, font fallback config, UI specs, accessibility tier, dialogue / writing samples.
  - `release-checklist` → target platforms with cert profiles, QA sign-off status, build pipeline, store metadata, prior changelog/patch-notes.
  - `code` → diff context (base branch), changed files list.
  - `security` → release branch / version tag, threat-model notes, dependency manifest.
  - `custom` → user-supplied via `AskUserQuestion`.
- `{{ITERATION}}` — iteration number; for N>1 also embed previous iteration's findings + user's fix notes
- `{{PRIOR_FINDINGS}}` — for N>1, the previous iteration's re-classified findings table; empty for N=1
- `{{SEVERITY_FLOOR}}` — resolved severity floor (`critical` / `high` / `medium` / `low`)
- `{{REVIEWER_GUIDANCE}}` — paste the Reviewer Guidance block from `.ags/rules/director-gates.md` § Reviewer Guidance verbatim, with `{{ITERATION}}` and `{{SEVERITY_FLOOR}}` substituted

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

### Phase 4b: Drop nitpicks + apply severity floor

After re-classification, walk every finding and tag with `dropped` reason (or keep) per `.ags/rules/review-workflow.md`:

- `dropped: nitpick` — wording polish, alt phrasing without rule cite, opinion-only style, redundant comment, formatting micro-fix where existing form is valid, "could also do X" alternatives without showing current approach is wrong.
- `dropped: below-floor` — re-classified severity is below `--min-severity` floor (resolved per Phase 1).
- `kept` — substantive finding at or above floor.

Record both raw count and dropped count in report (or in returned JSON for `--embedded-parallel`).

---

## Phase 5: Write Report

Render `.ags/templates/external-review/t_review-report.md` to the resolved report path.

If iteration N>1, **append** new `## Iteration N` section to existing file (Edit, not Write). Top-level summary table (latest verdict) updated in place.

In `--embedded-parallel` mode: append a compressed `## Iteration N (parallel)` block (severity floor used, kept count, dropped count by reason, kept findings table only) and skip the user write-confirmation question — caller's aggregator owns the dialog.

Standalone / `--embedded`: ask "May I write external review report to `[path]`?"

---

## Phase 6: Verdict & Block Decision

**Skip entirely in `--embedded-parallel` mode.** Return JSON to caller:

```json
{
  "kept": [ ...findings... ],
  "dropped": [ {"finding": ..., "reason": "nitpick" | "below-floor"} ],
  "skipped": null,
  "report_path": ".ags/project/reviews/...",
  "iteration": N,
  "min_severity": "..."
}
```

Caller's aggregator decides whether the loop continues.

Standalone / `--embedded` modes — compute verdict as below from kept (post Phase 4b) findings:

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

**Skip in `--embedded-parallel` mode** — the parent generator skill writes its own decisions-log entry at completion (covering the whole combined-review loop).

Standalone / `--embedded`: append entry to `.ags/project/decisions-log.md` only when verdict is PASS or user accepted CONCERNS:

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

### `--embedded` (standalone gate sub-step)

Used by `/ags-gate-check epic-done|release` where external review is its own gate.

- Caller passes `type`, `target`, optional `--iteration N`, optional `--min-severity`.
- This skill returns a structured verdict line: `EXTERNAL-REVIEW: [PASS | CONCERNS | BLOCK] — [report-path]`.
- Caller treats BLOCK as hard FAIL of its own gate. CONCERNS surfaces to caller's user prompt. PASS is silent.
- No `AskUserQuestion` prompts inside embedded mode beyond the BLOCK fix-and-retry instruction — the calling skill owns the user dialog.

### `--embedded-parallel` (generator skill combined-review loop)

Used by every document-generating skill (`/ags-architecture-decision`, `/ags-create-epics`, `/ags-design-system`, `/ags-art-bible`, `/ags-ux-design`, `/ags-qa-plan`, `/ags-asset-spec`, `/ags-localize`, `/ags-release-checklist`, `/ags-launch-checklist`, `/ags-day-one-patch`, `/ags-epic-contracts`, `/ags-create-stories`, `/ags-dev-story`, `/ags-map-systems`, `/ags-brainstorm`, `/ags-create-architecture`, `/ags-create-control-manifest`).

- Caller spawns this skill in parallel with internal reviewer Tasks within one loop iteration.
- Caller passes `type`, `target` (path to draft, possibly under `.ags/project/reviews/.tmp/`), `--iteration N`, `--min-severity [floor]`.
- This skill **does not block, does not dialog, does not write decisions-log**. Skips Phase 6, 7. Phase 5 writes a compressed iteration block to the report file.
- Returns JSON (see Phase 6) — `kept` is the list of substantive findings at or above the floor; caller's aggregator merges with internal-reviewer findings and decides loop exit.
- `codex` not on PATH → returns `{kept: [], dropped: [], skipped: "codex-unavailable", report_path: null}` immediately. Caller logs skip in decisions-log and proceeds with internal pool only.

Detect mode via arg flag `--embedded` or `--embedded-parallel`. Default = standalone mode (full prompts, full verdict, full dialog).
