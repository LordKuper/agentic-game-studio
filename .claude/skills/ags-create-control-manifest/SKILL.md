---
name: ags-create-control-manifest
description: "Produce a flat actionable rules sheet for programmers — what you must do, what you must never do, per system and per layer. Two modes: SEED (Foundation phase, derived from architecture skeleton + technical preferences + engine reference) and REFRESH (Production phase, regenerated from accumulated Accepted ADRs)."
argument-hint: "[seed | refresh]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Task
agent: technical-director
---

# Create Control Manifest

Flat, actionable rules sheet for programmers. Answers "what do I do?" and
"what must I never do?" — organized by architectural layer.

**Output:** `design/architecture/control-manifest.md`

**Modes:**

- **SEED (`seed` arg or first run)** — Foundation phase. Derive minimal rules from architecture skeleton, technical preferences, and engine reference docs. ADRs not required. Produces a usable starting manifest before any epics.
- **REFRESH (`refresh` arg or subsequent runs)** — Production phase. Regenerate from accumulated Accepted ADRs plus original seed inputs. Run after several epics have added ADRs (recommended every 3-5 epics).

If no argument and `design/architecture/control-manifest.md` does not exist → SEED. If exists → REFRESH.

**When to run:**
- Foundation: SEED mode after `/ags-create-architecture` skeleton is approved.
- Production: REFRESH mode after `/ags-architecture-review` periodically, or after a `revise` epic that changed architectural rules.

---

## 1. Load All Inputs

### ADRs
- Glob `design/architecture/adr-*.md` and read every file
- Filter to only Accepted ADRs (Status: Accepted) — skip Proposed, Deprecated,
  Superseded
- Note the ADR number and title for every rule sourced

### Technical Preferences
- Read `.ags/rules/technical-preferences.md`
- Extract: naming conventions, performance budgets, approved libraries/addons,
  forbidden patterns

### Engine Reference
- Read `.ags/docs/engine-reference/[engine]/VERSION.md` for engine + version
- Read `.ags/docs/engine-reference/[engine]/deprecated-apis.md` — these become
  forbidden API entries
- Read `.ags/docs/engine-reference/[engine]/current-best-practices.md` if it exists

Report: "Loaded [N] Accepted ADRs, engine: [name + version]."

---

## 2. Extract Rules from Each ADR

For each Accepted ADR, extract:

### Required Patterns (from "Implementation Guidelines" section)
- Every "must", "should", "required to", "always" statement
- Every specific pattern or approach mandated

### Forbidden Approaches (from "Alternatives Considered" sections)
- Every alternative that was explicitly rejected — *why* it was rejected becomes
  the rule ("never use X because Y")
- Any anti-patterns explicitly called out

### Performance Guardrails (from "Performance Implications" section)
- Budget constraints: "max N ms per frame for this system"
- Memory limits: "this system must not exceed N MB"

### Engine API Constraints (from "Engine Compatibility" section)
- Post-cutoff APIs that require verification
- Verified behaviours that differ from default LLM assumptions
- API fields or methods that behave differently in the pinned engine version

### Layer Classification
Classify each rule by the architectural layer of the system it governs:
- **Foundation**: Scene management, event architecture, save/load, engine init
- **Core**: Core gameplay loops, main player systems, physics/collision
- **Feature**: Secondary systems, secondary mechanics, AI
- **Presentation**: Rendering, audio, UI, VFX, shaders

If an ADR spans multiple layers, duplicate the rule into each relevant layer.

---

## 3. Add Global Rules

Combine rules that apply to all layers:

### From technical-preferences.md:
- Naming conventions (classes, variables, signals/events, files, constants)
- Performance budgets (target framerate, frame budget, draw call limits, memory ceiling)

### From deprecated-apis.md:
- All deprecated APIs → Forbidden API entries

### From current-best-practices.md (if available):
- Engine-recommended patterns → Required entries

### From technical-preferences.md forbidden patterns:
- Copy any "Forbidden Patterns" entries directly

---

## 4. Present Rules Summary Before Writing

Before writing the manifest, present a summary to the user:

```
## Control Manifest Preview
Engine: [name + version]
ADRs covered: [list ADR numbers]
Total rules extracted:
  - Foundation layer: [N] required, [M] forbidden, [P] guardrails
  - Core layer: [N] required, [M] forbidden, [P] guardrails
  - Feature layer: ...
  - Presentation layer: ...
  - Global: [N] naming conventions, [M] forbidden APIs, [P] approved libraries
```

Ask: "Does this look complete? Any rules to add or remove before I write the manifest?"

---

## 4b. Internal Review Loop — Technical Review

Spawn `technical-director` via Task using gate **TD-MANIFEST** (`.ags/rules/director-gates.md`).

Pass: the Control Manifest Preview from Phase 4 (rule counts per layer, full extracted rule list), the list of ADRs covered, engine version, and any rules sourced from technical-preferences.md or engine reference docs.

The technical-director reviews whether:
- All mandatory ADR patterns are captured and accurately stated
- Forbidden approaches are complete and correctly attributed
- No rules were added that lack a source ADR or preference document
- Performance guardrails are consistent with the ADR constraints

**Loop exit condition.** Single iteration where the reviewer returns clean (no critical/high/medium findings). Non-clean → user revises flagged rules / re-extracts from ADRs, re-spawn TD-MANIFEST. No iteration cap.

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

## 5. Write the Control Manifest

Ask: "May I write this to `design/architecture/control-manifest.md`?"

Format:

```markdown
# Control Manifest

> **Engine**: [name + version]
> **Last Updated**: [date]
> **Manifest Version**: [date]
> **ADRs Covered**: [ADR-NNNN, ADR-MMMM, ...]
> **Status**: [Active — regenerate with `/ags-create-control-manifest update` when ADRs change]

`Manifest Version` is the date this manifest was generated. Story files embed
this date when created. `/ags-story-readiness` compares a story's embedded version
to this field to detect stories written against stale rules. Always matches
`Last Updated` — they are the same date, serving different consumers.

This manifest is a programmer's quick-reference extracted from all Accepted ADRs,
technical preferences, and engine reference docs. For the reasoning behind each
rule, see the referenced ADR.

---

## Foundation Layer Rules

*Applies to: scene management, event architecture, save/load, engine initialisation*

### Required Patterns
- **[rule]** — source: [ADR-NNNN]
- **[rule]** — source: [ADR-NNNN]

### Forbidden Approaches
- **Never [anti-pattern]** — [brief reason] — source: [ADR-NNNN]

### Performance Guardrails
- **[system]**: max [N]ms/frame — source: [ADR-NNNN]

---

## Core Layer Rules

*Applies to: core gameplay loop, main player systems, physics, collision*

### Required Patterns
...

### Forbidden Approaches
...

### Performance Guardrails
...

---

## Feature Layer Rules

*Applies to: secondary mechanics, AI systems, secondary features*

### Required Patterns
...

### Forbidden Approaches
...

---

## Presentation Layer Rules

*Applies to: rendering, audio, UI, VFX, shaders, animations*

### Required Patterns
...

### Forbidden Approaches
...

---

## Global Rules (All Layers)

### Naming Conventions
| Element | Convention | Example |
|---------|-----------|---------|
| Classes | [from technical-preferences] | [example] |
| Variables | [from technical-preferences] | [example] |
| Signals/Events | [from technical-preferences] | [example] |
| Files | [from technical-preferences] | [example] |
| Constants | [from technical-preferences] | [example] |

### Performance Budgets
| Target | Value |
|--------|-------|
| Framerate | [from technical-preferences] |
| Frame budget | [from technical-preferences] |
| Draw calls | [from technical-preferences] |
| Memory ceiling | [from technical-preferences] |

### Approved Libraries / Addons
- [library] — approved for [purpose]

### Forbidden APIs ([engine version])
These APIs are deprecated or unverified for [engine + version]:
- `[api name]` — deprecated since [version] / unverified post-cutoff
- Source: `.ags/docs/engine-reference/[engine]/deprecated-apis.md`

### Cross-Cutting Constraints
- [constraint that applies everywhere, regardless of layer]
```

---

## 6. Suggest Next Steps

After writing the manifest:

- If epics/stories don't exist yet: "Run `/ags-create-epics layer: foundation` then `/ags-create-stories [epic-slug]` — programmers
  can now use this manifest when writing story implementation notes."
- If this is a regeneration (manifest already existed): "Updated. Recommend
  notifying the team of changed rules — especially any new Forbidden entries."

---

## Collaborative Protocol

1. **Load silently** — read all inputs before presenting
2. **Show summary first** — let user see scope before writing
3. **Ask before writing** — confirm before creating/overwriting. Write → **COMPLETE**. Decline → **BLOCKED**.
4. **Source every rule** — no rule without trace to ADR, technical preference, or engine ref doc
5. **No interpretation** — extract rules as stated; never paraphrase in ways that change meaning
