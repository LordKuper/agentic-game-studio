# Design Principles

Apply when designing systems, writing ADRs, GDDs, and code. Complements `.ags/rules/coding.md` (code-level SOLID/KISS/DRY/YAGNI) with broader system-level rules.

## 1. YAGNI — gameplay first

Do not build systems before gameplay is validated. Vertical slice over horizontal platforming. Stub (`coding.md` §11) preferred over premature full implementation. New system in ADR requires playtest or design evidence of need — not speculation.

## 2. KISS — simple core

Simple foundation. Complexity added iteratively after playtest. Balancing and iteration cheaper on simple core. Reject clever solutions when straightforward one works. Three similar lines beats premature abstraction.

## 3. Separation of Concerns

Separate layers: gameplay / UI / audio / data / save / AI. Each — own assembly / module. Cross-layer calls only through explicit contracts documented in ADR. No gameplay logic in UI code; no UI state in save data; no save serialization in gameplay loop.

## 4. Loose Coupling / High Cohesion

Systems communicate through interfaces / events / message bus — never direct references across module boundaries. Inside system: single responsibility, related data adjacent, no scattered ownership. Replacing one system must not require editing unrelated ones.

## 5. Single Source of Truth (SSoT)

One owner per fact. Runtime state — one authoritative system. Balance values — data-config (`coding.md` §4). Visual tokens — `design/art/DESIGN.md` (`design-system.md`). Entity ids — `design/registry/entities.yaml`. Document SSoT — see `document-boundaries.md` §1. Duplication = contract violation.

## 6. Fail Fast

Validate content / data / config on load, not in runtime. Schema check, missing-ref check, range check, enum check. Failure → loud structured log + abort load. No silent fallbacks that mask broken content. Crash beats corrupt save.

## 7. Observability by Design

Debug overlay, profiler hooks, structured logs (`coding.md` §9), dev console, cheat commands. Build observability into system from first iteration — not bolted on after release. Every gameplay system exposes inspectable state. Every perf-sensitive path exposes counters / timers.

## 8. Backward Compatibility

Save format versioned. Migration function per version bump. Mod API stable (`coding.md` §6). Content format — additive changes preferred; breaking change → ADR + migration plan + version bump. Patch must not invalidate existing saves, mods, or user content without explicit migration.

## 9. Evolutionary Architecture

Game design changes. Architecture must survive change. Hide decisions behind interfaces. Defer commitment where cost of reversal is high. ADR documents reversibility of decision. Avoid premature freezing of contracts that will be revisited in next epic.

## 10. Over-engineering smells

Concrete checklist for reviewers. Any item below is a **violation** unless author provides explicit justification (see §10.2). Applies to GDD, ADR, and code-level design.

### 10.1 Smell list

- **Interface / abstract base with one implementer** — extract only when ≥2 real implementers exist or a near-term second one is named and scheduled.
- **Generic / template parameter with one concrete type** — `Repository<T>` where only `Repository<Item>` ever instantiated.
- **Plugin / extension system with no plugins** — registry, dispatcher, hook framework built without a second consumer.
- **Config flag / mode without a second mode** — `useNewPathfinder=true` with no `false` branch in scope, or feature toggle for an unshipped alternative.
- **"For future use" fields** — data-config or schema fields with no current consumer code path.
- **Speculative tuning knob** — exposed parameter no designer has asked to tune; intent unverified by playtest.
- **Premature unification** — merging two systems that share surface but not semantics ("inventory and quest log both have items").
- **Premature optimization** — perf decision in ADR without profiler measurement or stated budget breach.
- **Layered indirection without payoff** — manager → service → provider → repository for a fact one system owns.
- **Custom DSL / scripting layer** — when same authoring need served by data-config or direct code.
- **Event bus for two known callers** — direct call simpler; event bus pays off ≥3 listeners or unknown future listeners with explicit reason.
- **Abstraction "in case we swap engine / DB / renderer"** — swap is not on roadmap, no ADR documents it.
- **Mechanic / subsystem in GDD with no player-experience justification** — feature exists because "RPGs have it", not because pillar / fantasy demands it.

### 10.2 Justification escape hatch

Author may keep a flagged item by adding explicit justification to the ADR/GDD:

```
Justification: <second consumer name + epic id where it lands>, OR
              <concrete near-term use case>, OR
              <ADR reference documenting the constraint>.
```

No justification → reviewer enforces removal. Vague justifications ("more flexible", "future-proof", "best practice") do **not** satisfy.

### 10.3 Severity and review handling

- Over-engineering findings are **always severity `critical`, category `over-engineering`**.
- Aggregator **MUST NOT** drop these as nitpicks.
- Iteration severity floor (`.ags/rules/review-workflow.md`) keeps `critical` at all iterations — over-engineering items survive every iteration until removed or justified per §10.2.
- Any unresolved over-engineering finding → **FAIL** verdict in `/ags-design-review` and `/ags-architecture-review` (not CONCERNS).

## Cross-references

- `.ags/rules/coding.md` — code-level engineering principles, data-driven design, testing, observability
- `.ags/rules/document-boundaries.md` — SSoT matrix for documents
- `.ags/rules/design-system.md` — DESIGN.md token authority
- `.ags/rules/coordination.md` — who approves what
