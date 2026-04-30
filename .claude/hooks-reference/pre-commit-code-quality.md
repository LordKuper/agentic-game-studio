# Hook: pre-commit-code-quality

## Trigger

Runs before commit touching `Assets/Scripts/`.

## Purpose

Enforce code standards pre-VCS. Catch style violations, missing docs, complex methods, hardcoded values that should be data-driven.

## Implementation

```bash
#!/bin/bash
# Pre-commit hook: Code quality checks
# Adapt the specific checks to your language and tooling

CODE_FILES=$(git diff --cached --name-only --diff-filter=ACM | grep -E '^Assets/Scripts/')

EXIT_CODE=0

if [ -n "$CODE_FILES" ]; then
    for file in $CODE_FILES; do
        # Check for hardcoded magic numbers in gameplay code
        if [[ "$file" == Assets/Scripts/Gameplay/* ]]; then
            # Look for numeric literals that are likely balance values
            # Adjust the pattern for your language
            if grep -nE '(damage|health|speed|rate|chance|cost|duration)[[:space:]]*[:=][[:space:]]*[0-9]+' "$file"; then
                echo "WARNING: $file may contain hardcoded gameplay values. Use data files."
                # Warning only, not blocking
            fi
        fi

        # Check for TODO/FIXME without assignee
        if grep -nE '(TODO|FIXME|HACK)[^(]' "$file"; then
            echo "WARNING: $file has TODO/FIXME without owner tag. Use TODO(name) format."
        fi

        # Run C# linter (Unity is the project's only engine)
        # dotnet format --verify-no-changes --include "$file" || EXIT_CODE=1
    done

    # Run unit tests for modified systems
    # Uncomment and adapt for your test framework
    # python -m pytest tests/unit/ -x --quiet || EXIT_CODE=1
fi

exit $EXIT_CODE
```

## Agent Integration

On fail:
1. Style violations: auto-fix via formatter or call `lead-programmer`.
2. Hardcoded values: call `gameplay-programmer` to externalize.
3. Test failures: call `qa-lead` to diagnose, `gameplay-programmer` to fix.
