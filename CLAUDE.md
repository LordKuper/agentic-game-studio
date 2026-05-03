# Agentic Game Studio

## User-interaction
@.ags/rules/user-interaction.md

## Project Structure
@.ags/rules/directory-structure.md

## Document composition
- English, caveman-style.
- Concise, clear, unambiguous.
- Use template from `.ags/templates` matching doc type. No template fits — propose structure, get user approval, then create.
- **Review pipeline** for any document-producing skill: existing internal review (department lead / specialist / director gate) wraps in a **loop** until a single iteration passes with zero critical / high / medium findings — then a user-confirm `AskUserQuestion` gate offers external Codex review (`[A] run /ags-external-review` / `[B] skip + log reason in decisions-log.md` / `[C] stop`). Internal loop has no iteration cap. External review runs only on explicit `[A]`. Skill-specific phase numbering varies; every generator skill embeds this contract.

## Context Management
@.ags/rules/context-management.md

## Collaboration Protocol
**User-driven, not autonomous.** Every task: **Question → Options → Decision → Draft → Approval**.

- Ask "May I write this to [filepath]?" before Write/Edit.
- Show drafts/summaries before approval.
- Multi-file changes need explicit changeset approval.
- No commits without user instruction.

See `.ags/rules/collaboration.md`.

## Coding Rules
@.ags/rules/coding.md

## Design System
@.ags/rules/design-system.md

## Agent Coordination
@.ags/rules/coordination.md

> **First session?** No engine configured, no game concept — run `/ags-start`.
