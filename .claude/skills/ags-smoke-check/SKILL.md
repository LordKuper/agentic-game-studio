---
name: ags-smoke-check
description: "Run the critical path smoke test gate before QA hand-off. Executes the automated test suite, verifies core functionality, and produces a PASS/FAIL report. Run after a sprint's stories are implemented and before manual QA begins. A failed smoke check means the build is not ready for QA."
argument-hint: "[sprint | quick | --platform pc|console|mobile|all]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, AskUserQuestion
---

# Smoke Check

Gate between "implementation done" and "QA hand-off". Runs automated tests, checks coverage gaps, batch-verifies critical paths with developer, produces PASS/FAIL report.

Rule: **build that fails smoke check does not go to QA.**

**Output:** `.ags/project/qa/smoke-[date].md`

---

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `tests/` with framework | `/ags-test-setup` | STOP. "Test framework not set up. Run `/ags-test-setup` first." |
| Test runner configured | `/ags-test-setup` | STOP. "Test runner not configured." |
| `.ags/project/qa/` | this skill (auto-create) | Auto-create — not a STOP. |

If STOP triggers, exit verdict **BLOCKED**.

---

## Parse Arguments

Args combinable: `/ags-smoke-check sprint --platform console`

**Base mode** (first arg, default `sprint`):
- `sprint` — full smoke check current sprint stories
- `quick` — skip coverage scan (Phase 3) + Batch 3; rapid re-checks

**Platform flag** (`--platform`, default none):
- `--platform pc` — keyboard, mouse, windowed mode
- `--platform console` — gamepad, TV safe zones, cert reqs
- `--platform mobile` — touch, orientation, battery/thermal
- `--platform all` — all variants; per-platform verdict table

If `--platform` provided: Phase 4 adds platform batches; Phase 5 outputs per-platform verdict table.

---

## Phase 1: Detect Test Setup

1. **Test framework check**: verify `tests/` exists. If not: "No test directory found at `tests/`. Run `/test-setup` to scaffold, or create dir manually." Stop.

2. **CI check**: check `.github/workflows/` for workflow referencing tests. Note CI status.

3. **Engine detection**: read `.ags/rules/technical-preferences.md`, extract `Engine:` value. Store for Phase 2.

4. **Smoke test list**: check `.ags/project/qa/smoke-tests.md` or `tests/smoke/`. If found, load for Phase 4. Else fallback to QA plan (Phase 4 fallback).

5. **QA plan check**: glob `.ags/project/qa/qa-plan-*.md`, take most recently modified. If none: "No QA plan found. Run `/ags-qa-plan sprint` before smoke-checking for best results."

Report: "Environment: [engine]. Test directory: [found/not found]. CI configured: [yes/no]. QA plan: [path/not found]."

---

## Phase 2: Run Automated Tests

Run test suite via Bash. Command per engine:

**Unity:**
Unity tests need editor — cannot run headlessly in most envs. Check artifacts:
```bash
ls -t test-results/ 2>/dev/null | head -5
```
If XML/JSON exists, read most recent, parse PASS/FAIL counts. Else: "Unity tests must be run from editor or CI. Confirm test status manually."

**Unknown engine / not configured:**
"Engine not configured in `.ags/rules/technical-preferences.md`. Run `/ags-setup-engine`, then re-run `/ags-smoke-check`."

**Test runner unavailable** (binary not on PATH, script missing):

"Automated tests could not execute — engine binary not on PATH. Status: NOT RUN. Confirm from local IDE or CI. Unconfirmed NOT RUN treated as PASS WITH WARNINGS, not FAIL — developer must manually confirm."

NOT RUN is not automatic FAIL. Record as warning. Phase 4 manual confirmation can resolve.

Parse runner output:
- Total tests run
- Passing count
- Failing count
- Names of failing tests (up to 10; else note count)
- Crash/error output

---

## Phase 3: Check Test Coverage

Story list priority:
1. QA plan from Phase 1 (Test Summary table)
2. Current sprint plan from `.ags/project/epics/` (most recent)
3. `quick` arg → skip phase. Note: "Coverage scan skipped — run `/ags-smoke-check sprint` for full analysis."

For each story:
1. Extract system slug from story file path (e.g., `.ags/project/epics/combat/story-001.md` → `combat`)
2. Glob `tests/unit/[system]/` + `tests/integration/[system]/` for files matching story slug
3. Check story file for `Test file:` header or "Test Evidence" section

Coverage status:

| Status | Meaning |
|--------|---------|
| **COVERED** | Test file matches story system + scope |
| **MANUAL** | Visual/Feel or UI; evidence doc found |
| **MISSING** | Logic/Integration story with no test file |
| **EXPECTED** | Config/Data — no test file required; spot-check |
| **UNKNOWN** | Story file missing/unreadable |

MISSING entries advisory. No FAIL verdict but appear prominently in report. Must resolve before `/ags-story-done` fully closes.

---

## Phase 4: Run Manual Smoke Checks

Smoke test checklist priority:
1. QA plan "Smoke Test Scope" section (if found Phase 1)
2. `.ags/project/qa/smoke-tests.md`
3. `tests/smoke/` contents
4. Standard fallback below (only if none above)

Tailor batches 2-3 to actual systems from sprint/QA plan. Replace placeholders with real mechanic names.

Use `AskUserQuestion`. Max 3 calls.

**Batch 1 — Core stability (always run):**
```
question: "Smoke check — Batch 1: Core stability. Please verify each:"
options:
  - "Game launches to main menu without crash — PASS"
  - "Game launches to main menu without crash — FAIL"
  - "New game / session starts successfully — PASS"
  - "New game / session starts successfully — FAIL"
  - "Main menu responds to all inputs — PASS"
  - "Main menu responds to all inputs — FAIL"
```

**Batch 2 — Sprint mechanic and regression (always run):**
```
question: "Smoke check — Batch 2: This sprint's changes and regression check:"
options:
  - "[Primary mechanic this sprint] — PASS"
  - "[Primary mechanic this sprint] — FAIL: [describe what broke]"
  - "[Second notable change this sprint, if any] — PASS"
  - "[Second notable change this sprint] — FAIL"
  - "Previous sprint's features still work (no regressions) — PASS"
  - "Previous sprint's features — regression found: [brief description]"
```

**Batch 3 — Data integrity and performance (run unless `quick`):**
```
question: "Smoke check — Batch 3: Data integrity and performance:"
options:
  - "Save / load completes without data loss — PASS"
  - "Save / load — FAIL: [describe what broke]"
  - "Save / load — N/A (save system not yet implemented)"
  - "No new frame rate drops or hitches observed — PASS"
  - "Frame rate drops or hitches found — FAIL: [where]"
  - "Performance — not checked in this session"
```

Record each response verbatim for Phase 5.

**Platform Batches** *(only if `--platform` provided)*:

**PC platform** (`--platform pc` or `--platform all`):
```
question: "Smoke check — PC Platform: Verify platform-specific behaviour:"
options:
  - "Keyboard controls work correctly across all menus and gameplay — PASS"
  - "Keyboard controls — FAIL: [describe issue]"
  - "Mouse input and cursor visibility correct in all states — PASS"
  - "Mouse input — FAIL: [describe issue]"
  - "Windowed and fullscreen modes function without graphical issues — PASS"
  - "Windowed/fullscreen — FAIL: [describe issue]"
  - "Resolution changes apply correctly — PASS"
  - "Resolution changes — FAIL: [describe issue]"
```

**Console platform** (`--platform console` or `--platform all`):
```
question: "Smoke check — Console Platform: Verify platform-specific behaviour:"
options:
  - "Gamepad input works correctly for all actions — PASS"
  - "Gamepad input — FAIL: [describe issue]"
  - "UI fits within TV safe zone margins (no text clipped) — PASS"
  - "TV safe zone — FAIL: [describe what is clipped]"
  - "No keyboard/mouse-only fallbacks shown to gamepad user — PASS"
  - "Input prompt inconsistency — FAIL: [describe]"
  - "Game boots correctly from cold start (no prior save) — PASS"
  - "Cold start — FAIL: [describe issue]"
```

**Mobile platform** (`--platform mobile` or `--platform all`):
```
question: "Smoke check — Mobile Platform: Verify platform-specific behaviour:"
options:
  - "Touch controls work correctly for all primary actions — PASS"
  - "Touch controls — FAIL: [describe issue]"
  - "Game handles orientation change (portrait ↔ landscape) correctly — PASS"
  - "Orientation change — FAIL: [describe what breaks]"
  - "Background / foreground transitions (home button) handled gracefully — PASS"
  - "Background/foreground — FAIL: [describe issue]"
  - "No visible performance issues on target device (no thermal throttling signs) — PASS"
  - "Mobile performance — FAIL: [describe issue]"
```

---

## Phase 5: Generate Report

````markdown
## Smoke Check Report
**Date**: [date]
**Sprint**: [sprint name / number, or "Not identified"]
**Engine**: [engine]
**QA Plan**: [path, or "Not found — run /ags-qa-plan first"]
**Argument**: [sprint | quick | blank]

---

### Automated Tests

**Status**: [PASS ([N] tests, [N] passing) | FAIL ([N] failures) |
NOT RUN ([reason])]

[If FAIL, list failing tests:]
- `[test name]` — [brief failure description from runner output]

[If NOT RUN:]
"Manual confirmation required: did tests pass in your local IDE or CI? This
will determine whether the automated test row contributes to a FAIL verdict."

---

### Test Coverage

| Story | Type | Test File | Coverage Status |
|-------|------|-----------|----------------|
| [title] | Logic | `tests/unit/[system]/[slug]_test.[ext]` | COVERED |
| [title] | Visual/Feel | `tests/evidence/[slug]-screenshots.md` | MANUAL |
| [title] | Logic | — | MISSING ⚠ |
| [title] | Config/Data | — | EXPECTED |

**Summary**: [N] covered, [N] manual, [N] missing, [N] expected.

---

### Manual Smoke Checks

- [x] Game launches without crash — PASS
- [x] New game starts — PASS
- [x] [Core mechanic] — PASS
- [ ] [Other check] — FAIL: [user's description]
- [x] Save / load — PASS
- [-] Performance — not checked this session

---

### Missing Test Evidence

Stories that must have test evidence before they can be marked COMPLETE via
`/ags-story-done`:

- **[story title]** (`[path]`) — Logic story has no test file.
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

[Verdict rules — first matching rule wins:]

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

Present full report in conversation, then ask:

"May I write this smoke check report to `.ags/project/qa/smoke-[date].md`?"

Write only after approval.

After writing, deliver gate verdict:

**FAIL:**

"Smoke check failed. Do not hand off to QA until resolved:

[List each failing automated test or smoke check with one-line description]

Fix failures, run `/ags-smoke-check` again to re-gate before QA."

**PASS WITH WARNINGS:**

"Smoke check passed with warnings. Build ready for manual QA.

Advisory items to resolve before `/ags-story-done`:
[list MISSING test evidence entries]

QA hand-off: share `.ags/project/qa/qa-plan-[sprint].md` with qa-lead agent for manual verification."

**PASS:**

"Smoke check passed cleanly. Build ready for manual QA.

QA hand-off: share `.ags/project/qa/qa-plan-[sprint].md` with qa-lead agent."

---

## Collaborative Protocol

- **Never treat NOT RUN as automatic FAIL** — record as NOT RUN; developer confirms manually. Unconfirmed NOT RUN → PASS WITH WARNINGS, not FAIL.
- **Never auto-fix failures** — report, state what must resolve. No edits to source/test files.
- **PASS WITH WARNINGS does not block QA hand-off** — records advisory gaps for `/ags-story-done`.
- **`quick` arg** skips Phase 3 + Phase 4 Batch 3. Use for rapid re-checks after fixing specific failure.
- Use `AskUserQuestion` for all manual smoke verification.
- **Never write report without asking** — Phase 6 requires explicit approval.
