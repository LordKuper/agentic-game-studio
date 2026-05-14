---
name: ags-art-bible
description: "Guided, section-by-section Art Bible authoring. Creates the visual identity specification that gates all asset production. Run after /ags-brainstorm is approved and before /ags-map-systems or any GDD authoring begins."
argument-hint: "[no arguments]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task, AskUserQuestion, Bash
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

## Source of Truth

**Template is SSoT.** Skill does NOT enumerate sections, agents, or modes — it parses `.ags/templates/t_art-bible.html` at runtime and derives the work plan from it. Editing the template (adding a section, changing an agent assignment, marking a section status-only) propagates to skill behavior with no skill edits required.

Output file: `design/art/ags-art-bible.html`.

### Template contract (the skill relies on these conventions)

Each authored block is `<section id="..." [data-*]>`:

| Attribute | Purpose | Default |
|---|---|---|
| `id` | Stable identifier for the section | required |
| `<h2>` (first child) | Display heading | required |
| `data-agent` | Primary agent to spawn for drafting | `art-director` |
| `data-consult` | Comma-separated agents spawned in parallel for cross-domain check | none |
| `data-mode` | `creative` (draft via agent) or `status` (system-recorded, no creative draft) | `creative` |
| `data-design-md` | `co-author` (skill must co-author DESIGN.md entries before approval) or `lint-required` (re-run lint before approval) | none |
| `data-depends-on` | `\|`-separated list of upstream sources this section reads. Each entry: `<path>` (full file), `<path>:<scope>` (namespace inside DESIGN.md, e.g. `colors,components`), `<path>#<section-id>` (specific section), or `self#<section-id>` (sibling section in same art-bible) | none |
| `data-checked-at` | ISO date (YYYY-MM-DD) — when the section was last validated against its `data-depends-on` sources | empty |
| `data-checked-against` | `\|`-separated git blob hashes (one per `data-depends-on` entry, same order) — snapshot of upstream content at last validation | empty |

Document-level status uses `<meta name="status" content="draft|approved">` and `<meta name="approved-at" content="YYYY-MM-DD">` in `<head>`. See `.ags/rules/document-boundaries.md`.

---

## Phase 0: Parse Arguments and Context Check

Read `design/gdd/game-concept.md`. If missing, fail:
> "No game concept found. Run `/ags-brainstorm` first — the art bible is authored after the game concept is approved."

Extract from `game-concept.md`:
- Game title (working title)
- Core fantasy and elevator pitch
- Game pillars (all of them)
- **Visual Identity Anchor** if present (from brainstorm Phase 4 art-director output)
- Target platform (if noted)

**Read template** `.ags/templates/t_art-bible.html`. Parse every `<section ...>` block. For each, extract:
- `id`
- `<h2>` text (display heading)
- `data-agent` (default `art-director`)
- `data-consult` (split on comma, trim; default empty)
- `data-mode` (default `creative`)
- `data-design-md` (default none)
- Full inner HTML (becomes the placeholder skeleton the section is built from)

This list IS the work plan. Skill iterates it. Skill MUST NOT inject sections not present in template, skip sections present in template, or reorder them.

**Retrofit mode detection**: Glob `design/art/ags-art-bible.html`. If exists:
- Read in full
- For each `<section id>` from the template plan, locate same id in existing file
- Decide status: `Complete` (body contains real content, no `.placeholder` / `[bracketed]` stubs left) or `Empty/Placeholder`
- Present table to user listing every template section + status:
  > "Found existing art bible at `design/art/ags-art-bible.html`. [N] sections complete, [M] need content. I'll work on incomplete sections only — existing content will not be touched."
- Sections not appearing in existing file: treat as new, author them.
- Sections appearing in existing file but no longer in template: surface as warning ("legacy section, no longer in template — keep / remove?") via `AskUserQuestion`. Do not silently drop.

If file does not exist, fresh authoring session — proceed.

Read `.ags/rules/technical-preferences.md` if exists — extract performance budgets and engine for technical constraints.

Read `.ags/rules/design-system.md` — DESIGN.md is canonical token format.

Glob `design/art/DESIGN.md`:
- If exists — read, extract token names, summary to user.
- If absent — flag: DESIGN.md MUST be authored alongside bible. Co-authoring happens automatically when iterating through sections with `data-design-md="co-author"`.

---

## Phase 1: Framing

Use `AskUserQuestion` with two tabs:
- Tab **"Scope"** — "Which sections need to be authored today?"
  Build options dynamically from parsed template — e.g. `Full bible — all [N] sections` / `Resume — fill in missing sections` / `Custom — pick sections`
- Tab **"References"** — "Do you have reference games, films, or art that define visual direction?" (free text)

If `game-concept.md` has Visual Identity Anchor:
> "Found a visual identity anchor from brainstorm: '[anchor name] — [one-line rule]'. I'll use this as foundation."

**Initialize output file** (if not retrofit): Copy `.ags/templates/t_art-bible.html` to `design/art/ags-art-bible.html`. Replace `<h1>` placeholder with game title. Set `<meta name="last-updated">` to today. Keep `<meta name="status" content="draft">`.

---

## Phase 2: Section Iteration Loop

Iterate over parsed sections in template order. Skip sections excluded by user scope (Phase 1) and sections marked Complete in retrofit mode.

For each section, run the protocol matching its `data-mode`:

### Mode: `creative` (default)

1. **Brief**: prepare context for the section. Always include:
   - Game concept (elevator pitch, fantasy, pillars, platform)
   - Visual identity anchor (if exists)
   - All previously approved sections (so each new section can anchor in prior decisions)
   - Section heading + section's current placeholder body from template (the placeholder text describes intent of the section — treat it as the spec)
   - References gathered in Phase 1
   - Cited governance docs (e.g. `.ags/rules/technical-preferences.md` for technical constraints)

2. **Agent spawn**:
   - Primary: spawn `data-agent` via Task with brief. Ask: "Draft the section titled '[heading]'. Use the placeholder body as guidance for what this section must cover. Anchor in stated pillars and prior sections. Do not invent sub-structure beyond what the placeholder body shows."
   - Consult: if `data-consult` non-empty, spawn each listed agent in **parallel** (single message, multiple Task calls). Each consult agent gets same brief + role-specific question (e.g. ux-designer: "Does the proposed art direction support readability/accessibility for the interaction patterns this game requires? Flag conflicts."; technical-artist: "Are any proposed art preferences in conflict with the engine constraints in `.ags/rules/technical-preferences.md`?").
   - If primary and consults conflict, surface BOTH positions explicitly via `AskUserQuestion`. Do NOT silently resolve.

3. **DESIGN.md co-authoring** (only if `data-design-md="co-author"`):
   - Identify token references (`{colors.*}`, `{typography.*}`, `{spacing.*}`, `{rounded.*}`, `{components.*}`) the section cites
   - For each missing token, co-author entry in `design/art/DESIGN.md` (prompt user for values, write to DESIGN.md front-matter)
   - Run `npx @google/design.md lint design/art/DESIGN.md` — must return errors=0 before section approval

4. **Approval**: present draft via `AskUserQuestion`:
   - Options: `[A] Lock in` / `[B] Revise` / `[C] Describe my own direction`

5. **Write to file**: replace the entire `<section id="X">…</section>` block in `design/art/ags-art-bible.html` with the approved content. **Preserve outer markup** (the `<section>` opening tag with its `id` and `data-*` attrs, the `<h2>` heading, sub-section `<h3>` structure where the placeholder defines it). Replace only `.placeholder` spans and stub cells with approved content.

6. **Stamp provenance** (mandatory on every section approval, including retrofit re-approval):
   - Resolve each entry in `data-depends-on` to a file path. For `<path>:<scope>` entries, the file is `<path>` (scope is informational — hash whole file). For `<path>#<section>` entries, the file is `<path>`. For `self#<id>` entries, the file is the art-bible itself (`design/art/ags-art-bible.html`).
   - For each resolved path, compute git blob hash via Bash: `git hash-object <path>` (works even on uncommitted files). If file is outside git, fall back to `sha256sum <path>` and prefix with `sha256:`.
   - Set `data-checked-at` on the `<section>` tag to today's date (YYYY-MM-DD).
   - Set `data-checked-against` to `|`-separated list of hashes (same order as `data-depends-on` entries).
   - This is non-creative book-keeping — no user prompt; happens automatically after the user approves the section content.

### Mode: `status`

System-recorded section, no creative draft. Skill fills it from observed project state:

- If `data-design-md="lint-required"`: run `npx @google/design.md lint design/art/DESIGN.md`. Record date + errors + warnings count into the section's status fields (look for `<dt>Last lint:</dt>` or analogous slot in the section's placeholder body). If errors > 0, abort the section and surface the lint output — fix DESIGN.md before retry.
- Fill any other observable fields the section's placeholder body declares (file paths, lint timestamps, etc.).
- No user `AskUserQuestion` approval — section is purely factual. Write directly after fields populated.

---

## Phase 3: Internal Review Loop (Art Director Sign-Off)

After all in-scope sections complete, spawn `creative-director` via Task using gate **AD-ART-BIBLE** (`.ags/rules/director-gates.md`).

Pass: art bible file path (`design/art/ags-art-bible.html`), game pillars, visual identity anchor.

**Loop exit condition**: single iteration where reviewer returns clean (no critical/high/medium findings). Non-clean → user revises affected sections (re-run Phase 2 protocol on those sections), re-spawn same gate. No iteration cap.

Record iteration count and final verdict in art bible by appending to `<header class="doc-header">`:
```html
<p class="meta"><strong>Art Director Sign-Off (AD-ART-BIBLE)</strong>: APPROVED [date] / CONCERNS (accepted) [date] / REVISED [date] | Iterations: [N]</p>
```

---

## Combined Review Loop (parallel external Codex)

Per `.ags/rules/review-workflow.md`. Internal review runs **in parallel** with external Codex inside one loop. Each iteration:

1. Resolve severity floor: iter 1-2 → all severities; iter 3-4 → critical/high; iter 5+ → critical only.
2. Persist current draft to `.ags/project/reviews/.tmp/[type]-[slug]-iter[N]-draft.html`.
3. **Spawn in one message, in parallel** (multiple Task calls + one Bash invocation):
   - Internal reviewer Task (creative-director, gate AD-ART-BIBLE).
   - `/ags-external-review art-bible [draft-path] --embedded-parallel --iteration [N] --min-severity [floor]` — Codex unavailable returns `skipped: codex-unavailable`; aggregator logs skip in `decisions-log` and continues with internal pool only.
4. Aggregator (`art-director`) merges findings, drops nitpicks + below-floor.
5. **Loop exit**: filtered set empty → write approval. Non-empty → surface kept findings, user revises affected sections, N++, repeat.

No iteration cap. No user-confirm gate before external. Record final iteration count for `decisions-log` entry written at skill completion.

---

## Phase 4: Approval and Close

On final approval:
1. Set `<meta name="status" content="approved">` in `<head>`.
2. Set `<meta name="approved-at" content="YYYY-MM-DD">` to today.
3. Swap `<span class="badge badge-draft">Draft</span>` for `<span class="badge badge-approved">Approved</span>`.
4. Append decision entry to `.ags/project/decisions-log.md`.

Before presenting next steps, check project state:
- Does `design/gdd/systems-index.md` exist? → map-systems done
- Does `.ags/rules/technical-preferences.md` contain configured engine? → setup-engine done
- Does `design/gdd/` contain any `*.md`? → design-system has run
- Does `design/gdd/gdd-cross-review-*.md` exist? → review-all-gdds done

Use `AskUserQuestion` for next steps. Only include options genuinely next:

- `[_] /ags-map-systems — decompose concept into systems before writing GDDs` (skip if systems-index.md exists)
- `[_] /ags-setup-engine — configure engine (asset standards may need revisiting)` (skip if engine configured)
- `[_] /ags-design-system — start the first GDD` (skip if any GDDs exist)
- `[_] /ags-review-all-gdds — cross-GDD consistency check` (skip if gdd-cross-review-*.md exists)
- `[_] /ags-asset-spec — generate per-asset visual specs from approved GDDs` (include if GDDs exist)
- `[_] /ags-consistency-check — scan GDDs against art bible` (include if GDDs exist)
- `[_] /ags-create-architecture — author master architecture document`
- `[_] Stop here`

Assign letters A, B, C… only to included options. Mark most logical pipeline-advancing option as `(recommended)`. Always include `/ags-create-architecture` and Stop here.

---

## Collaborative Protocol

Every `creative`-mode section: **Brief → Agent draft (with consults in parallel) → Conflict surfacing → Approval → Write to file**

- Never draft section without spawning `data-agent` (and `data-consult` if specified)
- Write each section to file immediately after approval — do not batch
- Surface agent disagreements — never silently resolve
- Art bible is a constraint document: restricts future decisions in exchange for visual coherence
- **HTML editing rule**: when replacing a section, preserve the outer `<section ... data-*>` tag attributes verbatim — they are the contract between template and skill.

---

## Recommended Next Steps

After art bible approved:
- `/ags-map-systems` — decompose concept into game systems
- `/ags-setup-engine` — if engine not yet configured
- `/ags-design-system [first-system]` — start per-system GDDs
- `/ags-consistency-check` — validate GDDs against art bible
- `/ags-create-architecture` — produce master architecture document
