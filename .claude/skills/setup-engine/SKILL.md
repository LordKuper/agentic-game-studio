---
name: setup-engine
description: "Configure the project's game engine and version. Pins Unity in CLAUDE.md, detects knowledge gaps, and populates engine reference docs via WebSearch when the version is beyond the LLM's training data."
argument-hint: "[version] | refresh | upgrade [old-version] [new-version] | no args for guided selection"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, WebSearch, WebFetch, Task, AskUserQuestion
---

When this skill is invoked:

## 1. Parse Arguments

The studio is Unity-only. The skill supports the following modes:

- **Version provided**: `/setup-engine 6000.0.30f1` — pin this Unity version
- **No args**: `/setup-engine` — fully guided mode (look up the latest stable Unity version, confirm with the user)
- **Refresh**: `/setup-engine refresh` — update reference docs (see Section 10)
- **Upgrade**: `/setup-engine upgrade [old-version] [new-version]` — migrate to a new Unity version (see Section 11)

If the user passes a non-Unity engine name (e.g. `godot`, `unreal`), reply:

> "This studio currently supports Unity only. The `unity-specialist` and
> `unity-dots-specialist` agents are the configured engine specialists. If you
> want to add another engine later, that requires new specialist agents and
> rule files."

Then stop unless the user opts back into Unity setup.

---

## 2. Confirm Unity is the right engine

Before pinning a version, verify Unity matches the project's needs.

Read `design/gdd/concept.md` if it exists — extract genre, scope, platform
targets, art style, team size. If no concept exists, inform the user:

> "No game concept found. Consider running `/brainstorm` first to discover
> what you want to build. Unity is well suited to mid-scope 3D, mobile, and
> games targeting console, with a massive asset store, C# scripting, and
> the strongest indie console certification path. If your concept is a 2D
> game with very tight performance budgets, or AAA-fidelity 3D, we should
> discuss before pinning Unity."

Use `AskUserQuestion`:
- Prompt: "Confirm Unity for this project?"
- Options:
  - `Yes — set up Unity` (Recommended)
  - `Discuss alternatives` — explain the tradeoffs and let the user defer engine setup

If the user picks "Discuss alternatives", explain that the studio currently
implements Unity-only specialist support and stop without writing files.

---

## 3. Look Up Current Version

Once Unity is confirmed:

- If a version was provided, use it
- If no version provided, use WebSearch to find the latest stable LTS release:
  - Search: `"Unity latest LTS version [current year]"`
  - Confirm with the user: "The latest stable Unity LTS is [version]. Use this?"

Always show the user the chosen version before continuing.

---

## 4. Update CLAUDE.md Technology Stack

Read `CLAUDE.md` and show the user the proposed Technology Stack changes.
Ask: "May I write these engine settings to `CLAUDE.md`?"

Wait for confirmation before making any edits.

Update the Technology Stack section, replacing the `[CHOOSE]` placeholders:

```markdown
- **Engine**: Unity [version]
- **Language**: C#
- **Build System**: Unity Build Pipeline
- **Asset Pipeline**: Unity Asset Import Pipeline + Addressables
```

---

## 5. Populate Technical Preferences

After updating CLAUDE.md, create or update `.ags/rules/technical-preferences.md`
with Unity defaults. Read the existing template first, then fill in:

### Engine & Language
- Engine: Unity [version]
- Language: C#

### Naming Conventions (Unity / C#)
- Classes: PascalCase (e.g., `PlayerController`)
- Public fields/properties: PascalCase (e.g., `MoveSpeed`)
- Private fields: `_camelCase` (e.g., `_moveSpeed`)
- Methods: PascalCase (e.g., `TakeDamage()`)
- Files: PascalCase matching class (e.g., `PlayerController.cs`)
- Constants: PascalCase or `UPPER_SNAKE_CASE`
- Assemblies: per-folder `.asmdef` (e.g., `Game.Combat.asmdef`)

### Input & Platform

Populate `## Input & Platform` using the answers gathered in Section 2 (or
extracted from the game concept). Derive the values using this mapping:

| Platform target | Gamepad Support | Touch Support |
|-----------------|-----------------|---------------|
| PC only         | Partial (recommended) | None    |
| Console         | Full            | None          |
| Mobile          | None            | Full          |
| PC + Console    | Full            | None          |
| PC + Mobile     | Partial         | Full          |
| Web             | Partial         | Partial       |

For **Primary Input**, use the dominant input for the game genre:
- Action / RPG / platformer targeting console → Gamepad
- Strategy / point-and-click / RTS → Keyboard/Mouse
- Mobile game → Touch
- Cross-platform → ask the user

Present the derived values and ask the user to confirm or adjust before writing.

### Remaining Sections
- **Performance Budgets**: ask whether to set defaults now (60 fps, 16.6 ms
  frame budget, 1000 draw call cap as a starting point for PC) or leave as
  `[TO BE CONFIGURED]` until target hardware is known.
- **Testing**: suggest Unity Test Framework (NUnit-based) — ask before adding.
- **Forbidden Patterns**: leave as placeholder — do NOT pre-populate.
- **Allowed Libraries**: leave as placeholder — do NOT pre-populate dependencies
  the project does not currently need. Only add a library here when it is
  actively being integrated, not speculatively.

> **Guardrail**: Never add speculative dependencies to Allowed Libraries. Add
> them only when the integration work is starting in this session.

### Engine Specialists Routing

Populate the `## Engine Specialists` section in `technical-preferences.md`:

```markdown
## Engine Specialists
- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist (C# review — primary covers it)
- **Shader Specialist**: unity-specialist (Shader Graph, HLSL, URP/HDRP materials)
- **UI Specialist**: unity-specialist (UI Toolkit UXML/USS, UGUI Canvas, runtime UI)
- **Addressables Specialist**: unity-specialist (asset loading, memory, content catalogs)
- **DOTS/ECS Specialist**: unity-dots-specialist (ECS, Jobs system, Burst compiler)
- **Routing Notes**: Invoke `unity-specialist` for architecture, general C# review,
  shaders/VFX, addressables, and UI. Invoke `unity-dots-specialist` only for
  ECS/Jobs/Burst code paths.

### File Extension Routing

| File Extension / Type                                    | Specialist to Spawn       |
|----------------------------------------------------------|---------------------------|
| Game code (.cs files)                                    | unity-specialist          |
| Shader / material files (.shader, .shadergraph, .mat)    | unity-specialist          |
| UI / screen files (.uxml, .uss, Canvas prefabs)          | unity-specialist          |
| Scene / prefab files (.unity, .prefab)                   | unity-specialist          |
| ECS / Jobs / Burst code (DOTS subsystems)                | unity-dots-specialist     |
| Native plugin files (.dll, native plugins)               | unity-specialist          |
| General architecture review                              | unity-specialist          |
```

Present the filled-in preferences to the user. Wait for approval before
writing the file.

---

## 6. Determine Knowledge Gap

Check whether the chosen Unity version is likely beyond the LLM's training data.

**Known approximate coverage** (update this as models change):
- LLM knowledge cutoff: **January 2026**
- Unity: training data likely covers up to ~6000.x mid-2025

Compare the user's chosen version against this baseline:

- **Within training data** → `LOW RISK` — reference docs optional but recommended
- **Near the edge** → `MEDIUM RISK` — reference docs recommended
- **Beyond training data** → `HIGH RISK` — reference docs required

Inform the user which category they are in and why.

---

## 7. Populate Engine Reference Docs

### If WITHIN training data (LOW RISK):

Create a minimal `.ags/docs/engine-reference/unity/VERSION.md`:

```markdown
# Unity — Version Reference

| Field                   | Value |
|-------------------------|-------|
| **Engine Version**      | [version] |
| **Project Pinned**      | [today's date] |
| **LLM Knowledge Cutoff**| January 2026 |
| **Risk Level**          | LOW — version is within LLM training data |

## Note

This Unity version is within the LLM's training data. Engine reference docs
are optional but can be added later if agents suggest incorrect APIs.

Run `/setup-engine refresh` to populate full reference docs at any time.
```

Do NOT create `breaking-changes.md`, `deprecated-apis.md`, etc. — they would
add context cost with minimal value.

### If BEYOND training data (MEDIUM or HIGH RISK):

Create the full reference doc set by searching the web:

1. **Search for the official migration / upgrade guide**:
   - `"Unity [old version] to [new version] migration guide"`
   - `"Unity [version] breaking changes"`
   - `"Unity [version] changelog"`
   - `"Unity [version] deprecated API"`

2. **Fetch and extract** from official documentation:
   - Breaking changes between each version from the training cutoff to current
   - Deprecated APIs with replacements
   - New features and best practices

Ask: "May I create the engine reference docs under `.ags/docs/engine-reference/unity/`?"

Wait for confirmation before writing any files.

3. **Create the full reference directory**:
   ```
   .ags/docs/engine-reference/unity/
   ├── VERSION.md                    # Version pin + knowledge gap analysis
   ├── breaking-changes.md           # Version-by-version breaking changes
   ├── deprecated-apis.md            # "Don't use X → Use Y" tables
   ├── current-best-practices.md     # New practices since training cutoff
   └── modules/                      # Per-subsystem references (create as needed)
   ```

4. **Populate each file** using real data from the web searches. Every file
   must have a "Last verified: [date]" header.

5. **For module files**: only create modules for subsystems where significant
   changes occurred. Don't create empty or minimal module files.

---

## 8. Update CLAUDE.md Import

Ask: "May I update the `@` import in `CLAUDE.md` to point to the new engine reference?"

Wait for confirmation, then update the `@` import under "Engine Version Reference":

```markdown
## Engine Version Reference

@.ags/docs/engine-reference/unity/VERSION.md
```

---

## 9. Update Agent Instructions

Ask: "May I add a Version Awareness section to the engine specialist agent files?"
before making any edits.

For `unity-specialist` and `unity-dots-specialist`, verify they have a "Version
Awareness" section that instructs the agent to:
1. Read `.ags/docs/engine-reference/unity/VERSION.md`
2. Check `deprecated-apis.md` before suggesting code
3. Check `breaking-changes.md` for relevant version transitions
4. Use WebSearch to verify uncertain APIs

---

## 10. Refresh Subcommand

If invoked as `/setup-engine refresh`:

1. Read the existing `.ags/docs/engine-reference/unity/VERSION.md` to get
   the current pinned version
2. Use WebSearch to check for:
   - New Unity releases since last verification
   - Updated migration guides
   - Newly deprecated APIs
3. Update all reference docs with new findings
4. Update "Last verified" dates on all modified files
5. Report what changed

---

## 11. Upgrade Subcommand

If invoked as `/setup-engine upgrade [old-version] [new-version]`:

### Step 1 — Read Current Version State

Read `.ags/docs/engine-reference/unity/VERSION.md` to confirm the current
pinned version, risk level, and any migration note URLs already recorded.
If `old-version` was not provided as an argument, use the pinned version
from this file.

### Step 2 — Fetch Migration Guide

Use WebSearch and WebFetch to locate the official Unity migration guide
between `old-version` and `new-version`:

- Search: `"Unity [old-version] to [new-version] migration guide"`
- Search: `"Unity [new-version] breaking changes changelog"`
- Fetch the migration guide URL from VERSION.md if one is recorded, or
  use the URL found via search.

Extract: renamed APIs, removed APIs, changed defaults, behavior changes,
and any "must migrate" items.

### Step 3 — Pre-Upgrade Audit

Scan the engine source root (`Assets/`) for code that uses APIs known to
be deprecated or changed in the target version:

- Use Grep to search for deprecated API names extracted from the migration
  guide (e.g. old function names, removed component types, changed property
  names)
- List each file that matches with the specific API reference found

Present the audit results as a table:

```
Pre-Upgrade Audit: Unity [old-version] → [new-version]
======================================================

Files requiring changes:
  File                                       | Deprecated API Found    | Effort
  ------------------------------------------ | ----------------------- | ------
  Assets/Scripts/Player/PlayerMovement.cs    | old_api_name            | Low
  Assets/Scripts/UI/HudController.cs         | removed_component_type  | Medium

Breaking changes to watch for:
  - [change description from migration guide]
  - [change description from migration guide]

Recommended migration order (dependency-sorted):
  1. [system/layer with fewest dependencies first]
  2. [next system]
  ...
```

If no deprecated APIs are found, report: "No deprecated API usage found —
upgrade may be low-risk."

### Step 4 — Confirm Before Updating

Ask the user before making any changes:

> "Pre-upgrade audit complete. Found [N] files using deprecated APIs.
> Proceed with upgrading VERSION.md to [new-version]?
> (This will update the pinned version and add migration notes — it does NOT
> change any source files. Source migration is done manually or via stories.)"

Wait for explicit confirmation before continuing.

### Step 5 — Update VERSION.md

After confirmation:

1. Update `.ags/docs/engine-reference/unity/VERSION.md`:
   - `Engine Version` → `[new-version]`
   - `Project Pinned` → today's date
   - `Last Docs Verified` → today's date
   - Re-evaluate and update the `Risk Level` and `Post-Cutoff Version Timeline`
     table if the new version falls beyond the LLM knowledge cutoff
   - Add a `## Migration Notes — [old-version] → [new-version]` section
     containing: migration guide URL, key breaking changes, deprecated APIs
     found in this project, and recommended migration order from the audit

2. If `breaking-changes.md` or `deprecated-apis.md` exist in the engine
   reference directory, append the new version's changes to those files.

### Step 6 — Post-Upgrade Reminder

After updating VERSION.md, output:

```
VERSION.md updated: Unity [old-version] → [new-version]

Next steps:
1. Migrate deprecated API usages in the [N] files listed above
2. Run /setup-engine refresh after upgrading the actual Unity binary to
   verify no new deprecations were missed
3. Run /architecture-review — the engine upgrade may invalidate ADRs that
   reference specific APIs or engine capabilities
4. If any ADRs are invalidated, run /propagate-design-change to update
   downstream stories
```

---

## 12. Output Summary

After setup is complete, output:

```
Engine Setup Complete
=====================
Engine:          Unity [version]
Language:        C#
Knowledge Risk:  [LOW/MEDIUM/HIGH]
Reference Docs:  [created/skipped]
CLAUDE.md:       [updated]
Tech Prefs:      [created/updated]
Agent Config:    [verified]

Next Steps:
1. Review .ags/docs/engine-reference/unity/VERSION.md
2. [If from /brainstorm] Run /map-systems to decompose your concept into individual systems
3. [If from /brainstorm] Run /design-system to author per-system GDDs (guided, section-by-section)
4. [If fresh start] Run /brainstorm to discover your game concept
5. Create your first milestone: /sprint-plan new
```

---

Verdict: **COMPLETE** — engine configured and reference docs populated.

## Guardrails

- NEVER guess a Unity version — always verify via WebSearch or user confirmation
- NEVER overwrite existing reference docs without asking — append or update
- Always show the user what you're about to change before making CLAUDE.md edits
- If WebSearch returns ambiguous results, show the user and let them decide
