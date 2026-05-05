# Stage

<!-- Single source of truth: current dev phase + active epic. Updated on phase transition or epic switch. -->

## Current

- **Phase**: <concept | production | polish | release>
- **Active epic**: <epic-id or "none">
- **Updated**: <YYYY-MM-DD>

## Phase definitions

| Phase | Entry criteria | Exit criteria |
|-------|---------------|---------------|
| concept | Project init | Game concept approved, systems-index drafted, foundational ADRs accepted |
| production | Concept gate passed | All in-scope systems implemented, feature-complete |
| polish | Feature-complete | Bug bar met, perf budgets met, content locked |
| release | Polish gate passed | Build shipped, release checklist signed off |

## Transition history

<!-- Append-only. Newest on top. -->

| Date | From → To | Active epic | Reason | Gate result |
|------|-----------|-------------|--------|-------------|
| YYYY-MM-DD | <phase> → <phase> | <epic-id> | <why> | PASS / CONCERNS / FAIL |
