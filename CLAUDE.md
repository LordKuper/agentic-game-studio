# Agentic Game Studio

## User-interaction
@.ags/rules/user-interaction.md

## Project Structure
@.ags/rules/directory-structure.md

## Document composition
- English, caveman-style.
- Concise, clear, unambiguous.
- File format (`.md`, `.html`, etc.) is owned by the skill that produces the document — pick the format that best fits the content (Markdown by default; HTML when inline visual layout / SVG diagrams add real value).
- Use template from `.ags/templates` matching doc type. No template fits — propose structure, get user approval, then create.
- **Drafting language**: discuss + iterate document content with user in their chat language (from `user-interaction.md`). On approval of the section/document, **translate to English at write time** — file on disk is always English. See `.ags/rules/user-interaction.md` § Document drafting flow.
- **Review pipeline** for any document-producing skill: internal reviewers (department lead / specialist / director gate) and external Codex run **in parallel** every iteration as one combined review pool. Aggregator (producer or skill-designated lead) drops nitpicks and applies an **iteration severity floor**: iterations 1-2 keep all severities; iterations 3-4 keep only critical + high; iterations 5+ keep only critical. Loop exits on first iteration whose filtered set is empty. No iteration cap. Codex unavailable → auto-skip + log to `decisions-log.md`, no user prompt. Canonical contract: `.ags/rules/review-workflow.md`.

## Context Management
@.ags/rules/context-management.md

## Collaboration Protocol
**User-driven, not autonomous.** Every task: **Question → Options → Decision → Draft → Approval**.

- Ask "May I write this to [filepath]?" before Write/Edit.
- Show drafts/summaries before approval.
- Multi-file changes need explicit changeset approval.
- No commits without user instruction.

See `.ags/rules/collaboration.md`.

## Document Boundaries
@.ags/rules/document-boundaries.md

## Coding Rules
@.ags/rules/coding.md

## Design Principles
@.ags/rules/design-principles.md

## Design System
@.ags/rules/design-system.md

## Agent Coordination
@.ags/rules/coordination.md

> **First session?** No engine configured, no game concept — run `/ags-start`.
