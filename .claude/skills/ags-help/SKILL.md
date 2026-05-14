---
name: ags-help
description: "Analyzes what is done and the users query and offers advice on what to do next. Use if user says what should I do next or what do I do now or I'm stuck or I don't know what to do"
argument-hint: "[optional: what you just finished, e.g. 'finished design-review' or 'stuck on ADRs']"
user-invocable: true
allowed-tools: Read, Glob, Grep, AskUserQuestion
context: |
  !echo "=== Live Project State ===" && echo "Stage:" && head -10 .ags/project/stage.md 2>/dev/null || echo 'not set' && echo "Epics index:" && head -5 .ags/project/epics/index.md 2>/dev/null || echo 'no epics yet' && echo "Session state:" && head -5 .ags/project/state.md 2>/dev/null || echo 'none'
model: haiku
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

# Studio Help — What Do I Do Next?

Read-only — reports findings, writes nothing.

Lightweight orientation. For full gap analysis, use `/ags-project-stage-detect`.

---

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `.ags/rules/workflow-catalog.yaml` | shipped with template | STOP. "Workflow catalog missing — broken install. Reinstall template." |
| `.ags/project/state.md` | /ags-start | "No active session. Run /ags-start to bootstrap." Then continue with degraded output (catalog-only). |

`stage.md`, `epics/index.md`, `EPIC.md`, `stubs.md` are optional — absence simply means earlier phase. Do not block.

---

## Step 1: Read the Catalog

Read `.ags/rules/workflow-catalog.yaml`. Authoritative list of phases, ordered steps, required/optional flags, artifact globs.

---

## Step 1b: Find Skills Not in the Catalog

Glob `.claude/skills/*/SKILL.md`. Extract `name:` from each frontmatter.

Compare against `command:` values in catalog. Skills not in catalog = **uncataloged** — usable but not phase-gated.

Show as footer in Step 7:

```
### Also installed (not in workflow)
- `/skill-name` — [description from SKILL.md frontmatter]
- `/skill-name` — [description]
```

Show only if at least one uncataloged skill exists. Limit to 10 most relevant for current phase (QA in production, team in polish, etc.).

---

## Step 2: Determine Current Phase

In order:

1. **Read `.ags/project/stage.md`** — if exists, authoritative. Parse the metadata table for `Phase` row. Accepted values: `concept`, `foundation`, `production`, `polish`, `release`. Also extract `Active Epic` value.

2. **If `stage.md` missing**, infer from artifacts (most-advanced wins):
   - `.ags/project/epics/*/EPIC.md` with Status `done` → `production` (or `polish` if all MVP epics done — needs manual confirmation)
   - `.ags/project/epics/index.md` exists → `production`
   - `design/architecture/architecture.md` exists → `foundation`
   - `design/gdd/systems-index.md` → `concept` (late)
   - `design/gdd/game-concept.md` → `concept`
   - Nothing → `concept`

---

## Step 3: Read Session Context

Read `.ags/project/state.md` if exists. Extract:
- Most recent work
- In-progress tasks/open questions
- Current epic/feature/task from STATUS block

Personalize output from this.

---

## Step 4: Check Step Completion for the Current Phase

For each step in current phase (from catalog):

### Artifact-based checks

If step has `artifact.glob`:
- Glob to verify files exist
- If `min_count` specified, verify count
- If `artifact.pattern` specified, Grep the matched file
- **Complete** = condition met
- **Incomplete** = missing or pattern not found

If step has `artifact.note` (no glob): mark **MANUAL** — ask user.

If step has no `artifact`: mark **UNKNOWN** — not trackable (e.g. repeatable work).

### Special case: production phase — read `epics/index.md` and active `EPIC.md`

When phase is `production`, check `.ags/project/epics/index.md` before glob-based story checks. Parse the table:

- Rows with Status `done` → completed epics
- Rows with Status `implementing` / `playtesting` → in progress
- Rows with Status `planned` / `designing` → upcoming

Then read the **active epic** from `stage.md` Active Epic field. Open `.ags/project/epics/[slug]/EPIC.md` and surface:

- Acceptance Criteria checked count
- Open stubs introduced by this epic (cross-ref `.ags/project/stubs.md`)
- Stories status from `stories/` folder

Index + active EPIC are authoritative for production progress. Skip glob check for `implement` and `story-done` catalog steps.

### Special case: `repeatable: true` (non-production)

Repeatable steps outside production (e.g. "System GDDs"): artifact tells if *any* work done, not finished. Label differently — show what detected, note may be ongoing.

---

## Step 5: Find Position and Identify Next Steps

From completion data, determine:

1. **Last confirmed complete step** — furthest completed required
2. **Current blocker** — first incomplete *required* step (do this next)
3. **Optional opportunities** — incomplete *optional* steps doable now
4. **Upcoming required** — required steps after blocker (show as "coming up")

If user provided argument (e.g. "just finished design-review"), advance past named step even if artifact ambiguous.

---

## Step 6: Check for In-Progress Work

If `state.md` shows active task/epic:
- Surface at top: "It looks like you were working on [X]"
- Suggest continue or confirm done

---

## Step 7: Present Output

Short, direct. Quick orientation, not a report.

```
## Where You Are: [Phase Label]

**In progress:** [from state.md, if any]

### ✓ Done
- [completed step name]
- [completed step name]

### → Next up (REQUIRED)
**[Step name]** — [description]
Command: `[/command]`

### ~ Also available (OPTIONAL)
- **[Step name]** — [description] → `/command`
- **[Step name]** — [description] → `/command`

### Coming up after that
- [Next required step name] (`/command`)
- [Next required step name] (`/command`)

---
Approaching **[next phase]** gate → run `/ags-gate-check` when ready.
```

**Formatting:**
- `✓` confirmed complete
- `→` current required next step (only one — first blocker)
- `~` optional steps available now
- Commands inline as backtick code
- Step with no command (e.g. "Implement Stories") — explain what to do
- MANUAL steps: ask "I can't tell if [step] is done — has it been completed?"

Verdict: **COMPLETE** — next steps identified.

---

## Step 8: Gate Warning (if close)

After current phase steps, check if approaching gate:
- All required complete or near-complete → add: "You're close to the **[Current] → [Next]** gate. Run `/ags-gate-check` when ready."
- Multiple required remaining → skip gate warning.

---

## Step 9: Escalation Paths

After recommendations, if user seems stuck/confused, add:

```
---
Need more detail?
- `/ags-project-stage-detect` — full gap analysis with all missing artifacts listed
- `/ags-gate-check` — formal readiness check for your next phase
- `/ags-start` — re-orient from scratch
```

Show only when user input suggests confusion ("I don't know", "stuck", "lost", "not sure"). Don't show for simple "what's next?".

---

## Collaborative Protocol

- **Never auto-run next skill.** Recommend, let user invoke.
- **Ask about MANUAL steps** rather than assume.
- **Match tone** — stressed user ("totally lost") → reassuring, one action, not six.
- **One primary recommendation** — user leaves knowing exactly one thing to do. Optional and "coming up" are secondary.
