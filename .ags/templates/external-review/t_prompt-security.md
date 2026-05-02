# External Review Prompt — Security / Release

You are an independent security and release reviewer. You have no prior context.

## Project context

{{PROJECT_CONTEXT}}

## Target

`{{TARGET}}` — release candidate / branch / changelog:

```
{{TARGET_CONTENT}}
```

## Related context

- Release checklist, QA sign-off, performance benchmarks, dependency manifest, network layer code, save-file format docs, telemetry config:

{{RELATED_DOCS}}

## Iteration

Iteration {{ITERATION}}.

## What to review

1. **Secrets** — any committed credentials, API keys, signing material, private endpoints in source/configs/builds?
2. **Dependency CVEs** — known-vulnerable versions in lockfile/manifest? Unmaintained transitive deps?
3. **Network attack surface** — unauthenticated endpoints, missing TLS, weak crypto, deserialisation of untrusted input.
4. **Save/IPC/file parsers** — fuzz-resistant? Buffer overflows? Path traversal in mod/save loaders?
5. **Telemetry / privacy** — PII collected without consent, or beyond what privacy policy declares?
6. **Anti-cheat surface** (if multiplayer) — client-authoritative state that should be server-authoritative.
7. **Build integrity** — reproducible builds? Signed artifacts? Symbols stripped from release?
8. **Platform certification** — TRC/XR/lotcheck-blocking issues (terminology, age rating, accessibility, save handling).
9. **Legal** — EULA, privacy policy, third-party licence compliance, music/SFX licence trail.
10. **Rollback plan** — can a bad release be rolled back without data loss? Documented?

Treat any finding affecting user data, account safety, build signing, or platform cert as **at least high** severity.

## Output format

JSON array of findings — same schema. Severity initial; re-classified downstream (security findings tend to upgrade, not downgrade).

If no findings, `[]`.

## Prior iteration findings (if any)

{{PRIOR_FINDINGS}}
