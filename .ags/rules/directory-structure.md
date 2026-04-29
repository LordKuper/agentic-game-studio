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
│   │   ├── sessions/                  # Per-session working files
│   │   │   ├── {slug}.md              # Active session checkpoints
│   │   │   ├── archived/              # Completed sessions
│   │   │   │   └── {slug}.md
│   │   │   └── .current               # One-line pointer file: slug of the active session
│   │   ├── stage.txt                  # Current development phase
│   │   ├── review-mode.txt            # Director-gate intensity (full / lean / solo)
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
- **Sessions**: every working session writes to `.ags/project/sessions/{slug}.md`.
  The active session slug is recorded in `.ags/project/sessions/.current`.
  Completed sessions are moved to `.ags/project/sessions/archived/`.
- **Project state files** (`stage.txt`, `review-mode.txt`, `sprint-status.yaml`)
  live directly under `.ags/project/` and persist across sessions.
- **`.ags/project/`** subtree is mostly working state — keep it gitignored
  unless the project has explicit reasons to track sprint/epic history in git.
