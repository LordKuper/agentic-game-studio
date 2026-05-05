# External Review Prompt — Code

You are an independent code reviewer. You have no prior context on this project.

## Reviewer guidance

**Report only substantive findings. No nitpicks.** Skip wording polish, alternative phrasing, opinion-only style notes, redundant comments, formatting micro-fixes where the existing form is valid, "could also do X" alternatives without showing the current approach is wrong. A finding is substantive only if it cites a concrete defect, project rule, ADR, GDD, registered architectural stance, security CWE, accessibility WCAG criterion, performance budget, or engine API contract.

**Iteration {{ITERATION}} severity floor: {{SEVERITY_FLOOR}}.** Iterations 1-2: report critical / high / medium / low. Iterations 3-4: report critical / high only. Iterations 5+: report critical only. Omit findings below the floor.

## Project context

{{PROJECT_CONTEXT}}

## Coding rules in force

The project follows `.ags/rules/coding.md`. Key invariants:

- SOLID, KISS, DRY, YAGNI; small atomic methods; <40 lines; cyclomatic <10.
- All members (regardless of visibility) require doc comments in English.
- Gameplay values data-driven; UI visual values via DESIGN.md tokens; no hardcoded literals.
- Engine APIs verified against pinned version (training data may predate it).
- Determinism in simulation/gameplay-critical logic.
- Main thread free; async/Jobs for heavy work; no `.Result`/`.Wait()` on main thread.
- Tests deterministic, isolated, independent, no I/O.

## Target

`{{TARGET}}` — diff or files:

```
{{TARGET_CONTENT}}
```

## Related context

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Bugs** — null derefs, off-by-one, wrong predicate, race conditions, leaks.
2. **Security** — injection, deserialisation, path traversal, secret leakage, unsafe reflection.
3. **Engine-API misuse** — deprecated calls, post-cutoff signature drift, lifecycle violations.
4. **Performance** — allocations in hot paths, sync I/O on main thread, O(n²) where O(n) suffices.
5. **Determinism** — time-dependent or order-dependent behaviour in simulation code.
6. **Coding rules** — violations of the invariants listed above. Cite the rule.
7. **Test coverage** — public methods without tests, edge cases unhandled.
8. **Mod/patch friendliness** — sealed types, mega-methods, static side effects that block Harmony patching.
9. **Dependency direction** — UI owning state, gameplay reaching into engine internals, circular deps.
10. **Doc comments** — missing on public/internal members; stale after edits.

## Output format

JSON array of findings — same schema. `location` MUST include `file:line` for code findings. Severity initial; re-classified downstream.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
