# Engine Reference Documentation

Version-pinned engine docs. LLM cutoff stale; engines update fast. Without these, agents suggest dead code.

Studio supports Unity only. Docs under `.ags/docs/engine-reference/unity/`.

## Structure

Each engine own dir:

```
<engine>/
├── VERSION.md              # Pinned version, verification date, knowledge gap window
├── breaking-changes.md     # API changes between versions, organized by risk level
├── deprecated-apis.md      # "Don't use X → Use Y" lookup tables
├── current-best-practices.md  # New practices not in model training data
└── modules/                # Per-subsystem quick references (~150 lines max each)
    ├── rendering.md
    ├── physics.md
    └── ...
```

## How Agents Use

Engine-specialist agents:

1. Read `VERSION.md` — confirm version.
2. Check `deprecated-apis.md` before suggesting API.
3. Check `breaking-changes.md` for version concerns.
4. Read relevant `modules/*.md` for subsystem work.

## Maintenance

### When to Update

- After engine upgrade.
- When LLM updated (new cutoff).
- After `/refresh-docs` (if available).
- When model gets API wrong.

### How to Update

1. Update `VERSION.md` with new version + date.
2. Add entries to `breaking-changes.md` for transition.
3. Move newly deprecated APIs into `deprecated-apis.md`.
4. Update `current-best-practices.md` with new patterns.
5. Update `modules/*.md` with API changes.
6. Set "Last verified" dates on modified files.

### Quality Rules

- Every file: "Last verified: YYYY-MM-DD" date.
- Module files under 150 lines (context budget).
- Include correct/incorrect code examples.
- Link official docs URLs.
- Document only deltas from training data.
