# External Review Prompt — Systems Index

You are an independent design / production reviewer. You have no prior context.

## Reviewer guidance

**Report only substantive findings. No nitpicks.** Skip wording polish, alternative phrasing, opinion-only style notes, redundant comments, formatting micro-fixes where the existing form is valid, "could also do X" alternatives without showing the current approach is wrong. A finding is substantive only if it cites a concrete defect, project rule, ADR, GDD, registered architectural stance, security CWE, accessibility WCAG criterion, performance budget, or engine API contract.

**Iteration {{ITERATION}} severity floor: {{SEVERITY_FLOOR}}.** Iterations 1-2: report critical / high / medium / low. Iterations 3-4: report critical / high only. Iterations 5+: report critical only. Omit findings below the floor.

## Project context

{{PROJECT_CONTEXT}}

## Target

Systems index: `{{TARGET}}`

```
{{TARGET_CONTENT}}
```

## Related context

- Game concept, pillars, milestone scope, prior systems indexes if any:

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Completeness vs concept** — every game-loop system implied by `game-concept.md` has a row? Any feature mentioned in concept missing here?
2. **Priority tiers** — MVP / Vertical-slice / Stretch tiers populated and credible? Any "MVP" item that is actually stretch?
3. **Dependency ordering** — depends-on graph acyclic? Each system's prerequisites listed before it in build order?
4. **Granularity** — systems sized for one epic each (1-3 weeks scope)? Any monoliths needing split? Any trivia needing merge?
5. **Naming consistency** — system names match what the GDDs / ADRs / registry use? Synonyms diverging?
6. **Domain coverage** — Core, Foundation, Presentation, Feature layers all represented where required?
7. **Cross-cutting concerns** — save/load, input, audio, accessibility, networking, telemetry — listed once with explicit owner, not duplicated per feature?
8. **Stretch creep** — Stretch tier realistic given team size? Items that should be cut, not deferred?
9. **Risk flags** — high-risk systems (post-cutoff engine APIs, novel mechanics) marked? Or buried?
10. **Status hygiene** — every row has Status / Owner / Priority filled? No `[TBD]` in MVP rows?

## Output format

JSON array of findings — same schema. Severity initial; re-classified downstream.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
