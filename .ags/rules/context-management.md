# Context Management

## File-Backed State (Primary Strategy)
**The file is the memory, not the conversation.**

### Active session state
Single file holds the entire active session:

- `.ags/project/state.md` — the one and only working-state file. Created by
  `/ags-start` if missing, overwritten when a new task begins.

There is **one active session at a time**. No slugs, no archive, no pointer
files. Switching to a new task overwrites `state.md`. History lives in git.

`state.md` content is free-form, shaped by the current task. Typical sections:
current task, progress checklist, key decisions, files in progress, open
questions. Update after each significant milestone (section approved, ADR
accepted, story closed).

**How skills resolve state:**
1. Read `.ags/project/state.md`.
2. If missing — prompt the user to run `/ags-start`.

### Project-level state files
Persist across overwrites of `state.md`:

- `.ags/project/stage.txt` — current development phase (concept,
  systems-design, technical-setup, …)
- `.ags/project/review-mode.md` — director-gate intensity (full / lean / solo)
- `.ags/project/sprint-status.yaml` — current sprint snapshot

### Incremental File Writing
When creating multi-section documents (design docs, architecture docs, lore entries):
1. Create the file immediately with a skeleton (all section headers, empty bodies)
2. Discuss and draft one section at a time in conversation
3. Write each section to the file as soon as it's approved
4. Update `.ags/project/state.md` after each section
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
- Path of the active state file (`.ags/project/state.md`)
- List of files modified in this session and their purpose
- Any architectural decisions made and their rationale
- Active sprint tasks and their current status
- Agent invocations and their outcomes (success/failure/blocked)
- Test results (pass/fail counts, specific failures)
- Unresolved blockers or questions awaiting user input
- The current task and what step we are on
- Which sections of the current document are written to file vs. still in progress

**After compaction:** read `.ags/project/state.md` and any files being actively
worked on to recover full context. The files contain the decisions; the
conversation history is secondary.

## Recovery After Session Crash
If a session dies or you start a new Claude conversation to continue work:
1. The `session-start.sh` hook prints a preview of `.ags/project/state.md`
   automatically.
2. Read `.ags/project/state.md` for full context.
3. Read the partially-completed file(s) listed in state.
4. Continue from the next incomplete section or task.

If `state.md` is missing, run `/ags-start` to bootstrap a new one.
