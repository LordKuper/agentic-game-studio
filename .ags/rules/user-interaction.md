# User Interaction Rules

## Resolution order

1. If `.ags/project/p_user-interaction.md` exists — follow it.
2. Else bootstrap:
   - Read `.ags/templates/t_user-interaction.md`.
   - Ask user one question per template field.
   - After all answers collected, create `.ags/project/p_user-interaction.md` from template, filled.
   - Then follow created file. No approval needed.
