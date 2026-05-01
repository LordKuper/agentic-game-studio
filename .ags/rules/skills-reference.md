# Available Skills (Slash Commands)

70 slash commands by phase. Type `/` in Claude Code to access.

## Onboarding & Navigation

| Command | Purpose |
|---------|---------|
| `/ags-start` | First-time onboarding — asks where you are, routes to right workflow |
| `/ags-help` | Context-aware "what next?" — reads stage, surfaces required next step |
| `/ags-project-stage-detect` | Full project audit — detect phase, identify gaps, recommend steps |
| `/ags-setup-engine` | Configure Unity version, detect knowledge gaps, populate version-aware reference docs |
| `/ags-adopt` | Brownfield format audit — checks structure of existing GDDs/ADRs/stories, produces migration plan |

## Game Design

| Command | Purpose |
|---------|---------|
| `/ags-brainstorm` | Guided ideation via studio methods (MDA, SDT, Bartle, verb-first) |
| `/ags-map-systems` | Decompose concept into systems, map dependencies, prioritize design order |
| `/ags-design-system` | Section-by-section GDD authoring for single game system |
| `/ags-review-all-gdds` | Cross-GDD consistency and game design holism review |
| `/ags-propagate-design-change` | When GDD revised, find affected ADRs, produce impact report |
| `/ags-consistency-check` | Scan all GDDs against entity registry for cross-document contradictions |
| `/ags-balance-check` | Analyze balance data, formulas, config — flag outliers |

## Art & UX

| Command | Purpose |
|---------|---------|
| `/ags-art-bible` | Author art bible: visual identity, palettes, art pipeline standards |
| `/ags-asset-spec` | Author per-asset specs: requirements, constraints, source files, references |
| `/ags-asset-audit` | Audit assets for naming, file size budgets, pipeline compliance |
| `/ux-design` | Section-by-section UX spec authoring (screen/flow, HUD, pattern library) |
| `/ux-review` | Validate UX specs for GDD alignment, accessibility, pattern compliance |

## Architecture

| Command | Purpose |
|---------|---------|
| `/ags-create-architecture` | Guided authoring of master architecture document |
| `/ags-architecture-decision` | Create Architecture Decision Record (ADR) |
| `/ags-architecture-review` | Validate all ADRs for completeness, dependency ordering, GDD coverage |
| `/ags-create-control-manifest` | Generate flat programmer rules sheet from accepted ADRs |

## Stories & Sprints

| Command | Purpose |
|---------|---------|
| `/ags-create-epics` | Translate GDDs + ADRs into epics — one per architectural module |
| `/ags-create-stories` | Break single epic into implementable story files |
| `/ags-dev-story` | Read story and implement — routes to correct programmer agent |
| `/ags-sprint-plan` | Generate or update sprint plan; initialize sprint-status.yaml |
| `/ags-sprint-status` | Fast 30-line sprint snapshot (reads sprint-status.yaml) |
| `/ags-story-readiness` | Validate story implementation-ready before pickup (READY/NEEDS WORK/BLOCKED) |
| `/ags-story-done` | 8-phase completion review after implementation; updates story file, surfaces next |
| `/ags-estimate` | Structured effort estimate with complexity, dependencies, risk |

## Reviews & Analysis

| Command | Purpose |
|---------|---------|
| `/ags-design-review` | Review GDD for completeness and consistency |
| `/ags-code-review` | Architectural code review for file or changeset |
| `/ags-security-audit` | Security review of changeset or branch (OWASP-style for game code) |
| `/ags-content-audit` | Audit GDD-specified content counts vs implemented |
| `/ags-scope-check` | Analyze feature/sprint scope vs original plan, flag scope creep |
| `/ags-perf-profile` | Structured performance profiling with bottleneck ID |
| `/tech-debt` | Scan, track, prioritize, report tech debt |
| `/ags-gate-check` | Validate readiness to advance phases (PASS/CONCERNS/FAIL) |

## QA & Testing

| Command | Purpose |
|---------|---------|
| `/ags-qa-plan` | Generate QA test plan for sprint or feature |
| `/ags-smoke-check` | Critical path smoke test gate before QA hand-off |
| `/ags-soak-test` | Soak test protocol for extended play sessions |
| `/ags-regression-suite` | Map test coverage to GDD critical paths, identify fixed bugs without regression tests |
| `/test-setup` | Scaffold test framework + CI/CD pipeline for Unity |
| `/test-helpers` | Generate Unity test helper libraries |
| `/test-evidence-review` | Quality review of test files and manual evidence |
| `/test-flakiness` | Detect non-deterministic (flaky) tests from CI logs |
| `/ags-skill-test` | Validate skill files for compliance and correctness |
| `/ags-skill-improve` | Improve skill via test-fix-retest loop |

## Production

| Command | Purpose |
|---------|---------|
| `/ags-milestone-review` | Review milestone progress, generate status report |
| `/ags-retrospective` | Run structured sprint or milestone retrospective |
| `/ags-bug-report` | Create structured bug report |
| `/ags-bug-triage` | Read all open bugs, re-evaluate priority vs severity, assign owner and label |
| `/ags-reverse-document` | Generate design or architecture docs from existing implementation |
| `/ags-playtest-report` | Generate structured playtest report or analyze existing notes |

## Release

| Command | Purpose |
|---------|---------|
| `/ags-release-checklist` | Generate and validate pre-release checklist for current build |
| `/ags-launch-checklist` | Complete launch readiness validation across all departments |
| `/ags-changelog` | Auto-generate changelog from git commits and sprint data |
| `/ags-patch-notes` | Generate player-facing patch notes from git history and internal data |
| `/ags-day-one-patch` | Plan and prepare day-one / launch patch (scope, risk, ship gate) |
| `/ags-hotfix` | Emergency fix workflow with audit trail, bypass normal sprint |

## Creative & Content

| Command | Purpose |
|---------|---------|
| `/ags-onboard` | Generate contextual onboarding doc for new contributor or agent |
| `/ags-localize` | Localization workflow: string extraction, validation, translation readiness |

## Team Orchestration

Coordinate multiple agents on single feature area:

| Command | Coordinates |
|---------|-------------|
| `/ags-team-combat` | game-designer + gameplay-programmer + ai-programmer + technical-artist + audio-director + qa-lead |
| `/ags-team-narrative` | narrative-director + game-designer |
| `/team-ui` | ux-designer + ui-programmer + art-director |
| `/ags-team-release` | release-manager + qa-lead + tools-programmer + producer |
| `/ags-team-polish` | performance-analyst + technical-artist + audio-director + qa-lead |
| `/ags-team-audio` | audio-director + technical-artist + gameplay-programmer |
| `/ags-team-level` | game-designer + narrative-director + art-director + systems-designer + qa-lead |
| `/ags-team-live-ops` | game-designer + systems-designer + producer |
| `/ags-team-qa` | qa-lead + gameplay-programmer + producer |
