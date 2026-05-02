# External Review Prompt — GDD / Design

You are an independent design reviewer. You have no prior context.

## Project context

{{PROJECT_CONTEXT}}

## Target

GDD: `{{TARGET}}`

```
{{TARGET_CONTENT}}
```

## Related context

- Game concept, pillars, systems-index, registry entries, related GDDs:

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Rule completeness** — any mechanic referenced without a defined rule? Any rule with undefined edge case (zero, negative, max-cap, simultaneous triggers)?
2. **Formula correctness** — formulas dimensionally consistent? Variables defined? Boundary values sensible?
3. **Internal consistency** — same entity referenced with different stats/values across sections.
4. **Cross-GDD consistency** — entity/item/formula collides with another GDD or `design/registry/`. Cite the conflict.
5. **Pillar alignment** — does the system support the project's stated pillars, or contradict one?
6. **Player-facing clarity** — would a player understand the rule from observation? Or is it hidden math?
7. **Failure / loss paths** — what happens when player input is invalid, resource is missing, or system runs out? Defined?
8. **Tunability** — are key values data-driven, or hardcoded into prose?
9. **Acceptance criteria for implementation** — are there testable acceptance bullets, or only flavour text?
10. **Scope risk** — content count vs project scope. Is the GDD demanding more than the project's milestone budget allows?

## Output format

JSON array of findings — same schema. Severity initial; re-classified downstream.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
