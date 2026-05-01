# Agent + Human Coordination Rules

## Roles

**Human** = final decision maker. Approves design, scope, file writes, releases.

**Agents** in three tiers:
- **Tier 1 — Leadership**: creative-director, technical-director, producer. Vision, architecture, schedule.
- **Tier 2 — Department Leads**: game-designer, lead-programmer, art-director, audio-director, narrative-director, qa-lead, release-manager. Domain ownership, review, delegation to specialists.
- **Tier 3 — Specialists**: implementers within single domain (gameplay code, systems design, VFX, UX, etc.).
- **Engine specialists**: `unity-specialist` (engine authority, reports to technical-director), `unity-dots-specialist` (ECS/Jobs/Burst, reports to unity-specialist). Advise programmers and technical-artist on engine patterns. No domain file ownership.

## Core Coordination Rules

1. **Vertical delegation**: leadership → department leads → specialists. Never skip a tier for complex decisions.
2. **Horizontal consultation**: same-tier agents may consult, cannot make binding decisions outside domain.
3. **No unilateral cross-domain changes**: never modify files outside designated directories without explicit delegation.
4. **Conflict resolution**: escalate to shared parent. No shared parent → creative-director (design) or technical-director (technical).
5. **Change propagation**: producer coordinates when change affects multiple domains.
6. **Document every decision**: verbal-only agreements lead to contradictions. Write it down.
7. **Tasks ≤ 1-3 days**: larger tasks must be broken down before assignment.
8. **No assumption-based implementation**: ambiguous spec → ask specifier. Wrong guess costs more than question.

## Delegation Matrix

| From | Can Delegate To |
|------|----------------|
| creative-director | game-designer, art-director, audio-director, narrative-director |
| technical-director | lead-programmer, performance-analyst, technical-artist (technical decisions), unity-specialist |
| producer | any agent (task assignment within their domain) |
| game-designer | systems-designer |
| lead-programmer | gameplay/engine/ai/tools/ui programmers |
| art-director | technical-artist, ux-designer |
| release-manager | tools-programmer (builds), qa-lead (release testing) |
| unity-specialist | unity-dots-specialist (DOTS/ECS); advises lead-programmer, programming specialists, technical-artist on Unity patterns |
| unity-dots-specialist | (advises all programmers on DOTS/ECS, Burst optimization) |

Department leads without sub-specialists absorb that scope directly. Examples: `audio-director` absorbs sound-designer/composer scope; `narrative-director` absorbs writer, world-builder, localization; `qa-lead` absorbs qa-tester; `tools-programmer` absorbs devops/build-engineer; `ux-designer` absorbs accessibility-specialist; `producer` absorbs analytics and community-manager; `unity-specialist` absorbs Unity shader, addressables, UI specialist scope.

## Escalation Paths

| Situation | Escalate To |
|-----------|------------|
| Two designers disagree on a mechanic | game-designer |
| Game design vs narrative conflict | creative-director |
| Game design vs technical feasibility | producer → creative-director + technical-director |
| Code architecture disagreement | technical-director |
| Cross-system code conflict | lead-programmer → technical-director |
| Schedule conflict between departments | producer |
| Scope exceeds capacity | producer → creative-director (for cuts) |
| Quality gate disagreement | qa-lead → technical-director |
| Performance budget violation | performance-analyst flags → technical-director decides |

## Subagents vs Agent Teams

**Subagents** (default): spawned via Task within one Claude Code session. Share permission context. Run sequentially or parallel within session.

**Agent teams** (experimental, opt-in): independent sessions, own context windows. Use when:
- Workstreams touch different files
- Each takes >30 min
- 3+ specialists work parallel epics

Skip agent teams when one session's output feeds another (use subagents) or task fits single context.

## Parallel Task Protocol

When orchestrating multiple independent agents:
1. Issue all independent Task calls before waiting
2. Collect all results before dependent phases
3. Surface BLOCKED agents immediately, never silently skip
4. Produce partial report if some block

## Workflow Patterns

### Epic Cycle (vertical slice)
producer (epic-plan from systems-index, `/ags-create-epics`) → game-designer (GDD sections for in-scope systems) + lead-programmer (ADR for epic) → specialist (impl + stubs marked `// TODO(epic-[id])`) → qa-lead (playtest, bug triage) → directors (lean gate `/ags-gate-check epic-done`) → producer (close in `epics/index.md`, append to `decisions-log.md`, retro via `/ags-epic-retro`).

Loop: next epic until feature-complete → polish phase.

### Bug Fix
qa-lead (report + triage) → producer (assign) → lead-programmer (root cause) → specialist (fix) → lead-programmer (review) → qa-lead (verify + regression).

### Balance Adjustment
producer (identify) → game-designer (evaluate vs intent) → systems-designer (model) → game-designer (approve) → data update → qa-lead (regression) → producer (monitor).

### Milestone Checkpoint
producer (review) → directors (creative/technical/quality reviews) → producer (facilitate go/no-go) → directors (scope decisions) → producer (document).

### Release Pipeline
producer (declare RC) → release-manager (cut branch + checklist) → qa-lead (regression sign-off) → narrative-director (loc check) → performance-analyst (benchmarks) → tools-programmer (build) → release-manager (changelog + tag) → technical-director (sign-off) → release-manager (deploy + monitor).

## Cross-Domain Notifications

**Design doc** changes → game-designer notifies lead-programmer, qa-lead, producer, affected specialists.

**Architecture decision (ADR)** changes → technical-director notifies lead-programmer, affected specialists, qa-lead, producer.

**Art bible / asset standards** changes → art-director notifies technical-artist, content creators, tools-programmer (if pipeline affected).

## Anti-Patterns

- **Bypassing hierarchy**: specialist makes lead-level decision without consultation
- **Cross-domain implementation**: editing files outside designated area without delegation
- **Shadow decisions**: agreements without written record
- **Monolithic tasks**: assigning >3-day work without breakdown
- **Assumption-based implementation**: guessing instead of asking spec owner
- **Stub debt accumulation**: closing an epic with open stubs without migration entry — gate blocks close
