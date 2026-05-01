# Difficulty Curve: [Game Title]

> **Status**: Draft | In Review | Approved
> **Author**: [game-designer / systems-designer]
> **Last Updated**: [Date]
> **Links To**: `design/gdd/game-concept.md`
> **Relevant GDDs**: [`design/gdd/combat.md`, `design/gdd/progression.md`]

---

## Difficulty Philosophy

[One paragraph. Design value statement. All tuning serves this. Not mechanical description.

Four common philosophies:

1. **Masochistic challenge as core fantasy**: Difficulty is the product. Overcoming = reward. Reducing = removes point. (Dark Souls, Celeste max assist off)
2. **Accessible entry, optional depth**: Base completable by most; depth/challenge opt-in. (Hades, Hollow Knight w/accessibility)
3. **Difficulty serves narrative pacing**: Rises + falls with story beats. Capable in resolution, threatened in crisis. (Last of Us, God of War)
4. **Relaxed engagement**: Challenge present, never focus. Failure gentle, infrequent. Comfort + expression > obstacle. (Stardew, Animal Crossing)

State philosophy. Add one sentence: what is player permitted to feel? How long frustrated before design intervenes? Acceptable failure cost?]

---

## Difficulty Axes

> Most games have multiple challenge dimensions. Identify explicitly — prevents tuning only execution while leaving others. Easy execution + overwhelming decisions = confusing not engaging.
>
> Per axis: can player control/reduce via choices, builds, settings? If not = forced challenge — be intentional.

| Axis | Description | Primary Systems | Player Control? |
|------|-------------|----------------|-----------------|
| **Execution difficulty** | [Precision + timing demands. e.g., "Dodging requires correct timing in 200ms window."] | [Combat, movement] | [Yes — practice / No — fixed threshold] |
| **Knowledge difficulty** | [Cost of not knowing. e.g., "Enemy weaknesses untelegraphed; uninformed take more damage."] | [Enemy design, UI, lore] | [Yes — in-game discovery / No — requires external] |
| **Resource pressure** | [Scarcity of progression resources. e.g., "Healing limited; efficiency required for long runs."] | [Economy, loot, crafting] | [Yes — build optimization / Partially] |
| **Time pressure** | [Time to think? Or rapid decisions? e.g., "Spawn timers + attack windows require real-time."] | [Combat pacing, timers] | [Yes — difficulty settings / No — core to genre] |
| **Decision complexity** | [How many simultaneous meaningful choices? e.g., "Build decisions across 4 systems; suboptimal = compounding disadvantage."] | [Progression, inventory, skills] | [Yes — UI + tutorialization / No — inherent strategy depth] |
| **[Add axis]** | [Description] | [Systems] | [Control] |

---

## Difficulty Curve Overview

> Intended challenge arc whole game. 1-10 scale: 1 = no challenge, 10 = max. Relative to THIS game's intent — 6/10 in soulslike ≠ 6/10 in cozy sim.
>
> "Primary challenge type" = axis doing most work. "New systems" = first-time only — learning is a difficulty form.
>
> "Target player state" = intended emotion. Playtested divergence → this is what to achieve.

| Phase | Duration | Difficulty (1-10) | Primary Challenge | New Systems | Target State |
|-------|----------|-------------------|-------------------|-------------|--------------|
| [Prologue / Tutorial] | [0-15 min] | [2/10] | [Knowledge] | [Movement, basic interaction] | [Safe, curious, building confidence] |
| [Early game] | [15 min - 2 hrs] | [3-5/10] | [Execution] | [Combat, inventory, first upgrade] | [Learning, occasional failure, clear cause-effect] |
| [Mid game opening] | [2-6 hrs] | [5-7/10] | [Decision complexity] | [Build choices, advanced enemies, crafting] | [Engaged, strategizing, growth] |
| [Mid game depth] | [6-15 hrs] | [6-8/10] | [Resource pressure] | [Elites, optional hard, endgame previews] | [Challenged, invested, approaching mastery] |
| [Late game] | [15-25 hrs] | [7-9/10] | [Execution + knowledge] | [Endgame systems, NG+] | [Mastery, build identity, peak challenge] |
| [Optional / Endgame] | [25+ hrs] | [8-10/10] | [All combined] | [Mastery challenges, achievements] | [Expert play, self-imposed goals, community] |

---

## Onboarding Ramp

> First hour does most difficult work: teach foundations without lessoning, create commitment. Most players who leave do so in first 30 min — onboarding failed.
>
> Scaffolding (Vygotsky ZPD adapted): introduce mechanic in isolation before combining. Cannot learn two skills simultaneously under pressure.

### What the Player Knows at Each Stage

| Time | Knows | Doesn't Know Yet |
|------|-------|------------------|
| [0 min] | [Literally nothing — this row = most important UX audit. What can player infer from title screen?] | [Everything] |
| [5 min] | [Core verb, basic world reading] | [All progression, all secondary mechanics] |
| [15 min] | [Core interaction loop, first goal] | [Build depth, advanced mechanics, danger severity] |
| [30 min] | [Made one strategic choice] | [Whether choice was optimal] |
| [60 min] | [Working model of core loop] | [Late-game depth, optional systems] |

### Mechanic Introduction Sequence

> Introduction order = design decision with consequences. Most essential verb first. Modifiers AFTER base mechanic internalized. Never two new mechanics same encounter.

| Mechanic | Introduced | Method | Stakes |
|----------|------------|--------|--------|
| [Core movement / primary verb] | [First 30s] | [Tutorial prompt / environmental / NPC] | [None — safe experiment] |
| [Primary interaction / action] | [First 2 min] | [Method] | [Low — reversible, forgiving] |
| [First resource mechanic] | [5 min] | [Method] | [Low — abundant at intro] |
| [First strategic choice] | [15 min] | [Method] | [Low — changeable / revisitable] |
| [First real failure risk] | [20-30 min] | [Method] | [Moderate — genuine threat with fair tools] |
| [Add mechanic] | [Timing] | [Method] | [Stakes] |

### The First Failure

[Describe intended design of first meaningful failure. One of most important beats.

Well-designed first failure teaches not punishes. Player immediately identifies wrong + alternative. Ambiguous cause → player blames game.

Answer: What causes first failure? What does player learn? Retry speed? Cost? Game feedback bridging cause + effect?]

### When the Player First Feels Competent

[Specific beat — not vague window. Player shifts "learning" → "doing". First time prediction comes true, or executes plan and works.

MUST happen first hour. Without it, player won't reach Phase 3 (First Mastery). Design deliberately.

What's the moment? Systems creating it? Player triggers it how? Game communicates success how?]

---

## Difficulty Spikes and Valleys

> Healthy curve = sawtooth (Csikszentmihalyi flow at macro). Tension builds, releases at milestone, re-engages at higher baseline. Flat = boredom. Uninterrupted escalation = fatigue.
>
> Spikes = intentional peaks testing skills. Valleys = troughs to breathe + experiment + feel powerful before next escalation. Both designed.
>
> "Recovery" = critical: post-spike player feels accomplished not depleted. Give valley, reward, narrative payoff.

| Name | Location | Type | Purpose | Recovery |
|------|----------|------|---------|----------|
| ["First Boss"] | [End of Area 1, ~1 hr] | [Spike] | [Tests Area 1 skills. Gate confirming readiness for complexity.] | [Safe area, upgrade, story beat for emotional relief before Area 2.] |
| ["Safe Zone"] | [Hub between Areas 1+2, ~1.5 hrs] | [Valley] | [Powerful from boss win. Experiment with builds before stakes rise.] | [N/A — IS recovery from preceding spike.] |
| ["Knowledge Wall"] | [Area 3 first encounter, ~4 hrs] | [Spike — knowledge] | [Forces engagement with mechanic players may have avoided.] | [Clear feedback on cause. Hint surfaces on third failure. Mechanic standard after.] |
| ["Pre-Climax Valley"] | [Just before final act, ~20 hrs] | [Valley] | [Breathing room before final escalation. Reflect on journey.] | [N/A — designed relief before finale.] |
| [Add spike/valley] | [Location] | [Type] | [Purpose] | [Recovery] |

---

## Balancing Levers

> Specific values + parameters tuning per phase. Centralized = tune curve without hunting GDDs. Each lever cross-references owning GDD.
>
> "Current" = design intent at writing — implementation in `assets/data/`. Range = safe operating; outside reliably breaks experience.

| Lever | Phase(s) | Effect | Current | Range | Notes |
|-------|----------|--------|---------|-------|-------|
| [Enemy HP multiplier] | [All] | [Higher = longer fights = more pressure + execution time] | [1.0x] | [0.7x-1.5x] | [Below 0.7x: fights end before pattern read. Above 1.5x: attrition replaces skill.] |
| [Enemy aggression timer] | [Mid+] | [Time between attacks; lower = less reaction] | [2.0s] | [1.2s-3.0s] | [Below 1.2s: sub-human reaction. Above 3.0s: passive.] |
| [Resource drop rate] | [Early] | [Lower = more pressure, punishes inefficiency] | [1.5x baseline] | [0.8x-2.0x] | [Onboarding generosity; reduces mid-game as skill assumed.] |
| [New mechanic introduction density] | [First hour] | [Concepts per minute; too high = overload] | [1 per 8 min] | [1 per 5 (max) — 1 per 15 (slow)] | [>1 per 5 → retention drop. <1 per 15 → boredom.] |
| [Failure cost] | [All] | [Time lost on failure; higher = more punishing + tension] | [2 min setback] | [30s-8 min] | [Scale with encounter frequency. Frequent failures need fast recovery.] |
| [Add lever] | [Phase] | [Effect] | [Setting] | [Range] | [Notes] |

---

## Player Skill Assumptions

> Every game implicitly assumes skill development. Explicit = verify each skill taught before tested + gap between intro + hard test sufficient for internalization.
>
> Same-encounter intro+test = surprise spike. Assumed-but-never-introduced = undocumented knowledge wall. Both fixable IF documented.
>
> "Taught by" = mechanism: tutorial, environmental, safe practice, NPC, organic discovery.
>
> "Tested by" = first encounter REQUIRING skill to survive without significant damage/cost.

| Skill | Introduced | Mastered By | Taught By | First Hard Test |
|-------|------------|-------------|-----------|-----------------|
| [Core movement / dodging] | [Tutorial 0-5 min] | [End Area 1 ~1 hr] | [Safe practice with visible hazards] | [First Elite ~45 min] |
| [Resource management] | [First shop ~10 min] | [Mid game ~4 hrs] | [Area 2 scarcity forces planning] | [Boss requiring consumables] |
| [Build decision-making] | [First upgrade ~20 min] | [End mid game ~10 hrs] | [Multiple playthroughs / community / in-game advisor] | [Endgame punishing build incoherence] |
| [Enemy pattern reading] | [Area 1 basic enemies] | [Area 3 ~4 hrs] | [Telegraphs visible + consistent from intro] | [Elite with 3+ patterns] |
| [Add skill] | [Introduced] | [Mastered] | [Taught] | [Tested] |

---

## Accessibility Considerations

> Difficulty accessibility = not making easier — ensuring players with different needs reach intended emotional experience. Be explicit about adjustable + non-adjustable. Justify both.
>
> Self-Determination Theory: players need to feel competent. Options helping competence without removing agency = always worth including. Options making competence meaningless undermine experience.

### What Can Be Adjusted

| Adjustment | Method | Effect | Tradeoff |
|-----------|--------|--------|----------|
| [Enemy speed reduction] | [Difficulty / accessibility menu] | [Lowers execution without changing knowledge/decision] | [Reduces combat timing tension; OK for narrative players] |
| [Extended input windows] | [Accessibility menu] | [Motor-impaired players achieve same outcomes with more time] | [Minimal — skill expression preserved, threshold relaxed] |
| [Hint frequency] | [Settings toggle] | [Surfaces guidance more/less aggressively] | [More hints reduce knowledge difficulty; organic players feel over-guided] |
| [Add option] | [Method] | [Effect] | [Tradeoff] |

### What Cannot Be Adjusted (and Why)

| Fixed Element | Why Not | Reasoning |
|---------------|---------|-----------|
| [Permadeath in roguelike] | [Removing eliminates resource pressure axis encounter balance built around] | [Decision weight = permanence; without, core loop loses meaning] |
| [Core narrative pacing] | [Difficulty valleys timed to story; adjustable decouples] | [Story + difficulty designed as one arc, not two tracks] |
| [Add fixed] | [Why] | [Reasoning] |

---

## Cross-System Difficulty Interactions

> Two systems simultaneous = combined difficulty often greater than sum (sometimes less). Frequently unintended, only surface in playtest. Document anticipated → QA + playtest checklist.
>
> "Intended?" Yes = designed feature. No = mitigate. Partial = OK in small doses, problematic if dominant.

| System A | System B | Combined Effect | Intended? |
|----------|----------|-----------------|-----------|
| [Combat difficulty] | [Resource scarcity] | [Resource-poor face combat with fewer options. Death spiral: failing creates worse conditions.] | [Partial — intended as stakes not trap. Pity mechanics required for unrecoverable states.] |
| [Build complexity] | [Time pressure] | [Learning build + time pressure = cognitive load beyond either alone.] | [No — reduce decision demand in high time-pressure encounters.] |
| [New mechanic intro] | [Resource pressure] | [Learning + optimizing simultaneously.] | [No — intro mechanics in low-resource environments.] |
| [Enemy density] | [Execution difficulty] | [High count + demanding enemies = exponential not linear scaling.] | [Partial — optional challenge only, not critical path.] |
| [Add A] | [Add B] | [Effect] | [Yes / No / Partial] |

---

## Validation Checklist

> Playtest checkpoints. ≥3 sessions per item. Note tester profile that revealed issues — difficulty problems are profile-specific.

### Onboarding (0-30 min)
- [ ] Genre-naive players complete tutorial without external help
- [ ] Zero confusion about what to do in first 5 min
- [ ] ≥1 tester says "I want to see what's next" within 15 min
- [ ] First failure produces visible learning (verbalizes wrong)

### Early Game (30 min - 2 hrs)
- [ ] Average reaches first competence within 60 min
- [ ] First major encounter passed within 3-5 attempts avg
- [ ] No "mechanic introduced too suddenly" complaints
- [ ] Players describe current goal without prompting

### Mid Game (2-10 hrs)
- [ ] Players discover ≥1 depth mechanic via organic play (no guide)
- [ ] Sessions report "want to try different build/strategy"
- [ ] No single difficulty axis dominates complaints — distributed
- [ ] Failed encounters: players identify cause without being told

### Late Game (10+ hrs)
- [ ] Final challenge feels like culmination
- [ ] Late-game failure not unfair (even if hard)
- [ ] Completers express reason to continue

### Accessibility
- [ ] All accessibility options function without breaking encounter intent
- [ ] Accessibility users feel competent not patronized
- [ ] Fixed elements accepted without negative reception

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| [Onboarding correctly calibrated for genre-naive?] | [game-designer] | [Date] | [Unresolved — schedule genre-naive playtests] |
| [First boss = correct spike or wall?] | [game-designer, systems-designer] | [Date] | [Unresolved — needs 5+ sessions for avg attempt count] |
| [Cross-system interactions produce unrecoverable states?] | [systems-designer] | [Date] | [Unresolved — targeted playtest with resource-constrained start] |
| [Add question] | [Owner] | [Date] | [Resolution] |
