# Skill Test Spec: /[skill-name]

## Skill Summary

[One paragraph: what skill does, when to use, what it produces. Include primary output artifact, verdict format, pipeline stage.]

---

## Static Assertions (Structural)

Verified automatically by `/skill-test static` — no fixture needed.

- [ ] Has required frontmatter: `name`, `description`, `argument-hint`, `user-invocable`, `allowed-tools`
- [ ] Has ≥2 phase headings (## Phase N or numbered ## sections)
- [ ] Contains verdict keywords: [list expected — PASS, FAIL, CONCERNS]
- [ ] Contains "May I write" collaborative protocol language (if writes files)
- [ ] Has next-step handoff at end

---

## Test Cases

### Case 1: Happy Path — [description]

**Fixture:** [Assumed project state. Files exist? Contents? E.g., "game-concept.md exists with all 8 required sections complete. systems-index.md exists. All MVP GDDs present + reviewed."]

**Input:** `/[skill-name] [args]`

**Expected behavior:**
1. [Phase 1 — what skill reads/checks]
2. [Phase 2 — what skill evaluates]
3. [Phase N — what skill outputs]

**Assertions:**
- [ ] Skill reads [specific file] before output
- [ ] Output includes verdict keyword [PASS/FAIL/etc.]
- [ ] Output lists [specific content] from fixture
- [ ] Skill asks approval before writing

---

### Case 2: Failure Path — [description, e.g., "Missing required artifact"]

**Fixture:** [Failure state. E.g., "game-concept.md missing. No files in design/gdd/."]

**Input:** `/[skill-name] [args]`

**Expected behavior:**
1. [Phase 1: detects missing file]
2. [Phase 2: surfaces gap, doesn't assume OK]
3. [Output: FAIL or BLOCKED with specific blocker named]

**Assertions:**
- [ ] Does NOT output PASS when fixture incomplete
- [ ] Names specific missing artifact
- [ ] Suggests remediation ("Run /[other-skill]")
- [ ] Does not create files to fill gap without asking

---

### Case 3: Edge Case — [description, e.g., "No argument provided"]

**Fixture:** [Project file state]

**Input:** `/[skill-name]` (no argument)

**Expected behavior:**
1. [What skill does without arguments]

**Assertions:**
- [ ] [assertion]

---

## Protocol Compliance

- [ ] Uses "May I write" before all file writes
- [ ] Presents findings before asking write approval
- [ ] Ends with recommended next step or follow-up skill
- [ ] Never auto-creates files without explicit approval
- [ ] Does not skip phases or jump straight to verdict

---

## Coverage Notes

[What's intentionally NOT tested + why. Examples:
- "Case 3 (all-mode) not covered — too many checks for single spec. Test each sub-mode individually."
- "DB integration path not covered — requires live environment."
- "Corrupted YAML edge cases deferred to future spec."]
