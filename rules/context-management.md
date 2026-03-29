# Context Management

This document defines how agentic AI tools in this repository must manage working
context: isolation, budgets, compaction, recovery, and subagent discipline.

For the session and task lifecycle, planning protocol, and execution order see
`rules/session-workflow.md`.

---

## Goals

- Keep the active context relevant to one task only.
- Persist state outside the chat so work survives compaction, crashes, and new
  sessions.
- Prevent unrelated tasks from sharing assumptions, file sets, or acceptance
  criteria.
- Make recovery deterministic for any agent that resumes the work later.

---

## Core Principles

- The file is the durable memory; the conversation is not.
- One active task per agent context.
- Unrelated work requires a checkpoint and a context reset before it begins.
- Read the minimum surface area needed for the current step.
- Write durable summaries immediately after meaningful decisions.
- Recover from written state first and chat history second.

---

## Canonical Context Artifacts

The repository-local structure for durable context is defined in
`rules/session-workflow.md`. The key paths are:

- `.ags/sessions/<session-id>/state.md` — session metadata and current status
- `.ags/sessions/<session-id>/session-scope.md` — approved detailed scope
- `.ags/sessions/<session-id>/execution-plan.md` — approved execution plan
- `.ags/sessions/<session-id>/tasks/<yyyy-mm-dd>-<task-slug>.md` — task briefs
- `.ags/sessions/<session-id>/archive/` — completed-task snapshots

Do not reuse a single task file for multiple unrelated tasks.

`state.md` is the single source of truth for the currently active session.
Each file under `tasks/` is a durable task brief and running log for one task.

---

## Task Identity and Isolation

Work belongs in the current context only if all of the following are true:

1. It serves the same deliverable or the same acceptance criteria.
2. It touches the same subsystem, document, or tightly related file set.
3. It is owned by the same primary agent chain defined in
   `rules/agent-coordination.md`.
4. It can be resumed from the same task brief without importing new goals.

Treat the work as a new task if any condition above fails.

The only acceptable exceptions are:

- a small clarification that directly unblocks the current task
- a tiny fix discovered while editing the same files and required to complete
  the current task

A new feature request, unrelated bug, separate review, different domain
question, or side investigation with its own success criteria is always a new
context.

---

## Execution Rules

- Keep the active working set small. Read narrowly, then expand only when the
  current evidence is insufficient.
- Summarize large reads into the task brief instead of relying on long chat
  history.
- Update the task brief and `state.md` after each meaningful milestone.
- When drafting a multi-section document, create the skeleton first and write
  approved sections to disk incrementally.
- After two failed correction attempts, or when the reasoning loop starts
  repeating, checkpoint the task and reset the context before trying again.
- Do not carry speculative ideas from one task into another without validating
  them again against the new task brief.

---

## Context Budgets

Use relative limits because model context windows differ.

- Soft limit: when the session feels roughly 60 to 70 percent full, checkpoint
  and compact or reset.
- Light task: direct review or a small read set in one narrow area.
- Medium task: feature work or focused investigation within one subsystem.
- Heavy task: broad refactor, cross-domain research, or anything that needs a
  large read surface. Heavy tasks must be split into smaller task briefs or
  delegated into bounded subtasks.

Warning signs that the context is too broad:

- more than one clear deliverable is being discussed
- more than one unrelated subsystem is open
- the agent is re-reading the same background repeatedly
- assumptions from the previous task are influencing the current one

---

## Proactive Compaction and Reset

Use compaction or a fresh thread proactively, not only when the tool is close to
failure.

Natural checkpoint points:

- after a section is written to file
- after a design or architecture decision
- after an implementation milestone
- after tests or verification complete
- before switching topics
- before pausing work for later resumption

Tool guidance:

- In Claude Code, Codex or similar tools with compaction, compact around the
  current task only.
- In tools with a clear or reset command, use it between unrelated tasks.
- In tools without compaction, start a new thread or session and reload from
  `state.md` plus the task brief.

Never compact multiple unrelated tasks into the same summary.

---

## Multi-Agent and Subagent Rules

- Each subagent must receive one objective, explicit scope, and a bounded file
  set.
- Each subagent must have a clear output contract: summary, changed files,
  decisions, verification, and blockers.
- Do not send full conversation history when a short task brief and selected
  files are sufficient.
- Do not reuse a subagent that investigated Task A for Task B unless Task B is
  the same task.
- Parallel agents must have disjoint write ownership or one designated
  integrator.
- Keep only the returned summary in the main task brief. Raw exploration logs
  are temporary.
- If a task crosses domains, keep one primary task brief and create separate
  subtask briefs when the read or write surface becomes large.

---

## Compaction Summary Checklist

A compaction or handoff summary must include:

- current session ID and title
- pointer to `state.md` and the task brief
- modified files and why they matter
- decisions already made and their rationale
- verification or test results
- blockers or open questions
- exact next step
- what is complete and what is still in progress

---

## Recovery After Disruption

After a crash, prompt-too-long failure, or fresh session:

1. Read `state.md`.
2. Read the current task brief.
3. Read only the files listed as relevant or modified.
4. Resume from the recorded next step.
5. If the state is stale or ambiguous, repair the task brief first before
   continuing work.

Recovery must be driven by the files on disk, not by memory of the earlier
conversation.

---

## Anti-Patterns

The following are prohibited:

- one thread for multiple independent tasks
- one task brief shared by unrelated work items
- broad repository scans without a narrowed objective
- keeping old assumptions alive after a task switch
- mixing design, bug fixing, review, and release work in one active context
  unless they serve the same deliverable
- continuing in a polluted context when repeated corrections show the agent has
  lost the thread

---

## Compliance

These rules apply to Claude Code, Codex, and any other agentic AI used in this
repository. Vendor-specific features are allowed only if they preserve the same
task isolation, file-backed recovery, and explicit task-switch discipline
defined in this document.
