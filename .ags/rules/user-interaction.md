# User Interaction Rules

## Resolution order

1. If `.ags/project/p_user-interaction.md` exists — follow it.
2. Else bootstrap:
   - Read `.ags/templates/t_user-interaction.md`.
   - Ask user one question per template field.
   - After all answers collected, create `.ags/project/p_user-interaction.md` from template, filled.
   - Then follow created file. No approval needed.

## Scope

Communication language = user-facing chat only. Assistant talk to user in chosen language.

Always English regardless of choice:
- All files (code, docs, GDDs, ADRs, templates, state files, commits, comments, doc-comments)
- Filenames, paths, identifiers
- Tool inputs, command output

Conflict: file content rule wins. Choice never overrides English-files rule.
