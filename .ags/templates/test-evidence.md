# Test Evidence: [Story Title]

> **Story**: `[path to story]`
> **Story Type**: [Visual/Feel | UI]
> **Date**: [date]
> **Tester**: [who tested]
> **Build / Commit**: [version or git hash]

---

## What Was Tested

[One paragraph: feature/behaviour validated. Include AC numbers from story this evidence covers.]

**Acceptance criteria covered**: [AC-1, AC-2, AC-3]

---

## Acceptance Criteria Results

| # | Criterion (from story) | Result | Notes |
|---|----------------------|--------|-------|
| AC-1 | [exact criterion text] | PASS / FAIL | [observations] |
| AC-2 | [exact criterion text] | PASS / FAIL | |
| AC-3 | [exact criterion text] | PASS / FAIL | |

---

## Screenshots / Video

Captured evidence. Store in same directory or `.ags/project/qa/evidence/[story-slug]/`.

| # | Filename | Shows | AC |
|---|----------|-------|-----|
| 1 | `[filename.png]` | [description of what visible] | AC-1 |
| 2 | `[filename.png]` | | AC-2 |

*Video: note timestamp + what demonstrates.*

---

## Test Conditions

- **Game state at start**: [e.g., "fresh save, level 1, no items"]
- **Platform / hardware**: [Windows 11, GTX 1080, 1080p]
- **Framerate during test**: ["stable 60fps" or "~45fps — within budget"]
- **Special setup**: ["dev menu used to trigger state"]

---

## Observations

[Noteworthy items not causing FAIL but recorded. Minor jitter, frame dip under load, technically passes but felt off. Polish candidates.]

- [Observation 1]
- [Observation 2]

If nothing notable: *No significant observations.*

---

## Sign-Off

All three required before story COMPLETE via `/ags-story-done`. Visual/Feel needs designer or art-lead. UI needs UX lead or designer.

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Developer (implemented) | | | [ ] Approved |
| Designer / Art Lead / UX Lead | | | [ ] Approved |
| QA Lead | | | [ ] Approved |

**Any sign-off can be "Deferred — [reason]"** if person unavailable. Deferred resolved before sprint review.

---

*Template: `.ags/templates/test-evidence.md`*
*Used for: Visual/Feel + UI story evidence records*
*Location: `.ags/project/qa/evidence/[story-slug]-evidence.md`*
