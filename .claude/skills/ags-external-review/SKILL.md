---
name: ags-external-review
description: "Internal-only: parallel Codex CLI reviewer invoked by authoring and review-verdict skills inside their Combined Review Loop. Not user-invocable. Returns structured findings JSON to the caller's aggregator; no verdict, no block. Codex unavailable returns {skipped: codex-unavailable}; loop continues with internal pool only."
argument-hint: "[type: adr|epic|code|gdd|concept|art-bible|design-system|ux|systems-index|control-manifest|story|contracts|qa-plan|asset-spec|localize|release-checklist|security|custom] [target-path or identifier] [--iteration N] [--min-severity critical|high|medium|low] --embedded-parallel"
user-invocable: false
allowed-tools: Read, Glob, Grep, Bash, Write, Edit, AskUserQuestion
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

# External Review (Codex) — parallel reviewer

Independent second opinion via the `codex` CLI. **Parallel-only**: spawned by authoring and review-verdict skills inside their Combined Review Loop, alongside internal reviewers. Returns structured findings to caller's aggregator — no verdict, no block, no user dialog.

**Why this exists:** internal review is biased by the authoring context. An external tool with no session history catches issues an in-context reviewer normalises away.

**Severity policy:** Codex returns findings with its own severity labels. Claude **re-classifies** every finding using the rubric in Phase 4 — Codex labels are advisory. This skill returns the re-classified findings; caller aggregator applies its own severity floor.

**No-nitpick rule:** Reviewer Guidance block from `.ags/rules/director-gates.md` § Reviewer Guidance is pasted into the Codex prompt. Substantive findings only. See `.ags/rules/review-workflow.md`.

**Document Boundary Check:** For doc-review types (ADR, GDD, concept, art-bible, design-system, UX, systems-index, control-manifest), the prompt also includes the Boundary Addendum from `.ags/rules/review-workflow.md` § Document Boundary Check — Codex flags violations of `.ags/rules/document-boundaries.md` as `high`-severity substantive findings.

---

## Phase 0: Prerequisites

| Check | If missing |
|---|---|
| `codex` binary on PATH (run `command -v codex` via Bash) | Return immediately with `{kept: [], dropped: [], skipped: "codex-unavailable", report_path: null}` so caller logs skip and continues with internal pool only. **Do NOT stop.** |
| Type argument is one of `adr|epic|code|gdd|concept|art-bible|design-system|ux|systems-index|control-manifest|story|contracts|qa-plan|asset-spec|localize|release-checklist|security|custom` | Return error JSON `{kept: [], dropped: [], skipped: "invalid-type", report_path: null}`. |
| Target path/identifier provided | Return error JSON `{kept: [], dropped: [], skipped: "no-target", report_path: null}`. |
| Target file/dir exists (where applicable) | Return error JSON `{kept: [], dropped: [], skipped: "target-not-found", report_path: null}`. |
| Prompt template `.ags/templates/external-review/t_prompt-[type].md` exists (skip for `custom`) | Return error JSON `{kept: [], dropped: [], skipped: "template-missing", report_path: null}`. |
| `--embedded-parallel` flag present | Return error JSON `{kept: [], dropped: [], skipped: "user-invocation-not-allowed", report_path: null}`. This skill is internal-only. |

---

## Phase 1: Resolve Target & Iteration

Parse arguments:

- `$1` = type
- `$2` = target (path or epic-slug)
- `--iteration N` (default 1)
- `--min-severity [critical|high|medium|low]` — required from caller. If absent, derive from iteration: N≤2 → `low`; N=3..4 → `high`; N≥5 → `critical`. Codex prompt is told to omit findings below this floor; Phase 4 still re-classifies and Phase 4b still drops.
- `--embedded-parallel` — mandatory mode flag.

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
- `story` → story basename without `.md`, prefix with epic slug if available: `[epic-slug]--[story]`
- `contracts` → `[epic-slug]-contracts`
- `qa-plan` → QA plan basename without `.md`
- `asset-spec` → asset-spec basename without `.md`
- `localize` → `localize` or locale-bundle basename
- `release-checklist` → `release-[version]` or checklist basename
- `security` → `release-[version]` or `security-[short-hash]`
- `custom` → caller-supplied

If report file already exists, this is a re-review iteration — read previous file, append a new `## Iteration N (parallel)` section. Do not overwrite past iterations.

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
  - `custom` → caller-supplied via the calling skill.
- `{{ITERATION}}` — iteration number; for N>1 also embed previous iteration's findings + fix notes
- `{{PRIOR_FINDINGS}}` — for N>1, the previous iteration's re-classified findings table; empty for N=1
- `{{SEVERITY_FLOOR}}` — resolved severity floor (`critical` / `high` / `medium` / `low`)
- `{{REVIEWER_GUIDANCE}}` — paste the Reviewer Guidance block from `.ags/rules/director-gates.md` § Reviewer Guidance verbatim, with `{{ITERATION}}` and `{{SEVERITY_FLOOR}}` substituted
- `{{BOUNDARY_ADDENDUM}}` — for doc-review types (`adr`, `gdd`, `concept`, `art-bible`, `design-system`, `ux`, `systems-index`, `control-manifest`), paste the Reviewer prompt addendum from `.ags/rules/review-workflow.md` § Document Boundary Check. Skip for `code` / `security` / `release-checklist` / `qa-plan` / `asset-spec` / `localize` / `epic` / `story` / `contracts` / `custom`.

For `custom`, the calling skill supplies prompt body and target context.

Write rendered prompt to `.ags/project/reviews/.tmp/[date]-[type]-[slug]-iter[N]-prompt.md` (gitignored).

---

## Phase 3: Invoke Codex CLI

Run via Bash:

```
codex review --input <prompt-file> --output <raw-output-file> --format json
```

If the `--format json` flag is not supported by the installed Codex version, fall back to plain text and parse heuristically (see Phase 4).

Raw output to `.ags/project/reviews/.tmp/[date]-[type]-[slug]-iter[N]-raw.json` (or `.txt`).

**Failure handling (no caller dialog):**
- Non-zero exit → return `{kept: [], dropped: [], skipped: "codex-error", report_path: null, error: "[stderr]"}`. Caller logs in decisions-log and continues with internal pool only.
- Empty output → return `{kept: [], dropped: [], skipped: "codex-empty", report_path: null}`.
- Timeout (>10 min) → return `{kept: [], dropped: [], skipped: "codex-timeout", report_path: null}`.

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

Record raw count and dropped count in the returned JSON.

---

## Phase 5: Append iteration block to report

Append a compressed `## Iteration N (parallel)` block (severity floor used, kept count, dropped count by reason, kept findings table only) to the resolved report path. Use Edit (file may not exist yet — Write on first iteration, Edit thereafter). No user write-confirmation — caller's aggregator owns dialog.

If report file does not exist, create it with the template header from `.ags/templates/external-review/t_review-report.md` plus the first `## Iteration N (parallel)` block.

---

## Phase 6: Return JSON to caller

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

Caller's aggregator merges kept findings with internal-reviewer findings and decides loop exit. This skill never writes to `decisions-log.md` — parent skill records its own loop-completion entry.

---

## Phase 7: Cleanup

Delete `.ags/project/reviews/.tmp/` files older than 7 days (Bash). Keep current iteration's tmp files until next iteration starts.

---

## Caller contract

Called by every authoring and review-verdict skill within its Combined Review Loop:

- Caller spawns this skill in parallel with internal reviewer Tasks within one loop iteration.
- Caller passes `type`, `target` (path to draft for authoring, or path to source artifact for review-verdict), `--iteration N`, `--min-severity [floor]`, `--embedded-parallel`.
- This skill **does not block, does not dialog, does not write decisions-log**. Phase 5 writes a compressed iteration block to the report file; Phase 6 returns JSON.
- `codex` not on PATH → returns `{kept: [], dropped: [], skipped: "codex-unavailable", report_path: null}` immediately. Caller logs skip in decisions-log and proceeds with internal pool only.
