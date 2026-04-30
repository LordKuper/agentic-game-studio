# Active Hooks

Configured in `.claude/settings.json`. Fire automatically.

| Hook | Event | Trigger | Action |
| ---- | ----- | ------- | ------ |
| `validate-commit.sh` | PreToolUse (Bash) | `git commit` | Validate design doc sections, JSON data files, hardcoded values, TODO format |
| `validate-push.sh` | PreToolUse (Bash) | `git push` | Warn on pushes to protected branches (develop/main) |
| `validate-assets.sh` | PostToolUse (Write/Edit) | Asset file changes | Check naming + JSON validity in `assets/` |
| `session-start.sh` | SessionStart | Session begins | Load sprint context, milestone, git activity; preview `.ags/project/state.md` |
| `detect-gaps.sh` | SessionStart | Session begins | Detect fresh projects (suggest `/ags-start`); missing docs when code exists → suggest `/reverse-document` or `/project-stage-detect` |
| `pre-compact.sh` | PreCompact | Context compression | Dump active state (`.ags/project/state.md`, modified files, WIP design docs) into conversation before compaction |
| `post-compact.sh` | PostCompact | After compaction | Remind Claude to restore state from `.ags/project/state.md` |
| `notify.sh` | Notification | Notification event | Show Windows toast via PowerShell |
| `session-stop.sh` | Stop | Session ends | Summarize accomplishments, update session log |
| `log-agent.sh` | SubagentStart | Agent spawned | Audit trail start — log invocation with timestamp |
| `log-agent-stop.sh` | SubagentStop | Agent stops | Audit trail stop — complete subagent record |
| `validate-skill-change.sh` | PostToolUse (Write/Edit) | Skill file changes | Advise running `/skill-test` after `.claude/skills/` write |

Hook reference docs: `.claude/hooks-reference/`
Hook input schemas: `.claude/hooks-reference/hook-input-schemas.md`
