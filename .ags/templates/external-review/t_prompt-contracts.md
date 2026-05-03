# External Review Prompt — Epic Contracts (stub interfaces)

You are an independent technical reviewer. You have no prior context.

## Project context

{{PROJECT_CONTEXT}}

## Target

Epic contracts (Contracts section of EPIC.md or contracts file): `{{TARGET}}`

```
{{TARGET_CONTENT}}
```

## Related context

- EPIC.md, related ADRs, registered stubs from `.ags/project/stubs.md`, owner-epics for each stub, architecture registry stances:

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Interface minimality** — each stub interface exposes only methods/properties actually consumed by this epic? Speculative API surface?
2. **Signature stability** — types and parameter shapes stable enough that the owner-epic can implement without breaking call sites? Likely-to-churn types flagged?
3. **Naming consistency** — interface names match the owner-epic's GDD/ADR vocabulary? Drift between contract and design doc?
4. **Default behaviour** — stub returns sane default (zero, empty collection, no-op)? Not silent `null` or undefined?
5. **NotImplementedException usage** — methods that MUST be implemented by owner-epic throw `NotImplementedException`, not silently default?
6. **Side-effect honesty** — stub does not mutate global state, log, or produce sound while pretending to be no-op?
7. **Stub registration** — every contract entry has matching row in `.ags/project/stubs.md` with owner-epic + reason + marker?
8. **Determinism** — stub returns deterministic value per input? No random or time-dependent default that masks bugs?
9. **Test impact** — any contract that, if defaulted, would mask test failures? Should owner-epic ship before this epic's tests are trusted?
10. **Owner-epic coverage** — every stub names an owner-epic that exists in `epics/index.md`, or explicitly `TBD-<reason>`?
11. **Architectural conformity** — interfaces respect registered stances (signal vs direct call, ownership boundaries) in `docs/registry/architecture.yaml`?

## Output format

JSON array of findings — same schema. Severity initial; re-classified downstream.

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
