# Agentic Game Studio

[![Claude Code](https://img.shields.io/badge/Claude%20Code-compatible-7c3aed)](https://docs.claude.com/claude-code)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Status](https://img.shields.io/badge/status-experimental-orange)]()
[![Engine](https://img.shields.io/badge/engine-agnostic-informational)]()

**A solo-developer framework that turns Claude Code into a full game studio.** It gives you a team of specialised AI agents (designers, programmers, QA, art, audio, release), a library of document templates, and a catalogue of slash commands that walk you through the entire lifecycle of a game — from the first concept brainstorm to the day-one patch.

You remain the director and the final decision-maker. The agents act as expert consultants: they ask focused questions, propose options with trade-offs, draft documents and code on your approval, and never write to disk or commit anything without explicit confirmation.

Heavily inspired by [Claude-Code-Game-Studios](https://github.com/Donchitos/Claude-Code-Game-Studios).

---

## Table of contents

1. [What you get](#what-you-get)
2. [Requirements](#requirements)
3. [Quick start](#quick-start)
4. [Repository layout](#repository-layout)
5. [Core concepts](#core-concepts)
6. [How you collaborate with the agents](#how-you-collaborate-with-the-agents)
7. [The agent hierarchy](#the-agent-hierarchy)
8. [The development workflow](#the-development-workflow)
9. [Document boundaries — one fact, one home](#document-boundaries--one-fact-one-home)
10. [Design principles baked into the framework](#design-principles-baked-into-the-framework)
11. [The combined review loop](#the-combined-review-loop)
12. [Customising the framework](#customising-the-framework)
13. [Credits & license](#credits--license)

---

## What you get

Cloning this repository gives you a ready-to-use project skeleton with:

- **A rule set** (`.ags/rules/`) that tells Claude Code how to behave: how to talk to you, how to manage state, how to write code, how to review documents, how to coordinate agents.
- **A template library** (`.ags/templates/`) for every artefact a real studio produces: game concept, GDDs, ADRs, art bible, UX specs, HUD design, QA plans, milestones, post-mortems, patch notes, and dozens more.
- **A skill catalogue** (`.claude/skills/`) — the `/ags-*` slash commands. Each skill drives one workflow step (brainstorming a concept, authoring a GDD, planning an epic, running QA, cutting a release, etc.) and enforces the rules and templates automatically.
- **Agent role definitions** (`.claude/agents/`) — twenty-plus specialised personas (creative director, lead programmer, ux-designer, unity-specialist, qa-lead, …) that Claude Code routes work to depending on the task.
- **Engine references** (`.ags/docs/engine-reference/`) — version-pinned snapshots of engine APIs and best practices, so the AI never guesses outdated signatures.

The framework is engine-agnostic in design; the only engine with fully-verified rules today is **Unity**.

---

## Requirements

- **Claude Code** — either the [CLI](https://docs.claude.com/claude-code) or the desktop app.
- **Git**.
- **A target game engine.** Unity is the recommended default. Other engines (Godot, Unreal, custom) work in principle, but their rule files are not yet hardened.
- *Optional:* the [`codex`](https://github.com/openai/codex) CLI for the external reviewer pass. The framework runs without it — Codex calls are simply skipped if the binary is not available.

---

## Quick start

```bash
git clone <this-repo> my-game
cd my-game
```

Open the project in Claude Code (CLI: `claude` inside the project directory; desktop: open the folder as a workspace), then walk through these steps in order:

1. **`/ags-start`** — guided onboarding. The agent asks you how you want to be addressed, picks a chat language, configures the target engine and version, captures the game concept in a single sentence, and chooses a review intensity.
2. **Concept phase** — bootstrap the design from the top down:
   - `/ags-brainstorm` to expand the concept into a structured game concept document.
   - `/ags-map-systems` to decompose the concept into discrete game systems and order them by dependency.
   - `/ags-design-system <system>` for each system to author a Game Design Document (GDD).
   - `/ags-art-bible` to lock the visual identity before any asset work begins.
   - `/ags-create-architecture` once the GDDs are stable, to draft the master architecture document and the first ADRs.
3. **First vertical slice** — move into the Production phase:
   - `/ags-create-epics` to define a vertical-slice epic touching one to three systems.
   - `/ags-epic-contracts` to lock the minimal API contracts between in-scope and stubbed systems.
   - `/ags-create-stories` to break the epic into implementation-ready stories.
   - `/ags-dev-story` for each story to implement the code with the right specialist agent.
4. **Quality & close** — once the slice runs end-to-end:
   - `/ags-qa-plan` and `/ags-smoke-check` to validate the build.
   - `/ags-gate-check epic-done` to verify nothing is left half-implemented.
   - `/ags-epic-retro` to capture lessons before starting the next epic.
5. **Lost?** — `/ags-help` reads the current project state and recommends the next step. `/ags-project-stage-detect` audits the whole project and reports where you are.

---

## Repository layout

```
CLAUDE.md             Master configuration loaded by Claude Code on every session
.claude/              Claude Code wiring
  agents/             Agent role specifications (one Markdown file per role)
  skills/             Slash-command skills (the /ags-* catalogue)
  hooks/              Lifecycle hook scripts
  settings.json       Project-level Claude Code settings
.ags/                 Studio workflow
  rules/              Behavioural rules (collaboration, coding, coordination, …)
  templates/          Document templates (t_*.md and t_*.html)
  docs/               Engine API snapshots and references
  project/            Live project state (mostly gitignored)
    state.md          Active session — overwritten on every new task
    stage.md          Phase + active epic + transition history
    stubs.md          Registry of TODO stubs
    decisions-log.md  Append-only decision chronology
    epics/            One folder per epic, with EPIC.md, scope.html, stories/
    qa/, bugs/, …     Domain-specific working artefacts
design/               Design documents — the project's source of truth for intent
  gdd/                Game Design Documents
  architecture/       ADRs and the master architecture document
  art/                Art bible, DESIGN.md, character profiles
  ux/                 UX specs, HUD design, interaction patterns
  narrative/          Lore, character sheets, dialogue specs
  registry/           Canonical entity ids (entities.yaml)
assets/               Game assets (art, audio, vfx, shaders, data)
tests/                Test code (engine-agnostic location)
<engine project>/     Engine source root, e.g. Assets/ for Unity
```

---

## Core concepts

A handful of recurring terms appear in every skill and rule file. They are worth learning early.

| Term | What it means in this framework |
|------|--------------------------------|
| **Phase** | One of four lifecycle stages — Concept, Production, Polish, Release — tracked in `stage.md`. Each phase has its own gate. |
| **Epic** | A *vertical slice* covering one to three game systems, shipped end-to-end. Production work is structured as a sequence of epics rather than as parallel horizontal layers. |
| **Story** | A one-to-three-day implementation task inside an epic. A story always cites the GDD requirement it serves and any governing ADR. |
| **Stub** | A deliberately minimal implementation of a neighbour system that an epic needs to reach but is not ready to build in full. Stubs carry a `// TODO(epic-<id>)` marker and live in `stubs.md` until a future epic closes them. |
| **GDD** | Game Design Document. Describes *what* a system does and *why* — concept, mechanics, balance intent, acceptance. Never describes *how* the code is structured. |
| **ADR** | Architecture Decision Record. Documents a single significant technical choice — the realisation of one or more GDD requirements. |
| **UX-spec / HUD-spec** | UX specs describe screen flows and controls; HUD specs describe in-game widgets and their data bindings. |
| **DESIGN.md** | The single source of truth for visual design tokens — colours, typography, spacing, radii, components — authored in the [DESIGN.md format](https://github.com/google-labs-code/design.md). UI code binds to tokens; raw hex codes and pixel literals are forbidden in UI code. |
| **Entity registry** | `design/registry/entities.yaml` — the canonical list of item, enemy, skill and faction ids. Every other document references entities by id, never re-defines them. |
| **Gate** | A quality checkpoint that blocks a phase transition or epic close until measurable criteria pass. Examples: `/ags-gate-check epic-done`, `/ags-launch-checklist`. |
| **Approval front-matter** | Every document under boundary control carries a YAML header with `status: draft` or `status: approved` plus `approved_at: YYYY-MM-DD`. Skills check this header automatically and refuse to start dependent work when a prerequisite document is still a draft. |
| **Review loop** | Every document-producing skill runs internal reviewers and an external reviewer in parallel, with severity rules that tighten as iterations grow. See [The combined review loop](#the-combined-review-loop). |
| **`state.md`** | The single active-session file. Holds the current task's progress, decisions and open questions. It is *overwritten* every time you start a new task — its history lives in git. |
| **`stage.md`** | Persistent across tasks. Holds the current phase, the active epic, and the chronology of phase transitions. |

---

## How you collaborate with the agents

Every interaction follows the same five-step rhythm, codified in [.ags/rules/collaboration.md](.ags/rules/collaboration.md):

> **Question → Options → Decision → Draft → Approval**

1. **Question.** The agent asks a focused, scoped question — never an open-ended "what should we do?".
2. **Options.** It offers two to four concrete options with the trade-offs spelled out, and recommends one.
3. **Decision.** You pick. The agent never decides for you on anything non-trivial.
4. **Draft.** The agent drafts the artefact (a GDD section, an ADR, a code change) and shows it to you *in your chat language*.
5. **Approval.** Once you approve, the agent translates the content into English (the on-disk format) and writes it to the file. Without explicit approval, nothing is written and nothing is committed.

Long documents are written *incrementally*: one section is discussed, approved, and saved before the next section is opened. This keeps context windows small and lets you change direction without throwing away work.

---

## The agent hierarchy

Work flows through three tiers plus two Unity engine specialists. Leadership delegates downward; specialists never modify files outside their domain without an explicit delegation from a lead.

```mermaid
flowchart TD
    Human([Human])

    %% Tier 1 — Leadership
    CD[creative-director]
    TD[technical-director]
    PR[producer]

    %% Tier 2 — Department Leads
    GD[game-designer]
    AD[art-director]
    ND[narrative-director]
    AUD[audio-director]
    LP[lead-programmer]
    QL[qa-lead]
    RM[release-manager]

    %% Tier 3 — Specialists
    SYS[systems-designer]
    TA[technical-artist]
    UX[ux-designer]
    GP[gameplay-programmer]
    EP[engine-programmer]
    AI[ai-programmer]
    TL[tools-programmer]
    UIP[ui-programmer]
    PA[performance-analyst]

    %% Engine specialists
    UTS[unity-specialist]
    DOTS[unity-dots-specialist]

    Human --> CD
    Human --> TD
    Human --> PR

    CD --> GD
    CD --> AD
    CD --> ND
    CD --> AUD

    TD --> LP
    TD --> PA
    TD --> TA
    TD --> UTS

    PR -.coordinates.-> CD
    PR -.coordinates.-> TD
    PR --> QL
    PR --> RM

    GD --> SYS
    AD --> TA
    AD --> UX

    LP --> GP
    LP --> EP
    LP --> AI
    LP --> TL
    LP --> UIP

    UTS --> DOTS
    UTS -.advises.-> LP
    UTS -.advises.-> TA
    UTS -.advises.-> UIP

    classDef tier1 fill:#1f3a5f,stroke:#4a90d9,color:#fff
    classDef tier2 fill:#2d5a3d,stroke:#5cb85c,color:#fff
    classDef tier3 fill:#4a3a5c,stroke:#a78bda,color:#fff
    classDef engine fill:#3a4a6c,stroke:#7ba2d9,color:#fff

    class CD,TD,PR tier1
    class GD,AD,ND,AUD,LP,QL,RM tier2
    class SYS,TA,UX,GP,EP,AI,TL,UIP,PA tier3
    class UTS,DOTS engine
```

**Solid arrows** show delegation; **dotted arrows** show coordination or advisory relationships.

- **Tier 1 — Leadership.** Sets vision, architecture, and schedule. The creative director resolves conflicts between design pillars; the technical director makes binding calls on technology choices; the producer coordinates across departments.
- **Tier 2 — Department Leads.** Own their domain. Each lead absorbs the work of specialists that aren't separately modelled — for example, the `audio-director` covers composition and sound design, the `narrative-director` covers writing and localisation, the `qa-lead` covers test execution.
- **Tier 3 — Specialists.** Implementers inside a single domain. They cannot make cross-domain decisions on their own.
- **Engine specialists.** `unity-specialist` is the authority on Unity APIs, shader pipelines, addressables, and UI Toolkit. `unity-dots-specialist` handles DOTS/ECS, Jobs, and Burst.

Conflicts escalate to the shared parent in the hierarchy. The full delegation matrix and escalation table live in [.ags/rules/coordination.md](.ags/rules/coordination.md).

---

## The development workflow

A project moves through four sequential phases. Each phase has entry criteria and a gate it must pass before the next phase opens.

```
Concept ──► Production ──► Polish ──► Release
```

- **Concept.** Brainstorm the idea, lock the pillars, map the systems, write the GDDs, draft the art bible, propose the first architecture. Output: an approved concept document, a systems index, a draft architecture, and at least one approved GDD.
- **Production.** Build the game one vertical slice at a time. This phase is where you spend most of your calendar; it is structured as a loop of epics (see below).
- **Polish.** Performance profiling, soak tests, security audit, accessibility passes, balancing iteration, localisation.
- **Release.** Certification, store submissions, release-candidate gate, day-one patch readiness.

### The epic loop

Inside the Production phase, work is organised as a continuous loop of vertical-slice epics. Each epic delivers something playable, even if rough.

```
plan ──► design ──► contracts ──► stories ──► implement
  ▲                                              │
  │                                              ▼
retro ◄── close (gate) ◄── review ◄── verify ◄──┘
```

1. **Plan** — `/ags-create-epics` picks one to three systems for the slice and names the stubs the epic will rely on.
2. **Design** — write or refresh the GDD sections that the epic needs. The lead programmer drafts ADRs for any new technical decisions.
3. **Contracts** — `/ags-epic-contracts` locks the minimal API between in-scope and stubbed systems so implementation doesn't drift.
4. **Stories** — `/ags-create-stories` decomposes the epic into one-to-three-day stories, each embedding its GDD requirement, ADR pointers, acceptance criteria, and test plan.
5. **Implement** — `/ags-dev-story` routes each story to the right specialist agent and walks through implementation plus tests.
6. **Verify** — `/ags-code-review`, `/ags-smoke-check`, `/ags-test-evidence-review`.
7. **Review** — playtest reports, bug triage, optional QA cycle via `/ags-team-qa`.
8. **Close (gate)** — `/ags-gate-check epic-done` refuses to close the epic if it leaves stubs without a migration plan or if any acceptance criterion is unverified.
9. **Retro** — `/ags-epic-retro` captures what worked, what didn't, and feeds the next plan.

Decisions accrue to `decisions-log.md`; the list of epics and their status lives in `epics/index.md`.

---

## Document boundaries — one fact, one home

The framework enforces a strict Single Source of Truth (SSoT) for every kind of decision. Duplication is treated as a contract violation: each fact has exactly one document type that owns it, and other documents reference it by link or id.

| Decision type | Owned by | Must *not* appear in |
|---------------|----------|----------------------|
| Concept, mechanic, balance *intent*, acceptance criteria | GDD (`design/gdd/<system>.md`) | ADR, UX-spec, HUD-spec, art-bible |
| Technical realisation, contracts, schemas, library choices, performance budgets in ms/MB/FPS | ADR (`design/architecture/adr-NNNN-*.md`) | GDD, UX-spec, HUD-spec |
| User flows, controls, screen states, navigation | UX-spec (`design/ux/<screen>.md`) | GDD, HUD-spec, ADR |
| HUD widgets, layout, visual data bindings | HUD-spec (`design/ux/hud.md`) | UX-spec, GDD, art-bible |
| Colours, typography, spacing, radii, component tokens | `design/art/DESIGN.md` | art-bible, UX-spec, HUD-spec, UI code |
| Numeric balance values | Data-config asset on the engine side | GDD (which cites the file by path) |
| Entity ids (items, enemies, skills, factions, …) | `design/registry/entities.yaml` | Every other document — they reference by id |
| Runtime ownership of mutable state | The ADR that declares it + the implementing system | Other systems read via interface or event |

### Approval gating

Every controlled document carries YAML front-matter at the top:

```yaml
---
status: draft        # or: approved
approved_at: 2026-05-15
---
```

Skills check this header automatically. For example, when you run `/ags-create-architecture` to write an ADR, the skill aborts with a clear error if the GDD it claims to realise is still in `status: draft`. The full **precondition chain** is:

- An **ADR** requires the GDD section it cites to be `approved`.
- A **UX-spec** requires its GDD to be `approved`.
- A **HUD-spec** requires its UX-spec *and* `design/art/DESIGN.md` to be `approved`.
- A **story touching player flow** must cite an approved UX-spec.
- A **story rendering UI** must cite a DESIGN.md token — raw hex codes or pixel values fail the lint.

The full SSoT matrix and enforcement rules live in [.ags/rules/document-boundaries.md](.ags/rules/document-boundaries.md).

---

## Design principles baked into the framework

The framework encodes nine system-level design principles ([.ags/rules/design-principles.md](.ags/rules/design-principles.md)) that complement the code-level SOLID/KISS/DRY/YAGNI rules in [.ags/rules/coding.md](.ags/rules/coding.md):

- **YAGNI — gameplay first.** Don't build a system until gameplay validates the need. A stub is always cheaper than a speculative full implementation.
- **KISS — simple core.** Start with the simplest possible foundation; iteration and balancing are far cheaper on a simple core than on a clever one.
- **Separation of concerns.** Gameplay, UI, audio, data, save, and AI live in separate assemblies. Cross-layer calls go through contracts documented in ADRs.
- **Loose coupling, high cohesion.** Systems talk to each other through interfaces, events, or a message bus — never via direct references across module boundaries.
- **Single source of truth.** Every fact has one owner. See the document-boundaries matrix above.
- **Fail fast.** Validate content, data, and configuration at load time. A loud crash is better than a corrupt save.
- **Observability by design.** Debug overlays, structured logs, counters, and inspectable state are part of every system from the first iteration — not bolted on after release.
- **Backward compatibility.** Save formats are versioned, every breaking change ships with a migration function, and mod APIs are kept stable across patches.
- **Evolutionary architecture.** Hide decisions behind interfaces and defer commitment where the cost of reversal is high. Every ADR records how reversible the decision is.

---

## The combined review loop

Every skill that produces a document runs the same review loop, defined canonically in [.ags/rules/review-workflow.md](.ags/rules/review-workflow.md). Reviewers run *in parallel* — both internal Claude reviewers (department lead + specialist + director gate) and the external Codex CLI fire at the same time and feed one aggregated pool of findings.

The aggregator (usually the `producer` agent or the lead designated by the skill) then:

1. Drops pure nitpicks.
2. Applies the **iteration severity floor**, which tightens as iterations accumulate:
   - **Iterations 1–2:** all severities (critical, high, medium, low) are kept.
   - **Iterations 3–4:** only critical and high are kept.
   - **Iterations 5+:** only critical is kept.
3. Exits the loop on the first iteration whose filtered findings set is empty.

There is no hard iteration cap — the severity floor guarantees convergence. If Codex is not installed, the external call is skipped silently and the skip is logged to `decisions-log.md`; you are not prompted.

Review reports accumulate in `.ags/project/reviews/`, one file per `<date>-<type>-<slug>`, with each iteration appended as a new `## Iteration N` section.

---

## Customising the framework

The whole framework is configured through plain Markdown files. Edit them and Claude Code picks up the change on the next session.

| File / directory | What it controls |
|------------------|------------------|
| [CLAUDE.md](CLAUDE.md) | Top-level project configuration and the list of rule files that are loaded automatically. |
| [.ags/rules/user-interaction.md](.ags/rules/user-interaction.md) | Chat language, tone, and the rule that documents are *drafted in your language* and *written to disk in English*. |
| [.ags/rules/collaboration.md](.ags/rules/collaboration.md) | The Question → Options → Decision → Draft → Approval protocol. |
| [.ags/rules/context-management.md](.ags/rules/context-management.md) | How state files are used, when to compact, and how to recover from a crashed session. |
| [.ags/rules/coding.md](.ags/rules/coding.md) | Engineering principles, test rules, stub conventions, performance and observability requirements. |
| [.ags/rules/design-principles.md](.ags/rules/design-principles.md) | The nine system-level design principles described above. |
| [.ags/rules/design-system.md](.ags/rules/design-system.md) | DESIGN.md as the visual-token authority, lint requirements, UI binding rules. |
| [.ags/rules/document-boundaries.md](.ags/rules/document-boundaries.md) | The SSoT matrix, approval front-matter, and the precondition chain. |
| [.ags/rules/review-workflow.md](.ags/rules/review-workflow.md) | The combined review loop contract. |
| [.ags/rules/coordination.md](.ags/rules/coordination.md) | Agent hierarchy, delegation rules, escalation paths, and workflow patterns. |
| [.ags/rules/director-gates.md](.ags/rules/director-gates.md) | The lean director-level approval gates used by `/ags-gate-check`. |
| [.ags/rules/directory-structure.md](.ags/rules/directory-structure.md) | The canonical repository layout. |
| [.ags/templates/](.ags/templates/) | All document templates, named `t_<doc>.md` or `t_<doc>.html`. HTML is used when inline visual layout adds value (art bible, epic scope). |
| [.claude/skills/](.claude/skills/) | The `/ags-*` slash-command catalogue. |
| [.claude/agents/](.claude/agents/) | The agent role specifications. |

If you want to harden the framework for an engine other than Unity, the work is concentrated in `.ags/rules/coding.md`, `.ags/docs/engine-reference/`, and the engine-specific specialist agents under `.claude/agents/`.

---

## Credits & license

Heavily inspired by Donchitos' [Claude-Code-Game-Studios](https://github.com/Donchitos/Claude-Code-Game-Studios).

Released under the [MIT License](LICENSE).
