---
name: ags-qa-plan
description: "Generate a QA test plan for a sprint or feature. Reads GDDs and story files, classifies stories by test type (Logic/Integration/Visual/UI), and produces a structured test plan covering automated tests required, manual test cases, smoke test scope, and playtest sign-off requirements. Run before sprint begins or when starting a major feature."
argument-hint: "[sprint | feature: system-name | story: path]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, AskUserQuestion
agent: qa-lead
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

# QA Plan

Generates structured QA plan for sprint, feature, or individual story. Reads
in-scope story files and referenced GDDs, classifies by test type, produces
plan telling developers what to automate, what to verify manually, smoke test
scope, and when to playtest.

Run before sprint begins. A test plan written after implementation is a
post-mortem, not a plan.

**Output:** `.ags/project/qa/qa-plan-[sprint-slug]-[date].md`

---

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| Active `.ags/project/epics/[slug]/EPIC.md` | `/ags-create-epics` | STOP. "No active epic. Run `/ags-create-epics`." |
| Stories under `epics/[slug]/stories/*.md` | `/ags-create-stories` | STOP. "No stories. Run `/ags-create-stories [slug]`." |
| Target system GDD(s) | `/ags-design-system` | WARN: limited without GDD acceptance criteria. |

If STOP triggers, exit verdict **BLOCKED**.

---

## Phase 1: Parse Scope

**Argument:** `$ARGUMENTS` (blank = ask user via AskUserQuestion)

Determine scope from the argument:

- **`sprint`** — read the most recent file in `.ags/project/epics/`, extract
  every story file path referenced. If `.ags/project/epics/index.md` exists,
  use it as the primary story list and fall back to the sprint plan for story
  metadata.
- **`feature: [system-name]`** — glob `.ags/project/epics/*/story-*.md`, filter
  to stories whose file path or title contains the system name. Also check the
  epic index file (`EPIC.md`) in that system's directory.
- **`story: [path]`** — validate that the path exists and load that single file.
- **No argument** — use `AskUserQuestion`:
  - "What is the scope for this QA plan?"
  - Options: "Current sprint", "Specific feature (enter system name)",
    "Specific story (enter path)", "Full epic"

After resolving scope, report: "Building QA plan for [N] stories in [scope]."

If a story file path is referenced but the file does not exist, note it as
MISSING and continue with the remaining stories. Do not fail the entire plan
for one missing file.

---

## Phase 2: Load Inputs

For each in-scope story file, read the full file and extract:

- **Story title** and story ID (from filename or header)
- **Story Type** field (if present in the file header — e.g., `Type: Logic`)
- **Acceptance criteria** — the complete numbered/bulleted list
- **Implementation files** — listed under "Files to Create / Modify" or similar
- **Engine notes** — any engine API warnings or version-specific notes
- **GDD reference** — the GDD path(s) cited
- **ADR reference** — the ADR(s) cited
- **Estimate** — hours or story points if present
- **Dependencies** — other stories this one depends on

After reading stories, load supporting context once (not per story):

- `design/gdd/systems-index.md` — to understand system priorities and which
  GDDs are approved
- For each unique GDD referenced across all stories: read only the
  **Acceptance Criteria** and **Formulas** sections. Do not load full GDD text —
  these two sections contain the testable requirements and the math to verify.
- `design/architecture/control-manifest.md` — scan for forbidden patterns that
  automated tests should guard against (if the file exists)

If no GDD is referenced in a story, note it as a gap but do not block the plan.
The story will be classified using acceptance criteria alone.

---

## Phase 3: Classify Each Story

For each story, assign a Story Type. If the story already has a `Type:` field
in its header, use that value and validate it against the criteria below. If the
field is missing or ambiguous, infer the type from the acceptance criteria.

| Story Type | Classification Indicators |
|---|---|
| **Logic** | Acceptance criteria reference calculations, formulas, numerical thresholds, state transitions, AI decisions, data validation, buff/debuff stacking, economy transactions, or any testable computation |
| **Integration** | Criteria involve two or more systems interacting, signals or events propagating across system boundaries, save/load round-trips, network sync, or persistence |
| **Visual/Feel** | Criteria reference animation behaviour, VFX, shader output, "feels responsive", perceived timing, screen shake, particle effects, audio sync, or visual feedback quality |
| **UI** | Criteria reference menus, HUD elements, buttons, screens, dialogue boxes, inventory panels, tooltips, or any player-facing interface element |
| **Config/Data** | Changes are limited to balance tuning values, data files, or configuration — no new code logic is involved |

**Mixed stories** (e.g., a story that adds both a formula and a UI display):
assign the primary type based on which acceptance criteria carry the highest
implementation risk, and note the secondary type. Mixed Logic+Integration or
Visual+UI combinations are the most common.

After classifying all stories, produce a classification summary table in
conversation before proceeding to Phase 4. This gives the user visibility into
how tests will be allocated.

---

## Phase 4: Generate Test Plan

Assemble the full QA plan document. Use this structure:

````markdown
# QA Plan: [Sprint/Feature Name]
**Date**: [date]
**Generated by**: /ags-qa-plan
**Scope**: [N stories across [N systems]]
**Engine**: [engine name from .ags/rules/technical-preferences.md, or "Not configured"]
**Sprint File**: [path to sprint plan if applicable]

---

## Test Summary

| Story | Type | Automated Test Required | Manual Verification Required |
|-------|------|------------------------|------------------------------|
| [story title] | Logic | Unit test — `tests/unit/[system]/` | None |
| [story title] | Integration | Integration test — `tests/integration/[system]/` | Smoke check |
| [story title] | Visual/Feel | None (not automatable) | Screenshot + lead sign-off |
| [story title] | UI | Interaction walkthrough | Manual step-through |
| [story title] | Config/Data | Data validation test | Spot-check in-game values |

---

## Automated Tests Required

### [Story Title] — [Type]
**Test file path**: `tests/[unit|integration]/[system]/[story-slug]_test.[ext]`
**What to test**:
- [Specific formula or rule from the GDD Formulas section]
- [Each named state transition or decision branch]
- [Each side effect that should or should not occur]

**Edge cases to cover**:
- Zero/minimum input values (e.g., 0 damage, empty inventory)
- Maximum/boundary input values (e.g., max level, stat cap)
- Invalid or null input (e.g., missing target, dead entity)
- [Any edge case explicitly called out in the GDD Edge Cases section]

**Estimated test count**: ~[N] unit tests

[If no GDD formula reference was found for this story, note:]
*No formula found in referenced GDD — test cases must be derived from acceptance
criteria directly. Review the GDD Formulas section before writing tests.*

---

## Manual QA Checklist

### [Story Title] — [Type]
**Verification method**: [Screenshot + designer sign-off | Playtest session |
Manual step-through | Comparison against reference footage]
**Who must sign off**: [designer / lead-programmer / qa-lead / art-lead]
**Evidence to capture**: [screenshot of X | video clip of Y | written playtest
notes | side-by-side comparison]

Checklist:
- [ ] [Specific observable condition — concrete and falsifiable]
- [ ] [Another condition]
- [ ] [Every acceptance criterion translated into a manual check item]

*If any criterion uses subjective language ("feels", "looks", "seems"), it must
be supplemented with a specific benchmark or a playtest protocol note.*

---

## Smoke Test Scope

Critical paths to verify before any QA hand-off for this sprint:

1. Game launches to main menu without crash
2. New game / new session can be started
3. [Primary mechanic introduced or changed this sprint]
4. [Any system with a regression risk from this sprint's changes]
5. Save / load cycle completes without data loss (if save system exists)
6. Performance is within budget on target hardware (no new frame spikes)

*Smoke tests are verified by the developer via `/ags-smoke-check`. Reference this
list when running that skill.*

---

## Playtest Requirements

| Story | Playtest Goal | Min Sessions | Target Player Type |
|-------|--------------|--------------|-------------------|
| [story] | [What question must the session answer?] | [N] | [new player / experienced] |

**Sign-off requirement**: Playtest notes must be written to
`.ags/project/session-logs/playtest-[sprint]-[story-slug].md` and reviewed by
the [designer / qa-lead] before the story can be marked COMPLETE.

If no stories require playtest validation: *No playtest sessions required for
this sprint.*

---

## Definition of Done — This Sprint

A story is DONE when ALL of the following are true:

- [ ] All acceptance criteria verified — via automated test result OR documented
      manual evidence (screenshot, video, or playtest notes with sign-off)
- [ ] Test file exists at the specified path for all Logic and Integration stories
- [ ] Manual evidence document exists for all Visual/Feel and UI stories
- [ ] Smoke check passes (run `/ags-smoke-check sprint` before QA hand-off)
- [ ] No regressions introduced
- [ ] Code reviewed (via `/ags-code-review` or documented peer review)
- [ ] Story file updated to `Status: Complete` (via `/ags-story-done`)
````

When generating content, use the actual story titles, GDD formula text, and
acceptance criteria extracted in Phase 2. Do not use placeholder text — every
test entry should reflect the real requirements of these specific stories.

---

## Phase 4b: Internal Review Loop

Spawn `qa-lead` (self-review) and `producer` via Task to review the drafted plan against project pillars, milestone scope, accessibility tier, and recent bug history.

**Loop exit condition.** Single iteration where every spawned reviewer returns clean (no critical/high/medium findings). Non-clean → user revises affected sections of the plan, re-spawn reviewers. No iteration cap.

Record iteration count.

## Combined Review Loop (parallel external Codex)

Per `.ags/rules/review-workflow.md`. The internal review section above runs **in parallel** with external Codex inside one loop. Each iteration:

1. Resolve severity floor: iter 1-2 → keep all severities; iter 3-4 → critical/high; iter 5+ → critical only.
2. Persist current draft to `.ags/project/reviews/.tmp/[type]-[slug]-iter[N]-draft.md`.
3. **Spawn in one message, in parallel** (multiple Task calls + one Bash invocation):
   - All internal reviewer Tasks listed above.
   - `/ags-external-review [type] [draft-path] --embedded-parallel --iteration [N] --min-severity [floor]` — Codex unavailable returns `skipped: codex-unavailable`; aggregator logs skip in decisions-log and continues with internal pool only.
4. Aggregator (`producer` by default; skill-designated lead where the skill specifies one) merges findings from internal + external, drops nitpicks + below-floor.
5. **Loop exit**: filtered set empty → proceed to write approval. Non-empty → surface aggregated kept findings, user revises draft, N++, repeat.

No iteration cap. No user-confirm gate before external — it runs every iteration automatically. Record final iteration count for the decisions-log entry written at skill completion.

---

## Phase 5: Write Output

Show the complete plan in conversation (or a summary if the plan is very long),
then ask:

"May I write this QA plan to `.ags/project/qa/qa-plan-[sprint-slug]-[date].md`?"

Write the plan exactly as generated — do not truncate.

After writing:

"QA plan written to `.ags/project/qa/qa-plan-[sprint-slug]-[date].md`.

Next steps:
- Share this plan with the team before sprint implementation begins
- Run `/ags-smoke-check sprint` after all stories are implemented to gate QA hand-off
- For Logic/Integration stories, create the test files at the listed paths
  before marking stories done — `/ags-story-done` checks for them"

---

## Collaborative Protocol

- **Never write without asking** — Phase 5 requires explicit approval
- **Classify conservatively** — ambiguous Logic vs Integration → classify as Integration
- **No invented test cases** — only from acceptance criteria and GDD formulas; flag absent formulas
- **Playtest requirements advisory** — user decides; flag borderline Visual/Feel, don't mandate
- Use `AskUserQuestion` for scope selection (no argument). All other phases non-interactive — present findings, ask once to approve write.
