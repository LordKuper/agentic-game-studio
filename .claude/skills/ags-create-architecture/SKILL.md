---
name: ags-create-architecture
description: "Author the master architecture document. Run as skeleton in Foundation phase (top-level layers, module boundaries, tech stack — no detailed ADRs yet); refresh in Production as ADRs accumulate per epic. Engine-version-aware: flags knowledge gaps and validates decisions against the pinned engine version."
argument-hint: "[focus-area: full | layers | data-flow | api-boundaries | adr-audit] [--review full|lean|solo]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Bash, AskUserQuestion, Task
agent: technical-director
---

# Create Architecture

Produces `design/architecture/architecture.md` — master architecture document translating approved GDDs into a technical blueprint.

**When to run:**
- **Foundation phase** — create skeleton: top-level layers, module boundaries, tech stack, foundation responsibilities. ADRs are not required yet.
- **Production phase** — refresh after several epics have added ADRs (recommended every 3-5 epics or after a major `revise` epic). Update the document to reflect cumulative architectural state.

**Distinct from `/ags-architecture-decision`**: ADRs record individual decisions. This skill creates the whole-system blueprint that gives ADRs context.

Resolve the review mode (once, store for all gate spawns this run):
1. If `--review [full|lean|solo]` was passed → use that
2. Else read `.ags/project/review-mode.md` → use that value
3. Else → default to `lean`

See `.ags/rules/director-gates.md` for the full check pattern.

**Argument modes:**
- **No argument / `full`**: Full guided walkthrough — all sections, start to finish
- **`layers`**: Focus on the system layer diagram only
- **`data-flow`**: Focus on data flow between modules only
- **`api-boundaries`**: Focus on API boundary definitions only
- **`adr-audit`**: Audit existing ADRs for engine compatibility gaps only

---

## Phase 0: Prerequisites + Load All Context

### 0p. Prerequisites (verify before any read)

STOP on first missing item with redirect.

| Artifact | Created by | If missing |
|---|---|---|
| `.ags/docs/engine-reference/[engine]/VERSION.md` | `/ags-setup-engine` | STOP. "Engine reference missing. Run `/ags-setup-engine` first." |
| `design/gdd/game-concept.md` (no `{{...}}`) | `/ags-brainstorm` | STOP. "No game concept. Run `/ags-brainstorm` first." |
| `design/gdd/systems-index.md` | `/ags-map-systems` | STOP. "No systems map. Run `/ags-map-systems` first — architecture skeleton needs the system catalog." |
| `.ags/rules/technical-preferences.md` (Engine: filled, no `[CHOOSE]`) | `/ags-setup-engine` | STOP. "Technical preferences not configured. Run `/ags-setup-engine`." |

If any STOP triggers, exit with verdict **BLOCKED — missing prerequisite** and surface the redirect.

Once all prerequisites pass, proceed to load full context:

### 0a. Engine Context (Critical)

Read the engine reference library completely:

1. `.ags/docs/engine-reference/[engine]/VERSION.md`
   → Extract: engine name, version, LLM cutoff, post-cutoff risk levels
2. `.ags/docs/engine-reference/[engine]/breaking-changes.md`
   → Extract: all HIGH and MEDIUM risk changes
3. `.ags/docs/engine-reference/[engine]/deprecated-apis.md`
   → Extract: APIs to avoid
4. `.ags/docs/engine-reference/[engine]/current-best-practices.md`
   → Extract: post-cutoff best practices that differ from training data
5. All files in `.ags/docs/engine-reference/[engine]/modules/`
   → Extract: current API patterns per domain

If no engine is configured, stop and prompt:
> "No engine is configured. Run `/ags-setup-engine` first. Architecture cannot be
> written without knowing which engine and version you are targeting."

### 0b. Design Context + Technical Requirements Extraction

Read all approved design documents and extract technical requirements from each:

1. `design/gdd/game-concept.md` — game pillars, genre, core loop
2. `design/gdd/systems-index.md` — all systems, dependencies, priority tiers
3. `.ags/rules/technical-preferences.md` — naming conventions, performance budgets,
   allowed libraries, forbidden patterns
4. **Every GDD in `design/gdd/`** — for each, extract technical requirements:
   - Data structures implied by the game rules
   - Performance constraints stated or implied
   - Engine capabilities the system requires
   - Cross-system communication patterns (what talks to what, how)
   - State that must persist (save/load implications)
   - Threading or timing requirements

Build a **Technical Requirements Baseline** — a flat list of all extracted
requirements across all GDDs, numbered `TR-[gdd-slug]-[NNN]`. This is the
complete set of what the architecture must cover. Present it as:

```
## Technical Requirements Baseline
Extracted from [N] GDDs | [X] total requirements

| Req ID | GDD | System | Requirement | Domain |
|--------|-----|--------|-------------|--------|
| TR-combat-001 | combat.md | Combat | Hitbox detection per-frame | Physics |
| TR-combat-002 | combat.md | Combat | Combo state machine | Core |
| TR-inventory-001 | inventory.md | Inventory | Item persistence | Save/Load |
```

This baseline feeds into every subsequent phase. No GDD requirement should be
left without an architectural decision to support it by the end of this session.

### 0c. Existing Architecture Decisions

Read all files in `design/architecture/` to understand what has already been decided.
List any ADRs found and their domains.

### 0d. Generate Knowledge Gap Inventory

Before proceeding, display a structured summary:

```
## Engine Knowledge Gap Inventory
Engine: [name + version]
LLM Training Covers: up to approximately [version]
Post-Cutoff Versions: [list]

### HIGH RISK Domains (must verify against engine reference before deciding)
- [Domain]: [Key changes]

### MEDIUM RISK Domains (verify key APIs)
- [Domain]: [Key changes]

### LOW RISK Domains (in training data, likely reliable)
- [Domain]: [no significant post-cutoff changes]

### Systems from GDD that touch HIGH/MEDIUM risk domains:
- [GDD system name] → [domain] → [risk level]
```

Ask: "This inventory identifies [N] systems in HIGH RISK engine domains. Shall I
continue building the architecture with these warnings flagged throughout?"

---

## Phase 1: System Layer Mapping

Map every system from `systems-index.md` into an architecture layer. The standard
game architecture layers are:

```
┌─────────────────────────────────────────────┐
│  PRESENTATION LAYER                         │  ← UI, HUD, menus, VFX, audio
├─────────────────────────────────────────────┤
│  FEATURE LAYER                              │  ← gameplay systems, AI, quests
├─────────────────────────────────────────────┤
│  CORE LAYER                                 │  ← physics, input, combat, movement
├─────────────────────────────────────────────┤
│  FOUNDATION LAYER                           │  ← engine integration, save/load,
│                                             │    scene management, event bus
├─────────────────────────────────────────────┤
│  PLATFORM LAYER                             │  ← OS, hardware, engine API surface
└─────────────────────────────────────────────┘
```

For each GDD system, ask:
- Which layer does it belong to?
- What are its module boundaries?
- What does it own exclusively? (data, state, behaviour)

Present the proposed layer assignment and ask for approval before proceeding to
the next section. Write the approved layer map immediately to the skeleton file.

**Engine awareness check**: For each system assigned to the Core and Foundation
layers, flag if it touches a HIGH or MEDIUM risk engine domain. Show the relevant
engine reference excerpt inline.

---

## Phase 2: Module Ownership Map

For each module defined in Phase 1, define ownership:

- **Owns**: what data and state this module is solely responsible for
- **Exposes**: what other modules may read or call
- **Consumes**: what it reads from other modules
- **Engine APIs used**: which specific engine classes/nodes/signals this module
  calls directly (with version and risk level noted)

Format as a table per layer, then as an ASCII dependency diagram.

**Engine awareness check**: For every engine API listed, verify against the
relevant module reference doc. If an API is post-cutoff, flag it:

```
⚠️  [ClassName.method()] — Unity 6000.x (post-cutoff, HIGH risk)
    Verified against: .ags/docs/engine-reference/unity/modules/[domain].md
    Behaviour confirmed: [yes / NEEDS VERIFICATION]
```

Get user approval on the ownership map before writing.

---

## Phase 3: Data Flow

Define how data moves between modules during key game scenarios. Cover at minimum:

1. **Frame update path**: Input → Core systems → State → Rendering
2. **Event/signal path**: How systems communicate without tight coupling
3. **Save/load path**: What state is serialised, which module owns serialisation
4. **Initialisation order**: Which modules must boot before others

Use ASCII sequence diagrams where helpful. For each data flow:
- Name the data being transferred
- Identify the producer and consumer
- State whether this is synchronous call, signal/event, or shared state
- Flag any data flows that cross thread boundaries

Get user approval per scenario before writing.

---

## Phase 4: API Boundaries

Define the public contracts between modules. For each boundary:

- What is the interface a module exposes to the rest of the system?
- What are the entry points (functions/signals/properties)?
- What invariants must callers respect?
- What must the module guarantee to callers?

Write in pseudocode or the project's actual language (from technical preferences).
These become the contracts programmers implement against.

**Engine awareness check**: If any interface uses engine-specific types (e.g.
`MonoBehaviour`, `ScriptableObject`, `Entity`, `IComponentData`, `AssetReference`
in Unity), flag the version and verify the type exists and has not changed
signature in the target engine version.

---

## Phase 5: ADR Audit + Traceability Check

Review all existing ADRs from Phase 0c against both the architecture built in
Phases 1-4 AND the Technical Requirements Baseline from Phase 0b.

### ADR Quality Check

For each ADR:
- [ ] Does it have an Engine Compatibility section?
- [ ] Is the engine version recorded?
- [ ] Are post-cutoff APIs flagged?
- [ ] Does it have a "GDD Requirements Addressed" section?
- [ ] Does it conflict with the layer/ownership decisions made in this session?
- [ ] Is it still valid for the pinned engine version?

| ADR | Engine Compat | Version | GDD Linkage | Conflicts | Valid |
|-----|--------------|---------|-------------|-----------|-------|
| ADR-0001: [title] | ✅/❌ | ✅/❌ | ✅/❌ | None/[conflict] | ✅/⚠️ |

### Traceability Coverage Check

Map every requirement from the Technical Requirements Baseline to existing ADRs.
For each requirement, check if any ADR's "GDD Requirements Addressed" section
or decision text covers it:

| Req ID | Requirement | ADR Coverage | Status |
|--------|-------------|--------------|--------|
| TR-combat-001 | Hitbox detection per-frame | ADR-0003 | ✅ |
| TR-combat-002 | Combo state machine | — | ❌ GAP |

Count: X covered, Y gaps. For each gap, it becomes a **Required New ADR**.

### Required New ADRs

List all decisions made during this architecture session (Phases 1-4) that do
not yet have a corresponding ADR, PLUS all uncovered Technical Requirements.
Group by layer — Foundation first:

**Foundation Layer (must create before any coding):**
- `/ags-architecture-decision [title]` → covers: TR-[id], TR-[id]

**Core Layer:**
- `/ags-architecture-decision [title]` → covers: TR-[id]

---

## Phase 6: Missing ADR List

Based on the full architecture, produce a complete list of ADRs that should exist
but don't yet. Group by priority:

**Must have before coding starts (Foundation & Core decisions):**
- [e.g. "Scene management and scene loading strategy"]
- [e.g. "Event bus vs direct signal architecture"]

**Should have before the relevant system is built:**
- [e.g. "Inventory serialisation format"]

**Can defer to implementation:**
- [e.g. "Specific shader technique for water"]

---

## Phase 6.5: Internal Review Loop

Before the external-review gate, run an internal review loop. Reviewers: technical-director (self-review via gate **TD-ARCHITECTURE**) + lead-programmer (gate **LP-FEASIBILITY**). Each iteration: spawn both via Task in parallel, collect verdicts and findings.

**Review mode check** for LP-FEASIBILITY:
- `solo` → spawn neither; loop runs only TD self-review.
- `lean` → skip LP-FEASIBILITY (not PHASE-GATE). Loop runs only TD self-review.
- `full` → spawn both.

**Loop exit condition.** Single iteration where every spawned reviewer returns clean (no critical, no high, no medium findings; low allowed). No iteration cap.

**On non-clean iteration**: surface aggregated findings (TD + LP, source-tagged) → user revises the relevant sections of the draft → re-spawn the same reviewers. The draft updates incrementally — write each user-approved section to the skeleton file as before; loop only re-reviews changed sections plus their dependents.

Record iteration count and final verdict per reviewer for the Document Status section.

---

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

**Note**: external Codex review is now embedded in the Combined Review Loop above (parallel with internal reviewers). The architecture draft is reviewed each iteration via `/ags-external-review adr [draft-path] --embedded-parallel` — the `adr` prompt template covers architectural soundness, GDD alignment, engine-version risk, and consistency. Reference the final report path in `Document Status` → `External Review: [path]`.

---

## Phase 7: Write the Master Architecture Document

Once all sections are approved, write the complete document to
`design/architecture/architecture.md`.

Ask: "May I write the master architecture document to `design/architecture/architecture.md`?"

The document structure:

```markdown
# [Game Name] — Master Architecture

## Document Status
- Version: [N]
- Last Updated: [date]
- Engine: [name + version]
- GDDs Covered: [list]
- ADRs Referenced: [list]

## Engine Knowledge Gap Summary
[Condensed from Phase 0d inventory — HIGH/MEDIUM risk domains and their implications]

## System Layer Map
[From Phase 1]

## Module Ownership
[From Phase 2]

## Data Flow
[From Phase 3]

## API Boundaries
[From Phase 4]

## ADR Audit
[From Phase 5]

## Required ADRs
[From Phase 6]

## Architecture Principles
[3-5 key principles that govern all technical decisions for this project,
derived from the game concept, GDDs, and technical preferences]

## Open Questions
[Decisions deferred — must be resolved before the relevant layer is built]
```

---

## Phase 7b: Record Sign-Off in Document Status

The substantive TD self-review and LP feasibility review have already run in Phase 6.5 (Internal Review Loop). This phase records their outcome.

Update the Document Status section of the written architecture document:
```
- Technical Director Sign-Off: [date] — APPROVED / APPROVED WITH CONDITIONS
- Lead Programmer Feasibility: FEASIBLE / CONCERNS ACCEPTED / REVISED / SKIPPED ([review-mode])
- Internal Review Iterations: [N]
- External Review: [report path | skipped — see decisions-log.md | not run]
```

Ask: "May I update the Document Status section in `design/architecture/architecture.md` with the sign-off?"

---

## Phase 8: Handoff

After writing the document, provide a clear handoff:

1. **Run these ADRs next** (from Phase 6, prioritised): list the top 3
2. **Gate check**: "Architecture document updated. Run `/ags-gate-check production` after foundation skeleton is complete to advance to Production. ADRs accumulate per epic — re-run this skill periodically to refresh."
3. **Update session state**: Write a summary to `.ags/project/state.md`

---

## Collaborative Protocol

Every phase:

1. **Load context silently** — do not narrate file reads
2. **Present findings** — show knowledge gap inventory and layer proposals
3. **Ask before deciding** — present options for each architectural choice
4. **Get approval before writing** — each phase section written only after user approves
5. **Incremental writing** — write each approved section immediately; do not accumulate. Survives crashes.

Never make binding architectural decision without user input. If unsure, present 2-4 options with pros/cons.

---

## Recommended Next Steps

- `/ags-create-control-manifest` for the seed manifest (Foundation phase) — does not require ADRs yet
- `/ags-gate-check production` once Foundation artifacts complete (skeleton, accessibility tier, control manifest seed, test framework)
- In Production: ADRs are added per epic via `/ags-architecture-decision` inside `/ags-create-epics` flow — no upfront ADR list required
