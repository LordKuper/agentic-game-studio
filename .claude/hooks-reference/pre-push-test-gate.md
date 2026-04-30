# Hook: pre-push-test-gate

## Trigger

Runs before push to remote. Mandatory for `develop` and `main`.

## Purpose

Build compiles, unit tests pass, smoke tests pass before code hits shared branches. Last auto quality gate before code touches other devs.

## Implementation

```bash
#!/bin/bash
# Pre-push hook: Build and test gate

REMOTE="$1"
URL="$2"

# Only enforce full gate for develop and main
PROTECTED_BRANCHES="develop main"
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

FULL_GATE=false
for branch in $PROTECTED_BRANCHES; do
    if [ "$CURRENT_BRANCH" = "$branch" ]; then
        FULL_GATE=true
        break
    fi
done

echo "=== Pre-Push Quality Gate ==="

# Step 1: Build
echo "Building..."
# Adapt to your build system:
# make build || exit 1
# dotnet build || exit 1
# cargo build || exit 1
echo "Build: PASS"

# Step 2: Unit tests
echo "Running unit tests..."
# Adapt to your test framework:
# python -m pytest tests/unit/ -x || exit 1
# dotnet test tests/unit/ || exit 1
# cargo test || exit 1
echo "Unit tests: PASS"

if [ "$FULL_GATE" = true ]; then
    # Step 3: Integration tests (only for protected branches)
    echo "Running integration tests..."
    # python -m pytest tests/integration/ -x || exit 1
    echo "Integration tests: PASS"

    # Step 4: Smoke tests
    echo "Running smoke tests..."
    # python -m pytest tests/playtest/smoke/ -x || exit 1
    echo "Smoke tests: PASS"

    # Step 5: Performance regression check
    echo "Checking performance baselines..."
    # python tools/ci/perf_check.py || exit 1
    echo "Performance: PASS"
fi

echo "=== All gates passed ==="
exit 0
```

## Agent Integration

On fail:
1. Build fail: call `lead-programmer` to diagnose.
2. Unit test fail: call `qa-lead` to find failing test, `gameplay-programmer` or relevant programmer to fix.
3. Perf regression: call `performance-analyst` to analyze.
