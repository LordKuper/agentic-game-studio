# Example Session: Using /design-system to Author a GDD

**Context:** Solo indie dev, week 3. `/map-systems` already ran, produced `design/gdd/systems-index.md`. Movement listed MVP tier, Foundation priority.

**Starting State:**
- `design/gdd/game-concept.md` exists (survival-crafting, 2D, Unity 6000.0.30f1)
- `design/gdd/systems-index.md` exists (15 systems, movement first in design order)
- No per-system GDDs yet
- No upstream deps for movement (foundation)

**Session Duration:** ~60 min
**Turns:** 14
**Outcome:** Complete `design/gdd/movement-system.md`, all 8 sections written, ready for `/design-review`

---

## Turn 1: User Invokes Skill

**User:**
> /design-system movement

## Turn 2: Agent Loads Context, Presents Plan

**Agent (game-designer, via /design-system):**
> Loading context:
>
> - Read `design/gdd/game-concept.md` — survival-crafting, exploration-heavy, lone survivor on alien planet
> - Read `design/gdd/systems-index.md` — movement is **Foundation / MVP**, no upstream deps, downstream: combat, exploration, stamina
> - No prior GDD for movement — starting fresh
>
> **Technical Feasibility Pre-Check:**
>
> | Domain | Signal |
> |--------|--------|
> | Engine | Unity 6000.0.30f1 — Rigidbody2D + 2D physics well-supported |
> | Physics | Built-in 2D physics (Box2D) — well-trodden, note for 3D work |
> | Downstream risk | Stamina downstream — movement must expose stamina callback hook |
>
> Template: **8 sections**. One at a time:
> `Overview → Player Fantasy → Detailed Rules → Formulas → Edge Cases → Dependencies → Tuning Knobs → Acceptance Criteria`
>
> Draft each → you approve/revise → write to file → next.
>
> Creating skeleton with empty headers — may I write `design/gdd/movement-system.md` with placeholders?

**User:**
> Yes

**Agent writes** `design/gdd/movement-system.md` with 8 empty section headers.

---

## Turn 3: Section 1 — Overview

**Agent:**
> **Section 1 draft — Overview:**
>
> > The movement system is the player's primary interface with the world. It governs how the lone survivor walks, runs, rolls, and traverses alien terrain. Movement is physically grounded — the planet has heavier-than-Earth gravity, and the survivor is physically exhausted — but remains responsive enough to stay out of the player's way. The system feeds directly into exploration pacing, combat positioning, and the stamina system.
>
> Capture intent?

**User:**
> Good but add something about the terrain — surfaces slow you down or have friction modifiers.

**Agent:**
> Revised:
>
> > The movement system is the player's primary interface with the world. It governs how the lone survivor walks, runs, rolls, and traverses alien terrain. Movement is physically grounded — the planet has heavier-than-Earth gravity, and the survivor is physically exhausted — but remains responsive enough to stay out of the player's way. Terrain surfaces apply friction and speed modifiers (slick rock, sand, shallow water) that make navigation a meaningful skill. The system feeds directly into exploration pacing, combat positioning, and the stamina system.
>
> Write to Section 1?

**User:**
> Yes

**Agent writes Section 1.**

---

## Turn 4: Section 2 — Player Fantasy

**Agent:**
> **Section 2 draft — Player Fantasy:**
>
> > The player should feel like a tired but capable survivor — not a superhero, not helpless. Early game movement is deliberate and taxing; the player earns fluency over time. When traversing alien terrain, the goal is careful navigation with moments of urgency (running from threats, leaping a gap). The fantasy is "I know this terrain" — every surface type becomes readable and manageable as the player internalizes the friction system.
>
> Approve, revise, change direction?

**User:**
> Love it. Write it.

**Agent writes Section 2.**

---

## Turn 5: Section 3 — Detailed Rules

**Agent:**
> **Section 3 draft — Detailed Rules:**
>
> > **Base States:**
> > - **Walk** (default): Full directional control, moderate speed, low stamina drain
> > - **Run** (hold Shift): Faster, medium stamina drain per second — cancels automatically when stamina hits 0
> > - **Roll/Dodge** (tap Space): Quick burst in movement direction, brief invincibility frames, high stamina cost (flat)
> > - **Idle**: Zero movement input, stamina regenerates at full rate
> >
> > **Terrain Modifiers:**
> > - Normal ground: 1.0x speed multiplier, standard friction
> > - Sand/loose soil: 0.8x speed, 1.1x stamina drain (harder to move)
> > - Slick rock (wet): 1.1x speed, reduced stop friction (player overshoots)
> > - Shallow water (ankle-deep): 0.7x speed, no friction modifier
> > - Deep water: movement disabled, replaced by swimming (separate system, out of scope)
> >
> > **Collision:**
> > - Rigidbody2D (kinematic) + collider; wall slide enabled
> > - Slopes up to 45° walked normally; >45° blocked
> > - One-way platforms supported (drop-through with Down + Jump)
>
> Trim or expand?

**User:**
> Add that running is disabled when carrying a heavy object (like a large resource chunk).

**Agent:**
> Added to Run rule:
> > - **Run** (hold Shift): Faster, medium stamina drain per second — cancels automatically when stamina hits 0. **Disabled when carrying heavy objects** (signals broadcast by inventory system).
>
> Write?

**User:**
> Yes

**Agent writes Section 3.**

---

## Turn 6: Section 4 — Formulas

**Agent:**
> **Section 4 draft — Formulas:**
>
> Variables first:
>
> | Variable | Type | Description |
> |----------|------|-------------|
> | `base_walk_speed` | float | Base walk speed in px/s (tunable) |
> | `run_multiplier` | float | Run speed = walk × this |
> | `terrain_speed_mod` | float | Per-terrain multiplier (0.7–1.1) |
> | `stamina_drain_walk` | float | Stamina units/sec while walking |
> | `stamina_drain_run` | float | Stamina units/sec while running |
> | `stamina_cost_roll` | float | Flat stamina cost per roll |
> | `terrain_drain_mod` | float | Per-terrain stamina drain multiplier |
>
> **Effective Speed:**
> ```
> effective_speed = base_walk_speed × run_multiplier (if running, else 1.0) × terrain_speed_mod
> ```
>
> **Stamina Drain (per second):**
> ```
> drain_per_sec = stamina_drain_walk (or stamina_drain_run if running) × terrain_drain_mod
> ```
>
> **Roll Stamina Cost (flat):**
> ```
> stamina_after_roll = current_stamina - stamina_cost_roll
> (clamped to 0 — roll still executes, but player lands with 0 stamina)
> ```
>
> Approve or adjust?

**User:**
> Looks right. Write it.

**Agent writes Section 4.**

---

## Turn 7–9: Sections 5, 6, 7

*(Same pattern — agent drafts, user approves or tweaks, agent writes. Abbreviated.)*

**Section 5 — Edge Cases:**
- Player rolls into wall (animation completes, no movement bonus)
- Stamina hits 0 mid-run (transitions to walk, no lurch)
- Two terrain types overlap (lower multiplier wins)
- Roll on slick rock (slick friction applies to stop, not roll itself)

**Section 6 — Dependencies:**
- **Stamina (downstream):** movement exposes `OnStaminaEvent(string type, float amount)` C# event
- **Inventory (downstream):** listens to `CarryingHeavyObjectChanged(bool isHeavy)` event
- **Input:** Unity Input System (`InputAction` for `Move`, etc.)
- **Terrain layer:** tile metadata drives modifier lookup

**Section 7 — Tuning Knobs:**
- `base_walk_speed` (default: 120 px/s)
- `run_multiplier` (default: 1.7)
- Per-terrain speed and drain modifiers (4 terrain types)
- `stamina_drain_walk`, `stamina_drain_run`, `stamina_cost_roll`

---

## Turn 10: Section 8 — Acceptance Criteria

**Agent:**
> **Section 8 draft — Acceptance Criteria:**
>
> Testable conditions for completion:
>
> - [ ] Player walks at `base_walk_speed` on normal ground (debug overlay)
> - [ ] Player runs at `base_walk_speed × run_multiplier` on normal ground
> - [ ] Run auto-cancels when stamina = 0, no input
> - [ ] Roll executes with correct iframes (hitbox debug)
> - [ ] Each terrain type applies correct speed mod (QA table in sprint plan)
> - [ ] Carrying heavy object disables run (inventory integration test)
> - [ ] Stamina system receives `OnStaminaEvent` during walk/run/roll
> - [ ] All movement values from `[SerializeField]` — no hardcoded
>
> Covers "done"?

**User:**
> Add one for slopes — make sure >45 degree slopes block movement.

**Agent:**
> Added:
> - [ ] Slopes ≤45° traversed normally; >45° block movement (player slides back)
>
> Write?

**User:**
> Yes. Write it.

**Agent writes Section 8.**

---

## Turn 11: Completion Summary

**Agent:**
> All 8 sections written to `design/gdd/movement-system.md`.
>
> **GDD Status:**
> - ✅ Overview
> - ✅ Player Fantasy
> - ✅ Detailed Rules
> - ✅ Formulas
> - ✅ Edge Cases
> - ✅ Dependencies (exposes 2 events: OnStaminaEvent, CarryingHeavyObjectChanged)
> - ✅ Tuning Knobs
> - ✅ Acceptance Criteria
>
> **Key decisions:**
> - Terrain modifiers as tile metadata (not code constants)
> - Roll = flat stamina cost (not per-second)
> - Heavy carry disables run (signal-driven, not hardcoded check)
>
> **Next:** `/design-review design/gdd/movement-system.md` before next system in queue (stamina next per index).
>
> Review validates completeness, consistency, catches issues before building on this GDD.

---

## What This Demonstrates

- **Incremental file writing**: each section to disk after approval — crash loses at most one in-progress section
- **Template population**: 8 sections become concrete content via dialogue, not bulk gen
- **Dependency signaling**: agent surfaces downstream event contracts (stamina, inventory) in Dependencies
- **Feasibility pre-check**: notes engine version risks before design, not after impl breaks
- **Section-by-section cycle**: no full draft dump — each section small, reviewable
- **Recovery**: crash → agent re-reads file on restart, resumes from first empty section
