# Game Studio Agent Architecture -- Quick Start Guide

## What Is This?

This is a complete Claude Code agent architecture for game development. It
organizes 21 specialized AI agents into a studio hierarchy that mirrors
real game development teams, with defined responsibilities, delegation
rules, and coordination protocols. The current implementation includes the
`unity-specialist` and `unity-dots-specialist` engine specialists for Unity
projects. All design agents and templates are grounded in established game
design theory (MDA Framework, Self-Determination Theory, Flow State, Bartle
Player Types).

## How to Use

### 1. Understand the Hierarchy

There are three tiers of agents:

- **Tier 1 (Opus)**: Directors who make high-level decisions
  - `creative-director` -- vision and creative conflict resolution
  - `technical-director` -- architecture and technology decisions
  - `producer` -- scheduling, coordination, and risk management

- **Tier 2 (Sonnet)**: Department leads who own their domain
  - `game-designer`, `lead-programmer`, `art-director`, `audio-director`,
    `narrative-director`, `qa-lead`, `release-manager`

- **Tier 3 (Sonnet/Haiku)**: Specialists who execute within their domain
  - Designers, programmers, artists, writers, testers, engineers

### 2. Pick the Right Agent for the Job

Ask yourself: "What department would handle this in a real studio?"

| I need to... | Use this agent |
|-------------|---------------|
| Design a new mechanic | `game-designer` |
| Write combat code | `gameplay-programmer` |
| Create a shader | `technical-artist` |
| Write dialogue | `narrative-director` |
| Plan the next sprint | `producer` |
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
| Brainstorm a new game idea | Use `/brainstorm` skill |

### 3. Use Slash Commands for Common Tasks

| Command | What it does |
|---------|-------------|
| `/ags-start` | First-time onboarding вЂ” asks where you are, guides you to the right workflow |
| `/help` | Context-aware "what do I do next?" вЂ” reads your current phase and artifacts |
| `/project-stage-detect` | Analyze project state, detect stage, identify gaps |
| `/setup-engine` | Configure engine + version, populate reference docs |
| `/adopt` | Brownfield audit and migration plan for existing projects |
| `/brainstorm` | Guided game concept ideation from scratch |
| `/map-systems` | Decompose concept into systems, map dependencies, guide per-system GDDs |
| `/design-system` | Guided, section-by-section GDD authoring for a single game system |
| `/quick-design` | Lightweight spec for small changes вЂ” tuning, tweaks, minor additions |
| `/review-all-gdds` | Cross-GDD consistency and game design theory review |
| `/propagate-design-change` | Find ADRs and stories affected by a GDD change |
| `/ux-design` | Author UX specs (screen/flow, HUD, interaction patterns) |
| `/ux-review` | Validate UX specs for accessibility and GDD alignment |
| `/create-architecture` | Master architecture document for the game |
| `/architecture-decision` | Creates an ADR |
| `/architecture-review` | Validate all ADRs, dependency ordering, GDD traceability |
| `/create-control-manifest` | Flat programmer rules sheet from Accepted ADRs |
| `/create-epics` | Translate GDDs + ADRs into epics (one per architectural module) |
| `/create-stories` | Break a single epic into implementable story files |
| `/dev-story` | Read a story and implement it вЂ” routes to the correct programmer agent |
| `/sprint-plan` | Creates or updates sprint plans |
| `/sprint-status` | Quick 30-line sprint snapshot |
| `/story-readiness` | Validate a story is implementation-ready before pickup |
| `/story-done` | End-of-story completion review вЂ” verifies acceptance criteria |
| `/estimate` | Produces structured effort estimates |
| `/design-review` | Reviews a design document |
| `/code-review` | Reviews code for quality and architecture |
| `/balance-check` | Analyzes game balance data |
| `/asset-audit` | Audits assets for compliance |
| `/content-audit` | GDD-specified content vs. implemented вЂ” find gaps |
| `/scope-check` | Detect scope creep against plan |
| `/perf-profile` | Performance profiling and bottleneck ID |
| `/tech-debt` | Scan, track, and prioritize tech debt |
| `/gate-check` | Validate phase readiness (PASS/CONCERNS/FAIL) |
| `/consistency-check` | Scan all GDDs for cross-document inconsistencies (conflicting stats, names, rules) |
| `/reverse-document` | Generate design/architecture docs from existing code |
| `/milestone-review` | Reviews milestone progress |
| `/retrospective` | Runs sprint/milestone retrospective |
| `/bug-report` | Structured bug report creation |
| `/playtest-report` | Creates or analyzes playtest feedback |
| `/onboard` | Generates onboarding docs for a role |
| `/release-checklist` | Validates pre-release checklist |
| `/launch-checklist` | Complete launch readiness validation |
| `/changelog` | Generates changelog from git history |
| `/patch-notes` | Generate player-facing patch notes |
| `/hotfix` | Emergency fix with audit trail |
| `/localize` | Localization scan, extract, validate |
| `/team-combat` | Orchestrate full combat team pipeline |
| `/team-narrative` | Orchestrate full narrative team pipeline |
| `/team-ui` | Orchestrate full UI team pipeline |
| `/team-release` | Orchestrate full release team pipeline |
| `/team-polish` | Orchestrate full polish team pipeline |
| `/team-audio` | Orchestrate full audio team pipeline |
| `/team-level` | Orchestrate full level creation pipeline |
| `/team-live-ops` | Orchestrate live-ops team for seasons, events, and post-launch content |
| `/team-qa` | Orchestrate full QA team cycle вЂ” test plan, test cases, smoke check, sign-off |
| `/qa-plan` | Generate a QA test plan for a sprint or feature |
| `/bug-triage` | Re-prioritize open bugs, assign to sprints, surface systemic trends |
| `/smoke-check` | Run critical path smoke test gate before QA hand-off (PASS/FAIL) |
| `/soak-test` | Generate a soak test protocol for extended play sessions |
| `/regression-suite` | Map coverage to GDD critical paths, flag gaps, maintain regression suite |
| `/test-setup` | Scaffold test framework + CI pipeline for the project's engine (run once) |
| `/test-helpers` | Generate engine-specific test helper libraries and factory functions |
| `/test-flakiness` | Detect flaky tests from CI history, flag for quarantine or fix |
| `/test-evidence-review` | Quality review of test files and manual evidence вЂ” ADEQUATE/INCOMPLETE/MISSING |
| `/skill-test` | Validate skill files for compliance and correctness (static / spec / audit) |

### 4. Use Templates for New Documents

Templates are in `.ags/templates/`:

- `game-design-document.md` -- for new mechanics and systems
- `architecture-decision-record.md` -- for technical decisions
- `architecture-traceability.md` -- maps GDD requirements to ADRs to story IDs
- `risk-register-entry.md` -- for new risks
- `narrative-character-sheet.md` -- for new characters
- `test-plan.md` -- for feature test plans
- `sprint-plan.md` -- for sprint planning
- `milestone-definition.md` -- for new milestones
- `level-design-document.md` -- for new levels
- `game-pillars.md` -- for core design pillars
- `art-bible.md` -- for visual style reference
- `technical-design-document.md` -- for per-system technical designs
- `post-mortem.md` -- for project/milestone retrospectives
- `sound-bible.md` -- for audio style reference
- `release-checklist-template.md` -- for platform release checklists
- `changelog-template.md` -- for player-facing patch notes
- `release-notes.md` -- for player-facing release notes
- `incident-response.md` -- for live incident response playbooks
- `game-concept.md` -- for initial game concepts (MDA, SDT, Flow, Bartle)
- `pitch-document.md` -- for pitching the game to stakeholders
- `economy-model.md` -- for virtual economy design (sink/faucet model)
- `faction-design.md` -- for faction identity, lore, and gameplay role
- `systems-index.md` -- for systems decomposition and dependency mapping
- `project-stage-report.md` -- for project stage detection output
- `design-doc-from-implementation.md` -- for reverse-documenting existing code into GDDs
- `architecture-doc-from-code.md` -- for reverse-documenting code into architecture docs
- `ux-spec.md` -- for per-screen UX specifications (layout zones, states, events)
- `hud-design.md` -- for whole-game HUD philosophy, zones, and element specs
- `accessibility-requirements.md` -- for project-wide accessibility tier and feature matrix
- `interaction-pattern-library.md` -- for standard UI controls and game-specific patterns
- `player-journey.md` -- for 6-phase emotional arc and retention hooks by time scale
- `difficulty-curve.md` -- for difficulty axes, onboarding ramp, and cross-system interactions
- `test-evidence.md` -- template for recording manual test evidence (screenshots, walkthrough notes)

Also in `.ags/templates/collaborative-protocols/` (used by agents, not typically edited directly):

- `design-agent-protocol.md` -- question-options-draft-approval cycle for design agents
- `implementation-agent-protocol.md` -- story pickup through /story-done cycle for programming agents
- `leadership-agent-protocol.md` -- cross-department delegation and escalation for director-tier agents

### 5. Follow the Coordination Rules

1. Work flows down the hierarchy: Directors -> Leads -> Specialists
2. Conflicts escalate up the hierarchy
3. Cross-department work is coordinated by the `producer`
4. Agents do not modify files outside their domain without delegation
5. All decisions are documented

## First Steps for a New Project

**Don't know where to begin?** Run `/ags-start`. It asks where you are and routes
you to the right workflow. No assumptions about your game, engine, or experience level.

If you already know what you need, jump directly to the relevant path:

### Path A: "I have no idea what to build"

1. **Run `/ags-start`** (or `/brainstorm open`) вЂ” guided creative exploration:
   what excites you, what you've played, your constraints
   - Generates 3 concepts, helps you pick one, defines core loop and pillars
   - Produces a game concept document and recommends an engine
2. **Set up the engine** вЂ” Run `/setup-engine` (uses the brainstorm recommendation)
   - Configures CLAUDE.md, detects knowledge gaps, populates reference docs
   - Creates `.ags/rules/technical-preferences.md` with naming conventions,
     performance budgets, and engine-specific defaults
   - If the engine version is newer than the LLM's training data, it fetches
     current docs from the web so agents suggest correct APIs
3. **Validate the concept** вЂ” Run `/design-review design/gdd/concept.md`
4. **Decompose into systems** вЂ” Run `/map-systems` to map all systems and dependencies
5. **Design each system** вЂ” Run `/design-system [system-name]` (or `/map-systems next`)
   to write GDDs in dependency order
6. **Plan the first sprint** вЂ” Run `/sprint-plan new`
7. **Playtest** вЂ” Run `/playtest-report` once the vertical slice is playable
8. Start building

### Path B: "I know what I want to build"

If you already have a game concept and engine choice:

1. **Set up the engine** вЂ” Run `/setup-engine [engine] [version]`
   (e.g., `/setup-engine godot 4.6`) вЂ” also creates technical preferences
2. **Write the Game Pillars** вЂ” delegate to `creative-director`
3. **Decompose into systems** вЂ” Run `/map-systems` to enumerate systems and dependencies
4. **Design each system** вЂ” Run `/design-system [system-name]` for GDDs in dependency order
5. **Create the initial ADR** вЂ” Run `/architecture-decision`
6. **Create the first milestone** in `.ags/project/milestones/`
7. **Plan the first sprint** вЂ” Run `/sprint-plan new`
8. Start building

### Path C: "I know the game but not the engine"

If you have a concept but don't know which engine fits:

1. **Run `/setup-engine`** with no arguments вЂ” it will ask about your game's
   needs (2D/3D, platforms, team size, language preferences) and recommend
   an engine based on your answers
2. Follow Path B from step 2 onward

### Path D: "I have an existing project"

If you have design docs or code already:

1. **Run `/ags-start`** (or `/project-stage-detect`) вЂ” analyzes what exists,
   identifies gaps, and recommends next steps
2. **Run `/adopt`** if you have existing GDDs, ADRs, or stories вЂ” audits
   internal format compliance and builds a numbered migration plan to fill gaps
   without overwriting your existing work
3. **Configure engine if needed** вЂ” Run `/setup-engine` if not yet configured
4. **Validate phase readiness** вЂ” Run `/gate-check` to see where you stand
5. **Plan the next sprint** вЂ” Run `/sprint-plan new`

## File Structure Reference

See `.ags/rules/directory-structure.md` for the full canonical layout. Quick map:

```
CLAUDE.md                          -- Master config (read this first)
.claude/
  settings.json                    -- Claude Code hooks and project settings
  agents/                          -- 21 agent definitions
  skills/                          -- 70 slash-command skill definitions
  hooks/                           -- Hook scripts (.sh) wired by settings.json
  hooks-reference/                 -- Hook reference docs
.ags/
  rules/                           -- Project rules (this file lives here)
    technical-preferences.md       -- Project-specific standards (populated by /setup-engine)
    coding.md                      -- Coding standards
    coordination.md                -- Agent coordination rules
    context-management.md          -- Context budgets and compaction instructions
    directory-structure.md         -- Canonical project directory layout
    workflow-catalog.yaml          -- 7-phase pipeline definition (read by /help)
    setup-requirements.md          -- System prerequisites
    settings-local-template.md     -- Personal settings.local.json guide
    director-gates.md              -- Director gate prompt catalog
    review-workflow.md             -- Sign-off matrix
  templates/                       -- Document templates
  docs/                            -- Engine reference snapshots, examples
  project/                         -- Working state (sessions, sprints, epics, etc.)
design/                            -- Game design documents (gdd, architecture, art, ux, narrative)
tests/                             -- Test code
<engine project root>              -- Engine source root, e.g. Assets/ for Unity
```
