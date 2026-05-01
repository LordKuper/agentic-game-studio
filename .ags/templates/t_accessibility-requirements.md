# Accessibility Requirements: [Game Title]

> **Status**: Draft | Committed | Audited | Certified
> **Author**: [ux-designer / producer]
> **Last Updated**: [Date]
> **Accessibility Tier Target**: [Basic / Standard / Comprehensive / Exemplary]
> **Platform(s)**: [PC / Xbox / PS5 / Switch / iOS / Android]
> **External Standards Targeted**:
> - WCAG 2.1 Level [A / AA / AAA]
> - AbleGamers CVAA Guidelines
> - Xbox Accessibility Guidelines (XAG) [Yes / No / Partial]
> - PlayStation Accessibility (Sony) [Yes / No / Partial]
> - Apple / Google Accessibility [Yes / No / N/A]
> **Accessibility Consultant**: [Name + org, or "None"]
> **Linked Documents**: `design/gdd/systems-index.md`, `docs/ux/interaction-pattern-library.md`

> Per-screen accessibility = UX specs. This = project-wide commitments, feature matrix, test plan, audit history. Created once during Technical Setup. Updated as features added + audits done. Conflicts: this doc wins unless producer approves revision.
>
> **Update**: After each `/ags-gate-check`, after audits, when new system added to `systems-index.md`.

---

## Accessibility Tier Definition

> Accessibility not binary. Four tiers = shared vocabulary, explicit commitment at start, prevents scope creep both directions. Commit to tier with specific feature targets, not just name.

### Tier Definitions

| Tier | Core Commitment | Effort |
|------|----------------|--------|
| **Basic** | Critical text readable. No color-only feature. Independent volume controls. Completable without photosensitivity risk. | Low — design constraints |
| **Standard** | Basic + full input remapping all platforms, subtitles with speaker ID, adjustable text size, ≥1 colorblind mode, no un-extendable timed input. | Medium — dedicated impl |
| **Comprehensive** | Standard + screen reader for menus, mono audio, difficulty assist, HUD repositioning, reduced motion, visual indicators for gameplay-critical audio. | High — platform API + UI architecture |
| **Exemplary** | Comprehensive + full subtitle customization (font/size/color/bg/position), high contrast, cognitive load assist, tactile/haptic alternatives for audio, external third-party audit. | Very High — accessibility specialist |

### This Project's Commitment

**Target Tier**: [Standard]

**Rationale**: [3-5 sentences. Justify, don't just state.

Consider: genre + barriers (fast-twitch = motor; reading-heavy = visual)? Target player + disability prevalence research? Platform requirements (Xbox = XAG for ID@Xbox)? Team capacity? Cost of dropping a tier?

Example: "Narrative RPG, turn-based combat, target 25-45. Turn-based eliminates severe motor barriers, but reading-heavy creates visual + cognitive barriers. Standard addresses these. Exemplary not achievable without dedicated accessibility engineer. Xbox ID@Xbox requires XAG which Standard meets. Dropping to Basic excludes 8-12% per AbleGamers data."]

**Features in scope (beyond tier baseline)**:
- [e.g., "Full subtitle customization — elevated from Comprehensive: dialogue-heavy game"]
- [e.g., "One-hand controller mode — combat has critical hold inputs"]

**Features out of scope**:
- [e.g., "Screen reader for game world (not menus) — engine work beyond capacity. Documented in Known Limitations."]

---

## Visual Accessibility

> Visual = largest accessibility-feature user base. Color vision deficiency: 8% men, 0.5% women. TV-distance text legibility = frequent largest failure. Document before impl — retrofitting min text sizes after asset lock = expensive.

| Feature | Target Tier | Scope | Status | Notes |
|---------|-------------|-------|--------|-------|
| Min text size — menu UI | Standard | All menus | Not Started | 24px @ 1080p. Scale at 4K. WCAG 2.1 SC 1.4.4: 200% resize without loss. |
| Min text size — subtitles | Standard | All voiced/captioned | Not Started | 32px @ 1080p. TV at 3m = constraint. |
| Min text size — HUD | Standard | In-game HUD | Not Started | 20px for critical (health, ammo, objective). Non-critical may be smaller. |
| Text contrast — UI | Standard | All UI text | Not Started | 4.5:1 body (WCAG AA). 3:1 large (18px+ or 14px bold). Automated check on final colors. |
| Text contrast — subtitles | Standard | Subtitles | Not Started | 7:1 (WCAG AAA). Drop shadow or opaque bg by default. |
| Colorblind — Protanopia | Standard | Color-coded gameplay | Not Started | Red-green ~6% men. Health bars, enemy indicators, map markers. Red→orange/yellow; green→teal. |
| Colorblind — Deuteranopia | Standard | Color-coded gameplay | Not Started | Green-red ~1% men. Same palette as Protanopia often covers. Verify Coblis. |
| Colorblind — Tritanopia | Standard | Color-coded gameplay | Not Started | Blue-yellow ~0.001%. Blue→purple; yellow→orange. |
| Color-as-only-indicator audit | Basic | All UI + gameplay | Not Started | List in table below. Each needs non-color backup before ship. |
| UI scaling | Standard | All UI | Not Started | 75%–150%. Default 100%. No layout breakage. HUD scaling independent of menu. |
| High contrast mode | Comprehensive | Menus min, HUD preferred | Not Started | Replace semi-transparent bgs with opaque. Mid-tones → black/white/system-HC. All interactive outlined. |
| Brightness/gamma | Basic | Global | Not Started | Graphics settings. Reference calibration image. -50% to +50%. |
| Screen flash / strobe warning | Basic | Cutscenes, VFX | Not Started | (1) Pre-launch photosensitivity warning. (2) Audit flash VFX vs Harding FPA (≤3/sec above luminance threshold). (3) Optional flash reduction mode (-80% amplitude). |
| Motion/animation reduction | Standard | UI transitions, camera shake, VFX | Not Started | Reduce/eliminate: shake, bob, blur, parallax in menus, looping bgs. Cannot eliminate player movement anim. Toggle in settings. |
| Subtitles — on/off | Basic | All voiced | Not Started | Default OFF (industry standard). Prominent at first launch. |
| Subtitles — speaker ID | Standard | All voiced | Not Started | Speaker name before line. Color-coded only if differs by more than hue (test colorblind). |
| Subtitles — style customization | Comprehensive | Subtitle display | Not Started | Font size (≥4), bg opacity (0–100%), color (white/yellow/custom), position (bottom/top/relative). |
| Subtitles — SFX captions | Comprehensive | Gameplay-critical SFX | Not Started | See Auditory section. Format: [SOUND DESCRIPTION] in brackets. |

### Color-as-Only-Indicator Audit

> Every gameplay/UI where color is sole differentiator. Resolve = non-color backup working in all 3 colorblind modes.

| Location | Color Signal | Communicates | Non-Color Backup | Status |
|----------|-------------|--------------|-----------------|--------|
| [Health bar] | [Red = low] | [Near death] | [Numeric value + flash] | [Not Started] |
| [Minimap markers] | [Red enemy, green ally] | [Allegiance] | [Triangles enemy, circles ally] | [Not Started] |
| [Item rarity] | [Border color grey/blue/purple/gold] | [Quality tier] | [Rarity name on hover/focus + star count] | [Not Started] |
| [Add row per element] | | | | |

---

## Motor Accessibility

> Games more motor-demanding than most software. Tremor (precision), hemiplegia (one hand), RSI (hold duration). AbleGamers: 35M US gamers with disability affecting play. Cheap upfront, very expensive post-launch.

| Feature | Target Tier | Scope | Status | Notes |
|---------|-------------|-------|--------|-------|
| Full input remapping | Standard | All inputs, all platforms | Not Started | Every default rebindable. Kb, mouse, controller, peripherals independent. No conflict allowed (warn). Persist to profile. |
| Input method switching | Standard | PC | Not Started | Switch kb/mouse ↔ gamepad anytime, no restart. UI updates prompts dynamically. |
| One-hand mode | [Tier] | [Multi-input actions] | Not Started | Audit every multi-input. Each: single-hand executable? If not, toggle alternative or hold-to-toggle. List paths here. |
| Hold-to-press alternatives | Standard | All hold inputs | Not Started | Every "hold X to Y" → toggle alt. Toggle: first press on, second off. List all hold inputs here. |
| Rapid input alternatives | Standard | Mashing / rapid sequences | Not Started | >3 presses/sec sustained → single-press toggle. Hades "hold to dash repeatedly" model. |
| Input timing adjustments | Standard | QTEs, timed presses, rhythm | Not Started | Timing window multiplier. Min 0.5x–3.0x. Default 1.0x. 500ms → 1500ms at 3x. List all timed inputs, test all multipliers. |
| Aim assist | Standard | Ranged combat / targeting | Not Started | Granular: Strength (0–100%), Radius, Magnetism (snap), Slowdown (near-target decel) as separate sliders. Default helpful not intrusive. |
| Auto-sprint / movement assists | Standard | Movement | Not Started | Sprint toggle (covered above). Auto-run (hold direction, continues without input). List all continuous-hold movement. |
| Platforming / traversal assists | [Tier] | [If platforming] | Not Started | Auto-grab, coyote time, jump height adjustment. N/A if no platforming. |
| HUD repositioning | Comprehensive | All HUD | Not Started | Move health, minimap, quest tracker. Important for head-tracking / eye-gaze users. |

---

## Cognitive Accessibility

> Affects ADHD, dyslexia, autism, brain injuries, anxiety — larger combined population than studios realize. Also benefits all in stress. Common failures: no pause, one-time tutorial info, too many simultaneous states. Hades + Celeste demonstrate cognitive assists don't harm experience.

| Feature | Target Tier | Scope | Status | Notes |
|---------|-------------|-------|--------|-------|
| Difficulty options | Standard | All difficulty params | Not Started | Granular sliders (damage dealt, taken, aggression, speed) over single Easy/Normal/Hard. Document adjustable + fixed. Fixed needs design justification. |
| Pause anywhere | Basic | All gameplay | Not Started | Pause during cutscenes, dialogue, tutorials. Document any restriction + justification. Any restriction = risk. |
| Tutorial persistence | Standard | All tutorials | Not Started | Retrievable from Help menu after dismissal. AbleGamers: many dismiss on reflex. |
| Quest / objective clarity | Standard | Quest systems | Not Started | Active objective accessible within 2 button presses anytime. Full text on demand, not truncated. Avoid inference ("investigate northern region" — where?). |
| Visual indicators for audio info | Standard | SFX with gameplay info | Not Started | Audit SFX with critical state. Visual equivalent? Off-screen enemy needs edge indicator. Critical warnings need visual cues. |
| Reading time for UI | Standard | Auto-dismissing dialogs | Not Started | Actionable info: ≥5s or no auto-dismiss. Document every auto-dismissing element + duration. |
| Cognitive load documentation | Comprehensive | Per system | Not Started | Per system in systems-index, document max simultaneous tracking. Flag >4. Review trigger, not hard rule. |
| Navigation assists | Standard | World nav | Not Started | Fast travel (visited locations), waypoints for current objective, optional always-visible objective indicator. Document inclusions + omissions. |

---

## Auditory Accessibility

> 7% deaf/hard of hearing. Plus many play in audio-reduced environments (commute, household, sleeping infant). Critical audio-only info = design failure even before accessibility. Principle: every sound changing what player should DO needs visual equivalent.

| Feature | Target Tier | Scope | Status | Notes |
|---------|-------------|-------|--------|-------|
| Subtitles for spoken dialogue | Basic | All voiced | Not Started | 100% — no exceptions. Narration, in-engine, distant radio/environment. Test sync. |
| Closed captions for critical SFX | Comprehensive | Identified SFX list (below) | Not Started | Only SFX communicating non-visual state. See audit table. |
| Mono audio | Comprehensive | Global output | Not Started | Folds stereo/spatial to mono. Preserves channel balance, not full-volume sum. Essential for single-sided deafness. |
| Independent volume controls | Basic | Music / SFX / Voice / UI buses | Not Started | ≥4 sliders. Persist. 0–100%, default 80%. Main settings + pause menu. |
| Visual representations for directional audio | Comprehensive | Off-screen threats + audio events | Not Started | Edge indicator pointing to source. Opacity scales with volume. Threat (red) + info (neutral) variants. The Last of Us Part II reference. |
| Hearing aid compatibility | Standard | High-frequency cues | Not Started | Audit all cues. Critical info above 4kHz needs low-freq or visual equivalent. Aids often filter highs. |

### Gameplay-Critical SFX Audit

> Every SFX with state needing action. Each needs visual backup OR caption.

| SFX | Communicates | Visual Backup | Caption Required | Status |
|-----|--------------|---------------|-----------------|--------|
| [Enemy attack windup] | [Incoming damage — dodge] | [Animation telegraph all camera angles] | [No — visual sufficient] | [Not Started] |
| [Trap trigger click] | [Trap firing] | [Not always visible by camera] | [Yes — "[CLICK]" + directional] | [Not Started] |
| [Low health heartbeat] | [HP critical] | [Health bar shows critical] | [No — visual sufficient] | [Not Started] |
| [Quest completion chime] | [Objective done] | [Tracker updates] | [No — visual sufficient] | [Not Started] |
| [Add each SFX changing player action] | | | | |

---

## Platform Accessibility API Integration

> Each platform provides native APIs. Using them = OS-level features (system screen readers, motor services) work in your game. Ignoring = users get no benefit inside game. Xbox cert REQUIRES XAG. Platform requirements = floor not ceiling.

| Platform | API / Standard | Features | Status | Notes |
|----------|---------------|---------|--------|-------|
| Xbox (GDK) | Xbox Game Core Accessibility / XAG | [Input remap via Ease of Access, high contrast, narrator for menus] | Not Started | XAG required for ID@Xbox Game Pass. https://docs.microsoft.com/gaming/accessibility/guidelines |
| PlayStation 5 | Sony Accessibility / AccessibilityNode API | [Screen reader passthrough for menus, mono, high contrast] | Not Started | PS5 supports system audio description + mono if game exposes AccessibilityNode on UI. |
| Steam (PC) | Steam Accessibility / SDL | [Controller remap via Steam Input, subtitles] | Not Started | Steam Input = system-level remap independent of in-game. In-game remap still required for kb/mouse. |
| iOS | UIAccessibility / VoiceOver | [VoiceOver for menus] | N/A | Only if mobile in scope. |
| Android | AccessibilityService / TalkBack | [TalkBack for menus] | N/A | Only if mobile in scope. |
| PC (Screen Reader) | JAWS / NVDA / Windows Narrator | [Menu announcements] | Not Started | UI exposes accessible names + roles via platform UI layer. Verify Unity package vs `.ags/docs/engine-reference/unity/modules/ui.md`. |

---

## Per-Feature Accessibility Matrix

> Accessibility = property of every system. Matrix = "accessibility impact" view. New system in systems-index → row added here. Unaddressed concern → cannot mark Approved in systems index.

| System | Visual Concerns | Motor Concerns | Cognitive Concerns | Auditory Concerns | Addressed | Notes |
|--------|----------------|---------------|-------------------|------------------|-----------|-------|
| [Combat] | [Color-coded enemy bars; attack anim motion sickness] | [Rapid combo input; hold-to-guard] | [Track patterns + cooldowns + resources simultaneously] | [Off-screen attack cues; critical damage warnings] | [Partial] | [Colorblind palette done; hold-to-block toggle pending] |
| [Inventory / Equipment] | [Border color = rarity] | [None — turn-based] | [Stat comparison reads multiple values] | [None] | [Partial] | [Non-color rarity in progress] |
| [Dialogue] | [Subtitle contrast] | [None] | [Long trees with timed choices] | [Must subtitle all] | [Not Started] | [Timed choices need extended timer] |
| [Navigation / World Map] | [Marker colors] | [None] | [Objective clarity; waypoints] | [Audio pings have no visual equivalent] | [Not Started] | |
| [Add system from systems-index.md] | | | | | | |

---

## Accessibility Test Plan

> Standard QA = does it work. Accessibility testing = does it work for users who NEED it. Different tests. Subtitles can pass QA (text displays) and fail accessibility (unreadable at TV distance for low vision). Three test types: automated (contrast, sizes), manual internal (simulators), user testing (actual users).

| Feature | Method | Cases | Pass | Owner | Status |
|---------|--------|-------|------|-------|--------|
| Text contrast | Automated — analyzer on UI screenshots | All text/bg combos all states | Body ≥4.5:1; large ≥3:1; subtitle bg ≥7:1 | ux-designer | Not Started |
| Colorblind modes | Manual — Coblis on screenshots with modes | Exploration, combat, inventory each mode | No info lost; objectives completable without color | ux-designer | Not Started |
| Input remapping | Manual — non-default bindings, complete tutorial + level | All defaults rebound; gameplay works; conflict prevention | All actions accessible; conflict prevention works; persists across restart | qa-lead | Not Started |
| Subtitle accuracy | Manual — verify against script, all lines | All voiced; timing; speaker ID | 100% subtitled; speaker ID multi-character; no display >3s after line ends | qa-lead | Not Started |
| Hold input toggles | Manual — toggle alts on, complete combat + traversal | All hold inputs in toggle | All actions completable in toggle; no sustained hold required | qa-lead | Not Started |
| Reduced motion | Manual — enable, navigate menus + first hour | Menu transitions; HUD anims; camera shake | No looping anims; no shake above threshold; transitions are fade or cut | ux-designer | Not Started |
| Platform screen reader (menu) | Manual — OS reader on, navigate menus | Main, settings, pause, inventory, map | All elements have announcements; logical order; reachable by kb/D-pad | ux-designer | Not Started |
| User testing — colorblind | Colorblind participants | Full session each mode | Complete content without clarification; no session-stopping confusion | producer | Not Started |
| User testing — motor | One-hand / adaptive controller participants | Full session toggle + extended timing | Complete MVP within tolerance of able-bodied time | producer | Not Started |

---

## Known Intentional Limitations

> Undocumented omissions = surprises at cert or community. Documenting limitation + rationale = deliberate not oversight. Identifies unserved players + mitigation. Every entry = risk — assess honestly.

| Feature | Tier Required | Why Not Included | Risk / Impact | Mitigation |
|---------|--------------|-----------------|--------------|------------|
| [Screen reader for in-game world (NPCs, objects, environmental text)] | Exemplary | Engine accessibility = menus only. World extension = custom spatial audio description, beyond scope | Blind/low-vision players can navigate menus but not explore world | Critical world info duplicated in accessible menus (quest log, map); evaluate post-launch DLC |
| [Full subtitle customization (font/color/bg)] | Comprehensive | Scope reduction — Standard target. Custom font rendering = asset pipeline work | Deaf/HoH with specific legibility needs; dyslexic users with custom fonts | Two preset styles (default + high-readability); log for post-launch update |
| [Tactile/haptic for all audio cues] | Exemplary | Non-Xbox rumble API integration out of scope v1.0 | Deaf players relying on haptic; PC players with non-Xbox controllers | Xbox controller haptic in scope; evaluate DualSense haptic for post-launch patch |
| [Add intentionally excluded] | | | | |

---

## Audit History

> Accessibility = not certified once. Platform requirements change. New features = new barriers. Standards evolve. History = due diligence + regression detection.

| Date | Auditor | Type | Scope | Findings | Status |
|------|---------|------|-------|----------|--------|
| [Date] | [Internal — ux-designer] | Internal review | [Pre-submission checklist vs tier] | [e.g., "12 verified, 3 open: subtitle contrast in 2 scenes, color-only minimap"] | [In Progress] |
| [Date] | [External — AbleGamers Player Panel] | User testing | [Motor — one-hand mode + timing] | [e.g., "Toggle modes work. QTE 3x failed for one — recommend 5x option."] | [Findings addressed] |
| [Add row per audit] | | | | | |

---

## External Resources

| Resource | URL | Relevance |
|----------|-----|-----------|
| WCAG 2.1 | https://www.w3.org/TR/WCAG21/ | Foundational — contrast, sizing, input |
| Game Accessibility Guidelines | https://gameaccessibilityguidelines.com | Game-specific checklist by category + cost |
| AbleGamers Player Panel | https://ablegamers.org/player-panel/ | User testing + consulting with disabled gamers |
| Xbox Accessibility Guidelines (XAG) | https://docs.microsoft.com/gaming/accessibility/guidelines | Required for Xbox cert; structured checklist |
| PlayStation Accessibility | https://www.playstation.com/en-us/accessibility/ | Sony platform requirements + design guidance |
| Coblis Color Blindness Simulator | https://www.color-blindness.com/coblis-color-blindness-simulator/ | Free colorblind simulation |
| Accessible Games Database | https://accessible.games | Research + examples |
| CVAA | https://www.fcc.gov/consumers/guides/21st-century-communications-and-video-accessibility-act-cvaa | US legal req for games with comms (voice chat, messaging) |

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| [Unity accessibility package supports dynamic HUD updates or only static menus?] | [ux-designer] | [Before Technical Setup gate] | [Unresolved — check `.ags/docs/engine-reference/unity/modules/ui.md`] |
| [Xbox ID@Xbox min XAG compliance for release window?] | [producer] | [Before Pre-Production gate] | [Unresolved] |
| [Dialogue system supports timed choice extensions without architecture change?] | [lead-programmer] | [During Technical Design] | [Unresolved] |
| [Add question] | [Owner] | [Deadline] | [Resolution] |
