# agentic-game-studio
An automated workflow for designing, developing and testing videogames.

## Agent Infrastructure

This project uses a structured multi-agent AI workflow. Two root-level files govern it:

- `agent-coordination.md` - 4-level agent hierarchy, escalation rules, communication flow, and the Agent Responsibilities Reference (primary lookup for deciding which agent to involve in any task).
- `agent-template.md` - required template for every agent definition. Copy it to `agents/<agent-name>.md` and fill in all fields before adding a new agent.

Agent definition files live in `agents/`.
