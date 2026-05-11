---
name: ags-propagate-design-change
description: "When an upstream design document is revised (GDD, DESIGN.md, accessibility-requirements, technical-preferences), scans downstream artifacts (ADRs, art-bible sections, traceability index) to identify what is now potentially stale. Produces a change impact report and guides resolution."
argument-hint: "[path/to/changed-doc]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Bash, Task
agent: technical-director
---

# Propagate Design Change

When an upstream design document changes, downstream artifacts written against it may be invalid. Finds every affected downstream (ADRs, art-bible sections), compares assumptions against current upstream, guides user through resolution.

**Accepted upstream document kinds** (auto-detected from argument path):
- `design/gdd/<system>.md` → ADR impact analysis (Phase 5). Also triggers art-bible scan (Phase 5b) if any art-bible section depends on this GDD.
- `design/art/DESIGN.md` → art-bible scan (Phase 5b). Also surfaces UX/HUD-spec token references (Phase 5c).
- `design/accessibility-requirements.md` → art-bible Accessibility section + UX-spec scan (Phase 5b).
- `.ags/rules/technical-preferences.md` → art-bible Production section + ADR feasibility (Phases 5 + 5b).

**Usage:**
- `/ags-propagate-design-change design/gdd/combat-system.md`
- `/ags-propagate-design-change design/art/DESIGN.md`

---

## 0. Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| Changed upstream doc path argument | user | STOP. "Usage: `/ags-propagate-design-change <path-to-changed-doc>`." |
| `design/architecture/architecture.md` (only required when changed doc is a GDD) | `/ags-create-architecture` | If changed doc is non-GDD (DESIGN.md / accessibility / technical-preferences), skip architecture-related phases; continue with art-bible scan. If changed doc IS a GDD: STOP. "No architecture doc. Run `/ags-create-architecture`." |
| `design/art/ags-art-bible.html` (only required when changed doc has art-bible deps) | `/ags-art-bible` | If absent, skip Phase 5b silently. |
| `.ags/project/epics/index.md` | `/ags-create-epics` | WARN: cannot map change to epics. |

If STOP triggers, exit verdict **BLOCKED**.

---

## 1. Validate Argument and Classify Upstream Kind

A path argument is **required**. If missing, fail with:
> "Usage: `/ags-propagate-design-change <path-to-changed-doc>`
> Accepted: GDD under design/gdd/, design/art/DESIGN.md, design/accessibility-requirements.md, .ags/rules/technical-preferences.md."

Verify file exists. If not, fail with:
> "[path] not found. Check the path and try again."

**Classify** the path into a kind that drives which downstream phases run:

| Path pattern | Kind | Downstream phases |
|---|---|---|
| `design/gdd/*.md` (excluding `game-concept.md`, `systems-index.md`, `game-pillars.md`) | `gdd` | 3, 4, 5 (ADR impact), 5b (art-bible scan) |
| `design/art/DESIGN.md` | `design-md` | 5b (art-bible scan), 5c (UX/HUD-spec token scan) |
| `design/accessibility-requirements.md` | `accessibility` | 5b (art-bible accessibility + UX-spec) |
| `.ags/rules/technical-preferences.md` | `tech-prefs` | 5 (ADR feasibility), 5b (art-bible production) |
| `design/gdd/game-concept.md` | `concept` | 5b (art-bible identity + references + style) |
| Other | reject | STOP. "Unsupported upstream type." |

Set `$KIND` for use in later phases. Skip Phases 3 and 4 if `$KIND ≠ gdd|concept` — git diff against ADR-relevant fields makes no sense for non-GDD upstream. Skip Phase 5 (ADR impact) if `$KIND ∉ {gdd, tech-prefs}`.

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

## 5b. Art-bible Impact Analysis

Runs when `$KIND ∈ {gdd, design-md, accessibility, tech-prefs, concept}` and `design/art/ags-art-bible.html` exists.

1. Read `design/art/ags-art-bible.html`. Parse every `<section ... data-depends-on="..." data-checked-against="...">`.
2. For each section, parse `data-depends-on` on `|`. Locate entries whose path matches the changed upstream:
   - Exact path match — section depends on this file directly
   - `<changed-path>:<scope>` — section depends on a scope namespace inside the changed file (treat as match)
   - `<changed-path>#<section>` — section depends on a sub-section of the changed file (treat as match)
3. For each matched section:
   - Compute current `git hash-object <changed-path>` and compare against the section's stored hash for that dep entry
   - Hash matches → ✅ STILL VALID (provenance already matches current upstream — section was re-stamped after the change)
   - Hash differs → 🟠 STALE — section needs re-review
   - Missing `data-checked-against` → 🟡 PROVENANCE-MISSING — section authored before provenance system
4. **Token-reference impact** (`$KIND = design-md` only):
   - Grep changed DESIGN.md against its previous git version: `git diff HEAD -- design/art/DESIGN.md`
   - Extract added / removed / renamed tokens
   - For each removed/renamed token: grep art-bible for `\{<token-path>\}` references → **🔴 BROKEN REFERENCE** for every hit
   - For added tokens: surface as informational (art-bible may want to cite the new token)

Produce per-section impact entry:

```
### Art-bible section: <section-id> (<heading>)
Status: STILL VALID | STALE | PROVENANCE-MISSING | BROKEN REFERENCE
Depends-on entry matched: <upstream-path>[:scope|#anchor]
Stored hash: <hash> | Current hash: <hash>
Recommended action:
  STILL VALID → no action
  STALE → run `/ags-art-bible` retrofit on this section to re-review and restamp
  PROVENANCE-MISSING → run `/ags-art-bible` retrofit to stamp (no content change needed if section is still accurate)
  BROKEN REFERENCE → fix token references at: <art-bible:line numbers>
```

Append all entries to the impact report (Phase 6).

## 5c. UX/HUD-spec Token Scan

Runs when `$KIND = design-md`. Coarse pass — UX/HUD specs do not have data-depends-on; rely on token references in body.

1. Glob `design/ux/*.md`.
2. For each file, grep `\{(colors|typography|spacing|rounded|components)\.[a-z0-9-]+\}`.
3. For each token cited that was removed/renamed in DESIGN.md → **🔴 BROKEN REFERENCE** (file:line:token).
4. Surface in impact report under "UX/HUD-spec Token Impact".

No automated provenance restamp here — these specs do not carry hashes. User-driven fix.

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

Based on resolution decisions, suggest:

- **ADRs marked Superseded**: "Run `/ags-architecture-decision [title]` to write the
  replacement ADR. Then re-run `/ags-propagate-design-change` to verify coverage."
- **ADRs to update in place**: List the specific fields to update in each ADR
- **Art-bible sections marked STALE**: "Run `/ags-art-bible` in retrofit mode — skill detects out-of-date sections from `data-checked-against` mismatch and re-runs the authoring loop for each. Provenance is re-stamped automatically on re-approval."
- **Art-bible BROKEN REFERENCE**: "Fix token citations at flagged lines. Run `/ags-consistency-check` afterwards to confirm MISSING TOKEN findings clear."
- **UX/HUD-spec BROKEN REFERENCE**: "Edit the cited specs to use the renamed token (or remove if dropped). No skill auto-fixes — manual edit."
- **If many downstreams affected**: "Run `/ags-architecture-review` after all ADRs updated to verify traceability matrix coherent. Run `/ags-consistency-check` to verify art-bible token references clean."

---

## Collaborative Protocol

1. **Read silently** — compute full impact before presenting anything
2. **Show full report first** — let user see scope before asking for action
3. **Ask per-ADR** — don't batch; each affected ADR may need different treatment
4. **Ask before writing** — confirm before modifying any file
5. **Non-destructive** — never delete ADR content; only add "Superseded by" notes
