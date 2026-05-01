# [Mechanic/System Name]

> **Status**: Draft | In Review | Approved | Implemented
> **Author**: [Agent or person]
> **Last Updated**: [Date]
> **Last Verified**: [Date — last confirmed accurate against current design]
> **Implements Pillar**: [Which pillar supports]

## Summary

[2–3 sentences: what system is, what it does for player, why it exists. For tiered context loading — skill scanning 20 GDDs uses this section to decide whether to read further. No jargon.]

> **Quick reference** — Layer: `[Foundation | Core | Feature | Presentation]` · Priority: `[MVP | Vertical Slice | Alpha | Full Vision]` · Key deps: `[System names or "None"]`

## Overview

[One paragraph for someone knowing nothing about project. What is it, what does player do, why exists?]

## Player Fantasy

[What should player FEEL? Emotional/power fantasy served. Guides all detail decisions below.]

## Detailed Design

### Core Rules

[Precise, unambiguous rules. Programmer implements without questions. Numbered for sequential, bullets for properties.]

### States and Transitions

[Every state, every valid transition.]

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|

### Interactions with Other Systems

[How interacts with combat? Inventory? Progression? UI? Per interaction: data in/out, ownership.]

## Formulas

[Every mathematical formula. Per formula:]

### [Formula Name]

```
result = base_value * (1 + modifier_sum) * scaling_factor
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_value | float | 1-100 | data file | Base before modifiers |
| modifier_sum | float | -0.9 to 5.0 | calculated | Sum of active modifiers |
| scaling_factor | float | 0.5-2.0 | data file | Level scaling |

**Expected output range**: [min] to [max]
**Edge case**: modifier_sum < -0.9 → clamp to -0.9 to prevent negatives.

## Edge Cases

[What happens in unusual situations. Each needs clear resolution.]

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| [What if X is zero?] | [Behavior] | [Reason] |
| [What if both effects trigger?] | [Priority rule] | [Design reason] |

## Dependencies

[Every system this depends on or depends on this.]

| System | Direction | Nature |
|--------|-----------|--------|
| [Combat] | This depends on Combat | Damage calc results |
| [Inventory] | Inventory depends on this | Item effect data |

## Tuning Knobs

[Every adjustable balancing value. Current, range, extremes.]

| Parameter | Current | Safe Range | Increase Effect | Decrease Effect |
|-----------|---------|------------|-----------------|-----------------|

## Visual/Audio Requirements

[Visual + audio feedback needs.]

| Event | Visual | Audio | Priority |
|-------|--------|-------|----------|

## Game Feel

> Visual/Audio = WHAT feedback events. Game Feel = HOW mechanic feels — responsiveness, weight, snap, kinesthetic quality. Drives animation budgets, input handling, hitbox timing. Retrofitting feel = expensive rework.

### Feel Reference

[Specific game/mechanic/moment capturing target feel. Cite exact mechanic, not just game. Optional anti-reference (what this should NOT feel like).]

> Example: "Should feel like Dark Souls weapon swings — weighty, committed, telegraphed, satisfying on contact. NOT floaty like early Halo melee."

### Input Responsiveness

[Max acceptable latency per action.]

| Action | Max Latency (ms) | Frame Budget (60fps) | Notes |
|--------|------------------|---------------------|-------|
| [Primary action] | [50ms] | [3 frames] | |
| [Secondary action] | | | |

### Animation Feel Targets

[Frame data per animation. Startup = windup before effect. Active = action happening (hitbox live, ability firing). Recovery = committed/vulnerable frames after.]

| Animation | Startup | Active | Recovery | Feel Goal | Notes |
|-----------|---------|--------|----------|-----------|-------|
| [Light attack] | | | | [Snappy, low commitment] | |
| [Heavy attack] | | | | [Weighty, high commitment] | |

### Impact Moments

[Punctuation of mechanic — peak feedback intensity making actions feel consequential. Every high-stakes event needs entry.]

| Impact Type | Duration (ms) | Effect | Configurable? |
|-------------|--------------|--------|---------------|
| Hit-stop (freeze frames) | [80ms] | [Freeze both objects on contact] | Yes |
| Screen shake | [150ms] | [Directional, decaying] | Yes |
| Camera impact | | | |
| Controller rumble | | | |
| Time-scale slowdown | | | |

### Weight and Responsiveness Profile

[Short prose. Answer:]

- **Weight**: Heavy/deliberate or light/reactive?
- **Player control**: Course-correct mid-action (high) or committed momentum-based (low)?
- **Snap quality**: Crisp binary or smooth analog?
- **Acceleration model**: Instant start (arcade) or ramp from zero (simulation)? Same for decel.
- **Failure texture**: On player error, fair or punishing? Read on WHY they failed?

### Feel Acceptance Criteria

[Testable subjective targets, precise enough for consistent verdicts.]

- [ ] [e.g., "Combat feels impactful — testers comment on weight unprompted"]
- [ ] [e.g., "No reviewer uses 'floaty', 'slippery', 'unresponsive'"]
- [ ] [e.g., "Input latency imperceptible at 60fps"]
- [ ] [e.g., "Hit-stop reads as satisfying, not lag/stutter"]

## UI Requirements

[What displayed when?]

| Information | Location | Update Frequency | Condition |
|-------------|----------|------------------|-----------|

## Cross-References

[Every explicit dependency on another GDD's specific mechanic/value/rule. Machine-checked by `/ags-review-all-gdds` Phase 2c. Prose references must appear here.]

| References | Target GDD | Specific Element | Nature |
|-----------|-----------|------------------|--------|
| ["combo multiplier feeds score"] | `design/gdd/score.md` | `combo_multiplier` output | Data dependency |
| ["death triggers respawn"] | `design/gdd/respawn.md` | Death state transition | State trigger |
| ["stamina gates dodge"] | `design/gdd/stamina.md` | Stamina depletion rule | Rule dependency |

> **Nature**: `Data dependency` (consume their output), `State trigger` (their state change triggers ours), `Rule dependency` (our rule assumes theirs), `Ownership handoff` (we hand off value to them).

## Acceptance Criteria

[Testable criteria confirming mechanic works as designed.]

- [ ] [Specific, measurable, testable]
- [ ] [Criterion 2]
- [ ] [Criterion 3]
- [ ] Performance: System update <[X]ms
- [ ] No hardcoded values in implementation

## Open Questions

[Not yet decided. Owner + deadline.]

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
