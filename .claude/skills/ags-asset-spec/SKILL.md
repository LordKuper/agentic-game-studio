---
name: ags-asset-spec
description: "Generate per-asset visual specifications and AI generation prompts from GDDs, level docs, or character profiles. Produces structured spec files and updates the master asset manifest. Run after art bible and GDD/level design are approved, before production begins."
argument-hint: "[system:<name> | level:<name> | character:<name>]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task, AskUserQuestion
---

If no argument is provided, check whether `design/assets/asset-manifest.md` exists:
- If it exists: read it, find the first context (system/level/character) with any asset at status "Needed" but no spec file written yet, and use `AskUserQuestion`:
  - Prompt: "The next unspecced context is **[target]**. Generate asset specs for it?"
  - Options: `[A] Yes — spec [target]` / `[B] Pick a different target` / `[C] Stop here`
- If no manifest: fail with:
  > "Usage: `/ags-asset-spec system:<name>` — e.g., `/ags-asset-spec system:tower-defense`
  > Or: `/ags-asset-spec level:iron-gate-fortress` / `/ags-asset-spec character:frost-warden`
  > Run after your art bible and GDDs are approved."

---

## Phase 0a: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `design/art/ags-art-bible.html` | `/ags-art-bible` | STOP. "No art bible. Run `/ags-art-bible` first." |
| Target system/level/character GDD | `/ags-design-system` | STOP. "No GDD for target. Run `/ags-design-system [name]` first." |

If STOP triggers, exit verdict **BLOCKED**.

---

## Phase 0: Parse Arguments

Extract:
- **Target type**: `system`, `level`, or `character`
- **Target name**: the name after the colon (normalize to kebab-case)

Spawn both `art-director` and `technical-artist` in parallel for the review loop.

---

## Phase 1: Gather Context

Read all source material **before** asking the user anything.

### Required reads:
- **Art bible**: Read `design/art/ags-art-bible.html` — fail if missing:
  > "No art bible found. Run `/ags-art-bible` first — asset specs are anchored to the art bible's visual rules and asset standards."
  Extract: Visual Identity Summary (Section 1), Color Palette (Section 4), Art Style (Section 5), Asset Production Standards (Section 10 — dimensions, formats, polycount budgets, texture resolution tiers).

- **Technical preferences**: Read `.ags/rules/technical-preferences.md` — extract performance budgets and naming conventions.

### Source doc reads (by target type):
- **system**: Read `design/gdd/[target-name].md`. Extract the **Visual/Audio Requirements** section. If it doesn't exist or reads `[To be designed]`:
  > "The Visual/Audio section of `design/gdd/[target-name].md` is empty. Either run `/ags-design-system [target-name]` to complete the GDD, or describe the visual needs manually."
  Use `AskUserQuestion`: `[A] Describe needs manually` / `[B] Stop — complete the GDD first`
- **level**: Read `design/levels/[target-name].md`. Extract art requirements, asset list, VFX needs, and the art-director's production concept specs from Step 4.
- **character**: Read `design/narrative/characters/[target-name].md` or search `design/narrative/` for the character profile. Extract visual description, role, and any specified distinguishing features.

### Optional reads:
- **Existing manifest**: Read `design/assets/asset-manifest.md` if it exists — extract already-specced assets for this target to avoid duplicates.
- **Related specs**: Glob `design/assets/specs/*.md` — scan for assets that could be shared (e.g., a common UI element specced for one system might apply here too).

### Present context summary:
> **Asset Spec: [Target Type] — [Target Name]**
> - Source doc: [path] — [N] asset types identified
> - Art bible: found — Asset Standards at Section 8
> - Existing specs for this target: [N already specced / none]
> - Shared assets found in other specs: [list or "none"]

---

## Phase 2: Asset Identification

From the source doc, extract every asset type mentioned — explicit and implied.

**For systems**: look for VFX events, sprite references, UI elements, audio triggers, particle effects, icon needs, and any "visual feedback" language.

**For levels**: look for unique environment props, atmospheric VFX, lighting setups, ambient audio, skybox/background, and any area-specific materials.

**For characters**: look for sprite sheets (idle, walk, attack, death), portrait/avatar, VFX attached to abilities, UI representation (icon, health bar skin).

Group assets into categories:
- **Sprite / 2D Art** — character sprites, UI icons, tile sheets
- **VFX / Particles** — hit effects, ambient particles, screen effects
- **Environment** — props, tiles, backgrounds, skyboxes
- **UI** — HUD elements, menu art, fonts (if custom)
- **Audio** — SFX, music tracks, ambient loops *(note: audio specs are descriptions only — no generation prompts)*
- **3D Assets** — meshes, materials (if applicable per engine)

Present the full identified list to the user. Use `AskUserQuestion`:
- Prompt: "I identified [N] assets across [N] categories for **[target]**. Review before speccing:"
- Show the grouped list in conversation text first
- Options: `[A] Proceed — spec all of these` / `[B] Remove some assets` / `[C] Add assets I didn't catch` / `[D] Adjust categories`

Do NOT proceed to Phase 3 without user confirmation of the asset list.

---

## Phase 3: Spec Generation

Spawn specialist agents in parallel. **Issue all Task calls simultaneously — do not wait for one before starting the next.**

**`art-director`** via Task:
- Provide: full asset list from Phase 2, art bible Visual Identity Statement, Color System, Shape Language, the source doc's visual requirements, and any reference games/art mentioned in the art bible Section 9
- Ask: "For each asset in this list, produce: (1) a 2–3 sentence visual description anchored to the art bible's shape language and color system — be specific enough that two different artists would produce consistent results; (2) a generation prompt ready for use with AI image tools (Midjourney/Stable Diffusion style — include style keywords, composition, color palette anchors, negative prompts); (3) which art bible rules directly govern this asset (cite by section). For audio assets, describe the sonic character instead of a generation prompt."

**`technical-artist`** via Task:
- Provide: full asset list, art bible Asset Standards (Section 8), technical-preferences.md performance budgets, engine name and version
- Ask: "For each asset in this list, specify: (1) exact dimensions or polycount (match the art bible Asset Standards tiers — do not invent new sizes); (2) file format and export settings; (3) naming convention (from technical-preferences.md); (4) any engine-specific constraints this asset type must respect; (5) LOD requirements if applicable. Flag any asset type where the art bible's preferred standard conflicts with the engine's constraints."

**Collect both responses before Phase 4.** If any conflict exists between art-director and technical-artist (e.g., art-director specifies 4K textures but technical-artist flags the engine budget requires 512px), surface it explicitly — do NOT silently resolve.

---

## Phase 4: Compile and Review

Combine the agent outputs into a draft spec per asset. Present all specs in conversation text using this format:

```
## ASSET-[NNN] — [Asset Name]

| Field | Value |
|-------|-------|
| Category | [Sprite / VFX / Environment / UI / Audio / 3D] |
| Dimensions | [e.g. 256×256px, 4-frame sprite sheet] |
| Format | [PNG / SVG / WAV / etc.] |
| Naming | [e.g. vfx_frost_hit_01.png] |
| Polycount | [if 3D — e.g. <800 tris] |
| Texture Res | [e.g. 512px — matches Art Bible §8 Tier 2] |

**Visual Description:**
[2–3 sentences. Specific enough for two artists to produce consistent results.]

**Art Bible Anchors:**
- §3 Shape Language: [relevant rule applied]
- §4 Color System: [color role — e.g. "uses Threat Blue per semantic color rules"]

**Generation Prompt:**
[Ready-to-use prompt. Include: style keywords, composition notes, color palette anchors, lighting direction, negative prompts.]

**Status:** Needed
```

After presenting all specs, use `AskUserQuestion`:
- Prompt: "Asset specs for **[target]** — [N] assets. Review complete?"
- Options: `[A] Approve all — write to file` / `[B] Revise a specific asset` / `[C] Regenerate with different direction`

If [B]: ask which asset and what to change. Revise inline and re-present. Do NOT re-spawn agents for minor text revisions — only re-spawn if the visual direction itself needs to change.

If [C]: ask what direction to change. Re-spawn the relevant agent with the updated brief.

---

## Phase 4b: Internal Review Loop

Spawn `art-director` and `technical-artist` via Task to review the assembled spec set against art bible, DESIGN.md tokens, engine renderer constraints, and budget.

**Loop exit condition.** Single iteration where every spawned reviewer returns clean (no critical/high/medium findings). Non-clean → user revises affected specs, re-spawn reviewers. No iteration cap.

Record iteration count.

## Combined Review Loop (parallel external Codex)

Per `.ags/rules/review-workflow.md`. The internal review section above runs **in parallel** with external Codex inside one loop. Each iteration:

1. Resolve severity floor: iter 1-2 → keep all severities; iter 3-4 → critical/high; iter 5+ → critical only.
2. Persist current draft to `.ags/project/reviews/.tmp/[type]-[slug]-iter[N]-draft.md`.
3. **Spawn in one message, in parallel** (multiple Task calls + one Bash invocation):
   - All internal reviewer Tasks listed above.
   - `/ags-external-review [type] [draft-path] --embedded-parallel --iteration [N] --min-severity [floor]` — Codex unavailable returns `skipped: codex-unavailable`; aggregator logs skip in decisions-log and continues with internal pool only.
4. Aggregator (`producer` by default; skill-designated lead where the skill specifies one) merges findings from internal + external, drops nitpicks + below-floor.
5. **Loop exit**: filtered set empty → proceed to write approval. Non-empty → surface aggregated kept findings, user revises draft, N++, repeat.

No iteration cap. No user-confirm gate before external — it runs every iteration automatically. Record final iteration count for the decisions-log entry written at skill completion.

---

## Phase 5: Write Spec File

After approval, ask: "May I write the spec to `design/assets/specs/[target-name]-assets.md`?"

Write the file with:

```markdown
# Asset Specs — [Target Type]: [Target Name]

> **Source**: [path to source GDD/level/character doc]
> **Art Bible**: design/art/ags-art-bible.html
> **Generated**: [date]
> **Status**: [N] assets specced / [N] approved / [N] in production / [N] done

[all asset specs in ASSET-NNN format]
```

Then update `design/assets/asset-manifest.md`. If it doesn't exist, create it:

```markdown
# Asset Manifest

> Last updated: [date]

## Progress Summary

| Total | Needed | In Progress | Done | Approved |
|-------|--------|-------------|------|----------|
| [N] | [N] | [N] | [N] | [N] |

## Assets by Context

### [Target Type]: [Target Name]
| Asset ID | Name | Category | Status | Spec File |
|----------|------|----------|--------|-----------|
| ASSET-001 | [name] | [category] | Needed | design/assets/specs/[target]-assets.md |
```

If the manifest already exists, append the new context block and update the Progress Summary counts.

Ask: "May I update `design/assets/asset-manifest.md`?"

---

## Phase 6: Close

Use `AskUserQuestion`:
- Prompt: "Asset specs complete for **[target]**. What's next?"
- Options:
  - `[A] Spec another system — /ags-asset-spec system:[next-system]`
  - `[B] Spec a level — /ags-asset-spec level:[level-name]`
  - `[C] Spec a character — /ags-asset-spec character:[character-name]`
  - `[D] Run /ags-asset-audit — validate delivered assets against specs`
  - `[E] Stop here`

---

## Asset ID Assignment

Asset IDs are assigned sequentially across the entire project — not per-context. Read the manifest before assigning IDs to find the current highest number:

```
Grep pattern="ASSET-" path="design/assets/asset-manifest.md"
```

Start new assets from `ASSET-[highest + 1]`. This ensures IDs are stable and unique across the whole project.

If no manifest exists yet, start from `ASSET-001`.

---

## Shared Asset Protocol

Before speccing an asset, check if an equivalent already exists in another context's spec:

- Common UI elements (health bars, score displays) are often shared across systems
- Generic environment props may appear in multiple levels
- Character VFX (hit sparks, death effects) may reuse a base spec with color variants

If a match is found: reference the existing ASSET-ID rather than creating a duplicate. Note the shared usage in the manifest's referenced-by column.

> "ASSET-012 (Generic Hit Spark) already specced for Combat system. Reusing for Tower Defense — adding tower-defense to referenced-by."

---

## Error Recovery Protocol

If any spawned agent returns BLOCKED or cannot complete:

1. Surface immediately: "[AgentName]: BLOCKED — [reason]"
2. If `technical-artist` blocks: proceed with art-director output only — note that technical constraints were not validated
3. If `art-director` blocks: derive descriptions from art bible rules — flag as "Art director not consulted — verify against art bible before production"
4. Always produce a partial spec — never discard work because one agent blocked

---

## Collaborative Protocol

Every phase: **Identify → Confirm → Generate → Review → Approve → Write**

- Never spec assets without confirming list with user first
- Always anchor specs to art bible — spec that contradicts art bible is wrong
- Surface all agent disagreements — do not silently pick one
- Write spec file only after explicit approval
- Update manifest immediately after writing spec

---

## Recommended Next Steps

- `/ags-asset-spec [next-context]` — continue speccing remaining systems, levels, or characters
- `/ags-asset-audit` — validate delivered assets against written specs
