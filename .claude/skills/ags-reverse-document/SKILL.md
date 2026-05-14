---
name: ags-reverse-document
description: "Generate design or architecture documents from existing implementation. Works backwards from code to create missing planning docs."
argument-hint: "<type> <path> (e.g., 'design Assets/Scripts/Gameplay/combat' or 'architecture Assets/Scripts/core')"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, AskUserQuestion
# Read-only diagnostic skill — no specialist agent delegation needed
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

# Reverse Documentation

Analyzes existing implementation (code, systems) and generates design or
architecture documentation. Use when: built feature without design doc,
inherited undocumented codebase, or need to document "why" behind existing code.

---

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| Engine source code present | engine init | STOP. "No engine source — nothing to reverse-document." |
| Target source path argument | user | STOP. "Usage: `/ags-reverse-document <path>` or `--all`." |

If STOP triggers, exit verdict **BLOCKED**.

---

## Workflow

## Phase 1: Parse Arguments

**Format**: `/ags-reverse-document <type> <path>`

**Type options**:
- `design` → Generate a game design document (GDD section)
- `architecture` → Generate an Architecture Decision Record (ADR)

**Path**: Directory or file to analyze
- `Assets/Scripts/Gameplay/combat/` → All combat-related code
- `Assets/Scripts/Core/event-system.cpp` → Specific file

**Examples**:
```bash
/ags-reverse-document design Assets/Scripts/Gameplay/magic-system
/ags-reverse-document architecture Assets/Scripts/Core/entity-component
```

## Phase 2: Analyze Implementation

**Read and understand the code**:

**For design docs (GDD):**
- Identify mechanics, rules, formulas
- Extract gameplay values (damage, cooldowns, ranges)
- Find state machines, ability systems, progression
- Detect edge cases handled in code
- Map dependencies (what systems interact?)

**For architecture docs (ADR):**
- Identify patterns (ECS, singleton, observer, etc.)
- Understand technical decisions (threading, serialization, etc.)
- Map dependencies and coupling
- Assess performance characteristics
- Find constraints and trade-offs

## Phase 3: Ask Clarifying Questions

**DO NOT** just describe the code. **ASK** about intent:

**Design questions**:
- "I see a [resource] system that depletes during [activity]. Was this for:
  - Pacing (prevent spam)?
  - Resource management (strategic depth)?
  - Or something else?"
- "The [mechanic] seems central. Is this a core pillar, or supporting feature?"
- "[Value] scales exponentially with [factor]. Intentional design, or needs rebalancing?"

**Architecture questions**:
- "You're using a service locator pattern. Was this chosen for:
  - Testability (mock dependencies)?
  - Decoupling (reduce hard references)?
  - Or inherited from existing code?"
- "I see manual memory management instead of smart pointers. Performance requirement, or legacy?"

## Phase 4: Present Findings

Before drafting, show what you discovered:

```
I've analyzed [path]/. Here's what I found:

MECHANICS IMPLEMENTED:
- [mechanic-a] with [property] (e.g. timing windows, cooldowns)
- [mechanic-b] (e.g. interaction between two states)
- [resource] system (depletes on [action], regens on [condition])
- [state] system (builds up, triggers [effect])

FORMULAS DISCOVERED:
- [Output] = [formula using discovered variables]
- [Secondary output] = [formula]

UNCLEAR INTENT AREAS:
1. [Resource] system — pacing or resource management?
2. [Mechanic] — core pillar or supporting feature?
3. [Value] scaling — intentional design or needs tuning?

Before I draft the design doc, could you clarify these points?
```

Wait for user to clarify intent before drafting.

## Phase 5: Draft Document Using Template

Based on type, use appropriate template:

| Type | Template | Output Path |
|------|----------|-------------|
| `design` | `.ags/templates/t_design-from-implementation.md` | `design/gdd/[system-name].md` |
| `architecture` | `.ags/templates/t_architecture-from-code.md` | `design/architecture/[decision-name].md` |

**Draft structure**:
- Capture **what exists** (mechanics, patterns, implementation)
- Document **why it exists** (intent clarified with user)
- Identify **what's missing** (edge cases not handled, gaps in design)
- Flag **follow-up work** (balance tuning, missing features)

## Phase 6: Show Draft and Request Approval

**Collaborative protocol**:
```
I've drafted the [system-name] design doc based on your code and clarifications.

[Show key sections: Overview, Mechanics, Formulas, Design Intent]

ADDITIONS I MADE:
- Documented [mechanic] as "[intent]" per your clarification
- Added edge cases not in code (e.g., what if [resource] hits 0 mid-[action]?)
- Flagged balance concern: [scaling type] scaling at [boundary condition]

SECTIONS MARKED AS INCOMPLETE:
- "[System] interaction with [other-system]" (not fully implemented yet)
- "[Variant or feature]" (only [subset] implemented so far)

May I write this to design/gdd/[system-name].md?
```

Wait for approval. User may request changes before writing.

## Phase 7: Write Document with Metadata

When approved, write the file with special markers:

```markdown
---
status: reverse-documented
source: [path/]
date: [today]
verified-by: [User name]
---

# [System Name] Design

> **Note**: This document was reverse-engineered from the existing implementation.
> It captures current behavior and clarified design intent. Some sections may be
> incomplete where implementation is partial or intent was unclear.

[Rest of document...]
```

## Phase 8: Flag Follow-Up Work

After writing, suggest next steps:

```
✅ Written to design/gdd/combat-system.md

FOLLOW-UP RECOMMENDED:
1. Run /ags-balance-check on combat formulas (exponential scaling concern)
2. Create ADR for stamina system architecture decision
3. Implement missing edge cases:
   - Stamina depletion mid-combo behavior
   - Stagger state during dodge
4. Extend design doc when weapon variety is implemented

Would you like me to tackle any of these now?
```

---

## Template Selection Logic

| If analyzing... | Use template... | Because... |
|----------------|-----------------|------------|
| `Assets/Scripts/Gameplay/*` | design-doc-from-implementation.md | Gameplay mechanics → GDD |
| `Assets/Scripts/Core/*`, `Assets/Scripts/AI/*` | architecture-doc-from-code.md | Core systems → ADR |
| `Assets/Scripts/Networking/*` | architecture-doc-from-code.md | Technical systems → ADR |
| `Assets/Scripts/UI/*` | design-doc-from-implementation.md | UI/UX → design spec |

---

## Example Session: Reverse-Document a System

```
User: /ags-reverse-document design Assets/Scripts/Gameplay/[system]

Agent: I'll analyze your [system] implementation to create a design doc.
       [Reads code, discovers mechanics, formulas]

Agent: I've analyzed Assets/Scripts/Gameplay/[system]/. Here's what I found:
       [Shows mechanics, formulas, unclear areas]

       Before drafting, could you clarify:
       1. [Resource] system intent?
       2. Is [mechanic] a core pillar?
       3. [Value] scaling — intentional or needs tuning?

User: 1. [Resource] is for pacing, prevent [unwanted behavior]
      2. [Mechanic] is core — I want [design intent]
      3. Yeah, exponential is too much, should be linear

Agent: Perfect, that clarifies the vision. Let me draft the design doc.
       [Shows draft with clarified intent]

       May I write this to design/gdd/[system-name].md?

User: Yes, write it.

Agent: ✅ Written to design/gdd/[system-name].md
       ✅ Marked as [REVERSE-DOCUMENTED]
       ✅ Flagged [value] scaling for rebalancing

       Next steps:
       - Update [formula] to [corrected scaling]
       - Run /ags-balance-check to validate [curve]
       - Document [mechanic] as core pillar in game-pillars.md
```

---

## Collaborative Protocol

1. **Analyze first** — read code, understand implementation
2. **Question intent** — ask about "why", not just "what"
3. **Present findings** — show discoveries, highlight unclear areas
4. **User clarifies** — separate intent from accidents
5. **Draft document** — based on reality + intent
6. **Show draft** — display key sections, explain additions
7. **Get approval** — "May I write to [filepath]?" Approve → **COMPLETE**. Decline → **BLOCKED**.
8. **Flag follow-up** — suggest related work, don't auto-execute

**NEVER assume intent. ALWAYS ask before documenting "why".**

---

## Combined Review Loop (parallel external Codex)

Per `.ags/rules/review-workflow.md`. Authoring + internal review runs **in parallel** with external Codex inside one loop. Each iteration:

1. Resolve severity floor: iter 1-2 → keep all severities; iter 3-4 → critical/high; iter 5+ → critical only.
2. Persist current draft to `.ags/project/reviews/.tmp/reverse-doc-[type]-[slug]-iter[N]-draft.md`.
3. **Spawn in one message, in parallel**:
   - All internal reviewer Tasks (specialists matching reversed doc type + relevant director gate).
   - `/ags-external-review [matching-type] [draft-path] --embedded-parallel --iteration [N] --min-severity [floor]` — pick the `--type` matching the reversed doc (`gdd` / `adr` / `ux` / etc.); fall back to `custom` if none fits. Codex unavailable returns `skipped: codex-unavailable`; aggregator logs skip in decisions-log and continues with internal pool only.
4. Aggregator (owning director for the reversed doc type — `game-designer` for GDD, `technical-director` for ADR, `ux-designer` for UX, etc.) merges findings from internal + Codex, drops nitpicks + below-floor.
5. **Loop exit**: filtered set empty → proceed to write approval. Non-empty → surface aggregated kept findings, user revises draft, N++, repeat.

No iteration cap. No user-confirm gate before external — it runs every iteration automatically.
