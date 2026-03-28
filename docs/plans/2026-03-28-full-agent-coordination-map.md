---
# Full Agent Coordination Map — All Levels and Engine Specialists

## Overview

Replace the current 10-agent placeholder in `agent-coordination.md` with a complete coordination map covering all game development zones for a mid-scale studio. Includes general specialists (40+ agents across 4 levels), engine-specific specialists for Unity, Godot, and Unreal Engine, a full delegation/interaction map, and updated escalation rules with recursive hierarchical conflict resolution. No agent definition files are created — only `agent-coordination.md` is modified. The quick-reference routing examples section is not touched.

## Context

- Files involved:
  - Modify: `agent-coordination.md`
- Related patterns: 4-level hierarchy, escalation rules, and communication flow already defined in `agent-coordination.md` — only the diagram, agent-list sections, delegation map, and escalation rules are replaced/updated
- Dependencies: None

## Development Approach

- Documentation-only change — no code, no tests
- Complete each task fully before moving to the next

## Implementation Steps

### Task 1: Replace Mermaid hierarchy diagram

**Files:**
- Modify: `agent-coordination.md`

Replace the current example diagram (which shows placeholder agents) with a diagram reflecting the full agent roster. The diagram must show:

- Level 2 (C-Level): creative-director, technical-director, producer
- Level 3 (under creative-director): game-designer, art-director, narrative-director, audio-director, live-ops-designer
- Level 3 (under technical-director): lead-programmer, qa-lead, release-manager, security-engineer, localization-lead, prototyper, accessibility-specialist, unreal-specialist, unity-specialist, godot-specialist
- Level 3 (under producer): community-manager
- Level 4 (under game-designer): systems-designer, level-designer, economy-designer
- Level 4 (under art-director): technical-artist, ux-designer
- Level 4 (under narrative-director): writer, world-builder
- Level 4 (under audio-director): sound-designer
- Level 4 (under lead-programmer): gameplay-programmer, engine-programmer, ai-programmer, network-programmer, tools-programmer, ui-programmer
- Level 4 (under qa-lead): qa-tester
- Level 4 (under technical-director direct): performance-analyst, devops-engineer, analytics-engineer
- Level 4 (under unreal-specialist): ue-gas-specialist, ue-blueprint-specialist, ue-replication-specialist, ue-umg-specialist
- Level 4 (under unity-specialist): unity-dots-specialist, unity-shader-specialist, unity-addressables-specialist, unity-ui-specialist
- Level 4 (under godot-specialist): godot-gdscript-specialist, godot-shader-specialist, godot-gdextension-specialist

- [x] Replace old Mermaid diagram with updated one showing all agents
- [x] Add a legend block beneath the diagram listing level color/shape conventions

### Task 2: Replace Agent Responsibilities Reference table

**Files:**
- Modify: `agent-coordination.md`

Replace the current 10-row table with a full table. Each row: agent file path, level, area of responsibility, "involve for" description.

- [x] Add all Level 2 agents: creative-director, technical-director, producer
- [x] Add all Level 3 general agents: game-designer, art-director, narrative-director, audio-director, lead-programmer, qa-lead, release-manager, security-engineer, localization-lead, prototyper, accessibility-specialist, live-ops-designer, community-manager
- [x] Add all Level 3 engine-lead agents: unreal-specialist, unity-specialist, godot-specialist
- [x] Add all Level 4 general agents: systems-designer, level-designer, economy-designer, technical-artist, ux-designer, writer, world-builder, sound-designer, gameplay-programmer, engine-programmer, ai-programmer, network-programmer, tools-programmer, ui-programmer, qa-tester, performance-analyst, devops-engineer, analytics-engineer
- [x] Add all Level 4 Unreal sub-specialists: ue-gas-specialist, ue-blueprint-specialist, ue-replication-specialist, ue-umg-specialist
- [x] Add all Level 4 Unity sub-specialists: unity-dots-specialist, unity-shader-specialist, unity-addressables-specialist, unity-ui-specialist
- [x] Add all Level 4 Godot sub-specialists: godot-gdscript-specialist, godot-shader-specialist, godot-gdextension-specialist

### Task 3: Add Delegation Map section

**Files:**
- Modify: `agent-coordination.md`

Add a new "Delegation Map" section after the Agent Responsibilities Reference. This section documents the allowed from→to delegation relationships as a table.

- [x] Add delegation table covering all Level 2→3 paths
- [x] Add delegation table covering all Level 3→4 paths
- [x] Include engine specialist delegation: each [engine]-specialist → its sub-specialists
- [x] Include cross-team delegations (e.g. release-manager → devops-engineer + qa-lead; live-ops-designer → economy-designer + community-manager + analytics-engineer)

### Task 4: Update Escalation Rules — conflict resolution chain

**Files:**
- Modify: `agent-coordination.md`

Replace the current **Disagreement** rule with a recursive hierarchical conflict resolution rule:

New rule:
1. When agents at the same level disagree and cannot reach a decision, each conflicting agent identifies its direct parent in the hierarchy.
2. Those parent agents attempt to resolve the conflict between themselves, following the same lateral negotiation process.
3. If the parent agents cannot agree, the conflict is escalated one level further up the chain: each parent identifies its own parent, and those agents attempt resolution.
4. This process repeats level by level until either the conflict is resolved or it reaches the CEO, who makes the final decision.

The **Blocked** rule and the closing note are not changed.

- [ ] Replace the Disagreement bullet list in the Escalation Rules section with the new recursive chain rule
- [ ] Ensure the new rule references the hierarchical subordination structure defined in the diagram (i.e., "direct parent" means the agent's parent as shown in the hierarchy)

### Task 5: Verify acceptance criteria

- [ ] Confirm all agents from the Donchitos reference are present in the table
- [ ] Confirm Mermaid diagram renders without syntax errors
- [ ] Confirm no placeholder agents remain in the document
- [ ] Confirm Escalation Rules section reflects the new recursive conflict resolution chain
- [ ] Move this plan to `docs/plans/completed/`
