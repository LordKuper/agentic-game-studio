# Session Workflow

This document defines the session and task lifecycle for all agentic AI tools in
this repository: how sessions are created, scoped, planned, executed, paused,
switched, and completed.

For rules on managing agent context, compaction, recovery, and isolation see
`rules/context-management.md`.

---

## Session Lifecycle

A session represents one coherent unit of work: implementing a game feature,
mechanic, concept, or other deliverable. Each session progresses through these
phases:

1. **scoping** — the user has given a brief topic; agents ask clarifying questions
   and the detailed scope is being formed through iterative Q&A.
2. **scope-approved** — the user has approved the detailed scope description.
   `session-scope.md` is finalized.
3. **planning** — agents collaborate to produce a detailed execution plan broken
   into atomic tasks.
4. **plan-approved** — the user has approved the execution plan.
   `execution-plan.md` is finalized.
5. **in-progress** — tasks from the approved plan are being executed sequentially.
6. **paused** — work is suspended; all context is persisted in `state.md` and the
   current task brief.
7. **completed** — every task in the plan is done or explicitly skipped; the
   session is closed.

Transition rules:

- `scoping → scope-approved` requires explicit user approval of the scope.
- `scope-approved → planning` happens automatically after scope approval.
- `planning → plan-approved` requires explicit user approval of the plan.
- `plan-approved → in-progress` happens automatically when the first task starts.
- `in-progress → paused` happens on session switch or when the user suspends work.
- `paused → in-progress` happens when the user resumes the session.
- `in-progress → completed` happens when all tasks are done or skipped.
- Any phase may transition to `completed` if the user explicitly closes the
  session.

---

## Session Identity and Naming

Each session has a unique `session-id` in the format:

```
<yyyy-mm-dd>-<slug>
```

where `<slug>` is a short kebab-case description of the feature or mechanic
(for example `2026-03-30-combat-system`). The date is the session creation date.

The session directory is `.ags/sessions/<session-id>/`.

---

## Multiple Active Sessions

Multiple sessions may exist in non-completed states simultaneously. Rules:

- Only one session may be active in a single agent context at any time.
- An index of all sessions is maintained at `.ags/sessions/index.md`.
- `index.md` is a table with columns: session-id, title, status, last-updated.
- When a new session is created, it must be registered in `index.md`.
- When a session changes status, `index.md` must be updated.

---

## Session Directory Structure

Each session stores its durable state in:

- `.ags/sessions/<session-id>/state.md` — session metadata and current status
- `.ags/sessions/<session-id>/session-scope.md` — approved detailed scope
- `.ags/sessions/<session-id>/execution-plan.md` — approved execution plan
- `.ags/sessions/<session-id>/tasks/<yyyy-mm-dd>-<task-slug>.md` — task briefs
- `.ags/sessions/<session-id>/archive/` — completed-task snapshots

---

## Scoping Protocol

The scoping phase transforms a brief user topic into an exhaustive, unambiguous
description of the session subject.

1. The user provides a brief description of the session topic (a mechanic,
   feature, concept, or other deliverable).
2. Create the session directory and `state.md` with status `scoping`.
3. Register the session in `.ags/sessions/index.md`.
4. Using `rules/agent-coordination.md`, identify the agents whose domains are
   relevant to designing the detailed scope (for example: `game-designer`,
   `systems-designer`, `narrative-director`, depending on the topic).
5. Record the selected scoping agents in `state.md`.
6. The selected agents collaboratively formulate a list of clarifying questions
   that the user must answer. Questions must be non-repeating and targeted at
   closing specific knowledge gaps about the topic.
7. Present the questions to the user.
8. The user answers the questions.
9. Based on the answers, evaluate whether gaps remain. If they do, formulate a
   new batch of non-repeating questions and repeat from step 7. Continue until
   the topic is fully covered with no remaining ambiguity within the session
   scope.
10. Compile all gathered information into a structured detailed scope description.
11. Present the scope description to the user for approval.
12. If the user requests corrections, apply them and re-present. Repeat until
    approved.
13. Write the approved scope to `session-scope.md` in the session directory.
14. Update `state.md` status to `scope-approved`.

Rules for the Q&A loop:

- Questions must not repeat across iterations.
- Questions must be specific and actionable, not open-ended or vague.
- Each iteration should narrow the remaining gaps, not expand scope.
- If the user explicitly says the scope is sufficient, stop asking even if the
  agents see remaining gaps — record those gaps as assumptions or out-of-scope
  items in `session-scope.md`.

---

## Planning Protocol

After the scope is approved, agents produce a detailed execution plan.

1. Using `rules/agent-coordination.md`, identify the agents whose domains are
   relevant to planning the implementation of the approved scope. These may
   differ from the scoping agents (for example: scoping may involve
   `game-designer` and `systems-designer`, while planning adds
   `lead-programmer` and `technical-artist`).
2. Record the selected planning agents in `state.md`.
3. The selected agents collaboratively produce a detailed, sequentially ordered
   plan broken into atomic tasks. Each task must have:
   - a sequential number
   - a task-slug (kebab-case, used as file name)
   - a short description of the deliverable
   - the expected deliverable type (code, document, asset, config, etc.)
   - the primary responsible agent
4. Write the draft plan to `execution-plan.md` in the session directory.
5. Present the plan to the user for review.
6. If the user requests corrections, apply them and re-present. Repeat until
   approved.
7. Update `state.md` status to `plan-approved`.

Plan quality requirements:

- Tasks must be atomic: each task has one clear deliverable and can be completed
  in a single focused work session.
- Tasks must be ordered by dependency: no task may depend on a later task.
- The plan must cover the entire approved scope — nothing from
  `session-scope.md` may be silently dropped.
- Each task must reference which part of the scope it addresses.

---

## Task Execution Order

Tasks from the approved plan are executed strictly sequentially:

- The next task does not start until the current task is completed and its brief
  is finalized.
- Parallel execution of tasks within a single session is prohibited.
- If a task is blocked, the agent must record the blocker in the task brief and
  in `state.md`, then either resolve the blocker or consult the user — not skip
  ahead to the next task without explicit user approval.
- Skipping a task requires explicit user approval and must be recorded in both
  the task brief (status `skipped`) and `execution-plan.md`.
- When a task is completed, its entry in `execution-plan.md` must be marked as
  `done`.

---

## Task Start Protocol

Before substantial work on a task starts:

1. Determine the primary owner using `rules/agent-coordination.md` and the
   responsible agent recorded in `execution-plan.md`.
2. Create the task brief file at
   `tasks/<yyyy-mm-dd>-<task-slug>.md` within the session directory.
3. Record the goal, scope, constraints, acceptance criteria, and initial
   relevant files.
4. Update `state.md`: set `current-task` to point to the new task brief.
5. Read only the files needed for the current step.
6. If the startup read set is already broad, split the task or delegate focused
   research instead of loading everything into one context.

An agent must not start implementation or long-form drafting before the task
goal and scope are written down.

---

## Required Contents of `state.md`

`state.md` must contain one session context only.

Required fields:

- **session-id** — unique identifier in `<yyyy-mm-dd>-<slug>` format
- **title** — human-readable session title
- **status** — one of: `scoping`, `scope-approved`, `planning`, `plan-approved`,
  `in-progress`, `paused`, `completed`
- **created** — session creation date
- **last-updated** — date of the last update to this file
- **scoping-agents** — agents involved in scope definition
- **planning-agents** — agents involved in execution planning
- **current-task** — pointer to the active task brief file (null if not in
  execution)
- **relevant-files** — key files for the session as a whole
- **decisions** — important session-level decisions and their rationale
- **next-step** — what to do when resuming this session

---

## Required Contents of `session-scope.md`

The approved scope document must contain:

- **session-id** — back-reference to the session
- **title** — the subject being described
- **overview** — high-level summary of the feature, mechanic, or concept
- **detailed-description** — exhaustive structured description compiled from
  the Q&A process
- **requirements** — specific functional and non-functional requirements
- **constraints** — technical, design, or other limitations
- **out-of-scope** — explicitly excluded items
- **assumptions** — things taken as given without user confirmation
- **references** — links to related documents, files, or external resources

---

## Required Contents of `execution-plan.md`

The approved plan must contain:

- **session-id** — back-reference to the session
- **scope-reference** — pointer to `session-scope.md`
- **plan** — ordered list of tasks, each with:
  - sequential number
  - status (`pending`, `in-progress`, `done`, `skipped`)
  - task-slug
  - short description
  - deliverable type
  - responsible agent
  - scope section addressed

---

## Required Contents of a Task Brief

Each task brief must contain:

- **task-id** — matches the file name (`<yyyy-mm-dd>-<task-slug>`)
- **session-id** — back-reference to the parent session
- **order** — sequential number in the execution plan
- **status** — one of: `pending`, `in-progress`, `done`, `skipped`
- **goal**
- **in-scope**
- **out-of-scope**
- **acceptance-criteria**
- **constraints**
- **relevant-files**
- **decisions**
- **assumptions**
- **open-questions**
- **checkpoints-or-milestones**
- **verification-or-test-results**
- **next-step**

The task brief is the durable memory for the task. Chat history is secondary.

---

## Task Switch Protocol (Within a Session)

When moving to the next task within the same session:

1. Finalize the current task brief: record decisions, modified files, verification
   results, and set status to `done`.
2. Mark the completed task as `done` in `execution-plan.md`.
3. Update `state.md`: advance `current-task` to the next task.
4. Compact or reset the context to shed the previous task's working set.
5. Create the new task brief and follow the Task Start Protocol.

---

## Session Switch Protocol

When switching between sessions:

1. Finalize or checkpoint the current task brief with decisions, modified files,
   blockers, and exact next step.
2. Update the current session's `state.md`: set status to `paused`, record
   `next-step`.
3. Update `.ags/sessions/index.md` with the new status.
4. Clear or compact the context, or start a new thread.
5. Load the target session's `state.md`.
6. Load the target session's current task brief.
7. Update the target session's status to `in-progress` in both `state.md` and
   `index.md`.
8. Resume from the recorded `next-step`.

If the user interrupts with a request belonging to a different session, this
protocol must be followed before work continues on the new request. If the
request does not belong to any existing session, treat it as a new session and
follow the Scoping Protocol.

---

## Anti-Patterns

The following are prohibited:

- starting planning before the scope is approved by the user
- starting task execution before the execution plan is approved by the user
- executing tasks out of order or in parallel within a single session
- skipping a task without explicit user approval
- asking duplicate questions across Q&A iterations during scoping
- silently dropping scope items when producing the execution plan
- working on two sessions in one agent context without following the Session
  Switch Protocol
- maintaining multiple active sessions without an up-to-date `index.md`
- starting implementation before the task goal and scope are written down

---

## Templates

### `index.md`

```markdown
# Sessions Index

| session-id | title | status | last-updated |
|---|---|---|---|
| 2026-03-30-combat-system | Combat System | in-progress | 2026-03-30 |
| 2026-03-28-inventory-ui | Inventory UI | paused | 2026-03-29 |
```

### `state.md`

```markdown
# Session: <title>

## Metadata

- **session-id:** <yyyy-mm-dd>-<slug>
- **title:** <human-readable title>
- **status:** scoping | scope-approved | planning | plan-approved | in-progress | paused | completed
- **created:** <yyyy-mm-dd>
- **last-updated:** <yyyy-mm-dd>

## Agents

- **scoping:** <agent-1>, <agent-2>
- **planning:** <agent-1>, <agent-2>, <agent-3>

## Current Task

→ `tasks/<yyyy-mm-dd>-<task-slug>.md`

## Relevant Files

- <path> — <why it matters>

## Decisions

- <decision> — <rationale>

## Next Step

<what to do when resuming this session>
```

### `session-scope.md`

```markdown
# Scope: <title>

## Metadata

- **session-id:** <yyyy-mm-dd>-<slug>

## Overview

<high-level summary>

## Detailed Description

<structured exhaustive description from Q&A>

## Requirements

- <requirement>

## Constraints

- <constraint>

## Out of Scope

- <item>

## Assumptions

- <assumption>

## References

- <link or path>
```

### `execution-plan.md`

```markdown
# Execution Plan: <title>

## Metadata

- **session-id:** <yyyy-mm-dd>-<slug>
- **scope:** session-scope.md

## Plan

| # | status | task-slug | description | deliverable | agent | scope section |
|---|---|---|---|---|---|---|
| 1 | pending | <slug> | <description> | code | <agent> | <section> |
| 2 | pending | <slug> | <description> | document | <agent> | <section> |
| 3 | pending | <slug> | <description> | asset | <agent> | <section> |
```

### Task brief (`tasks/<yyyy-mm-dd>-<task-slug>.md`)

```markdown
# Task: <short title>

## Metadata

- **task-id:** <yyyy-mm-dd>-<task-slug>
- **session-id:** <parent session-id>
- **order:** <number in execution plan>
- **status:** pending | in-progress | done | skipped

## Goal

<what this task must accomplish>

## In Scope

- <item>

## Out of Scope

- <item>

## Acceptance Criteria

- [ ] <criterion>

## Constraints

- <constraint>

## Relevant Files

- <path> — <why>

## Decisions

- <decision> — <rationale>

## Assumptions

- <assumption>

## Open Questions

- <question>

## Checkpoints

- [ ] <milestone>

## Verification / Test Results

<results of testing or verification>

## Next Step

<exact next action>
```

---

## Compliance

These rules apply to Claude Code, Codex, and any other agentic AI used in this
repository. Vendor-specific features are allowed only if they preserve the same
session lifecycle, task ordering, and file-backed persistence defined in this
document.
