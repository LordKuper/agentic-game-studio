# AGS Implementation Plan

A C# CLI application that orchestrates AI agents across the full game development cycle — from concept to post-release support. The CEO (human user) sets direction and approves decisions; agents handle everything else.

## Core Design

- **CEO approves everything** — agents propose options with rationale, CEO decides.
- **File-based persistence** — no databases, all state on disk in `.ags/` and `gdd/`.
- **One active session** — pause/resume with disk persistence.
- **AI provider abstraction** — `IAIProvider` + adapters (Claude Code, Codex, ...), failover on rate-limit.
- **Global install** — `%USERPROFILE%\ags\`, binary + standard resources (agents, rules, skills, templates).
- **Resource overlay** — project-local `.ags/` overrides standard resources (full file replacement, no merge).
- **Unity priority** — Unreal and Godot formally supported.

## Architecture

```
CLI -> Resource Loader -> Subsystems (Setup, Settings, MainMenu, ...)
                       -> Stage Detection Engine (disk artifacts -> lifecycle stage)
                       -> Next Steps Recommender (stage + state -> actions)
                       -> Session Manager (lifecycle, persistence, git branches)
                       -> Agent Orchestrator (prompt assembly, sequencing, inter-agent comms)
                       -> AI Provider Abstraction (IAIProvider -> adapters -> CLI subprocesses)
                       -> Build System Integration (build, test runner)
                       -> Git Manager (branches, commits, PRs)
```

**Data flow:** CEO input -> Session Manager -> Agent Orchestrator -> AI Provider -> agent output (files) -> state update -> Next Steps -> CEO approval.

## Project Lifecycle

6 stages, derived from disk artifacts (not a stored label):

1. **Concept** — no `gdd/game-concept.md` or not approved
2. **Pre-production** — concept approved, engine/prototype/plan in progress
3. **Production** — all pre-production gates passed, content creation underway
4. **Testing/QA** — core features ready, QA sessions active
5. **Release Preparation** — QA passed, preparing release
6. **Post-release/Live Ops** — release shipped

Transitions proposed by the application, confirmed by CEO.

## Session Lifecycle

Scoping -> Planning -> Execution -> Completion (Pause/Resume available at any step).

Each session: directory `.ags/sessions/<id>/`, files state/scope/plan/tasks, git branch `session/<id>`, PR to main on completion.

## Agent Orchestrator

Prompt = agent definition + rules + task context + project context + CEO instructions. Inter-agent communication via files (handoff files). CEO approves every key decision.

## Non-Goals

Art/audio asset generation, game launching, databases, Web UI/GUI, multi-user collaboration, cloud deployment of AGS.

---

## Checklist Management Rules

- Completed tasks are marked `[x]`.
- When all items within a phase section are done (e.g. "0.1 Project Restructure"), the section collapses to a single summary item marked `[x]`.
- When all sections of a phase are done, the entire phase collapses to a single summary item marked `[x]`.

---

## Implementation Roadmap

### Phase 0: Installation and Distribution
- [x] Project restructure, resource loader, install script, GitHub release packaging

### Phase 1: Foundation (Infrastructure)

- [x] **1.1 AI Provider Abstraction** — IAIProvider, adapters (Claude Code, Codex), registry, provider selection

**1.1.1 Rate Limit Handling and Provider Failover**
- [x] `IsRateLimited` / `RateLimitResetsAt` in AIProviderResult
- [x] Rate-limit detection in ClaudeCodeAdapter and CodexAdapter
- [x] Provider cooldown tracking in AIProviderRegistry (mark, expiry, persistence to config.json)
- [x] `IsAvailable` returns false during cooldown; `RateLimitDefaultCooldown` setting (default 30min)
- [x] Task failover: on rate-limit, select next available provider from agent's `models` list, restart task
- [ ] Wait-and-retry when all providers rate-limited (infrastructure ready: `GetEarliestCooldownExpiry`; wiring deferred to Phase 3)
- [ ] CEO notification on provider switch / all-providers-exhausted (deferred to Phase 3 / Phase 6 UX)
- [x] Tests: rate-limit detection, cooldown expiry, failover selection, all-exhausted wait

- [x] **1.2 Git Manager** — branch creation, task commits, conflict detection, PR generation, merge/cleanup
- [x] **1.3 Prompt Assembly Engine** — agent .md loader, rule loader, context assembler, prompt template

### Phase 2: Session System

- [x] **2.1 Session Manager Core** — creation, state transitions, pause/resume, directory management, git branch on start

**2.2 Scoping Protocol**
- [ ] Agent selection for scoping
- [ ] Q&A loop (agent asks -> CEO answers -> repeat)
- [ ] Scope document generation (session-scope.md)
- [ ] CEO approval flow for scope
- [ ] Tests: mock agent interaction, scope document validation

**2.3 Planning Protocol**
- [ ] Agent selection for planning
- [ ] Execution plan generation from approved scope
- [ ] Task decomposition into atomic tasks
- [ ] CEO approval flow for plan
- [ ] Tests: plan structure validation, task dependency ordering

**2.4 Task Execution Engine**
- [ ] Sequential task execution from plan
- [ ] Task brief creation and management
- [ ] Task start/complete/skip protocol
- [ ] Task switch protocol within session
- [ ] Tests: execution order enforcement, state persistence

### Phase 3: Agent Orchestrator

**3.1 Single-Agent Invocation**
- [ ] Assemble prompt for single agent (definition + rules + task context)
- [ ] Invoke via AI provider, capture and parse result
- [ ] Detect modified files via git diff, update task brief
- [ ] Tests: end-to-end with mock provider

**3.2 Inter-Agent Communication**
- [ ] Detect dependency, invoke agent B, write handoff file, feed back to agent A
- [ ] Tests: multi-agent handoff scenarios

**3.3 CEO Interaction Loop**
- [ ] Present proposals with options, capture choice, feed back to agent
- [ ] Handle corrections and re-invocations
- [ ] Tests: interaction flow with simulated CEO input

### Phase 4: Stage Detection and Recommendations

**4.1 Stage Detection Engine**
- [ ] Artifact scanner + stage determination logic
- [ ] Stage display in main menu
- [ ] Tests: one test per stage with fixture directories

**4.2 Next Steps Recommender**
- [ ] Recommendation rules per stage + agent involvement lookup
- [ ] Complexity estimation + presentation in main menu
- [ ] Tests: recommendation output per stage scenario

### Phase 5: Build System Integration

**5.1 Build Manager**
- [ ] `IBuildManager` interface + Unity adapter + Unreal/Godot stubs
- [ ] Tests: mock engine CLI responses

**5.2 Test Runner Integration**
- [ ] Invoke engine test runner, parse results, record in task briefs
- [ ] Fail task if tests fail
- [ ] Tests: result parsing, task state on failure

**5.3 Build Verification in Session Flow**
- [ ] Post-code-gen build check, pre-completion build gate, orchestrator integration
- [ ] Tests: session flow with build steps

### Phase 6: Main Menu and UX Polish

**6.1 Enhanced Main Menu**
- [ ] Project info, recommended steps, resumable sessions, contextual options
- [ ] Tests: menu rendering with various states

**6.2 Session Progress Display**
- [ ] Working indicator, task summaries, file reports, session completion with PR info
- [ ] Tests: output formatting

**6.3 Error Handling and Recovery**
- [ ] AI provider failures, session/git state recovery, clear error messages
- [ ] Tests: failure scenarios

### Phase 7: ags-start Skill

**7.1 Onboarding Flow**
- [ ] Repo inspection, entry state routing (A/B/C/D), concept/engine doc generation
- [ ] First session recommendations
- [ ] Tests: each entry path, document generation

### Phase 8: Hardening

**8.1 Edge Cases**
- [ ] Empty repo, corrupted state, multiple engine fingerprints, empty session, timeout handling

**8.2 Documentation**
- [ ] README, CLI help text, configuration reference

**8.3 Performance**
- [ ] Optimize file scanning and git operations for large projects/repos
