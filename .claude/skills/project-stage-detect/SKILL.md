---
name: project-stage-detect
description: "Automatically analyze project state, detect stage, identify gaps, and recommend next steps based on existing artifacts. Use when user asks 'where are we in development', 'what stage are we in', 'full project audit'."
argument-hint: "[optional: role filter like 'programmer' or 'designer']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write
model: haiku
# Read-only diagnostic skill — no specialist agent delegation needed
---

# Project Stage Detection

Scans project to determine current development stage, artifact completeness,
and gaps. Useful for: starting with existing project, onboarding, pre-milestone
check, or "where are we?"

---

## Workflow

### 1. Scan Key Directories

Analyze project structure and content:

**Design Documentation** (`design/`):
- Count GDD files in `design/gdd/*.md`
- Check for game-concept.md, game-pillars.md, systems-index.md
- If systems-index.md exists, count total systems vs. designed systems
- Analyze completeness (Overview, Detailed Design, Edge Cases, etc.)
- Count narrative docs in `design/narrative/`
- Count level designs in `design/levels/`

**Source Code** (`Assets/Scripts/`):
- Count source files (language-agnostic)
- Identify major systems (directories with 5+ files)
- Check for core/, gameplay/, ai/, networking/, ui/ directories
- Estimate lines of code (rough scale)

**Production Artifacts** (`.ags/project/`):
- Check for active sprint plans
- Look for milestone definitions
- Find roadmap documents

**Architecture Docs** (`design/architecture/`):
- Count ADRs (Architecture Decision Records)
- Check for overview/index documents

**Tests** (`tests/`):
- Count test files
- Estimate test coverage (rough heuristic)

### 2. Classify Project Stage

Based on scanned artifacts, determine stage. Check `.ags/project/stage.txt` first —
if it exists, use its value (explicit override from `/gate-check`). Otherwise,
auto-detect using these heuristics (check from most-advanced backward):

| Stage | Indicators |
|-------|-----------|
| **Concept** | No game concept doc, brainstorming phase |
| **Systems Design** | Game concept exists, systems index missing or incomplete |
| **Technical Setup** | Systems index exists, engine not configured |
| **Pre-Production** | Engine configured, `Assets/Scripts/` has <10 source files |
| **Production** | `Assets/Scripts/` has 10+ source files, active development |
| **Polish** | Explicit only (set by `/gate-check` Production → Polish gate) |
| **Release** | Explicit only (set by `/gate-check` Polish → Release gate) |

### 3. Collaborative Gap Identification

**DO NOT** just list missing files. Instead, **ask clarifying questions**:

- "I see combat code (`Assets/Scripts/Gameplay/combat/`) but no `design/gdd/combat-system.md`. Should we reverse-document it?"
- "You have 15 ADRs but no architecture overview. Should I create one to help new contributors?"
- "No sprint plans in `.ags/project/`. Are you tracking work elsewhere (Jira, Trello, etc.)?"
- "I found a game concept but no systems index. Have you decomposed the concept into individual systems yet, or should we run `/map-systems`?"

### 4. Generate Stage Report

Use template: `.ags/templates/project-stage-report.md`

**Report structure**:
```markdown
# Project Stage Analysis

**Date**: [date]
**Stage**: [Concept/Systems Design/Technical Setup/Pre-Production/Production/Polish/Release]
**Stage Confidence**: [PASS — clearly detected / CONCERNS — ambiguous signals / FAIL — critical gaps block progress]

## Completeness Overview
- Design: [X%] ([N] docs, [gaps])
- Code: [X%] ([N] files, [systems])
- Architecture: [X%] ([N] ADRs, [gaps])
- Production: [X%] ([status])
- Tests: [X%] ([coverage estimate])

## Gaps Identified
1. [Gap description + clarifying question]
2. [Gap description + clarifying question]

## Recommended Next Steps
[Priority-ordered list based on stage and role]
```

### 5. Role-Filtered Recommendations (Optional)

If user provided a role argument (e.g., `/project-stage-detect programmer`):

**Programmer**:
- Focus on architecture docs, test coverage, missing ADRs
- Code-to-docs gaps

**Designer**:
- Focus on GDD completeness, missing design sections

**Producer**:
- Focus on sprint plans, milestone tracking, roadmap
- Cross-team coordination docs

**General** (no role):
- Holistic view of all gaps
- Highest-priority items across domains

### 6. Request Approval Before Writing

**Collaborative protocol**:
```
I've analyzed your project. Here's what I found:

[Show summary]

Gaps identified:
1. [Gap 1 + question]
2. [Gap 2 + question]

Recommended next steps:
- [Priority 1]
- [Priority 2]
- [Priority 3]

May I write the full stage analysis to .ags/project/project-stage-report.md?
```

Wait for user approval before creating the file.

---

## Example Usage

```bash
# General project analysis
/project-stage-detect

# Programmer-focused analysis
/project-stage-detect programmer

# Designer-focused analysis
/project-stage-detect designer
```

---

## Follow-Up Actions

After generating the report, suggest relevant next steps:

- **Concept exists but no systems index?** → `/map-systems` to decompose into systems
- **Missing design docs?** → `/reverse-document design Assets/Scripts/[system]`
- **Missing architecture docs?** → `/architecture-decision` or `/reverse-document architecture`
- **No sprint plan?** → `/sprint-plan`
- **Approaching milestone?** → `/milestone-review`

---

## Collaborative Protocol

1. **Question first** — ask about gaps, don't assume
2. **Present options** — "Should I create X, or is it tracked elsewhere?"
3. **User decides** — wait for direction
4. **Show draft** — display report summary
5. **Get approval** — "May I write to .ags/project/project-stage-report.md?"

**NEVER** silently write files. **ALWAYS** show findings and ask before creating artifacts.
