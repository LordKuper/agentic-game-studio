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

# --- Sessions overview ---
SESSIONS_DIR=".ags/project/sessions"
CURRENT_FILE="$SESSIONS_DIR/.current"

if [ -d "$SESSIONS_DIR" ]; then
    # Active (pointer)
    if [ -f "$CURRENT_FILE" ]; then
        SLUG=$(head -1 "$CURRENT_FILE" 2>/dev/null | tr -d '[:space:]')
        ACTIVE_FILE="$SESSIONS_DIR/$SLUG.md"
        if [ -n "$SLUG" ] && [ -f "$ACTIVE_FILE" ]; then
            echo ""
            echo "= ACTIVE SESSION: $SLUG ="
            echo "File: $ACTIVE_FILE"
            head -10 "$ACTIVE_FILE" 2>/dev/null
            echo "= END ACTIVE PREVIEW ="
        fi
    fi

    # Other unfinished sessions (excluding active + archived/)
    UNFINISHED=$(find "$SESSIONS_DIR" -maxdepth 1 -name '*.md' -type f 2>/dev/null | sort)
    if [ -n "$UNFINISHED" ]; then
        OTHERS=""
        for f in $UNFINISHED; do
            base=$(basename "$f" .md)
            if [ "$base" != "$SLUG" ]; then
                OTHERS="$OTHERS$f\n"
            fi
        done
        if [ -n "$OTHERS" ]; then
            echo ""
            echo "= UNFINISHED SESSIONS ="
            printf "$OTHERS"
            echo "Run /ags-start to resume one or start a new session."
        fi
    fi
fi

echo "==================================="
exit 0
