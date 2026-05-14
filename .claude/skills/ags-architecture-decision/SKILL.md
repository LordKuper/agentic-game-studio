---
name: ags-architecture-decision
description: "Creates an Architecture Decision Record (ADR) documenting a significant technical decision, its context, alternatives considered, and consequences. Every major technical choice should have an ADR."
argument-hint: "[title]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Task, AskUserQuestion
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

On invoke:

## Prerequisites

Verify required artifacts before starting. STOP on first missing item with redirect.

| Artifact | Created by | If missing |
|---|---|---|
| `.ags/docs/engine-reference/[engine]/VERSION.md` | `/ags-setup-engine` | STOP. "Engine reference missing. Run `/ags-setup-engine` first." |
| `design/gdd/game-concept.md` (no `{{...}}`) | `/ags-brainstorm` | STOP. "No game concept. Run `/ags-brainstorm` first." |
| `design/gdd/systems-index.md` | `/ags-map-systems` | STOP. "No systems map. Run `/ags-map-systems` first." |
| At least one `design/gdd/[system].md` (Approved or Designed status) | `/ags-design-system` | STOP. "No system GDDs yet. Run `/ags-design-system [system-name]` for at least one system." |
| Cited GDD section has `status: approved` in YAML front-matter | user approval of GDD | STOP. "ABORT — precondition not met. Required: design/gdd/<sys>.md status=approved. Found: status=draft (or missing front-matter). Fix: get GDD approved (set status: approved + approved_at), then retry." See `.ags/rules/document-boundaries.md`. |
| `design/architecture/architecture.md` | `/ags-create-architecture` | STOP. "No architecture skeleton. Run `/ags-create-architecture` (Foundation phase)." |

If any STOP triggers, exit with verdict **BLOCKED — missing prerequisite** and surface the redirect.

---

## 0. Parse Arguments — Detect Retrofit Mode

**If argument starts with `retrofit` + file path** (e.g., `/ags-architecture-decision retrofit design/architecture/adr-0001-event-system.md`):

Enter **retrofit mode**:

1. Read existing ADR fully.
2. Identify present sections by heading scan:
   - YAML front-matter `status:` — **BLOCKING if missing**: `/ags-story-readiness` cannot check ADR acceptance
   - `## ADR Dependencies` — HIGH if missing: dependency ordering breaks
   - `## Engine Compatibility` — HIGH if missing: post-cutoff risk unknown
   - `## GDD Requirements Addressed` — MEDIUM if missing: traceability lost
3. Present:
   ```
   ## Retrofit: [ADR title]
   File: [path]

   Sections already present (will not be touched):
   ✓ status (front-matter): [current value, or "MISSING — will add"]
   ✓ [section]

   Missing sections to add:
   ✗ front-matter status — BLOCKING (stories cannot validate ADR acceptance without this)
   ✗ ADR Dependencies — HIGH
   ✗ Engine Compatibility — HIGH
   ```
4. Ask: "Add the [N] missing sections? Existing content untouched."
5. If yes:
   - **status (front-matter)**: default `draft`. Ask user only if retrofitting an already-approved ADR — set `status: approved` + `approved_at: YYYY-MM-DD`.
   - **ADR Dependencies**: ask — "Depend on any ADR? Enable or block any ADR/epic?" Accept "None".
   - **Engine Compatibility**: read engine reference docs (Step 0 below), confirm domain with user, generate table with verified data.
   - **GDD Requirements Addressed**: ask — "Which GDDs motivated this? What requirement does this ADR address?"
   - Append missing sections via Edit. **Never modify existing sections.** Append/fill only.
6. Update `## Date` if absent.
7. Suggest: "Run `/ags-architecture-review` to re-validate coverage."

If NOT retrofit, proceed to Step 0 (normal authoring).

**No-argument guard**: If title empty, ask before Phase 0:

> "What technical decision are you documenting? Provide a short title (e.g., `event-system-architecture`, `physics-engine-choice`)."

Use response as title, proceed to Step 0.

---

## 0a. GDD Precondition Check (BLOCKING — automatic, no user prompt)

Per `.ags/rules/document-boundaries.md`:

1. Identify GDD section(s) this ADR will serve. Derive from title/argument/context. If unclear, ask user once: "Which GDD section does this ADR serve? (path#anchor)".
2. Read each cited GDD's YAML front-matter (top of file between `---` markers).
3. Check `status:` field.
   - `status: approved` AND `approved_at:` populated → PASS, continue.
   - `status: draft`, missing front-matter, or missing `approved_at` → ABORT with:
     ```
     ABORT — precondition not met.
     Required: design/gdd/<sys>.md status=approved.
     Found: <observed state>.
     Fix: get GDD approved (set status: approved + approved_at: YYYY-MM-DD), then retry.
     ```
   - Foundational ADR (no GDD) → user must explicitly state "foundational" in argument; record as `**GDD source**: Foundational — no GDD requirement. Enables: <list>` later in Step 4.
4. Record cited paths + approval dates — written into ADR `**GDD source**:` line immediately before `## Date` section in Step 4.

---

## 0. Load Engine Context (ALWAYS FIRST)

1. Read `.ags/docs/engine-reference/[engine]/VERSION.md`:
   - Engine name + version
   - LLM knowledge cutoff date
   - Post-cutoff risk levels (LOW / MEDIUM / HIGH)

2. Identify **domain** from title/description. Common: Physics, Rendering, UI, Audio, Navigation, Animation, Networking, Core, Input, Scripting.

3. Read module reference if exists: `.ags/docs/engine-reference/[engine]/modules/[domain].md`.

4. Read `.ags/docs/engine-reference/[engine]/breaking-changes.md` — flag domain changes post-cutoff.

5. Read `.ags/docs/engine-reference/[engine]/deprecated-apis.md` — flag deprecated APIs in domain.

6. **Display knowledge gap warning** if domain MEDIUM or HIGH risk:

   ```
   ⚠️  ENGINE KNOWLEDGE GAP WARNING
   Engine: [name + version]
   Domain: [domain]
   Risk Level: HIGH — This version is post-LLM-cutoff.

   Key changes verified from engine-reference docs:
   - [Change 1 relevant to this domain]
   - [Change 2]

   This ADR will be cross-referenced against the engine reference library.
   Proceed with verified information only — do NOT rely solely on training data.
   ```

   No engine configured: prompt "No engine configured. Run `/ags-setup-engine` first, or tell me which engine."

---

## 1. Determine the next ADR number

Scan `design/architecture/` for existing ADRs.

---

## 2. Gather context

Read related code, existing ADRs, relevant GDDs from `design/gdd/`.

### 2a: Architecture Registry Check (BLOCKING gate)

Read `docs/registry/architecture.yaml`. Extract entries relevant to domain/decision.

Present relevant stances **before** collaborative design as locked constraints:

```
## Existing Architectural Stances (must not contradict)

State Ownership:
  player_health → owned by health-system (ADR-0001)
  Interface: HealthComponent.current_health (read-only float)
  → If this ADR reads or writes player health, it must use this interface.

Interface Contracts:
  damage_delivery → signal pattern (ADR-0003)
  Signal: damage_dealt(amount, target, is_crit)
  → If this ADR delivers or receives damage events, it must use this signal.

Forbidden Patterns:
  ✗ autoload_singleton_coupling (ADR-0001)
  ✗ direct_cross_system_state_write (ADR-0000)
  → The proposed approach must not use these patterns.
```

If proposed decision contradicts a registered stance, surface immediately:

> "⚠️ Conflict: This ADR proposes [X], but ADR-[NNNN] established [Y] as accepted pattern. Proceeding produces contradictory ADRs and inconsistent stories.
> Options: (1) Align with existing stance, (2) Supersede ADR-[NNNN] explicitly, (3) Explain why this case is exception."

Do not proceed to Step 3 until conflict resolved or accepted as intentional exception.

---

## 3. Guide the decision collaboratively

Derive best guesses from context (GDDs, engine reference, existing ADRs). Present **confirm/adjust** prompt via `AskUserQuestion` — not open-ended.

**Derive assumptions:**
- **Problem**: infer from title + GDD context
- **Alternatives**: 2-3 concrete options from engine reference + GDD
- **Dependencies**: scan ADRs for upstream; assume None if unclear
- **GDD linkage**: extract GDD systems title relates to
- **status (front-matter)**: always `draft` for new ADRs — never ask

**Scope**: problem framing, alternatives, upstream dependencies, GDD linkage, status only. Schema design questions (e.g., "How should spawn timing work?") are NOT assumptions — separate step after confirmation. Do not include schema design in assumptions widget.

**After assumptions confirmed**: if ADR involves schema/data design, separate multi-tab `AskUserQuestion` per design question before drafting.

**Present assumptions:**

```
Here's what I'm assuming before drafting:

Problem: [one-sentence problem statement derived from context]
Alternatives I'll consider:
  A) [option derived from engine reference]
  B) [option derived from GDD requirements]
  C) [option from common patterns]
GDD systems driving this: [list derived from context]
Dependencies: [upstream ADRs if any, otherwise "None"]
status (front-matter): draft

[A] Proceed — draft with these assumptions
[B] Change the alternatives list
[C] Adjust the GDD linkage
[D] Add a performance budget constraint
[E] Something else needs changing first
```

Do not generate ADR until user confirms or corrects.

**Design Principles Check** (per `.ags/rules/design-principles.md`) — before generating ADR, verify decision against:

- §3 Separation of Concerns — decision keeps gameplay / UI / audio / data / save / AI in separate layers; cross-layer access via interface or event only.
- §4 Loose Coupling / High Cohesion — no direct cross-module references; system has single responsibility with related data adjacent.
- §5 SSoT — decision names a single owner for any runtime fact it introduces; no shared mutable state with ambiguous authority.
- §6 Fail Fast — content / config / data loading paths include validation; no silent fallback on missing or malformed data.
- §7 Observability — gameplay-critical or perf-sensitive system specifies debug / log / metric hooks in Implementation Guidelines.
- §8 Backward Compatibility — change to save format / mod API / content schema names migration plan; ADR notes version impact.
- §9 Evolutionary Architecture — ADR `Consequences` notes reversibility (cheap / costly / one-way); commitments hidden behind interface where reversal cost is high.

If decision violates a principle without explicit justification, raise as Open Question or revise before drafting.

**After engine specialist + TD reviews return** (Step 4.5/4.6), unresolved decisions get separate `AskUserQuestion` per point with proposed options + free-text escape:

```
Decision: [specific unresolved point]
[A] [option from specialist review]
[B] [alternative option]
[C] Different approach — I'll describe it
```

**ADR Dependencies** — derive from ADRs, then confirm:
- Depend on ADR not yet Accepted?
- Unlock or unblock another ADR/epic?
- Block any epic from starting?

Record in **ADR Dependencies**. Write "None" if no constraint.

---

## 4. Generate the ADR

Format:

```markdown
---
status: draft
approved_at:
---

# ADR-[NNNN]: [Title]

**GDD source**: `design/gdd/<system>.md#<section-anchor>` (status: approved at YYYY-MM-DD)
<!-- Multiple sources = multiple lines. Foundational: "Foundational — no GDD requirement. Enables: <list>". Filled from Step 0a. -->

## Date
[Date of decision]

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | [e.g. Unity 6000.0.30f1] |
| **Domain** | [Physics / Rendering / UI / Audio / Navigation / Animation / Networking / Core / Input] |
| **Knowledge Risk** | [LOW / MEDIUM / HIGH — from VERSION.md] |
| **References Consulted** | [List engine-reference docs read, e.g. `.ags/docs/engine-reference/unity/modules/physics.md`] |
| **Post-Cutoff APIs Used** | [Any APIs from post-LLM-cutoff versions this decision depends on, or "None"] |
| **Verification Required** | [Specific behaviours to test before shipping, or "None"] |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | [ADR-NNNN (must be Accepted before this can be implemented), or "None"] |
| **Enables** | [ADR-NNNN (this ADR unlocks that decision), or "None"] |
| **Blocks** | [Epic/Story name — cannot start until this ADR is Accepted, or "None"] |
| **Ordering Note** | [Any sequencing constraint that isn't captured above] |

## Context

### Problem Statement
[What problem are we solving? Why does this decision need to be made now?]

### Constraints
- [Technical constraints]
- [Timeline constraints]
- [Resource constraints]
- [Compatibility requirements]

### Requirements
- [Must support X]
- [Must perform within Y budget]
- [Must integrate with Z]

## Decision

[The specific technical decision made, described in enough detail for someone
to implement it.]

### Architecture Diagram
[ASCII diagram or description of the system architecture this creates]

### Key Interfaces
[API contracts or interface definitions this decision creates]

## Alternatives Considered

### Alternative 1: [Name]
- **Description**: [How this would work]
- **Pros**: [Advantages]
- **Cons**: [Disadvantages]
- **Rejection Reason**: [Why this was not chosen]

### Alternative 2: [Name]
- **Description**: [How this would work]
- **Pros**: [Advantages]
- **Cons**: [Disadvantages]
- **Rejection Reason**: [Why this was not chosen]

## Consequences

### Positive
- [Good outcomes of this decision]

### Negative
- [Trade-offs and costs accepted]

### Risks
- [Things that could go wrong]
- [Mitigation for each risk]

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| [system-name].md | [specific rule, formula, or performance constraint from that GDD] | [how this decision satisfies it] |

## Performance Implications
- **CPU**: [Expected impact]
- **Memory**: [Expected impact]
- **Load Time**: [Expected impact]
- **Network**: [Expected impact, if applicable]

## Migration Plan
[If this changes existing code, how do we get from here to there?]

## Validation Criteria
[How will we know this decision was correct? What metrics or tests?]

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

5. **Write approval** — `AskUserQuestion`:

GDD sync issues found:
- "ADR draft complete. How proceed?"
  - [A] Write ADR + update GDD same pass
  - [B] Write ADR only — I'll update GDD manually
  - [C] Not yet — review further

No GDD sync issues:
- "ADR draft complete. May I write?"
  - [A] Write ADR to `design/architecture/adr-[NNNN]-[slug].md`
  - [B] Not yet — review further

If yes, write file, create dir if needed. Option [A] with GDD update: also update GDD(s) to new names.

6. **Update Architecture Registry**

Scan written ADR for new architectural stances:
- State it claims ownership of
- Interface contracts (signal signatures, method APIs)
- Performance budgets claimed
- API choices made explicitly
- Patterns banned (Consequences → Negative or "do not use X")

Present candidates:
```
Registry candidates from this ADR:
  NEW state ownership:      player_stamina → stamina-system
  NEW interface contract:   stamina_depleted signal
  NEW performance budget:   stamina-system: 0.5ms/frame
  NEW forbidden pattern:    polling stamina each frame (use signal instead)
  EXISTING (referenced_by update only): player_health → already registered ✅
```

**Registry append logic**: When writing to `docs/registry/architecture.yaml`, do NOT assume sections empty. Previous ADRs in session may have entries. Before each Edit:
1. Read current state of `docs/registry/architecture.yaml`
2. Find correct section (state_ownership, interfaces, forbidden_patterns, api_decisions)
3. Append new entry AFTER last existing entry — do not replace `[]` placeholder that may not exist
4. If section has entries, use closing of last entry as `old_string` anchor, append after

**BLOCKING — do not write to `docs/registry/architecture.yaml` without explicit user approval.**

`AskUserQuestion`:
- "Update `docs/registry/architecture.yaml` with these [N] new stances?"
  - Options: "Yes — update registry", "Not yet — review candidates", "Skip registry update"

Yes only: append. Never modify existing — superseding stance: set old to `status: superseded_by: ADR-[NNNN]` and add new.

---

## 7. Closing Next Steps

After ADR written (and registry optionally updated), close with `AskUserQuestion`.

Before generating widget:
1. Read `docs/registry/architecture.yaml` — check unwritten priority ADRs (flagged in technical-preferences.md or systems-index.md as prerequisites)
2. All prerequisite ADRs written? Include "Start writing GDDs" option.
3. List ALL remaining priority ADRs as individual options — not just next one or two.

Widget format:
```
ADR-[NNNN] written and registry updated. What would you like to do next?
[1] Write [next-priority-adr-name] — [brief description from prerequisites list]
[2] Write [another-priority-adr] — [brief description]  (include ALL remaining ones)
[N] Start writing GDDs — run `/ags-design-system [first-undesigned-system]` (only show if all prerequisite ADRs are written)
[N+1] Stop here for this session
```

No remaining priority ADRs and no undesigned GDDs: offer "Stop here" only, suggest `/ags-architecture-review` in fresh session.

**Always include this fixed notice (do NOT omit):**

> To validate ADR coverage against your GDDs, open a **fresh Claude Code session**
> and run `/ags-architecture-review`.
>
> **Never run `/ags-architecture-review` in the same session as `/ags-architecture-decision`.**
> The reviewing agent must be independent of the authoring context to give an unbiased
> assessment. Running it here would invalidate the review.

Update stories `Status: Blocked` pending this ADR → `Status: Ready`.
