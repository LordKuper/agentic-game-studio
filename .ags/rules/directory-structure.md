# Directory Structure

```text
/
├── CLAUDE.md                          # Master configuration
├── .claude/                           # Agent definitions, skills, hooks
│   ├── agents/                        # Agent role specifications
│   ├── skills/                        # Slash-command skill definitions
│   ├── hooks/                         # Hook scripts (.sh)
│   ├── hooks-reference/               # Hook reference docs
│   └── settings.json                  # Project Claude Code settings
├── .ags/                              # Studio workflow: rules, templates, project state
│   ├── docs/                          # Technical documentation, curated engine API snapshots (version-pinned), examples
│   ├── project/                       # Project-state working directory (gitignored except patterns below)
│   │   ├── state.md                   # Active session state (single file, overwritten on new task)
│   │   ├── stage.txt                  # Current development phase
│   │   ├── review-mode.md             # Director-gate intensity (full / lean / solo)
│   │   ├── sprint-status.yaml         # Sprint snapshot
│   │   ├── sprints/                   # Sprint plans
│   │   ├── epics/                     # Epic definitions and story files
│   │   ├── milestones/                # Milestone definitions
│   │   ├── playtests/                 # Playtest reports
│   │   ├── bugs/                      # Bug reports
│   │   ├── qa/                        # QA plans
│   │   ├── risks/                     # Risk register entries
│   │   └── release/                   # Release-readiness artifacts
│   ├── rules/                         # Agent rules (this file lives here)
│   └── templates/                     # Document templates
├── assets/                            # Game assets (art, audio, vfx, shaders, data, scripts)
├── design/                            # Game design documents
│   ├── architecture/                  # ADRs, control manifest, master architecture doc
│   ├── gdd/                           # GDD root: engine.md, game-concept.md, systems-index.md, per-system GDDs, narrative, levels, balance
│   ├── art/                           # Art bible, asset specs, character profiles
│   ├── ux/                            # UX specs, HUD designs, interaction patterns
│   ├── narrative/                     # Lore, character sheets, dialogue specs
│   ├── accessibility-requirements.md  # Accessibility tier and feature matrix
│   └── registry/                      # Cross-document entity registry
├── tests/                             # Test code (engine-agnostic location)
└── <engine project>/                  # Engine-specific source root, e.g. `Assets/` for Unity
```

## Notes

- **Production code** lives in the engine's source root (e.g. `Assets/` for Unity).
  Tests live in `tests/` regardless of engine.
- **Active session**: a single file `.ags/project/state.md` holds the entire
  working session. Only one active session at a time. Starting a new task
  overwrites it. History lives in git.
- **Project state files** (`stage.txt`, `review-mode.md  `, `sprint-status.yaml`)
  live directly under `.ags/project/` and persist across `state.md` overwrites.
- **`.ags/project/`** subtree is mostly working state — keep it gitignored
  unless the project has explicit reasons to track sprint/epic history in git.
