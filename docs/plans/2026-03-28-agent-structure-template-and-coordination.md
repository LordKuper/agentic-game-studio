# Agent Structure: Template and Coordination

## Overview

Create the foundational agent infrastructure: `agent-template.md` in the project root as the authoritative template for all future agent definitions, and `agent-coordination.md` defining the 4-level hierarchy, escalation rules, and a consolidated responsibilities reference so it is immediately clear which agents to involve for any given topic. Individual agent definition files are out of scope for this plan.

## Context

- Files involved:
  - Create: `agent-template.md` (root)
  - Create: `agent-coordination.md` (root)
  - Create: `agents/` directory (empty, placeholder for future agent files)
- Related patterns: Project follows SOLID, KISS, YAGNI; existing AGENTS.md at root
- Dependencies: None

## Development Approach

- Testing approach: N/A — these are documentation/configuration markdown files, no code
- Complete each task fully before moving to the next

## Implementation Steps

### Task 1: Create agent-template.md

**Files:**
- Create: `agent-template.md`

- [x] Create agent-template.md with the following sections:
  - `name:` — agent's name
  - `description:` — what the agent does (brief, one-paragraph summary)
  - `must_not:` — explicit list of behaviours, actions, and decisions this agent must never perform or initiate (e.g. "must not make final budget decisions", "must not override another agent's domain")
  - `models:` — ordered list of suitable AI models (highest priority first), values from: claude-opus-4-6, claude-sonnet-4-6, claude-haiku-4-5
  - `max_iterations:` — maximum number of execution iterations per session (integer)
- [x] Include inline comments/guidance in the template explaining what to fill in each field
- [x] Note in the template that the agent's responsibilities are documented in agent-coordination.md, not in the individual agent file

### Task 2: Create agent-coordination.md

**Files:**
- Create: `agent-coordination.md`

- [x] Document the 4-level hierarchy:
  - Level 1: CEO — human user, ultimate decision-maker, not an AI agent
  - Level 2: C-Level Agents — AI agents responsible for strategic domains (e.g. CTO, CCO, CPO)
  - Level 3: Team-lead Agents — AI agents managing specific functional teams
  - Level 4: Specialist Agents — AI agents executing focused domain tasks
- [x] Include a Mermaid hierarchy diagram showing the 4 levels and their relationships
- [x] Define escalation rules: when agents at the same level disagree, the conflict is escalated to the most appropriate parent agent on the level above, chosen based on the subject matter of the disagreement
- [x] Document communication flow: top-down task assignment, bottom-up result reporting, lateral peer collaboration within a level
- [x] Add an "Agent Responsibilities Reference" section listing each agent (by filename) with its area of responsibility and the kinds of questions/topics it should be involved in — this is the primary lookup table for deciding which agents to engage
- [x] Note that concrete agent definitions live in `agents/<agent-name>.md` and must conform to `agent-template.md`

### Task 3: Create agents/ directory placeholder

**Files:**
- Create: `agents/.gitkeep`

- [ ] Create `agents/.gitkeep` so the empty directory is tracked in git

### Task 4: Verify and finalize

- [ ] Confirm agent-template.md includes all required fields including must_not
- [ ] Confirm agent-template.md does not duplicate responsibility data already in agent-coordination.md
- [ ] Confirm agent-coordination.md Agent Responsibilities Reference covers all hierarchy levels and is sufficient to determine agent selection for any topic
- [ ] Move this plan to `docs/plans/completed/`
