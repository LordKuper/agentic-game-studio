# Setup Requirements

Template needs tools for full functionality. Hooks fail gracefully if missing — nothing breaks, lose validation features.

## Required

| Tool | Purpose | Install |
| ---- | ---- | ---- |
| **Git** | Version control, branch management | [git-scm.com](https://git-scm.com/) |
| **Claude Code** | AI agent CLI | `npm install -g @anthropic-ai/claude-code` |

## Recommended

| Tool | Used By | Purpose | Install |
| ---- | ---- | ---- | ---- |
| **jq** | Hooks (4 of 8) | JSON parsing in commit/push/asset/agent hooks | See below |
| **Python 3** | Hooks (2 of 8) | JSON validation for data files | [python.org](https://www.python.org/) |
| **Bash** | All hooks | Shell script execution | Included with Git for Windows |

### Installing jq

**Windows**:
```
winget install jqlang.jq
choco install jq
scoop install jq
```

**macOS**:
```
brew install jq
```

**Linux**:
```
sudo apt install jq     # Debian/Ubuntu
sudo dnf install jq     # Fedora
sudo pacman -S jq       # Arch
```

## Platform Notes

### Windows
- Git for Windows includes **Git Bash** — provides `bash` used by all hooks in `settings.json`.
- Ensure Git Bash on PATH (default if installed via Git installer).
- Hooks use `bash .claude/hooks/[name].sh` — works on Windows; Claude Code invokes via shell that finds `bash.exe`.

### macOS / Linux
- Bash native.
- Install `jq` via package manager for full hook support.

## Verifying Setup

```bash
git --version          # git version
bash --version         # bash version
jq --version           # jq version (optional)
python3 --version      # python version (optional)
```

## Without Optional Tools

| Missing | Effect |
| ---- | ---- |
| **jq** | Commit validation, push protection, asset validation, agent audit hooks silently skip checks. Commits/pushes still work. |
| **Python 3** | JSON data file validation skipped. Invalid JSON can commit without warning. |
| **Both** | All hooks exit 0 with no validation. No safety nets. |

## Recommended IDE

Works with any editor. Optimized for:
- **VS Code** with Claude Code extension
- **Cursor** (Claude Code compatible)
- Terminal Claude Code CLI
