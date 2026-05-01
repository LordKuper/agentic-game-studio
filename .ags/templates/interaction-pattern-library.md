# Interaction Pattern Library: [Game Title]

> **Status**: Draft | Stable | Under Revision
> **Author**: [ux-designer]
> **Last Updated**: [Date]
> **Version**: [1.0]
> **Engine**: Unity [version]
> **UI Framework**: Unity UI Toolkit (preferred) or uGUI
> **Related Documents**:
> - `docs/ags-art-bible.md` — visual standards
> - `design/accessibility-requirements.md` — accessibility commitments
> - `docs/ux/ux-spec-[screen].md` — screen specs that reference patterns

> Single source of truth for reusable interactions. Screen specs reference pattern names. Living doc — add new pattern before first use.
>
> **Status**:
> - **Draft**: Specified, not validated.
> - **Stable**: Implemented, tested, shipped.
> - **Deprecated**: Phasing out. Do not use in new screens.

---

## How to Use This Library

**Designing screen**: Browse Catalog Index. Reference pattern by name. New pattern needed → add here first.

**Implementing screen**: Look up pattern. Implementation Notes = engine guidance. Accessibility = non-negotiable.

**Reviewing spec**: All interactive elements MUST reference a pattern OR have full inline spec. "Standard button" not valid.

**Updating Stable pattern**: Audit usages. Get ux-designer approval. Update doc before/with implementation.

---

## Pattern Catalog Index

> Add row when adding pattern. Update "Used In" when adopted.

| Pattern Name | Category | Description | Used In (Screens) | Status |
|-------------|----------|-------------|------------------|--------|
| Button (Primary) | Input | Main CTA. High weight. One per screen. | [Main Menu, Pause, Settings] | Draft |
| Button (Secondary) | Input | Alt/cancel action. Lower weight. | [Modal dialogs, settings] | Draft |
| Button (Destructive) | Input | Irreversible. Requires confirmation. | [Delete Save, Reset] | Draft |
| Toggle | Input | Binary on/off. | [Accessibility, audio] | Draft |
| Slider | Input | Continuous value. | [Volume, brightness, text size] | Draft |
| Dropdown / Select | Input | Discrete list selection. | [Resolution, language, key bind] | Draft |
| List Item | Layout / Input | Selectable row in vertical list. | [Achievements, quest log, settings] | Draft |
| Grid Item | Layout / Input | Selectable cell in 2D grid. | [Inventory, ability select, shop] | Draft |
| Modal Dialog | Feedback / Layout | Blocking overlay needing decision. | [Confirmations, errors] | Draft |
| Confirmation Dialog | Feedback / Layout | Modal for destructive confirm. | [Delete Save, Leave Match] | Draft |
| Toast / Notification | Feedback | Non-blocking corner message. | [Achievement, autosave] | Draft |
| Tooltip | Feedback | Contextual hover/focus info. | [Items, abilities, settings] | Draft |
| Progress Bar | Feedback / Layout | Linear progress indicator. | [Loading, XP bar, quest progress] | Draft |
| Input Field | Input | Text entry. | [Player name, search, key bind] | Draft |
| Tab Bar | Navigation | Tabbed sections within screen. | [Character sheet, settings, crafting] | Draft |
| Scroll Container | Layout | Scrollable region. | [Inventory, lore, credits] | Draft |
| Inventory Slot | Game-Specific | Item container (empty/filled/equipped/locked). | [Inventory, equipment] | Draft |
| Ability / Skill Icon | Game-Specific | Ability button with cooldown/charges/locked. | [HUD ability bar, skill tree] | Draft |
| Health / Resource Bar | Game-Specific | Value bar with thresholds and damage flash. | [HUD] | Draft |
| Minimap | Game-Specific | Overview map with markers. | [HUD] | Draft |
| Quest / Objective Tracker | Game-Specific | Active objective display. | [HUD] | Draft |
| Dialogue Box | Game-Specific | NPC conversation UI. | [Dialogue sequences] | Draft |
| Context Action Prompt | Game-Specific | "Press X to [action]" near interactables. | [World interaction] | Draft |
| Damage Number | Game-Specific | Floating combat number. | [Combat HUD] | Draft |
| Status Effect Icon | Game-Specific | Buff/debuff with duration. | [HUD status, enemy display] | Draft |
| Notification Banner | Game-Specific | Achievement/level up/item acquired. | [Global overlay] | Draft |
| Screen Push | Navigation | Forward nav with directional anim. | [Menu navigation] | Draft |
| Screen Pop (Back) | Navigation | Back nav with reversed anim. | [Menu navigation] | Draft |
| Screen Replace | Navigation | Replace without stacking history. | [Main Menu to Loading] | Draft |
| Modal Open / Close | Navigation | Overlay dimming background. | [Modal dialogs] | Draft |
| Tab Switch | Navigation | Same-screen content swap. | [Tabbed screens] | Draft |
| Focus Management | Navigation | Focus rules for screens. | [All screens] | Draft |
| Escape / Cancel | Navigation | Universal back behavior. | [All screens] | Draft |
| Loading State | Feedback | Loading indicators. | [All loading] | Draft |
| Empty State | Feedback | Empty list/grid presentation. | [Empty inventory, no quests] | Draft |
| Error State | Feedback | Error communication. | [Save fail, network, invalid input] | Draft |
| Success Confirmation | Feedback | Completed action confirm. | [Settings saved, item crafted] | Draft |
| Optimistic UI | Feedback | Show success before confirmation. | [Online features] | Draft |

---

## Standard Control Patterns

---

#### Button (Primary)

**Category**: Input
**Status**: Draft
**When to Use**: Single most important action. "Start Game," "Confirm," "Buy." Max one visible at a time.
**When NOT to Use**: Alt/secondary actions. Destructive actions. Non-primary intent.

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Default | Full-opacity fill, primary color. Label centered. | — | — | — | — |
| Hovered (mouse) | Brightness +15%, scale 1.03x, pointer cursor | Mouse over | From Default | 80ms ease-out | [UI hover] |
| Focused (kb/gamepad) | Focus ring (2px, offset 3px, high contrast). Same brightness as Hovered. | Tab / D-pad | From Default | 80ms ease-out | [UI focus] |
| Pressed | Scale 0.97x, brightness -10% | Click / Enter / A / Cross | Action fires on press-up. Scale on press-down. | 60ms in / 80ms out | [UI confirm] |
| Disabled | 40% opacity, no pointer | — | None | — | — |
| Loading | Spinner replaces label. Button stays pressed/disabled. | — | Prevent double-submit | Async duration | — |

**Accessibility**:
- Keyboard: Tab to focus, Enter/Space to activate. Reachable via Tab from any element.
- Gamepad: D-pad/stick to focus. A (Xbox) / Cross (PS) activates. Default focus on screen open.
- Screen reader: Role "button". Name = label. State "dimmed" when disabled. Announce: "[Label] button — [result]."
- Colorblind: Higher visual weight (fill vs outline, size) plus color. Never color alone.
- Touch target: min 44x44pt iOS / 48x48dp Android.

**Implementation Notes**: Unity: UI Toolkit (preferred) or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Button (Secondary)

**Category**: Input
**Status**: Draft
**When to Use**: Alt/cancel. "Back," "Cancel," "Skip." Recedes visually.
**When NOT to Use**: Destructive actions (use Destructive). Most important action (use Primary).

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Default | Outlined, transparent fill, secondary color. Smaller/lower weight than Primary. | — | — | — | — |
| Hovered | Fill 15% opacity. Border brightens. Scale 1.02x. | Mouse over | From Default | 80ms ease-out | [UI hover, soft] |
| Focused | Focus ring (same as Primary). | Tab / D-pad | From Default | 80ms ease-out | [UI focus] |
| Pressed | Scale 0.97x, fill 30% | Click / Enter / B / Circle | Action on press-up | 60ms ease-in | [UI cancel/back] |
| Disabled | 40% opacity | — | None | — | — |

**Accessibility**: Same as Primary. In dialogs, Secondary maps to platform cancel (B / Circle / Esc).

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Button (Destructive)

**Category**: Input
**Status**: Draft
**When to Use**: Irreversible action. "Delete Save," "Reset Settings," "Leave Match," "Discard."
**When NOT to Use**: Reversible actions.

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Default | Destructive color (desaturated red — verify colorblind). Optional warning icon. | — | — | — | — |
| Hovered / Focused | Like Primary but destructive color | — | — | 80ms | [UI hover] |
| Pressed (first press) | Does NOT execute. Opens Confirmation Dialog. Brief pulse. | Click / Enter | Trigger Confirmation Dialog | 100ms pulse | [UI warning, distinct] |
| — | Confirmation Dialog handles execution | — | — | — | — |
| Disabled | 40% opacity | — | None | — | — |

> **Critical rule**: Destructive button NEVER executes directly. ALWAYS triggers Confirmation Dialog. No exceptions.

**Accessibility**: Screen reader: "[Label] button — this action cannot be undone." Use `description` for warning.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Toggle

**Category**: Input
**Status**: Draft
**When to Use**: Binary on/off, current state visible at glance. "Subtitles On/Off."
**When NOT to Use**: 3+ options (Dropdown). One-shot actions (Button). Complex consequences (add description).

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Off / Default | Track muted. Thumb left. Label "Off". | — | — | — | — |
| Hovered | Track +10% brightness. Pointer cursor. | Mouse over | Transition | 60ms | [UI hover] |
| Focused | Focus ring around track + thumb. | Tab / D-pad | — | 60ms | [UI focus] |
| Activated (On) | Thumb slides right. Track active color. Label "On". | Click / Enter / A / Cross | State change. Fire onChange. Persist. | 150ms ease-in-out | [Toggle ON] |
| Activated (Off) | Thumb slides left. Track muted. | Same | State change | 150ms ease-in-out | [Toggle OFF] |
| Disabled | 40% opacity. State visible. | — | None | — | — |

**Accessibility**:
- Space/Enter to toggle. Avoid directional inputs.
- Screen reader: Role "switch". State "on"/"off" — name does NOT include state. Correct: name "Subtitles", state "on".
- Label changes with state (not just thumb position) for left/right perception issues.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Slider

**Category**: Input
**Status**: Draft
**When to Use**: Continuous range, approximate values OK. Volume, brightness, text size.
**When NOT to Use**: Precise values (Input Field). Discrete short list (Dropdown). Binary (Toggle).

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Default | Track. Fill (left of thumb). Thumb. Value label. | — | — | — | — |
| Hovered | Thumb 1.2x. Track brightens. | Mouse over | — | 60ms | — |
| Focused | Focus ring on thumb. Track brightens. | Tab / D-pad | — | 60ms | [UI focus] |
| Dragging | Thumb follows cursor. Real-time fill + label. | Click + drag | Continuous onChange | Real time | [Slider adjust loop] |
| Step adjust | Thumb moves one step (5% or 1 unit). | Left/Right arrows or D-pad | Step change. onChange per step. | Instant | [Slider step] |
| Fast adjust | Larger step (25%). | Page Up/Down | Large step | Instant | [Same] |
| Released | Value locks. Final onChange. | Mouse release | — | — | — |
| Disabled | 40% opacity. Value visible. | — | None | — | — |

**Accessibility**:
- Left/Right = small step. PageUp/Down = large. Home/End = min/max.
- Screen reader: Role "slider". Name = label. Value announced on change: "Music Volume, 80 percent." Min/max on first focus.
- Always show numeric value alongside fill.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Dropdown / Select

**Category**: Input
**Status**: Draft
**When to Use**: 3-15 discrete options, only selected value visible at rest. Resolution, language.
**When NOT to Use**: Binary (Toggle). 15+ options (List/scrollable). Visible comparison needed.

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Closed / Default | Label (left). Value (right). Chevron-down. | — | — | — | — |
| Hovered | Row fill 10% | Mouse over | — | 60ms | — |
| Focused (closed) | Focus ring on row. | Tab / D-pad | — | 60ms | [UI focus] |
| Opening | List appears below (or above near bottom). Selected item highlighted/focused. | Click / Enter / A / Cross | Open list | 100ms expand | [UI expand] |
| List item hovered/focused | Highlights | Mouse / D-pad | — | 60ms | [UI hover] |
| Item selected | Closes. New value shown. onChange fires. | Click / Enter / A / Cross | Select, close | 80ms collapse | [UI confirm] |
| Dismissed | Closes. Value unchanged. | Esc / B / Circle / outside click | Dismiss | 80ms | [UI cancel] |
| Disabled | 40% opacity | — | — | — | — |

**Accessibility**:
- Up/Down arrows in list. Enter selects. Esc dismisses. First-letter jump.
- Screen reader: Role "combobox". Expanded/collapsed announced. Items: "English, 1 of 12."
- List MUST NOT obscure current item or opening control.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### List Item

**Category**: Layout / Input
**Status**: Draft
**When to Use**: Selectable row in vertical list. Achievements, quests, save slots.
**When NOT to Use**: 2D grid (Grid Item). Non-selectable rows (drop hover/focus/pressed).

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Default | Full-width row. Optional icon left. Primary label. Optional metadata. Optional chevron right. | — | — | — | — |
| Hovered | Row bg 12% highlight | Mouse over | — | 60ms | — |
| Focused | Focus ring OR row bg 20%. | D-pad / Tab | — | 60ms | [UI focus] |
| Selected (persistent) | Row bg 25%. Optional left-border or checkmark. Distinct from focused. | — | Rendered | — | — |
| Pressed | Brightness flash, then nav/action | Click / Enter / A / Cross | Nav or action | 80ms flash | [UI confirm] |
| Disabled | 40% opacity | — | — | — | — |

**Accessibility**:
- Up/Down or D-pad navigates. Reaching bottom stops (no wrap unless designed).
- Screen reader: Role "listitem", parent "list". Name = primary label. Position: "Quest Log, 3 of 12."
- Min row height: 44pt / 48dp touch. 56px for controller-primary.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Grid Item

**Category**: Layout / Input
**Status**: Draft
**When to Use**: 2D grid cell. Inventory, ability select, crafting, portraits.
**When NOT to Use**: Single column (List Item). Non-selectable display cells.

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Empty | Subtle border or dashed outline. Distinct from disabled. | — | — | — | — |
| Populated | Item icon. Stack count bottom-right. Quality border/overlay. | — | — | — | — |
| Hovered | Brightness +15%. Tooltip after 400ms. | Mouse over | — | 60ms | — |
| Focused | Focus ring (2px, offset 2px). Tooltip after 400ms (300ms gamepad). | D-pad | — | 60ms | [UI focus] |
| Selected (persistent) | Distinct border (thicker, contrasting). Optional checkmark. | Click / Enter / A / Cross | Select. Coexists with focus. | Instant | [UI select] |
| Pressed | Scale 0.95x, then action | Double-click / Enter / A / Cross | Action (equip/use/inspect) | 80ms | [UI confirm] |
| Locked | Padlock overlay. No interaction. | — | None | — | — |
| Drag source | 50% opacity, drag preview at cursor. | Click + drag (mouse) | Begin drag | Instant | [UI grab] |
| Drop target (valid) | Brightens, accept color | Item over cell | — | 60ms | — |
| Drop target (invalid) | Red tint or shake | Invalid drop | — | 60ms | [UI error] |

**Accessibility**:
- D-pad/arrows navigate. Grid dimensions communicated. Row/col announced.
- Screen reader: Role "gridcell", parent "grid". Name = item or "empty slot". State "selected"/"dimmed". Position: "row 2, column 3."
- Tooltips MUST appear on focus, not only hover.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Modal Dialog

**Category**: Feedback / Layout
**Status**: Draft
**When to Use**: Decision/ack must resolve before continuing. Background dimmed, non-interactive.
**When NOT to Use**: Non-blocking (Toast). Deferrable info (help system). Background should remain interactive.

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Opening | Overlay 0→60% opacity. Dialog scale 0.9→1.0. Center entry. | Code | Focus to first interactive (or Primary) | 200ms ease-out | [UI modal open] |
| Active | Background non-interactive. Input within dialog only. | Kb/gamepad in dialog | — | — | — |
| Dismissing (confirmed) | Scale to 1.1, fade. Overlay fades. | Primary pressed | Execute, return focus to trigger | 180ms | [UI confirm] |
| Dismissing (cancelled) | Scale to 0.9, fade. Overlay fades. | Secondary / Esc / B / Circle | No action, return focus | 150ms | [UI cancel] |
| Cannot dismiss | Blocking error: provide only resolution, no cancel. | — | — | — | — |

> **Focus trap rule**: Tab/D-pad cycle within dialog only. WCAG 2.1 SC 2.1.2. On close, focus returns to trigger.

**Accessibility**:
- Role "dialog". Name = title (required, even if hidden visually). Announce title + first focusable on open. Focus trap.
- Esc = cancel. Enter = primary.
- Motion reduction: instant appear/disappear. Overlay fade 100ms.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Confirmation Dialog

**Category**: Feedback / Layout
**Status**: Draft
**When to Use**: Confirm destructive action. Triggered by Button (Destructive). Always 2 options: confirm (specific action) + cancel.
**When NOT to Use**: Non-destructive. Notifications without decision. 3+ actions.

> **Label rule**: Confirm button labeled with specific action, NOT "OK" or "Yes". "Delete Save File" not "OK". Apple HIG.

**Structure**:
- Title: action-describing. "Delete save file?" not "Are you sure?"
- Body: one sentence consequence. "This cannot be undone."
- Confirm: Button (Primary), specific action label.
- Cancel: Button (Secondary), "Cancel."
- Default focus: Cancel.

**Accessibility**: Inherits Modal Dialog. Announce "Alert dialog, [title]". Default focus on Cancel = requirement.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Toast / Notification

**Category**: Feedback
**Status**: Draft
**When to Use**: Brief non-blocking info. "Game saved." "Achievement unlocked." Auto-disappears.
**When NOT to Use**: Decisions (Modal). Errors needing action. Critical info player must not miss.

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Entering | Slide from edge (typically bottom-right). Fade 0→100%. | Code | — | 200ms ease-out | [Sound by type] |
| Displayed | Full opacity. Optional icon, title, body, dismiss X. | Hover pauses timer | Pause auto-dismiss | — | — |
| Auto-dismiss | Fade + slide out | Timer (5s 1-line, 8s 2-line) | Remove | 200ms ease-in | — |
| Manual dismiss | Fade + slide immediately | Click X / swipe | Remove | 150ms | [UI cancel, quiet] |
| Queue overflow | New pushes oldest out | New while displayed | FIFO, max 3 simultaneous | — | — |

**Accessibility**:
- Read aloud without focus. HTML: `role="status"` or `role="alert"`. Verify engine support.
- Motion reduction: fade only, no slide.
- Never sole channel for actionable info. Pair with persistent UI.
- Auto-dismiss min 5s. Consider 10-15s setting for cognitive accessibility.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Tooltip

**Category**: Feedback
**Status**: Draft
**When to Use**: Supplemental info. Item descriptions, stat explanations. Non-blocking.
**When NOT TO Use**: Required reading (use label/body). Touch-only platforms (use info button + modal).

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Hidden | — | — | — | — | — |
| Hover trigger | — | Mouse enters | 400ms delay timer | — | — |
| Gamepad/kb trigger | — | Element focused | 300ms delay timer | — | — |
| Appearing | Fade in, scale 0.95→1.0. Above element (adjust near edge). | Timer expires | Show | 120ms ease-out | — |
| Displayed | Optional title. Body. Max 300px. Multi-line OK. | — | — | — | — |
| Hiding | Fade out | Mouse leaves / focus moves | Hide | 80ms ease-in | — |

**Accessibility**:
- Reachable without hover. Critical info in parent's accessible name. Full text in `description`. Read on focus.
- Delay (300-400ms) required — instant tooltips disrupt gamepad nav.
- Contrast 4.5:1 min.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Progress Bar

**Category**: Feedback / Layout
**Status**: Draft
**When to Use**: Linear progress to defined endpoint. Loading, XP, "3 of 10 enemies."
**When NOT to Use**: Circular/radial. Fluctuating values (Health/Resource Bar). No defined endpoint.

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Default | Track. Fill left-to-right. Value label (% or N/M). | — | — | — | — |
| Increasing | Fill animates to new value | Value changes | Smooth fill | 300ms ease-out | [Context-dependent] |
| At max | Full fill. Optional pulse/glow. | Reaches 100% | Completion event | 200ms | [Completion if appropriate] |
| At zero | Fill hidden. Track visible. | — | — | — | — |
| Indeterminate | Animated loop segment. Used for unknown duration. | — | — | Loop | — |

**Accessibility**:
- Role "progressbar". Name = what's progressing. Value: numeric + percentage + max. "Experience Points, 450 of 1000, 45 percent."
- Numeric label always; not color-only.
- Indeterminate: announce "Loading, in progress" only.
- Motion reduction: indeterminate → static "loading". Smooth fill → instant jump.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Input Field

**Category**: Input
**Status**: Draft
**When to Use**: Text entry. Player name, search, key remapping, numeric value.
**When NOT to Use**: Known options (Dropdown/List). Console — minimize text entry (virtual keyboard friction).

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Default | Border. Placeholder muted. Empty input. | — | — | — | — |
| Hovered | Border brightens | Mouse over | — | 60ms | — |
| Focused | Border bright. Cursor blinks 530ms. Placeholder hidden. | Tab / click | Open virtual kb on console/mobile | Instant | [UI focus] |
| Typing | Chars appear. Cursor advances. | Keyboard | Update value | Immediate | [Keystroke, optional] |
| Value present | Field shows value. Clear X right of field. | — | — | — | — |
| At limit | No more chars. Optional shake + indicator color. | Input at limit | Reject | 200ms shake | [UI error, subtle] |
| Clear | Empties. Cursor returns. X disappears. | Click X / clear input | Clear | Instant | [UI cancel, subtle] |
| Validation error | Border error color (verify colorblind). Error msg below. | On submit/blur | Show error | Instant | [UI error] |
| Validated | Border success color. Optional success icon. | On pass | — | Instant | — |
| Disabled | 40% opacity. Value visible. | — | — | — | — |

**Accessibility**:
- All standard text shortcuts (Home, End, Ctrl+A/C/V/Z).
- Role "textbox". Name = label (NOT placeholder). Value announced. Limit announced when reached. Errors announced immediately.
- Visible label required — placeholder cannot be sole label.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Tab Bar

**Category**: Navigation
**Status**: Draft
**When to Use**: Single screen split into discrete sections, one visible at time. Max 5-6 tabs.
**When NOT to Use**: 6+ tabs. Simultaneous visibility needed. Different screens (Screen Push).

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Default (inactive) | Tab label. No indicator. | — | — | — | — |
| Active | Label + active indicator (underline/fill/bg). Content shows. | — | — | — | — |
| Hovered (inactive) | Bg fills slightly | Mouse over | — | 60ms | — |
| Focused | Focus ring on label. | Tab key (within bar) or D-pad L/R | — | 60ms | [UI focus] |
| Activated | Indicator transitions. Content fades/slides. | Click / Enter / A / Cross | Switch tab | 150ms ease | [UI tab switch] |
| Shoulder button | — | L1/R1 / LB/RB | Prev/next tab | 150ms | [UI tab switch] |

**Accessibility**:
- Arrows nav within bar (L/R). Tab key enters content. ARIA tab panel pattern.
- Roles: "tab", "tablist", "tabpanel". Active state "selected". Tabpanel labeled by tab.
- Active tab distinguished beyond color (underline/fill/weight).

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Scroll Container

**Category**: Layout
**Status**: Draft
**When to Use**: Content exceeds container. Inventory, lore, credits, settings.
**When NOT to Use**: Pagination clearer. Infinite scroll without loading/end states.

**Interaction Specification**:

| State | Visual | Input | Response | Duration | Audio |
|-------|--------|-------|----------|----------|-------|
| Content fits | No scrollbar (or always-visible at full). | — | — | — | — |
| Scrollable | Scrollbar appears (right). Thumb size = viewport/content ratio. | — | — | — | — |
| Scrolling (mouse) | Content moves. Thumb proportional. | Wheel | 3 lines per tick (OS configurable) | Smooth | — |
| Scrollbar drag | Content moves. Thumb follows. | Click + drag thumb | Proportional | Real time | — |
| Keyboard scroll | One item per press. | Up/Down when container focused, no child focused | One unit | Immediate | — |
| Gamepad scroll | Auto-scroll to keep focus visible. | D-pad to off-screen items | Auto-scroll | 150ms | — |
| Boundary | Stops. Thumb at end. | Boundary reached | Stop | — | — |
| Focus follows scroll | Auto-scroll focused child into view. | Child focused | Reveal | 200ms ease | — |

**Accessibility**:
- Auto-scroll on focus — no explicit scrollbar interaction needed.
- Screen reader: announce "scrollable" + position ("showing 5-15 of 30"). Verify engine support.
- Fade edges = visual aid only. Always include scrollbar.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

## Game-Specific UI Patterns

---

#### Inventory Slot

**Category**: Game-Specific
**Status**: Draft
**When to Use**: Every item container in inventory grid.

**States**:

| State | Visual | Notes |
|-------|--------|-------|
| Empty | Subtle border, no content. Interactable (receives items). | Avoid invisible empty slots — players lose grid dimensions |
| Populated | Item icon 80% slot. Stack count bottom-right. Quality border (icon + color). Equipped badge top-right. | |
| Focused | Focus ring. Tooltip after 300ms. | |
| Selected | Thicker/contrasting border. Multi-select. | |
| Drag source | Slot dims, ghost follows pointer. | See Grid Item |
| Locked | Padlock overlay. No interaction. Item at 50% opacity. | DLC, locked loadout |
| Highlighted | Animated glow pulse. Quest/new items. | Motion reduction → static badge |
| Cooldown overlay | Radial fill from 12 o'clock CW. | Active items with cooldowns |

**Accessibility**: Stack/quality need text or icon backups. Tooltip = keyboard + screen reader reachable. Locked announces "locked".

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Ability / Skill Icon

**Category**: Game-Specific
**Status**: Draft
**When to Use**: HUD ability bar, skill tree nodes.

**States**:

| State | Visual | Notes |
|-------|--------|-------|
| Available | Full opacity. Keybind label below. | |
| On cooldown | Radial overlay CW from 12. Number in center when >2s. | |
| Charges remaining | Charge pip indicators below. Number for screen readers. | |
| Out of resource | Desaturate to ~20%. Border dims. Distinct from cooldown. | |
| Locked | Silhouette only. Padlock badge. Unlock condition in tooltip. | |
| Active / channeling | Pulsing border. Radial fill = channel duration. | |
| Just activated | Scale 0.9x → 1.0x (overshoot 1.05x). | Respect motion reduction. |

**Accessibility**: Cooldown/charge need numeric value (screen reader can't parse radial). Names + descriptions in tooltip.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Health / Resource Bar

**Category**: Game-Specific
**Status**: Draft
**When to Use**: Continuously varying critical resource. Health, mana, stamina, shield.

**States and behaviors**:

| Event | Visual | Audio | Duration |
|-------|--------|-------|---------|
| Damage | Fill shrinks. Damage flash. Ghost bar drains over 0.5s. | [Damage by amount] | Instant + 500ms ghost |
| Heal | Fill grows. Heal color flash (green + icon/glow backup). | [Heal] | 300ms ease-in |
| Below 25% | Warning fill color. Border pulses (static badge in motion reduction). Optional heartbeat audio. | [Low health loop] | Continuous |
| At zero | Empty bar. Optional shake. Death event. | [Death] | 200ms shake |
| Maximum | 100%, brief glow. | — | 200ms |
| Overflow (shield) | Separate bar segment beyond fill, shield color. | [Shield gain] | 200ms |

**Accessibility**: Numeric value accessible (tooltip or persistent). Threshold states need non-color backup. 25% warning needs visual signal independent of color.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Dialogue Box

**Category**: Game-Specific
**Status**: Draft
**When to Use**: Any speaker-attributed dialogue.

**Structure**: Speaker portrait/name. Body text. Continue prompt. Optional: skip-all, voice indicator, subtitle indicator.

**States and behaviors**:

| State | Visual | Input | Response | Duration |
|-------|--------|-------|----------|---------|
| Line entering | Typewriter reveal. Or fade-in (accessibility setting). | — | — | Speed configurable |
| Revealing | Animating. Continue hidden/dim. | Any advance input | Skip to end of line | Immediate |
| Line complete | Full line. Continue visible/animated. | — | — | — |
| Advancing | Continue hides. Text fades/wipes. New line. | Enter / A / Cross / Space / click | Advance | 100ms |
| Choices appearing | Choice buttons below. Continue hidden. Focus to first choice. | D-pad / kb to select, Enter / A / Cross to confirm | Select | 150ms enter |
| Closing | Box fades | Final line advanced | Return control | 200ms |
| Skipping all | Confirmation: "Skip dialogue?" | Dedicated skip | Skip | — |

**Accessibility**: Subtitles ON by default for all voiced. Typewriter speed = user setting. NEVER auto-advance — player paces. Speaker name always shown. All choices kb/gamepad navigable. Position announced to screen reader.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Context Action Prompt

**Category**: Game-Specific
**Status**: Draft
**When to Use**: Prompt near interactable. "Press [A] to open chest."

**States**:

| State | Visual | Notes |
|-------|--------|-------|
| Appearing | Fade in + rise 8px. | Motion reduction: fade only. |
| Idle | Platform-correct icon + label. Updates if input method changes. | Never hardcode "Press A". |
| Holding | Radial fill on icon = hold progress. Label active verb ("Opening..."). | |
| Cannot interact | Icon dims. Reason if known ("Too heavy", "Need key"). | Optional. |
| Disappearing | Fade out. | Player exits zone. |

**Accessibility**: Icon + text label always (custom button labels, adaptive controllers). Position avoids HUD overlap.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Damage Number

**Category**: Game-Specific
**Status**: Draft
**When to Use**: Floating combat feedback above participants.

**Variants**:

| Variant | Visual | Notes |
|---------|--------|-------|
| Normal | White, normal weight, medium size. | |
| Critical | 1.5x size, bold, orange/yellow (verify colorblind). Scale impact 1.3x → 1.0x. | Recognizable by size alone. |
| Healing | Green (+ prefix and upward trajectory backups). | |
| Miss/Evade | "MISS", grey italic, smaller. | |
| Status (DoT) | Smaller, status-effect color. | |

**Behavior**: Float upward 1.0s. Fade last 0.4s. Stagger horizontally to avoid overlap. Max simultaneous: [define per game — 8-12 per character].

**Accessibility**: Supplementary only — health bars authoritative. Provide disable option. Game must remain fully playable when disabled.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

## Navigation Patterns

---

#### Screen Push / Pop / Replace

**Category**: Navigation
**Status**: Draft

| Pattern | Trigger | Animation | Stack | Focus |
|---------|---------|-----------|-------|-------|
| Push | Open submenu/detail | New slides from right. Previous dims, slides left. | Previous remains | Focus to first interactive on new |
| Pop (Back) | Back / Esc / B / Circle | Current slides right out. Previous slides from left, brightens. | Current removed | Focus to triggering element |
| Replace | Peer screen, loading | Fade out, fade in. No directional bias. | Current removed, new added | Focus to first interactive on new |

**Durations**: Push/Pop 250ms ease-in-out. Replace 200ms fade out + 200ms fade in.

**Motion reduction**: Slides → fades. Duration -50%.

**Implementation Notes**: Unity: UI Toolkit or uGUI. See `.ags/docs/engine-reference/unity/modules/ui.md`.

---

#### Focus Management

**Category**: Navigation
**Status**: Draft

> Most common kb/gamepad accessibility failure. Implement consistently.

| Rule | Description |
|------|-------------|
| Screen open | Focus on most logical interactive — Primary, first list item, or last-focused. Never non-interactive. |
| Screen close/pop | Focus to triggering element. If gone, nearest preceding interactive. |
| Modal open | Trapped inside modal. |
| Modal close | Returns to triggering element. |
| Element disabled | Focus to next available in tab order. |
| Element destroyed | Focus to nearest preceding in tab order. |
| Screen without interactive | No-op. Back/cancel still works. |
| Tab key | Forward through interactive in document order. Shift+Tab back. |
| D-pad | Spatial direction. Spatial preferred over tab order. Never wrap between unrelated regions. |
| Focus always visible | Focus ring ALWAYS visible on kb/gamepad focus. Never suppress. |

---

#### Escape / Cancel

**Category**: Navigation
**Status**: Draft

> Most-used menu input. Consistent across all screens.

| Platform | Input | Behavior |
|----------|-------|---------|
| PC kb | Escape | Close top modal / back one screen / at root open "quit?" confirm |
| PC gamepad | B (Xbox) / Circle (PS) | Same as Escape |
| Xbox | B | Same |
| PlayStation | Circle | Same |
| Switch | B | Same (NOTE: Nintendo first-party uses B for confirm sometimes — verify and document) |

**Rules**: NEVER override to non-back action. No back action available → does nothing or "must choose" message. Every screen defines Escape behavior in spec.

---

## Feedback and Loading Patterns

---

#### Loading State

**Category**: Feedback
**Status**: Draft

| Scope | Pattern | Notes |
|-------|---------|-------|
| Full screen (initial) | Loading screen + art + progress bar (determinate if possible) + tip text. | Never empty black. |
| Full screen (transition) | Fade to black, loading, fade in. | Removes scene pop. |
| Component / inline | Spinner or skeleton. No layout shift on load. | Skeleton preferred. |
| Background / async | No indicator unless >2s. Then small spinner/toast. | <2s indicator more disruptive than waiting. |

**Accessibility**: Announce "[Context] loading, please wait." Then "[Context] loaded." Tips and UI on loading screen exposed to screen readers.

---

#### Empty State

**Category**: Feedback
**Status**: Draft

> Empty != error. Designed starting point.

| Location | Content | Notes |
|----------|---------|----|
| Inventory | Icon. "Your inventory is empty." Sub: "Items you find on your journey will appear here." | "Found" implies failed search — avoid. |
| Quest Log | Icon. "No active quests." Sub: "Talk to characters marked with [icon] to start a quest." | Give clear action. |
| Achievements | Icon. "No achievements yet." Hints: "Try [Action] to earn your first." | Gamified motivation. |
| Search | Icon. "No results for '[term]'." Sub: "Try a different search or [browse all]." | Mirror term + alternative action. |

**Rule**: Every empty state MUST include icon + message + (sub-message OR action button). Blank container never acceptable.

---

#### Error State

**Category**: Feedback
**Status**: Draft

| Type | Pattern | Tone |
|-----|---------|------|
| Input validation | Inline error below field. Icon left. Red border (icon backup). | Specific: "Username must be 3-20 characters." Not "Invalid input." |
| Operation failed | Toast for non-critical. Modal for critical (save fail). | Calm, actionable: "Save failed. Check storage space." |
| System error | Full-screen with code, recovery options, support contact. | Reassuring. Never blame player. |
| Soft error | Toast or inline. | Explanatory: "Not enough gold." Not "Action unavailable." |

**Principle**: Errors never player's fault. Tell what happened + what next. Remove "invalid" — replace with specifics.

---

## Animation Standards

> Apply to ALL patterns. Consistency = system feel.

| Animation | Duration (ms) | Easing | Notes |
|-----------|--------------|--------|-------|
| Button hover/focus enter | 80 | ease-out | Snappy |
| Button hover/focus exit | 60 | ease-in | Faster exit |
| Button press scale down | 60 | ease-in | Immediate |
| Button press scale up | 80 | ease-out | Bouncy |
| Screen push enter | 250 | ease-in-out | Slide from right |
| Screen pop exit | 250 | ease-in-out | Slide to right |
| Modal open | 200 | ease-out | Expand from center |
| Modal close | 150 | ease-in | Collapse faster |
| Toast enter | 200 | ease-out | Slide from edge |
| Toast exit | 200 | ease-in | |
| Tab switch | 150 | ease-in-out | Cross-fade or slide |
| Tooltip appear | 120 | ease-out | After 300-400ms delay |
| Tooltip disappear | 80 | ease-in | |
| Progress bar fill | 300 | ease-out | Smooth value changes |
| Value flash | 100 on + 100 off | linear | Brief |
| Dialogue text reveal | 30 per char | linear | Configurable |
| HUD damage flash | 80 | linear | White/red overlay |

**Motion reduction**: Slides/scales → fades. Duration -50%. Looping → static equivalents.

---

## Sound Standards

> Sound = primary feedback channel. Categories defined here, assets in `docs/sound-bible.md`.

| Event | Sound Category | Notes |
|-------|---------------|-------|
| Button hover/focus | UI Hover | <80ms, non-fatiguing on rapid nav. |
| Button (Primary) confirm | UI Confirm — Primary | "Yes, let's go" sound. |
| Button (Secondary) cancel/back | UI Cancel | Subtly downward pitch. |
| Button (Destructive) opens confirm | UI Warning | Distinct from confirm. |
| Confirmation — confirm destructive | UI Confirm — Destructive | Final, weighted. |
| Toggle ON | UI Toggle On | Brief, snappy, bright. |
| Toggle OFF | UI Toggle Off | Same family, flatter. |
| Slider adjust | UI Slider | Subtle continuous drag. Single click per D-pad step. |
| Dropdown open | UI Expand | Directional. |
| Dropdown close/select | UI Select | Confirmation feel. |
| Tab switch | UI Tab | Horizontal feel. |
| Modal open | UI Modal Open | Prominent. |
| Modal close (cancel) | UI Modal Close | Return-to-context. |
| Toast informational | UI Notification | Background-level. |
| Toast achievement | UI Achievement | Celebratory, brief. |
| Toast warning | UI Warning — Toast | Alert, not alarming. |
| Error state | UI Error | Friendly clear. Not buzzer. |
| Success | UI Success | Clean, satisfying. |
| Ability activate | Gameplay — Ability Activate | In-world feel. |
| Damage received | Gameplay — Damage | See sound-bible.md. |
| Item pickup | Gameplay — Item Acquire | Brief, rewarding. |
| Level up | Gameplay — Progression | Celebratory. |
| Dialogue advance | UI Dialogue | Subtle, matches typewriter. |

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| [Engine accessibility — screen reader for toasts without focus? Verify `.ags/docs/engine-reference/unity/`.] | [ux-designer] | [Before first menu impl] | [Unresolved] |
| [Switch confirm/cancel mapping? Nintendo first-party differs.] | [producer] | [Before cert submission] | [Unresolved] |
| [Damage numbers pooled or dedicated render target? Verify perf budget with technical-director.] | [lead-programmer, ux-designer] | [Before combat HUD impl] | [Unresolved] |
| [Max simultaneous toasts before overwhelming? Playtest.] | [ux-designer] | [First playtest] | [Unresolved] |
| [Add question] | [Owner] | [Deadline] | [Resolution] |
