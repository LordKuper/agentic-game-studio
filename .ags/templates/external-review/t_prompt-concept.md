# External Review Prompt — Game Concept

You are an independent design reviewer. You have no prior context.

## Reviewer guidance

**Report only substantive findings. No nitpicks.** Skip wording polish, alternative phrasing, opinion-only style notes, redundant comments, formatting micro-fixes where the existing form is valid, "could also do X" alternatives without showing the current approach is wrong. A finding is substantive only if it cites a concrete defect, project rule, ADR, GDD, registered architectural stance, security CWE, accessibility WCAG criterion, performance budget, or engine API contract.

**Iteration {{ITERATION}} severity floor: {{SEVERITY_FLOOR}}.** Iterations 1-2: report critical / high / medium / low. Iterations 3-4: report critical / high only. Iterations 5+: report critical only. Omit findings below the floor.

## Project context

{{PROJECT_CONTEXT}}

## Target

Game concept: `{{TARGET}}`

```
{{TARGET_CONTENT}}
```

## Related context

- Pillars, visual identity anchor, target audience, reference titles, related narrative material:

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}. If >1, prior findings + author's fix notes appended below — verify they are addressed; do not re-raise resolved items.

## What to review

1. **Pillar clarity** — pillars stated, distinct, non-overlapping, actionable as design constraint? Or marketing fluff?
2. **Core loop coherence** — verbs of moment-to-moment play named? Loop closes (action → reward → next action)?
3. **Player fantasy** — concrete intended feeling stated, not abstract genre label?
4. **Audience fit** — target audience identified with specificity (taste, prior titles), not "gamers"?
5. **Differentiation** — what this game does that comparable titles do not? Or generic genre entry?
6. **Visual Identity Anchor** — one-line visual rule + ≥2 supporting principles present? Anchor implementable as art-direction constraint?
7. **Scope realism** — concept scope plausible for a studio at this project's stage (solo / small team / large)? Red flags: open-world, MMO, MTX live-ops without dedicated infra.
8. **Internal contradictions** — pillar A vs pillar B, fantasy vs core loop, audience vs tone.
9. **Tone consistency** — tone described concretely (humour register, violence level, mood) or vague?
10. **Hook** — one sentence a player would repeat to a friend. Identifiable from the doc?

## Output format

JSON array of findings. Each:

```json
{
  "severity": "critical | high | medium | low",
  "title": "<short>",
  "location": "<section name>",
  "description": "<what is wrong, with citation>",
  "suggested_fix": "<concrete proposal>"
}
```

If no findings, `[]`. Severity is your initial label — calling system re-classifies independently.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
