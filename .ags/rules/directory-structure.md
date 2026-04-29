# Directory Structure

```text
/
├── CLAUDE.md                    # Master configuration
├── .claude/                     # Agent definitions, skills, hooks
├── .ags/                        # Session workflow, rules, templates
│   └── project/                 # Project-related decisions, sessions/{name}.md (ephemeral, gitignored)
│   └── rules/                   # Agent rules
│   └── templates/               # Document templates
├── assets/                      # Game assets (art, audio, vfx, shaders, data, scripts)
├── design/                      # Game design documents
│   └── gdd/                     # GDD root: engine.md, concept.md, narrative, levels, balance
├── docs/                        # Technical documentation (architecture, api, postmortems)
│   └── engine-reference/        # Curated engine API snapshots (version-pinned)
```
