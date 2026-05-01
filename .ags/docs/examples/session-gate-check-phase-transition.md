# Example Session: /ags-gate-check Phase Transition — Systems Design → Technical Setup

**Context:** Solo indie dev, end of week 5. Systems Design feels complete — 6 MVP GDDs, cross-review done, engine set. Wants to advance to Technical Setup.

**Starting State:**
- `.ags/project/stage.txt` — currently `systems-design`
- `design/gdd/systems-index.md` — 6 MVP systems, all `Status: GDD Complete`
- `design/gdd/` — 6 GDD files
- `design/gdd/gdd-cross-review-2026-03-10.md` — verdict: CONCERNS (1 low-priority)
- Engine configured (Unity 6000.0.30f1, `/ags-setup-engine` complete)

**Session Duration:** ~20 min
**Turns:** 7
**Outcome:** Gate passed, `stage.txt` → `technical-setup`, clear next-steps list

---

## Turn 1: User Initiates Gate Check

**User:**
> /ags-gate-check

## Turn 2: Agent Detects Phase, Runs Check

**Agent (via /ags-gate-check):**
> Reading `.ags/project/stage.txt` — current phase: **systems-design**
>
> Running gate: **Systems Design → Technical Setup**
>
> ---
> **Required Artifacts Checklist:**
>
> | Artifact | Requirement | Status |
> |----------|-------------|--------|
> | `design/gdd/systems-index.md` | Exists, all MVP listed | ✅ |
> | Per-system GDDs | All MVP have GDD | ✅ (6/6) |
> | `/ags-design-review` per GDD | Each reviewed | ✅ (6/6 found) |
> | Cross-GDD review | `design/gdd/gdd-cross-review-*.md` exists | ✅ |
> | Cross-review verdict | PASS or CONCERNS (not FAIL) | ✅ CONCERNS |
> | Engine configured | `.ags/project/stage.txt` or CLAUDE.md entry | ✅ Unity 6000.0.30f1 |
>
> ---
> **Content Scan:**
>
> GDD completeness (8 sections each)...
>
> | GDD | Missing Sections |
> |-----|-----------------|
> | movement-system.md | None ✅ |
> | stamina-system.md | None ✅ |
> | combat-system.md | None ✅ |
> | inventory-system.md | None ✅ |
> | crafting-system.md | None ✅ |
> | exploration-system.md | None ✅ |
>
> ---
> **Cross-Review Concerns (from `gdd-cross-review-2026-03-10.md`):**
>
> 1. ⚠️ LOW — Crafting + inventory both define "item stack size" independently with different defaults (crafting: 99, inventory: 64). One should be authority; other should ref. Not blocking — resolve via ADR.
>
> ---
> **Verdict: PASS**
>
> All artifacts present. Cross-review concern LOW, resolvable via ADR in Technical Setup. Normal at this stage.
>
> **May I advance `.ags/project/stage.txt` to `technical-setup`?**

## Turn 3: User Confirms

**User:**
> Yes, advance it.

## Turn 4: Agent Updates Stage, Presents Next Steps

**Agent:**
> Updated `.ags/project/stage.txt` → `technical-setup`
>
> ---
> **You are now in: Technical Setup**
>
> Establish technical foundation before production coding. Goal: complete approved architecture so every Pre-Production story has clear technical guidance.
>
> **Required steps (in order):**
>
> 1. **`/ags-create-architecture`** — Master architecture doc. System decomposition, data flow, layer boundaries, module responsibilities.
>
> 2. **`/ags-architecture-decision` (one per significant choice)** — At minimum:
>    - Rendering approach (2D camera, viewport, etc.)
>    - Save/load strategy
>    - Audio bus architecture
>    - Item stack size authority (crafting vs inventory)
>
> 3. **`/ags-architecture-review`** — Validates all ADRs for completeness, dep ordering, GDD traceability. Produces TR-registry.
>
> 4. **`/ags-create-control-manifest`** — Converts Accepted ADRs to flat programmer rules sheet. Impl agents consult during coding.
>
> 5. **`/ags-gate-check`** — Technical Setup → Pre-Production. Requires: architecture doc, all ADRs Accepted, control manifest, dep ordering resolved.
>
> **Optional but recommended:**
> - `/tech-debt` scan if carrying spike code forward
> - `/ags-setup-engine` re-run if Unity 6000.0.30f1 API snapshots not fetched
>
> **Tip:** Start with `/ags-architecture-decision` for stack size conflict. Quick win, closes open question before ambiguity in 2 ADRs later.

## Turn 5: User Asks About Cross-Review Concern

**User:**
> What do I actually write in the ADR for the stack size conflict?

## Turn 6: Agent Advises

**Agent:**
> Decision: **which system owns canonical `max_stack_size`?**
>
> Options:
>
> **Option A: Inventory owns it**
> - `ItemData` ScriptableObject (inventory) gets `public int MaxStackSize = 64;`
> - Crafting reads from `ItemData`, never sets own
> - Pros: inventory = source of truth for item properties
> - Cons: crafting team needs to know to look at inventory data
>
> **Option B: Item data ScriptableObject owns it (shared asset)**
> - `ItemData.MaxStackSize` = authority for both
> - Both crafting + inventory ref `ItemData`
> - Pros: single source, no coupling between modules
> - Cons: one more thing in shared data layer
>
> Most studios go B (shared resource) once >2 systems touch item data. Either valid for solo project.
>
> Run `/ags-architecture-decision` — guides ADR format including Dependencies section linking to 2 GDDs.

## Turn 7: User Wraps Up

**User:**
> Got it. I'll start with /ags-architecture-decision for this.

**Agent:**
> Good start. ADR documents decision, reasoning, GDD requirements addressed — months later, rationale preserved.
>
> Phase gate logged. Run `/ags-help` during Technical Setup to see progress vs required checklist.

---

## What This Demonstrates

- **Gate check is automated, not manual**: agent reads artifacts + checks them — user doesn't fill form
- **CONCERNS ≠ FAIL**: LOW concern passes. FAIL would require resolution before advancing.
- **Stage.txt is authority**: status line, `/ags-help`, all skills read from `.ags/project/stage.txt` — updating changes what every subsequent skill sees
- **Next steps phase-specific**: agent gives ordered checklist for Technical Setup, not generic
- **Surfaces carry-forward work**: stack size conflict was cross-review note; gate ensures it becomes concrete ADR, not lost
- **One advance per gate**: user confirmed explicitly. Gate doesn't auto-advance; human confirmation required.
