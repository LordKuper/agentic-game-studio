# AGS Implementation Plan

A C# CLI application that orchestrates AI agents across the full game development cycle — from concept to post-release support. The CEO (human user) sets direction and approves decisions; agents handle everything else.

## Working Agreement

Before implementing any checklist item, provide a short description of the planned approach (affected files, key decisions, expected behaviour). Wait for confirmation before proceeding.

---

## Core Design

- **CEO approves everything** — agents propose options with rationale, CEO decides.
- **File-based persistence** — no databases, all state on disk in `.ags/` and `gdd/`.
- **One active session** — pause/resume with disk persistence.
- **AI provider abstraction** — `IAIProvider` + adapters (Claude Code, Codex, ...), failover on rate-limit.
- **Global install** — `%USERPROFILE%\ags\`, binary + standard resources (agents, rules, skills, templates).
- **Resource overlay** — project-local `.ags/` overrides standard resources (full file replacement, no merge).
- **Unity priority** — Unreal and Godot formally supported.
- **No game development logic in code** — the app does not contain game development stages, transitions, or workflow rules. All such logic lives in `rules/` (md files) and `skills/` (skill directories per the Codex skill spec). The app only discovers AI providers and invokes skills.

## Architecture

```
CLI -> Resource Loader -> Settings (AI provider priority list)
    -> AI Provider Registry (discover available providers, track cooldowns, failover)
    -> Skill Runner (load skill from skills/, assemble prompt, invoke via default AI)
         -> ags-start skill (entry point — reads rules/, drives all further workflow)
    -> Session Manager (lifecycle, persistence, git branches)
    -> Prompt Assembly Engine (skill/agent content + rules + context)
    -> Git Manager (branches, commits, PRs)
    -> Build System Integration (build, test runner)
```

**Startup flow:** check available AI providers (from enabled providers in settings) → invoke `ags-start` skill via the highest-priority available provider → `ags-start` determines all further workflow, reads `rules/`, and coordinates sessions and agents.

**Data flow:** CEO input → Session Manager → Skill Runner / Agent Orchestrator → AI Provider → output (files) → state update → CEO approval.

## Skill Format

Skills live in `skills/<skill-name>/SKILL.md` and follow the Codex skill spec:

```
skills/
  ags-start/
    SKILL.md          # required: frontmatter (name, description) + imperative steps
    references/       # optional: rule references, docs
    agents/
      openai.yaml     # optional: UI config, tool dependencies
```

`SKILL.md` opens with YAML frontmatter (`name`, `description`) followed by imperative steps describing inputs, outputs, and tool invocations.

## Session Lifecycle

Scoping -> Planning -> Execution -> Completion (Pause/Resume available at any step).

Each session: directory `.ags/sessions/<id>/`, files state/scope/plan/tasks, git branch `session/<id>`, PR to main on completion.

## Agent Orchestrator

Prompt = agent definition + rules + task context + project context + CEO instructions. Inter-agent communication via files (handoff files). CEO approves every key decision.

## AI Agent Usage Rules

- **No hardcoded agent lists** — agent names are never enumerated in code. The set of agents for any task is derived at runtime from `agent-coordination.md`.
- **Default AI** (`DefaultModels` setting, priority-ordered model list) — used for skill invocation and general file-level tasks.
- **Named agents** (`InvokeAgent`) — used for domain-specific tasks where the agent's definition, rules, and practical guidance are required. Agent names come from `agent-coordination.md` or from task context, never from constants in code.
- **agent-coordination.md** is the authoritative source for agent hierarchy and responsibilities. If it is missing from the project overlay, the default AI creates it before any agent resolution is attempted.

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
- [ ] Wait-and-retry when all providers rate-limited (infrastructure ready: `GetEarliestCooldownExpiry`; wiring deferred to Phase 4)
- [ ] CEO notification on provider switch / all-providers-exhausted (deferred to Phase 4 / Phase 6 UX)
- [x] Tests: rate-limit detection, cooldown expiry, failover selection, all-exhausted wait

- [x] **1.2 Git Manager** — branch creation, task commits, conflict detection, PR generation, merge/cleanup
- [x] **1.3 Prompt Assembly Engine** — agent .md loader, rule loader, context assembler, prompt template

### Phase 2: Skill Runner

**2.1 Skill Loader**
- [ ] Skill discovery from `skills/` directory (global install + project overlay)
- [ ] `SKILL.md` parser: frontmatter extraction (`name`, `description`) + body
- [ ] Skill prompt assembly: SKILL.md body + optional `references/` content + session context
- [ ] Tests: loader with fixture skill directories, overlay override behaviour

**2.2 Skill Invocation**
- [ ] `ISkillRunner` interface + implementation: load skill, assemble prompt, invoke via default AI provider
- [ ] Result handling: parse AI output, apply file writes described in skill output
- [ ] Tests: end-to-end with mock AI provider

**2.3 ags-start Skill**
- [ ] `skills/ags-start/SKILL.md`: repo inspection, entry-state routing (new repo / existing project / in-progress session / post-release), first-run onboarding
- [ ] References to `rules/` files for stage definitions and workflow logic
- [ ] Invoked automatically on app startup after provider availability check
- [ ] Tests: each entry path with fixture repo states

### Phase 3: Session System

- [x] **3.1 Session Manager Core** — creation, state transitions, pause/resume, directory management, git branch on start
- [x] **3.2 Scoping Protocol** — agent selection via default AI from agent-coordination.md, Q&A loop, session-scope.md generation, CEO approval flow

**3.3 Planning Protocol**
- [ ] Agent selection for planning
- [ ] Execution plan generation from approved scope
- [ ] Task decomposition into atomic tasks
- [ ] CEO approval flow for plan
- [ ] Tests: plan structure validation, task dependency ordering

**3.4 Task Execution Engine**
- [ ] Sequential task execution from plan
- [ ] Task brief creation and management
- [ ] Task start/complete/skip protocol
- [ ] Task switch protocol within session
- [ ] Tests: execution order enforcement, state persistence

### Phase 4: Agent Orchestrator

**4.1 Single-Agent Invocation**
- [ ] Assemble prompt for single agent (definition + rules + task context)
- [ ] Invoke via AI provider, capture and parse result
- [ ] Detect modified files via git diff, update task brief
- [ ] Wait-and-retry when all providers rate-limited (wire up `GetEarliestCooldownExpiry`)
- [ ] CEO notification on provider switch / all-providers-exhausted
- [ ] Tests: end-to-end with mock provider

**4.2 Inter-Agent Communication**
- [ ] Detect dependency, invoke agent B, write handoff file, feed back to agent A
- [ ] Tests: multi-agent handoff scenarios

**4.3 CEO Interaction Loop**
- [ ] Present proposals with options, capture choice, feed back to agent
- [ ] Handle corrections and re-invocations
- [ ] Tests: interaction flow with simulated CEO input

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
- [ ] Project info, resumable sessions, contextual options
- [ ] Stage and recommended next steps provided by ags-start skill output (not derived in code)
- [ ] Tests: menu rendering with various states

**6.2 Session Progress Display**
- [ ] Working indicator, task summaries, file reports, session completion with PR info
- [ ] Tests: output formatting

**6.3 Error Handling and Recovery**
- [ ] AI provider failures, session/git state recovery, clear error messages
- [ ] Tests: failure scenarios

### Phase 7: Hardening

**7.1 Edge Cases**
- [ ] Empty repo, corrupted state, multiple engine fingerprints, empty session, timeout handling

**7.2 Documentation**
- [ ] README, CLI help text, configuration reference

**7.3 Performance**
- [ ] Optimize file scanning and git operations for large projects/repos
