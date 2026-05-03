# External Review Prompt — Control Manifest

You are an independent UX / input reviewer. You have no prior context.

## Project context

{{PROJECT_CONTEXT}}

## Target

Control manifest: `{{TARGET}}`

```
{{TARGET_CONTENT}}
```

## Related context

- Game concept, target platforms, accessibility tier, related UX specs, engine input system in use:

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Action completeness** — every player verb from game-concept / GDDs has a control entry? Any orphan action without binding?
2. **Platform coverage** — bindings defined for every target platform (KB+M, Gamepad, Touch, Steam Deck) per project's platform list?
3. **Convention adherence** — bindings follow platform conventions (e.g. console A=accept, B=cancel; PC Esc=menu)? Deviations justified?
4. **Conflict detection** — any binding maps two distinct actions to the same input in the same context? Any modifier-key collision?
5. **Context separation** — gameplay / menu / cinematic / vehicle / inventory contexts defined separately? Or one mega-context with mode flags?
6. **Accessibility — remapping** — every action remappable? Locked bindings flagged with reason?
7. **Accessibility — alternative inputs** — toggle-vs-hold, repeat-rate, hold-duration, dwell-click options where required by tier?
8. **Haptics & feedback** — haptic patterns assigned for primary feedback events? Or "TBD"?
9. **Engine binding feasibility** — bindings expressible in pinned engine's input system (Unity Input System actions/composites, Godot InputEventAction, etc)?
10. **Localization readiness** — action display names externalised? Glyphs (gamepad button icons) handled per platform?
11. **Default-set completeness** — at least one default binding per (action × platform) pair? No `[TBD]` in defaults?

## Output format

JSON array of findings — same schema. Severity initial; re-classified downstream.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
