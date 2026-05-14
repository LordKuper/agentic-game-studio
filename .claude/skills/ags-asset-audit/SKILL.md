---
name: ags-asset-audit
description: "Audits game assets for compliance with naming conventions, file size budgets, format standards, and pipeline requirements. Identifies orphaned assets, missing references, and standard violations."
argument-hint: "[category|all]"
user-invocable: true
allowed-tools: Read, Glob, Grep, AskUserQuestion
# Read-only diagnostic skill — no specialist agent delegation needed
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

## Phase 0: Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `design/art/ags-art-bible.html` | `/ags-art-bible` | STOP. "No art bible. Run `/ags-art-bible` first." |
| `.ags/rules/technical-preferences.md` | `/ags-setup-engine` | STOP. "Naming conventions not configured. Run `/ags-setup-engine`." |
| `assets/` directory exists | engine init | STOP. "No `assets/` directory found." |

If STOP triggers, exit with verdict **BLOCKED — missing prerequisite**.

---

## Phase 1: Read Standards

Read the art bible or asset standards from the relevant design docs and the CLAUDE.md naming conventions.

---

## Phase 2: Scan Asset Directories

Scan the target asset directory using Glob:

- `assets/art/**/*` for art assets
- `assets/audio/**/*` for audio assets
- `assets/vfx/**/*` for VFX assets
- `assets/shaders/**/*` for shaders
- `assets/data/**/*` for data files

---

## Phase 3: Run Compliance Checks

**Naming conventions:**
- Art: `[category]_[name]_[variant]_[size].[ext]`
- Audio: `[category]_[context]_[name]_[variant].[ext]`
- All files must be lowercase with underscores

**File standards:**
- Textures: Power-of-two dimensions, correct format (PNG for UI, compressed for 3D), within size budget
- Audio: Correct sample rate, format (OGG for SFX, OGG/MP3 for music), within duration limits
- Data: Valid JSON/YAML, schema-compliant

**Orphaned assets:** Search code for references to each asset file. Flag any with no references.

**Missing assets:** Search code for asset references and verify the files exist.

---

## Phase 4: Output Audit Report

```markdown
# Asset Audit Report -- [Category] -- [Date]

## Summary
- **Total assets scanned**: [N]
- **Naming violations**: [N]
- **Size violations**: [N]
- **Format violations**: [N]
- **Orphaned assets**: [N]
- **Missing assets**: [N]
- **Overall health**: [CLEAN / MINOR ISSUES / NEEDS ATTENTION]

## Naming Violations
| File | Expected Pattern | Issue |
|------|-----------------|-------|

## Size Violations
| File | Budget | Actual | Overage |
|------|--------|--------|---------|

## Format Violations
| File | Expected Format | Actual Format |
|------|----------------|---------------|

## Orphaned Assets (no code references found)
| File | Last Modified | Size | Recommendation |
|------|-------------|------|---------------|

## Missing Assets (referenced but not found)
| Reference Location | Expected Path |
|-------------------|---------------|

## Recommendations
[Prioritized list of fixes]

## Verdict: [COMPLIANT / WARNINGS / NON-COMPLIANT]
```

This skill is read-only — it produces a report but does not write files.

---

## Phase 5: Next Steps

- Fix naming violations using the patterns defined in CLAUDE.md.
- Delete confirmed orphaned assets after manual review.
- Run `/ags-content-audit` to cross-check asset counts against GDD-specified requirements.

---

## Combined Review Loop (parallel external Codex)

Per `.ags/rules/review-workflow.md`. Audit phases run **in parallel** with external Codex inside one loop. Each iteration:

1. Resolve severity floor: iter 1-2 → keep all severities; iter 3-4 → critical/high; iter 5+ → critical only.
2. **Spawn in one message, in parallel**:
   - All internal reviewer Tasks (art-director + technical-artist).
   - For each flagged asset-spec under review: `/ags-external-review asset-spec [asset-spec-path] --embedded-parallel --iteration [N] --min-severity [floor]`. For orphan-asset bundles: one additional `custom` call with the flagged file list + art-bible reference. Codex unavailable → `skipped: codex-unavailable`; aggregator logs skip in decisions-log and continues with internal pool only.
3. Aggregator (`art-director`) merges findings from internal + Codex, drops nitpicks + below-floor.
4. **Loop exit**: filtered set empty → emit final verdict. Non-empty → surface aggregated kept findings, user resolves, N++, repeat.

No iteration cap. No user-confirm gate before external — it runs every iteration automatically. Record final iteration count in the verdict report and decisions-log entry. Codex reviews the source asset specs / bundles, NOT this audit report.
