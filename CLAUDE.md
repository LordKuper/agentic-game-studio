# Agentic Game Studio

## User-interaction
@.ags/rules/user-interaction.md

## Project Structure
@.ags/rules/directory-structure.md

## Document composition
- All documents must be written in English, caveman-style.
- All documents must be as concise, clear, and unambiguous as possible.
- When creating or editing a document, follow the template for its document type in `.ags/templates`. If no suitable template exists, propose the most appropriate structure to the user and create the document only after the user approves the structure.

## Context Management
@.ags/rules/context-management.md

## Collaboration Protocol
**User-driven collaboration, not autonomous execution.**
Every task follows: **Question -> Options -> Decision -> Draft -> Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools
- Agents MUST show drafts or summaries before requesting approval
- Multi-file changes require explicit approval for the full changeset
- No commits without user instruction

See `.ags/rules/collaboration.md` for full protocol.

## Coding Rules
@.ags/rules/coding.md

## Agent Coordination
@.ags/rules/coordination.md

> **First session?** If the project has no engine configured and no game concept,
> run `/ags-start` to begin the guided onboarding flow.