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

## Standalone gate exception

`/ags-gate-check release` and `/ags-gate-check epic-done` retain external review as a separate gate (not parallel-with-internal), because at gate time the artifact is already written and the gate is a verdict, not an authoring loop. Those gates pass `--min-severity` based on their own iteration counter.

## Cross-references

- `.ags/rules/director-gates.md` — gate prompt structure, reviewer guidance block.
- `.claude/skills/ags-external-review/SKILL.md` — `--embedded-parallel` mode contract.
- `.ags/templates/external-review/t_review-report.md` — report layout including dropped findings.
