# Coding Rules

Rules for writing and editing code. All code in English.

## 1. Engineering principles

- Follow SOLID, KISS, DRY, YAGNI.
- Small atomic functions. Single clear responsibility.
- Readability and maintainability over cleverness.
- No hidden coupling, global state, action at a distance.
- No backwards-compatibility shims unless game already shipped.

## 2. Project layout

- Production code and test code in separate assemblies/projects.
- Never place production code in test projects or vice versa.

## 3. Documentation

- All types and **all members** (methods, events, properties, fields) — regardless of visibility — MUST have doc comments in English.
- Use language-native doc format (XML-doc for C#, docstrings for Python, JSDoc for TS, etc.).
- When modifying code, update doc comments to stay accurate.

## 4. Data-driven design

- Gameplay values (balance, tuning, content) live in external config files.
- Never hardcode gameplay values in code.
- Mods and designers tweak data without code changes.

## 5. Engine API reference

- LLM training data may predate pinned engine version.
- **Always check engine documentation for pinned version before using any engine API.**
- Do not guess post-cutoff API signatures. Look them up.

## 6. Modding & patchability

Write code patchable by Harmony or equivalent:

- Small purposeful methods (clear patch points).
- Avoid mega-methods and excessive inlining.
- Stable public entry points, predictable side effects.
- Do not seal types/methods likely mod extension points unless strong reason.
- No static constructors with heavy side effects.
- Prefer data-driven behavior.

## 7. Determinism

- Simulation, generation, gameplay-critical logic: same inputs → same outputs.
- No time-dependent or order-dependent behavior in core logic unless required.
- If nondeterminism required, isolate behind small interface, test deterministic part separately.

## 8. Performance & main-thread freedom

- Keep main thread free. Move off-main any work that can move.
- CPU-heavy work → Unity Jobs system, Burst-compiled where applicable.
- I/O-bound work → `async/await` (engine-native awaitable). Never `.Result` / `.Wait()` on main thread.
- Offload file I/O, network, heavy parsing, long computation.
- Main-thread blocking only when engine API requires it and no async alternative documented.
- No `Thread.Sleep`, busy-wait, synchronous locks on main thread.
- Apply engine-native compilation/optimization (e.g. Unity `[BurstCompile]`) where it pays off. Profile first when in doubt; do not blindly annotate trivial code.

## 9. Observability

- Structured logging via scoped logger pattern (per-class scope with source tag).
- Configure log level once at startup; do not mutate at runtime.
- Logs actionable, gated by level/category. No spam.
- Prefer measurable signals (counters, timers, events) for perf-sensitive areas.

## 10. Testing

### 10.1 Verification-driven development

- Tests first when adding gameplay systems.
- Compare expected vs actual output before marking work complete.
- Every implementation must have way to prove it works.

### 10.2 Coverage

- All public methods covered by tests.
- Intentional exclusion → document reason in code, keep surface minimal.

### 10.3 Test rules

- **Deterministic** — same result every run. No random seeds, no time-dependent assertions.
- **Isolated** — each test sets up and tears down own state. No order dependencies.
- **Independent** — unit tests do not call external APIs, databases, file I/O. Use DI.
- **No hardcoded data** — fixtures via constants or factory functions, not inline magic numbers (exception: boundary value tests where number is the point).
- **Naming** — files `[system]_[feature]_test.[ext]`; functions `test_[scenario]_[expected]`.
- **Static state isolation** — any test mutating global static state (composition root, log handler, etc.) MUST save/restore in `[SetUp]`/`[TearDown]` to prevent cross-test leakage.

## 11. TODO stubs (epic workflow)

Vertical-slice epics may stub neighbor systems instead of implementing them all at once.

- Stub = interface + `NotImplementedException` or sane default.
- Code marker required: `// TODO(epic-[id]): [reason]`.
- Every stub registered in `.ags/project/stubs.md` (Open Stubs table).
- Owner-epic field in registry names the future epic that closes the stub.
- Gate `/ags-gate-check epic-done` blocks epic close if stubs introduced by the closing epic remain Open and have no Migration entry.
- Stub interfaces are stable; changes require ADR or new epic.

## 12. Commits

- Reference relevant story ID or design doc in commit message.
- Prefer new commits over amending.
- Never commit secrets (`.env`, credentials, keys).
