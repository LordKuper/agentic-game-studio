# External Review Prompt — Asset Spec

You are an independent art-pipeline reviewer. You have no prior context.

## Reviewer guidance

**Report only substantive findings. No nitpicks.** Skip wording polish, alternative phrasing, opinion-only style notes, redundant comments, formatting micro-fixes where the existing form is valid, "could also do X" alternatives without showing the current approach is wrong. A finding is substantive only if it cites a concrete defect, project rule, ADR, GDD, registered architectural stance, security CWE, accessibility WCAG criterion, performance budget, or engine API contract.

**Iteration {{ITERATION}} severity floor: {{SEVERITY_FLOOR}}.** Iterations 1-2: report critical / high / medium / low. Iterations 3-4: report critical / high only. Iterations 5+: report critical only. Omit findings below the floor.

## Project context

{{PROJECT_CONTEXT}}

## Target

Asset spec: `{{TARGET}}`

```
{{TARGET_CONTENT}}
```

## Related context

- Art bible, DESIGN.md tokens, pinned engine renderer (URP/HDRP/built-in), pipeline tools, related GDDs:

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Naming convention** — naming rule unambiguous (prefix, domain, variant, LOD)? Examples present? Collisions impossible?
2. **Format / resolution** — file format and dimensions appropriate for engine + platform (texture compression, audio codec, mesh budget)?
3. **Budget compliance** — vertex / triangle / texture-memory / draw-call / animation-clip budgets within technical-preferences targets?
4. **LOD / streaming** — LOD chain or streaming strategy specified for assets above budget threshold?
5. **Authoring source** — source file location + tool + version specified? Or only the export specified?
6. **Pipeline steps** — concept → block-out → final → engine import described? Each step has clear hand-off?
7. **Naming-vs-engine compatibility** — any naming rule that breaks engine importer (case-sensitivity, reserved chars, length limits)?
8. **Reference / IP** — references cited with source + license? Anything risking IP infringement?
9. **DESIGN.md token usage** — UI assets reference DESIGN.md tokens for color / typography? No raw hex duplicated?
10. **Variants / states** — variant matrix (skin tone, damage state, material override) enumerated, not implicit?
11. **Acceptance criteria** — pass/fail check definitions for QA (silhouette readable at N px, runtime cost ≤ X ms, etc)?
12. **Localization** — text-on-textures avoided / externalised? Glyph atlas plan for non-Latin scripts?

## Output format

JSON array of findings — same schema. Severity initial; re-classified downstream.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
