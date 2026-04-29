# Context Management

## File-Backed State (Primary Strategy)
**The file is the memory, not the conversation.**

### Sessions
Working state lives under `.ags/project/sessions/`:

- `.ags/project/sessions/{slug}.md` — one file per active working session.
  Slug chosen at `/ags-start` (e.g. `gdd-combat`, `arch-foundation`).
- `.ags/project/sessions/.current` — single-line pointer file containing the
  slug of the currently active session. Written by `/ags-start` on open or
  resume; updated when the session closes.
- `.ags/project/sessions/archived/{slug}.md` — completed sessions are moved
  here by `/ags-start` (or by the user) once the work is done.

A session file holds: current task, progress checklist, key decisions made,
files being worked on, open questions. Update it after each significant
milestone (section approved, ADR accepted, story closed).

**How skills resolve the active session:**
1. Read `.ags/project/sessions/.current` → get the slug.
2. Operate on `.ags/project/sessions/{slug}.md`.
3. If `.current` is missing or empty, prompt the user to run `/ags-start`.

### Project-level state files
Not session-scoped — shared across sessions:

- `.ags/project/stage.txt` — current development phase (concept,
  systems-design, technical-setup, …)
- `.ags/project/review-mode.txt` — director-gate intensity (full / lean / solo)
- `.ags/project/sprint-status.yaml` — current sprint snapshot

### Incremental File Writing
When creating multi-section documents (design docs, architecture docs, lore entries):
1. Create the file immediately with a skeleton (all section headers, empty bodies)
2. Discuss and draft one section at a time in conversation
3. Write each section to the file as soon as it's approved
4. Update the active session file after each section
5. After writing a section, previous discussion about that section can be safely
   compacted — the decisions are in the file

## Proactive Compaction
- **Compact proactively** at ~60-70% context usage, not reactively at the limit
- **Use `/clear`** between unrelated tasks, or after 2+ failed correction attempts
- **Natural compaction points:** after writing a section to file, after committing,
  after completing a task, before starting a new topic
- **Focused compaction:** `/compact Focus on [current task] — sections 1-3 are
  written to file, working on section 4`

## Subagent Delegation
Use subagents for research and exploration to keep the main session clean.
Subagents run in their own context window and return only summaries:

- **Use subagents** when investigating across multiple files, exploring unfamiliar code,
  or doing research that would consume >5k tokens of file reads
- **Use direct reads** when you know exactly which 1-2 files to check
- Subagents do not inherit conversation history — provide full context in the prompt

## Compaction Instructions
When context is compacted, preserve the following in the summary:
- Active session slug (from `.ags/project/sessions/.current`) and its session file path
- List of files modified in this session and their purpose
- Any architectural decisions made and their rationale
- Active sprint tasks and their current status
- Agent invocations and their outcomes (success/failure/blocked)
- Test results (pass/fail counts, specific failures)
- Unresolved blockers or questions awaiting user input
- The current task and what step we are on
- Which sections of the current document are written to file vs. still in progress

**After compaction:** read the active session file and any files being actively
worked on to recover full context. The files contain the decisions; the
conversation history is secondary.

## Recovery After Session Crash
If a session dies or you start a new Claude conversation to continue work:
1. The `session-start.sh` hook prints a preview of the active session and lists
   any unfinished sessions automatically.
2. Read `.ags/project/sessions/.current` to get the active slug.
3. Read `.ags/project/sessions/{slug}.md` for full context.
4. Read the partially-completed file(s) listed in the session.
5. Continue from the next incomplete section or task.

If `.current` is missing, run `/ags-start` to pick or resume a session.
