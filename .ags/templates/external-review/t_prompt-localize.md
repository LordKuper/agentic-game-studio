# External Review Prompt — Localization Plan / String Bundle

You are an independent localization reviewer. You have no prior context.

## Project context

{{PROJECT_CONTEXT}}

## Target

Localization artifact: `{{TARGET}}`

```
{{TARGET_CONTENT}}
```

## Related context

- Target locales, font fallbacks, UI specs, accessibility tier, in-game writing samples:

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **String externalisation** — every player-facing string in source bundle (UI, dialogue, error messages, tooltips, system prompts)? Hardcoded text in code = blocker.
2. **Key naming** — keys descriptive (`ui.button.start_game` not `string_42`)? Stable across locales?
3. **Context comments** — translator context provided per string (where shown, character count budget, tone, gendered subject)?
4. **Pluralisation** — ICU MessageFormat or equivalent used for plural / gender / select cases? Or naive concatenation that breaks in target locales?
5. **String length budgets** — max-length per UI slot stated? German / Russian / Finnish overflow risk handled?
6. **Variable interpolation** — placeholders use named tokens (`{playerName}`), not positional `%s`? Order can vary by locale.
7. **RTL support** — UI mirroring rules for Arabic / Hebrew? Layout flipping, icon mirroring exceptions?
8. **Font fallback** — non-Latin / CJK glyph coverage planned per font? Fallback chain defined?
9. **Locale list** — target locales explicit with priority tier? Cert-blocked locales (e.g. PRC requirements) flagged?
10. **Audio localization** — VO scope (subtitled / dubbed / partial) per locale? Lip-sync constraint handled?
11. **Cert / legal** — region-specific content rules (Germany: blood, China: skeletons, Korea: gambling) acknowledged where relevant?
12. **QA loop** — pseudo-loc pass, in-context review, native-speaker pass scheduled?

## Output format

JSON array of findings — same schema. Severity initial; re-classified downstream.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
