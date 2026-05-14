---
name: ags-brainstorm
description: "Guided game concept ideation — from zero idea to a structured game concept document. Uses professional studio ideation techniques, player psychology frameworks, and structured creative exploration."
argument-hint: "[genre or theme hint, or 'open']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, WebSearch, Task, AskUserQuestion
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

When this skill is invoked:

1. **Parse the argument** for an optional genre/theme hint (e.g., `roguelike`,
   `space survival`, `cozy farming`). If `open` or no argument, start from
   scratch.

2. **Check for existing concept work**:
   - Read `design/gdd/game-concept.md` if it exists (resume, don't restart)
   - Read `design/gdd/game-pillars.md` if it exists (build on established pillars)

3. **Run through ideation phases** interactively, asking the user questions at
   each phase. Do NOT generate everything silently — the goal is **collaborative
   exploration** where the AI acts as a creative facilitator, not a replacement
   for the human's vision.

   **Use `AskUserQuestion`** at key decision points throughout brainstorming:
   - Constrained taste questions (genre preferences, scope, team size)
   - Concept selection ("Which 2-3 concepts resonate?") after presenting options
   - Direction choices ("Develop further, explore more, or move to systems map?")
   - Pillar ranking after concepts are refined
   Write full creative analysis in conversation text first, then use
   `AskUserQuestion` to capture the decision with concise labels.

   Professional studio brainstorming principles to follow:
   - Withhold judgment — no idea is bad during exploration
   - Encourage unusual ideas — outside-the-box thinking sparks better concepts
   - Build on each other — "yes, and..." responses, not "but..."
   - Use constraints as creative fuel — limitations often produce the best ideas
   - Time-box each phase — keep momentum, don't over-deliberate early

---

### Phase 1: Creative Discovery

Start by understanding the person, not the game. Ask conversationally (not as checklist):

**Emotional anchors**:
- What's a moment in a game that genuinely moved you, thrilled you, or made
  you lose track of time? What specifically created that feeling?
- Is there a fantasy or power trip you've always wanted in a game but never
  quite found?

**Taste profile**:
- What 3 games have you spent the most time with? What kept you coming back?
  *(Ask this as plain text — the user must be able to type specific game names freely.
  Do NOT put this in an AskUserQuestion with preset options.)*
- Are there genres you love? Genres you avoid? Why?
- Do you prefer games that challenge you, relax you, tell you stories,
  or let you express yourself? *(Use `AskUserQuestion` for this — constrained choice.)*

**Practical constraints** (shape the sandbox before brainstorming).
Bundle these into a single multi-tab `AskUserQuestion` with these exact tab labels:
- Tab "Experience" — "What kind of experience do you most want players to have?" (Challenge & Mastery / Story & Discovery / Expression & Creativity / Relaxation & Flow)
- Tab "Timeline" — "What's your realistic development timeline?" (Weeks / Months / 1-2 years / Multi-year)
- Tab "Dev level" — "Where are you in your dev journey?" (First game / Shipped before / Professional background)

Use exactly these tab names — do not rename or duplicate them.

**Synthesize** into a **Creative Brief** — 3-5 sentence summary of emotional goals, taste profile, and constraints. Read back, confirm it captures intent.

---

### Phase 2: Concept Generation

Using the creative brief as a foundation, generate **3 distinct concepts**
that each take a different creative direction. Use these ideation techniques:

**Technique 1: Verb-First Design**
Start with the core player verb (build, fight, explore, solve, survive,
create, manage, discover) and build outward from there. The verb IS the game.

**Technique 2: Mashup Method**
Combine two unexpected elements: [Genre A] + [Theme B]. The tension between
the two creates the unique hook. (e.g., "farming sim + cosmic horror",
"roguelike + dating sim", "city builder + real-time combat")

**Technique 3: Experience-First Design (MDA Backward)**
Start from the desired player emotion (aesthetic goal from MDA framework:
sensation, fantasy, narrative, challenge, fellowship, discovery, expression,
submission) and work backward to the dynamics and mechanics that produce it.

For each concept, present:
- **Working Title**
- **Elevator Pitch** (1-2 sentences — must pass the "10-second test")
- **Core Verb** (the single most common player action)
- **Core Fantasy** (the emotional promise)
- **Unique Hook** (passes the "and also" test: "Like X, AND ALSO Y")
- **Primary MDA Aesthetic** (which emotion dominates?)
- **Estimated Scope** (small / medium / large)
- **Why It Could Work** (1 sentence on market/audience fit)
- **Biggest Risk** (1 sentence on the hardest unanswered question)

Present all three. Then use `AskUserQuestion` to capture the selection.

**CRITICAL**: This MUST be a plain list call — no tabs, no form fields. Use exactly this structure:

```
AskUserQuestion(
  prompt: "Which concept resonates with you? You can pick one, combine elements, or ask for fresh directions.",
  options: [
    "Concept 1 — [Title]",
    "Concept 2 — [Title]",
    "Concept 3 — [Title]",
    "Combine elements across concepts",
    "Generate fresh directions"
  ]
)
```

Do NOT use a `tabs` field here. The `tabs` form is for multi-field input only — using it here causes an "Invalid tool parameters" error. This is a plain `prompt` + `options` call.

Never pressure toward a choice — let them sit with it.

---

### Phase 3: Core Loop Design

For the chosen concept, use structured questioning to build the core loop — if it isn't fun in isolation, no content or polish will save the game.

**30-Second Loop** (moment-to-moment):

Ask these as `AskUserQuestion` calls — derive the options from the chosen concept, don't hardcode them:

1. **Core action feel** — prompt: "What's the primary feel of the core action?" Generate 3-4 options that fit the concept's genre and tone, plus a free-text escape (`I'll describe it`).

2. **Key design dimension** — identify the most important design variable for this specific concept (e.g., world reactivity, pacing, player agency) and ask about it. Generate options that match the concept. Always include a free-text escape.

After capturing answers, analyze: Is this action intrinsically satisfying? What makes it feel good? (Audio feedback, visual juice, timing satisfaction, tactical depth?)

**5-Minute Loop** (short-term goals):
- What structures the moment-to-moment play into cycles?
- Where does "one more turn" / "one more run" psychology kick in?
- What choices does the player make at this level?

**Session Loop** (30-120 minutes):
- What does a complete session look like?
- Where are the natural stopping points?
- What's the "hook" that makes them think about the game when not playing?

**Progression Loop** (days/weeks):
- How does the player grow? (Power? Knowledge? Options? Story?)
- What's the long-term goal? When is the game "done"?

**Player Motivation Analysis** (based on Self-Determination Theory):
- **Autonomy**: How much meaningful choice does the player have?
- **Competence**: How does the player feel their skill growing?
- **Relatedness**: How does the player feel connected (to characters,
  other players, or the world)?

---

### Phase 4: Pillars and Boundaries

Game pillars keep all decisions pointing same direction. Prevent scope creep, keep vision sharp.

Collaboratively define **3-5 pillars**:
- Each pillar has a **name** and **one-sentence definition**
- Each pillar has a **design test**: "If we're debating between X and Y,
  this pillar says we choose __"
- Pillars should feel like they create tension with each other — if all
  pillars point the same way, they're not doing enough work

Then define **3+ anti-pillars** (what this game is NOT):
- Anti-pillars prevent the most common form of scope creep: "wouldn't it
  be cool if..." features that don't serve the core vision
- Frame as: "We will NOT do [thing] because it would compromise [pillar]"

**Pillar confirmation**: After presenting the full pillar set, use `AskUserQuestion`:
- Prompt: "Do these pillars feel right for your game?"
- Options: `[A] Lock these in` / `[B] Rename or reframe one` / `[C] Swap a pillar out` / `[D] Something else`

If the user selects B, C, or D, make the revision, then use `AskUserQuestion` again:
- Prompt: "Pillars updated. Ready to lock these in?"
- Options: `[A] Lock these in` / `[B] Revise another pillar` / `[C] Something else`

Repeat until the user selects [A] Lock these in.

**After pillars and anti-pillars are agreed, spawn BOTH `creative-director` AND `art-director` via Task in parallel before moving to Phase 5. Issue both Task calls simultaneously — do not wait for one before starting the other.**

- **`creative-director`** — gate **CD-PILLARS** (`.ags/rules/director-gates.md`)
  Pass: full pillar set with design tests, anti-pillars, core fantasy, unique hook.

- **`art-director`** — gate **AD-CONCEPT-VISUAL** (`.ags/rules/director-gates.md`)
  Pass: game concept elevator pitch, full pillar set with design tests, target platform (if known), any reference games or visual touchstones the user mentioned.

Collect both verdicts, then present them together using a two-tab `AskUserQuestion`:
- Tab **"Pillars"**: present creative-director feedback. Options mirror the standard CD-PILLARS handling — `Lock in as-is` / `Revise [specific pillar]` / `Discuss further`.
- Tab **"Visual anchor"**: present the art-director's 2-3 named visual direction options. Options: each named direction (one per option) + `Combine elements across directions` + `Describe my own direction`.

The user's selected visual anchor (the named direction or their custom description) is stored as the **Visual Identity Anchor** — it will be written into the game-concept document and becomes the foundation of the art bible.

If the creative-director returns CONCERNS or REJECT on pillars, resolve pillar issues before asking for the visual anchor selection — visual direction should flow from confirmed pillars.

---

### Phase 5: Player Type Validation

Using Bartle taxonomy and Quantic Foundry motivation model, validate who this game is for:

- **Primary player type**: Who will LOVE this game? (Achievers, Explorers, Socializers, Competitors, Creators, Storytellers)
- **Secondary appeal**: Who else might enjoy it?
- **Who is this NOT for**: Being clear about who won't like it is as important as knowing who will
- **Market validation**: Successful games serving similar player type? Audience size?

---

### Phase 6: Scope and Feasibility

Ground the concept in reality:

- **Target platform**: Use `AskUserQuestion` — "What platforms are you targeting for this game?"
  Options: `PC (Steam / Epic)` / `Mobile (iOS / Android)` / `Console` / `Web / Browser` / `Multiple platforms`
  Record the answer — it will be passed to `/ags-setup-engine` so the Unity build profile and performance budgets are configured for the right platform.

- **Engine note**: This studio currently supports Unity only. The user does not need to choose an engine here — `/ags-setup-engine` will pin the Unity version after brainstorming.
- **Art pipeline**: What's the art style and how labor-intensive is it?
- **Content scope**: Estimate level/area count, item count, gameplay hours
- **MVP definition**: What's the absolute minimum build that tests "is the
  core loop fun?"
- **Biggest risks**: Technical risks, design risks, market risks
- **Scope tiers**: What's the full vision vs. what ships if time runs out?

**After identifying biggest technical risks, spawn `technical-director` via Task using gate TD-FEASIBILITY (`.ags/rules/director-gates.md`) before scope tiers are defined.**

Pass: core loop description, platform target, engine choice (or "undecided"), list of identified technical risks.

Present the assessment to the user. If HIGH RISK, offer to revisit scope before finalising. If CONCERNS, note them and continue.

**After scope tiers are defined, spawn `producer` via Task using gate PR-SCOPE (`.ags/rules/director-gates.md`).**

Pass: full vision scope, MVP definition, timeline estimate, team size.

Present the assessment to the user. If UNREALISTIC, offer to adjust the MVP definition or scope tiers before writing the document.

---

4. **Generate game concept document** using `.ags/templates/t_concept.md`. Fill ALL sections from brainstorm, including MDA analysis, player motivation profile, and flow state design sections.

   **Include Visual Identity Anchor section** with:
   - Selected visual direction name
   - One-line visual rule
   - 2-3 supporting visual principles with design tests
   - Color philosophy summary

   Seed of the art bible — captures the core visual decision before it's forgotten.

### 4a. Internal Review Loop

Before write approval, run a final consolidated internal review on the assembled concept draft. Spawn `creative-director` (gate **CD-PILLARS**), `art-director` (gate **AD-CONCEPT-VISUAL**), `technical-director` (gate **TD-FEASIBILITY**), `producer` (gate **PR-SCOPE**) in parallel via Task. Pass the full drafted document.

**Loop exit condition.** Single iteration in which every spawned director returns READY (no critical/high/medium findings). No iteration cap. Non-clean → consolidate findings, ask user to revise the relevant sections, re-spawn the same panel.

Record iteration count.

### 4b. External Review Gate (user confirm)

After the internal loop is CLEAN, ask via `AskUserQuestion`:

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

5. Use `AskUserQuestion` for write approval:
- Prompt: "Game concept is ready. May I write it to `design/gdd/game-concept.md`?"
- Options: `[A] Yes — write it` / `[B] Not yet — revise a section first`

If [B]: ask which section to revise using `AskUserQuestion` with options: `Elevator Pitch` / `Core Fantasy & Unique Hook` / `Pillars` / `Core Loop` / `MVP Definition` / `Scope Tiers` / `Risks` / `Something else — I'll describe`

After revising, show the updated section as a diff or clear before/after, then use `AskUserQuestion` — "Ready to write the updated concept document?"
Options: `[A] Yes — write it` / `[B] Revise another section`
Repeat until the user selects [A].

If yes, generate the document using the template at `.ags/templates/t_concept.md`, fill in ALL sections from the brainstorm conversation, and write the file, creating directories as needed.

**Scope consistency rule**: The "Estimated Scope" field in the Core Identity table must match the full-vision timeline from the Scope Tiers section — not just say "Large (9+ months)". Write it as "Large (X–Y months, solo)" or "Large (X–Y months, team of N)" so the summary table is accurate.

6. **Suggest next steps** (in order — professional studio pre-production pipeline). List ALL — do not abbreviate:
   1. `/ags-setup-engine` — configure engine, populate version-aware reference docs
   2. `/ags-art-bible` — visual identity spec. Do BEFORE writing GDDs. Gates asset production, shapes architecture (rendering, VFX, UI).
   3. `/ags-design-review design/gdd/game-concept.md` — validate concept completeness before going downstream
   4. `creative-director` — discuss vision, pillar refinement
   5. `/ags-map-systems` — decompose concept into systems; maps dependencies, priorities, systems index
   6. `/ags-design-system` — author per-system GDDs in dependency order
   7. `/ags-create-architecture` — master architecture blueprint + Required ADR list
   8. `/ags-architecture-decision (×N)` — one ADR per decision in Required ADR list
   9. `/ags-gate-check` — phase gate before committing to production
   10. `/ags-playtest-report` — validate core hypothesis once vertical slice playable
   11. `/ags-create-epics new` — plan first sprint if validated

7. **Output a summary** with the chosen concept's elevator pitch, pillars,
   primary player type, engine recommendation, biggest risk, and file path.

Verdict: **COMPLETE** — game concept created and handed off for next steps.

---

## Context Window Awareness

If context ≥70% during any phase, append to current response:

> **Context is approaching the limit (≥70%).** Game concept saved to `design/gdd/game-concept.md`. Open fresh Claude Code session to continue — progress not lost.

---

## Recommended Next Steps

Pre-production pipeline order:
1. `/ags-setup-engine` — configure engine, populate version-aware reference docs
2. `/ags-art-bible` — visual identity before any GDDs
3. `/ags-map-systems` — decompose concept into systems with dependencies
4. `/ags-design-system [first-system]` — author per-system GDDs in dependency order
5. `/ags-create-architecture` — master architecture blueprint
6. `/ags-gate-check pre-production` — validate readiness before production
