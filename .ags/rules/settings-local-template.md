# settings.local.json Template

Create `.claude/settings.local.json` for personal overrides. Do NOT commit. Add to `.gitignore`.

## Example settings.local.json

```json
{
  "permissions": {
    "allow": [
      "Bash(git *)",
      "Bash(npm *)",
      "Read",
      "Glob",
      "Grep"
    ],
    "deny": [
      "Bash(rm -rf *)",
      "Bash(git push --force *)"
    ]
  }
}
```

## Permission Modes

### During Development (Default)
**Normal mode** — Claude asks before most commands. Safest for production code.

### During Code Review
**Read-only** permissions — read and search, no modify.

## Customizing Hooks Locally

Add personal hooks in `settings.local.json` that extend (not override) project hooks. Example: notify on build complete.

```json
{
  "hooks": {
    "Stop": [
      {
        "matcher": "",
        "hooks": [
          {
            "type": "command",
            "command": "bash -c 'echo Session ended at $(date)'",
            "timeout": 5
          }
        ]
      }
    ]
  }
}
```
