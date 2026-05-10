---
status: draft        # draft | approved
approved_at:         # YYYY-MM-DD; required when status: approved
---

# UX Specification: [Screen / Flow Name]

> **Precondition**: cited GDD must have `status: approved` (auto-checked by `/ags-ux-design`). See `.ags/rules/document-boundaries.md`.
> **Author**: [Name or agent — e.g., ui-designer]
> **Last Updated**: [Date]
> **Screen / Flow Name**: [Identifier — e.g., `InventoryScreen`, `NewGameFlow`]
> **Platform Target**: [PC | Console | Mobile | All]
> **Related GDDs**: [GDD sections — e.g., `design/gdd/inventory.md § UI Requirements`]
> **Related ADRs**: [e.g., `ADR-0012: UI Framework Selection`]
> **Design Tokens**: `design/art/DESIGN.md` (DESIGN.md spec) — colors, typography, spacing, radii, component styles referenced as `{colors.x}` / `{typography.y}` / `{spacing.z}` / `{components.w}`. No raw hex / px / pt values in this spec.
> **Related UX Specs**: [Sibling/parent screens]
> **Accessibility Tier**: Basic | Standard | Comprehensive | Exemplary

> **Scope**: Discrete screens/flows (menus, dialogs, inventory, settings, cutscene UI). Persistent in-game overlays → use `hud-design.md`. Hybrid (e.g., pause overlay) → screen spec, note overlay relationship in Navigation Position.

---

## 1. Purpose & Player Need

> Justify screen from player perspective. Filter for every subsequent decision.

**What player need does this screen serve?**

[One paragraph. Real human need, not system function. What would player say they want? What would frustrate them?

Bad: "Displays player's items and equipment."
Good: "Lets player understand what they're carrying and quickly decide what to take into the next encounter, without breaking mental model. Inventory = planning tool between action."]

**Player goal** (what player wants):

[One sentence. Acceptance-criterion specific. Example: "Find item within three button presses and equip without separate screen."]

**Game goal** (what system needs):

[One sentence. Example: "Record equipment choices, relay to combat system before encounter loads." Prevents UI that looks good but fails its system.]

---

## 2. Player Context on Arrival

> Same screen feels different based on arrival context. Calibrate to actual player.

| Question | Answer |
|----------|--------|
| What was player just doing? | [e.g., Combat / Esc from exploration / Cutscene] |
| Emotional state? | [e.g., High tension / Calm exploration] |
| Cognitive load? | [e.g., High — tracking enemies / Low — no threats] |
| What info do they have? | [e.g., Picked up item, haven't seen stats] |
| Most likely trying to do? | [e.g., Compare new item to current weapon — primary use case] |
| Likely afraid of? | [e.g., Missing something, irreversible mistake, losing place] |

**Emotional design target**:

[One sentence. Example: "Confident and in control — complete information, complete authority over choices, no ambiguity about outcomes."]

---

## 3. Navigation Position

> Defines transitions, back behavior, pause relationship. 8 entry points = complexity flag — resolve in design.

**Screen hierarchy**:

```
[Root — e.g., Main Menu]
  └── [Parent — e.g., Settings]
        └── [This — e.g., Audio Settings]
              ├── [Child — e.g., Advanced Audio]
              └── [Child — e.g., Speaker Test]
```

**Modal behavior**: [Modal | Non-modal | Overlay (paused) | Overlay-live (continues)]

> Modal: document dismiss. Back/B? Esc? Click outside? Cannot dismiss? Undismissable = high friction — justify.

**Reachability — entry points**:

| Entry Point | Triggered By | Notes |
|-------------|-------------|-------|
| [Main Menu → Play] | [Selects "New Game"] | [Primary] |
| [Pause → Resume] | [Start from gameplay] | [Secondary] |
| [Game event] | [Tutorial first time only] | [Systemic — must not break if dismissed] |

---

## 4. Entry & Exit Points

> Every entry needs corresponding exit. Empty cells = unfinished design.

**Entry table**:

| Trigger | Source Screen / State | Transition Type | Data Passed In | Notes |
|---------|----------------------|-----------------|----------------|-------|
| [Inventory button] | [Gameplay/Exploration] | [Overlay push, paused] | [Loadout, contents] | [Any non-combat state] |
| [Item pickup accepted] | [Pickup dialog] | [Replace with full inventory] | [New item pre-highlighted] | [New item visually distinguished] |
| [Quest directs to inventory] | [Quest Update notification] | [Overlay push] | [Quest item ID for highlight] | [Deep-link to relevant item] |

**Exit table**:

| Exit Action | Destination | Transition | Data Returned/Saved | Notes |
|-------------|------------|------------|---------------------|-------|
| [Player closes (Back/B/Esc)] | [Previous — Exploration] | [Overlay pop, resumes] | [Updated loadout committed] | [Commit before transition] |
| [Equip on item] | [Same screen, updated] | [In-place state change] | [Loadout change event] | [No nav, just refresh] |
| [Map shortcut] | [Map Screen] | [Replace] | [None] | [Inventory state preserved if return] |

---

## 5. Layout Specification

> Handoff between UX and UI programming. Communicate hierarchy, proximity, proportion. ASCII OK.
>
> Draw at one standard resolution (e.g., 1920x1080). Adaptations noted separately.

### 5.1 Wireframe

```
[ASCII art layout. Suggested chars:
 ┌ ┐ └ ┘ │ ─    borders
 ╔ ╗ ╚ ╝ ║ ═    emphasized/modal
 [ ]              interactive
 { }              content areas
 ...              scrollable
 ●                focused on open

Example:
┌──────────────────────────────────────────────┐
│  [← Back]        INVENTORY         [Options] │  ← HEADER
├──────────────────────────────────────────────┤
│ ┌──────────────┐  ┌─────────────────────────┐│
│ │ CATEGORY NAV │  │  ITEM DETAIL PANEL      ││  ← CONTENT
│ │  ● Weapons   │  │  Item Name              ││
│ │    Armor     │  │  {item icon}            ││
│ │    Consumable│  │  Stats comparison       ││
│ │    Key Items │  │  Description text...    ││
│ ├──────────────┤  └─────────────────────────┘│
│ │ ITEM GRID    │                             │
│ │ {□}{□}{□}{□} │                             │
│ │ {□}{□}{□}{□} │                             │
│ │ ...          │                             │
│ └──────────────┘                             │
├──────────────────────────────────────────────┤
│   [Equip]     [Drop]     [Compare]  [Close]  │  ← ACTION BAR
└──────────────────────────────────────────────┘
]
```

### 5.2 Zone Definitions

| Zone | Description | Approx Size | Scrollable? | Overflow |
|------|-------------|-------------|-------------|----------|
| [Header] | [Top bar: nav, title, global actions] | [Full width, ~10% h] | [No] | [Truncate ellipsis] |
| [Category Nav] | [Left: tabs] | [~25% w, ~75% h] | [Yes vertical] | [Scroll indicator at bottom] |
| [Item Grid] | [Center: icons for category] | [~45% w, ~75% h] | [Yes vertical] | [Page-based 4x4] |
| [Detail Panel] | [Right: stats, description] | [~30% w, ~75% h] | [Yes for long desc] | [Fade at bottom, scroll] |
| [Action Bar] | [Bottom: contextual actions] | [Full width, ~15% h] | [No] | [Collapse to icon-only below 4] |

### 5.3 Component Inventory

> Every discrete component. Drives implementation task list.

| Component | Type | Zone | Purpose | Required? | Reuse? |
|-----------|------|------|---------|-----------|--------|
| [Back Button] | [Button] | [Header] | [Returns to previous] | [Yes] | [Yes — NavButton] |
| [Screen Title] | [Text] | [Header] | ["INVENTORY"] | [Yes] | [Yes — ScreenTitle] |
| [Category Tab] | [Toggle Button] | [Category Nav] | [Filter grid] | [Yes] | [No — new] |
| [Item Slot] | [Icon + Frame] | [Item Grid] | [Slot, empty or filled] | [Yes] | [No — new] |
| [Item Name Label] | [Text] | [Detail] | [Selected item name] | [Yes] | [Yes — BodyText] |
| [Stat Comparison Row] | [Compound] | [Detail] | [Stat vs equipped] | [Yes] | [No — new] |
| [Equip Button] | [Primary Button] | [Action Bar] | [Equip selected] | [Yes] | [Yes — PrimaryAction] |
| [Empty State Message] | [Text + Icon] | [Item Grid] | [Empty category] | [Yes] | [Yes — EmptyState] |

**Primary focus on open**: [e.g., First item in Grid — or deep-linked highlight. If empty, first Category Tab.]

---

## 6. States & Variants

> Screen = set of states, each must look + behave correctly. Document every state. Test matrix for QA.

| State | Trigger | Visual Change | Behavioral Change | Notes |
|-------|---------|---------------|-------------------|-------|
| [Loading] | [Opening, no data] | [Skeleton/shimmer in Grid; Action Bar disabled] | [Only Close works] | [Should not exceed 500ms; if so, investigate fetch] |
| [Empty — no items] | [Category zero items] | [EmptyState replaces Grid: icon + "Nothing here yet."] | [No item actions; hide Drop/Equip/Compare] | [Remove disabled buttons, don't dim them] |
| [Populated] | [Category has items] | [Grid fills; first slot auto-focused] | [All actions for selected] | [Default common state] |
| [Item Selected] | [Player navigates to slot] | [Detail populates; focus ring; Action Bar updates] | [Equip/Drop/Compare per type] | [Equip disabled if equipped — show "Equipped" badge] |
| [Confirmation Pending — Drop] | [Drop action] | [Confirm dialog overlays] | [Background suspended] | [Modal confirm. Items unrecoverable.] |
| [Error — load failed] | [Data fetch fail] | [Grid: icon + "Couldn't load items." + Retry] | [Only Retry, Close] | [Log error; no technical details to player] |
| [Item Newly Acquired] | [Opened from pickup deep-link] | ["New" badge on item; Detail pre-populated] | [Same as Selected + badge] | [Badge persists until manual nav off slot] |

---

## 7. Interaction Map

> Source of truth for every input. Forces full thinking: mouse, kb, gamepad, touch × hover, focus, pressed, disabled. Gaps = bugs. Input for accessibility audit.

### 7.1 Navigation Inputs

| Input | Platform | Action | Visual Response | Audio Cue | Notes |
|-------|----------|--------|-----------------|-----------|-------|
| [Arrows / D-Pad] | [All] | [Move focus within zone] | [Focus ring moves] | [Soft tick] | [Wrap at zone edges; don't cross zones] |
| [Tab / R1] | [KB / Gamepad] | [Next zone] | [Ring jumps to first in zone] | [Distinct zone tone] | [Shift+Tab / L1 backward] |
| [Mouse hover] | [PC] | [Hover state] | [Highlight/underline/color] | [None] | [Hover ≠ focus — only click does] |
| [Mouse click] | [PC] | [Select + focus] | [Pressed flash, then focused] | [Soft click] | [Right-click context if applicable] |
| [Touch tap] | [Mobile] | [Select + activate] | [Press ripple] | [Soft click] | [Tap = click + confirm for low-risk; explicit confirm for destructive] |

### 7.2 Action Inputs

| Input | Platform | Context | Action | Response | Animation | Audio | Notes |
|-------|----------|---------|--------|----------|-----------|-------|-------|
| [Enter / A / Left click] | [All] | [Item slot focused] | [Select item, populate Detail] | [Detail slides/updates] | [Fade/slide 120ms] | [Soft select] | [Already selected: no-op] |
| [Enter / A] | [All] | [Equip focused] | [Equip selected] | [Press anim; "Equipped" badge updates] | [Badge swap 80ms] | [Equip success] | [Fires EquipItem to Inventory] |
| [Triangle / Y / Right-click] | [All] | [Item slot focused] | [Item context menu] | [Menu appears adjacent] | [Popover 80ms] | [Menu open] | [Equip, Drop, Inspect, Compare] |
| [Square / X] | [Gamepad] | [Item slot focused] | [Quick-equip] | [Inline equip anim] | [Slot flash 80ms] | [Equip success] | [Convenience; no screen state change] |
| [Esc / B / Back] | [All] | [Any, screen level] | [Close, return previous] | [Screen exit] | [Slide out 200ms] | [Back/close] | [Commits all changes. No discard.] |
| [F / L2] | [KB / Gamepad] | [Any] | [Toggle filter panel] | [Filter overlay opens] | [Slide from right 200ms] | [Panel open] | [Disabled if empty category] |

### 7.3 State-Specific Behaviors

| State | Input Restriction | Reason |
|-------|------------------|--------|
| [Loading] | [Item + action inputs disabled] | [No data; prevent races] |
| [Confirm dialog open] | [Only Confirm + Cancel] | [Modal locks background] |
| [Error] | [Only Retry + Close] | [No data to navigate] |

---

## 8. Data Requirements

> UI/state separation = most important UI architecture boundary. UI reads, doesn't own. UI fires events, doesn't write state.

| Data Element | Source System | Update Frequency | Owner | Format | Null/Missing Handling |
|--------------|--------------|-----------------|-------|--------|----------------------|
| [Item list] | [Inventory] | [On open; on InventoryChanged] | [InventorySystem] | [Array of ItemData: id, name, icon, category, stats, is_equipped] | [Empty array → Empty State. Never null.] |
| [Equipped loadout] | [Equipment] | [On open; on EquipmentChanged] | [EquipmentSystem] | [Dict slot_id → item_id] | [Unequipped slot null — UI shows empty icon] |
| [Stat comparisons] | [Stats] | [On selection change] | [StatsSystem] | [Dict stat_name → {current, new, delta}] | [No selection → placeholder. Stats handles gracefully.] |
| [Currency] | [Economy] | [On open only — not live] | [EconomySystem] | [Int gold] | [Mode without economy → hide row] |
| [New item flag] | [Inventory] | [On open] | [InventorySystem] | [Array of new item_ids] | [Empty → no badges] |

> **Rule**: Screen NEVER writes directly. Actions fire events (Section 9). Systems update + notify.

---

## 9. Events Fired

> Other half of UI/system boundary. Specifying at design prevents UI doing game logic, prevents game surprised by UI.

| Player Action | Event | Payload | Receiver | Notes |
|---------------|-------|---------|----------|-------|
| [Equip item] | [EquipItemRequested] | [{item_id, target_slot}] | [Equipment] | [Validates, fires EquipmentChanged; UI listens] |
| [Drop item] | [DropItemRequested] | [{item_id, quantity}] | [Inventory] | [After confirm. Removes + fires InventoryChanged.] |
| [Open compare] | [ItemCompareOpened] | [{item_a_id, item_b_id}] | [Analytics] | [No state change. Local UI only.] |
| [Close screen] | [InventoryScreenClosed] | [{session_duration_ms}] | [Analytics] | [Every close. Engagement metric.] |
| [Change category] | [InventoryCategoryChanged] | [{category}] | [Analytics] | [No state change.] |

---

## 10. Transition & Animation

> Transitions communicate hierarchy + causality. Slide right = forward. Fade = context break. Inconsistent = broken feel. Plan for reduced motion from start.

| Transition | Trigger | Direction/Type | Duration (ms) | Easing | Interruptible? | Skipped by Reduced Motion? |
|------------|---------|---------------|--------------|--------|----------------|---------------------------|
| [Screen enter] | [Pushed onto stack] | [Slide from right] | [250] | [Ease out cubic] | [No — must complete] | [Yes — instant 0ms] |
| [Screen exit — Back] | [Back pressed] | [Slide to right] | [200] | [Ease in cubic] | [No] | [Yes — instant] |
| [Screen exit — Forward] | [Nav to child] | [Slide to left] | [200] | [Ease in cubic] | [No] | [Yes — instant] |
| [Detail panel update] | [New item selected] | [Cross-fade] | [120] | [Linear] | [Yes — cancels on rapid nav] | [Yes — instant swap] |
| [Loading → Populated] | [Data arrives] | [Skeleton fade out, content fade in] | [180] | [Ease out] | [No] | [Yes — instant] |
| [Action button press] | [Button activated] | [Scale 95% press, return release] | [60 down / 60 up] | [Ease out / in] | [Yes — early release returns] | [No — tactile, not decorative] |
| [Confirm dialog open] | [Destructive initiated] | [Bg dim 60%; dialog scale 95%→100%] | [150] | [Ease out] | [No] | [Yes — instant, no scale] |
| [New item badge] | [Open with new item] | [Badge 0%→110%→100% scale] | [200 total] | [Ease out back] | [No] | [Yes — instant 100%] |

---

## 11. Input Method Completeness Checklist

> Console cert + accessibility legal req. Fill before Approved. Unchecked blocks impl.

**Keyboard**
- [ ] All interactive reachable via Tab + arrows alone
- [ ] Tab order = visual reading order (LTR, top-bottom per zone)
- [ ] Every mouse action also keyboard
- [ ] Focus always visible
- [ ] Focus does not escape screen (focus trap for modals)
- [ ] Esc closes/cancels (not quits game)

**Gamepad**
- [ ] All reachable via D-Pad + left stick
- [ ] Face button mapping per platform (Section 7.2)
- [ ] No analog precision unreplicable by D-Pad
- [ ] Trigger/bumper shortcuts documented
- [ ] Controller disconnect handled gracefully

**Mouse**
- [ ] Hover states on all interactive
- [ ] Hit targets min 32x32px (44x44px preferred)
- [ ] Right-click defined (context or no-op, not undefined)
- [ ] Scroll wheel defined in scrollable zones

**Touch (if applicable)**
- [ ] Targets min 44x44px
- [ ] Swipe gestures don't conflict with system
- [ ] All actions one-hand portrait
- [ ] Long-press defined if used

---

## 12. Screen-Level Accessibility Requirements

> Specify at design — retrofit expensive. Project-wide in `design/accessibility-requirements.md`.
>
> Tiers:
> - Basic: WCAG 2.1 AA contrast, kb navigable, no motion-only info
> - Standard: Basic + screen reader, colorblind-safe, focus mgmt
> - Comprehensive: Standard + reduced motion, text scaling, high contrast
> - Exemplary: Comprehensive + cognitive load mgmt, AAA, certified

**Text contrast**:

| Text Element | Background | Required Ratio | Current | Pass? |
|--------------|-----------|---------------|---------|-------|
| [Item name in Detail] | [Dark panel ~#1a1a1a] | [4.5:1 WCAG AA] | [TBD] | [ ] |
| [Category tab — inactive] | [Mid-grey] | [4.5:1] | [TBD] | [ ] |
| [Category tab — active] | [Accent color] | [4.5:1] | [TBD] | [ ] |
| [Action button label] | [Button by state] | [4.5:1] | [TBD] | [ ] |
| [Stat delta (positive)] | [Detail panel] | [4.5:1 — NOT green alone] | [TBD] | [ ] |

**Colorblind-unsafe + mitigations**:

| Element | Risk | Mitigation |
|---------|------|------------|
| [Stat delta red/green] | [Deuteranopia, most common] | [Arrow icons (↑/↓) + +/- prefix. Color redundant.] |
| [Item rarity colors] | [Multiple types — common industry failure] | [Rarity name text label below icon. Color supplemental.] |

**Focus order** (Tab sequence):

[e.g.,
1. Back (Header)
2. Options (Header)
3. Category Tab 1 — Weapons
4. Category Tab 2 — Armor
5. Category Tab 3 — Consumables
6. Category Tab 4 — Key Items
7. Item Slot [0,0]
8. Item Slot [0,1] ... (LTR, top-bottom)
9. Last slot
10. Equip
11. Drop
12. Compare
13. Close
→ Cycles to Back

Detail Panel not focusable — driven by item focus.]

**Screen reader announcements**:

| State Change | Announcement | Timing |
|--------------|--------------|--------|
| [Screen opens] | ["Inventory screen. [N] items. [Category] selected."] | [Focus settle] |
| [Slot focused] | ["[Item]. [Category]. [Rarity]. [Stats summary]. [Equipped/Not]."] | [On focus] |
| [Equip] | ["[Item] equipped to [slot]."] | [After EquipmentChanged] |
| [Drop] | ["[Item] dropped."] | [After InventoryChanged] |
| [Category change] | ["[Category]. [N] items."] | [Tab focus] |
| [Empty state] | ["No items in [category]."] | [Empty renders] |

**Cognitive load assessment**:

[Estimate concurrent info streams. This screen: (1) grid position, (2) detail stats, (3) equipped loadout, (4) actions, (5) category. = 5 streams, within 7±2 but high. Mitigation: detail auto-updates on nav; surface stat compare automatically.]

---

## 13. Localization Considerations

> UI without loc planning breaks on translation. German +30-40%. Arabic/Hebrew RTL. CJK shorter. Cheap upfront, expensive after.

**General rules**:
- All text tolerates min 40% expansion from English
- RTL (Arabic, Hebrew): mirrored layout. Document mirror exceptions.
- CJK: text 20-30% shorter — verify layouts not broken with less
- No text in images — all from loc strings

| Text Element | English Length | Max Chars | Expansion Budget | RTL | Overflow | Risk |
|--------------|----------------|-----------|-----------------|-----|----------|------|
| [Title "INVENTORY"] | [9] | [16] | [78%] | [Mirror or center] | [Truncate ellipsis] | [Low] |
| [Item name] | [~15 avg, ~35 max] | [50] | [43%] | [Right-align] | [Truncate + tooltip with full name] | [Medium — long fantasy names] |
| [Item description] | [~80–120] | [200] | [67%] | [Right-align, wrap] | [Scroll in panel] | [Low — scrollable] |
| [Action "Equip"] | [5] | [14] | [180%] | [Mirror, right-align] | [Shrink 90% min, then truncate] | [Medium — German "Ausrüsten" = 9] |
| [Tab "Consumables"] | [11] | [18] | [64%] | [Mirror] | [Abbreviate "Consum." per loc file] | [High — long localized labels] |

---

## 14. Acceptance Criteria

> Contractual "done." Tester verifies independently. Binary pass/fail.

**Performance**
- [ ] First frame within 200ms on min-spec
- [ ] Fully interactive within 500ms on min-spec
- [ ] Item nav no perceptible frame drop (target ±5fps)

**Layout & Rendering**
- [ ] No overlap/cutoff/overflow at min resolution [specify]
- [ ] Same at max resolution [specify]
- [ ] Correct at 4:3, 16:9, 16:10, 21:9 if PC
- [ ] No text overflow in English
- [ ] No overflow in longest-translation language [typically German]
- [ ] All states (Loading/Empty/Populated/Error/Confirm) render correctly
- [ ] Item grid scrolls smoothly when fully populated

**Input**
- [ ] All reachable by kb (Tab + arrows only)
- [ ] All reachable by gamepad (D-Pad + face buttons only)
- [ ] All reachable by mouse without keyboard
- [ ] No undocumented simultaneous input
- [ ] Focus always visible on kb/gamepad
- [ ] Focus does not escape screen

**Events & Data**
- [ ] All Section 9 events fire correctly on all exit paths (debug log)
- [ ] No direct game system writes (verify: no state mutations)
- [ ] Inventory persists after close + reopen
- [ ] Handles InventoryChanged from other systems while open without crash

**Accessibility**
- [ ] All text meets contrast Section 12
- [ ] Stat compare not color-only
- [ ] Screen reader announces item + stats on focus (verify platform reader)
- [ ] Reduced motion = instant transitions
- [ ] High contrast renders without breakage (if Tier requires)

**Localization**
- [ ] No element overflows in any supported language
- [ ] RTL renders correctly (if RTL target)
- [ ] All text from loc strings — no hardcoded

---

## 15. Open Questions

> Unresolved questions. Owner + deadline. Approved spec = zero open questions or explicit deferral rationale.

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| [Item compare auto (always show equipped) or player-triggered?] | [ui-designer] | [Sprint 4, Day 3] | [Pending] |
| [Controller cursor in grid or d-pad-only?] | [lead-programmer + ui-designer] | [Sprint 4, Day 3] | [Pending — depends on ADR-0015] |
| [Item drop policy — permanent loss or drop-to-world?] | [systems-designer] | [GDD update] | [Blocked on inventory GDD Edge Cases] |
| [Max inventory size — hard cap or infinite scroll?] | [systems-designer] | [Sprint 3, Day 5] | [Pending] |
