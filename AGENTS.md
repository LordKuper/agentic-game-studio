# AGENTS Instructions

These rules apply to source code output produced by agents (C# files, test files, scripts).

- All communication with the user must be in English.
- All C# classes/structs/interfaces/enums, methods, events, properties must be documented in English using XML documentation comments.
- When modifying existing code, update XML-doc so it stays accurate and consistent.
- Do not enable nullable reference types (`#nullable enable`) or use C# 8+ nullable syntax (`string?`, `!` null-forgiving) in any new or modified C# code files. Remove these if they appear in files you modify.
- Follow SOLID, KISS, DRY, YAGNI.
- Prefer small, atomic functions with a single, clear responsibility.
- Favor readability and maintainability over cleverness.
- Maintain automated test coverage of at least 80% across the entire project codebase.
- Place all tests in the `tests` folder.

# Agent Definition Rules
- Agent definitions live in `agents` folder and must be named `<agent-name>.md`
- Agent definitions must conform to the template defined in `agent-template.md`