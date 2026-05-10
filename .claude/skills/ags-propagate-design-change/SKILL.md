---
name: ags-propagate-design-change
description: "When a GDD is revised, scans all ADRs and the traceability index to identify which architectural decisions are now potentially stale. Produces a change impact report and guides the user through resolution."
argument-hint: "[path/to/changed-gdd.md]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Bash, Task
agent: technical-director
---

# Propagate Design Change

When GDD changes, ADRs written against it may be invalid. Finds every affected
ADR, compares ADR assumptions against current GDD, guides user through resolution.

**Usage:** `/ags-propagate-design-change design/gdd/combat-system.md`

---

## 0. Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| Changed GDD path argument | user | STOP. "Usage: `/ags-propagate-design-change <path-to-changed-gdd>`." |
| `design/architecture/architecture.md` | `/ags-create-architecture` | STOP. "No architecture doc. Run `/ags-create-architecture`." |
| `.ags/project/epics/index.md` | `/ags-create-epics` | WARN: cannot map change to epics. |

If STOP triggers, exit verdict **BLOCKED**.

---

## 1. Validate Argument

A GDD path argument is **required**. If missing, fail with:
> "Usage: `/ags-propagate-design-change design/gdd/[system].md`
> Provide the path to the GDD that was changed."

Verify the file exists. If not, fail with:
> "[path] not found. Check the path and try again."

---

## 1b. Document Boundary Check (mandatory — per `.ags/rules/review-workflow.md` § Document Boundary Check)

Before propagating, validate the changed GDD against `.ags/rules/document-boundaries.md`:

- front-matter `status:` valid (warn if `draft` — propagation runs but downstream skills will reject until approved).
- no tech-leak (class/namespace/library/perf-literals/data-schema).
- no GDD→ADR cite (forbidden — GDD must not reference ADR).
- no raw color/typography/spacing literals.
- entity ids cited from `design/registry/entities.yaml` only.

Delegation: invoke `/ags-consistency-check entity:<system>` (or `full`) on the changed GDD and merge Boundary Violations into the impact report. Hard violations (🔴 BOUNDARY) → ABORT propagation: "Fix boundary violations in changed GDD before propagating to ADRs."

After downstream ADRs identified, re-run boundary check on each affected ADR (`**GDD source**:` line, no concept-rule duplication from cited GDD). Record violations in Phase 6 Impact Report.

---

## 2. Read the Changed GDD

Read the current GDD in full.

---

## 3. Read the Previous Version

Run git to get the previous committed version:

```bash
git show HEAD:design/gdd/[filename].md
```

If the file has no git history (new file), report:
> "No previous version in git — this appears to be a new GDD, not a revision.
> Nothing to propagate."

If git returns the previous version, do a conceptual diff:
- Identify sections that changed (new rules, removed rules, modified formulas,
  changed acceptance criteria, changed tuning knobs)
- Identify sections that are unchanged
- Produce a change summary:

```
## Change Summary: [GDD filename]
Date of revision: [today]

Changed sections:
- [Section name]: [what changed — new rule, removed rule, formula modified, etc.]

Unchanged sections:
- [Section name]

Key changes affecting architecture:
- [Change 1 — likely to affect ADRs]
- [Change 2]
```

---

## 4. Load Architecture Inputs

Read all ADRs in `design/architecture/`:
- For each ADR, read the full file
- Extract the "GDD Requirements Addressed" table
- Note which GDD documents and requirement IDs each ADR references

Read `design/architecture/architecture-traceability.md` if it exists.

Report: "Loaded [N] ADRs. [M] reference [gdd filename]."

---

## 5. Impact Analysis

For each ADR that references the changed GDD:

Compare the ADR's "GDD Requirements Addressed" entries against the changed sections
of the GDD. For each referenced requirement:

1. **Locate the requirement** in the current GDD — does it still exist?
2. **Compare**: What did the GDD say when the ADR was written vs. what it says now?
3. **Assess the ADR decision**: Is the architectural decision still valid?

Classify each affected ADR as one of:

| Status | Meaning |
|--------|---------|
| ✅ **Still Valid** | The GDD change doesn't affect what this ADR decided |
| ⚠️ **Needs Review** | The GDD change may affect this ADR — human judgment needed |
| 🔴 **Likely Superseded** | The GDD change directly contradicts what this ADR assumed |

For each affected ADR, produce an impact entry:

```
### ADR-NNNN: [title]
Status: [Still Valid / Needs Review / Likely Superseded]

What the ADR assumed about this GDD:
  "[relevant quote from the ADR's GDD Requirements Addressed section]"

What the GDD now says:
  "[relevant quote from the current GDD]"

Assessment:
  [Explanation of whether the ADR decision is still valid, and why]

Recommended action:
  [Keep as-is | Review and update | Mark Superseded and write new ADR]
```

---

## 6. Present Impact Report

Present the full impact report to the user before asking for any action. Format:

```
## Design Change Impact Report
GDD: [filename]
Date: [today]
Changes detected: [N sections changed]
ADRs referencing this GDD: [M]

### Not Affected
[ADRs referencing this GDD whose decisions remain valid]

### Needs Review ([count])
[ADRs that may need updating]

### Likely Superseded ([count])
[ADRs whose assumptions are now contradicted]
```

---

## 6b. Director Gate — Technical Impact Review

Spawn `technical-director` via Task using gate **TD-CHANGE-IMPACT** (`.ags/rules/director-gates.md`).

Pass: the full Design Change Impact Report from Phase 6 (change summary, all affected ADRs with their Still Valid / Needs Review / Likely Superseded classifications, and recommended actions).

The technical-director reviews whether:
- The impact classifications are correct (no ADRs under-classified)
- The recommended actions are architecturally sound
- Any cascading effects on other ADRs or systems were missed

Apply the verdict:
- **APPROVE** → proceed to Phase 7 resolution workflow
- **CONCERNS** → surface the specific ADRs or recommendations flagged; use `AskUserQuestion` with options: `Revise the impact assessment` / `Accept with noted concerns` / `Discuss further`
- **REJECT** → do not proceed to resolution; re-analyze the impact before continuing

---

## 7. Resolution Workflow

For each ADR marked "Needs Review" or "Likely Superseded", ask the user what to do:

Ask for each ADR in turn:
> "ADR-NNNN ([title]) — [status]. What would you like to do?"
> Options:
> - "Mark Superseded (I'll write a new ADR)" — updates ADR status line to `Superseded by: [pending]`
> - "Update in place (minor revision)" — opens the ADR for editing; note what to revise
> - "Keep as-is (the change doesn't actually affect this decision)"
> - "Skip for now (revisit later)"

For ADRs marked **Superseded**:
- Update the ADR's Status field: `Superseded by ADR-[next number] (pending — see change-impact-[date]-[system].md)`
- Ask: "May I update the status in [ADR filename]?"

---

## 8. Update Traceability Index

If `design/architecture/architecture-traceability.md` exists:
- Add the changed GDD requirements to the "Superseded Requirements" table:

```markdown
## Superseded Requirements
| Date | GDD | Requirement | Changed To | ADRs Affected | Resolution |
|------|-----|-------------|------------|---------------|------------|
| [date] | [gdd] | [old requirement text] | [new requirement text] | ADR-NNNN | [Superseded/Updated/Valid] |
```

Ask: "May I update the traceability index?"

---

## 9. Output Change Impact Document

Ask: "May I write the change impact report to `design/architecture/change-impact-[date]-[system-slug].md`?"

The document contains:
- The change summary from step 3
- The full impact analysis from step 5
- Resolution decisions made in step 7
- List of ADRs that need to be written or updated

If user approved: Verdict: **COMPLETE** — change impact report saved.
If user declined: Verdict: **BLOCKED** — user declined write.

---

## 10. Follow-Up Actions

Based on the resolution decisions, suggest:

- **ADRs marked Superseded**: "Run `/ags-architecture-decision [title]` to write the
  replacement ADR. Then re-run `/ags-propagate-design-change` to verify coverage."
- **ADRs to update in place**: List the specific fields to update in each ADR
- **If many ADRs affected**: "Run `/ags-architecture-review` after all ADRs are updated
  to verify the full traceability matrix is still coherent."

---

## Collaborative Protocol

1. **Read silently** — compute full impact before presenting anything
2. **Show full report first** — let user see scope before asking for action
3. **Ask per-ADR** — don't batch; each affected ADR may need different treatment
4. **Ask before writing** — confirm before modifying any file
5. **Non-destructive** — never delete ADR content; only add "Superseded by" notes
