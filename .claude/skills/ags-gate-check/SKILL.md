---
name: ags-gate-check
description: "Validate readiness to advance between development phases. Produces a PASS/CONCERNS/FAIL verdict with specific blockers and required artifacts. Use when user says 'are we ready to move to X', 'can we advance to production', 'check if we can start the next phase', 'pass the gate'."
argument-hint: "[target-phase: systems-design | technical-setup | pre-production | production | polish | release] [--review full|lean|solo]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Task, AskUserQuestion
model: opus
---

# Phase Gate Validation

Validates readiness to advance to next development phase. Checks artifacts, quality, blockers.

**Distinct from `/ags-project-stage-detect`**: that's diagnostic ("where are we?"). This is prescriptive ("are we ready?" with formal verdict).

## Production Stages (7)

1. **Concept** — Brainstorming, game concept document
2. **Systems Design** — Mapping systems, GDDs
3. **Technical Setup** — Engine config, architecture decisions
4. **Pre-Production** — Prototyping, vertical slice validation
5. **Production** — Feature dev (Epic/Feature/Task tracking active)
6. **Polish** — Performance, playtesting, bug fixing
7. **Release** — Launch prep, certification

**On gate pass**, write new stage name to `.ags/project/stage.txt` (single line, e.g. `Production`). Updates status line.

---

## 1. Parse Arguments

**Target phase:** `$ARGUMENTS[0]` (blank = auto-detect current, validate next transition)

Resolve review mode (once, store for all gate spawns):
1. `--review [full|lean|solo]` passed → use that
2. Else read `.ags/project/review-mode.md` → use value
3. Else → default `lean`

In `solo` mode, director spawns (CD-PHASE-GATE, TD-PHASE-GATE, PR-PHASE-GATE, AD-PHASE-GATE) skipped — gate-check becomes artifact-existence checks only. In `lean` mode, all four directors still run (phase gates are purpose of lean mode).

- **With argument**: `/ags-gate-check production` — validate that phase
- **No argument**: Auto-detect using `/ags-project-stage-detect` heuristics, **confirm with user before running**:

  Use `AskUserQuestion`:
  - Prompt: "Detected stage: **[current stage]**. Running gate for [Current] → [Next] transition. Is this correct?"
  - Options:
    - `[A] Yes — run this gate`
    - `[B] No — pick a different gate` (if selected, second widget listing all gate options: Concept → Systems Design, Systems Design → Technical Setup, Technical Setup → Pre-Production, Pre-Production → Production, Production → Polish, Polish → Release)
  
  Do not skip confirmation when no argument provided.

---

## 2. Phase Gate Definitions

### Gate: Concept → Systems Design

**Required Artifacts:**
- [ ] `design/gdd/game-concept.md` exists, has content
- [ ] Game pillars defined (in concept doc or `design/gdd/game-pillars.md`)
- [ ] Visual Identity Anchor section in `design/gdd/game-concept.md` (from brainstorm Phase 4 art-director output)

**Quality Checks:**
- [ ] Game concept reviewed (`/ags-design-review` verdict not MAJOR REVISION NEEDED)
- [ ] Core loop described, understood
- [ ] Target audience identified
- [ ] Visual Identity Anchor: one-line visual rule + at least 2 supporting principles

---

### Gate: Systems Design → Technical Setup

**Required Artifacts:**
- [ ] Systems index at `design/gdd/systems-index.md` with at least MVP systems
- [ ] All MVP-tier GDDs in `design/gdd/` individually pass `/ags-design-review`
- [ ] Cross-GDD review report in `design/gdd/` (from `/ags-review-all-gdds`)

**Quality Checks:**
- [ ] All MVP GDDs pass design review (8 required sections, no MAJOR REVISION NEEDED)
- [ ] `/ags-review-all-gdds` verdict not FAIL
- [ ] All cross-GDD consistency issues resolved or explicitly accepted
- [ ] System dependencies mapped, bidirectionally consistent
- [ ] MVP priority tier defined
- [ ] No stale GDD references flagged

---

### Gate: Technical Setup → Pre-Production

**Required Artifacts:**
- [ ] Engine chosen (CLAUDE.md Technology Stack not `[CHOOSE]`)
- [ ] Technical preferences configured (`.ags/rules/technical-preferences.md` populated)
- [ ] Art bible at `design/art/ags-art-bible.md` with Sections 1–4 (Visual Identity Foundation)
- [ ] At least 3 ADRs in `design/architecture/` covering Foundation systems (scene management, event architecture, save/load)
- [ ] Engine reference docs in `.ags/docs/engine-reference/[engine]/`
- [ ] Test framework: `tests/unit/` and `tests/integration/` exist
- [ ] CI/CD test workflow at `.github/workflows/tests.yml` (or equivalent)
- [ ] At least one example test file confirms framework functional
- [ ] Master architecture doc at `design/architecture/architecture.md`
- [ ] Architecture traceability index at `design/architecture/architecture-traceability.md`
- [ ] `/ags-architecture-review` run (review report in `design/architecture/`)
- [ ] `design/accessibility-requirements.md` with accessibility tier committed
- [ ] `design/ux/interaction-patterns.md` exists (pattern library initialized)

**Quality Checks:**
- [ ] ADRs cover core systems (rendering, input, state management)
- [ ] Technical preferences have naming conventions, performance budgets
- [ ] Accessibility tier defined ("Basic" acceptable — undefined not)
- [ ] At least one screen's UX spec started
- [ ] All ADRs have **Engine Compatibility section** with engine version stamped
- [ ] All ADRs have **GDD Requirements Addressed section** with explicit GDD linkage
- [ ] No ADR references APIs in `.ags/docs/engine-reference/[engine]/deprecated-apis.md`
- [ ] All HIGH RISK engine domains (per VERSION.md) addressed in architecture or flagged as open
- [ ] Architecture traceability matrix has **zero Foundation layer gaps**

**ADR Circular Dependency Check**: For all ADRs in `design/architecture/`, read each ADR's "ADR Dependencies" / "Depends On" section. Build dependency graph (ADR-A → ADR-B = A depends on B). Cycle detected (e.g. A→B→A, or A→B→C→A):
- Flag as **FAIL**: "Circular ADR dependency: [ADR-X] → [ADR-Y] → [ADR-X]. Neither can reach Accepted while cycle exists. Remove one 'Depends On' edge."

**Engine Validation** (read `.ags/docs/engine-reference/[engine]/VERSION.md` first):
- [ ] ADRs touching post-cutoff engine APIs flagged with Knowledge Risk: HIGH/MEDIUM
- [ ] `/ags-architecture-review` engine audit shows no deprecated API usage
- [ ] All ADRs agree on same engine version

---

### Gate: Pre-Production → Production

**Required Artifacts:**
- [ ] First sprint plan in `.ags/project/sprints/`
- [ ] Art bible complete (all 9 sections), AD-ART-BIBLE sign-off recorded in `design/art/ags-art-bible.md`
- [ ] Character visual profiles for key characters in narrative docs
- [ ] All MVP-tier GDDs from systems index complete
- [ ] Master architecture doc at `design/architecture/architecture.md`
- [ ] At least 3 ADRs covering Foundation-layer decisions in `design/architecture/`
- [ ] Control manifest at `design/architecture/control-manifest.md` (from `/ags-create-control-manifest` from Accepted ADRs)
- [ ] Epics in `.ags/project/epics/` with Foundation and Core layer epics (use `/ags-create-epics layer: foundation` and `/ags-create-epics layer: core`, then `/ags-create-stories [epic-slug]`)
- [ ] Vertical Slice build exists, playable (not just scope-defined)
- [ ] Vertical Slice playtested, at least 3 sessions (internal OK)
- [ ] Vertical Slice playtest report in `.ags/project/playtests/` or equivalent
- [ ] UX specs for key screens: main menu, core gameplay HUD (in `design/ux/`), pause menu
- [ ] HUD design doc at `design/ux/hud.md` (if game has HUD)
- [ ] All key screen UX specs passed `/ux-review` (APPROVED or NEEDS REVISION accepted)

**Quality Checks:**
- [ ] **Core loop fun validated** — playtest data confirms central mechanic enjoyable, not just functional. Check Vertical Slice playtest report.
- [ ] UX specs cover all UI Requirements sections from MVP GDDs
- [ ] Interaction pattern library documents patterns used in key screens
- [ ] Accessibility tier from `design/accessibility-requirements.md` addressed in all key screen UX specs
- [ ] Sprint plan references real story file paths from `.ags/project/epics/` (stories must embed GDD req ID + ADR reference)
- [ ] **Vertical Slice COMPLETE**, not just scoped — full core loop end-to-end. At least one [start → challenge → resolution] cycle works.
- [ ] Architecture doc has no unresolved open questions in Foundation/Core layers
- [ ] All ADRs have Engine Compatibility sections stamped
- [ ] All ADRs have ADR Dependencies sections (even "None")
- [ ] Manual validation: GDDs + architecture + epics coherent (run `/ags-review-all-gdds` and `/ags-architecture-review` if not recent)
- [ ] **Core fantasy delivered** — at least one playtester independently described experience matching Player Fantasy section of core system GDDs (unprompted).

**Vertical Slice Validation** (FAIL if any item NO):
- [ ] Human played core loop without developer guidance
- [ ] Game communicates what to do within first 2 minutes
- [ ] No critical "fun blocker" bugs in Vertical Slice
- [ ] Core mechanic feels good (subjective — ask user)

> **Note**: Any Vertical Slice Validation FAIL → verdict auto-FAIL regardless of other checks. Advancing without validated Vertical Slice is #1 cause of production failure (per GDC postmortem data, 155 projects).

---

### Gate: Production → Polish

**Required Artifacts:**
- [ ] `Assets/Scripts/` has active code organized into subsystems
- [ ] All core mechanics from GDD implemented (cross-ref `design/gdd/` with `Assets/Scripts/`)
- [ ] Main gameplay path playable end-to-end
- [ ] Test files in `tests/unit/` and `tests/integration/` covering Logic and Integration stories
- [ ] All Logic stories from sprint have unit test files in `tests/unit/`
- [ ] Smoke check run with PASS or PASS WITH WARNINGS — report in `.ags/project/qa/`
- [ ] QA plan in `.ags/project/qa/` (from `/ags-qa-plan`) covering this sprint or final production sprint
- [ ] QA sign-off in `.ags/project/qa/` (from `/ags-team-qa`) APPROVED or APPROVED WITH CONDITIONS
- [ ] At least 3 distinct playtest sessions in `.ags/project/playtests/`
- [ ] Playtest reports cover: new player experience, mid-game systems, difficulty curve
- [ ] Fun hypothesis from Game Concept explicitly validated or revised

**Quality Checks:**
- [ ] Tests passing (run via Bash)
- [ ] No critical/blocker bugs
- [ ] Core loop plays as designed (vs GDD acceptance criteria)
- [ ] Performance within budget (technical-preferences.md targets)
- [ ] Playtest findings reviewed, critical fun issues addressed (not just documented)
- [ ] No "confusion loops" — no point where >50% playtesters got stuck without knowing why
- [ ] Difficulty curve matches `design/difficulty-curve.md` (if exists)
- [ ] All implemented screens have UX specs (no "designed in-code" screens)
- [ ] Interaction pattern library up-to-date with all implemented patterns
- [ ] Accessibility compliance verified vs committed tier in `design/accessibility-requirements.md`

---

### Gate: Polish → Release

**Required Artifacts:**
- [ ] All features from milestone plan implemented
- [ ] Content complete (all levels, assets, dialogue from design docs exist)
- [ ] Localization strings externalized (no hardcoded player-facing text in `Assets/Scripts/`)
- [ ] QA test plan exists (`/ags-qa-plan` output in `.ags/project/qa/`)
- [ ] QA sign-off (`/ags-team-qa` output — APPROVED or APPROVED WITH CONDITIONS)
- [ ] All Must Have story test evidence present (Logic/Integration: tests pass; Visual/Feel/UI: sign-off docs in `.ags/project/qa/evidence/`)
- [ ] Smoke check passes cleanly (PASS) on release candidate build
- [ ] No test regressions from previous sprint (test suite passes fully)
- [ ] Balance data reviewed (`/ags-balance-check` run)
- [ ] Release checklist completed (`/ags-release-checklist` or `/ags-launch-checklist`)
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

## 3. Run the Gate Check

**Before artifact checks**, read `docs/consistency-failures.md` if exists. Extract entries whose Domain matches target phase. Carry as context — recurring conflict patterns warrant increased scrutiny on those checks.

For each item in target gate:

### Artifact Checks
- Glob and Read to verify files exist, have meaningful content
- Verify real content, not just template header
- For code: verify directory structure, file counts

**Systems Design → Technical Setup gate — cross-GDD review check**:
Glob `design/gdd/gdd-cross-review-*.md` to find `/ags-review-all-gdds` report. No match → mark "cross-GDD review report exists" **FAIL**, surface prominently: "No `/ags-review-all-gdds` report found in `design/gdd/`. Run `/ags-review-all-gdds` before advancing."
File found → read it, check verdict line: FAIL means cross-GDD consistency check failed, must resolve before advancing.

### Quality Checks
- Test checks: run test suite via Bash if test runner configured
- Design review checks: Read GDD, check 8 required sections
- Performance checks: Read technical-preferences.md, compare against profiling data in `tests/performance/` or recent `/ags-perf-profile` output
- Localization checks: Grep hardcoded strings in `Assets/Scripts/`

### Cross-Reference Checks
- Compare `design/gdd/` vs `Assets/Scripts/` implementations
- Every system in architecture docs has corresponding code
- Sprint plans reference real work items

---

## 4. Collaborative Assessment

For unverifiable items, **ask user**:

- "I can't verify core loop plays well. Has it been playtested?"
- "No playtest report found. Has informal testing been done?"
- "Performance profiling data unavailable. Want to run `/ags-perf-profile`?"

**Never assume PASS for unverifiable.** Mark MANUAL CHECK NEEDED.

---

## 4b. Director Panel Assessment

Before final verdict, spawn all four directors as **parallel subagents** via Task using parallel gate protocol from `.ags/rules/director-gates.md`. Issue all four Task calls simultaneously — do not wait between.

**Spawn in parallel:**

1. **`creative-director`** — gate **CD-PHASE-GATE** (`.ags/rules/director-gates.md`)
2. **`technical-director`** — gate **TD-PHASE-GATE** (`.ags/rules/director-gates.md`)
3. **`producer`** — gate **PR-PHASE-GATE** (`.ags/rules/director-gates.md`)
4. **`art-director`** — gate **AD-PHASE-GATE** (`.ags/rules/director-gates.md`)

Pass each: target phase name, artifacts present list, context fields per gate definition.

**Collect all four, present Director Panel summary:**

```
## Director Panel Assessment

Creative Director:  [READY / CONCERNS / NOT READY]
  [feedback]

Technical Director: [READY / CONCERNS / NOT READY]
  [feedback]

Producer:           [READY / CONCERNS / NOT READY]
  [feedback]

Art Director:       [READY / CONCERNS / NOT READY]
  [feedback]
```

**Apply to verdict:**
- Any director NOT READY → minimum FAIL (user may override with explicit acknowledgement)
- Any director CONCERNS → minimum CONCERNS
- All four READY → eligible for PASS (still subject to Section 3 checks)

---

## 5. Output the Verdict

```
## Gate Check: [Current Phase] → [Target Phase]

**Date**: [date]
**Checked by**: gate-check skill

### Required Artifacts: [X/Y present]
- [x] design/gdd/game-concept.md — exists, 2.4KB
- [ ] design/architecture/ — MISSING (no ADRs found)
- [x] .ags/project/sprints/ — exists, 1 sprint plan

### Quality Checks: [X/Y passing]
- [x] GDD has 8/8 required sections
- [ ] Tests — FAILED (3 failures in tests/unit/)
- [?] Core loop playtested — MANUAL CHECK NEEDED

### Blockers
1. **No Architecture Decision Records** — Run `/ags-architecture-decision` to create one covering core system architecture before production.
2. **3 test failures** — Fix failing tests in tests/unit/ before advancing.

### Recommendations
- [Priority actions to resolve blockers]
- [Optional improvements that aren't blocking]

### Verdict: [PASS / CONCERNS / FAIL]
- **PASS**: All required artifacts present, all quality checks passing
- **CONCERNS**: Minor gaps exist but addressable in next phase
- **FAIL**: Critical blockers must be resolved before advancing
```

---

## 5a. Chain-of-Verification

After draft verdict in Phase 5, challenge before finalising.

**Step 1 — Generate 5 challenge questions** to disprove verdict:

For **PASS** draft:
- "Which quality checks did I verify by reading a file vs. inferring?"
- "Are there MANUAL CHECK NEEDED items I marked PASS without user confirmation?"
- "Did I confirm all listed artifacts have real content, not empty headers?"
- "Could any blocker I dismissed as minor actually prevent the phase from succeeding?"
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

## 6. Update Stage on PASS

Verdict **PASS** + user confirms advance:

1. Write new stage name to `.ags/project/stage.txt` (single line, no trailing newline)
2. Updates status line for all future sessions

Example: passing "Pre-Production → Production":
```bash
echo -n "Production" > .ags/project/stage.txt
```

**Always ask before writing**: "Gate passed. May I update `.ags/project/stage.txt` to 'Production'?"

---

## 7. Closing Next-Step Widget

After verdict + any stage.txt update, close with `AskUserQuestion`.

**Tailor options to gate:**

For **systems-design PASS**:
```
Gate passed. What would you like to do next?
[A] Run /ags-create-architecture — produce master architecture blueprint and ADR work plan (recommended next step)
[B] Design more GDDs first — return when all MVP systems complete
[C] Stop here for this session
```

> **Note for systems-design PASS**: `/ags-create-architecture` is required next step before writing any ADRs. Produces master architecture doc + prioritized ADR list. Running `/ags-architecture-decision` without this means writing ADRs without blueprint — skip at own risk.

For **technical-setup PASS**:
```
Gate passed. What would you like to do next?
[A] Start Pre-Production — begin prototyping Vertical Slice
[B] Write more ADRs first — run /ags-architecture-decision [next-system]
[C] Stop here for this session
```

For all other gates, two most logical next steps + "Stop here".

---

## 8. Follow-Up Actions

Suggest specific next steps based on verdict:

- **No art bible?** → `/ags-art-bible` to create visual identity spec
- **Art bible exists but no asset specs?** → `/ags-asset-spec system:[name]` to generate per-asset visual specs and prompts from approved GDDs
- **No game concept?** → `/ags-brainstorm`
- **No systems index?** → `/ags-map-systems` to decompose concept
- **Missing design docs?** → `/ags-reverse-document` or delegate to `game-designer`
- **No UX specs?** → `/ux-design [screen name]` or `/team-ui [feature]` for full pipeline
- **UX specs not reviewed?** → `/ux-review [file]` or `/ux-review all`
- **No accessibility requirements doc?** → `AskUserQuestion` to offer creating now:
  - Prompt: "Gate requires `design/accessibility-requirements.md`. Shall I create it from template?"
  - Options: `Create it now — I'll choose accessibility tier`, `I'll create it myself`, `Skip for now`
  - If "Create it now": second `AskUserQuestion` for tier:
    - Prompt: "Which accessibility tier fits this project?"
    - Options: `Basic — remapping + subtitles only (lowest effort)`, `Standard — Basic + colorblind modes + scalable UI`, `Comprehensive — Standard + motor accessibility + full settings menu`, `Exemplary — Comprehensive + external audit + full customization`
  - Then write `design/accessibility-requirements.md` from `.ags/templates/accessibility-requirements.md`, fill chosen tier. Confirm: "May I write `design/accessibility-requirements.md`?"
- **No interaction pattern library?** → `/ux-design patterns`
- **GDDs not cross-reviewed?** → `/ags-review-all-gdds` (run after all MVP GDDs individually approved)
- **Cross-GDD consistency issues?** → fix flagged GDDs, re-run `/ags-review-all-gdds`
- **No test framework?** → `/test-setup`
- **No QA plan for current sprint?** → `/ags-qa-plan sprint` before implementation
- **Missing ADRs?** → `/ags-architecture-decision`
- **No master architecture doc?** → `/ags-create-architecture`
- **ADRs missing engine compatibility sections?** → Re-run `/ags-architecture-decision` or manually add Engine Compatibility sections
- **Missing control manifest?** → `/ags-create-control-manifest` (requires Accepted ADRs)
- **Missing epics?** → `/ags-create-epics layer: foundation` then `/ags-create-epics layer: core` (requires control manifest)
- **Missing stories for epic?** → `/ags-create-stories [epic-slug]` (after each epic created)
- **Stories not implementation-ready?** → `/ags-story-readiness` to validate before devs pick up
- **Tests failing?** → delegate to `lead-programmer` or `qa-lead`
- **No playtest data?** → `/ags-playtest-report`
- **Less than 3 playtest sessions?** → Run more, use `/ags-playtest-report` to structure findings
- **No Difficulty Curve doc?** → Consider creating at `design/difficulty-curve.md` before polish
- **No player journey document?** → create `design/player-journey.md` from template
- **Need quick sprint check?** → `/ags-sprint-status` for progress snapshot
- **Performance unknown?** → `/ags-perf-profile`
- **Not localized?** → `/ags-localize`
- **Ready for release?** → `/ags-launch-checklist`

---

## Collaborative Protocol

Follows collaborative design principle:

1. **Scan first**: Check all artifacts and quality gates
2. **Ask about unknowns**: Don't assume PASS for unverifiable
3. **Present findings**: Show full checklist with status
4. **User decides**: Verdict is recommendation — user makes final call
5. **Get approval**: "May I write this gate check report to .ags/project/gate-checks/?"

**Never** block user from advancing — verdict is advisory. Document risks, let user decide.
