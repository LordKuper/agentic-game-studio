# Example Session: Designing the Crafting System

**Context:** Solo indie dev, week 2, designs crafting system from game pillars.

**Starting State:**
- Game concept + pillars defined
- Engine set up (Unity 6000.0.30f1)
- Core combat designed, not implemented
- Pillar 2: "Emergent Discovery Through Experimentation"

**Session Duration:** ~45 min
**Turns:** 12
**Outcome:** Complete crafting GDD ready for impl

---

## Turn 1: User Initiates Design

**User:**
> I need to design the crafting system. Pillar 2 is "Emergent Discovery Through Experimentation", so I want players to discover recipes by trying combinations.

## Turn 2: Agent Asks Clarifying Questions

**Agent (game-designer):**
> I'll help design crafting with discovery at core. Before proposing mechanics:
>
> 1. **Discovery Method**: Pure trial-and-error, or hints/clues?
> 2. **Failure Punishment**: Invalid combo:
>    - Materials lost (harsh)
>    - Partial refund (moderate)
>    - No cost (forgiving)
> 3. **Progression**: Crafting skill improves? What does it unlock?
> 4. **Scope**: Core pillar or supporting system?
> 5. **Reference Games**: Loved/hated systems? (Minecraft, Terraria, Stardew, Potion Craft?)

## Turn 3: User Provides Context

**User:**
> 1. I want hints that unlock gradually - start with pure discovery, earn hints through skill
> 2. Moderate punishment - waste materials but gain partial XP
> 3. Yes, skill unlocks hint system and improves success rates
> 4. Core feature - one of the three main loops (combat, exploration, crafting)
> 5. Love Potion Craft's deduction mechanic. Hate Minecraft wikis being mandatory.

## Turn 4: Agent Presents Options

**Agent:**
> Perfect context! Based on emergent discovery + deduction-over-wikis + Potion Craft inspiration:
>
> **Option A: Pure Randomized Discovery**
> - **Mechanics**: Try 2-4 ingredient combos, roll for success
> - **Hints**: None — players document manually
> - **Pros**:
>   - Max emergent discovery
>   - Community sharing doesn't break loop
>   - Strong pillar align
> - **Cons**:
>   - High frustration for completionists
>   - Low skill expression (RNG)
>   - Wiki may feel required despite intent
> - **Aligns**: ✅✅ Discovery, ⚠️ Meaningful Choices (RNG reduces agency)
>
> **Option B: Tag-Based Deduction (Potion Craft-style)**
> - **Mechanics**: Each ingredient has hidden tags (Fire, Water, Healing, Damage, etc.)
>   - Recipes need specific tag combos
>   - Skill 0: No tags visible, pure experimentation
>   - Skill 5: Inspect items, see 1 tag
>   - Skill 10: See 2 tags, etc.
> - **Hints**: Tags ARE hints, unlocked by skill
> - **Pros**:
>   - Rewards observation/pattern recognition (skill expression)
>   - Progressive scaffolding — accessible to all
>   - "Aha!" moments when patterns click
>   - No wiki — discoverable in-game
> - **Cons**:
>   - More complex (tag system + UI)
>   - Careful tag design needed to avoid degeneracy
> - **Aligns**: ✅✅ Discovery, ✅✅ Meaningful Choices
>
> **Option C: Ingredient Compatibility Matrix**
> - **Mechanics**: Each ingredient pairs well/poorly
>   - Success rate = compatibility score
>   - Skill 0: No compat shown, ~30% base
>   - Higher skill: Color-coded compat (red/yellow/green)
> - **Hints**: Visual compat indicators unlock with skill
> - **Pros**:
>   - Simple to implement/understand
>   - Clear progression arc
>   - Feels fair (players know why they failed)
> - **Cons**:
>   - Less emergent (more prescriptive)
>   - Optimal recipes obvious with hints
> - **Aligns**: ✅ Discovery, ✅ Meaningful Choices
>
> **My Recommendation: Option B (Tag-Based Deduction)**
>
> **Reasoning:**
> - Only option with TRUE emergent discovery via systems
> - Mastery via tag patterns, not memorizing recipes
> - Progressive reveal serves Explorers + Achievers
> - Potion Craft proved it works
> - Avoids wiki — solvable in-game
>
> Which option resonates? Or hybrid?

## Turn 5: User Decides with Modification

**User:**
> Option B feels perfect, but I want to add: failed attempts should give hints about which tags were WRONG, not just which were right. Like "You added Fire, but this recipe needs Water instead."

## Turn 6: Agent Incorporates Feedback and Drafts

**Agent:**
> Excellent! Adds deduction game ON TOP of tag discovery. Failed attempts = info, not waste.
>
> Drafting **Overview** + **Player Fantasy**:
>
> ---
>
> ## Overview
>
> The crafting system uses a tag-based deduction mechanic where players experiment with ingredient combinations to discover recipes. Each ingredient has 1-3 hidden tags (Fire, Water, Earth, Air, Healing, Damage, Poison, etc.). Recipes require specific tag combinations. Failed attempts waste materials but provide feedback ("This needs Water, not Fire") that teaches players tag patterns. As crafting skill increases, players unlock the ability to inspect ingredients to see their tags directly, transforming pure experimentation into informed deduction.
>
> ## Player Fantasy
>
> "I'm an alchemist-scientist discovering nature's hidden patterns. Each failed potion teaches me something new about how elements interact. Over time, I develop intuition - I can look at a mushroom and KNOW it contains Earth and Poison tags before even testing it. I don't need recipes from a wiki; I understand the RULES that generate recipes, and I can create my own solutions."
>
> **Target MDA Aesthetics**:
> - Discovery (primary): Uncovering hidden tag patterns
> - Challenge (secondary): Deduction puzzle
> - Expression (tertiary): Creating custom recipes once rules mastered
>
> ---
>
> Capture intent? Continue to **Detailed Rules**?

## Turn 7: User Approves and Requests Next Section

**User:**
> Yes, that's perfect! Continue to Detailed Rules.

## Turn 8: Agent Drafts Rules, Asks About Edge Case

**Agent:**
> Drafting Detailed Rules...
>
> ---
>
> ## Detailed Rules
>
> ### Core Loop
> 1. Player selects 2-4 ingredients from inventory
> 2. Player activates crafting station
> 3. System checks ingredient tags against recipe database
> 4. **If exact tag match**: Success! Recipe learned, item created, full XP awarded
> 5. **If partial match** (some tags correct): Failure. Feedback: "Close! This needs [missing tag]." Materials consumed, 40% XP awarded, recipe marked as "Discovered (Incomplete)"
> 6. **If no match**: Failure. Feedback: "Incompatible. Try replacing [wrong tag] with [needed tag]." Materials consumed, 15% XP awarded
>
> ### Tag System
> - 12 core tags: Fire, Water, Earth, Air, Healing, Damage, Poison, Light, Dark, Metal, Plant, Magic
> - Each ingredient has 1-3 tags (simple = 1, rare = 3)
> - Each recipe needs 2-6 total tags (basic = 2, advanced = 6)
> - Tag combos must be unique
>
> ### Skill Progression
> - Skill 0-4: No inspection. Pure experimentation.
> - Skill 5-9: Inspect, see 1 tag
> - Skill 10-14: See 2 tags per ingredient
> - Skill 15+: See all tags
>
> ### XP Gain
> ```
> success_xp = base_xp * recipe_complexity^1.3
> failure_xp_partial = success_xp * 0.40
> failure_xp_none = success_xp * 0.15
> ```
>
> Where `recipe_complexity` = unique tags required (2-6)
>
> ---
>
> **Question:** Player discovers tag combo NOT matching any recipe. Should we:
> A) Allow it, create "junk items" (wasted materials)
> B) Always provide feedback steering toward valid recipes
> C) Procedurally generate minor potion for any valid tag combo not in DB
>
> Affects "experimentation" feel — A harsh but realistic, C rewards all experimentation.

## Turn 9: User Resolves Edge Case

**User:**
> Option C - let's procedurally generate minor potions. If someone finds Fire+Water (not a designed recipe), give them "Warm Water" that restores 5 HP. Rewards experimentation even when "off the beaten path."

## Turn 10: Agent Completes Remaining Sections

**Agent:**
> Perfect! Keeps experimentation always rewarding. Updating rules...
>
> [Agent drafts Formulas, Edge Cases, Dependencies, Tuning Knobs, AC sections — each shown, minor feedback incorporated]
>
> [After all sections]
>
> Complete crafting GDD ready. Summary:
>
> **8 Required Sections:**
> ✅ Overview - Tag-based deduction
> ✅ Player Fantasy - Alchemist-scientist
> ✅ Detailed Rules - Core loop with tag matching + feedback
> ✅ Formulas - XP curves, success rates, skill thresholds
> ✅ Edge Cases - Procedural minor potions, inventory full, max skill
> ✅ Dependencies - Inventory system, progression integration
> ✅ Tuning Knobs - XP rates, skill thresholds, tag count, feedback verbosity
> ✅ AC - Discover 10 recipes in 30 min sans wiki, skill progression meaningful
>
> **Delegations:**
> - Consulted `systems-designer` for XP curve validation
> - Consulted `systems-designer` for material cost balancing
>
> May I write to `design/gdd/crafting-system.md`?

## Turn 11: User Approves Writing

**User:**
> Yes, write it!

## Turn 12: Agent Writes File and Offers Next Steps

**Agent:**
> ✅ Created `design/gdd/crafting-system.md`
>
> **Next Steps:**
> 1. Run `/design-review design/gdd/crafting-system.md` to validate vs 8-section standard
> 2. Create data file structure in `assets/data/` for ingredients/recipes
> 3. When implementing: `/team-combat` pattern but for crafting (game-designer → gameplay-programmer → ui-programmer → qa-lead)
>
> Run design review now, or read it first?

---

## Session Summary

**What Worked:**
- Agent asked Qs upfront, didn't guess
- 3 distinct options with pros/cons + pillar align
- User mod ("wrong tag feedback") incorporated immediately
- Edge case (non-recipe combos) flagged proactively
- Each section approved before next
- Explicit permission before writing file

**Key Collaborative Moments:**
- Turn 2: Agent asked 5 Qs to understand vision
- Turn 4: 3 options, recommended but deferred
- Turn 5: User modified rec, agent incorporated
- Turn 8: Agent caught edge case, asked
- Turn 9: User choice (procedural potions) changed direction
- Turn 10: Approval before writing

**Files Created:**
- `design/gdd/crafting-system.md` (8-section GDD)

**Context Usage:** ~35% (estimated)
**User Satisfaction:** High — user made all creative decisions
