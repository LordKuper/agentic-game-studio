# Example Session: UX Pipeline — /ux-design → /ux-review → /team-ui

**Context:** Solo indie dev, Pre-Production. HUD + inventory screen = first UI. Game has player journey + GDDs for inventory + combat.

**Starting State:**
- `design/gdd/inventory-system.md` — complete
- `design/gdd/combat-system.md` — complete
- `design/player-journey.md` — complete (6-phase arc)
- No UX specs yet
- `design/ux/` doesn't exist

**Session Duration:** ~90 min across two sessions (HUD then inventory)
**Turns:** 16
**Outcome:** `design/ux/hud.md` + `design/ux/screens/inventory.md` created + reviewed; team-ui handoff for visual

---

## Part 1: HUD Design with /ux-design

### Turn 1: User Starts HUD Design

**User:**
> /ux-design hud

### Turn 2: Agent Loads Context, Presents Approach

**Agent (ux-designer, via /ux-design):**
> Starting HUD design. Loading context...
>
> - `design/player-journey.md` read — key states: Calm Exploration, Tension (threat detected), Crisis (low HP/stamina), Combat
> - `design/gdd/combat-system.md` — HUD needs: HP bar, stamina bar, ability cooldowns (up to 3), damage numbers
> - `design/gdd/inventory-system.md` — HUD needs: quick slots (4), active item indicator
>
> **HUD Philosophy Q:** Player's primary attention focus during play?
>
> Determines HUD aggression:
> - **Option A: Diegetic (low presence)** — HP shown only when damaged, stamina when draining. World-first, HUD recedes.
> - **Option B: Persistent minimal** — HP + stamina always visible, small/unobtrusive. Quick reference, no mode-switch.
> - **Option C: Full tactical** — All info always (HP, stamina, cooldowns, map, slots). Always full awareness.
>
> Survival game + "lone survivor" fantasy → A or B most consistent with genre.

**User:**
> B — I want it always there but minimal. The player is always managing resources.

### Turn 3: Agent Drafts HUD Philosophy Section

**Agent:**
> **HUD Philosophy draft:**
>
> > **Approach: Persistent Minimal**
> > The HUD is always visible but occupies minimal screen real estate. The player is a resource-managing survivor — denying HP/stamina visibility creates frustration, not tension. All critical stats are readable at a glance without interrupting exploration focus. Elements scale in visual weight when they become urgent (HP below 30% pulses, stamina at 0 flashes), then return to minimal state.
>
> May I write to `design/ux/hud.md` as Philosophy section?

**User:**
> Yes

**Agent creates `design/ux/hud.md`, writes Philosophy.**

*(Turns 4–7 continue section-by-section: Info Architecture, Zones, Element Specs, State Machine, Visual Budget, Platform Adaptation — each drafted, approved, written.)*

---

**Final HUD structure:**

```
design/ux/hud.md
  - Philosophy: Persistent Minimal
  - Info Architecture: HP (top-left), Stamina (top-left below HP), Quick Slots (bottom-center),
    Ability Cooldowns (bottom-right), Damage Numbers (world-space)
  - Zones: 4 defined (top-left vitals, bottom-center interaction, bottom-right abilities, world-space)
  - Element Specs: 6 elements with dimensions, font sizes, opacity rules
  - State Machine: Calm / Tension / Crisis / Combat (4 states, transition triggers)
  - Visual Budget: Max 8% screen coverage, 3 simultaneous animations
  - Platform Adaptation: Controller (aim reticle added), Mobile (out of scope v1)
```

---

## Part 2: Inventory Screen with /ux-design

### Turn 8: User Starts Inventory Screen

**User:**
> /ux-design screen inventory

### Turn 9: Agent Scans GDD and Player Journey

**Agent:**
> Loading context for inventory screen...
>
> From `design/gdd/inventory-system.md`:
> - Grid inventory, 6×8 slots
> - Items have weight (affects movement) + stack size
> - Player can equip, drop, inspect, split stacks
>
> From `design/player-journey.md`:
> - Inventory opens often during Calm Exploration
> - "Organized and in control" feel
> - Anxiety: "I need to decide what to drop" — clear, not panicked
>
> **Player Need:** *Quickly assess what I'm carrying, decide keep/drop, get back into world.*
>
> **Key Q:** How do players navigate inventory actions?
> - **Option A: Right-click context menu** — hover → right-click → menu (Escape/common)
> - **Option B: Drag-based** — drag to drop zone to drop; drag to equipment slot to equip (Stardew Valley)
> - **Option C: Inspect panel** — click item → details panel right with action buttons

**User:**
> B — drag-based. I want it tactile.

*(Turns 10–12 continue section-by-section for inventory screen: Player Need, Layout Zones, States, Interaction Map, Data Requirements, Events Fired, Accessibility, Localization.)*

---

## Part 3: /ux-review

### Turn 13: User Runs Review

**User:**
> /ux-review design/ux/

### Turn 14: Agent Reviews Both Documents

**Agent (via /ux-review):**
> Reviewing all UX specs in `design/ux/`...
>
> ---
> **HUD Review (`hud.md`):**
>
> | Check | Status | Notes |
> |-------|--------|-------|
> | GDD alignment — required elements | ✅ | HP, stamina, slots, cooldowns specced |
> | State machine coverage | ✅ | All 4 states |
> | Accessibility tier | ⚠️ | No colorblind mode for HP/stamina (red/yellow) |
> | Visual budget | ✅ | 8% screen, within budget |
> | Platform notes | ✅ | Controller documented |
>
> **Inventory Screen Review (`screens/inventory.md`):**
>
> | Check | Status | Notes |
> |-------|--------|-------|
> | Player Need stated | ✅ | |
> | All GDD actions covered | ✅ | Equip, drop, inspect, split all in interaction map |
> | Drag-drop fully specified | ⚠️ | Drop target when inventory full undefined |
> | Accessibility: keyboard alt for drag | ❌ | MISSING — drag-only fails motor accessibility |
> | Localization: strings tagged | ✅ | |
> | Data events fired | ✅ | `item_equipped`, `item_dropped`, `stack_split` mapped |
>
> ---
> **Verdict: NEEDS REVISION**
>
> Two issues before /team-ui handoff:
>
> 1. **BLOCKING (Accessibility):** Inventory has no keyboard alt to drag-drop. Motor accessibility requires non-drag path. Add "press X to pick up / X again to place" toggle, or details panel with keyboard buttons.
>
> 2. **ADVISORY (HUD):** HP/stamina bars use color alone. Add secondary indicator (shape, icon, pattern) for colorblind. Visual pass OK.
> 3. **ADVISORY (Inventory):** Full-inventory drop behavior unspec. Resolve in GDD before impl, or document out-of-scope.

### Turn 15: User Resolves Blocking Issue

**User:**
> For keyboard: press F to pick up item at cursor, press F again on target slot to place it. Add this to the inventory spec.

**Agent:**
> Adding to Interaction Map under "Keyboard / Controller Path":
>
> > Press F (or South button) to pick up item at cursor. Item enters "held" state with visual indicator. Press F again on target slot to place. Press Escape to cancel and return item.
>
> May I write to `design/ux/screens/inventory.md` and re-run accessibility check?

**User:**
> Yes

**Agent writes update. Re-checks accessibility — passes.**

---

## Part 4: Handoff to /team-ui

### Turn 16: User Kicks Off Team UI

**User:**
> /team-ui

**Agent (via /team-ui):**
> Running UX pipeline context check...
>
> - `design/ux/hud.md` — reviewed ✅
> - `design/ux/screens/inventory.md` — reviewed ✅
> - `/ux-review` verdict: NEEDS REVISION resolved — current: APPROVED ✅
>
> Proceeding to **Phase 2: Visual Design**
>
> Handing off to `art-director` for visual spec (palette, typography, iconography per art bible)...
>
> *(team-ui continues: visual design → layout impl → accessibility audit → final review)*

---

## What This Demonstrates

- **Context-driven design**: agent reads player-journey.md to ground HUD in player emotional state, not feature lists
- **UX review = hard gate**: `/team-ui` checks for passing `/ux-review` before visual design
- **Accessibility caught early**: missing keyboard alt flagged by review, not by QA in final week
- **Blocking vs advisory**: missing keyboard = BLOCKING (stops handoff); colorblind = ADVISORY (visual pass)
- **Section-by-section UX**: same pattern as `/design-system` — each section to file before next
- **Separate HUD + screen files**: `design/ux/hud.md` = whole-game HUD; per-screen specs in `design/ux/screens/`
- **Pattern library enforced by /team-ui**: after this session, inventory drag-drop becomes documented pattern in `design/ux/interaction-patterns.md` for future screens
