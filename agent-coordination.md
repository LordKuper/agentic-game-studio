# Agent Coordination

This document defines the organisational structure for all AI agents in this project:
the 4-level hierarchy, escalation rules, communication flow, and the Agent Responsibilities
Reference. It is the authoritative source for deciding which agents to engage for any topic.

Concrete agent definitions live in `agents/<agent-name>.md` and must conform to `agent-template.md`.

---

## Hierarchy

The project uses a 4-level structure:

- Level 1 - CEO: The human user. Ultimate decision-maker. Not an AI agent.
- Level 2 - C-Level Agents: AI agents responsible for strategic domains (e.g. CTO, CCO, CPO).
- Level 3 - Team-Lead Agents: AI agents managing specific functional teams under a C-Level.
- Level 4 - Specialist Agents: AI agents executing focused, narrow domain tasks.

```mermaid
graph TD
    CEO["Level 1 — CEO (Human)"]

    CD["Level 2 — creative-director"]
    TD["Level 2 — technical-director"]
    PR["Level 2 — producer"]

    GD["Level 3 — game-designer"]
    AD["Level 3 — art-director"]
    ND["Level 3 — narrative-director"]
    AUD["Level 3 — audio-director"]
    LOD["Level 3 — live-ops-designer"]

    LP["Level 3 — lead-programmer"]
    QAL["Level 3 — qa-lead"]
    RM["Level 3 — release-manager"]
    SEC["Level 3 — security-engineer"]
    LL["Level 3 — localization-lead"]
    PT["Level 3 — prototyper"]
    AS["Level 3 — accessibility-specialist"]
    UE3["Level 3 — unreal-specialist"]
    UN3["Level 3 — unity-specialist"]
    GO3["Level 3 — godot-specialist"]

    CM["Level 3 — community-manager"]

    SYD["Level 4 — systems-designer"]
    LVD["Level 4 — level-designer"]
    ECD["Level 4 — economy-designer"]
    TA["Level 4 — technical-artist"]
    UXD["Level 4 — ux-designer"]
    WR["Level 4 — writer"]
    WB["Level 4 — world-builder"]
    SD["Level 4 — sound-designer"]

    GPP["Level 4 — gameplay-programmer"]
    EP["Level 4 — engine-programmer"]
    AIP["Level 4 — ai-programmer"]
    NP["Level 4 — network-programmer"]
    TP["Level 4 — tools-programmer"]
    UIP["Level 4 — ui-programmer"]
    QAT["Level 4 — qa-tester"]

    PA["Level 4 — performance-analyst"]
    DO["Level 4 — devops-engineer"]
    AE["Level 4 — analytics-engineer"]

    UEGAS["Level 4 — ue-gas-specialist"]
    UEBP["Level 4 — ue-blueprint-specialist"]
    UEREP["Level 4 — ue-replication-specialist"]
    UEUMG["Level 4 — ue-umg-specialist"]

    UDOTS["Level 4 — unity-dots-specialist"]
    USHD["Level 4 — unity-shader-specialist"]
    UADDR["Level 4 — unity-addressables-specialist"]
    UUISP["Level 4 — unity-ui-specialist"]

    GGDS["Level 4 — godot-gdscript-specialist"]
    GSHD["Level 4 — godot-shader-specialist"]
    GEXT["Level 4 — godot-gdextension-specialist"]

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
    TD --> AS
    TD --> UE3
    TD --> UN3
    TD --> GO3
    TD --> PA
    TD --> DO
    TD --> AE

    PR --> CM

    GD --> SYD
    GD --> LVD
    GD --> ECD

    AD --> TA
    AD --> UXD

    ND --> WR
    ND --> WB

    AUD --> SD

    LP --> GPP
    LP --> EP
    LP --> AIP
    LP --> NP
    LP --> TP
    LP --> UIP

    QAL --> QAT

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
```

**Diagram Legend**

- Level 1 (CEO / Human): single root node; ultimate decision-maker
- Level 2 (C-Level): strategic domain owners — creative-director, technical-director, producer
- Level 3 (Team-Lead): functional team leads and engine-lead specialists reporting to a C-Level
- Level 4 (Specialist): narrow-domain executors and engine sub-specialists reporting to a Team-Lead

---

## Escalation Rules

**Disagreement:** When agents at the same level disagree or cannot reach a decision independently:

1. Identify the subject matter of the disagreement.
2. Determine the most appropriate parent agent on the level above whose domain covers that subject.
3. Escalate the conflict to that parent agent for resolution.
4. If the disagreement spans multiple domains (no single parent covers it), escalate to the CEO.

**Blocked:** When an agent cannot proceed (missing permissions, outside its scope, lacks information):

1. Report the blocker upward to the direct parent agent with a clear description of what is needed.
2. Do not attempt to acquire permissions or expand scope unilaterally.
3. If the parent cannot resolve the blocker, it escalates further up the chain until it reaches the CEO.

Agents must not attempt to resolve cross-domain conflicts unilaterally. Escalation is not a
sign of failure - it is the correct procedure for maintaining clear ownership.

---

## Communication Flow

- Top-down: Task assignment flows from higher levels to lower levels.
  The CEO assigns work to C-Level agents; C-Level agents delegate to Team-Lead agents;
  Team-Lead agents direct Specialist agents.

- Bottom-up: Results, status updates, and blockers are reported upward.
  Specialists report to their Team-Lead; Team-Leads report to their C-Level; C-Levels report to the CEO.

- Lateral: Agents at the same level may collaborate directly as peers.
  Lateral collaboration does not require routing through a parent unless a conflict arises.
  Lateral communication should be transparent - outcomes are still reported upward.

---

## Agent Responsibilities Reference

This table is the primary lookup for deciding which agents to involve for a given topic.
Each row maps an agent to its area of responsibility and the types of questions or tasks
that belong to it.

| Agent File | Level | Area of Responsibility | Involve For |
|---|---|---|---|
| agents/cto-agent.md | 2 - C-Level | Technology strategy and engineering direction | Architecture decisions, tech stack choices, build/CI/CD strategy, engineering standards |
| agents/cco-agent.md | 2 - C-Level | Creative and narrative direction | Story direction, tone and voice, creative vision, narrative consistency |
| agents/cpo-agent.md | 2 - C-Level | Product and design direction | Feature prioritisation, player experience, UX decisions, product roadmap |
| agents/engineering-lead.md | 3 - Team-Lead | Engineering team management | Sprint planning for engineering, code review policy, team coordination across backend/QA |
| agents/narrative-lead.md | 3 - Team-Lead | Narrative team management | Writing pipeline, lore consistency, managing narrative writers and editors |
| agents/design-lead.md | 3 - Team-Lead | Game design team management | Level design oversight, game feel, balancing, player journey |
| agents/backend-engineer.md | 4 - Specialist | Backend and systems implementation | Feature implementation, bug fixes, database schema, server-side logic |
| agents/qa-engineer.md | 4 - Specialist | Quality assurance and testing | Test plans, bug triage, regression coverage, automated testing |
| agents/narrative-writer.md | 4 - Specialist | Writing and dialogue | Quest text, NPC dialogue, item descriptions, lore entries |
| agents/level-designer.md | 4 - Specialist | Level and environment design | Map layout, encounter placement, environmental storytelling, pacing |

Note: This table reflects the intended hierarchy. Entries marked here are planned agents -
they may not yet have a corresponding file in `agents/`. As agents are created and added to
`agents/`, this table must be kept in sync. Only agents with an existing file in `agents/`
are currently operative. Any agent not listed here is not officially part of the coordination
structure.

---

## Quick-Reference: Which Agent for Which Topic?

Use this as a fast triage guide. Find the topic area, then engage the listed agent.

If the recommended agent file does not exist yet in `agents/`, handle the topic directly
with the CEO (human user) until that agent is created.

- "Should we use Unity or Godot?" -> cto-agent (architecture/tech strategy)
- "The story feels tonally inconsistent" -> cco-agent (creative direction)
- "Players are dropping off at level 3" -> cpo-agent (product/player experience)
- "The save system is crashing" -> backend-engineer (implementation bug), escalate to engineering-lead if systemic
- "This quest dialogue doesn't match the lore" -> narrative-writer to fix, narrative-lead if it's a lore-consistency policy question
- "Level 7 is too hard" -> level-designer (balancing), escalate to design-lead if it's a systemic design question
- "We need a new test suite for combat" -> qa-engineer (test plan), engineering-lead for resourcing
