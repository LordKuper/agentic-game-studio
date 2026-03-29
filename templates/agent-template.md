# Agent Template

Use this file as the starting point for every new agent definition.
Copy it to `agents/<agent-name>.md` and fill in each required field.
Do not leave placeholder text in a production agent file.

## Required Fields

| Field | Value |
| --- | --- |
| `name` | `<agent-name>` |
| `description` | `<Brief summary of the agent's purpose and responsibilities.>` |
| `must_not` | `- <prohibition 1><br>- <prohibition 2>` |
| `models` | `- <model-name 1><br>- <model-name 2>` |
| `max_iterations` | `<integer, 10-50>` |

---

<!-- Optional reference material below. Remove it from a production agent file if it is no longer useful. -->
## Optional Guidance

### Field Rules

- `name`: Unique agent identifier in kebab-case, for example `cto-agent` or `narrative-lead`.
- `description`: One concise paragraph describing the agent's purpose, the kinds of tasks it handles, and the value it provides.
- `must_not`: Explicit list of behaviors, actions, and decisions this agent must never perform or initiate. Be specific.
- `models`: Ordered list of suitable AI models, highest priority first. All models must have both Claude and ChatGPT models in the list. Technical roles should prioritise Claude models over ChatGPT and vice versa. The agent runner will use the first model that is available.
- `max_iterations`: Maximum number of execution iterations (tool-use / reasoning cycles) allowed per session. Typical values are `10-50`.

### Model Values

- Use versionless model names only. Do not include provider-specific version numbers.
- Allowed values are `chatgpt`, `claude-opus`, `claude-sonnet`, and `claude-haiku`.

<!-- Template guidance only - remove this entire Notes section when creating a production agent file. -->
### Notes

- The agent's area of responsibility, escalation paths, and peer agents are documented in `agent-coordination.md`, not in this file.
- Keep individual agent files focused on configuration. Use `agent-coordination.md` for organizational context.
- All agents must conform to this template. Required fields may not be removed.
- Additional optional sections may be added below the required fields when needed.
- Any code produced by an agent must comply with the project-wide rules in `AGENTS.md` at the repository root.
- `AGENTS.md` rules apply to source code output, not to agent definition files.
