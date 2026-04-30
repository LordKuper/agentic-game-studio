#!/bin/bash
# Claude Code SessionStart hook: Load project context at session start
# Input schema (SessionStart): No stdin input

echo "= Agentic Game Studio ="

# Current branch
BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null)
if [ -n "$BRANCH" ]; then
    echo "Branch: $BRANCH"

    # Recent commits
    echo ""
    echo "Recent commits:"
    git log --oneline -5 2>/dev/null | while read -r line; do
        echo "  $line"
    done
fi

# --- Active session state ---
STATE_FILE=".ags/project/state.md"

if [ -f "$STATE_FILE" ]; then
    echo ""
    echo "= ACTIVE STATE ="
    echo "File: $STATE_FILE"
    head -20 "$STATE_FILE" 2>/dev/null
    echo "= END STATE PREVIEW ="
fi

echo "==================================="
exit 0
