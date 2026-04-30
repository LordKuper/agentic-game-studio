# Active Hooks

Hooks are configured in `.claude/settings.json` and fire automatically:

| Hook | Event | Trigger | Action |
| ---- | ----- | ------- | ------ |
| `validate-commit.sh` | PreToolUse (Bash) | `git commit` commands | Validates design doc sections, JSON data files, hardcoded values, TODO format |
| `validate-push.sh` | PreToolUse (Bash) | `git push` commands | Warns on pushes to protected branches (develop/main) |
| `validate-assets.sh` | PostToolUse (Write/Edit) | Asset file changes | Checks naming conventions and JSON validity for files in `assets/` |
| `session-start.sh` | SessionStart | Session begins | Loads sprint context, milestone, git activity; previews `.ags/project/state.md` if present |
| `detect-gaps.sh` | SessionStart | Session begins | Detects fresh projects (suggests `/ags-start`) and missing documentation when code exists, suggests `/reverse-document` or `/project-stage-detect` |
| `pre-compact.sh` | PreCompact | Context compression | Dumps active session state (`.ags/project/state.md`, modified files, WIP design docs) into conversation before compaction so it survives summarization |
| `post-compact.sh` | PostCompact | After compaction | Reminds Claude to restore state from `.ags/project/state.md` |
| `notify.sh` | Notification | Notification event | Shows Windows toast notification via PowerShell |
| `session-stop.sh` | Stop | Session ends | Summarizes accomplishments and updates session log |
| `log-agent.sh` | SubagentStart | Agent spawned | Audit trail start вЂ” logs subagent invocation with timestamp |
| `log-agent-stop.sh` | SubagentStop | Agent stops | Audit trail stop вЂ” completes subagent record |
| `validate-skill-change.sh` | PostToolUse (Write/Edit) | Skill file changes | Advises running `/skill-test` after any `.claude/skills/` file is written or edited |

Hook reference documentation: `.claude/hooks-reference/`
Hook input schema documentation: `.claude/hooks-reference/hook-input-schemas.md`
