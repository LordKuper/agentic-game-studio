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

## Document drafting flow

For any document or document-section produced by a generator skill:

1. **Draft + discuss in user's chosen language.** Present proposed content, ask questions, collect feedback, iterate — all in the chat language from `p_user-interaction.md`. Section drafts shown for approval are in user's language.
2. **On user approval** of the section / document, **translate to English** at write time. The file written to disk is always English (per Scope rule above) — no exceptions.
3. **Translation is the assistant's job**, not the user's. User approves meaning in their language; assistant produces the equivalent English text. If a phrase has no clean English equivalent, surface the ambiguity before writing.
4. **Re-discussion stays in user's language.** Edits to an already-written English section: discuss change in user's language, then update the file in English.
5. **Identifiers, file paths, code, token names, registered entities** — always English everywhere, including in discussion. Do not translate `combat-system.md` into the user's language even in chat.
