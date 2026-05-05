# Review Mode

<!-- Director-gate intensity. Controls how heavy review pipeline runs at gates. -->

## Current

- **Mode**: <full | lean | solo>
- **Set**: <YYYY-MM-DD>
- **Set by**: <user | producer>
- **Reason**: <one line>

## Modes

| Mode | Reviewers | Use when |
|------|-----------|----------|
| full | All relevant directors + leads + Codex | High-risk gates: epic close, milestone, release. Default for production phase. |
| lean | Department lead + Codex | Routine gates, low-risk changes, solo-dev project with leads delegated. |
| solo | Author self-review + Codex | Solo dev, prototypes, concept phase, throwaway work. |

## Switching rules

- Mode persists until explicitly changed.
- Release phase forces `full` regardless of prior setting.
- Codex unavailable → auto-skip + log to `decisions-log.md` (no user prompt). See `.ags/rules/review-workflow.md`.

## History

| Date | From → To | Reason |
|------|-----------|--------|
| YYYY-MM-DD | <mode> → <mode> | <why> |
