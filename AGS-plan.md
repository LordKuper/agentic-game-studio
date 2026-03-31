# AGS Implementation Plan

This document defines the implementation plan for the Agentic Game Studio (AGS)
CLI application: a monolith C# tool that orchestrates AI agents to design,
develop, test, and ship a video game project from concept to post-release
support.

---

## 1. Project Overview

### 1.1 Goal

Build a CLI application that simulates a mid-size game studio where all roles
except CEO are performed by AI agents. The CEO (human user) sets direction,
resolves conflicts, approves decisions, and conducts real playtests. AI agents
handle everything else: design, programming, QA, release management, live ops.

### 1.2 Target Audience

Solo indie developers or small teams (1-5 people) who want to leverage AI to
handle the bulk of game production work across all disciplines.

### 1.3 Scope of Games

Any scale from a mobile Tetris clone to a sandbox like Minecraft. The
application adapts its session complexity and agent involvement to the scope of
the game being built.

### 1.4 Key Principles

- CEO approves every decision; agents propose, CEO disposes.
- Agents prepare options with rationale; they do not make unilateral choices.
- One active session at a time; paused sessions persist on disk.
- File-based persistence only (no database).
- AI provider abstraction from day one (interfaces + adapters).
- Unity is the priority engine; Unreal and Godot are formally supported.
- Application is installed globally; project-local `.ags/` overrides and extends
  standard resources.

---

## 2. Installation and Distribution

### 2.1 Installation Model

AGS is installed into a global directory, not into each game project. The
default installation path is:

```
%USERPROFILE%\ags\
```

The installer adds this path to the user's `PATH` environment variable so that
the `ags` command is available from any directory.

### 2.2 Installed Directory Structure

```
%USERPROFILE%\ags\
  ags.exe                  <- compiled CLI binary
  agents\                  <- standard agent definitions (.md)
  rules\                   <- standard operating rules (.md)
  skills\                  <- standard skills
  templates\               <- standard templates (agent-template.md, etc.)
  config\                  <- global AGS defaults (not project-specific)
```

All standard agents, skills, templates, and rules ship with the binary and are
placed alongside it during installation.

### 2.3 Resource Overlay (Standard vs. Project-Local)

When AGS runs from a game project directory, it merges two layers of resources:

1. **Standard layer** — files shipped with AGS in `%USERPROFILE%\ags\`
2. **Project layer** — files in `.ags/` inside the current project directory

Resolution order (project wins over standard):

| Resource type | Standard path | Project override path |
|---|---|---|
| Agent definitions | `<ags-install>/agents/<name>.md` | `.ags/agents/<name>.md` |
| Rules | `<ags-install>/rules/<name>.md` | `.ags/rules/<name>.md` |
| Skills | `<ags-install>/skills/<name>/` | `.ags/skills/<name>/` |
| Templates | `<ags-install>/templates/<name>.md` | `.ags/templates/<name>.md` |

Rules:
- If a file exists in both layers, the project layer takes precedence entirely
  (full file replacement, not merge).
- Files that exist only in the project layer are additions (new agents, new
  rules, etc.).
- Files that exist only in the standard layer are used as-is.
- The overlay is resolved at runtime when the resource is loaded; no copying is
  performed.

### 2.4 Resource Loader

```csharp
/// Resolves resources by checking the project layer first, then the standard
/// layer.
class ResourceLoader
{
    /// Gets the absolute path to a resource, checking project-local .ags/ first,
    /// then the global AGS install directory.
    string ResolveResourcePath(string resourceType, string resourceName);

    /// Gets all available resource names for a given type, merging both layers.
    IReadOnlyList<string> ListResources(string resourceType);

    /// Reads the content of a resolved resource.
    string ReadResource(string resourceType, string resourceName);
}
```

### 2.5 Windows Installation Script

A PowerShell script `install.ps1` is hosted in the GitHub repository and handles
end-to-end installation:

1. Detect or prompt for install directory (default: `%USERPROFILE%\ags`)
2. Download the latest release archive from GitHub Releases
3. Extract the archive to the install directory
4. Add the install directory to the user's `PATH` environment variable (if not
   already present)
5. Verify installation by running `ags -version`
6. Print success message with next steps

The script must be idempotent: re-running it upgrades an existing installation
without duplicating PATH entries or losing user data.

Usage:

```powershell
irm https://raw.githubusercontent.com/<owner>/agentic-game-studio/main/scripts/install.ps1 | iex
```

### 2.6 GitHub Release Packaging

Each release is published as a GitHub Release with:
- A self-contained .zip archive containing `ags.exe` and all standard resources
- The application is published as a single-file .NET deployment or
  framework-dependent deployment (TBD based on target .NET version)
- Release notes generated from merged session PRs since the last release

---

## 3. Architecture

### 3.1 High-Level Architecture

```
CLI Entry Point (Program.cs)
  |
  +-- Resource Loader
  |     resolves standard vs. project-local agents, rules, skills, templates
  |
  +-- Subsystems (Setup, Settings, MainMenu, ...)
  |
  +-- Stage Detection Engine
  |     reads disk artifacts -> determines current project stage
  |
  +-- Next Steps Recommender
  |     stage + project state -> recommended actions
  |
  +-- Session Manager
  |     session lifecycle, state persistence, git branch management
  |
  +-- Agent Orchestrator
  |     prompt assembly, agent sequencing, inter-agent communication
  |
  +-- AI Provider Abstraction
  |     IAIProvider interface
  |       +-- ClaudeCodeAdapter (CLI subprocess)
  |       +-- CodexAdapter (CLI subprocess)
  |       +-- (future adapters)
  |
  +-- Build System Integration
  |     project build, test runner invocation
  |
  +-- Git Manager
        branch creation, commits, PR preparation
```

### 3.2 Module Responsibilities

| Module | Responsibility |
|---|---|
| Resource Loader | Resolves agents, rules, skills, templates from standard + project overlay |
| Stage Detection Engine | Scans disk artifacts to determine which project lifecycle stage is current |
| Next Steps Recommender | Maps current stage + project state to a prioritized list of recommended actions/sessions |
| Session Manager | Creates, pauses, resumes, completes sessions; manages session directories and state files |
| Agent Orchestrator | Assembles prompts from agent definitions + rules + task context; invokes AI provider; routes inter-agent communication through files |
| AI Provider Abstraction | Universal interface for calling any AI CLI tool as a subprocess |
| Build System Integration | Invokes engine-specific build commands and test runners |
| Git Manager | Creates session branches, commits agent work, prepares PRs for CEO review |

### 3.3 Data Flow

```
CEO input
  -> MainMenu / CLI command
    -> Session Manager (create or resume session)
      -> Agent Orchestrator (assemble prompt, pick agent)
        -> AI Provider (invoke CLI subprocess)
          -> Agent output (files written to disk)
            -> Session Manager (update state, task brief)
              -> Next Steps Recommender (propose next action)
                -> CEO approval
```

---

## 4. Project Lifecycle Stages

### 4.1 Stage Definitions

| # | Stage | Description | Entry Signal |
|---|---|---|---|
| 1 | Concept | Idea formation, conceptual vision | No `gdd/game-concept.md` |
| 2 | Pre-production | GDD, engine decision, prototyping, technical design | `gdd/game-concept.md` exists and is approved |
| 3 | Production | Asset creation, code implementation, content authoring | `gdd/engine-decision.md` exists, prototype validated, production plan approved |
| 4 | Testing / QA | Integration testing, bug fixing, polish | Core features implemented, QA sessions initiated |
| 5 | Release Preparation | Build packaging, submission, deployment readiness | QA pass completed, release checklist initiated |
| 6 | Post-release / Live Ops | Patches, live events, ongoing content, community management | First public release shipped |

### 4.2 Stage Detection

The Stage Detection Engine scans the project directory for artifact signals.
Detection is based on the presence, absence, and state of specific files and
directories. The engine does not rely on a stored stage label; it derives the
stage from disk state every time it runs.

#### Detection Rules

**Concept stage** when:
- `gdd/game-concept.md` does not exist, OR
- `gdd/game-concept.md` exists but has no approved status marker

**Pre-production stage** when:
- `gdd/game-concept.md` exists and is approved
- AND at least one of the following is NOT yet satisfied:
  - `gdd/engine-decision.md` exists and is approved
  - A prototype session has been completed
  - A production plan session has been completed

**Production stage** when:
- All pre-production gates are satisfied
- AND active sessions are creating production content (code, assets, data)
- AND no release checklist session has been initiated

**Testing / QA stage** when:
- Production sessions have delivered core features
- AND at least one QA-focused session is active or completed

**Release Preparation stage** when:
- QA sessions report a passing state
- AND a release preparation session is active or completed

**Post-release / Live Ops stage** when:
- A release has been shipped (release session completed with ship confirmation)

#### Artifact Signals Reference

| Artifact | Location | Signals |
|---|---|---|
| Game concept | `gdd/game-concept.md` | Existence, approved marker |
| Engine decision | `gdd/engine-decision.md` | Existence, approved marker |
| Session index | `.ags/sessions/index.md` | Session types, statuses |
| Session state files | `.ags/sessions/*/state.md` | Session phase, completion |
| Game project files | `assets/scripts/`, engine project files | Existence, quantity |
| Build artifacts | Engine-specific output directories | Existence of successful build |
| Test results | `.ags/sessions/*/tasks/*` | Test verification results in task briefs |
| Release record | `.ags/releases/` or release session | Ship confirmation |

### 4.3 Stage Transitions

Transitions are proposed by the application and confirmed by the CEO.

```
Concept ──[concept approved]──> Pre-production
Pre-production ──[engine + prototype + plan done]──> Production
Production ──[core features done, QA initiated]──> Testing / QA
Testing / QA ──[QA passed]──> Release Preparation
Release Preparation ──[shipped]──> Post-release / Live Ops
```

There is no formal stage rollback. Concept changes during Production are handled
within Production by creating a concept-revision session, not by reverting the
stage.

---

## 5. Next Steps Recommender

### 5.1 Purpose

After stage detection, the recommender proposes 1-4 concrete next actions to the
CEO, prioritized by what will move the project forward most effectively.

### 5.2 Recommendation Logic

The recommender follows a priority chain per stage:

**Concept:**
1. Run `ags-start` skill if `gdd/game-concept.md` is missing
2. Refine concept if open questions remain in `gdd/game-concept.md`
3. Propose engine decision session if `gdd/engine-decision.md` is missing

**Pre-production:**
1. Complete engine decision if missing
2. Start prototype session if no prototype exists
3. Start production planning session after prototype validation
4. Start technical foundation session (project setup, CI, folder structure)

**Production:**
1. Resume any paused production session
2. Start next priority feature session from production backlog
3. Start QA session if enough features are implemented
4. Start integration testing session for cross-feature validation

**Testing / QA:**
1. Resume active QA session
2. Start bug-fix session for critical issues
3. Start polish session for UX/performance
4. Propose release preparation when QA passes

**Release Preparation:**
1. Start release checklist session
2. Start platform submission session
3. Start build verification session

**Post-release / Live Ops:**
1. Start patch session for reported issues
2. Start live event session
3. Start content update session
4. Start community feedback analysis session

### 5.3 Recommendation Presentation

Recommendations are shown in the main menu as numbered options. Each
recommendation includes:
- Short title
- Why it is recommended now (1 sentence)
- Which agents would be involved
- Estimated complexity (light / medium / heavy)

The CEO selects one to start or chooses to do something else.

---

## 6. AI Provider Abstraction

### 6.1 Interface

```csharp
/// Defines the contract for invoking an AI agent through any provider.
interface IAIProvider
{
    /// Gets the unique identifier for this provider.
    string ProviderId { get; }

    /// Gets whether this provider is currently installed and available.
    bool IsAvailable { get; }

    /// Invokes an AI agent with the given prompt and returns the result.
    AIProviderResult Invoke(AIProviderRequest request);
}
```

### 6.2 Request Model

```csharp
/// Represents a request to an AI provider.
class AIProviderRequest
{
    /// The assembled system prompt (agent definition + rules + context).
    string SystemPrompt { get; }

    /// The user-facing task prompt.
    string TaskPrompt { get; }

    /// The working directory for the AI subprocess.
    string WorkingDirectory { get; }

    /// Maximum time to wait for a response.
    TimeSpan Timeout { get; }

    /// Additional provider-specific arguments.
    IReadOnlyDictionary<string, string> ProviderArguments { get; }
}
```

### 6.3 Result Model

```csharp
/// Represents the result of an AI provider invocation.
class AIProviderResult
{
    /// Whether the invocation succeeded.
    bool Success { get; }

    /// The text output from the AI agent.
    string Output { get; }

    /// The exit code of the CLI subprocess.
    int ExitCode { get; }

    /// Error message if the invocation failed.
    string ErrorMessage { get; }

    /// Files modified during the invocation.
    IReadOnlyList<string> ModifiedFiles { get; }
}
```

### 6.4 Adapters

**ClaudeCodeAdapter:**
- Invokes `claude` CLI as a subprocess
- Passes system prompt via `--system-prompt` or equivalent flag
- Passes task prompt as the main argument
- Sets working directory to the game project root
- Captures stdout as output
- Detects modified files via git diff after invocation

**CodexAdapter:**
- Invokes `codex` CLI as a subprocess
- Same pattern as ClaudeCodeAdapter with Codex-specific flags

### 6.5 Provider Registry

```csharp
/// Manages available AI providers and selects the active one.
class AIProviderRegistry
{
    /// Registers a provider adapter.
    void Register(IAIProvider provider);

    /// Gets the currently active provider.
    IAIProvider GetActiveProvider();

    /// Sets the active provider by ID.
    void SetActiveProvider(string providerId);

    /// Gets all registered providers.
    IReadOnlyList<IAIProvider> GetAllProviders();
}
```

The active provider is stored in the project-local `.ags/config.json`.

### 6.6 Rate Limit Handling and Provider Failover

AI providers may reject requests when the usage quota (tokens, requests per
minute, etc.) is exhausted. The application must detect these situations and
handle them transparently.

#### Detection

`AIProviderResult` is extended with a rate-limit signal:

```csharp
/// Whether the invocation failed due to a rate limit or quota exhaustion.
bool IsRateLimited { get; }

/// The time at which the provider is expected to become available again,
/// as parsed from the provider's error response. Null if the reset time
/// could not be determined.
DateTimeOffset? RateLimitResetsAt { get; }
```

Each adapter is responsible for recognizing rate-limit errors in the CLI
subprocess output (exit code, stderr, or structured error JSON) and populating
these fields.

#### Provider Cooldown

When a rate-limit response is detected:

1. The `AIProviderRegistry` marks the provider as **temporarily unavailable**
   until `RateLimitResetsAt`.
2. If the reset time could not be parsed from the response, the provider is
   marked unavailable for a configurable default cooldown period
   (`RateLimitDefaultCooldown` in `.ags/config.json`, default: 30 minutes,
   stored as minutes).
3. `IsAvailable` returns `false` for cooled-down providers.
4. The cooldown expires automatically; no manual re-enable is required.

#### Cooldown Persistence

Cooldown state is persisted to `.ags/config.json` so that it survives
application restarts. The config stores a map of provider IDs to their cooldown
expiry timestamps:

```json
{
  "ProviderCooldowns": {
    "claude-sonnet": "2026-03-31T14:32:00+03:00",
    "chatgpt": "2026-03-31T14:45:00+03:00"
  }
}
```

On startup, the `AIProviderRegistry` reads `ProviderCooldowns` from the config
and restores the cooldown state. Expired entries (where the timestamp is in the
past) are ignored and cleaned up on the next config write. When a provider is
marked rate-limited or when a cooldown expires, the config is updated on disk
immediately.

```csharp
/// Marks a provider as temporarily unavailable due to rate limiting.
/// Persists the cooldown expiry to .ags/config.json.
void MarkRateLimited(string providerId, DateTimeOffset availableAfter);

/// Gets the time at which a rate-limited provider becomes available again.
/// Returns null if the provider is not rate-limited.
DateTimeOffset? GetCooldownExpiry(string providerId);
```

#### Task Failover

When a rate limit is hit during task execution:

1. The Agent Orchestrator logs the rate-limit event.
2. It consults the current agent's `models` list (priority-ordered) to find the
   next available provider.
3. If an available provider is found, the orchestrator **restarts the current
   task from the beginning** using that provider. Any partial output from the
   failed invocation is discarded.
4. If no providers are available (all are rate-limited), the orchestrator reports
   the situation to the CEO and waits until the earliest cooldown expires, then
   retries automatically.
5. The CEO is informed of provider switches via a status message (e.g.,
   `"claude-sonnet rate-limited until 14:32, switching to chatgpt"`).

#### Configuration

| Setting | Location | Default | Description |
|---|---|---|---|
| `RateLimitDefaultCooldown` | `.ags/config.json` | `30` (minutes) | Cooldown period when the reset time cannot be parsed from the provider response |
| `ProviderCooldowns` | `.ags/config.json` | `{}` | Map of provider ID to cooldown expiry timestamp (ISO 8601). Persisted across restarts; expired entries are cleaned up automatically |

---

## 7. Agent Orchestrator

### 7.1 Purpose

The Agent Orchestrator is the central coordinator that translates session tasks
into AI provider calls. It reads agent definitions, assembles prompts, manages
inter-agent communication, and persists results.

### 7.2 Prompt Assembly

For each agent invocation, the orchestrator assembles a prompt from:

1. **Agent definition** (resolved via Resource Loader from standard or
   project-local `agents/<agent-name>.md`) — role, responsibilities, constraints
2. **Applicable rules** (resolved via Resource Loader from standard or
   project-local `rules/*.md`) — session workflow, context management, directory
   structure, agent coordination
3. **Task context** — current task brief, session scope, execution plan
4. **Project context** — relevant files listed in the task brief
5. **CEO instructions** — any specific guidance from the current interaction

The assembly order matters: rules constrain the agent, the agent definition
shapes the role, the task context scopes the work, and CEO instructions override
where applicable.

### 7.3 Inter-Agent Communication

When agent A needs input from agent B during task execution:

1. Agent A's output indicates a dependency on another agent's domain.
2. The orchestrator detects the dependency (by parsing agent output or by
   predefined task dependencies in the execution plan).
3. The orchestrator invokes agent B as a separate subprocess with:
   - Agent B's definition as system prompt
   - A focused request derived from agent A's need
   - Relevant shared files
4. Agent B writes its output to a handoff file in the session's task directory.
5. The orchestrator feeds agent B's output back to agent A in a follow-up call.
6. Agent A continues its work with the new information.

All inter-agent communication is persisted in files. There are no in-memory
message queues between agents.

### 7.4 Agent Invocation Lifecycle

```
1. Read task brief -> determine responsible agent
2. Read agent definition file
3. Read applicable rules
4. Assemble system prompt + task prompt
5. Call AI provider via IAIProvider.Invoke()
6. Capture result
7. Detect modified files (git diff)
8. Update task brief with decisions, modified files, results
9. If inter-agent handoff needed -> invoke secondary agent (steps 1-8)
10. Present result summary to CEO
11. Wait for CEO approval or correction
12. If correction -> re-invoke with correction context
13. If approval -> mark step complete, advance to next
```

---

## 8. Session Manager

### 8.1 Session Lifecycle Implementation

The Session Manager implements the lifecycle defined in
`rules/session-workflow.md`:

| Phase | Implementation |
|---|---|
| Scoping | Invoke scoping agents, run Q&A loop with CEO, write `session-scope.md` |
| Planning | Invoke planning agents, produce `execution-plan.md`, CEO approval |
| Execution | Sequential task execution through Agent Orchestrator |
| Pause/Resume | Persist state to files, restore from files |
| Completion | Finalize all task briefs, merge session branch |

### 8.2 Session Directory Management

On session creation:
1. Generate session-id: `<yyyy-mm-dd>-<slug>`
2. Create `.ags/sessions/<session-id>/`
3. Create `state.md` with initial metadata
4. Register in `.ags/sessions/index.md`
5. Create git branch `session/<session-id>`

On session completion:
1. Mark all tasks done/skipped in `execution-plan.md`
2. Update `state.md` status to `completed`
3. Update `index.md`
4. Prepare PR from session branch to main
5. Notify CEO that PR is ready for review

### 8.3 Session Index Management

`.ags/sessions/index.md` is the authoritative list of all sessions. The Session
Manager updates it on every state transition. The main menu reads it to show
resumable sessions.

---

## 9. Git Manager

### 9.1 Branch Strategy

- `main` — stable, CEO-approved code
- `session/<session-id>` — one branch per session, all agent work lands here

### 9.2 Commit Strategy

- Each completed task produces at least one commit on the session branch.
- Commit messages follow the format: `[<session-id>] <task-slug>: <description>`
- Agent-generated code is committed automatically after task completion.
- CEO can review the diff at any point.

### 9.3 PR Flow

On session completion:
1. Git Manager ensures the session branch is up to date with main.
2. If conflicts exist, a merge resolution task is created for the appropriate
   agent.
3. A PR description is generated from the session scope and execution plan.
4. The PR is presented to the CEO for review.
5. CEO approves -> merge to main.
6. Session branch is deleted after merge.

### 9.4 Implementation

```csharp
/// Manages git operations for AGS sessions.
class GitManager
{
    /// Creates a new branch for a session.
    void CreateSessionBranch(string sessionId);

    /// Commits current changes with a task-scoped message.
    void CommitTaskChanges(string sessionId, string taskSlug, string description);

    /// Checks for conflicts between the session branch and main.
    bool HasConflictsWithMain(string sessionId);

    /// Prepares a PR description from session artifacts.
    string GeneratePRDescription(string sessionId);

    /// Merges the session branch into main.
    void MergeToMain(string sessionId);

    /// Deletes the session branch after successful merge.
    void DeleteSessionBranch(string sessionId);
}
```

---

## 10. Build System Integration

### 10.1 Purpose

AGS must be able to build the game project and run automated tests without
launching the game itself.

### 10.2 Engine-Specific Build Commands

| Engine | Build Command | Test Command |
|---|---|---|
| Unity | `Unity -batchmode -buildTarget <target> -executeMethod <method> -quit` | `Unity -batchmode -runTests -testResults <path> -quit` |
| Unreal | `UnrealBuildTool <project> <target> <platform> <config>` | `UnrealEditor-Cmd <project> -RunAutomationTests` |
| Godot | `godot --export-release <preset> <path>` | `godot --headless --script <test-runner>` |

### 10.3 Build Manager Interface

```csharp
/// Defines the contract for building and testing a game project.
interface IBuildManager
{
    /// Gets the engine this manager supports.
    string EngineId { get; }

    /// Builds the project and returns the result.
    BuildResult Build(BuildRequest request);

    /// Runs automated tests and returns the result.
    TestResult RunTests(TestRequest request);
}
```

### 10.4 Integration with Session Workflow

Testing is a mandatory step in every execution plan. The Agent Orchestrator
invokes the Build Manager:
- After code generation tasks, to verify the project still compiles.
- During dedicated test tasks, to run the full test suite.
- Before session completion, as a final verification gate.

Test results are recorded in the relevant task brief under
"Verification / Test Results".

---

## 11. CLI User Experience

### 11.1 Main Menu Structure

```
=== Agentic Game Studio ===

Project: My Game (Unity) | Stage: Production | Active session: none

Recommended next steps:
  1. Resume session "2026-03-28-inventory-ui" (paused)
  2. Start new session: "Combat System Implementation" (suggested)
  3. Start new session: "QA Pass - Core Loop" (suggested)

Other options:
  4. Start a custom session
  5. Settings
  6. Exit

Select an option:
```

### 11.2 Session Interaction

During an active session, the CLI operates in a conversational loop:

```
[Session: 2026-03-30-combat-system | Task 3/7: damage-calculation]

Agent (systems-designer) proposes:
  Damage formula: base_damage * (1 + strength_modifier) - armor_reduction

  Options:
    A) Use this formula (simple, predictable)
    B) Add critical hit multiplier (more depth, needs balancing)
    C) Suggest a different approach

  Your choice (A/B/C/custom):
```

### 11.3 Progress Display

During agent work (AI provider call in progress):

```
[Working...] systems-designer is designing damage calculation...
```

After task completion:

```
[Done] Task 3/7 "damage-calculation" completed.
  Files modified: assets/scripts/Combat/DamageCalculator.cs (new)
  Files modified: assets/scripts/Combat/DamageCalculator.Tests.cs (new)

  Next task: 4/7 "hit-detection"
  Proceed? (Y/n):
```

### 11.4 CEO Decision Points

The CEO is prompted at these points:
- Session scope approval
- Execution plan approval
- Each agent decision that requires a choice
- Task completion acknowledgement
- Session completion and PR review
- Stage transition confirmation

---

## 12. Persistence Model

### 12.1 File Structure

All persistent state lives under `.ags/` and `gdd/` as defined in
`rules/directory-structure.md`.

Key state files:

| File | Purpose |
|---|---|
| `.ags/config.json` | Application settings (active provider, engine, preferences, provider cooldowns) |
| `.ags/sessions/index.md` | Session registry |
| `.ags/sessions/<id>/state.md` | Session metadata and current status |
| `.ags/sessions/<id>/session-scope.md` | Approved scope |
| `.ags/sessions/<id>/execution-plan.md` | Approved execution plan |
| `.ags/sessions/<id>/tasks/*.md` | Task briefs |
| `gdd/game-concept.md` | Game conceptual vision |
| `gdd/engine-decision.md` | Engine selection record |

### 12.2 State Recovery

On application startup:
1. Read `.ags/config.json` for settings.
2. Read `.ags/sessions/index.md` for session inventory.
3. Detect project stage from disk artifacts.
4. Identify any in-progress or paused sessions.
5. Present main menu with context-aware options.

On crash recovery:
1. Follow the recovery protocol from `rules/context-management.md`.
2. Read `state.md` of the last active session.
3. Read the current task brief.
4. Resume from recorded next step.

---

## 13. Testing Strategy

### 13.1 AGS Application Tests

- Unit tests for all business logic (stage detection, recommendation engine,
  prompt assembly, git operations, session management).
- Integration tests for AI provider adapters using mock CLI responses.
- Integration tests for build system integration using mock engine CLIs.
- Target: 80% code coverage as specified in AGENTS.md.

### 13.2 Test Organization

All tests live in the `tests/` directory, mirroring the `src/` structure:

```
tests/
  StageDetection/
  NextStepsRecommender/
  SessionManager/
  AgentOrchestrator/
  AIProvider/
  BuildSystem/
  GitManager/
```

### 13.3 Mock Strategy

AI provider calls are mocked at the `IAIProvider` interface level. Tests verify
that:
- Correct prompts are assembled for each agent/task combination.
- Results are correctly parsed and persisted.
- Inter-agent handoffs work through file-based communication.
- Session state transitions follow the defined lifecycle.

---

## 14. Implementation Roadmap

### Phase 0: Installation and Distribution

Set up the global installation model, resource overlay, and installer script.

**0.1 Project Restructure for Global Install**
- [x] Reorganize the repo so that `agents/`, `rules/`, `skills/`, `templates/`
  are published alongside the binary as standard resources
- [x] Configure .csproj to include standard resources in the publish output
- [x] Ensure the binary resolves its own install directory at runtime to locate
  standard resources
- [x] Tests: verify resource files are present in publish output

**0.2 Resource Loader**
- [x] Implement `ResourceLoader` class (resolve standard vs. project-local)
- [x] Implement `ResolveResourcePath` (project `.ags/` first, then install dir)
- [x] Implement `ListResources` (merged listing from both layers)
- [x] Implement `ReadResource`
- [x] Wire Resource Loader into existing subsystems that read agents, rules,
  skills, templates
- [x] Tests: overlay resolution (project wins), additions, standard-only
  fallback

**0.3 Windows Installation Script**
- [x] Write `scripts/install.ps1` PowerShell script
- [x] Detect or prompt for install directory (default `%USERPROFILE%\ags`)
- [x] Download latest release .zip from GitHub Releases API
- [x] Extract archive to install directory
- [x] Add install directory to user `PATH` (idempotent, no duplicates)
- [x] Verify installation by running `ags -version`
- [x] Print success message with next steps
- [x] Tests: manual testing on clean Windows install

**0.4 GitHub Release Packaging**
- [x] Configure CI or release script to produce a self-contained .zip with
  `ags.exe` + standard resources
- [x] Publish .zip as a GitHub Release asset
- [x] Add release notes template

### Phase 1: Foundation (Infrastructure)

Establish the core infrastructure that everything else builds on.

**1.1 AI Provider Abstraction**
- [x] Define `IAIProvider`, `AIProviderRequest`, `AIProviderResult` interfaces
- [x] Implement `ClaudeCodeAdapter` (CLI subprocess invocation)
- [x] Implement `CodexAdapter`
- [x] Implement `AIProviderRegistry`
- [x] Add provider selection to settings
- [x] Tests: unit tests for adapters with mock CLI, registry tests

**1.1.1 Rate Limit Handling and Provider Failover**
- [x] Extend `AIProviderResult` with `IsRateLimited` and `RateLimitResetsAt`
- [x] Implement rate-limit detection in `ClaudeCodeAdapter` (parse stderr / exit
  code / error JSON for quota-exceeded signals)
- [x] Implement rate-limit detection in `CodexAdapter`
- [x] Add provider cooldown tracking to `AIProviderRegistry`
  (`MarkRateLimited`, `GetCooldownExpiry`, automatic expiry)
- [x] Persist cooldown state (`ProviderCooldowns`) to `.ags/config.json`;
  restore on startup; clean up expired entries on write
- [x] `IsAvailable` returns `false` while provider is in cooldown
- [x] Add `RateLimitDefaultCooldown` setting to `.ags/config.json` schema
  (default: 30 minutes)
- [ ] Implement task failover in Agent Orchestrator: on rate-limit, select next
  available provider from agent's `models` list and restart task
  (infrastructure ready: `GetNextAvailableProvider` on registry; wiring deferred
  to Phase 3 when the Agent Orchestrator is built)
- [ ] Implement wait-and-retry when all providers are rate-limited (wait for
  earliest cooldown expiry, then retry)
  (infrastructure ready: `GetEarliestCooldownExpiry` on registry; wiring deferred
  to Phase 3)
- [ ] Add CEO notification on provider switch and on all-providers-exhausted
  (deferred to Phase 3 / Phase 6 UX layer)
- [x] Tests: rate-limit detection per adapter, cooldown expiry, failover
  selection, all-exhausted wait behavior

**1.2 Git Manager**
- [x] Implement branch creation (`session/<id>`)
- [x] Implement task commit logic
- [x] Implement conflict detection with main
- [x] Implement PR description generation
- [x] Implement merge and branch cleanup
- [x] Tests: integration tests with a test git repo

**1.3 Prompt Assembly Engine**
- [ ] Build prompt assembler that reads agent .md files
- [ ] Build rule loader that reads rules/*.md
- [ ] Build context assembler that reads task briefs and session state
- [ ] Define prompt template structure (agent def + rules + task context + CEO input)
- [ ] Tests: unit tests for prompt composition

### Phase 2: Session System

Implement the session lifecycle from `rules/session-workflow.md`.

**2.1 Session Manager Core**
- [ ] Session creation (directory, state.md, index.md registration)
- [ ] Session state transitions (scoping -> scope-approved -> planning -> ... ->
  completed)
- [ ] Session pause and resume
- [ ] Session directory structure management
- [ ] Git branch creation on session start
- [ ] Tests: lifecycle state machine tests

**2.2 Scoping Protocol**
- [ ] Agent selection for scoping (from agent-coordination.md)
- [ ] Q&A loop implementation (agent asks -> CEO answers -> repeat until complete)
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

Wire up agent invocations through the AI provider.

**3.1 Single-Agent Invocation**
- [ ] Assemble prompt for a single agent from definition + rules + task context
- [ ] Invoke via AI provider
- [ ] Capture and parse result
- [ ] Detect modified files via git diff
- [ ] Update task brief with results
- [ ] Tests: end-to-end with mock provider

**3.2 Inter-Agent Communication**
- [ ] Detect when agent A needs input from agent B
- [ ] Invoke agent B as a separate subprocess
- [ ] Write handoff file to session task directory
- [ ] Feed handoff result back to agent A
- [ ] Tests: multi-agent handoff scenarios

**3.3 CEO Interaction Loop**
- [ ] Present agent proposals with options to CEO
- [ ] Capture CEO choice
- [ ] Feed CEO decision back to agent
- [ ] Handle corrections and re-invocations
- [ ] Tests: interaction flow with simulated CEO input

### Phase 4: Stage Detection and Recommendations

**4.1 Stage Detection Engine**
- [ ] Artifact scanner (check file existence, read status markers)
- [ ] Stage determination logic per detection rules (section 4.2)
- [ ] Stage display in main menu
- [ ] Tests: one test per stage with fixture directories

**4.2 Next Steps Recommender**
- [ ] Recommendation rules per stage (section 5.2)
- [ ] Agent involvement lookup from agent-coordination.md
- [ ] Complexity estimation (light/medium/heavy)
- [ ] Recommendation presentation in main menu
- [ ] Tests: recommendation output per stage scenario

### Phase 5: Build System Integration

**5.1 Build Manager**
- [ ] Define `IBuildManager` interface
- [ ] Implement Unity build adapter (CLI invocation)
- [ ] Implement Unreal build adapter (stub, lower priority)
- [ ] Implement Godot build adapter (stub, lower priority)
- [ ] Tests: mock engine CLI responses

**5.2 Test Runner Integration**
- [ ] Invoke engine test runner
- [ ] Parse test results
- [ ] Record results in task briefs
- [ ] Fail task if tests fail
- [ ] Tests: result parsing, task state on failure

**5.3 Build Verification in Session Flow**
- [ ] Post-code-generation build check
- [ ] Pre-session-completion build gate
- [ ] Integration with Agent Orchestrator (trigger build after code tasks)
- [ ] Tests: session flow with build steps

### Phase 6: Main Menu and UX Polish

**6.1 Enhanced Main Menu**
- [ ] Show project name, engine, current stage
- [ ] Show recommended next steps from recommender
- [ ] Show resumable sessions from index
- [ ] Contextual options based on project state
- [ ] Tests: menu rendering with various project states

**6.2 Session Progress Display**
- [ ] Working indicator during AI provider calls
- [ ] Task completion summaries
- [ ] File modification reports
- [ ] Session completion summary with PR info
- [ ] Tests: output formatting

**6.3 Error Handling and Recovery**
- [ ] Graceful handling of AI provider failures
- [ ] Session state recovery after crash
- [ ] Git state recovery (dirty working tree, failed merges)
- [ ] Clear error messages for CEO
- [ ] Tests: failure scenarios

### Phase 7: ags-start Skill Implementation

**7.1 Onboarding Flow**
- [ ] Implement the `ags-start` skill as a code path (not just a prompt file)
- [ ] Repository inspection (engine fingerprints, existing docs)
- [ ] Entry state routing (A/B/C/D paths)
- [ ] Concept document generation
- [ ] Engine decision document generation
- [ ] First session recommendations
- [ ] Tests: each entry path, document generation

### Phase 8: Hardening

**8.1 Edge Cases**
- [ ] Empty repository handling
- [ ] Corrupted state file recovery
- [ ] Multiple engine fingerprints detected
- [ ] Session with no tasks
- [ ] Agent invocation timeout handling

**8.2 Documentation**
- [ ] Update README.md with usage instructions
- [ ] CLI help text for all commands
- [ ] Configuration reference

**8.3 Performance**
- [ ] Optimize file scanning for large projects
- [ ] Optimize git operations for repos with many branches

---

## 15. Dependencies and Risks

### External Dependencies

| Dependency | Risk | Mitigation |
|---|---|---|
| Claude Code CLI | API changes, availability | Adapter pattern isolates changes |
| Codex CLI | API changes, availability | Adapter pattern isolates changes |
| Unity CLI | Version-specific flags | Build adapter handles version differences |
| Git | Must be installed | Detect on startup, clear error message |

### Technical Risks

| Risk | Impact | Mitigation |
|---|---|---|
| AI provider output is unpredictable | Agents may produce unexpected file changes | Git diff review before commit; CEO approval gate |
| Large projects slow down file scanning | Stage detection becomes slow | Incremental scanning; cache last-known state |
| Context limits in AI providers | Complex tasks may not fit in one call | Provider's responsibility (per design decision); split tasks smaller in planning |
| AI provider rate limits mid-task | Task fails partway, wasted tokens and time | Detect rate-limit responses, mark provider on cooldown, failover to next provider in agent's `models` list, restart task |
| Git conflicts between sessions | Merging session branches may fail | Conflict detection before merge; agent-assisted resolution |

---

## 16. Non-Goals (Explicit Exclusions)

- Art asset generation (images, 3D models, textures)
- Audio asset generation (music, SFX, voice)
- Game launching or runtime testing (manual playtest is CEO's job)
- Database or server-side persistence
- Web UI or GUI (CLI only)
- Multi-user collaboration features (single CEO model)
- Cloud deployment of AGS itself
