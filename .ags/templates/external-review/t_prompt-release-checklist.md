# External Review Prompt — Release / Launch Checklist

You are an independent release-management reviewer. You have no prior context.

## Project context

{{PROJECT_CONTEXT}}

## Target

Release / launch checklist: `{{TARGET}}`

```
{{TARGET_CONTENT}}
```

## Related context

- Target platforms + cert requirements, QA sign-off status, build pipeline, store metadata, prior patch notes / changelog:

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Platform cert coverage** — every target storefront / platform has a cert section (Steam, Epic, Switch, PlayStation, Xbox, mobile stores)? Generic checklist hides platform-specific landmines.
2. **Build pipeline readiness** — release-branch policy, version numbering, signing, symbols, debug-asset stripping all defined?
3. **Store metadata completeness** — title, description, screenshots, trailer, age rating, system requirements, language list per store?
4. **Legal** — EULA, privacy policy, third-party licences, age ratings, region-specific compliance (EU AI Act, GDPR, COPPA, China publishing) addressed?
5. **QA gate dependencies** — checklist requires QA sign-off + smoke pass + regression clean before submission? Or implicit?
6. **Performance / certification budgets** — platform-specific perf, memory, load-time targets verified per checkpoint?
7. **Localization completeness** — all shipping locales QA'd, audio/subtitle coverage matrix verified per locale?
8. **Live-ops / day-one readiness** — server-side dependency status, day-one patch pipeline, rollback plan?
9. **Customer support / community** — community channels prepared, known-issues list, FAQ, support escalation path?
10. **Telemetry / crash reporting** — analytics + crash reporting wired before submission, opt-in flow compliant?
11. **Marketing / PR sync** — embargo dates, key art delivery, press kit, content-creator builds scheduled?
12. **Rollback / hotfix plan** — emergency-patch procedure documented? Authority for go/no-go declared?
13. **Sign-off authority** — final ship decision named (release-manager + technical-director)?

## Output format

JSON array of findings — same schema. Severity initial; re-classified downstream.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
