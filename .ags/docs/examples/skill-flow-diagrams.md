# Skill Flow Diagrams

Visual maps of skill chains across 7 dev phases. Show what runs before/after, what artifacts flow.

---

## Full Pipeline Overview (Zero to Ship)

```
PHASE 1: CONCEPT
  /ags-start ──────────────────────────────────────────────────────► routes to A/B/C/D
  /brainstorm ──────────────────────────────────────────────────► design/gdd/game-concept.md
  /setup-engine ────────────────────────────────────────────────► CLAUDE.md + technical-preferences.md
  /design-review [game-concept.md] ────────────────────────────► concept validated
  /gate-check ─────────────────────────────────────────────────► PASS → advance to systems-design
        │
        ▼
PHASE 2: SYSTEMS DESIGN
  /map-systems ────────────────────────────────────────────────► design/gdd/systems-index.md
        │
        ▼ (per system, dep order)
  /design-system [name] ──────────────────────────────────────► design/gdd/[system].md
  /design-review [system].md ─────────────────────────────────► per-GDD review comments
        │
        ▼ (after all MVP GDDs done)
  /review-all-gdds ────────────────────────────────────────────► design/gdd/gdd-cross-review-[date].md
  /gate-check ─────────────────────────────────────────────────► PASS → advance to technical-setup
        │
        ▼
PHASE 3: TECHNICAL SETUP
  /create-architecture ────────────────────────────────────────► design/architecture/master.md
  /architecture-decision (×N) ─────────────────────────────────► design/architecture/[adr-nnn].md
  /architecture-review ────────────────────────────────────────► review report + design/architecture/tr-registry.yaml
  /create-control-manifest ────────────────────────────────────► design/architecture/control-manifest.md
  /gate-check ─────────────────────────────────────────────────► PASS → advance to pre-production
        │
        ▼
PHASE 4: PRE-PRODUCTION
  [UX — before epics, so specs exist when stories written]
  /ux-design [screen/hud/patterns] ────────────────────────────► design/ux/*.md
  /ux-review ──────────────────────────────────────────────────► UX specs approved (HARD gate for /team-ui)

  [Test infrastructure — scaffold before stories ref tests]
  /test-setup ─────────────────────────────────────────────────► test framework + CI/CD pipeline
  /test-helpers ───────────────────────────────────────────────► tests/Helpers/[engine-specific].cs

  [Stories]
  /create-epics [layer] ───────────────────────────────────────► .ags/project/epics/*/EPIC.md
  /create-stories [epic-slug] ─────────────────────────────────► .ags/project/epics/*/story-*.md
  /playtest-report ────────────────────────────────────────────► tests/playtest/vertical-slice.md
  /sprint-plan new ────────────────────────────────────────────► .ags/project/sprints/sprint-01.md
  /gate-check ─────────────────────────────────────────────────► PASS → advance to production
        │
        ▼
PHASE 5: PRODUCTION (sprint loop)
  /sprint-status ──────────────────────────────────────────────► sprint snapshot
  /story-readiness [story] ────────────────────────────────────► story validated READY
        │
        ▼ (pick up + implement)
  /dev-story [story] ──────────────────────────────────────────► routes to correct programmer agent
        │
        ▼ (during impl, as needed)
  /code-review ────────────────────────────────────────────────► code review report
  /scope-check ────────────────────────────────────────────────► scope creep detected / clear
  /content-audit ──────────────────────────────────────────────► GDD content gaps
  /bug-report ─────────────────────────────────────────────────► .ags/project/qa/bugs/bug-NNN.md
  /bug-triage ─────────────────────────────────────────────────► bugs re-prioritized + assigned

  [Team skills for feature areas]
  /team-combat / /team-narrative / /team-ui / /team-level / /team-audio

  [QA cycle per sprint]
  /qa-plan ────────────────────────────────────────────────────► .ags/project/qa/qa-plan-sprint-NN.md
  /smoke-check ────────────────────────────────────────────────► smoke test gate (PASS/FAIL)
  /regression-suite ───────────────────────────────────────────► coverage gaps + missing regression tests
  /test-evidence-review ───────────────────────────────────────► evidence quality report
  /test-flakiness ─────────────────────────────────────────────► flaky test report
        │
        ▼
  /story-done [story] ─────────────────────────────────────────► story closed + next surfaced
  /sprint-plan [next] ─────────────────────────────────────────► next sprint
        │
        ▼ (after Production milestone)
  /milestone-review ───────────────────────────────────────────► milestone report
  /gate-check ─────────────────────────────────────────────────► PASS → advance to polish
        │
        ▼
PHASE 6: POLISH
  /perf-profile ───────────────────────────────────────────────► perf report + fixes
  /balance-check ──────────────────────────────────────────────► balance report + fixes
  /asset-audit ────────────────────────────────────────────────► asset compliance report
  /tech-debt ──────────────────────────────────────────────────► docs/tech-debt-register.md
  /soak-test ──────────────────────────────────────────────────► soak test protocol + results
  /localize ───────────────────────────────────────────────────► localization readiness report
  /team-polish ────────────────────────────────────────────────► polish sprint orchestrated
  /team-qa ────────────────────────────────────────────────────► full QA cycle sign-off
  /gate-check ─────────────────────────────────────────────────► PASS → advance to release
        │
        ▼
PHASE 7: RELEASE
  /launch-checklist ───────────────────────────────────────────► launch readiness report
  /release-checklist ──────────────────────────────────────────► platform-specific checklist
  /changelog ──────────────────────────────────────────────────► CHANGELOG.md
  /patch-notes ────────────────────────────────────────────────► player-facing notes
  /team-release ───────────────────────────────────────────────► release pipeline orchestrated
        │
        ▼ (post-launch, ongoing)
  /hotfix ─────────────────────────────────────────────────────► emergency fix with audit trail
  /team-live-ops ──────────────────────────────────────────────► live-ops content plan
```

---

## Skill Chain: /design-system in Detail

How single GDD authored, reviewed, handed to architecture:

```
systems-index.md (input)
game-concept.md (input)
upstream GDDs (input, if any)
        │
        ▼
/design-system [name]
        │
        ├── Pre-check: feasibility table + engine risk flags
        │
        ├── Section cycle × 8:
        │     question → options → decision → draft → approval → WRITE
        │     [each section to file immediately after approval]
        │
        └── Output: design/gdd/[system].md (complete, 8 sections)
                │
                ▼
        /design-review design/gdd/[system].md
                │
                ├── APPROVED → mark DONE in systems-index, next system
                ├── NEEDS REVISION → agent shows issues, re-enter section cycle
                └── MAJOR REVISION → significant redesign before next system
                        │
                        ▼ (after all MVP GDDs + cross-review)
                /review-all-gdds
                        │
                        └── Output: gdd-cross-review-[date].md
```

---

## Skill Chain: UX / UI Pipeline in Detail

UX specs authored Phase 4 (Pre-Production), before epics, so story AC can ref specific UX artifacts.

```
design/gdd/*.md (UI/UX requirements extracted)
design/player-journey.md (emotional arc, if authored)
        │
        ▼
/ux-design hud              → design/ux/hud.md
/ux-design screen [name]    → design/ux/screens/[name].md
/ux-design patterns         → design/ux/interaction-patterns.md
        │
        ▼
/ux-review design/ux/
        │
        ├── APPROVED → UX specs ready, proceed to /create-epics
        ├── NEEDS REVISION → blocking issues listed → fix → re-run
        └── MAJOR REVISION → fundamental UX problems → redesign before epics
                │
                ▼ (after APPROVED — Phase 5 when implementing UI)
        /team-ui
                │
                ├── Phase 1: /ux-design (if specs missing) + /ux-review
                ├── Phase 2: visual design (art-director)
                ├── Phase 3: layout impl (ui-programmer)
                ├── Phase 4: accessibility audit (ux-designer)
                └── Phase 5: final review

Note: /ux-design and /ux-review = Phase 4 (Pre-Production).
      /team-ui = Phase 5 (Production) when UI feature being built.
```

---

## Skill Chain: Dev Story Flow in Detail

How story moves backlog → closed:

```
/story-readiness [story]
        │
        ├── READY → Status: ready-for-dev → pick up
        ├── NEEDS WORK → agent shows gaps → resolve → re-run
        └── BLOCKED → ADR still Proposed, or upstream story incomplete
                │
                ▼ (after READY)
        /dev-story [story]
                │
                ├── Reads: story file, linked GDD requirement, ADR decisions, control manifest
                ├── Routes to: gameplay-programmer / engine-programmer / ui-programmer / etc.
                │
                └── Implementation begins
                        │
                        ▼ (optional, during/after impl)
                /code-review          → architectural review of changeset
                /scope-check          → verify no scope creep vs original AC
                /test-evidence-review → validate test files + manual evidence
                        │
                        ▼
                /story-done [story]
                        │
                        ├── COMPLETE → Status: Complete, sprint-status.yaml updated, next surfaced
                        ├── COMPLETE WITH NOTES → complete but criteria deferred (logged)
                        └── BLOCKED → AC can't be verified → investigate
```

---

## Skill Chain: Story Lifecycle (Backlog to Closed)

Story backlog → closed (summary):

```
/create-epics [layer]
        │
        └── Output: .ags/project/epics/[slug]/EPIC.md
                │
                ▼
        /create-stories [epic-slug]
                │
                └── Output: .ags/project/epics/[slug]/story-NNN-[slug].md
                            (Status: Ready or Blocked if ADR Proposed)
                │
                ▼
        /story-readiness [story]
                │
                ├── READY → /dev-story → implement → /story-done
                ├── NEEDS WORK → resolve gaps → re-run
                └── BLOCKED → fix upstream dep first
```

---

## Skill Chain: QA Pipeline in Detail

```
[Phase 4 — one-time infrastructure setup]
/test-setup ────────────────────────────────────────────────────► test framework scaffolded + CI/CD wired
/test-helpers ──────────────────────────────────────────────────► tests/Helpers/[engine].cs (NUnit / Unity Test Framework)

[Phase 5 — per-sprint QA cycle]
/qa-plan [sprint or feature]
        │
        ├── Reads: story files, GDDs, AC
        ├── Classifies each story by test type:
        │     Logic → automated unit test (BLOCKING)
        │     Integration → integration test or documented playtest (BLOCKING)
        │     Visual/Feel → screenshot + lead sign-off (ADVISORY)
        │     UI → manual walkthrough or interaction test (ADVISORY)
        │     Config/Data → smoke check (ADVISORY)
        └── Output: .ags/project/qa/qa-plan-sprint-NN.md
                │
                ▼
        /smoke-check
                │
                ├── PASS → QA hand-off cleared
                └── FAIL → block sprint close → fix critical paths first
                        │
                        ▼
                /regression-suite
                        │
                        └── Coverage gaps + fixed bugs without regression tests
                                │
                                ▼
                        /test-evidence-review
                                │
                                └── Validates evidence quality, not existence
                                        │
                                        ▼ (if CI run history available)
                        /test-flakiness
                                │
                                └── Flaky test report + fix recs

[Phase 6 — extended stability]
/soak-test ─────────────────────────────────────────────────────► soak test protocol + observed results
/team-qa ───────────────────────────────────────────────────────► full QA cycle sign-off for release gate

[Ongoing — bug mgmt]
/bug-report ────────────────────────────────────────────────────► .ags/project/qa/bugs/bug-NNN.md
/bug-triage ────────────────────────────────────────────────────► open bugs re-prioritized + assigned

[Meta — harness validation]
/skill-test [lint|spec|catalog] ────────────────────────────────► skill file structural + behavioral check
```

---

## Skill Chain: UX Pipeline in Detail (Legacy Reference)

```
design/gdd/*.md (UX requirements extracted)
design/player-journey.md (emotional arc)
        │
        ▼
/ux-design hud              → design/ux/hud.md
/ux-design screen [name]    → design/ux/screens/[name].md
/ux-design patterns         → design/ux/interaction-patterns.md
        │
        ▼
/ux-review design/ux/
        │
        ├── APPROVED → all specs ready for /team-ui
        ├── NEEDS REVISION → blocking issues listed → fix → re-run
        └── MAJOR REVISION → fundamental UX problems → significant redesign
                │
                ▼ (after APPROVED)
        /team-ui
                │
                ├── Phase 1: context load + /ux-design (if specs missing)
                ├── Phase 2: visual design (art-director)
                ├── Phase 3: layout impl (ui-programmer)
                ├── Phase 4: accessibility audit (ux-designer)
                └── Phase 5: final review
```

---

## Brownfield Onboarding Flow

For projects with existing work (use `/ags-start` option D or run directly):

```
/project-stage-detect    → stage detection report
        │
        ▼
/adopt
        │
        ├── Phase 1: detect what exists
        ├── Phase 2: FORMAT audit (not just existence)
        ├── Phase 3: classify gaps (BLOCKING / HIGH / MEDIUM / LOW)
        ├── Phase 4: ordered migration plan
        ├── Phase 5: write docs/adoption-plan-[date].md
        └── Phase 6: fix most urgent gap inline (optional)
                │
                ▼
        /design-system retrofit [path]    → fills missing GDD sections
        /architecture-decision retrofit [path] → fills missing ADR sections
        /gate-check                       → where are you in the pipeline?
```

---

## How to Read

| Symbol | Meaning |
|--------|---------|
| `──►` | Produces this artifact |
| `│ ▼` | Flows into next step |
| `├──` | Branch (multiple outcomes) |
| `×N` | Runs N times (per system, story, etc.) |
| `(input)` | Read by skill but not produced here |
| `[optional]` | Not required for gate to pass |
| `WRITE` (caps) | File written to disk immediately |

---

## Common Entry Points

| Where you are | Run this |
|---------------|---------|
| Brand new, no idea | `/ags-start` → `/brainstorm` |
| Have concept, no engine | `/setup-engine` |
| Have concept + engine | `/map-systems` |
| Mid-systems design | `/design-system [next system]` or `/map-systems next` |
| All GDDs done | `/review-all-gdds` → `/gate-check` |
| In technical setup | `/create-architecture` → `/architecture-decision` |
| Starting UX design | `/ux-design screen [name]` or `/ux-design hud` |
| Scaffolding tests | `/test-setup` → `/test-helpers` |
| Stories ready, ready to code | `/story-readiness [story]` → `/dev-story [story]` |
| Story done | `/story-done [story]` |
| Sprint QA | `/qa-plan` → `/smoke-check` → `/regression-suite` |
| Bug backlog needs sorting | `/bug-triage` |
| Extended stability | `/soak-test` |
| Not sure | `/help` |
| Existing project | `/adopt` |
