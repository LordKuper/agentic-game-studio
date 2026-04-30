# Available Skills (Slash Commands)

70 slash commands by phase. Type `/` in Claude Code to access.

## Onboarding & Navigation

| Command | Purpose |
|---------|---------|
| `/ags-start` | First-time onboarding — asks where you are, routes to right workflow |
| `/help` | Context-aware "what next?" — reads stage, surfaces required next step |
| `/project-stage-detect` | Full project audit — detect phase, identify gaps, recommend steps |
| `/setup-engine` | Configure Unity version, detect knowledge gaps, populate version-aware reference docs |
| `/adopt` | Brownfield format audit — checks structure of existing GDDs/ADRs/stories, produces migration plan |

## Game Design

| Command | Purpose |
|---------|---------|
| `/brainstorm` | Guided ideation via studio methods (MDA, SDT, Bartle, verb-first) |
| `/map-systems` | Decompose concept into systems, map dependencies, prioritize design order |
| `/design-system` | Section-by-section GDD authoring for single game system |
| `/review-all-gdds` | Cross-GDD consistency and game design holism review |
| `/propagate-design-change` | When GDD revised, find affected ADRs, produce impact report |
| `/consistency-check` | Scan all GDDs against entity registry for cross-document contradictions |
| `/balance-check` | Analyze balance data, formulas, config — flag outliers |

## Art & UX

| Command | Purpose |
|---------|---------|
| `/art-bible` | Author art bible: visual identity, palettes, art pipeline standards |
| `/asset-spec` | Author per-asset specs: requirements, constraints, source files, references |
| `/asset-audit` | Audit assets for naming, file size budgets, pipeline compliance |
| `/ux-design` | Section-by-section UX spec authoring (screen/flow, HUD, pattern library) |
| `/ux-review` | Validate UX specs for GDD alignment, accessibility, pattern compliance |

## Architecture

| Command | Purpose |
|---------|---------|
| `/create-architecture` | Guided authoring of master architecture document |
| `/architecture-decision` | Create Architecture Decision Record (ADR) |
| `/architecture-review` | Validate all ADRs for completeness, dependency ordering, GDD coverage |
| `/create-control-manifest` | Generate flat programmer rules sheet from accepted ADRs |

## Stories & Sprints

| Command | Purpose |
|---------|---------|
| `/create-epics` | Translate GDDs + ADRs into epics — one per architectural module |
| `/create-stories` | Break single epic into implementable story files |
| `/dev-story` | Read story and implement — routes to correct programmer agent |
| `/sprint-plan` | Generate or update sprint plan; initialize sprint-status.yaml |
| `/sprint-status` | Fast 30-line sprint snapshot (reads sprint-status.yaml) |
| `/story-readiness` | Validate story implementation-ready before pickup (READY/NEEDS WORK/BLOCKED) |
| `/story-done` | 8-phase completion review after implementation; updates story file, surfaces next |
| `/estimate` | Structured effort estimate with complexity, dependencies, risk |

## Reviews & Analysis

| Command | Purpose |
|---------|---------|
| `/design-review` | Review GDD for completeness and consistency |
| `/code-review` | Architectural code review for file or changeset |
| `/security-audit` | Security review of changeset or branch (OWASP-style for game code) |
| `/content-audit` | Audit GDD-specified content counts vs implemented |
| `/scope-check` | Analyze feature/sprint scope vs original plan, flag scope creep |
| `/perf-profile` | Structured performance profiling with bottleneck ID |
| `/tech-debt` | Scan, track, prioritize, report tech debt |
| `/gate-check` | Validate readiness to advance phases (PASS/CONCERNS/FAIL) |

## QA & Testing

| Command | Purpose |
|---------|---------|
| `/qa-plan` | Generate QA test plan for sprint or feature |
| `/smoke-check` | Critical path smoke test gate before QA hand-off |
| `/soak-test` | Soak test protocol for extended play sessions |
| `/regression-suite` | Map test coverage to GDD critical paths, identify fixed bugs without regression tests |
| `/test-setup` | Scaffold test framework + CI/CD pipeline for Unity |
| `/test-helpers` | Generate Unity test helper libraries |
| `/test-evidence-review` | Quality review of test files and manual evidence |
| `/test-flakiness` | Detect non-deterministic (flaky) tests from CI logs |
| `/skill-test` | Validate skill files for compliance and correctness |
| `/skill-improve` | Improve skill via test-fix-retest loop |

## Production

| Command | Purpose |
|---------|---------|
| `/milestone-review` | Review milestone progress, generate status report |
| `/retrospective` | Run structured sprint or milestone retrospective |
| `/bug-report` | Create structured bug report |
| `/bug-triage` | Read all open bugs, re-evaluate priority vs severity, assign owner and label |
| `/reverse-document` | Generate design or architecture docs from existing implementation |
| `/playtest-report` | Generate structured playtest report or analyze existing notes |

## Release

| Command | Purpose |
|---------|---------|
| `/release-checklist` | Generate and validate pre-release checklist for current build |
| `/launch-checklist` | Complete launch readiness validation across all departments |
| `/changelog` | Auto-generate changelog from git commits and sprint data |
| `/patch-notes` | Generate player-facing patch notes from git history and internal data |
| `/day-one-patch` | Plan and prepare day-one / launch patch (scope, risk, ship gate) |
| `/hotfix` | Emergency fix workflow with audit trail, bypass normal sprint |

## Creative & Content

| Command | Purpose |
|---------|---------|
| `/onboard` | Generate contextual onboarding doc for new contributor or agent |
| `/localize` | Localization workflow: string extraction, validation, translation readiness |

## Team Orchestration

Coordinate multiple agents on single feature area:

| Command | Coordinates |
|---------|-------------|
| `/team-combat` | game-designer + gameplay-programmer + ai-programmer + technical-artist + audio-director + qa-lead |
| `/team-narrative` | narrative-director + game-designer |
| `/team-ui` | ux-designer + ui-programmer + art-director |
| `/team-release` | release-manager + qa-lead + tools-programmer + producer |
| `/team-polish` | performance-analyst + technical-artist + audio-director + qa-lead |
| `/team-audio` | audio-director + technical-artist + gameplay-programmer |
| `/team-level` | game-designer + narrative-director + art-director + systems-designer + qa-lead |
| `/team-live-ops` | game-designer + systems-designer + producer |
| `/team-qa` | qa-lead + gameplay-programmer + producer |
