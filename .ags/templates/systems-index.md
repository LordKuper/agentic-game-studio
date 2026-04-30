# Systems Index: [Game Title]

> **Status**: [Draft / Under Review / Approved]
> **Created**: [Date]
> **Last Updated**: [Date]
> **Source Concept**: design/gdd/game-concept.md

---

## Overview

[One paragraph: game's mechanical scope. What systems needed? Reference core loop + pillars. Help any team member understand "big picture" of design + build.]

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | [Player Controller] | Core | MVP | [Not Started / In Design / In Review / Approved / Implemented] | [design/gdd/player-controller.md or "—"] | [Input, Physics] |
| 2 | [Camera System] | Core | MVP | Not Started | — | Player Controller |

[Row per identified system. Use categories + priorities below. Inferred systems (not explicitly in concept) → mark "(inferred)".]

---

## Categories

| Category | Description | Typical Systems |
|----------|-------------|-----------------|
| **Core** | Foundation everything depends on | Player controller, input, physics, camera, scene management, state machine |
| **Gameplay** | Systems making game fun | Combat, AI, stealth, movement abilities, interaction |
| **Progression** | Player growth over time | XP/leveling, skill trees, unlocks, achievements, reputation |
| **Economy** | Resource creation + consumption | Currency, loot, crafting, shops, item DB, drop tables |
| **Persistence** | Save state + continuity | Save/load, settings, cloud sync, profile |
| **UI** | Player-facing displays | HUD, menus, inventory, dialogue UI, map, notifications |
| **Audio** | Sound + music | Music manager, SFX bus, ambient, adaptive music, voice |
| **Narrative** | Story + dialogue delivery | Dialogue, quest tracking, cutscenes, journal, lore |
| **Meta** | Outside core loop | Analytics, tutorials, accessibility, photo mode |

[Not every game needs every category. Remove inapplicable. Add custom if needed.]

---

## Priority Tiers

| Tier | Definition | Target Milestone | Design Urgency |
|------|------------|------------------|----------------|
| **MVP** | Core loop functions. Test "is this fun?" | First playable | Design FIRST |
| **Vertical Slice** | One complete polished area. Full experience demo. | Vertical slice / demo | Design SECOND |
| **Alpha** | All features rough. Complete mechanical scope, placeholder content OK. | Alpha milestone | Design THIRD |
| **Full Vision** | Polish, edge cases, nice-to-haves, content-complete. | Beta / Release | As needed |

---

## Dependency Map

[Sorted by dependency order — design + build top to bottom.]

### Foundation Layer (no dependencies)

1. [System] — [why foundational]

### Core Layer (depends on foundation)

1. [System] — depends on: [list]

### Feature Layer (depends on core)

1. [System] — depends on: [list]

### Presentation Layer (depends on features)

1. [System] — depends on: [list]

### Polish Layer (depends on everything)

1. [System] — depends on: [list]

---

## Recommended Design Order

[Combining dependency + priority. Each GDD complete + reviewed before next. Independent systems same layer = parallel.]

| Order | System | Priority | Layer | Agent(s) | Effort |
|-------|--------|----------|-------|----------|--------|
| 1 | [First system] | MVP | Foundation | game-designer | [S/M/L] |
| 2 | [Second] | MVP | Foundation | game-designer | [S/M/L] |

[Effort: S = 1 session, M = 2-3, L = 4+. Session = one focused conversation producing complete GDD.]

---

## Circular Dependencies

[Cycles found during analysis. Need architectural attention — break with interface or design simultaneously.]

- [None found] OR
- [System A <-> System B: description + proposed resolution]

---

## High-Risk Systems

[Technically unproven, design-uncertain, scope-dangerous. Validate early regardless of priority — spike, research, playable test.]

| System | Risk Type | Description | Mitigation |
|--------|-----------|-------------|------------|
| [System] | [Technical / Design / Scope] | [What could go wrong] | [Spike, research, scope fallback] |

---

## Progress Tracker

| Metric | Count |
|--------|-------|
| Total systems identified | [N] |
| Design docs started | [N] |
| Design docs reviewed | [N] |
| Design docs approved | [N] |
| MVP systems designed | [N/total MVP] |
| Vertical Slice systems designed | [N/total VS] |

---

## Next Steps

- [ ] Review + approve enumeration
- [ ] Design MVP-tier first (`/design-system [name]`)
- [ ] Run `/design-review` per completed GDD
- [ ] Run `/gate-check pre-production` when MVP designed
