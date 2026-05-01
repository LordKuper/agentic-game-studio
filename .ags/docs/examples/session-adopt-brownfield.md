# Example Session: /ags-adopt — Brownfield Project Onboarding

**Context:** Dev built game 3 months before discovering agent system. Has code, some design docs, rough notes — nothing in right format for skills.

**Starting State:**
- `Assets/Scripts/Gameplay/` — ~4000 lines C# (Unity 6)
- `design/` — 3 markdown files, rough notes, not GDD format
- No `design/gdd/`
- No `design/architecture/`
- No `.ags/project/`
- No `design/gdd/systems-index.md`
- CLAUDE.md exists (Unity 6000.0.30f1)

**Session Duration:** ~30 min
**Turns:** 8
**Outcome:** Adoption plan written, urgent gap (missing systems index) fixed inline

---

## Turn 1: User Invokes /ags-adopt

**User:**
> /ags-adopt

## Turn 2: Agent Detects Project State

**Agent (technical-director, via /ags-adopt, context: fork):**
> Running brownfield audit...
>
> **Phase 1: Project State Detection**
>
> | Artifact Category | Found | Notes |
> |------------------|-------|-------|
> | Engine configuration | ✅ | Unity 6000.0.30f1 in CLAUDE.md |
> | Source code | ✅ | ~4000 lines in `Assets/Scripts/Gameplay/` |
> | Design docs | ⚠️ | 3 files in `design/`, not GDD format |
> | Architecture docs | ❌ | `design/architecture/` missing |
> | Systems index | ❌ | `design/gdd/systems-index.md` missing |
> | Production tracking | ❌ | `.ags/project/` missing |
> | Sprint/story files | ❌ | None |
>
> **Phase 2: Format Audit (existing files)**
>
> Checking internal structure of 3 design files...
>
> *`design/combat-notes.md`:*
> - Informal notes, no GDD template sections
> - Has: rule descriptions, damage formula, rough balance numbers
> - Missing: Overview, Player Fantasy, Edge Cases, Dependencies, Tuning Knobs, Acceptance Criteria (6/8 sections)
>
> *`design/crafting-ideas.md`:*
> - Brainstorm-style, no structure
> - No formulas, no AC
> - Pre-GDD content — useful as input, not yet a GDD
>
> *`design/inventory.md`:*
> - Closest to GDD — has Overview + Rules
> - Missing: Player Fantasy, Formulas, Edge Cases, Tuning Knobs, AC (5/8 sections)
>
> **Phase 3: Gap Classification**
>
> | Gap | Severity | Reason |
> |-----|----------|--------|
> | No systems index | BLOCKING | `/ags-design-system`, `/ags-create-stories`, `/ags-gate-check` all need it |
> | GDD files not template format | HIGH | Skills can't ref TR-IDs from malformed GDDs |
> | No architecture docs | HIGH | Can't run `/ags-architecture-review` or `/ags-create-control-manifest` |
> | No production tracking | MEDIUM | Sprint/story skills won't work; not needed til production |
> | `crafting-ideas.md` — pre-GDD | LOW | Useful as input; needs full authoring before blocks anything |

## Turn 3: Agent Presents Migration Plan

**Agent:**
> **Phase 4: Migration Plan**
>
> 7 ordered steps. NEVER overwrites existing content — fills gaps only.
>
> | # | Step | Skill | Priority | Notes |
> |---|------|-------|----------|-------|
> | 1 | Create `design/gdd/systems-index.md` | `/ags-map-systems` or `/ags-adopt` inline | BLOCKING | Unblocks downstream skills |
> | 2 | Retrofit `design/inventory.md` → GDD | `/ags-design-system retrofit design/inventory.md` | HIGH | Adds 5 missing sections |
> | 3 | Retrofit `design/combat-notes.md` → GDD | `/ags-design-system retrofit design/combat-notes.md` | HIGH | Adds 6 missing sections |
> | 4 | Convert `design/crafting-ideas.md` → full GDD | `/ags-design-system crafting` (new authoring) | MEDIUM | Too informal to retrofit |
> | 5 | Create architecture ADRs from code | `/ags-reverse-document` + `/ags-architecture-decision` | HIGH | Captures decisions in code |
> | 6 | Create master architecture doc | `/ags-create-architecture` | HIGH | After ADRs |
> | 7 | Set up production tracking | `/ags-sprint-plan new` | MEDIUM | After architecture |
>
> **Important:** Step 1 first. Rest can parallel/reorder.
>
> May I write this to `docs/adoption-plan-2026-03-12.md`?

**User:**
> Yes. Write it.

**Agent writes adoption plan.**

## Turn 4: Agent Offers to Fix Most Urgent Gap

**Agent:**
> BLOCKING gap: missing `design/gdd/systems-index.md`.
>
> I can create one now by reading code + design docs to infer systems. Faster than `/ags-map-systems` from scratch — populated from what exists, you confirm/adjust.
>
> Do that now?

**User:**
> Yes, do it.

## Turn 5: Agent Reads Codebase, Drafts Systems Index

**Agent:**
> Reading `Assets/Scripts/Gameplay/`...
>
> System boundaries found in code:
>
> - `Assets/Scripts/Gameplay/Combat/` — damage, hit detection, status effects
> - `Assets/Scripts/Gameplay/Inventory/` — item mgmt, weight, stacks
> - `Assets/Scripts/Gameplay/Crafting/` — recipe lookup, result gen (refs inventory)
> - `Assets/Scripts/Gameplay/Movement/` — CharacterController movement, terrain mods (ref'd by combat)
> - `Assets/Scripts/Gameplay/UI/` — HUD, inventory screen, dialogue
> - `Assets/Scripts/Gameplay/World/` — tile map, terrain, chunks
>
> **Draft systems index:**
>
> | System | Category | Priority | Dependencies | GDD Status |
> |--------|----------|----------|--------------|------------|
> | Movement | Foundation | MVP | None | ❌ Missing |
> | World/Terrain | Foundation | MVP | Movement | ❌ Missing |
> | Combat | Core | MVP | Movement | ⚠️ Needs retrofit |
> | Inventory | Core | MVP | None | ⚠️ Needs retrofit |
> | Crafting | Feature | MVP | Inventory | ❌ Pre-GDD only |
> | UI / HUD | Presentation | MVP | Combat, Inventory, Movement | ❌ Missing |
>
> Look right? Anything missed/miscategorized?

**User:**
> Looks right. Add a Stamina system — it's in combat/ but should be its own system.

**Agent:**
> Added Stamina as Core / MVP:
>
> | Stamina | Core | MVP | Movement, Combat | ❌ Missing |
>
> Updated dep: Movement depends on Stamina (drains it).
>
> May I write to `design/gdd/systems-index.md`?

## Turn 6: User Approves

**User:**
> Yes

**Agent writes `design/gdd/systems-index.md`.**

## Turn 7: Agent Shows What's Now Unlocked

**Agent:**
> `design/gdd/systems-index.md` created.
>
> **BLOCKING gap resolved.** Skills now run:
>
> - ✅ `/ags-design-system [name]` — guided GDD authoring
> - ✅ `/ags-design-system retrofit [path]` — fill gaps in existing docs
> - ✅ `/ags-gate-check` — has systems index to validate
> - ✅ `/ags-help` — shows correct phase workflow
>
> **Recommended next steps (from plan):**
>
> 1. `/ags-design-system retrofit design/inventory.md` — most complete existing doc, easiest win
> 2. `/ags-design-system retrofit design/combat-notes.md`
> 3. `/ags-design-system crafting` — author crafting GDD using ideas file as input
>
> After those 3: 5/7 systems with complete GDDs. Then `/ags-review-all-gdds` for cross-system consistency before architecture work.

## Turn 8: User Confirms Plan

**User:**
> Perfect. I'll start with the inventory retrofit.

**Agent:**
> Run:
> ```
> /ags-design-system retrofit design/inventory.md
> ```
>
> Skill detects existing Overview + Rules sections, shows missing sections, runs section cycle for gaps only. Existing content untouched.

---

## What This Demonstrates

- **FORMAT audit, not existence audit**: `/ags-adopt` checks internal structure vs skill expectations. File named `gdd.md` with no template sections = gap.
- **Migration, not replacement**: existing content never overwritten. Plan fills gaps only.
- **BLOCKING gaps surface first**: missing systems index identified BLOCKING because most skills depend on it.
- **Inline urgent gap fix**: agent fixes immediately in same session, not just reports.
- **Inferred from code**: systems index bootstrapped from codebase — brownfield code already contains answer.
- **Retrofit vs new authoring**: `/ags-design-system retrofit` for existing-but-incomplete; `/ags-design-system` for net-new. Plan distinguishes per file.
- **Context: fork**: `/ags-adopt` runs in forked context, avoids polluting main session with large file reads.
