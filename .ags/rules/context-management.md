# Context Management

## File-Backed State (Primary Strategy)
**File is memory, not conversation.**

### Active session state
Single file holds entire active session:

- `.ags/project/state.md` — sole working-state file. Created by `/ags-start` if missing. Overwritten on new task.

**One active session at a time.** No slugs, archive, pointer files. New task overwrites `state.md`. History in git.

`state.md` content free-form, shaped by current task. Typical sections: current task, progress checklist, key decisions, files in progress, open questions. Update after each significant milestone (section approved, ADR accepted, story closed).

**How skills resolve state:**
1. Read `.ags/project/state.md`.
2. If missing — prompt user to run `/ags-start`.

### Project-level state files
Persist across overwrites of `state.md`. All Markdown:

- `.ags/project/stage.md` — current dev phase (concept, production, polish, release) + active epic ID + transition history
- `.ags/project/review-mode.md` — director-gate intensity (full / lean / solo)
- `.ags/project/stubs.md` — TODO stub registry across epics
- `.ags/project/decisions-log.md` — append-only decisions chronology
- `.ags/project/epics/index.md` — registry of all epics with status
- `.ags/project/epics/[slug]/EPIC.md` — active or recent epic (vertical slice definition)

### Incremental File Writing
For multi-section docs (design docs, architecture docs, lore entries):
1. Create file immediately with skeleton (all section headers, empty bodies).
2. Discuss and draft one section at a time.
3. Write each section to file as soon as approved.
4. Update `.ags/project/state.md` after each section.
5. After section written, prior discussion safe to compact — decisions in file.

## Proactive Compaction
- **Compact proactively** at ~60-70% usage, not at limit.
- **Use `/clear`** between unrelated tasks, or after 2+ failed correction attempts.
- **Natural compaction points**: after writing section to file, after committing, after task complete, before new topic.
- **Focused compaction**: `/compact Focus on [current task] — sections 1-3 written, working on section 4`.

## Subagent Delegation
Use subagents for research and exploration. Keep main session clean. Subagents run in own context window, return only summaries:

- **Use subagents** when investigating across multiple files, exploring unfamiliar code, or research >5k tokens of file reads.
- **Use direct reads** when you know exact 1-2 files to check.
- Subagents do not inherit conversation history — provide full context in prompt.

## Compaction Instructions
On compact, preserve in summary:
- Path of active state file (`.ags/project/state.md`)
- Files modified this session and purpose
- Architectural decisions and rationale
- Active epic ID, scoped systems, story status, open stubs
- Agent invocations and outcomes (success/failure/blocked)
- Test results (pass/fail counts, specific failures)
- Unresolved blockers or questions awaiting user input
- Current task and step
- Which sections of current doc written to file vs. in progress

**After compaction**: read `.ags/project/state.md` and any active files to recover context. Files contain decisions; conversation is secondary.

## Recovery After Session Crash
If session dies or new conversation continues work:
1. `session-start.sh` hook prints preview of `.ags/project/state.md` automatically.
2. Read `.ags/project/state.md` for full session context.
3. Read `.ags/project/stage.md` for phase + active epic.
4. Read active `.ags/project/epics/[slug]/EPIC.md` for epic state, stories, contracts, stubs.
5. Read `.ags/project/stubs.md` if open stubs are relevant.
6. Continue from next incomplete section, story, or task.

If `state.md` missing, run `/ags-start` to bootstrap.
