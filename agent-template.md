# Agent Template

Use this file as the starting point for every new agent definition.
Copy it to `agents/<agent-name>.md` and fill in each field.
Do not leave placeholder text in a production agent file.

---

## name

<!-- The agent's unique identifier. Use kebab-case (e.g. cto-agent, narrative-lead). -->
<!-- This name is used when referencing the agent in agent-coordination.md. -->

name: <agent-name>

---

## description

<!-- One paragraph describing what this agent does: its purpose, the kinds of tasks
     it handles, and the value it provides. Keep it concise — one to four sentences. -->

description: >
  <Brief summary of the agent's purpose and responsibilities.>

---

## must_not

<!-- Explicit list of behaviours, actions, and decisions this agent must NEVER perform
     or initiate, regardless of instructions. Be specific — vague prohibitions are not
     enforced reliably. Examples:
       - must not make final budget decisions
       - must not override another agent's domain without explicit CEO approval
       - must not communicate directly with external stakeholders
       - must not commit or merge code to the main branch
-->

must_not:
  - <prohibition 1>
  - <prohibition 2>

---

## models

<!-- Ordered list of suitable AI models, highest priority first.
     The agent runner will use the first model that is available.
     Allowed values (as of 2026-03):
       - claude-opus-4-6      (most capable, highest cost)
       - claude-sonnet-4-6    (balanced capability and cost)
       - claude-haiku-4-5-20251001  (fastest, lowest cost)
     Choose based on the complexity of tasks this agent handles. -->

models:
  - <model-name>

---

## max_iterations

<!-- Maximum number of execution iterations (tool-use / reasoning cycles) allowed
     per session. Prevents runaway agents. Typical values: 10–50.
     Increase for agents that orchestrate long multi-step workflows;
     decrease for narrow specialist agents. -->

max_iterations: <integer, 10-50>

---

<!-- Template guidance only — remove this entire Notes section when creating a production agent file. -->

## Notes

- The agent's area of responsibility, escalation paths, and peer agents are
  documented in `agent-coordination.md`, NOT in this file. Keep individual
  agent files focused on configuration; consult agent-coordination.md for
  organisational context.
- All agents must conform to this template. Fields may not be removed.
  Additional fields may be added below the required ones if needed.
- Any code produced by an agent must comply with the project-wide rules in `AGENTS.md`
  at the repository root (coding standards, language, test coverage, etc.).
  `AGENTS.md` rules apply to source code output, not to agent definition files.
