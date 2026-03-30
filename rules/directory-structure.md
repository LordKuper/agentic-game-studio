# Directory Structure

This document defines the target repository structure for the
`agentic-game-studio` project.

Its purpose is to keep project content, AGS operational data, source code, and
design documentation separated by responsibility.

---

## Goals

- Make file placement predictable.
- Separate AGS operational files from game content.
- Keep game assets grouped by production domain.
- Keep design documentation separate from implementation assets.

---

## Target Structure

```text
.ags/
  config.json
  agents/
  rules/
  sessions/
  skills/
assets/
  art/
  audio/
  data/
  scenes/
  scripts/
gdd/
```

---

## Directory Definitions

### `.ags/`

Stores files required for the operation of the `agentic-game-studio` (AGS)
application itself.

Allowed content:

- `config.json`
- AGS agent overrides and extensions
- AGS metadata
- AGS rule overrides and extensions
- AGS skill overrides and extensions
- AGS internal state
- other files required for AGS runtime workflows

This directory is not for game content, design documents, or gameplay source
files unless they are explicitly part of AGS internal operation.

### `.ags/rules/`

Stores overrides or extensions for the standard AGS operating rules.

Allowed content:

- repository-specific AGS rule overrides
- repository-specific AGS rule extensions
- additional AGS rule documents used by this project

### `.ags/skills/`

Stores overrides or extensions for the standard AGS skills.

Allowed content:

- repository-specific AGS skill overrides
- repository-specific AGS skill extensions
- project-local AGS skill definitions

### `.ags/agents/`

Stores overrides or extensions for the standard AGS agents.

Allowed content:

- repository-specific AGS agent overrides
- repository-specific AGS agent extensions
- project-local AGS agent definitions

### `.ags/sessions/`

Stores information about user work sessions with AGS.

Allowed content:

- session state
- session plans
- task logs
- other session-specific artifacts used by AGS workflows

### `assets/`

Stores all game assets.

Only game production content should live under this directory.

The exact physical placement of game assets may depend on the target game
engine and its technical constraints. However, regardless of engine-specific
conventions, the repository must preserve logical separation between asset
domains such as art, audio, data, scenes, and scripts.

### `assets/art/`

Stores visual art assets and related production files.

Examples:

- materials
- models
- shaders
- textures
- concept art
- VFX source assets

### `assets/audio/`

Stores audio assets.

Examples:

- music
- sound effects
- voice assets
- audio source files

### `assets/data/`

Stores structured game data and design-linked content used by the game.

Examples:

- game objects
- specifications
- localization
- configuration data
- balance tables
- other non-code runtime data

### `assets/scenes/`

Stores game scenes and scene-related files.

### `assets/scripts/`

Stores source code and UI implementation that belong to the game project.

Examples:

- gameplay code
- UI code
- UI assets tightly coupled to scripted implementation

### `gdd/`

Stores the game design document and all conceptual decision records.

Examples:

- narrative documents
- lore documents
- balance design notes
- feature concepts
- mechanic descriptions

---

## Placement Rules

- Every new repository file must be placed in the most specific directory that
  matches its responsibility.
- AGS operational files must stay under `.ags/`.
- AGS overrides and extensions for standard rules, skills, and agents must stay
  under `.ags/rules/`, `.ags/skills/`, and `.ags/agents/` respectively.
- Game assets must stay under `assets/`.
- Engine-specific asset layout decisions are allowed only if they preserve the
  same logical separation of asset responsibilities.
- Conceptual, narrative, and design decision documents must stay under `gdd/`.
- New subdirectories may be added inside the directories defined here if they
  preserve the same responsibility boundaries.
- Do not mix AGS operational data with game assets or design documentation.

---

## Source Of Truth

This document is the target structure reference for repository organization.
When new folders are introduced, they must remain consistent with the intent
defined here.
