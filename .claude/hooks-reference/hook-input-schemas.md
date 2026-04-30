# Hook Input/Output Schemas

JSON payloads each Claude Code hook gets on stdin per event.

## PreToolUse

Fires before tool runs. **Allow** (exit 0) or **block** (exit 2).

### PreToolUse: Bash

```json
{
  "tool_name": "Bash",
  "tool_input": {
    "command": "git commit -m 'feat: add player health system'",
    "description": "Commit changes with message",
    "timeout": 120000
  }
}
```

### PreToolUse: Write

```json
{
  "tool_name": "Write",
  "tool_input": {
    "file_path": "Assets/Scripts/Gameplay/Health.cs",
    "content": "using UnityEngine;\npublic class Health : MonoBehaviour { ... }"
  }
}
```

### PreToolUse: Edit

```json
{
  "tool_name": "Edit",
  "tool_input": {
    "file_path": "Assets/Scripts/Gameplay/Health.cs",
    "old_string": "private int health = 100;",
    "new_string": "[SerializeField] private int health = 100;"
  }
}
```

### PreToolUse: Read

```json
{
  "tool_name": "Read",
  "tool_input": {
    "file_path": "Assets/Scripts/Gameplay/Health.cs"
  }
}
```

## PostToolUse

Fires after tool done. **Cannot block** (exit code ignored). Stderr shown as warnings.

### PostToolUse: Write

```json
{
  "tool_name": "Write",
  "tool_input": {
    "file_path": "assets/data/enemy_stats.json",
    "content": "{\"goblin\": {\"health\": 50}}"
  },
  "tool_output": "File written successfully"
}
```

### PostToolUse: Edit

```json
{
  "tool_name": "Edit",
  "tool_input": {
    "file_path": "assets/data/enemy_stats.json",
    "old_string": "\"health\": 50",
    "new_string": "\"health\": 75"
  },
  "tool_output": "File edited successfully"
}
```

## SubagentStart

Fires when subagent spawned via Task tool.

```json
{
  "agent_name": "game-designer",
  "model": "sonnet",
  "description": "Design the combat healing mechanic"
}
```

## SessionStart

Fires when session begins. **No stdin** — stdout shown to Claude as context.

## PreCompact

Fires before context compression. **No stdin** — saves state pre-compress.

## Stop

Fires when session ends. **No stdin** — cleanup and logging.

## Exit Code Reference

| Exit Code | Meaning | Applicable Events |
|-----------|---------|-------------------|
| 0 | Allow / Success | All events |
| 2 | Block (stderr shown to Claude) | PreToolUse only |
| Other | Error, tool proceeds | All events |

## Notes

- Hooks get JSON on **stdin** (pipe). Use `INPUT=$(cat)` to capture.
- Parse with `jq` if available, fall back to `grep` for cross-platform.
- Windows: `grep -P` often missing. Use `grep -E` instead.
- Windows path sep `\`. Normalize via `sed 's|\\|/|g'` when comparing paths.
