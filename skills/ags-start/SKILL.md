---
name: ags-start
description: "Entry point for AGS. Inspects the repository, determines the current project state, and routes to the appropriate skill."
---

# AGS Start

This skill is invoked automatically on every AGS startup, after AI provider availability is confirmed.
Its sole responsibility is to determine where the project currently stands and hand off to the correct skill.
It does not perform any project work itself.

## Step 1: Silent file check

Check whether each of the following files exists. Do not read or analyse their contents yet.

- `gdd/game-concept.md`
- `gdd/engine-decision.md`
- `.ags/project-state.md`

Record which files are present and which are absent.

## Step 2: Determine project state

Apply the following rules in order:

1. If any of the three files from Step 1 is absent, the state is **initial**. Record which file(s) are missing.

2. If all three files are present, read `.ags/project-state.md` and compare its structure to `templates/project-state-template.md`:
   - The file must contain a Lifecycle section with a checklist whose items match the template exactly (same items, same order).
   - If the structure does not match, the state is **initial**. Record that the file format is invalid.
   - If the structure matches, the current stage is the first checklist item that is not marked `[x]`. The state is that stage name.

## Step 3: Route to the appropriate skill

**If the state is initial:**

Write one short sentence explaining why the state was determined to be initial (missing file name(s) or invalid format). Then invoke the `ags-onboard` skill.

**Otherwise:**

Invoke the `ags-main-menu` skill, passing the current stage as context.

## References

- [Project state template](../../templates/project-state-template.md)
- [Session workflow](../../rules/session-workflow.md)
- [Agent coordination](../../rules/agent-coordination.md)
