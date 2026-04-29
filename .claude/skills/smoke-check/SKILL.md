---
name: smoke-check
description: "Run the critical path smoke test gate before QA hand-off. Executes the automated test suite, verifies core functionality, and produces a PASS/FAIL report. Run after a sprint's stories are implemented and before manual QA begins. A failed smoke check means the build is not ready for QA."
argument-hint: "[sprint | quick | --platform pc|console|mobile|all]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, AskUserQuestion
---

# Smoke Check

This skill is the gate between "implementation done" and "ready for QA
hand-off". It runs the automated test suite, checks for test coverage gaps,
batch-verifies critical paths with the developer, and produces a PASS/FAIL
report.

The rule is simple: **a build that fails smoke check does not go to QA.**
Handing a broken build to QA wastes their time and demoralises the team.

**Output:** `.ags/project/qa/smoke-[date].md`

---

## Parse Arguments

Arguments can be combined: `/smoke-check sprint --platform console`

**Base mode** (first argument, default: `sprint`):
- `sprint` вЂ” full smoke check against the current sprint's stories
- `quick` вЂ” skip coverage scan (Phase 3) and Batch 3; use for rapid re-checks

**Platform flag** (`--platform`, default: none):
- `--platform pc` вЂ” add PC-specific checks (keyboard, mouse, windowed mode)
- `--platform console` вЂ” add console-specific checks (gamepad, TV safe zones,
  platform certification requirements)
- `--platform mobile` вЂ” add mobile-specific checks (touch, portrait/landscape,
  battery/thermal behaviour)
- `--platform all` вЂ” add all platform variants; output per-platform verdict table

If `--platform` is provided, Phase 4 adds platform-specific batches and
Phase 5 outputs a per-platform verdict table in addition to the overall verdict.

---

## Phase 1: Detect Test Setup

Before running anything, understand the environment:

1. **Test framework check**: verify `tests/` directory exists.
   If it does not: "No test directory found at `tests/`. Run `/test-setup`
   to scaffold the testing infrastructure, or create the directory manually
   if tests live elsewhere." Then stop.

2. **CI check**: check whether `.github/workflows/` contains a workflow file
   referencing tests. Note in the report whether CI is configured.

3. **Engine detection**: read `.ags/rules/technical-preferences.md` and
   extract the `Engine:` value. Store this for test command selection in
   Phase 2.

4. **Smoke test list**: check whether `.ags/project/qa/smoke-tests.md` or
   `tests/smoke/` exists. If a smoke test list is found, load it for use in
   Phase 4. If neither exists, smoke tests will be drawn from the current QA
   plan (Phase 4 fallback).

5. **QA plan check**: glob `.ags/project/qa/qa-plan-*.md` and take the most
   recently modified file. If found, note the path вЂ” it will be used in
   Phase 3 and Phase 4. If not found, note: "No QA plan found. Run
   `/qa-plan sprint` before smoke-checking for best results."

Report findings before proceeding: "Environment: [engine]. Test directory:
[found / not found]. CI configured: [yes / no]. QA plan: [path / not found]."

---

## Phase 2: Run Automated Tests

Attempt to run the test suite via Bash. Select the command based on the engine
detected in Phase 1:

**Unity:**
Unity tests require the editor and cannot be run headlessly via shell in most
environments. Check for recent test result artifacts:
```bash
ls -t test-results/ 2>/dev/null | head -5
```
If test result files exist (XML or JSON), read the most recent one and parse
PASS/FAIL counts. If no artifacts exist: "Unity tests must be run from the
editor or CI pipeline. Please confirm test status manually before proceeding."

**Unknown engine / not configured:**
"Engine not configured in `.ags/rules/technical-preferences.md`. Run
`/setup-engine` to specify the engine, then re-run `/smoke-check`."

**If the test runner is not available in this environment** (engine binary not
on PATH, runner script not found, etc.), report clearly:

"Automated tests could not be executed вЂ” engine binary not found on PATH.
Status will be recorded as NOT RUN. Confirm test results from your local IDE
or CI pipeline. Unconfirmed NOT RUN is treated as PASS WITH WARNINGS, not
FAIL вЂ” the developer must manually confirm results."

Do not treat NOT RUN as an automatic FAIL. Record it as a warning. The
developer's manual confirmation in Phase 4 can resolve it.

Parse runner output and extract:
- Total tests run
- Passing count
- Failing count
- Names of any failing tests (up to 10; if more, note the count)
- Any crash or error output from the runner itself

---

## Phase 3: Check Test Coverage

Draw the story list from, in priority order:
1. The QA plan found in Phase 1 (its Test Summary table lists expected test
   file paths per story)
2. The current sprint plan from `.ags/project/sprints/` (most recently modified
   file)
3. If the `quick` argument was passed, skip this phase entirely and note:
   "Coverage scan skipped вЂ” run `/smoke-check sprint` for full coverage
   analysis."

For each story in scope:

1. Extract the system slug from the story's file path
   (e.g., `.ags/project/epics/combat/story-001.md` в†’ `combat`)
2. Glob `tests/unit/[system]/` and `tests/integration/[system]/` for files
   whose name contains the story slug or a closely related term
3. Check the story file itself for a `Test file:` header field or a
   "Test Evidence" section

Assign a coverage status to each story:

| Status | Meaning |
|--------|---------|
| **COVERED** | A test file was found matching this story's system and scope |
| **MANUAL** | Story type is Visual/Feel or UI; a test evidence document was found |
| **MISSING** | Logic or Integration story with no matching test file |
| **EXPECTED** | Config/Data story вЂ” no test file required; spot-check is sufficient |
| **UNKNOWN** | Story file missing or unreadable |

MISSING entries are advisory gaps. They do not cause a FAIL verdict but must
appear prominently in the report and must be resolved before `/story-done` can
fully close those stories.

---

## Phase 4: Run Manual Smoke Checks

Draw the smoke test checklist from, in priority order:
1. The QA plan's "Smoke Test Scope" section (if QA plan was found in Phase 1)
2. `.ags/project/qa/smoke-tests.md` (if it exists)
3. `tests/smoke/` directory contents (if it exists)
4. The standard fallback list below (used only when none of the above exist)

Tailor batches 2 and 3 to the actual systems identified from the sprint or QA
plan. Replace bracketed placeholders with real mechanic names from the current
sprint's stories.

Use `AskUserQuestion` to batch-verify. Keep to at most 3 calls.

**Batch 1 вЂ” Core stability (always run):**
```
question: "Smoke check вЂ” Batch 1: Core stability. Please verify each:"
options:
  - "Game launches to main menu without crash вЂ” PASS"
  - "Game launches to main menu without crash вЂ” FAIL"
  - "New game / session starts successfully вЂ” PASS"
  - "New game / session starts successfully вЂ” FAIL"
  - "Main menu responds to all inputs вЂ” PASS"
  - "Main menu responds to all inputs вЂ” FAIL"
```

**Batch 2 вЂ” Sprint mechanic and regression (always run):**
```
question: "Smoke check вЂ” Batch 2: This sprint's changes and regression check:"
options:
  - "[Primary mechanic this sprint] вЂ” PASS"
  - "[Primary mechanic this sprint] вЂ” FAIL: [describe what broke]"
  - "[Second notable change this sprint, if any] вЂ” PASS"
  - "[Second notable change this sprint] вЂ” FAIL"
  - "Previous sprint's features still work (no regressions) вЂ” PASS"
  - "Previous sprint's features вЂ” regression found: [brief description]"
```

**Batch 3 вЂ” Data integrity and performance (run unless `quick` argument):**
```
question: "Smoke check вЂ” Batch 3: Data integrity and performance:"
options:
  - "Save / load completes without data loss вЂ” PASS"
  - "Save / load вЂ” FAIL: [describe what broke]"
  - "Save / load вЂ” N/A (save system not yet implemented)"
  - "No new frame rate drops or hitches observed вЂ” PASS"
  - "Frame rate drops or hitches found вЂ” FAIL: [where]"
  - "Performance вЂ” not checked in this session"
```

Record each response verbatim for the Phase 5 report.

**Platform Batches** *(run only if `--platform` argument was provided)*:

**PC platform** (`--platform pc` or `--platform all`):
```
question: "Smoke check вЂ” PC Platform: Verify platform-specific behaviour:"
options:
  - "Keyboard controls work correctly across all menus and gameplay вЂ” PASS"
  - "Keyboard controls вЂ” FAIL: [describe issue]"
  - "Mouse input and cursor visibility correct in all states вЂ” PASS"
  - "Mouse input вЂ” FAIL: [describe issue]"
  - "Windowed and fullscreen modes function without graphical issues вЂ” PASS"
  - "Windowed/fullscreen вЂ” FAIL: [describe issue]"
  - "Resolution changes apply correctly вЂ” PASS"
  - "Resolution changes вЂ” FAIL: [describe issue]"
```

**Console platform** (`--platform console` or `--platform all`):
```
question: "Smoke check вЂ” Console Platform: Verify platform-specific behaviour:"
options:
  - "Gamepad input works correctly for all actions вЂ” PASS"
  - "Gamepad input вЂ” FAIL: [describe issue]"
  - "UI fits within TV safe zone margins (no text clipped) вЂ” PASS"
  - "TV safe zone вЂ” FAIL: [describe what is clipped]"
  - "No keyboard/mouse-only fallbacks shown to gamepad user вЂ” PASS"
  - "Input prompt inconsistency вЂ” FAIL: [describe]"
  - "Game boots correctly from cold start (no prior save) вЂ” PASS"
  - "Cold start вЂ” FAIL: [describe issue]"
```

**Mobile platform** (`--platform mobile` or `--platform all`):
```
question: "Smoke check вЂ” Mobile Platform: Verify platform-specific behaviour:"
options:
  - "Touch controls work correctly for all primary actions вЂ” PASS"
  - "Touch controls вЂ” FAIL: [describe issue]"
  - "Game handles orientation change (portrait в†” landscape) correctly вЂ” PASS"
  - "Orientation change вЂ” FAIL: [describe what breaks]"
  - "Background / foreground transitions (home button) handled gracefully вЂ” PASS"
  - "Background/foreground вЂ” FAIL: [describe issue]"
  - "No visible performance issues on target device (no thermal throttling signs) вЂ” PASS"
  - "Mobile performance вЂ” FAIL: [describe issue]"
```

---

## Phase 5: Generate Report

Assemble the full smoke check report:

````markdown
## Smoke Check Report
**Date**: [date]
**Sprint**: [sprint name / number, or "Not identified"]
**Engine**: [engine]
**QA Plan**: [path, or "Not found вЂ” run /qa-plan first"]
**Argument**: [sprint | quick | blank]

---

### Automated Tests

**Status**: [PASS ([N] tests, [N] passing) | FAIL ([N] failures) |
NOT RUN ([reason])]

[If FAIL, list failing tests:]
- `[test name]` вЂ” [brief failure description from runner output]

[If NOT RUN:]
"Manual confirmation required: did tests pass in your local IDE or CI? This
will determine whether the automated test row contributes to a FAIL verdict."

---

### Test Coverage

| Story | Type | Test File | Coverage Status |
|-------|------|-----------|----------------|
| [title] | Logic | `tests/unit/[system]/[slug]_test.[ext]` | COVERED |
| [title] | Visual/Feel | `tests/evidence/[slug]-screenshots.md` | MANUAL |
| [title] | Logic | вЂ” | MISSING вљ  |
| [title] | Config/Data | вЂ” | EXPECTED |

**Summary**: [N] covered, [N] manual, [N] missing, [N] expected.

---

### Manual Smoke Checks

- [x] Game launches without crash вЂ” PASS
- [x] New game starts вЂ” PASS
- [x] [Core mechanic] вЂ” PASS
- [ ] [Other check] вЂ” FAIL: [user's description]
- [x] Save / load вЂ” PASS
- [-] Performance вЂ” not checked this session

---

### Missing Test Evidence

Stories that must have test evidence before they can be marked COMPLETE via
`/story-done`:

- **[story title]** (`[path]`) вЂ” Logic story has no test file.
  Expected location: `tests/unit/[system]/[story-slug]_test.[ext]`

[If none:] "All Logic and Integration stories have test coverage."

---

### Platform-Specific Results *(only if `--platform` was provided)*

| Platform | Checks Run | Passed | Failed | Platform Verdict |
|----------|-----------|--------|--------|-----------------|
| PC | [N] | [N] | [N] | PASS / FAIL |
| Console | [N] | [N] | [N] | PASS / FAIL |
| Mobile | [N] | [N] | [N] | PASS / FAIL |

**Platform notes**: [any platform-specific observations not captured in pass/fail]

Any platform with one or more FAIL checks contributes to the overall FAIL verdict.

---

### Verdict: [PASS | PASS WITH WARNINGS | FAIL]

[Verdict rules вЂ” first matching rule wins:]

**FAIL** if ANY of:
- Automated test suite ran and reported one or more test failures
- Any Batch 1 (core stability) check returned FAIL
- Any Batch 2 (primary sprint mechanic or regression check) returned FAIL

**PASS WITH WARNINGS** if ALL of:
- Automated tests PASS or NOT RUN (developer has not yet confirmed)
- All Batch 1 and Batch 2 smoke checks PASS
- One or more Logic/Integration stories have MISSING test evidence

**PASS** if ALL of:
- Automated tests PASS
- All smoke checks in all batches PASS or N/A
- No MISSING test evidence entries
````

---

## Phase 6: Write and Gate

Present the full report in conversation, then ask:

"May I write this smoke check report to `.ags/project/qa/smoke-[date].md`?"

Write only after approval.

After writing, deliver the gate verdict:

**If verdict is FAIL:**

"The smoke check failed. Do not hand off to QA until these failures are
resolved:

[List each failing automated test or smoke check with a one-line description]

Fix the failures and run `/smoke-check` again to re-gate before QA hand-off."

**If verdict is PASS WITH WARNINGS:**

"Smoke check passed with warnings. The build is ready for manual QA.

Advisory items to resolve before running `/story-done` on affected stories:
[list MISSING test evidence entries]

QA hand-off: share `.ags/project/qa/qa-plan-[sprint].md` with the qa-lead
agent to begin manual verification."

**If verdict is PASS:**

"Smoke check passed cleanly. The build is ready for manual QA.

QA hand-off: share `.ags/project/qa/qa-plan-[sprint].md` with the qa-lead
agent to begin manual verification."

---

## Collaborative Protocol

- **Never treat NOT RUN as automatic FAIL** вЂ” record it as NOT RUN and let
  the developer confirm status manually. Unconfirmed NOT RUN contributes to
  PASS WITH WARNINGS, not FAIL.
- **Never auto-fix failures** вЂ” report them and state what must be resolved.
  Do not attempt to edit source code or test files.
- **PASS WITH WARNINGS does not block QA hand-off** вЂ” it records advisory
  gaps for `/story-done` to follow up on.
- **`quick` argument** skips Phase 3 (coverage scan) and Phase 4 Batch 3.
  Use it for rapid re-checks after fixing a specific failure.
- Use `AskUserQuestion` for all manual smoke check verification.
- **Never write the report without asking** вЂ” Phase 6 requires explicit
  approval before any file is created.
