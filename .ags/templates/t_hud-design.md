---
status: draft        # draft | approved
approved_at:         # YYYY-MM-DD; required when status: approved
---

# HUD Design: [Game Name]

> **Precondition**: cited UX-spec(s) AND `design/art/DESIGN.md` must have `status: approved` (auto-checked by `/ags-ux-design hud`). See `.ags/rules/document-boundaries.md`.
> **Author**: [Name or agent — e.g., ui-designer]
> **Last Updated**: [Date]
> **Game**: [Single doc per game]
> **Platform Targets**: [PC, PS5, Xbox Series X, Steam Deck...]
> **Related GDDs**: [Every system exposing HUD info — `design/gdd/combat.md`, `progression.md`, `quests.md`]
> **Accessibility Tier**: Basic | Standard | Comprehensive | Exemplary
> **Style Reference**: [`design/gdd/ags-art-bible.md § HUD Visual Language`]
> **Design Tokens**: `design/art/DESIGN.md` (DESIGN.md spec) — all visual values (color, typography, spacing, radii, component styles) referenced as `{colors.x}` / `{typography.y}` / `{spacing.z}` / `{components.w}`. No raw hex / px / pt values in this doc.

> **Scope**: Overlays during active gameplay — health, ammo, minimap, quest, subtitles, damage numbers, toasts. Menus/pause/inventory/dialogs → `ux-spec.md`. Test: appears while player controls character → here.

---

## 1. HUD Philosophy

> Design constraint, measured against every decision. Without philosophy, individual elements creep. Write before specifying any element.

**Game's relationship with on-screen info?**

[One paragraph. Design statement, not feature list. Consider genre, pacing, fantasy.

Stealth: "World is the interface. Looking away = HUD failed."
Tactics: "Complete situational awareness IS the game. HUD = battlefield."

Example diegetic-first action RPG: "Screen info = concession not feature. Each element earns space by answering: would player make worse decisions without this visible? If 'they'd adapt,' put in environment."]

**Visibility principle** — when in doubt, show or hide?

[State default for ambiguous cases:
- HIDE: on demand (Dark Souls — no quest tracker, no minimap, stats in menu)
- SHOW: cluttered better than uncertain
- CONTEXTUAL: appears when relevant, fades when not
Most games benefit from contextual default.]

**Rule of Necessity**:

[Complete: "HUD element earns place when ______________."

Examples:
- "...player would stop playing to find info elsewhere, or make worse decisions without it."
- "...removing in playtest causes frustration/confusion in >25% of testers in first hour."

Rule = veto for feature requests. Cite in reviews.]

---

## 2. Information Architecture

> Forcing function: categorize EVERY game info type, explicit decision per type. "Figure out later" = 18 elements competing for peripheral vision.

| Info Type | Always Show | Contextual | On Demand | Hidden (diegetic) | Reasoning |
|-----------|-------------|-----------|-----------|-------------------|-----------|
| [Health] | [X if action] | [X if exploration — show only when injured] | [ ] | [ ] | [Always visible: retreat/heal decisions instant in combat] |
| [Primary resource (mana/stamina/ammo)] | [ ] | [X — when consumed or critically low] | [ ] | [ ] | [Stable levels not decision-relevant] |
| [Secondary resource (currency/materials)] | [ ] | [ ] | [X — inventory] | [ ] | [Totals don't affect immediate decisions] |
| [Minimap/Compass] | [X] | [ ] | [ ] | [ ] | [Constant during exploration] |
| [Quest objective] | [ ] | [X — on change or near location] | [ ] | [ ] | [Player remembers; remind at key moments] |
| [Enemy health] | [ ] | [X — combat only] | [ ] | [ ] | [Irrelevant outside combat] |
| [Status effects] | [ ] | [X — when active] | [ ] | [ ] | [Affect decisions only when present] |
| [Subtitles] | [X when dialogue plays] | [ ] | [ ] | [ ] | [Accessibility requirement] |
| [Combo/streak] | [ ] | [X — active, hide on reset] | [ ] | [ ] | [Active performance, not baseline] |
| [Timer] | [ ] | [X — timed sequences] | [ ] | [ ] | [Specific encounters only] |
| [Tutorial prompts] | [ ] | [X — first-time only] | [ ] | [ ] | [Never repeat to experienced] |
| [Score/points] | [ ] | [X — score-relevant modes] | [ ] | [ ] | [Hidden where irrelevant] |
| [XP/level progress] | [ ] | [ ] | [X — character screen] | [ ] | [Doesn't affect in-moment decisions] |
| [Waypoint/objective marker] | [ ] | [X — when navigating] | [ ] | [ ] | [Suppress in cutscenes, free exploration] |

---

## 3. Layout Zones

> World = primary content, HUD = frame. Define zones with positions + safe margins before placing elements. Prevents (1) ad-hoc clutter, (2) cert rejection from safe zone violations.

### 3.1 Zone Diagram

```
[Customize to your game. Axes = approx screen %.]

 0%                                             100%
 ┌──────────────────────────────────────────────────┐  0%
 │  [SAFE MARGIN — 10% from edge all sides]         │
 │  ┌────────────────────────────────────────────┐  │
 │  │ [TOP-LEFT]              [TOP-CENTER]  [TOP-RIGHT] │  ~15%
 │  │  Health, resource       Quest name    Ammo, mag │
 │  │                                              │  │
 │  │               [CENTER-SCREEN]               │  │  ~50%
 │  │                Crosshair / reticle           │  │
 │  │               (minimize HUD here)            │  │
 │  │                                              │  │
 │  │ [BOTTOM-LEFT]     [BOTTOM-CENTER]   [BOTTOM-RIGHT] │  ~85%
 │  │  Minimap          Subtitles          Notifications │
 │  │  Ability icons    Tutorial prompts             │  │
 │  └────────────────────────────────────────────┘  │
 │                                                  │
 └──────────────────────────────────────────────────┘  100%
```

> Center 40% (H+V) = primary focus area. Keep clear. Center elements (crosshair, prompts, hit markers) minimal, high-contrast, brief.

### 3.2 Zone Specification Table

| Zone | Position | Safe Zone OK | Primary Elements | Max Simultaneous | Notes |
|------|----------|--------------|-----------------|------------------|-------|
| [Top Left] | [Top-left, safe margin] | [Yes — 10% top, 10% left] | [Health, stamina, shield] | [3] | [Vital status. Player resources priority.] |
| [Top Center] | [Top edge, centered] | [Yes — 10% top] | [Quest objective, area name] | [1] | [Narrative context. Minimal text.] |
| [Top Right] | [Top-right, safe margin] | [Yes — 10% top, 10% right] | [Ammo, ability cooldowns] | [2] | [Weapon/ability state.] |
| [Center] | [Screen center ±15%] | [N/A] | [Crosshair, interaction prompt, hit marker] | [1] | [CRITICAL: nothing persistent. Momentary only.] |
| [Bottom Left] | [Bottom-left, safe margin] | [Yes — 10% bottom, 10% left] | [Minimap, ability icons] | [2] | [Navigation + abilities. Small.] |
| [Bottom Center] | [Bottom edge, centered] | [Yes — 10% bottom] | [Subtitles, tutorial prompts] | [2 — subtitle + tutorial] | [Highest accessibility zone.] |
| [Bottom Right] | [Bottom-right, safe margin] | [Yes — 10% bottom, 10% right] | [Notifications, pickups] | [3 stacked] | [Transient. Stack vertically. Oldest disappears first.] |

**Safe zone margins by platform**:

| Platform | Top | Bottom | Left | Right | Notes |
|----------|-----|--------|------|-------|-------|
| [PC windowed] | [0%] | [0%] | [0%] | [0%] | [Min res 1280x720 — don't crowd] |
| [PC fullscreen] | [3%] | [3%] | [3%] | [3%] | [4K TV-connected PCs] |
| [Console TV] | [10%] | [10%] | [10%] | [10%] | [Action-safe. Some TVs overscan beyond.] |
| [Steam Deck] | [5%] | [5%] | [5%] | [5%] | [Smaller screen, higher crowd risk] |
| [Mobile portrait] | [15% top] | [10% bot] | [5%] | [5%] | [15% top = notch/camera] |
| [Mobile landscape] | [5%] | [5%] | [15% L] | [15% R] | [Thumb obscures sides] |

---

## 4. HUD Element Specifications

> Each element needs spec to build correctly. Ad-hoc = inconsistent sizing, mismatched updates, missing urgency, accessibility fails.

### 4.1 Element Overview Table

> One row per element. Master inventory.

| Element | Zone | Always Visible | Visibility Trigger | Data Source | Update Freq | Max Size (% W) | Min Readable | Overlap Priority | Accessibility Alt |
|---------|------|---------------|-------------------|-------------|-------------|----------------|--------------|------------------|------------------|
| [Health Bar] | [Top Left] | [Yes] | [N/A] | [PlayerStats] | [On change] | [20%] | [120px wide] | [1 — highest] | [Numerical: "80/100"] |
| [Stamina Bar] | [Top Left] | [No] | [Show on consume; hide 3s after full] | [PlayerStats] | [Realtime] | [15%] | [80px] | [2] | [Numerical, or hide if full] |
| [Shield] | [Top Left] | [No] | [Active or recently hit] | [PlayerStats] | [On change] | [20%] | [120px] | [3] | [Numerical + shield icon, not color] |
| [Ammo] | [Top Right] | [No] | [Weapon equipped; hide unarmed] | [WeaponSystem] | [On fire/reload] | [10%] | ["88/888" at min res] | [4] | [Text fallback "32 / 120"] |
| [Minimap] | [Bottom Left] | [Yes] | [Suppress in cinematic] | [Navigation] | [Realtime] | [18%] | [150x150px] | [5] | [Cardinal compass strip; toggleable] |
| [Quest Objective] | [Top Center] | [No] | [On change; near location; hide 5s] | [QuestSystem] | [On event] | [30%] | [Body text size] | [6] | [Read aloud on change] |
| [Crosshair] | [Center] | [No] | [Ranged equipped; hide melee/unarmed] | [Weapon/Aim] | [Realtime] | [3%] | [12px diameter] | [1 — center priority] | [Reduce motion: static. Enlarge option.] |
| [Interaction Prompt] | [Center] | [No] | [Within range of interactable] | [InteractionSystem] | [On enter/exit range] | [15%] | [24px icon + text] | [2 — center] | [Text always present, not icon-only] |
| [Subtitles] | [Bottom Center] | [No — on when dialogue + setting] | [Voiced or ambient dialogue] | [DialogueSystem] | [Per line] | [60%] | [24px font min] | [1 — highest in zone] | [IS the accessibility feature — Section 8] |
| [Damage Numbers] | [World-space] | [No] | [Damage event; 800ms duration] | [Combat] | [On event] | [5% per number] | [18px min] | [3] | [Disable option; can overwhelm photosensitive] |
| [Status Effects] | [Top Left below health] | [No] | [Effect active] | [StatusSystem] | [On add/remove] | [3% per icon] | [24px per icon] | [3] | [Icon + text label on focus. Never icon-only.] |
| [Notification Toast] | [Bottom Right] | [No] | [Loot, XP, achievement, quest update] | [Multiple — Section 6] | [On event] | [25%] | [Body text size] | [7 — lowest] | [Queued. Read by screen reader if subtitles on.] |

### 4.2 Element Detail Blocks

> One block per element from 4.1.

---

**Health Bar**

- Visual: [Horizontal fill, LTR. Segmented at 25/50/75%. Bg dark semi-transparent (40% opacity). Fill color = urgency state.]
- Data: [Current HP as %. Numerical "80 / 100" always visible.]
- Update: [Lerp 150ms per change. Damage >25% = 1-frame white flash, then drain.]
- Urgency states:
  - Normal (>50%): [Green fill, no special]
  - Caution (25–50%): [Yellow, warn pulse every 4s]
  - Critical (<25%): [Red, slow pulse 1Hz, edge vignette]
  - Zero: [Empties, turns grey, death state begins]
- Interaction: [Display only.]
- Customization: [Opacity adjustable. Repositionable in accessibility.]

---

**Minimap**

- Visual: [Circular mask, 75px radius @ 1920x1080. Player center. North up unless rotate unlocked. Range = 80 world units default.]
- Data: [Player pos, nearby enemies (if perk), quest markers in range, POI icons, obstacles.]
- Update: [Realtime, every frame. Enemy icons fade in/out 300ms.]
- Urgency: [None for map. Enemy icons red in combat-alert.]
- Interaction: [Press Map button → full map (separate spec).]
- Customization: [Size S/M/L (70/90/110px). Opacity 30–100%. Rotation locked-N or relative. Disable → compass fallback.]

---

**[Repeat for every element in 4.1]**

---

## 5. HUD States by Gameplay Context

> HUD = dynamic system, adapts to context. Static HUD looks wrong in cutscenes, cluttered in exploration, occludes in boss fights. Spec = HUD state machine.

| Context | Shown | Hidden | Modified | Transition Into |
|---------|-------|--------|----------|----------------|
| [Exploration — no threats] | [Minimap, Quest (60% faded), Subtitles if active] | [Ammo, Crosshair, Damage Numbers, Status (if none)] | [Health 40% opacity] | [Fade 500ms when no enemies 10s] |
| [Combat — active] | [Health (full), Stamina (when used), Ammo, Crosshair, Damage, Status, Enemy bars] | [Quest (temp), Toasts (queue paused)] | [Minimap -15% scale, 100% opacity] | [Snap on first detection. No fade.] |
| [Dialogue/Cutscene] | [Subtitles, speaker name] | [All gameplay HUD] | [N/A] | [All fade 300ms on cutscene flag] |
| [Cinematic camera] | [Subtitles only] | [All else, including speaker name] | [Letterbox if applicable] | [Immediate. Letterbox slides 400ms.] |
| [Inventory/Menu] | [None — full-screen render] | [All HUD] | [World visible, paused] | [HUD hides 150ms] |
| [Death/Respawn] | [Death overlay — separate spec] | [All gameplay HUD] | [Desaturate + darken 800ms] | [HUD fades 600ms at HP 0] |
| [Loading/Transition] | [Loading indicator, tip text] | [All gameplay HUD] | [N/A] | [Instant on transition] |
| [Tutorial — new mechanic] | [Standard + Tutorial Prompt] | [Nothing extra] | [Prompt dims background subtly] | [Fade in 200ms on ShowTutorial] |
| [Boss] | [Boss bar (large, top center or bottom), all combat] | [Quest] | [Boss bar distinct visual style] | [Slides in 400ms] |

---

## 6. Information Hierarchy

> Not all info equal. When space limited or stress high, principled priority order. Enforced systematically, not "feels obvious."

| Element | Priority Tier | Reasoning | Replacement If Hidden |
|---------|--------------|-----------|----------------------|
| [Subtitles] | [MUST KEEP — never hide during dialogue] | [Accessibility, legal, story] | [N/A] |
| [Health Bar] | [MUST KEEP — when damageable] | [Survival decisions impossible without] | [Auditory cues supplement, not replace] |
| [Crosshair] | [MUST KEEP — while ranged aiming] | [Targeting without = precision failure] | [Dot-only mode minimum; never fully hidden while aiming] |
| [Interaction Prompt] | [MUST KEEP — within range] | [Without it, interactables invisible] | [Environmental cues supplement; affordance must be explicit] |
| [Ammo] | [SHOULD KEEP] | [Low-ammo decisions need awareness] | [Empty-chamber click for experienced players] |
| [Minimap] | [SHOULD KEEP] | [Spatial awareness; loss = repeated map opens] | [Compass strip acceptable] |
| [Status Effects] | [SHOULD KEEP — while active] | [Active debuffs change viable actions] | [Animation states (limping, sparks) partially communicate] |
| [Quest Objective] | [CAN HIDE] | [Player remembers from context] | [Memory] |
| [Damage Numbers] | [CAN HIDE] | [Feedback, not decision-critical] | [Hit sounds + reactions] |
| [Notification Toasts] | [CAN HIDE in high intensity] | [Mid-combat XP toast = noise] | [Queue, release post-combat] |
| [Combo Counter] | [ALWAYS HIDE on reset / not attacking] | [Stale combo info actively misleading] | [N/A] |

---

## 7. Visual Budget

> Hard limits, not guidelines. Breach = explicit approval + displace existing.

| Constraint | Limit | Method | Estimate | Status |
|-----------|-------|--------|----------|--------|
| Max simultaneous HUD elements | [8] | [Count visible non-faded per frame] | [TBD] | [Verify] |
| Max % screen (exploration) | [12%] | [Pixel area / total] | [TBD] | [Verify] |
| Max % screen (combat) | [22%] | [Same — combat adds ammo/crosshair/enemy bars] | [TBD] | [Verify] |
| Max % center zone (40% W/H) | [5%] | [Crosshair + interaction prompt only] | [TBD] | [Verify] |
| Min contrast HUD text on any bg | [4.5:1 WCAG AA] | [Measured against darkest + lightest world] | [TBD] | [Verify] |
| Max opacity HUD bg panels | [65%] | [Preserves world visibility through panel] | [TBD] | [Verify] |
| Min element size at min res | [40px icons, 18px text] | [Measured at lowest target res] | [TBD] | [Verify] |

> **Apply budgets**: Every proposed element states (1) which budget line, (2) new total, (3) what reduces or goes contextual. "Small icon" = not analysis.

---

## 8. Feedback & Notification Systems

> Most-added, worst-controlled HUD area. Without rules → firehose of overlapping toasts → players ignore entirely.

| Notification | Trigger | Position | Duration (ms) | Anim In/Out | Max Sim | Priority | Queue | Dismissible |
|--------------|---------|----------|--------------|------------|---------|----------|-------|-------------|
| [Item Pickup] | [Inventory] | [Bottom Right toast] | [2000] | [Slide right 200ms / fade 300ms] | [3 stacked] | [Low] | [FIFO; oldest pushed up] | [No — auto] |
| [XP Gain] | [Progression] | [Bottom Right, below items] | [1500] | [Fade 150ms in / 300ms out] | [1 — merge "XP +150"] | [Very Low — suppress in combat, queue] | [Combat-aware] | [No] |
| [Level Up] | [Progression] | [Center — persistent] | [Persistent until input] | [Scale 80%→100% + fade 400ms] | [1] | [High — interrupts] | [Pauses other notifs until dismissed] | [Yes — any input] |
| [Quest Update] | [Quest] | [Top Center] | [4000] | [Slide down 250ms / fade 400ms] | [1 — single-message zone] | [Medium] | [If new during prev: extend +2000ms; no stack] | [No] |
| [Objective Complete] | [Quest] | [Top Center] | [3000] | [Same as Update + completion sound] | [1] | [Med-High — preempts Update] | [Preempts queued] | [No] |
| [Critical Warning (low HP, hazard)] | [Combat/Environment] | [Edge vignette + text bottom-center] | [Persistent while active] | [Fade in 200ms; out 500ms when clears] | [1 per type] | [Critical — never suppressed] | [Bypasses all queues] | [No] |
| [Achievement] | [Achievement] | [Bottom Right — distinct] | [4000] | [Slide right + icon expand 300ms / fade 400ms] | [1] | [Low] | [Behind item toasts; one at a time] | [No] |
| [Hint/Tutorial] | [Tutorial] | [Bottom Center] | [Persistent until done/dismiss] | [Fade 300ms] | [1] | [Medium] | [One at a time] | [Yes — B / Esc] |

**Queue rules**:
1. Combat-aware: Low priority queued during combat. Flushed in batch on exit, max 3 in sequence.
2. Merge: same type within 500ms → single ("Item Pickup x3").
3. Critical: never queued, never merged, always immediate.

---

## 9. Platform Adaptation

> 1080p HUD may be illegible on 55" 4K TV, broken at 1280x720 Steam Deck, hidden behind mobile notch. Spec before impl. Test before cert.

| Platform | Safe Zone | Resolution | Input | HUD Notes |
|----------|-----------|-----------|-------|-----------|
| [PC Windows, 1920x1080 ref] | [3%] | [1280x720 to 3840x2160] | [Mouse+kb, controller optional] | [Scale at all res. Test 1280x720 before cert. Ultrawide 21:9 — minimap not stretch.] |
| [PC Steam Deck 1280x800] | [5%] | [Fixed 1280x800] | [Controller + touch] | [Min text sizes critical. Test ALL elements. Touch targets irrelevant (controller default).] |
| [PS5 / Xbox Series X] | [10%] | [1080p to 4K] | [Controller] | [Cert requires TV safe zone. Action-safe 90%. Test on real TV — overscan differs from monitor.] |
| [Mobile iOS/Android] | [15% top, 10% other] | [360x640 to 414x896] | [Touch] | [Notch/camera at top. Bottom home indicator. Specify portrait + landscape separately.] |

**Repositionability requirement**: Players reposition at minimum (accessibility cert):
- Health bar
- Minimap
- Ability bar (if present)

Saved per profile, not single slot. Across sessions.

---

## 10. Accessibility — HUD Specific

> HUD = most visible accessibility failures (encountered every session). Colorblind, illegible at min scale, no disable distracting anim = top complaints.

### 10.1 Colorblind Modes

| Element | Color-Only Risk | Fix |
|---------|----------------|-----|
| [Health fill] | [Red = low uses red/green] | [Icon pulse + vignette as non-color. Red supplemental.] |
| [Damage numbers] | [Red = taken, green = healed] | [Minus prefix damage, plus heal. Symbols not color.] |
| [Enemy bars] | [Faction/threat color] | [Text label or icon badge. Never color-only.] |
| [Status icons] | [Tint = status type] | [Distinct shapes encode meaning. Color secondary.] |
| [Minimap icons] | [Player vs enemy vs objective by color] | [Distinct shapes: circle player, triangle enemy, star objective. Color supplements.] |

### 10.2 Text Scaling

[Describe behavior at 150% scale (max for your Tier). What reflows? Clips? Architecturally blocked?

Example: "Health label grows; bar expands. Quest text wraps at 150% — verify Top Center fits 2 lines. Damage numbers don't scale (world-space) — accepted limitation."]

**Test matrix**:

| Element | 100% | 125% | 150% | Overflow |
|---------|------|------|------|----------|
| [Health label] | [Pass] | [Pass] | [TBD] | [Bar expands; no stamina overlap] |
| [Quest text] | [Pass] | [TBD] | [TBD] | [Wraps to 2 lines; zone height expands] |
| [Toast text] | [Pass] | [TBD] | [TBD] | [Width expands to 35% screen, then wraps] |
| [Subtitle] | [Pass] | [TBD] | [TBD] | [Dedicated zone — must fit scale] |

### 10.3 Motion Sensitivity

| Animation | Severity | Disabled by Reduced Motion? | Replacement |
|-----------|----------|-----------------------------|-------------|
| [Health low-HP pulse] | [Mild] | [Yes] | [Solid fill, no pulse. Vignette remains.] |
| [Edge vignette] | [Moderate] | [Optional separate toggle] | [Static darkened corners 30% opacity] |
| [Damage numbers float up] | [Mild] | [Yes] | [Instant in place, no float] |
| [Toast slide-in] | [Mild] | [Yes] | [Instant at final position] |
| [Level up center anim] | [High] | [Yes — required] | [Static card, no scale, no particles] |
| [Combo scale pulse] | [Mild] | [Yes] | [Increment without scale] |

### 10.4 Subtitles Specification

> Highest-impact accessibility feature. Same rigor as rest of HUD. Don't leave to impl discretion.

- **Default**: [ON or OFF — document + rationale. Industry standard ON.]
- **Position**: Bottom Center, centered, above safe margin
- **Max chars per line**: [42]
- **Max simultaneous lines**: [2 before scrolling]
- **Speaker ID**: [Name in color or above text — never color alone. Colon prefix: "ARIA: The door is locked."]
- **Background**: [Semi-transparent black, 70% opacity, behind text]
- **Min font**: [24px @ 1080p, scales with text scale]
- **Line break**: [At natural pauses — before conjunctions, after commas, never mid-word]
- **Persistence**: [Hold for spoken duration + 300ms — never disappear while audio plays]
- **Non-dialogue captions**: [Document if ambient/music/SFX captioned — "[tense music]", "[explosion]" — and where shown]

### 10.5 HUD Opacity and Visibility Controls

Available from Accessibility menu:

| Setting | Range | Default | Effect |
|---------|-------|---------|--------|
| [HUD Opacity Global] | [0–100%] | [100%] | [Scales all opacities] |
| [HUD Text Scale] | [75–150%] | [100%] | [Scales text; layout adapts] |
| [Damage Numbers] | [On/Off] | [On] | [Enable/disable floating damage] |
| [Minimap] | [On/Off/Compass] | [On] | [Compass fallback when off] |
| [Notification Verbosity] | [All/Important/Off] | [All] | [Important = quest + level up] |
| [Motion Reduction] | [On/Off] | [Off] | [Replaces animated transitions with instant] |
| [High Contrast] | [On/Off] | [Off] | [HC theme — see art bible HC variants] |

---

## 11. Tuning Knobs

> HUD = data-driven like gameplay. Hardcoded = engineer-only. Externalize before impl.

| Parameter | Current | Range | Increase Effect | Decrease Effect | Player Adjust? | Notes |
|-----------|---------|-------|----------------|-----------------|----------------|-------|
| [Notif duration default] | [2000ms] | [500–5000ms] | [Persist longer; more clutter] | [Faster; higher miss risk] | [No — verbosity instead] | [Per-type Section 8 wins] |
| [Notif queue max] | [8] | [3–15] | [More preserved, slower clear] | [Older dropped] | [No] | [Expand if playtests show losses] |
| [Low-HP pulse Hz] | [1] | [0.5–2] | [More urgent, fatiguing] | [Calmer, may miss urgency] | [No — Reduced Motion disables] | [Linked to accessibility] |
| [Combat HUD reveal] | [0ms] | [0–300ms] | [Softer reveal] | [Instant — highest response] | [No] | [Keep 0ms — combat must be instant] |
| [Exploration fade-out] | [10000ms] | [3000–30000ms] | [Cleaner exploration] | [More reassurance] | [No] | [10s starting estimate] |
| [Minimap range world units] | [80] | [40–200] | [More context] | [Tighter view] | [Yes — S/M/L] | [Exposed as preset] |
| [Minimap size px @ 1080p] | [75] | [50–120] | [Larger, more screen] | [Smaller, less intrusive] | [Yes — S/M/L] | [3 sizes] |
| [Damage number duration] | [800ms] | [400–1500ms] | [Linger, easier read, cluttered] | [Faster clear, harder parse] | [No] | [Tune by combat density] |
| [Global HUD opacity] | [100%] | [0–100%] | [Visible] | [Hidden] | [Yes — Accessibility slider] | [0% = full off; some prefer] |

---

## 12. Acceptance Criteria

> Cert checklist. QA verifies each independently.

**Layout & Visibility**
- [ ] All elements within safe zones on all platforms
- [ ] No two elements overlap in any documented context
- [ ] HUD <[12]% screen in exploration (ref res)
- [ ] HUD <[22]% in combat
- [ ] No element in center [40]% during exploration (crosshair excepted in combat)
- [ ] All visible/legible at min res on all platforms

**Per-Context Correctness**
- [ ] Correct elements per Section 5 context
- [ ] Context transitions within timing spec
- [ ] Boss bar appears/disappears correctly
- [ ] Death state hides all gameplay HUD

**Accessibility**
- [ ] Text 4.5:1 contrast against all backgrounds (test light AND dark scenes)
- [ ] No color-only differentiator (verify: remove color, info still communicated)
- [ ] Subtitles for all voiced + ambient when enabled
- [ ] Subtitles never disappear while audio plays
- [ ] Reduced Motion disables all 10.3 animations
- [ ] 150% text scale: no overflow or overlap
- [ ] All 10.5 settings functional + persist

**Notifications**
- [ ] Same-type within 500ms merge
- [ ] Low-priority queued in combat, released after
- [ ] Critical warnings immediate regardless of queue/combat
- [ ] No more than [3] toasts simultaneous
- [ ] Queue cleared on level transition

**Platform**
- [ ] 10% safe zone on console (test physical TV, not monitor)
- [ ] Correct at 1280x720 Steam Deck — no clip/overlap
- [ ] Health, Minimap, Ability Bar repositionable + persist
- [ ] Controller disconnect doesn't corrupt HUD state

---

## 13. Open Questions

> Resolve before Approved.

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| [Minimap show enemies by default or after detection skill?] | [systems-designer + ui-designer] | [Sprint 5, Day 2] | [Pending — depends on progression GDD] |
| [Boss bar standard or distinct? Distinct if bosses much more important.] | [game-designer] | [Sprint 5, Day 1] | [Pending] |
| [Damage numbers diegetic (world-space, occluded) or screen-space (always readable)?] | [ui-designer + lead-programmer] | [Sprint 4, Day 5] | [Pending — affects render layer] |
| [Mobile portrait vs landscape — both? Each needs own zone layout.] | [producer] | [Sprint 3, Day 3] | [Pending — platform scope decision first] |
