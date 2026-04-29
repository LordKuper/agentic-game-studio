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

# --- Active session state recovery ---
STATE_FILE=".ags/project/session-state.md"
if [ -f "$STATE_FILE" ]; then
    echo ""
    echo "= SESSION STATE DETECTED ="
    echo "A previous session left state at: $STATE_FILE"
    echo "Read this file to recover context and continue where you left off."
    echo ""
    echo "Quick summary:"
    head -20 "$STATE_FILE" 2>/dev/null
    TOTAL_LINES=$(wc -l < "$STATE_FILE" 2>/dev/null)
    if [ "$TOTAL_LINES" -gt 20 ]; then
        echo "  ... ($TOTAL_LINES total lines — read the full file to continue)"
    fi
    echo "= END SESSION STATE PREVIEW ="
fi

echo "==================================="
exit 0