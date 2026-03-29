# Agent Coordination

This document defines the organisational structure for all AI agents in this project:
the 4-level hierarchy, escalation rules, communication flow, and the Agent Responsibilities
Reference. It is the authoritative source for deciding which agents to engage for any topic.

---

## Hierarchy

The project uses a 4-level structure:

- Level 1 - CEO: The human user. Ultimate decision-maker. Not an AI agent.
- Level 2 - C-Level Agents: AI agents responsible for strategic domains such as creative,
  technology, and production.
- Level 3 - Lead / Domain-Owner Agents: AI agents that own a functional area under a C-Level
  and coordinate the specialists in that area.
- Level 4 - Specialist Agents: AI agents that execute focused, narrow domain tasks under a
  Level 3 lead.

Every AI agent must have a direct parent exactly one level above it. The coordination model
does not allow AI agents to skip a reporting level.

```mermaid
graph TD
    CEO["CEO (Human)"]

    CD["creative-director"]
    TD["technical-director"]
    PR["producer"]

    GD["game-designer"]
    AD["art-director"]
    ND["narrative-director"]
    AUD["audio-director"]
    LOD["live-ops-designer"]

    LP["lead-programmer"]
    QAL["qa-lead"]
    RM["release-manager"]
    SEC["security-engineer"]
    LL["localization-lead"]
    PT["prototyper"]
    ACCS["accessibility-specialist"]
    PFL["performance-lead"]
    PLT["platform-lead"]
    DL["data-lead"]
    UE3["unreal-specialist"]
    UN3["unity-specialist"]
    GO3["godot-specialist"]

    CM["community-manager"]

    SYD["systems-designer"]
    LVD["level-designer"]
    ECD["economy-designer"]
    TA["technical-artist"]
    UXD["ux-designer"]
    WR["writer"]
    WB["world-builder"]
    SD["sound-designer"]
    EVD["event-designer"]
    RTD["retention-designer"]

    GPP["gameplay-programmer"]
    EP["engine-programmer"]
    AIP["ai-programmer"]
    NP["network-programmer"]
    TP["tools-programmer"]
    UIP["ui-programmer"]
    QAT["qa-tester"]
    ROS["release-operations-specialist"]
    SANA["security-analyst"]
    LOCS["localization-specialist"]
    LQA["locale-qa-specialist"]
    PPP["prototype-programmer"]
    ACT["accessibility-tester"]
    PA["performance-analyst"]
    DO["devops-engineer"]
    AE["analytics-engineer"]
    CMD["community-moderator"]
    SMS["social-media-specialist"]

    UEGAS["ue-gas-specialist"]
    UEBP["ue-blueprint-specialist"]
    UEREP["ue-replication-specialist"]
    UEUMG["ue-umg-specialist"]

    UDOTS["unity-dots-specialist"]
    USHD["unity-shader-specialist"]
    UADDR["unity-addressables-specialist"]
    UUISP["unity-ui-specialist"]

    GGDS["godot-gdscript-specialist"]
    GSHD["godot-shader-specialist"]
    GEXT["godot-gdextension-specialist"]

    CEO --> CD
    CEO --> TD
    CEO --> PR

    CD --> GD
    CD --> AD
    CD --> ND
    CD --> AUD
    CD --> LOD

    TD --> LP
    TD --> QAL
    TD --> RM
    TD --> SEC
    TD --> LL
    TD --> PT
    TD --> ACCS
    TD --> PFL
    TD --> PLT
    TD --> DL
    TD --> UE3
    TD --> UN3
    TD --> GO3

    PR --> CM

    GD --> SYD
    GD --> LVD
    GD --> ECD

    AD --> TA
    AD --> UXD

    ND --> WR
    ND --> WB

    AUD --> SD

    LOD --> EVD
    LOD --> RTD

    LP --> GPP
    LP --> EP
    LP --> AIP
    LP --> NP
    LP --> TP
    LP --> UIP

    QAL --> QAT
    RM --> ROS
    SEC --> SANA
    LL --> LOCS
    LL --> LQA
    PT --> PPP
    ACCS --> ACT
    PFL --> PA
    PLT --> DO
    DL --> AE

    CM --> CMD
    CM --> SMS

    UE3 --> UEGAS
    UE3 --> UEBP
    UE3 --> UEREP
    UE3 --> UEUMG

    UN3 --> UDOTS
    UN3 --> USHD
    UN3 --> UADDR
    UN3 --> UUISP

    GO3 --> GGDS
    GO3 --> GSHD
    GO3 --> GEXT

    classDef level2 fill:#4a90d9,color:#fff,stroke:#2c6fad
    classDef level3 fill:#7bc47f,color:#000,stroke:#4e9e52
    classDef level4 fill:#f5c842,color:#000,stroke:#c9a200

    class CD,TD,PR level2
    class GD,AD,ND,AUD,LOD,LP,QAL,RM,SEC,LL,PT,ACCS,PFL,PLT,DL,UE3,UN3,GO3,CM level3
    class SYD,LVD,ECD,TA,UXD,WR,WB,SD,EVD,RTD,GPP,EP,AIP,NP,TP,UIP,QAT,ROS,SANA,LOCS,LQA,PPP,ACT,PA,DO,AE,CMD,SMS,UEGAS,UEBP,UEREP,UEUMG,UDOTS,USHD,UADDR,UUISP,GGDS,GSHD,GEXT level4
```

**Diagram Legend**

- Level 1 (CEO / Human): single root node; ultimate decision-maker; default styling
- Level 2 (C-Level): blue fill; strategic domain owners; creative-director,
  technical-director, producer
- Level 3 (Lead / Domain Owner): green fill; functional leads, domain owners, and
  engine-lead specialists reporting to a C-Level
- Level 4 (Specialist): yellow fill; narrow-domain executors and engine sub-specialists
  reporting to a Level 3 lead

---

## Escalation Rules

**Disagreement:** When agents disagree and cannot reach a decision through lateral negotiation,
regardless of whether they are at the same level, from the same branch, or from different
branches, the conflict is resolved by recursive hierarchical escalation:

1. Each conflicting agent identifies its direct parent in the hierarchy as shown in the
   diagram above.
2. Those parent agents attempt to resolve the conflict directly between themselves.
3. If the parent agents cannot agree, the conflict is escalated one level further up the
   chain: each parent identifies its own direct parent, and those agents attempt resolution.
4. This process repeats level by level until either the conflict is resolved at some level or
   it reaches the CEO, who makes the final binding decision.

"Direct parent" means the agent's immediate supervisor as shown in the hierarchy diagram,
not the most domain-relevant agent at the level above.

**Special case - shared direct parent:** If step 1 yields the same parent for both conflicting
agents, step 2 collapses: there is only one parent agent, so that parent resolves the conflict
directly without a lateral-negotiation step between parents. This applies to any sibling pair,
for example `systems-designer` and `level-designer` under `game-designer`, or
`performance-lead` and `qa-lead` under `technical-director`.

**Blocked:** When an agent cannot proceed because of missing permissions, lack of information,
or work outside its scope:

1. Report the blocker upward to the direct parent agent with a clear description of what is
   needed.
2. Do not attempt to acquire permissions or expand scope unilaterally.
3. If the parent cannot resolve the blocker, it escalates further up the chain until it
   reaches the CEO.

Agents must not attempt to resolve cross-domain conflicts unilaterally. Escalation is not a
sign of failure; it is the correct procedure for maintaining clear ownership.

---

## Communication Flow

- Top-down: Task assignment flows from higher levels to lower levels. The CEO assigns work to
  C-Level agents; C-Level agents delegate to Lead / Domain-Owner agents; Lead / Domain-Owner
  agents direct Specialist agents.

- Bottom-up: Results, status updates, and blockers are reported upward. Specialists report to
  their Level 3 lead; Level 3 leads report to their C-Level; C-Levels report to the CEO.

- Lateral: Agents at the same level may collaborate directly as peers. Lateral collaboration
  does not require routing through a parent unless a conflict arises. Lateral communication
  should be transparent; outcomes are still reported upward.

---

## Agent Responsibilities Reference

This table is the primary lookup for deciding which agents to involve for a given topic.
Each row maps an agent to its area of responsibility and the types of questions or tasks
that belong to it.

| Agent File | Level | Area of Responsibility | Involve For |
|---|---|---|---|
| agents/creative-director.md | 2 - C-Level | Creative strategy and vision | Overall creative direction, tone, cross-team creative alignment, creative conflicts |
| agents/technical-director.md | 2 - C-Level | Technology strategy and engineering direction | Architecture decisions, tech stack choices, build strategy, engineering standards |
| agents/producer.md | 2 - C-Level | Production planning and delivery oversight | Milestone planning, resource allocation, execution priorities, stakeholder alignment |
| agents/game-designer.md | 3 - Lead / Domain Owner | Game design and player experience | Game systems design, gameplay loop, feature specs, design review |
| agents/art-director.md | 3 - Lead / Domain Owner | Visual art direction and standards | Art style consistency, asset quality bar, UX visual standards |
| agents/narrative-director.md | 3 - Lead / Domain Owner | Narrative and writing direction | Story structure, lore consistency, writing pipeline, world canon |
| agents/audio-director.md | 3 - Lead / Domain Owner | Audio direction and standards | Sound design vision, music direction, audio implementation guidelines |
| agents/live-ops-designer.md | 3 - Lead / Domain Owner | Live operations and ongoing content design | Events, seasonal content, engagement loops, live feature specs |
| agents/lead-programmer.md | 3 - Lead / Domain Owner | Programming team management | Code architecture, programming standards, feature scoping with engineering |
| agents/qa-lead.md | 3 - Lead / Domain Owner | Quality assurance management | Test strategy, bug triage policy, QA pipeline, release readiness |
| agents/release-manager.md | 3 - Lead / Domain Owner | Release and deployment coordination | Release planning, deployment gates, version management, go/no-go decisions |
| agents/security-engineer.md | 3 - Lead / Domain Owner | Security standards and review | Security audits, threat modelling, authentication review, vulnerability response |
| agents/localization-lead.md | 3 - Lead / Domain Owner | Localisation pipeline and standards | Translation workflows, locale-specific QA, cultural adaptation, string management |
| agents/prototyper.md | 3 - Lead / Domain Owner | Rapid prototyping and experimentation | Throwaway prototypes, mechanic validation, quick feasibility proofs |
| agents/accessibility-specialist.md | 3 - Lead / Domain Owner | Accessibility standards and implementation | Accessibility audits, game accessibility compliance, assistive feature design |
| agents/performance-lead.md | 3 - Lead / Domain Owner | Performance engineering leadership | Performance budgets, optimisation priorities, profiling standards, runtime regression escalation |
| agents/platform-lead.md | 3 - Lead / Domain Owner | Build platform and infrastructure leadership | CI/CD ownership, build platform direction, deployment infrastructure priorities, operational standards |
| agents/data-lead.md | 3 - Lead / Domain Owner | Telemetry and data platform leadership | Analytics architecture, event taxonomy, dashboard priorities, data governance |
| agents/unreal-specialist.md | 3 - Lead / Domain Owner | Unreal Engine platform expertise | UE architecture decisions, plugin selection, Unreal best-practice review |
| agents/unity-specialist.md | 3 - Lead / Domain Owner | Unity platform expertise | Unity architecture decisions, package selection, Unity best-practice review |
| agents/godot-specialist.md | 3 - Lead / Domain Owner | Godot platform expertise | Godot architecture decisions, addon selection, Godot best-practice review |
| agents/community-manager.md | 3 - Lead / Domain Owner | Community engagement and feedback | Community communications, player feedback triage, social channel management |
| agents/systems-designer.md | 4 - Specialist | Game systems and mechanics design | Rules systems, combat mechanics, progression systems, skill trees, economy rules |
| agents/level-designer.md | 4 - Specialist | Level and environment design | Map layout, encounter placement, environmental storytelling, pacing |
| agents/economy-designer.md | 4 - Specialist | In-game economy and monetisation design | Currency sinks and sources, drop rates, store pricing, economy balance |
| agents/technical-artist.md | 4 - Specialist | Art pipeline and technical art | Shaders, VFX, art tool pipeline, performance-friendly asset setup |
| agents/ux-designer.md | 4 - Specialist | User experience and interface design | UI layout, player onboarding flows, HUD design, accessibility UX |
| agents/writer.md | 4 - Specialist | Game writing and dialogue | Quest text, NPC dialogue, item descriptions, cutscene scripts |
| agents/world-builder.md | 4 - Specialist | World lore and setting construction | Lore documents, world history, faction design, setting consistency |
| agents/sound-designer.md | 4 - Specialist | Sound effects and audio implementation | SFX creation, audio events, mix guidelines, audio asset integration |
| agents/event-designer.md | 4 - Specialist | Live event design and scheduling | Limited-time event structure, cadence planning, reward beats, event rule design |
| agents/retention-designer.md | 4 - Specialist | Retention and engagement design | Daily and weekly loops, return-player incentives, progression hooks, long-term engagement tuning |
| agents/gameplay-programmer.md | 4 - Specialist | Gameplay systems implementation | Feature implementation, player controller, game rules code, gameplay bugs |
| agents/engine-programmer.md | 4 - Specialist | Engine-level systems and core tech | Rendering pipeline, engine extensions, core systems, performance-critical code |
| agents/ai-programmer.md | 4 - Specialist | AI and behaviour programming | NPC AI, pathfinding, behaviour trees, decision systems |
| agents/network-programmer.md | 4 - Specialist | Networking and multiplayer implementation | Network architecture, replication, latency handling, multiplayer bugs |
| agents/tools-programmer.md | 4 - Specialist | Internal tooling and editor extensions | Editor tools, pipeline automation, developer-facing utilities |
| agents/ui-programmer.md | 4 - Specialist | UI and HUD implementation | UI widget implementation, UI bindings, HUD logic, menu systems |
| agents/qa-tester.md | 4 - Specialist | Manual and exploratory testing | Test case execution, bug reporting, regression testing, edge-case exploration |
| agents/release-operations-specialist.md | 4 - Specialist | Release package preparation and execution support | Release checklists, submission packages, rollout preparation, release rehearsal support |
| agents/security-analyst.md | 4 - Specialist | Operational security analysis and validation | Vulnerability validation, security test execution, dependency risk review, incident triage support |
| agents/localization-specialist.md | 4 - Specialist | Translation and locale adaptation execution | Text localisation, cultural adaptation, glossary enforcement, translated asset updates |
| agents/locale-qa-specialist.md | 4 - Specialist | Locale-specific quality assurance | Linguistic QA, truncation checks, font coverage, locale regressions |
| agents/prototype-programmer.md | 4 - Specialist | Rapid prototype implementation | Fast mechanic spikes, prototype scaffolding, throwaway proof-of-concept code |
| agents/accessibility-tester.md | 4 - Specialist | Accessibility validation and usability checks | Assistive feature testing, contrast and readability checks, control remapping validation |
| agents/performance-analyst.md | 4 - Specialist | Performance profiling and optimisation | CPU/GPU profiling, memory analysis, frame-rate budgets, performance regressions |
| agents/devops-engineer.md | 4 - Specialist | CI/CD, infrastructure, and build systems | Build pipelines, deployment automation, infrastructure provisioning, monitoring |
| agents/analytics-engineer.md | 4 - Specialist | Data and telemetry engineering | Analytics pipelines, event tracking, dashboards, data-driven insight support |
| agents/community-moderator.md | 4 - Specialist | Day-to-day community moderation | Discord and forum moderation, rule enforcement, issue escalation, sentiment monitoring |
| agents/social-media-specialist.md | 4 - Specialist | Social channel publishing and campaign execution | Social post scheduling, campaign asset coordination, announcement rollouts, channel optimisation |
| agents/ue-gas-specialist.md | 4 - Specialist | Unreal Gameplay Ability System | GAS architecture, ability design, attribute sets, gameplay effects in Unreal |
| agents/ue-blueprint-specialist.md | 4 - Specialist | Unreal Blueprint scripting | Blueprint logic, visual scripting patterns, Blueprint optimisation |
| agents/ue-replication-specialist.md | 4 - Specialist | Unreal network replication | Actor replication, RPCs, replication graphs, Unreal multiplayer architecture |
| agents/ue-umg-specialist.md | 4 - Specialist | Unreal UMG and UI | UMG widget design, data bindings, Common UI, HUD implementation in Unreal |
| agents/unity-dots-specialist.md | 4 - Specialist | Unity DOTS and ECS | Entities, components, systems, Burst and Jobs integration, DOTS performance patterns |
| agents/unity-shader-specialist.md | 4 - Specialist | Unity shaders and visual effects | ShaderGraph, HLSL, URP and HDRP custom shaders, VFX Graph |
| agents/unity-addressables-specialist.md | 4 - Specialist | Unity Addressables and asset management | Addressable asset setup, remote content delivery, memory management in Unity |
| agents/unity-ui-specialist.md | 4 - Specialist | Unity UI Toolkit and uGUI | UI Toolkit layouts, USS, uGUI canvas setup, Unity HUD implementation |
| agents/godot-gdscript-specialist.md | 4 - Specialist | Godot GDScript programming | GDScript patterns, scene and signal architecture, GDScript performance tips |
| agents/godot-shader-specialist.md | 4 - Specialist | Godot shaders and visual effects | Godot shader language, VisualShader, post-processing, material customisation |
| agents/godot-gdextension-specialist.md | 4 - Specialist | Godot GDExtension and native modules | GDExtension bindings, C++ Godot modules, performance-critical native code |

Note: This table reflects the intended hierarchy. Entries listed here are planned agents -
they may not yet have a corresponding file in `agents/`. As agents are created and added to
`agents/`, this table must be kept in sync. Only agents with an existing file in `agents/`
are currently operative. Any agent not listed here is not officially part of the coordination
structure.

---

## Delegation Map

This section documents the allowed delegation relationships in the hierarchy. A delegating
agent assigns work to a subordinate; the subordinate reports results back up. Cross-team
coordination paths are permitted where explicitly listed below. Cross-team paths are
coordination relationships, not hierarchical delegation; they do not carry the same
authority as a direct-parent assignment.

### Level 2 -> Level 3 delegations

| Delegating Agent | May delegate to |
|---|---|
| creative-director | game-designer, art-director, narrative-director, audio-director, live-ops-designer |
| technical-director | lead-programmer, qa-lead, release-manager, security-engineer, localization-lead, prototyper, accessibility-specialist, performance-lead, platform-lead, data-lead, unreal-specialist, unity-specialist, godot-specialist |
| producer | community-manager |

### Level 3 -> Level 4 delegations

| Delegating Agent | May delegate to |
|---|---|
| game-designer | systems-designer, level-designer, economy-designer |
| art-director | technical-artist, ux-designer |
| narrative-director | writer, world-builder |
| audio-director | sound-designer |
| live-ops-designer | event-designer, retention-designer |
| lead-programmer | gameplay-programmer, engine-programmer, ai-programmer, network-programmer, tools-programmer, ui-programmer |
| qa-lead | qa-tester |
| release-manager | release-operations-specialist |
| security-engineer | security-analyst |
| localization-lead | localization-specialist, locale-qa-specialist |
| prototyper | prototype-programmer |
| accessibility-specialist | accessibility-tester |
| performance-lead | performance-analyst |
| platform-lead | devops-engineer |
| data-lead | analytics-engineer |
| community-manager | community-moderator, social-media-specialist |
| unreal-specialist | ue-gas-specialist, ue-blueprint-specialist, ue-replication-specialist, ue-umg-specialist |
| unity-specialist | unity-dots-specialist, unity-shader-specialist, unity-addressables-specialist, unity-ui-specialist |
| godot-specialist | godot-gdscript-specialist, godot-shader-specialist, godot-gdextension-specialist |

### Cross-team coordination paths

These paths are explicitly documented because the initiating agent's domain spans the
receiving agent's area of concern. They cover both cross-chain relationships and same-chain
peer relationships where the dependency is non-obvious. These are coordination paths, not
hierarchical delegation: the initiating agent may request work or sign-off from the receiving
agent, but does not hold authority over that agent's priorities or direction.

| Initiating Agent | Coordinates with | Reason |
|---|---|---|
| release-manager | platform-lead | Release manager owns deployment gates; platform-lead owns the build and deployment platform |
| release-manager | devops-engineer | Release manager depends on the execution details of the delivery pipeline |
| release-manager | qa-lead | Release go/no-go decisions require QA readiness confirmation |
| live-ops-designer | economy-designer | Live events require economy balance adjustments |
| live-ops-designer | community-manager | Live content rollouts require community communication coordination |
| live-ops-designer | data-lead | Live operations are planned against telemetry priorities and KPI definitions |
| live-ops-designer | analytics-engineer | Live operations are data-driven; analytics-engineer provides telemetry support |

When a cross-team initiating agent and the receiving agent's direct parent give conflicting
instructions, the direct parent's instructions take precedence. The cross-team initiating
agent must escalate the conflict to its own direct parent rather than issuing instructions
that override the receiving agent's chain of command.
