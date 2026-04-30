# Example Session: Scope Crisis - Strategic Decision Making

**Context:** Solo indie dev, week 8 (month 2), approaching Alpha.

**Crisis:**
- Alpha milestone in 2 weeks
- Crafting system (Pillar 2) not implemented
- Lead programmer says full crafting takes 3 weeks
- Investor demo at Alpha — must show all pillars

**Session Duration:** ~25 min
**Turns:** 8
**Outcome:** Strategic scope decision with documented ADR

---

## Turn 1: User Escalates to Creative Director

**User:**
> We have a problem. Alpha is in 2 weeks. The crafting system (Pillar 2 "Emergent Discovery") isn't implemented yet. Lead programmer says it'll take 3 weeks to implement fully. But the investor demo is at Alpha and they need to see both pillars (Combat + Crafting). What should I do?

## Turn 2: Creative Director Gathers Context

**Agent (creative-director):**
> Classic pillar vs schedule conflict — what I'm here for. Gather context first.
>
> Reviewing docs:
> - `.ags/project/milestones/alpha.md` (milestone def)
> - `design/pillars.md` (non-negotiables)
> - `design/gdd/crafting-system.md` (what was designed)
> - `.ags/project/sprints/sprint-current.md` (current work)
>
> [Reads documents]
>
> **Context:**
> - Pillar 2: "Emergent Discovery Through Experimentation" (crafting = primary expression)
> - Alpha success: "All pillars demonstrated playable"
> - Investor demo: Validate vision, secure next funding round
> - Current crafting: Tag-based deduction, 12 tags, ~50 recipes, progressive hint unlocking
>
> **Clarifying Qs:**
> 1. Alpha date **hard** (investor won't reschedule) or **soft** (1 more week possible)?
> 2. **Minimum crafting** that demos Pillar 2? (1 recipe? 10? Full tag system?)
> 3. What **breaks** if crafting cut entirely? (Game still make sense?)
> 4. How **critical** is investor relationship? (Nice-to-have vs make-or-break?)
> 5. **Team status**? (Burned out? Fresh, can push?)

## Turn 3: User Provides Decision Context

**User:**
> 1. Hard date - investor flying in specifically for demo, can't reschedule
> 2. I think minimum is: discovery mechanic working (tag system) + 10 recipes. Doesn't need polish.
> 3. Without crafting, Alpha only shows combat. That's 50% of the vision - investor might think it's just another combat game
> 4. Make-or-break. This investor is our only lead. No funding = project dies.
> 5. Just me solo, I'm not burned out yet.

## Turn 4: Creative Director Frames Decision

**Agent:**
> Now I understand what's at stake.
>
> **Core Question:**
> Full crafting + slip Alpha, simplified crafting + make Alpha, or cut crafting + risk investor missing full vision?
>
> **What's at Stake:**
> - **Vision Integrity**: Investor see Pillar 2 at all?
> - **Schedule Trust**: Keep Alpha commitment?
> - **Project Survival**: No funding = no game
> - **Quality Standards**: Is "rough but working" acceptable?
>
> **Decision Criteria (priority order):**
> 1. **Investor Confidence** (highest — survival depends on it)
> 2. **Pillar Representation** (high — need to see full vision)
> 3. **Schedule Integrity** (medium — matters less than funding)
> 4. **Polish Quality** (lowest — vertical slice can be rough)
>
> Three options...

## Turn 5: Creative Director Presents Options

**Agent:**
> **Option A: Full Crafting (3 weeks, slip Alpha by 1 week)**
>
> **Execution:**
> - Full tag system, all 50 recipes
> - Full UI polish, all features from GDD
> - Miss Alpha, reschedule investor demo (+1 week)
>
> **Pros:**
> - ✅ Pillar 2 production-quality
> - ✅ No design compromises
> - ✅ Polished first impression
>
> **Cons:**
> - ❌ Breaks investor commitment
> - ❌ Signals schedule issues ("if Alpha slipped, will release?")
> - ❌ May damage trust more than rough demo
>
> **Risk: CRITICAL**
> - Investor may not reschedule (busy schedule)
> - Even if yes, slip damages credibility
> - Unlikely to succeed given hard date
>
> **Verdict: NOT RECOMMENDED** — Date is hard, non-negotiable
>
> ---
>
> **Option B: Simplify Crafting to Core Pillar (1.5 weeks, make Alpha)**
>
> **Execution:**
> - Core tag discovery mechanic ONLY
> - 10 recipes (demonstrates system)
> - Basic UI (functional, not polished)
> - No progressive hint unlock (all tags visible at Skill 0 for demo clarity)
> - No failed-attempt feedback (success/fail only)
>
> **Scope: "Vertical Slice of Pillar 2"**
> - Players CAN discover recipes via tagged ingredients
> - Players CAN learn tag patterns, deduce new recipes
> - SOUL of Pillar 2 present, just unpolished
>
> **Pros:**
> - ✅ Makes Alpha (schedule integrity)
> - ✅ Pillar 2 demoed (full vision)
> - ✅ Core mechanic playable + provable
> - ✅ "Alpha → Beta" narrative ("rough → polished")
>
> **Cons:**
> - ⚠️ Rough first impression (vertical slices expected rough)
> - ⚠️ "Feels small" risk (mitigation: roadmap showing post-Alpha expansion)
>
> **Risk: MEDIUM**
> - Investor might not "get" pillar from 10 recipes (mitigation: good demo script)
> - Might feel incomplete (mitigation: "vertical slice proving concept")
>
> **Verdict: RECOMMENDED** — Balances all constraints
>
> ---
>
> **Option C: Cut Crafting from Alpha, Polish Combat (0 weeks, make Alpha)**
>
> **Execution:**
> - Alpha with Pillar 1 (Combat) only, fully polished
> - Extra 2 weeks → super-polish combat, content, shine
> - Crafting as "design doc + concept art" (not playable)
>
> **Pros:**
> - ✅ Deadline made, zero stress
> - ✅ What's there is AAA-polished
> - ✅ No "half-baked" risk
>
> **Cons:**
> - ❌ Pillar 2 not demoed (50% vision missing)
> - ❌ "Another combat game" — undifferentiated
> - ❌ Harder to fund "potential" vs "proven"
> - ❌ If combat alone doesn't wow, no fallback
>
> **Risk: HIGH**
> - Investor won't see what makes game special
> - "Combat fine, but where's the hook?"
> - Competitors have combat — need differentiator shown
>
> **Verdict: NOT RECOMMENDED** — Too risky for make-or-break
>
> ---
>
> ## My Recommendation: Option B (Simplified Crafting Core)
>
> **Reasoning:**
> 1. **Investor Confidence**: Making committed date = reliability
> 2. **Pillar Visibility**: Rough+working > polished+missing. Investors back VISION, not polish.
> 3. **Demo Narrative**: "Alpha vs Beta progression" is compelling — shows trajectory
> 4. **Risk Mitigation**: If crafting doesn't land, combat solid. If we cut crafting (C) and combat doesn't wow alone, nothing.
> 5. **Historical Precedent**: Hades, Dead Cells, Slay the Spire all showed rough Early Access verticals; investors loved SYSTEMS over polish
>
> **Option B Sacrifices:**
> - Polish (acceptable for vertical slice)
> - Feature completeness (50 → 10 recipes)
> - Progressive unlocks (deferred post-Alpha)
>
> **Option B Protects:**
> - Schedule trust
> - Pillar representation
> - Vision clarity
> - Project survival (funding)
>
> **If B chosen:**
> 1. Create ADR documenting scope decision
> 2. Update `design/gdd/crafting-system.md` with "Alpha Scope" markers
> 3. Brief gameplay-programmer on simplified impl
> 4. Create demo script: how to PRESENT 10-recipe system to show 50-recipe vision
>
> **But this is your call.** You know investor better. If you believe they value polish > pillar rep, C might be right. If they'll forgive 1-week slip, A safest creatively.
>
> Decision?

## Turn 6: User Makes Decision

**User:**
> Option B. The investor is technical and will appreciate the systems thinking even if it's rough. Let's simplify the crafting scope to make Alpha. Can you create the ADR and update the GDD?

## Turn 7: Creative Director Documents Decision

**Agent:**
> Excellent. Documenting thoroughly so future-you understands why.
>
> **Creating:**
> 1. ADR-007: Alpha Crafting Scope Reduction
> 2. Updated design/gdd/crafting-system.md with Alpha/Beta scope markers
> 3. Updated .ags/project/milestones/alpha.md success criteria
> 4. Demo script for investor presentation
>
> [Creates documents]
>
> **ADR Summary:**
> ```markdown
> ## Decision
> Simplify crafting system to core tag discovery mechanic (10 recipes)
> for Alpha milestone. Defer full implementation (50 recipes + progressive
> unlocks) to post-Alpha.
>
> ## Context
> - Alpha in 2 weeks (hard date, investor demo)
> - Full crafting requires 3 weeks
> - Investor demo is make-or-break for funding
>
> ## Consequences
> Positive:
> - Makes Alpha deadline, preserves investor trust
> - Both pillars demonstrated (rough but playable)
> - Can show Alpha→Beta improvement narrative
>
> Negative:
> - First impression is "rough"
> - Limited recipe variety
> - Missing progressive unlock system
>
> ## Validation Criteria
> We'll know this was the right choice if:
> - Investor demo successfully shows both pillars
> - Funding secured for next development phase
> - Post-Alpha sprint can implement full system without rework
> ```
>
> **Updated GDD Markers:**
> ```markdown
> ## Detailed Rules
>
> ### Core Loop (**ALPHA SCOPE**)
> [Core mechanic description]
>
> ### Progressive Unlocks (**POST-ALPHA**)
> [Skill-based hint system - deferred]
>
> ### Recipe Catalog
> - Alpha: 10 recipes (demonstrates tag variety)
> - Beta: 50 recipes (full content)
> ```
>
> **Demo Script Guidance:**
> ```markdown
> ## Presenting Crafting to Investor (10-recipe Alpha)
>
> **Frame it as proof-of-concept:**
> "This is a vertical slice of the crafting pillar. You'll see 10 recipes
> here, but the SYSTEM supports unlimited recipes through tag combinations.
> Watch how players deduce patterns..."
>
> **Show, don't tell:**
> - Live demo: Discover 2 recipes on the fly
> - Explain: "Notice Fire+Water made Steam Potion. Now the player knows
>   Fire+Water is valid. They can deduce that Fire+Earth might work..."
> - Connect to pillar: "This is Emergent Discovery - the game teaches rules,
>   players become scientists."
>
> **Roadmap slide:**
> "Alpha: 10 recipes (proof of concept)
>  Beta: 50 recipes (content expansion)
>  Release: 100+ recipes (includes community-designed recipes)"
>
> **Emphasize systems:**
> "We're not making a content game, we're making a systems game. The tag
> system can generate endless combinations. We're building a platform for
> discovery, not a recipe database."
> ```
>
> Proceed to write?

## Turn 8: User Approves and Agent Completes

**User:**
> Yes, create all those documents and also notify the gameplay-programmer about the scope change.

**Agent:**
> ✅ Complete! Created/Updated:
> - `design/architecture/adr-007-alpha-crafting-scope.md`
> - `design/gdd/crafting-system.md` (Alpha/Beta scope markers added)
> - `.ags/project/milestones/alpha.md` (updated success criteria)
> - `.ags/project/demo-script-investor-alpha.md` (new presentation guide)
>
> **Notifications:**
>
> To `gameplay-programmer`:
> > "Crafting scope simplified for Alpha: Core tag mechanic + 10 recipes only.
> > Progressive unlocks and full recipe set deferred to post-Alpha. Updated
> > GDD has implementation guidance. Estimated 1.5 weeks to implement Alpha scope."
>
> To `producer`:
> > "Strategic decision made: Simplified crafting to make Alpha deadline.
> > ADR-007 documents rationale. Sprint plan needs updating to reflect new scope."
>
> **Next Steps:**
> 1. Gameplay-programmer can start simplified crafting impl
> 2. You can practice demo script
> 3. Producer updates sprint plan with new tasks
>
> **Validation:**
> Right decision when:
> - Alpha ships on time, both pillars playable
> - Investor demo communicates vision
> - Funding secured for continued dev
>
> Good luck with demo. Right call — vision over polish for this audience.

---

## Session Summary

**What Worked:**
- Read context docs before proposing solutions
- 5 clarifying Qs to understand constraints
- Framed decision (stakes, eval criteria)
- 3 distinct options with honest pros/cons
- Clear recommendation + reasoning, deferred to user
- Documented thoroughly: ADR, all affected docs
- Demo script to support presentation

**Key Collaborative Moments:**
- Turn 2: Gathered context before solutions
- Turn 3: User gave criteria priority (funding > schedule > polish)
- Turn 5: 3 options, recommendation, "your call"
- Turn 6: User strategic decision
- Turn 7: Demo script supporting presentation
- Turn 8: Cascaded decision to depts

**Strategic Decision Quality:**
- User made final call
- Agent gave expert analysis via theory + precedent
- Trade-offs explicit + honest
- Decision documented for future
- Success criteria defined

**Files Created/Updated:**
- ADR-007 (decision)
- Updated crafting GDD (scope markers)
- Updated Alpha milestone
- Demo script

**Context Usage:** ~30%
**User Satisfaction:** High — confident decision, full info
