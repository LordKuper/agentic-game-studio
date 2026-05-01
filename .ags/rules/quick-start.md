# Game Studio Agent Architecture — Quick Start

## What Is This?

Complete Claude Code agent architecture for game dev. 21 specialized AI agents in studio hierarchy mirroring real game dev teams. Defined responsibilities, delegation rules, coordination protocols. Current implementation includes `unity-specialist` and `unity-dots-specialist` for Unity projects. All design agents and templates grounded in established game design theory (MDA Framework, Self-Determination Theory, Flow State, Bartle Player Types).

## How to Use

### 1. Understand the Hierarchy

Three tiers:

- **Tier 1 (Opus)**: Directors — high-level decisions
  - `creative-director` — vision and creative conflict resolution
  - `technical-director` — architecture and technology decisions
  - `producer` — scheduling, coordination, risk management

- **Tier 2 (Sonnet)**: Department leads — own their domain
  - `game-designer`, `lead-programmer`, `art-director`, `audio-director`, `narrative-director`, `qa-lead`, `release-manager`

- **Tier 3 (Sonnet/Haiku)**: Specialists — execute within domain
  - Designers, programmers, artists, writers, testers, engineers

### 2. Pick the Right Agent

Ask: "What department would handle this in a real studio?"

| I need to... | Use this agent |
|-------------|---------------|
| Design a new mechanic | `game-designer` |
| Write combat code | `gameplay-programmer` |
| Create a shader | `technical-artist` |
| Write dialogue | `narrative-director` |
| Plan the next epic | `producer` |
| Review code quality | `lead-programmer` |
| Write test cases | `qa-lead` |
| Design a level | `game-designer` |
| Fix a performance problem | `performance-analyst` |
| Set up CI/CD | `tools-programmer` |
| Design a loot table | `systems-designer` |
| Resolve a creative conflict | `creative-director` |
| Make an architecture decision | `technical-director` |
| Manage a release | `release-manager` |
| Prepare strings for translation | `narrative-director` |
| Review code for security issues | `lead-programmer` |
| Check accessibility compliance | `ux-designer` |
| Get Unity advice | `unity-specialist` |
| Design DOTS/ECS architecture | `unity-dots-specialist` |
| Write Unity shaders/VFX, manage Addressables, build UI Toolkit/UGUI | `unity-specialist` |
| Plan live events and seasons | `game-designer` |
| Write patch notes for players | `producer` |
| Brainstorm a new game idea | Use `/ags-brainstorm` skill |

### 3. Use Slash Commands for Common Tasks

| Command | What it does |
|---------|-------------|
| `/ags-start` | First-time onboarding — asks where you are, routes to right workflow |
| `/ags-help` | Context-aware "what next?" — reads stage.md + epics/index.md + active EPIC.md |
| `/ags-project-stage-detect` | Analyze project state, detect phase, identify gaps |
| `/ags-setup-engine` | Configure engine + version, populate reference docs |
| `/ags-adopt` | Brownfield audit and migration plan for existing projects |
| `/ags-brainstorm` | Guided game concept ideation from scratch |
| `/ags-map-systems` | Decompose concept into systems, map dependencies, drive epic planning |
| `/ags-design-system` | Section-by-section GDD authoring (modes: new / revise / retrofit) |
| `/ags-review-all-gdds` | Cross-GDD consistency and game design theory review |
| `/ags-art-bible` | Author art bible: visual identity, palettes, art pipeline standards |
| `/ags-asset-spec` | Author per-asset specs: requirements, source files, references |
| `/ags-propagate-design-change` | Find ADRs and stories affected by GDD change |
| `/ux-design` | Author UX specs (screen/flow, HUD, interaction patterns) |
| `/ux-review` | Validate UX specs for accessibility and GDD alignment |
| `/ags-create-architecture` | Master architecture document — skeleton in Foundation, refresh in Production |
| `/ags-architecture-decision` | Create ADR (per epic in Production) |
| `/ags-architecture-review` | Validate cumulative architecture coherence |
| `/ags-create-control-manifest` | Programmer rules sheet — SEED (Foundation) or REFRESH (Production) modes |
| `/ags-create-epics` | Plan one vertical-slice epic (1-3 systems, modes new / revise / stub) |
| `/ags-epic-contracts` | Lock minimal API for stub-mode systems in active epic |
| `/ags-create-stories` | Break single epic into implementable story files |
| `/ags-dev-story` | Read story and implement — routes to correct programmer agent |
| `/ags-stub-track` | Sync `stubs.md` with `// TODO(epic-...)` markers in code (scan / close / migrate) |
| `/ags-epic-retro` | Run epic retrospective; appends to EPIC.md and decisions-log.md |
| `/ags-story-readiness` | Validate story implementation-ready before pickup |
| `/ags-story-done` | End-of-story completion review — verifies acceptance criteria |
| `/ags-estimate` | Produce structured effort estimates |
| `/ags-design-review` | Review design document |
| `/ags-code-review` | Review code for quality and architecture |
| `/ags-security-audit` | Security review of changeset / branch (OWASP-style for game code) |
| `/ags-balance-check` | Analyze game balance data |
| `/ags-asset-audit` | Audit assets for compliance |
| `/ags-content-audit` | GDD-specified content vs implemented — find gaps |
| `/ags-scope-check` | Detect scope creep against epic plan |
| `/ags-perf-profile` | Performance profiling and bottleneck ID |
| `/tech-debt` | Scan, track, prioritize tech debt; reads `stubs.md` for stub debt |
| `/ags-gate-check` | Validate phase or epic-done readiness (PASS / CONCERNS / FAIL) |
| `/ags-consistency-check` | Scan all GDDs for cross-document inconsistencies |
| `/ags-reverse-document` | Generate design/architecture docs from existing code |
| `/ags-milestone-review` | Review milestone progress |
| `/ags-bug-report` | Structured bug report creation |
| `/ags-playtest-report` | Create or analyze playtest feedback |
| `/ags-onboard` | Generate onboarding docs for a role |
| `/ags-release-checklist` | Validate pre-release checklist |
| `/ags-launch-checklist` | Complete launch readiness validation |
| `/ags-changelog` | Generate changelog from git history |
| `/ags-patch-notes` | Generate player-facing patch notes |
| `/ags-day-one-patch` | Plan and prepare day-one / launch patch |
| `/ags-hotfix` | Emergency fix with audit trail |
| `/ags-localize` | Localization scan, extract, validate |
| `/ags-team-combat` | Orchestrate full combat team pipeline |
| `/ags-team-narrative` | Orchestrate full narrative team pipeline |
| `/team-ui` | Orchestrate full UI team pipeline |
| `/ags-team-release` | Orchestrate full release team pipeline |
| `/ags-team-polish` | Orchestrate full polish team pipeline |
| `/ags-team-audio` | Orchestrate full audio team pipeline |
| `/ags-team-level` | Orchestrate full level creation pipeline |
| `/ags-team-live-ops` | Orchestrate live-ops team for seasons, events, post-launch content |
| `/ags-team-qa` | Orchestrate full QA team cycle — test plan, test cases, smoke check, sign-off |
| `/ags-qa-plan` | Generate QA test plan for an epic |
| `/ags-bug-triage` | Re-prioritize open bugs, surface systemic trends |
| `/ags-smoke-check` | Critical path smoke test gate before QA hand-off |
| `/ags-soak-test` | Soak test protocol for extended play sessions |
| `/ags-regression-suite` | Map coverage to GDD critical paths, flag gaps |
| `/test-setup` | Scaffold test framework + CI pipeline (run once) |
| `/test-helpers` | Generate engine-specific test helper libraries and factories |
| `/test-flakiness` | Detect flaky tests from CI history |
| `/test-evidence-review` | Quality review of test files and manual evidence |
| `/ags-skill-test` | Validate skill files for compliance and correctness |
| `/ags-skill-improve` | Improve skill via test-fix-retest loop |

### 4. Use Templates for New Documents

Templates in `.ags/templates/`:

- `t_concept.md` — initial game concept (MDA, SDT, Flow, Bartle)
- `t_engine.md` — engine selection
- `t_user-interaction.md` — user collaboration preferences (filled by /ags-start)
- `t_epic.md` — vertical-slice epic (1-3 systems, modes new / revise / stub)
- `t_stubs.md` — TODO stubs registry
- `t_decisions-log.md` — append-only decisions chronology
- `t_gdd.md` — for new mechanics and systems
- `t_adr.md` — for technical decisions
- `t_architecture-traceability.md` — maps GDD requirements to ADRs to story IDs
- `t_risk-register.md` — for new risks
- `t_character-sheet.md` — for new characters
- `t_test-plan.md` — for feature test plans
- `t_milestone.md` — for new milestones (groups of epics)
- `t_level-design.md` — for new levels
- `t_game-pillars.md` — for core design pillars
- `t_art-bible.md` — for visual style reference
- `t_tech-design.md` — for per-system technical designs
- `t_post-mortem.md` — for project/milestone retrospectives
- `t_sound-bible.md` — for audio style reference
- `t_release-checklist.md` — for platform release checklists
- `t_patch-notes.md` — for player-facing patch notes
- `t_release-notes.md` — for player-facing release notes
- `t_incident-response.md` — for live incident response playbooks
- `t_pitch.md` — for pitching game to stakeholders
- `t_economy-model.md` — for virtual economy design (sink/faucet model)
- `t_faction-design.md` — for faction identity, lore, gameplay role
- `t_systems-index.md` — for systems decomposition and dependency mapping
- `t_design-from-implementation.md` — for reverse-documenting code into GDDs
- `t_architecture-from-code.md` — for reverse-documenting code into architecture docs
- `t_ux-spec.md` — for per-screen UX specs (layout zones, states, events)
- `t_hud-design.md` — for whole-game HUD philosophy, zones, element specs
- `t_accessibility-requirements.md` — for project-wide accessibility tier and feature matrix
- `t_interaction-patterns.md` — for standard UI controls and game-specific patterns
- `t_player-journey.md` — for 6-phase emotional arc and retention hooks by time scale
- `t_difficulty-curve.md` — for difficulty axes, onboarding ramp, cross-system interactions
- `t_test-evidence.md` — template for recording manual test evidence (screenshots, walkthrough notes)

Also in `.ags/templates/collaborative-protocols/` (used by agents, not edited directly):

- `design-agent-protocol.md` — question-options-draft-approval cycle for design agents
- `implementation-agent-protocol.md` — story pickup through `/ags-story-done` cycle for programming agents
- `leadership-agent-protocol.md` — cross-department delegation and escalation for director-tier agents

### 5. Follow the Coordination Rules

1. Work flows down hierarchy: Directors → Leads → Specialists
2. Conflicts escalate up
3. Cross-department work coordinated by `producer`
4. Agents do not modify files outside domain without delegation
5. All decisions documented (in `decisions-log.md`, ADRs, EPIC.md retros)

## First Steps for a New Project

**Don't know where to begin?** Run `/ags-start`. Asks where you are, routes you to right workflow. No assumptions about game, engine, or experience level.

If you know what you need, jump to relevant path:

### Path A: "I have no idea what to build"

1. **Run `/ags-start`** (or `/ags-brainstorm open`) — guided creative exploration. Generates 3 concepts, helps pick one, defines core loop and pillars. Produces `design/gdd/game-concept.md`.
2. **Set up engine** — `/ags-setup-engine [version]` (studio is Unity-only).
3. **Validate concept** — `/ags-design-review design/gdd/game-concept.md`.
4. **Decompose into systems** — `/ags-map-systems`.
5. **Author art bible** — `/ags-art-bible`.
6. **Phase gate** — `/ags-gate-check foundation` (when concept artifacts complete).
7. **Foundation** — `/ags-create-architecture` (skeleton), `/ags-create-control-manifest` (seed), commit accessibility tier, `/test-setup`.
8. **Phase gate** — `/ags-gate-check production`.
9. **Plan first epic** — `/ags-create-epics`. Pick 1-3 systems with modes new / revise / stub.
10. **Implement loop**: `/ags-epic-contracts` → `/ags-design-system` → `/ags-architecture-decision` → `/ags-create-stories` → `/ags-dev-story` → `/ags-stub-track` → `/ags-playtest-report` → `/ags-epic-retro` → `/ags-gate-check epic-done` → next epic.

### Path B: "I know what I want to build"

If you have game concept and engine choice:

1. **Set up engine** — `/ags-setup-engine [version]`.
2. **Write Game Pillars** — delegate to `creative-director`.
3. **Decompose into systems** — `/ags-map-systems`.
4. **Author art bible** — `/ags-art-bible`.
5. **Phase gate** — `/ags-gate-check foundation`.
6. **Foundation artifacts** — `/ags-create-architecture` skeleton, `/ags-create-control-manifest seed`, accessibility tier, `/test-setup`.
7. **Phase gate** — `/ags-gate-check production`.
8. **Plan first epic** — `/ags-create-epics`. Loop epics until feature-complete.

### Path C: "I have an existing project"

If you have design docs or code:

1. **Run `/ags-start`** (or `/ags-project-stage-detect`) — analyzes what exists, identifies gaps, recommends steps.
2. **Run `/ags-adopt`** if you have existing GDDs, ADRs, or stories — audits format compliance and builds numbered migration plan.
3. **Configure engine if needed** — `/ags-setup-engine`.
4. **Validate phase readiness** — `/ags-gate-check` to see where you stand.
5. **Plan next epic** — `/ags-create-epics` once in production phase.

## File Structure Reference

See `.ags/rules/directory-structure.md` for full canonical layout. Quick map:

```
CLAUDE.md                          -- Master config (read first)
.claude/
  settings.json                    -- Claude Code hooks and project settings
  agents/                          -- 21 agent definitions
  skills/                          -- Slash-command skill definitions
  hooks/                           -- Hook scripts (.sh) wired by settings.json
  hooks-reference/                 -- Hook reference docs
.ags/
  rules/                           -- Project rules (this file lives here)
    technical-preferences.md       -- Project standards (populated by /ags-setup-engine)
    coding.md                      -- Coding standards (incl. TODO-stub rules)
    coordination.md                -- Agent coordination rules (incl. epic cycle)
    context-management.md          -- Context budgets and compaction
    directory-structure.md         -- Canonical directory layout
    workflow-catalog.yaml          -- 5-phase pipeline (concept / foundation / production / polish / release)
    setup-requirements.md          -- System prerequisites
    settings-local-template.md     -- Personal settings.local.json guide
    director-gates.md              -- Director gate prompt catalog
    review-workflow.md             -- Sign-off matrix
  templates/                       -- Document templates (t_*.md and others)
  docs/                            -- Engine reference snapshots
  project/                         -- Working state
    state.md                       -- Active session
    stage.md                       -- Phase + active epic + history
    epics/                         -- Vertical-slice epics + stories
      index.md                     -- Registry of all epics
      [slug]/EPIC.md               -- Epic definition
      [slug]/stories/              -- Story files for epic
    stubs.md                       -- TODO stub registry
    decisions-log.md               -- Append-only decisions chronology
design/                            -- Game design documents (gdd, architecture, art, ux, narrative)
tests/                             -- Test code
<engine project root>              -- Engine source root, e.g. Assets/ for Unity
```
