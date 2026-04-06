---
name: ags-main-menu
description: "Main menu for an established AGS project. Reads project context, displays the project header and open sessions, and routes to the appropriate session action based on CEO choice."
---

# AGS Main Menu

This skill is invoked by `ags-start` when the project state is valid and a current stage has been determined.
It presents the project status and available actions to the CEO, then executes the chosen action.

## Step 1: Read project context

Read the following files:

- `.ags/project-state.md` — to extract the current stage (first unchecked item in the Lifecycle checklist)
- `gdd/game-concept.md` — to extract the project name
- `.ags/sessions/index.md` — to get the list of sessions; if the file does not exist, treat the session list as empty

## Step 2: Display the main menu

Output a header in this format:

```
<Project Name> (<Current Stage>)
```

Then list all sessions whose status is one of: `scoping`, `scope-approved`, `planning`, `plan-approved`, `in-progress`, `paused`.
For each such session show: session title and status.

Then present the available actions:

- One **Resume** option for each open session listed above (if any)
- **Start a new session**

Wait for the CEO to choose before proceeding.

## Step 3: Execute the chosen action

**Resume [session]:**
Follow the Session Switch Protocol defined in `rules/session-workflow.md`.
Load the target session's `state.md` and resume from the recorded `next-step`.

**Start a new session:**
Follow the Scoping Protocol defined in `rules/session-workflow.md`.

## References

- [Session workflow](../../rules/session-workflow.md)
- [Agent coordination](../../rules/agent-coordination.md)
- [Project state template](../../templates/project-state-template.md)
