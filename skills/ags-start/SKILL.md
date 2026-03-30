---
name: ags-start
description: "Entry-point onboarding for AGS game projects. Use when beginning a new project or resuming a loosely defined one and you need to inspect the repository, converge on a documented project concept in gdd/game-concept.md, make an explicit engine decision in gdd/engine-decision.md, and recommend the first AGS sessions to create."
compatibility: "Designed for AGS repositories that use the local .ags/, gdd/, assets/, src/, and rules/ layout and allow local filesystem inspection."
---

# Start

This skill is the default first touchpoint for project work in AGS.
Its job is to turn an undefined or partially defined game project into:

- a confirmed conceptual vision in `gdd/game-concept.md`
- a confirmed engine decision in `gdd/engine-decision.md`
- a clear list of the first AGS sessions to run

The entry state only changes how much discovery work is needed.
It does not change the required end state.
Do not finish the skill until both foundation documents exist and have been reviewed with the user.

## Workflow

### 1. Inspect the repository silently

Before asking questions, gather just enough context to tailor the conversation.

Check:

- `.ags/config.json` if it exists
- `.ags/sessions/index.md` and any active or paused sessions
- markdown documents in `gdd/`
- implementation and asset roots such as `assets/` and `src/`
- signs of an existing engine choice

Use engine fingerprints such as:

- Unity: `Assets/`, `ProjectSettings/`, `Packages/manifest.json`
- Unreal: `*.uproject`, `Config/`, `Content/`, `Source/`
- Godot: `project.godot`, `.godot/`, `addons/`

Also note:

- whether the repository is empty, concept-only, prototype-only, or already in production
- whether there is a mismatch between the repository state and the user's self-assessment
- whether there is already an active session that should be resumed instead of starting a new one

Keep the findings concise and internal until they are useful in the conversation.
Do not dump a raw inventory of the repository.

### 2. Ask where the user is starting from

Open with a short onboarding prompt and give these four entry states:

> **A) No idea yet**  
> I want to make a game, but I do not have a usable concept yet.
>
> **B) Seed idea**  
> I have a theme, genre, mechanic, mood, or fantasy, but it is still vague.
>
> **C) Clear concept**  
> I already know the game I want to make, but it is not documented well enough yet.
>
> **D) Existing work**  
> I already have documents, prototype code, assets, or prior planning and want to continue from there.

Wait for the user's answer before routing further.
Use the answer only to choose the shortest path to the required outputs.

### 3. Route based on the answer

#### If A: No idea yet

Ask a few focused prompts:

- what kinds of games they enjoy making or playing
- what player fantasy or emotional tone sounds appealing
- whether there are hard constraints such as team size, timeline, target platform, or genre limits

Then:

- summarize the early direction in plain language
- capture it as a first-draft concept note
- continue into engine selection after the concept is coherent enough to support a real decision
- do not stop at brainstorming; converge to both required documents

#### If B: Seed idea

Ask the user to describe the idea in one to three sentences.
Extract and confirm:

- genre
- player fantasy
- core action or loop hypothesis
- target platform if known
- scope assumptions and obvious risks

Then:

- turn the seed into a concrete concept summary
- identify what is known versus still assumed
- ask whether there is already an engine preference
- continue until both required documents can be written and confirmed

#### If C: Clear concept

Ask for the minimum missing information:

- one-sentence pitch
- genre and core mechanic
- target platform
- expected scope
- engine preference or uncertainty

Then:

- summarize the concept back to the user for confirmation
- document the concept cleanly instead of re-brainstorming it
- move quickly to engine confirmation
- continue until both required documents are complete

#### If D: Existing work

Now surface the findings from the silent scan.
State only what matters, for example:

- existing engine markers
- design docs already present in `gdd/`
- source or prototype files already present
- active or paused sessions already tracked in `.ags/sessions/`

Then determine what is missing:

- missing concept documentation
- missing engine decision record
- missing prototype validation
- missing session structure
- reconstruct or normalize the concept if needed
- force an explicit engine decision if the project still does not have one
- continue until both required documents are complete

### 4. Produce the mandatory foundation documents

Regardless of the starting route, the skill is not complete until these two files exist, are updated if needed, and are confirmed with the user:

- `gdd/game-concept.md`
- `gdd/engine-decision.md`

`gdd/game-concept.md` should capture:

- project pitch
- genre
- player fantasy
- core loop hypothesis
- target platform
- target scope
- creative references or intended feel
- known risks
- open questions

`gdd/engine-decision.md` should capture:

- selected engine
- version if known
- why it was chosen
- constraints that matter to the choice
- alternatives considered
- consequences and follow-up technical risks

If the user is undecided about the engine:

- identify the smallest set of decision-driving factors
- make a recommendation with a brief rationale
- ask the user to choose
- do not finish the skill while the engine remains unresolved

If stronger or more specific documents already exist in `gdd/`, update those instead of creating duplicates, but still ensure these two canonical files either exist directly or clearly point to the authoritative equivalent.

Do not invent new engine fields in `.ags/config.json` unless the AGS config schema is explicitly extended to support them.
The current AGS config is runtime configuration, not the source of truth for engine selection.

After drafting or updating both documents:

- summarize the concept and engine choice back to the user
- apply corrections if requested
- only then move to starter-session recommendations

### 5. Recommend the first AGS sessions

Recommend two to four sessions, not a giant backlog.
Optimize for momentum and uncertainty reduction.
Base the sessions on the now-confirmed concept and engine decision.

Use this order by default:

1. concept stabilization
2. engine confirmation
3. smallest playable prototype
4. first production planning session

When proposing sessions:

- use the workflow in `rules/session-workflow.md`
- choose responsible agents using `rules/agent-coordination.md`
- keep each session narrow enough to lead to one approved scope and one execution plan
- do not mix unrelated goals into the same first session

If an active session already exists and clearly matches the user's intent, recommend resuming it before creating a new one.

Use patterns such as:

- `concept-foundation` or `concept-alignment` when the concept still needs one final refinement pass
- `technical-foundation` or `engine-setup` once the engine decision is locked
- `core-loop-prototype` for the smallest playable proof of the game
- `production-bootstrap` for the first properly scoped implementation session after the prototype direction is clear

### 6. Completion Rule

The skill is complete only when all of the following are true:

- `gdd/game-concept.md` exists and represents the agreed conceptual vision
- `gdd/engine-decision.md` exists and records an explicit engine choice
- the user has seen the resulting concept and engine summary
- the first AGS sessions have been recommended

If any of those items is still missing, continue the onboarding flow instead of stopping early.
After presenting the recommended sessions, ask which one to start first.
Do not auto-start a session without the user's confirmation.
Once the user chooses, create the session following `rules/session-workflow.md`.

## Edge Cases

- If multiple engine fingerprints are present, treat the project as ambiguous and resolve that before planning implementation.
- If the user says the project is new but the repository already contains code or sessions, mention the mismatch and ask whether to continue, resume, or reset direction.
- If the user wants to choose an engine before the concept is clear, allow it, but keep refining the concept afterward until `gdd/game-concept.md` is still strong enough to support the decision.
- If the user already has an active session in progress, prefer resuming it unless they explicitly want a separate new session.
- If the repository has implementation but no usable `gdd/` documents, prioritize concept reconstruction before new feature work.

## References

- [Session workflow](../../rules/session-workflow.md)
- [Agent coordination](../../rules/agent-coordination.md)
- [Directory structure](../../rules/directory-structure.md)
