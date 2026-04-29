# User Interaction Rules

## Resolution order

1. If `.ags/project/p_user-interaction.md` exist — follow rules in it.
2. Else — bootstrap:
   - Read template `.ags/templates/t_user-interaction.md`.
   - Ask user one question per template field.
   - After all answers collected, create `.ags/project/p_user-interaction.md` from template, filled with answers.
   - Then follow created file without asking approval.
