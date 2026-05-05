# External Review Prompt — Epic close

You are an independent reviewer for an epic-done gate. You have no prior context.

## Reviewer guidance

**Report only substantive findings. No nitpicks.** Skip wording polish, alternative phrasing, opinion-only style notes, redundant comments, formatting micro-fixes where the existing form is valid, "could also do X" alternatives without showing the current approach is wrong. A finding is substantive only if it cites a concrete defect, project rule, ADR, GDD, registered architectural stance, security CWE, accessibility WCAG criterion, performance budget, or engine API contract.

**Iteration {{ITERATION}} severity floor: {{SEVERITY_FLOOR}}.** Iterations 1-2: report critical / high / medium / low. Iterations 3-4: report critical / high only. Iterations 5+: report critical only. Omit findings below the floor.

## Project context

{{PROJECT_CONTEXT}}

## Target

Epic slug: `{{TARGET}}`

EPIC.md:

```
{{TARGET_CONTENT}}
```

## Related context

- Stories: {{RELATED_DOCS}}
- Open stubs introduced by this epic, GDD references, ADRs added, PR diff `epic/{{TARGET}} → main` (if available).

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Acceptance Criteria** — every box checked? Any criterion checked without evidence in stories or playtests?
2. **Stories** — every story Status=Done? Any Done story without test evidence (Logic/Integration tier)?
3. **Stubs** — every stub introduced by this epic Closed or Migrated with explicit owner-epic? Any orphan stubs?
4. **GDD/ADR drift** — does the implemented behaviour match the GDDs/ADRs the epic was scoped to? Surface unrecorded deviations.
5. **PR diff** — does the diff stay inside the systems the epic scoped? Cross-domain edits without delegation are violations.
6. **Test coverage** — Must-Have stories have automated tests? Visual/Feel stories have manual QA evidence?
7. **Bugs** — any S1/S2 bugs against this epic still open?
8. **Retrospective** — filled with concrete actions, or generic boilerplate?
9. **Architecture consistency** — any stance changes that should have triggered an ADR but didn't?
10. **Cross-system contracts** — if epic was stub-mode, are the stub contracts still honoured by the implementation?

## Output format

JSON array of findings — same schema as ADR review. Severity is initial; calling system re-classifies.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
