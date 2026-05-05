# External Review Prompt — UX Spec / HUD / Interaction Patterns

You are an independent UX reviewer. You have no prior context.

## Reviewer guidance

**Report only substantive findings. No nitpicks.** Skip wording polish, alternative phrasing, opinion-only style notes, redundant comments, formatting micro-fixes where the existing form is valid, "could also do X" alternatives without showing the current approach is wrong. A finding is substantive only if it cites a concrete defect, project rule, ADR, GDD, registered architectural stance, security CWE, accessibility WCAG criterion, performance budget, or engine API contract.

**Iteration {{ITERATION}} severity floor: {{SEVERITY_FLOOR}}.** Iterations 1-2: report critical / high / medium / low. Iterations 3-4: report critical / high only. Iterations 5+: report critical only. Omit findings below the floor.

## Project context

{{PROJECT_CONTEXT}}

## Target

UX document: `{{TARGET}}`

```
{{TARGET_CONTENT}}
```

## Related context

- Game concept, control manifest, DESIGN.md tokens, accessibility tier, related GDDs (combat, inventory, etc):

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Flow completeness** — primary user flows mapped end-to-end? Entry, success, error, abort paths?
2. **Information architecture** — screen hierarchy and navigation explicit, no orphan screens or dead-ends?
3. **Input mapping consistency** — actions map to control-manifest entries; no contradictions across screens; modifier keys used consistently?
4. **Feedback** — every player action has confirmation feedback (visual + audio + haptic per platform)? Latency budget specified?
5. **Error & failure paths** — invalid input, network drop, save failure handled with player-readable messaging?
6. **Accessibility tier compliance** — tier (Basic/Standard/Full) requirements met? Remapping, text scaling, colorblind palettes, screen-reader cues, subtitles, reduced-motion all addressed where required?
7. **DESIGN.md token usage** — all visual values cited as tokens, no raw hex/px/pt? Component variants reference DESIGN.md component names?
8. **Layout adaptability** — supported aspect ratios / resolutions / safe-area handling described? Console-vs-PC differences explicit?
9. **State diagrams** — modal, focus, hover, pressed, disabled states defined for all interactive elements?
10. **Localization-readiness** — string length budgets stated? RTL handling? Font fallbacks for non-Latin scripts?
11. **Latency / motion budgets** — animation and transition durations within input-feel targets (≤100ms primary feedback)?
12. **Onboarding & discoverability** — first-time-user path defined? Affordances signalled visually, not via tutorial walls?

## Output format

JSON array of findings — same schema. Severity initial; re-classified downstream.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
