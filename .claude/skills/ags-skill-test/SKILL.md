---
name: ags-skill-test
description: "Validate skill files for structural compliance and behavioral correctness. Three modes: static (linter), spec (behavioral), audit (coverage report)."
argument-hint: "static [skill-name | all] | spec [skill-name] | category [skill-name | all] | audit"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, AskUserQuestion
---

# Skill Test

Validates `.claude/skills/*/SKILL.md` for structural compliance + behavioral correctness. No external deps.

**Four modes:**

| Mode | Command | Purpose | Token Cost |
|------|---------|---------|------------|
| `static` | `/ags-skill-test static [name\|all]` | Structural linter — 7 compliance checks per skill | Low (~1k/skill) |
| `spec` | `/ags-skill-test spec [name]` | Behavioral verifier — evaluates assertions in test spec | Medium (~5k/skill) |
| `category` | `/ags-skill-test category [name\|all]` | Category rubric — checks skill against category-specific metrics | Low (~2k/skill) |
| `audit` | `/ags-skill-test audit` | Coverage report — skills, agent specs, last test dates | Low (~3k total) |

---

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| Target skill argument or `.claude/skills/` directory | user / template | STOP. "No target skill. Usage: `/ags-skill-test <skill-name>` or `--all`." |

If STOP triggers, exit verdict **BLOCKED**.

---

## Phase 1: Parse Arguments

Mode from first arg:

- `static [name]` → 7 structural checks one skill
- `static all` → 7 checks all skills (Glob `.claude/skills/*/SKILL.md`)
- `spec [name]` → read skill + test spec, evaluate assertions
- `category [name]` → category-specific rubric from `CCGS Skill Testing Framework/quality-rubric.md`
- `category all` → category rubric every skill with `category:` in catalog
- `audit` (or no arg) → read catalog, list skills/agents, show coverage

Missing/unrecognized arg → output usage, stop.

---

## Phase 2A: Static Mode — Structural Linter

For each skill: read `SKILL.md` fully, run 7 checks.

### Check 1 — Required Frontmatter Fields
Must contain in YAML:
- `name:`
- `description:`
- `argument-hint:`
- `user-invocable:`
- `allowed-tools:`

**FAIL** if any absent.

### Check 2 — Multiple Phases
Must have ≥2 numbered phase headings:
- `## Phase N` or `## Phase N:`
- `## N.` (numbered top-level)
- ≥2 distinct `##` headings if not explicitly numbered

**FAIL** if <2 phase-like headings.

### Check 3 — Verdict Keywords
Must contain ≥1: `PASS`, `FAIL`, `CONCERNS`, `APPROVED`, `BLOCKED`, `COMPLETE`, `READY`, `COMPLIANT`, `NON-COMPLIANT`

**FAIL** if none.

### Check 4 — Collaborative Protocol Language
Must contain ask-before-write:
- `"May I write"` (canonical)
- `"before writing"` or `"approval"` near file-write instructions
- `"ask"` + `"write"` close proximity

**WARN** if absent (read-only skills may legit skip).
**FAIL** if `allowed-tools` includes `Write`/`Edit` but no ask-before-write language.

### Check 5 — Next-Step Handoff
Must end with recommended next action / follow-up. Look for:
- Final section mentioning another skill (e.g., `/ags-story-done`)
- "Recommended next" / "next step"
- "Follow-Up" / "After this" section

**WARN** if absent.

### Check 6 — Fork Context Complexity
If frontmatter has `context: fork`, skill should have ≥5 phase headings. Fork context for complex multi-phase only.

**WARN** if `context: fork` set but <5 phases.

### Check 7 — Argument Hint Plausibility
`argument-hint` non-empty. If body mentions multiple modes (`Mode A | Mode B`), hint should reflect them. Cross-reference hint vs first phase's "Parse Arguments".

**WARN** if hint `""` or modes don't match hint.

---

### Static Mode Output Format

Single skill:
```
=== Skill Static Check: /[name] ===

Check 1 — Frontmatter Fields:    PASS
Check 2 — Multiple Phases:       PASS (7 phases found)
Check 3 — Verdict Keywords:      PASS (PASS, FAIL, CONCERNS)
Check 4 — Collaborative Protocol: PASS ("May I write" found)
Check 5 — Next-Step Handoff:     WARN (no follow-up section found)
Check 6 — Fork Context Complexity: PASS (8 phases, context: fork set)
Check 7 — Argument Hint:         PASS

Verdict: WARNINGS (1 warning, 0 failures)
Recommended: Add a "Follow-Up Actions" section at the end of the skill.
```

`static all`: summary table + non-compliant list:
```
=== Skill Static Check: All 52 Skills ===

Skill                  | Result       | Issues
-----------------------|--------------|-------
gate-check             | COMPLIANT    |
design-review          | COMPLIANT    |
story-readiness        | WARNINGS     | Check 5: no handoff
...

Summary: 48 COMPLIANT, 3 WARNINGS, 1 NON-COMPLIANT
Aggregate Verdict: N WARNINGS / N FAILURES
```

---

## Phase 2B: Spec Mode — Behavioral Verifier

### Step 1 — Locate Files

Skill at `.claude/skills/[name]/SKILL.md`.
Spec path from `CCGS Skill Testing Framework/catalog.yaml` `spec:` field.

Errors:
- Missing skill: "Skill '[name]' not found in `.claude/skills/`."
- Missing spec path: "No spec path set for '[name]' in catalog.yaml."
- Spec file missing: "Spec file missing at [path]. Run `/ags-skill-test audit` to see coverage gaps."

### Step 2 — Read Both Files

Read skill + spec fully.

### Step 3 — Evaluate Assertions

For each **Test Case** in spec:

1. Read **Fixture** (assumed project state)
2. Read **Expected behavior** steps
3. Read each **Assertion** checkbox

For each assertion: evaluate whether skill's instructions, followed correctly given fixture, satisfy it. Reasoning check, not code execution.

Mark each:
- **PASS** — instructions clearly satisfy
- **PARTIAL** — partially address with ambiguity
- **FAIL** — would NOT satisfy given fixture

**Protocol Compliance** assertions (always present):
- Skill requires "May I write" before file writes
- Presents findings before requesting approval
- Ends with recommended next step
- Avoids auto-creating files without approval

### Step 4 — Build Report

```
=== Skill Spec Test: /[name] ===
Date: [date]
Spec: CCGS Skill Testing Framework/skills/[category]/[name].md

Case 1: [Happy Path — name]
  Fixture: [summary]
  Assertions:
    [PASS] [assertion text]
    [FAIL] [assertion text]
       Reason: The skill's Phase 3 says "..." but the fixture state means "..."
  Case Verdict: FAIL

Case 2: [Edge Case — name]
  ...
  Case Verdict: PASS

Protocol Compliance:
  [PASS] Uses "May I write" before file writes
  [PASS] Presents findings before asking approval
  [WARN] No explicit next-step handoff at end

Overall Verdict: FAIL (1 case failed, 1 warning)
```

### Step 5 — Offer to Write Results

"May I write these results to `CCGS Skill Testing Framework/results/skill-test-spec-[name]-[date].md` and update `CCGS Skill Testing Framework/catalog.yaml`?"

If yes:
- Write results file to `CCGS Skill Testing Framework/results/`
- Update skill entry in catalog.yaml:
  - `last_spec: [date]`
  - `last_spec_result: PASS|PARTIAL|FAIL`

---

## Phase 2D: Category Mode — Rubric Evaluation

### Step 1 — Locate Skill and Category

Skill at `.claude/skills/[name]/SKILL.md`.
`category:` field in catalog.yaml.

Errors:
- Skill not found: "Skill '[name]' not found."
- No `category:` field: "No category assigned for '[name]' in catalog.yaml. Add `category: [name]` to skill entry first."

`category all`: collect all skills with `category:` field, process each. `category: utility` skills evaluated against U1 (static checks pass) + U2 (gate mode correct) only — skip to static for U1.

### Step 2 — Read Rubric Section

Read `CCGS Skill Testing Framework/quality-rubric.md`. Extract section matching skill's category (e.g., `### gate`, `### team`).

### Step 3 — Read Skill

Read SKILL.md fully.

### Step 4 — Evaluate Rubric Metrics

For each metric in category rubric:
1. Check whether skill's instructions clearly satisfy criterion
2. Mark PASS/FAIL/WARN
3. For FAIL/WARN: identify exact gap (quote section or note absence)

### Step 5 — Output Report

```
=== Skill Category Check: /[name] ([category]) ===

Metric G1 — Director panel spawn:  FAIL
  Gap: Phase 3 spawns only CD-PHASE-GATE; TD-PHASE-GATE, PR-PHASE-GATE, AD-PHASE-GATE absent
Metric G2 — No auto-advance:       PASS

Verdict: FAIL (1 failure, 0 warnings)
Fix: Add TD-PHASE-GATE, PR-PHASE-GATE, and AD-PHASE-GATE to the director
     panel in Phase 3.
```

### Step 6 — Offer to Update Catalog

"May I update `CCGS Skill Testing Framework/catalog.yaml` to record this category check (`last_category`, `last_category_result`) for [name]?"

---

## Phase 2C: Audit Mode — Coverage Report

### Step 1 — Read Catalog

Read `CCGS Skill Testing Framework/catalog.yaml`. If missing — note first-run state.

### Step 2 — Enumerate Skills and Agents

Glob `.claude/skills/*/SKILL.md`. Extract skill name from dir.
Read `agents:` section from catalog for agent list.

### Step 3 — Build Skill Coverage Table

For each skill:
- Spec file exists? (use `spec:` path from catalog, or glob `CCGS Skill Testing Framework/skills/*/[name].md`)
- Look up `last_static`, `last_static_result`, `last_spec`, `last_spec_result`, `last_category`, `last_category_result`, `category` from catalog (or "never"/"—")
- Priority from catalog `priority:` field (critical/high/medium/low)

### Step 3b — Build Agent Coverage Table

For each agent:
- Spec exists? (catalog `spec:` or glob `CCGS Skill Testing Framework/agents/*/[name].md`)
- Look up `last_spec`, `last_spec_result`, `category`

### Step 4 — Output Report

```
=== Skill Test Coverage Audit ===
Date: [date]

SKILLS (72 total)
Specs written: 72 (100%) | Never static tested: 72 | Never category tested: 72

Skill                  | Cat      | Has Spec | Last Static | S.Result | Last Cat | C.Result | Priority
-----------------------|----------|----------|-------------|----------|----------|----------|----------
gate-check             | gate     | YES      | never       | —        | never    | —        | critical
design-review          | review   | YES      | never       | —        | never    | —        | critical
...

AGENTS (49 total)
Agent specs written: 49 (100%)

Agent                  | Category   | Has Spec | Last Spec   | Result
-----------------------|------------|----------|-------------|--------
creative-director      | director   | YES      | never       | —
technical-director     | director   | YES      | never       | —
...

Top 5 Priority Gaps (skills with no spec, critical/high priority):
(none if all specs are written)

Skill coverage:  72/72 specs (100%)
Agent coverage:  49/49 specs (100%)
```

No file writes in audit mode.

Offer: "Run `/ags-skill-test static all` for structural compliance? `/ags-skill-test category all` for category rubric? `/ags-skill-test spec [name]` for behavioral test?"

---

## Phase 3: Recommended Next Steps

After mode completes, contextual follow-up:

- After `static [name]`: "Run `/ags-skill-test spec [name]` to validate behavioral correctness if spec exists."
- After `static all` with failures: "Address NON-COMPLIANT first. Run `/ags-skill-test static [name]` for detail."
- After `spec [name]` PASS: "Update catalog.yaml to record pass date. Run `/ags-skill-test audit` for next gap."
- After `spec [name]` FAIL: "Review failing assertions, update skill or spec to resolve mismatch."
- After `audit`: "Start with critical-priority gaps. Use spec template at `CCGS Skill Testing Framework/templates/skill-test-spec.md` for new specs."
