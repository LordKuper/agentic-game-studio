---
name: ags-consistency-check
description: "Scan all GDDs against the entity registry to detect cross-document inconsistencies: same entity with different stats, same item with different values, same formula with different variables. Grep-first approach — reads registry then targets only conflicting GDD sections rather than full document reads."
argument-hint: "[full | since-last-review | entity:<name> | item:<name>]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, AskUserQuestion
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

# Consistency Check

Detects cross-document inconsistencies by comparing GDDs against entity registry (`design/registry/entities.yaml`). Grep-first: reads registry once, targets only GDD sections mentioning registered names — no full reads unless conflict needs investigation.

**Write-time safety net.** Catches what `/ags-design-system`'s per-section checks miss and what `/ags-review-all-gdds`'s holistic review catches too late.

**When to run:**
- After writing each new GDD (before moving to the next system)
- Before `/ags-review-all-gdds` (so that skill starts with a clean baseline)
- Before `/ags-create-architecture` (inconsistencies poison downstream ADRs)
- On demand: `/ags-consistency-check entity:[name]` to check one entity specifically

**Output:** Conflict report + optional registry corrections

---

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| ≥1 GDD under `design/gdd/*.md` | `/ags-design-system` | STOP. "No GDDs to check. Run `/ags-design-system [system]` first." |
| `design/registry/entities.yaml` (optional) | manual | WARN: structural checks only. Limited coverage without registry. |

If STOP triggers, exit with verdict **BLOCKED — missing prerequisite**.

---

## Phase 1: Parse Arguments and Load Registry

**Modes:**
- No argument / `full` — check all registered entries against all GDDs
- `since-last-review` — check only GDDs modified since the last review report
- `entity:<name>` — check one specific entity across all GDDs
- `item:<name>` — check one specific item across all GDDs

**Load the registry:**

```
Read path="design/registry/entities.yaml"
```

If the file does not exist or has no entries:
> "Entity registry is empty. Run `/ags-design-system` to write GDDs — the registry
> is populated automatically after each GDD is completed. Nothing to check yet."

Stop and exit.

Build four lookup tables from the registry:
- **entity_map**: `{ name → { source, attributes, referenced_by } }`
- **item_map**: `{ name → { source, value_gold, weight, ... } }`
- **formula_map**: `{ name → { source, variables, output_range } }`
- **constant_map**: `{ name → { source, value, unit } }`

Count total registered entries. Report:
```
Registry loaded: [N] entities, [N] items, [N] formulas, [N] constants
Scope: [full | since-last-review | entity:name]
```

---

## Phase 2: Locate In-Scope GDDs

```
Glob pattern="design/gdd/*.md"
```

Exclude: `game-concept.md`, `systems-index.md`, `game-pillars.md` — these are
not system GDDs.

For `since-last-review` mode:
```bash
git log --name-only --pretty=format: -- design/gdd/ | grep "\.md$" | sort -u
```
Limit to GDDs modified since the most recent `design/gdd/gdd-cross-review-*.md`
file's creation date.

Report the in-scope GDD list before scanning.

---

## Phase 3: Grep-First Conflict Scan

For each registered entry, grep every in-scope GDD for the entry's name.
Do NOT do full reads — extract only the matching lines and their immediate
context (-C 3 lines).

This is the core optimization: instead of reading 10 GDDs × 400 lines each
(4,000 lines), you grep 50 entity names × 10 GDDs (50 targeted searches,
each returning ~10 lines on a hit).

### 3a: Entity Scan

For each entity in entity_map:

```
Grep pattern="[entity_name]" glob="design/gdd/*.md" output_mode="content" -C 3
```

For each GDD hit, extract the values mentioned near the entity name:
- any numeric attributes (counts, costs, durations, ranges, rates)
- any categorical attributes (types, tiers, categories)
- any derived values (totals, outputs, results)
- any other attributes registered in entity_map

Compare extracted values against the registry entry.

**Conflict detection:**
- Registry says `[entity_name].[attribute] = [value_A]`. GDD says `[entity_name] has [value_B]`. → **CONFLICT**
- Registry says `[item_name].[attribute] = [value_A]`. GDD says `[item_name] is [value_B]`. → **CONFLICT**
- GDD mentions `[entity_name]` but doesn't specify the attribute. → **NOTE** (no conflict, just unverifiable)

### 3b: Item Scan

For each item in item_map, grep all GDDs for the item name. Extract:
- sell price / value / gold value
- weight
- stack rules (stackable / non-stackable)
- category

Compare against registry entry values.

### 3c: Formula Scan

For each formula in formula_map, grep all GDDs for the formula name. Extract:
- variable names mentioned near the formula
- output range or cap values mentioned

Compare against registry entry:
- Different variable names → **CONFLICT**
- Output range stated differently → **CONFLICT**

### 3d: Constant Scan

For each constant in constant_map, grep all GDDs for the constant name. Extract:
- Any numeric value mentioned near the constant name

Compare against registry value:
- Different number → **CONFLICT**

---

### 3e: Document Boundary Scan (per `.ags/rules/document-boundaries.md`)

For each in-scope GDD / ADR / UX-spec / HUD-spec, run targeted greps. Any hit → boundary violation, classify alongside conflicts.

**Tech-leak in GDD** (`design/gdd/*.md`):
```
Grep pattern="\b(class|namespace|using|import|package|public\s+(class|interface|struct))\s+\w+" glob="design/gdd/*.md"
Grep pattern="\b\d+\s?(ms|MB|KB|FPS|fps|µs|us)\b" glob="design/gdd/*.md"
Grep pattern="\b(MonoBehaviour|ScriptableObject|GameObject|UnityEngine|Godot|UE5|UnrealEngine|System\.\w+|Microsoft\.\w+)\b" glob="design/gdd/*.md"
```
Allowed exception: ms/FPS literals inside Acceptance Criteria expressed as player-observable budget — manual review.

**GDD→ADR cite (forbidden)**:
```
Grep pattern="\b(adr-\d{3,}|ADR-\d{3,})\b" glob="design/gdd/*.md"
```
Any hit = violation. GDD must not cite ADR.

**Concept-leak in ADR** (mechanic rules without GDD cite):
- ADR missing `**GDD source**:` line → violation.
- ADR has section describing player-facing rules / formulas not present in cited GDD → manual flag (low-confidence; surface for review).

**Raw visual literal outside DESIGN.md**:
```
Grep pattern="#[0-9a-fA-F]{3,8}\b" glob="design/{art,ux,gdd}/**/*.md"
Grep pattern="\b\d+(\.\d+)?(px|pt|rem|em)\b" glob="design/{art,ux,gdd}/**/*.md"
```
Allowed location: `design/art/DESIGN.md` only. All others = violation.

**Duplicated content** (≥3 consecutive sentences identical between GDD ↔ ADR/UX/HUD):
- Approximate via Bash: `diff` blocks ≥120 chars matching across pairs (skill calls `git diff --no-index --word-diff=none` between extracted sections, or simpler: split files into sentence chunks, hash, intersect). Flag overlapping fingerprints.

**Missing approval marker**:
- Any GDD / ADR / UX-spec / HUD-spec / art-bible / DESIGN.md / game-concept / engine doc lacking YAML front-matter or `status:` field → violation.
- Any doc cited as predecessor (per precondition chain) whose `status` ≠ `approved` → violation.

Boundary violations classified:
- **🔴 BOUNDARY** — hard rule break (tech-leak in GDD, GDD→ADR cite, raw literal outside DESIGN.md, missing approval on cited predecessor).
- **⚠️ DUPLICATION** — content fingerprint overlap; surface for SSoT consolidation.
- **ℹ️ MARKER GAP** — doc lacks front-matter; informational on existing files, BLOCKING for new precondition checks.

---

### 3f: Art-bible Dependency Freshness Scan

Targets `design/art/ags-art-bible.html`. For every `<section>` carrying `data-depends-on`, verify the section's recorded provenance still matches the current upstream state.

**Algorithm**:

1. Read `design/art/ags-art-bible.html`. If file absent, skip 3f entirely (no art-bible yet).
2. For each `<section ... data-depends-on="..." data-checked-at="..." data-checked-against="...">`:
   a. Parse `data-depends-on` on `|`. Each entry is one of: `<path>`, `<path>:<scope>`, `<path>#<section-id>`, `self#<section-id>`.
   b. Parse `data-checked-against` on `|`. Same length as `data-depends-on`. If lengths differ or attribute empty → **🟡 STALE: provenance-missing**.
   c. For each upstream entry, resolve the file path (scope/section anchor informational — hash whole file):
      - `<path>` / `<path>:<scope>` / `<path>#<section-id>` → file `<path>`.
      - `self#<id>` → file `design/art/ags-art-bible.html` itself (cross-section drift inside same file).
   d. Compute current hash via Bash: `git hash-object <path>` (or `sha256sum` for non-git files, prefixed `sha256:`).
   e. Compare with stored hash for that entry.
      - Match → entry fresh.
      - Mismatch → flag entry stale, record `<section-id> ← <upstream-path>` mismatch.
   f. Also verify upstream file still exists. Missing file → **🔴 DANGLING DEPENDENCY**.

3. **Token reference validity** (run alongside 3f, target art-bible only):
   - Grep art-bible body for token patterns: `\{(colors|typography|spacing|rounded|components)\.[a-z0-9-]+\}`
   - Read `design/art/DESIGN.md` front-matter YAML; build set of defined tokens.
   - Any token cited in art-bible but missing from DESIGN.md → **🔴 MISSING TOKEN** (high — breaks the SSoT contract).
   - Any token defined in DESIGN.md but never cited in art-bible → **ℹ️ ORPHAN TOKEN** (informational).

**Classification**:
- **🟠 STALE** — upstream changed since section was last validated. Section may contradict current upstream; user must re-review and re-approve (which restamps provenance).
- **🟡 STALE: provenance-missing** — section authored before provenance system, or attrs cleared. Same fix path: re-run `/ags-art-bible` on this section to stamp.
- **🔴 DANGLING DEPENDENCY** — declared upstream file no longer exists. Either restore file or update `data-depends-on` to new path.
- **🔴 MISSING TOKEN** — art-bible cites a token that does not exist in DESIGN.md. Either add the token or remove the reference.
- **ℹ️ ORPHAN TOKEN** — token defined but not cited. Informational; may indicate DESIGN.md drift or planned-but-unused token.

Append findings to Phase 5 Output Report under a new sub-section "**Art-bible Dependency Freshness**".

Fix path for STALE entries: invoke `/ags-art-bible` with retrofit mode — skill re-runs the section's authoring loop, then re-stamps `data-checked-at` / `data-checked-against`. Auto-skipped if section content does not actually need changes — user may approve unchanged content to restamp provenance only.

---

## Phase 4: Deep Investigation (Conflicts Only)

For each conflict found in Phase 3, do a targeted full-section read of the
conflicting GDD to get precise context:

```
Read path="design/gdd/[conflicting_gdd].md"
```
(Or use Grep with wider context if the file is large)

Confirm the conflict with full context. Determine:
1. **Which GDD is correct?** Check the `source:` field in the registry — the
   source GDD is the authoritative owner. Any other GDD that contradicts it
   is the one that needs updating.
2. **Is the registry itself out of date?** If the source GDD was updated after
   the registry entry was written (check git log), the registry may be stale.
3. **Is this a genuine design change?** If the conflict represents an intentional
   design decision, the resolution is: update the source GDD, update the registry,
   then fix all other GDDs.

For each conflict, classify:
- **🔴 CONFLICT** — same named entity/item/formula/constant with different values
  in different GDDs. Must resolve before architecture begins.
- **⚠️ STALE REGISTRY** — source GDD value changed but registry not updated.
  Registry needs updating; other GDDs may be correct already.
- **ℹ️ UNVERIFIABLE** — entity mentioned but no comparable attribute stated.
  Not a conflict; just noting the reference.

---

## Phase 5: Output Report

```
## Consistency Check Report
Date: [date]
Registry entries checked: [N entities, N items, N formulas, N constants]
GDDs scanned: [N] ([list names])

---

### Boundary Violations (per `.ags/rules/document-boundaries.md`)

🔴 BOUNDARY — [file:line] [rule violated] → [fix action]
⚠️ DUPLICATION — [file-A] ↔ [file-B] [overlapping section] → [consolidate to SSoT: which]
ℹ️ MARKER GAP — [file] missing front-matter `status:` → add YAML block

---

### Art-bible Dependency Freshness

🟠 STALE — design/art/ags-art-bible.html#[section-id] ← [upstream-path] changed since [data-checked-at] → re-run `/ags-art-bible` retrofit on this section
🟡 STALE: provenance-missing — design/art/ags-art-bible.html#[section-id] → stamp via `/ags-art-bible` retrofit
🔴 DANGLING DEPENDENCY — design/art/ags-art-bible.html#[section-id] declares missing upstream [path] → restore or update data-depends-on
🔴 MISSING TOKEN — design/art/ags-art-bible.html cites `{ns.name}` not present in design/art/DESIGN.md → add token or remove reference
ℹ️ ORPHAN TOKEN — design/art/DESIGN.md defines `{ns.name}` not cited anywhere → confirm intentional

---

### Conflicts Found (must resolve before architecture)

🔴 [Entity/Item/Formula/Constant Name]
   Registry (source: [gdd]): [attribute] = [value]
   Conflict in [other_gdd].md: [attribute] = [different_value]
   → Resolution needed: [which doc to change and to what]

---

### Stale Registry Entries (registry behind the GDD)

⚠️ [Entry Name]
   Registry says: [value] (written [date])
   Source GDD now says: [new value]
   → Update registry entry to match source GDD, then check referenced_by docs.

---

### Unverifiable References (no conflict, informational)

ℹ️ [gdd].md mentions [entity_name] but states no comparable attributes.
   No conflict detected. No action required.

---

### Clean Entries (no issues found)

✅ [N] registry entries verified across all GDDs with no conflicts.

---

Verdict: PASS | CONFLICTS FOUND
```

**Verdict:**
- **PASS** — no conflicts. Registry and GDDs agree on all checked values.
- **CONFLICTS FOUND** — one or more conflicts detected. List resolution steps.

---

## Phase 6: Registry Corrections

If stale registry entries were found, ask:
> "May I update `design/registry/entities.yaml` to fix the [N] stale entries?"

For each stale entry:
- Update the `value` / attribute field
- Set `revised:` to today's date
- Add a YAML comment with the old value: `# was: [old_value] before [date]`

If new entries were found in GDDs that are not in the registry, ask:
> "Found [N] entities/items mentioned in GDDs that aren't in the registry yet.
> May I add them to `design/registry/entities.yaml`?"

Only add entries that appear in more than one GDD (true cross-system facts).

**Never delete registry entries.** Set `status: deprecated` if an entry is removed
from all GDDs.

After writing: Verdict: **COMPLETE** — consistency check finished.
If conflicts remain unresolved: Verdict: **BLOCKED** — [N] conflicts need manual resolution before architecture begins.

### 6b: Append to Reflexion Log

If any 🔴 CONFLICT entries were found (regardless of whether they were resolved),
append an entry to `docs/consistency-failures.md` for each conflict:

```markdown
### [YYYY-MM-DD] — /ags-consistency-check — 🔴 CONFLICT
**Domain**: [system domain(s) involved]
**Documents involved**: [source GDD] vs [conflicting GDD]
**What happened**: [specific conflict — entity name, attribute, differing values]
**Resolution**: [how it was fixed, or "Unresolved — manual action needed"]
**Pattern**: [generalised lesson, e.g. "Item values defined in combat GDD were not
referenced in economy GDD before authoring — always check entities.yaml first"]
```

Only append if `docs/consistency-failures.md` exists. If the file is missing,
skip this step silently — do not create the file from this skill.

---

## Next Steps

- **PASS**: `/ags-review-all-gdds` for holistic review, or `/ags-create-architecture` if all MVP GDDs complete.
- **CONFLICTS FOUND**: Fix flagged GDDs, re-run `/ags-consistency-check` to confirm.
- **STALE REGISTRY**: Update registry (Phase 6), re-run to verify.
- Run `/ags-consistency-check` after each new GDD — catch issues early, not at architecture time.

---

## Combined Review Loop (parallel external Codex)

Per `.ags/rules/review-workflow.md`. The scan phases run **in parallel** with external Codex inside one loop. Each iteration:

1. Resolve severity floor: iter 1-2 → keep all severities; iter 3-4 → critical/high; iter 5+ → critical only.
2. **Spawn in one message, in parallel**:
   - All internal reviewer Tasks (registry grep, cross-doc fingerprint, boundary scan).
   - For each flagged GDD: `/ags-external-review gdd [gdd-path] --embedded-parallel --iteration [N] --min-severity [floor]`. For cross-doc / SSoT-zone violation bundle: one additional `custom` call with bundle of affected docs. Codex unavailable → `skipped: codex-unavailable`; aggregator logs skip in decisions-log and continues with internal pool only.
3. Aggregator (`producer`) merges findings from internal + Codex, drops nitpicks + below-floor.
4. **Loop exit**: filtered set empty → emit final verdict. Non-empty → surface aggregated kept findings, user resolves, N++, repeat.

No iteration cap. No user-confirm gate before external — it runs every iteration automatically. Record final iteration count in the verdict report and decisions-log entry. Codex reviews the flagged source docs, NOT this consistency report.
