---
name: ags-balance-check
description: "Analyzes game balance data files, formulas, and configuration to identify outliers, broken progressions, degenerate strategies, and economy imbalances. Use after modifying any balance-related data or design. Use when user says 'balance report', 'check game balance', 'run a balance check'."
argument-hint: "[system-name|path-to-data-file]"
user-invocable: true
allowed-tools: Read, Glob, Grep, AskUserQuestion
agent: systems-designer
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `design/gdd/systems-index.md` | `/ags-map-systems` | STOP. "No systems map. Run `/ags-map-systems` first." |
| Target system GDD `design/gdd/[system].md` | `/ags-design-system` | STOP. "GDD missing. Run `/ags-design-system [system]`." |
| Balance data files for target system | manual / data pipeline | WARN: formulas-only analysis. Add data under `assets/data/` for richer report. |

If STOP triggers, exit with verdict **BLOCKED — missing prerequisite**.

---

## Phase 1: Identify Balance Domain

Determine the balance domain from `$ARGUMENTS[0]`:

- **Combat** → weapon/ability DPS, time-to-kill, damage type interactions
- **Economy** → resource faucets/sinks, acquisition rates, item pricing
- **Progression** → XP/power curves, dead zones, power spikes
- **Loot** → rarity distribution, pity timers, inventory pressure
- **File path given** → load that file directly and infer domain from content

If no argument, ask the user which system to check.

---

## Phase 2: Read Data Files

Read relevant files from `assets/data/` and `design/balance/` for the identified domain.
Note every file read — they will appear in the Data Sources section of the report.

---

## Phase 3: Read Design Document

Read the GDD for the system from `design/gdd/` to understand intended design targets,
tuning knobs, and expected value ranges. This is the baseline for "correct" behaviour.

---

## Phase 4: Perform Analysis

Run domain-specific checks:

**Combat balance:**
- Calculate DPS for all weapons/abilities at each power tier
- Check time-to-kill at each tier
- Identify any options that dominate all others (strictly better)
- Check if defensive options can create unkillable states
- Verify damage type/resistance interactions are balanced

**Economy balance:**
- Map all resource faucets and sinks with flow rates
- Project resource accumulation over time
- Check for infinite resource loops
- Verify gold sinks scale with gold generation
- Check if any items are never worth purchasing

**Progression balance:**
- Plot the XP curve and power curve
- Check for dead zones (no meaningful progression for too long)
- Check for power spikes (sudden jumps in capability)
- Verify content gates align with expected player power
- Check if skip/grind strategies break intended pacing

**Loot balance:**
- Calculate expected time to acquire each rarity tier
- Check pity timer math
- Verify no loot is strictly useless at any stage
- Check inventory pressure vs acquisition rate

---

## Phase 5: Output the Analysis

```
## Balance Check: [System Name]

### Data Sources Analyzed
- [List of files read]

### Health Summary: [HEALTHY / CONCERNS / CRITICAL ISSUES]

### Outliers Detected
| Item/Value | Expected Range | Actual | Issue |
|-----------|---------------|--------|-------|

### Degenerate Strategies Found
- [Strategy description and why it is problematic]

### Progression Analysis
[Graph description or table showing progression curve health]

### Recommendations
| Priority | Issue | Suggested Fix | Impact |
|----------|-------|--------------|--------|

### Values That Need Attention
[Specific values with suggested adjustments and rationale]
```

---

## Phase 6: Fix & Verify Cycle

After report, ask: "Would you like to fix any balance issues now?"

If yes:
- Ask which issue first (by priority row in Recommendations table)
- Guide user to update relevant data file in `assets/data/` or formula in `design/balance/`
- After each fix, offer to re-run balance checks to verify no new outliers
- If fix changes a tuning knob from a GDD or ADR, remind:
  > "This value is defined in a design document. Run `/ags-propagate-design-change [path]` to find downstream impacts before committing."

If no: summarize open issues, suggest saving to `design/balance/balance-check-[system]-[date].md`.

End: "Re-run `/ags-balance-check` after fixes to verify."

---

## Combined Review Loop (parallel external Codex)

Per `.ags/rules/review-workflow.md`. Balance-analysis phases run **in parallel** with external Codex inside one loop. Each iteration:

1. Resolve severity floor: iter 1-2 → keep all severities; iter 3-4 → critical/high; iter 5+ → critical only.
2. **Spawn in one message, in parallel**:
   - All internal reviewer Tasks (game-designer + systems-designer).
   - For the cited GDD: `/ags-external-review gdd [gdd-path] --embedded-parallel --iteration [N] --min-severity [floor]` — balance config bundled via `{{RELATED_DOCS}}`. Codex unavailable → `skipped: codex-unavailable`; aggregator logs skip in decisions-log and continues with internal pool only.
3. Aggregator (`game-designer`) merges findings from internal + Codex, drops nitpicks + below-floor.
4. **Loop exit**: filtered set empty → emit final verdict. Non-empty → surface aggregated kept findings, user resolves, N++, repeat.

No iteration cap. No user-confirm gate before external — it runs every iteration automatically. Record final iteration count in the verdict report and decisions-log entry. Codex reviews the source GDD + balance config, NOT this balance-check report.
