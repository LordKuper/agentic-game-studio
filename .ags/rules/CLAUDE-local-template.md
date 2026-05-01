# CLAUDE.local.md Template

Copy to project root as `CLAUDE.local.md` for personal overrides. Gitignored. Not committed.

```markdown
# Personal Preferences

## Model Preferences
- Prefer Opus for complex design tasks
- Use Haiku for quick lookups and simple edits

## Workflow Preferences
- Always run tests after code changes
- Compact context proactively at 60% usage
- Use /clear between unrelated tasks

## Local Environment
- Python command: python (or py / python3)
- Shell: Git Bash on Windows
- IDE: VS Code with Claude Code extension

## Communication Style
- Keep responses concise
- Show file paths in all code references
- Explain architectural decisions briefly

## Personal Shortcuts
- "review" → run /ags-code-review on last changed files
- "status" → show git status + sprint progress
```

## Setup

1. Copy template: `cp .ags/rules/CLAUDE-local-template.md CLAUDE.local.md`
2. Edit to match preferences.
3. Verify `CLAUDE.local.md` in `.gitignore` (Claude Code reads from project root).
