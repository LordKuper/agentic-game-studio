# Review Workflow

Canonical contract for every document-producing skill. Skills reference this file instead of embedding the loop inline.

## Sign-off matrix (orthogonal to the loop below)

1. Code changes → relevant department lead agent.
2. Design changes → `game-designer` + `creative-director`.
3. Architecture changes → `technical-director`.
4. Cross-domain changes → `producer`.

## Combined review loop

Internal reviewers (skill-specific specialists + director gate) and external Codex reviewer run **in parallel** within the same iteration. Codex is one general-profile reviewer in the pool, not a separate post-internal step. The skill-designated **aggregator** (producer for most generators; relevant lead for domain-specific skills) collects findings from every reviewer, drops nitpicks, applies the iteration severity floor, and decides loop exit.

```
Loop iteration N (start N = 1):
  severity_floor =
    N <= 2 → critical | high | medium | low   (all severities kept)
    N == 3 or 4 → critical | high             (medium / low dropped)
    N >= 5 → critical                         (high / medium / low dropped)

  parallel spawn:
    - skill-specific internal reviewers (specialists, director gate)
    - /ags-external-review [type] [draft-path] --embedded-parallel --min-severity [floor]
      (Codex unavailable on PATH → log skip in decisions-log.md, continue with internal pool)

  aggregator collects findings from every reviewer.

  drop:
    - nitpick (see definition below)
    - below severity_floor

  if filtered set is empty → exit loop, proceed to write approval.
  else → surface aggregated findings to user → user revises draft → N++ → re-run.
```

No iteration cap. No user-confirm gate before the external reviewer — it runs every iteration automatically.

## Severity floor — rationale

Iterations 1-2 catch real defects of any size. By iteration 3 the draft is mature; only critical / high should still surface. By iteration 5 the loop must be closing on hard blockers only — anything else means the reviewer is grinding. Floor exists to prevent infinite loops on opinion-grade findings.

## Architecture-impact escalation

If fixing a finding requires introducing a new abstraction / entity / layer or otherwise increases architectural complexity (new interface, new module, new pattern, extra level of indirection, new dependency, splitting an existing entity into several), the fix MUST be approved by the user **before** it is applied.

Applies to: code, design documents, ADRs, GDDs, epic contracts, stubs.

Flow:
1. Aggregator keeps the finding in the kept set as usual.
2. Before the author / executing agent starts the fix, they formulate the proposed change.
3. If the proposed change matches the criterion above, the agent does **not** write the change. Instead it describes to the user: which abstraction / entity is being introduced, why, and the alternatives (including "leave as is and accept the finding / lower severity / drop as nitpick").
4. User decides: approve, pick an alternative, or drop the finding.
5. The decision is recorded in `decisions-log.md` with `Type: architecture` when the change is accepted.

Not treated as complexity (may be fixed without approval): rename, typo / value fix, adding a missing field to an existing table, updating a citation to an ADR / rule, removing a duplicate, fixing a link.

This rule overrides severity floor: even a critical finding does not authorize silently introducing a new abstraction.

## User decision presentation format (mandatory)

Whenever a review finding, fix plan, or aggregator step requires a user decision (architecture-impact escalation, boundary fix that moves content, choice between conflicting reviewer recommendations, severity dispute, drop-vs-fix call, any "approve / pick alternative / drop" prompt), the agent surfacing the question MUST present it in this shape — in the **user's chat language** (per `.ags/rules/user-interaction.md`):

1. **Problem / finding** — full description of the defect, its location (file + line / section), why it matters, which rule / ADR / GDD / contract is implicated. No shorthand assuming the user remembers the review context.
2. **Options** — at least two concrete options. Each option is a self-contained plan: what changes, where, what gets written / removed / introduced.
3. **Recommended** — exactly one option marked `**Recommended**`. If no recommendation possible, state why explicitly and ask the user to choose.
4. **Consequences per option** — for every option (including the recommended one and "drop / accept finding"): what becomes possible, what becomes harder, what gets locked in, what downstream artifacts must change, reversibility cost.

Forbidden shortcuts:
- "Approve?" without options.
- Options without consequences.
- Recommendation without justification.
- Presenting the question in English while chat language is non-English (translate at presentation time; file writes stay English per `user-interaction.md`).

After user picks, record the decision in `decisions-log.md` with the chosen option, the rejected alternatives (one line each), and the rationale cited by the user.

## Nitpick — definition (mandatory drop)

A finding is a **nitpick** and must be dropped by the aggregator regardless of severity floor when it matches any of:

- Wording polish, alternative phrasing, prose smoothing without a concrete defect.
- Style preference with no project-rule / ADR / standard cite.
- Opinion-only suggestion ("would read better if…", "consider renaming X to Y" without a rule reason).
- Redundant comment / restated existing content.
- Formatting micro-fix where the existing form is already valid.
- "Could also do X" alt approaches without showing the current approach is wrong.

Substantive findings (always kept, subject to floor):

- Concrete defect with a failure mode (bug, contract break, security CWE, accessibility WCAG criterion, perf budget violation).
- Inconsistency with a registered architectural stance, ADR, GDD, or project rule (cite required).
- Missing required section / contract / test coverage on a Must-Have story.
- Engine API misuse (deprecated, post-cutoff without verification, wrong signature).

## Reviewer instruction (mandatory in every gate prompt and Codex prompt)

Every reviewer (internal agent gate, Codex) MUST receive this block in its prompt:

> **Report only substantive findings. No nitpicks.** Skip wording polish, alternative phrasing, opinion-only style notes, redundant comments. A finding is substantive only if it cites a concrete defect, project rule, ADR, GDD, registered stance, security CWE, accessibility WCAG criterion, performance budget, or engine API contract.
>
> **Iteration {{ITERATION}} severity floor: {{SEVERITY_FLOOR}}.** Omit findings below this floor — the aggregator will drop them anyway.

Director-gates.md `## Reviewer guidance` section is the canonical source. Skills paste it verbatim when spawning a gate.

## Codex unavailable handling

`codex` not on PATH → `/ags-external-review --embedded-parallel` returns immediately with `{kept: [], dropped: [], skipped: "codex-unavailable"}`. Caller logs to `.ags/project/decisions-log.md`:

```
## [YYYY-MM-DD HH:MM] — External review skipped: [type] [slug] iter [N]

**Type**: process
**Reason**: codex CLI not on PATH
**Decided by**: system (auto)
```

Loop continues with internal reviewers only. No user prompt.

## Aggregator responsibilities

- Single agent per skill run. Default: `producer`. Domain-specific skills may designate the relevant lead (e.g. `lead-programmer` for `ags-architecture-decision` ADR loop, `art-director` for `ags-art-bible`).
- Receives raw findings from every reviewer (internal + external).
- Applies the drop rules in this file.
- Surfaces only kept findings to the user.
- Records iteration count, dropped count by reason, kept count by severity in the external review report (when external ran) and in the skill's verdict line.
- Flags kept findings whose fix would trigger Architecture-impact escalation with `arch-escalate`. User must approve the fix plan before the change is written.

## Standalone gate exception

`/ags-gate-check release` and `/ags-gate-check epic-done` retain external review as a separate gate (not parallel-with-internal), because at gate time the artifact is already written and the gate is a verdict, not an authoring loop. Those gates pass `--min-severity` based on their own iteration counter.

## Document Boundary Check (mandatory in every doc-review iteration)

Every doc-review skill (architecture-review, design-review, review-all-gdds, ux-review, gate-check, propagate-design-change, external-review) MUST include a Document Boundary Check step that enforces `.ags/rules/document-boundaries.md`. Findings count as **substantive** (not nitpicks) — never dropped by the iteration severity floor.

### Per-doc-type checks (run on every reviewed artifact)

| Doc type | Check |
|---|---|
| GDD (`design/gdd/*.md`) | (1) front-matter `status:` present + valid; (2) no class/namespace/method names, no `using/import`, no library names from engine-reference; (3) no ms/MB/FPS literals outside player-observable acceptance; (4) no `adr-NNNN` / `ADR-NNNN` cite; (5) no raw color/typography/spacing literals; (6) no data-schema definitions (JSON/YAML/SQL shapes); (7) entity ids cited from `design/registry/entities.yaml` only. |
| ADR (`design/architecture/adr-*.md`) | (1) front-matter `status:` present + valid; (2) `**GDD source**:` line present + cited GDD `status: approved` (or explicit `Foundational — no GDD requirement`); (3) does NOT redefine concept rules from cited GDD (drift risk — must reference, not copy); (4) cited GDD section anchors resolve. |
| UX-spec (`design/ux/<screen>.md`, not `hud.md`) | (1) front-matter `status:` present + valid; (2) cited GDD `status: approved`; (3) no mechanic-rule duplication from GDD; (4) no raw color/typography/spacing literals — only `{tokens}` from DESIGN.md. |
| HUD-spec (`design/ux/hud.md`) | (1) front-matter `status:` present + valid; (2) cited UX-spec(s) AND `design/art/DESIGN.md` `status: approved`; (3) no raw visual literals; (4) no mechanic-rule duplication. |
| art-bible (`design/art/ags-art-bible.html`) | (1) `<meta name="status">` present + valid; (2) Section 8 (UI Art Standards) cites DESIGN.md tokens, no raw values; (3) `design/art/DESIGN.md` `status: approved`. |
| DESIGN.md (`design/art/DESIGN.md`) | (1) front-matter `status:` present + valid; (2) lint passes (`npx @google/design.md lint` errors=0). |
| game-concept, engine doc | front-matter `status:` present + valid. |

### Reviewer prompt addendum (paste into every review pool prompt)

> **Document Boundary Check** — apply `.ags/rules/document-boundaries.md` to the artifact under review. Report any boundary violation as a **substantive finding** (severity: high). Includes: tech-leak in GDD, GDD→ADR cite, raw visual literal outside DESIGN.md, missing/invalid front-matter `status:`, missing `**GDD source**:` line in ADR, content duplication across SSoT zones, citation of unapproved predecessor.

### Tooling delegation

Skills MAY delegate the mechanical scan to `/ags-consistency-check` (Phase 3e covers boundary detection). Doing so satisfies this requirement provided the skill surfaces the resulting Boundary Violations section to the user / aggregator.

### Aggregator handling

- Boundary findings: **never dropped** by severity floor (always at least `high`).
- Boundary fix that requires moving content between docs (e.g. lift schema out of GDD into new ADR) → triggers Architecture-impact escalation rules (user approval required before write).

---

## Cross-references

- `.ags/rules/director-gates.md` — gate prompt structure, reviewer guidance block.
- `.ags/rules/document-boundaries.md` — SSoT zones + approval-marker chain enforced by Document Boundary Check.
- `.claude/skills/ags-external-review/SKILL.md` — `--embedded-parallel` mode contract.
- `.claude/skills/ags-consistency-check/SKILL.md` — Phase 3e boundary scan (delegation target).
- `.ags/templates/external-review/t_review-report.md` — report layout including dropped findings.
