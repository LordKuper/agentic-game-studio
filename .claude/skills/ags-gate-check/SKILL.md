---
name: ags-gate-check
description: "Validate readiness to advance between development phases or to close an epic. Produces PASS / CONCERNS / FAIL verdict with blockers and required artifacts. Use when user says 'are we ready to move to X', 'close this epic', 'pass the gate'."
argument-hint: "[target: foundation | production | polish | release | epic-done]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Edit, Task, AskUserQuestion
model: opus
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

# Gate Validation

Validates readiness to advance phase **or** close an epic. Checks artifacts, quality, blockers, stub debt.

**Distinct from `/ags-project-stage-detect`**: that's diagnostic ("where are we?"). This is prescriptive ("are we ready?" with formal verdict).

## Phases (5)

1. **Concept** — game idea, pillars, art bible, systems map
2. **Foundation** — architecture skeleton, accessibility tier, control manifest seed, test framework
3. **Production** — epic-driven vertical slices, looped until feature-complete
4. **Polish** — performance, balance, playtests, bug fixing
5. **Release** — launch prep, certification, ship

Plus a **per-epic gate** within Production: `epic-done`.

**On gate pass**, update `.ags/project/stage.md` (Phase, Active Epic, Transition History).

---

## 1. Parse Arguments

**Target:** `$ARGUMENTS[0]`. Accepted values: `foundation`, `production`, `polish`, `release`, `epic-done`. Blank = auto-detect from `stage.md`.

Phase gates (foundation/production/polish/release) spawn the full director panel in parallel. `epic-done` also spawns the full panel (CD + TD + AD + PR).

- **With argument**: `/ags-gate-check production` — validate that transition.
- **No argument**: read `.ags/project/stage.md` for current phase + active epic. Confirm with user via `AskUserQuestion`:
  - Prompt: "Detected phase: **[current]**, active epic: **[id or —]**. Run gate for [Current → Next] or [epic-done]?"
  - Options: `[A] Phase gate [Current → Next]`, `[B] Epic-done for [active epic]`, `[C] Pick a different gate`
  Do not skip confirmation when no argument provided.

---

## 2. Gate Definitions

### Gate: Concept → Foundation

**Required Artifacts:**
- [ ] `design/gdd/game-concept.md` exists, has content
- [ ] Game pillars defined (in concept doc or `design/gdd/game-pillars.md`)
- [ ] Visual Identity Anchor section in `design/gdd/game-concept.md`
- [ ] `design/art/ags-art-bible.html` exists (Sections 1-5 minimum)
- [ ] `design/gdd/systems-index.md` with at least MVP systems

**Quality Checks:**
- [ ] Game concept reviewed (`/ags-design-review` verdict not MAJOR REVISION)
- [ ] Core loop described
- [ ] Target audience identified
- [ ] Visual Identity Anchor: one-line visual rule + ≥2 supporting principles
- [ ] Systems index has dependency ordering and priority tiers

---

### Gate: Foundation → Production

**Required Artifacts:**
- [ ] Engine chosen (`.ags/rules/technical-preferences.md` populated, `Engine:` filled)
- [ ] `design/architecture/architecture.md` (skeleton — top-level layers, module boundaries, tech stack)
- [ ] `design/accessibility-requirements.md` with accessibility tier committed
- [ ] `design/architecture/control-manifest.md` (seed from `/ags-create-control-manifest`)
- [ ] `.ags/docs/engine-reference/[engine]/VERSION.md` exists
- [ ] Test framework: `tests/unit/`, `tests/integration/`, CI workflow file
- [ ] At least one example test confirms framework functional

**Quality Checks:**
- [ ] Architecture skeleton names module boundaries (no `[TBD]` for Foundation layer)
- [ ] Technical preferences have naming conventions, performance budgets
- [ ] Accessibility tier defined (Basic acceptable — undefined not)
- [ ] Engine compatibility check passes for skeleton (no deprecated APIs in `architecture.md`)

> Foundation does NOT require ADRs to exist yet. ADRs accumulate per epic in Production.

---

### Gate: Production → Polish

**Required Artifacts:**
- [ ] All MVP epics in `.ags/project/epics/index.md` have Status=done
- [ ] Each MVP epic's `EPIC.md` has `epic-done` gate verdict READY (or CONCERNS with override)
- [ ] `.ags/project/stubs.md` has no Open Stubs that affect MVP gameplay (Open OK only if explicitly Migrated to a Polish-phase task with approval)
- [ ] At least 3 distinct playtest reports across epics in `.ags/project/playtests/`
- [ ] Smoke check report with PASS or PASS WITH WARNINGS in `.ags/project/qa/`
- [ ] All MVP-tier GDDs from systems-index complete (Approved status)
- [ ] Master `architecture.md` has cumulative ADR coverage (no Foundation/Core layer gaps)
- [ ] Cumulative architecture-review report exists for latest epic batch

**Quality Checks:**
- [ ] Core loop plays end-to-end without developer guidance
- [ ] Core loop fun validated — playtest data confirms
- [ ] Tests passing (run via Bash if test runner configured)
- [ ] No critical/blocker bugs open
- [ ] Performance within budget (technical-preferences.md targets)
- [ ] All `revise`-mode epics' impact resolved (no broken consumers)
- [ ] Cross-GDD consistency clean (latest `/ags-review-all-gdds` verdict not FAIL)
- [ ] Accessibility tier compliance verified
- [ ] No "confusion loops" — no point >50% playtesters got stuck
- [ ] Difficulty curve matches `design/difficulty-curve.md` (if exists)

---

### Gate: Polish → Release

**Required Artifacts:**
- [ ] All features from milestone plan implemented
- [ ] Content complete (levels, assets, dialogue per design docs)
- [ ] Localization strings externalized (no hardcoded player-facing text in source)
- [ ] QA test plan exists (`.ags/project/qa/`)
- [ ] QA sign-off APPROVED or APPROVED WITH CONDITIONS (`/ags-team-qa` output)
- [ ] All Must-Have story test evidence present
- [ ] Smoke check passes cleanly (PASS) on release candidate build
- [ ] No test regressions — full test suite passes
- [ ] Balance data reviewed (`/ags-balance-check` run)
- [ ] Release checklist completed
- [ ] Store metadata prepared (if applicable)
- [ ] Changelog / patch notes drafted

**Quality Checks:**
- [ ] Full QA pass signed off by `qa-lead`
- [ ] All tests passing
- [ ] Performance targets met across all target platforms
- [ ] No known critical, high, or medium-severity bugs
- [ ] Accessibility basics covered (remapping, text scaling if applicable)
- [ ] Localization verified for all target languages
- [ ] Legal requirements met (EULA, privacy policy, age ratings if applicable)
- [ ] Build compiles, packages cleanly

---

### Gate: epic-done (per-epic)

Closes a single epic. Run once per epic, before declaring it done.

**Resolve epic**: read `.ags/project/stage.md` for active epic, or ask user.

**Required Artifacts:**
- [ ] `EPIC.md` Acceptance Criteria — all items checked
- [ ] All stories under `.ags/project/epics/[slug]/stories/` have Status=Done
- [ ] `EPIC.md` Retrospective section filled (run `/ags-epic-retro` if missing)
- [ ] At least one playtest report linked under `.ags/project/playtests/epic-[slug]-*.md`
- [ ] `.ags/project/stubs.md` reconciled by latest `/ags-stub-track` scan

**Quality Checks:**
- [ ] All stubs introduced by this epic are Closed in `stubs.md`, **OR** moved to Migrated table with explicit approval entry
- [ ] No new S1/S2 bugs open against this epic in `.ags/project/bugs/`
- [ ] Code reviewed (story-done verdicts not BLOCKED)
- [ ] GDD/ADR deviations documented in EPIC.md or new ADR
- [ ] Cross-GDD consistency check ran for this epic's GDD changes (`/ags-consistency-check`)
- [ ] Architecture review ran covering this epic's ADRs (`/ags-architecture-review`)

**Stub Block Rule:** if any STUB introduced by this epic remains in `stubs.md` Open Stubs **without** a Migrated entry, gate is **FAIL** until resolved by `/ags-stub-track close|migrate`.

---

## 3. Run the Gate Check

For each item in target gate:

### Artifact Checks
- Glob and Read to verify files exist with meaningful content (not just template header).
- For code: verify directory structure and file counts.

### Quality Checks
- Run test suite via Bash if test runner configured.
- Read GDD, check 8 required sections.
- Compare performance data against `technical-preferences.md`.
- Grep hardcoded strings in engine source root.

### Document Boundary Check (mandatory — per `.ags/rules/review-workflow.md` § Document Boundary Check)

Run on artifacts in scope of this gate (per Gate Definitions above):

- Concept→Foundation: game-concept, engine doc, art-bible, DESIGN.md, systems-index, accessibility-requirements.
- Foundation→Production: + ADRs, control-manifest, all GDDs.
- Production→Polish / Polish→Release / epic-done: + epic doc, stories, UX/HUD specs.

Per `.ags/rules/document-boundaries.md`: front-matter `status:` validity, SSoT zone violations (tech-leak in GDD, GDD→ADR cite, raw visual literals outside DESIGN.md, missing `**GDD source**:` in ADR, content duplication, unapproved predecessor cited).

Delegation: invoke `/ags-consistency-check full` and merge Boundary Violations into gate findings. Boundary findings classified `high`, never dropped — gate verdict downgrades to **FAIL** on any 🔴 BOUNDARY violation, **CONCERNS** on ⚠️ DUPLICATION / ℹ️ MARKER GAP unless waived in `decisions-log.md`.

### Cross-Reference Checks
- Compare `design/gdd/` vs implementation in engine source root.
- Every system in architecture has corresponding code (production phase).
- Every Open Stub has owner-epic field set.

### Stub Reconciliation (epic-done only)
- Read `.ags/project/stubs.md`.
- Filter stubs where `Introduced (epic) = [closing epic id]`.
- Verify each is in Closed Stubs or Migrated Stubs.
- Any remaining in Open Stubs → block gate.

---

## 4. Collaborative Assessment

For unverifiable items, ask user:

- "I can't verify core loop plays well. Has it been playtested?"
- "Performance profiling data unavailable. Run `/ags-perf-profile`?"
- "Is the migrated stub STUB-NNN approved by you?"

**Never assume PASS for unverifiable.** Mark MANUAL CHECK NEEDED.

---

## 4b. Director Panel Assessment (Internal Review Loop)

Spawn the full director panel (CD + TD + PR + AD) in parallel for any gate.

Spawn parallel via Task using gate names:

1. **`creative-director`** — gate **CD-PHASE-GATE**
2. **`technical-director`** — gate **TD-PHASE-GATE**
3. **`producer`** — gate **PR-PHASE-GATE** (or **PR-EPIC-DONE** for epic-done)
4. **`art-director`** — gate **AD-PHASE-GATE**

Pass each: target gate name, artifacts present list, context fields.

Collect all responses, present Director Panel summary:

```
## Director Panel Assessment (Iteration [N])

Creative Director:  [READY / CONCERNS / NOT READY]
  [feedback]

Technical Director: [READY / CONCERNS / NOT READY]
  [feedback]

Producer:           [READY / CONCERNS / NOT READY]
  [feedback]

Art Director:       [READY / CONCERNS / NOT READY]
  [feedback]
```

### Loop semantics

This panel runs as an internal-review loop. Iterate until a single iteration in which **every** spawned director returns READY.

- Any director NOT READY or CONCERNS → present consolidated findings to user, ask to address blockers (fix artifacts, run missing skills, update GDDs/ADRs as appropriate), then re-spawn the same panel for the next iteration. No iteration cap.
- All READY → exit loop, proceed to 4c.

User may override with explicit acknowledgement (`AskUserQuestion`: "Force-exit loop and accept current verdicts?") — this is recorded in the gate output as `Internal review override: [reason]`.

Record iteration count and per-director final verdicts for the gate report.

---

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

## 5. Output the Verdict

```
## Gate Check: [Target gate name]

**Date**: [date]
**Checked by**: gate-check skill

### Required Artifacts: [X/Y present]
- [x] design/architecture/architecture.md — exists, 5.2KB
- [ ] tests/unit/ — MISSING

### Quality Checks: [X/Y passing]
- [x] Engine compat clean
- [ ] Tests — FAILED (3 failures in tests/unit/)
- [?] Core loop playtested — MANUAL CHECK NEEDED

### Stubs (epic-done only)
- Open: [N] (must be 0 or all Migrated)
- Closed by this epic: [N]
- Migrated: [N]

### Blockers
1. **[blocker]** — [how to resolve]

### Recommendations
- [Priority actions]

### Verdict: [PASS / CONCERNS / FAIL]
- **PASS**: All required artifacts present, all quality checks passing
- **CONCERNS**: Minor gaps exist but addressable in next phase or epic
- **FAIL**: Critical blockers must be resolved before advancing
```

---

## 5a. Chain-of-Verification

After draft verdict, challenge before finalising.

**Step 1 — Generate 5 challenge questions** to disprove verdict:

For **PASS** draft:
- "Which quality checks did I verify by reading a file vs. inferring?"
- "Are there MANUAL CHECK NEEDED items I marked PASS without user confirmation?"
- "Did I confirm all listed artifacts have real content, not empty headers?"
- "Could any blocker I dismissed as minor actually prevent the phase or epic from succeeding?"
- "Which single check am I least confident in, why?"

For **CONCERNS** draft:
- "Could any listed CONCERN be elevated to blocker given current state?"
- "Is concern resolvable in next phase, or compounds over time?"
- "Did I soften any FAIL into CONCERN to avoid harder verdict?"
- "Are there artifacts I didn't check that could reveal more blockers?"
- "Do all CONCERNS together create blocking problem even if each is minor alone?"

For **FAIL** draft:
- "Have I separated hard blockers from strong recommendations?"
- "Are there PASS items I was too lenient about?"
- "Am I missing additional blockers user should know?"
- "Can I provide minimal path to PASS — specific 3 things that must change?"
- "Is fail condition resolvable, or indicates deeper design problem?"

**Step 2 — Answer each independently.** Do NOT reference draft verdict — re-check files or ask user.

**Step 3 — Revise if needed:**
- Answer reveals missed blocker → upgrade verdict (PASS→CONCERNS or CONCERNS→FAIL)
- Answer reveals over-stated blocker → downgrade only with specific evidence
- Consistent answers → confirm unchanged

**Step 4 — Note verification** in final report:
`Chain-of-Verification: [N] questions checked — verdict [unchanged | revised from X to Y]`

---

## 6. Update stage.md and epic state on PASS

Verdict **PASS** + user confirms advance:

### 6a. Phase gate (foundation / production / polish / release)

Read `.ags/project/stage.md`. If missing, create from skeleton:

```markdown
# Stage

| Field | Value |
|-------|-------|
| Phase | [new phase] |
| Active Epic | — |
| Updated | [now] |

## Transition History

| Date | Phase | Active Epic | Note |
|------|-------|-------------|------|
| [now] | [new phase] | — | Phase gate passed |
```

If exists, Edit:
- Update `Phase` to new phase.
- Set `Active Epic` to `—` for `production`-entry (next `/ags-create-epics` sets it), or unchanged for other transitions.
- Update `Updated` to now.
- Append row to Transition History.

Ask: "Gate passed. May I update `.ags/project/stage.md` to phase `[new phase]`?"

### 6b. epic-done gate

Read `.ags/project/epics/[slug]/EPIC.md`. Ask: "Gate passed. May I:
- Update `EPIC.md` Status → `done`, Closed → today, fill Gate Verdict table
- Update `.ags/project/epics/index.md` row → Status=done, Closed=today
- Update `.ags/project/stage.md` → clear Active Epic (`—`), append Transition History row
- Append entry to `.ags/project/decisions-log.md`?"

If approved, Edit each file accordingly.

Phase remains `production` after epic close — user runs `/ags-create-epics` to start next, or `/ags-gate-check production` when MVP epics complete.

---

## 7. Closing Next-Step Widget

After verdict + any state update, close with `AskUserQuestion`.

**Tailor options to gate:**

For **Concept → Foundation PASS**:
```
Gate passed. What next?
[A] Run /ags-create-architecture — produce architecture skeleton (required for foundation)
[B] Continue concept work
[C] Stop here for this session
```

For **Foundation → Production PASS**:
```
Gate passed. What next?
[A] Run /ags-create-epics — plan first vertical-slice epic (recommended)
[B] Refine foundation artifacts
[C] Stop here for this session
```

For **epic-done PASS**:
```
Epic closed. What next?
[A] Run /ags-create-epics — plan next epic
[B] Run /ags-gate-check production — check if MVP epics done, advance to polish
[C] Stop here for this session
```

For **Production → Polish PASS**:
```
Gate passed. What next?
[A] Run /ags-perf-profile — start polish work with performance baseline
[B] Run /ags-balance-check
[C] Stop here for this session
```

For **Polish → Release PASS**:
```
Gate passed. What next?
[A] Run /ags-release-checklist
[B] Run /ags-launch-checklist
[C] Stop here for this session
```

---

## 8. Follow-Up Actions

Suggest specific next steps based on missing artifacts:

- **No game concept?** → `/ags-brainstorm`
- **No art bible?** → `/ags-art-bible`
- **No systems index?** → `/ags-map-systems`
- **No architecture skeleton?** → `/ags-create-architecture`
- **No accessibility doc?** → offer to create from `.ags/templates/t_accessibility-requirements.md` with tier choice via `AskUserQuestion`
- **No control manifest?** → `/ags-create-control-manifest`
- **No test framework?** → `/test-setup`
- **No epic to start?** → `/ags-create-epics`
- **Active epic has stub-mode systems but no contracts?** → `/ags-epic-contracts [slug]`
- **Stubs out of sync with code?** → `/ags-stub-track scan`
- **Epic has Open Stubs blocking close?** → `/ags-stub-track close [STUB-ID] [story-ref]` or `/ags-stub-track migrate [STUB-ID] [new-owner-epic] [reason]`
- **Epic missing retro?** → `/ags-epic-retro [slug]`
- **GDDs not cross-reviewed for this epic?** → `/ags-review-all-gdds`
- **Cross-GDD consistency issues?** → `/ags-consistency-check`
- **Architecture coherence unknown?** → `/ags-architecture-review`
- **No QA plan?** → `/ags-qa-plan`
- **Tests failing?** → delegate to `lead-programmer` or `qa-lead`
- **No playtest data?** → `/ags-playtest-report`
- **<3 playtests for polish?** → run more, structure findings via `/ags-playtest-report`
- **No difficulty curve?** → consider `design/difficulty-curve.md`
- **Performance unknown?** → `/ags-perf-profile`
- **Not localized?** → `/ags-localize`
- **Ready for release?** → `/ags-launch-checklist`

---

## Collaborative Protocol

1. **Scan first** — check all artifacts and quality gates
2. **Ask about unknowns** — don't assume PASS for unverifiable
3. **Present findings** — show full checklist with status
4. **User decides** — verdict is recommendation; user makes final call
5. **Get approval** — "May I write this gate check report and update stage.md?"

**Never** block user from advancing — verdict is advisory. Document risks, let user decide.
