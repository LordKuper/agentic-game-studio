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
│   ├── docs/                          # Technical docs, version-pinned engine API snapshots, examples
│   ├── project/                       # Project-state working dir (gitignored except patterns below)
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
└── <engine project>/                  # Engine source root, e.g. `Assets/` for Unity
```

## Notes

- **Production code**: engine source root (e.g. `Assets/` for Unity). Tests in `tests/` regardless of engine.
- **Active session**: single file `.ags/project/state.md` holds entire working session. One active session at a time. New task overwrites it. History in git.
- **Project state files** (`stage.txt`, `review-mode.md`, `sprint-status.yaml`) live under `.ags/project/`. Persist across `state.md` overwrites.
- **`.ags/project/`** mostly working state — keep gitignored unless project explicitly tracks sprint/epic history in git.
