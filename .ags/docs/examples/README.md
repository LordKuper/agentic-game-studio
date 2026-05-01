# Collaborative Session Examples

End-to-end session transcripts. Show collaborative workflow: agents ask, present options, wait for approval. No autonomous content gen.

---

## Visual Reference

New users start here:
[Skill Flow Diagrams](skill-flow-diagrams.md) — visual maps of 7 phases + skill chains.

---

## Available Examples

### CORE WORKFLOW

### [Skill Flow Diagrams](skill-flow-diagrams.md)
**Type:** Visual Reference
**Complexity:** All levels

Full pipeline (zero to ship). Detailed chains: design-system, story lifecycle, UX, brownfield. **Start here for big picture.**

---

### [Session: Authoring a GDD with /ags-design-system](session-design-system-skill.md)
**Type:** Design (skill-driven)
**Skill:** `/ags-design-system`
**Duration:** ~60 min (14 turns)
**Complexity:** Medium

**Scenario:**
Dev runs `/ags-design-system movement` after `/ags-map-systems`. Skill loads context from concept + dep GDDs, runs feasibility pre-check, walks 8 GDD sections one at a time. Draft → approve → write each before next.

**Key Moments:**
- Feasibility pre-check flags engine version risks (Unity 6000.0.30f1)
- Incremental writing: each section on disk after approval
- Session crash mid-section 5 → agent resumes from first empty section
- Dep signals (stamina, inventory) surfaced in Dependencies section
- Explicit handoff: "run `/ags-design-review` before next system"

**Learn:**
- How `/ags-design-system` differs from "write a GDD"
- Section-by-section cycle prevents 30k-token context bloat
- Incremental writing survives crashes
- Skill surfaces downstream dep contracts

---

### [Session: Full Story Lifecycle](session-story-lifecycle.md)
**Type:** Full Workflow
**Skills:** `/ags-story-readiness` → implementation → `/ags-story-done`
**Duration:** ~50 min (13 turns)
**Complexity:** Medium

**Scenario:**
Dev picks story from sprint backlog. `/ags-story-readiness` catches roll-direction ambiguity pre-code. After implementation, `/ags-story-done` verifies 9 ACs, identifies 2 deferred (inventory not integrated), closes story with notes.

**Key Moments:**
- `/ags-story-readiness` catches ambiguity Turn 2 — resolved before impl
- ADR status check: story BLOCKED if ADR Proposed
- Manifest version check: story guidance not drifted
- Deferred criteria tracked (not lost) when integration impossible
- `sprint-status.yaml` updated at close, next ready story surfaced

**Learn:**
- Why `/ags-story-readiness` prevents late ambiguity
- Deferred criteria (COMPLETE WITH NOTES vs BLOCKED)
- TR-ID refs prevent false deviation flags
- Full loop: backlog → implemented → closed

---

### [Session: Gate Check and Phase Transition](session-gate-check-phase-transition.md)
**Type:** Phase Gate
**Skill:** `/ags-gate-check`
**Duration:** ~20 min (7 turns)
**Complexity:** Low

**Scenario:**
Dev finishes Systems Design, runs `/ags-gate-check`. Gate finds 6 MVP GDDs complete, cross-review passed with 1 low concern. Gate passes, `stage.txt` updated, agent gives ordered checklist for Technical Setup.

**Key Moments:**
- Gate validates artifacts AND internal completeness (8 sections per GDD)
- CONCERNS ≠ FAIL: low cross-review note passes
- stage.txt update changes what `/ags-help`, `/ags-sprint-status`, all skills see
- Cross-review concern surfaces as concrete next ADR
- Next-phase checklist specific + ordered, not generic

**Learn:**
- What gate check actually validates (not just file existence)
- PASS/CONCERNS/FAIL verdicts
- Why stage.txt = phase authority
- What changes after phase transition

---

### [Session: UX Pipeline — /ux-design → /ux-review → /team-ui](session-ux-pipeline.md)
**Type:** UX Pipeline
**Skills:** `/ux-design`, `/ux-review`, `/team-ui`
**Duration:** ~90 min (16 turns)
**Complexity:** Medium-High

**Scenario:**
Dev designs HUD + inventory screen. `/ux-design` reads player journey + GDDs to ground decisions in player emotional state. `/ux-review` catches blocking accessibility gap (no keyboard alt to drag-drop) + advisory colorblind issue. After fix, `/team-ui` accepts handoff.

**Key Moments:**
- HUD philosophy choice (diegetic / persistent / tactical) grounded in genre
- `/ux-review` distinguishes BLOCKING (stops handoff) vs ADVISORY (visual pass)
- Accessibility caught before impl, not in QA
- Keyboard alt added in 1 turn; review re-runs, passes
- `/team-ui` checks for passing `/ux-review` before visual design

**Learn:**
- How `/ux-design` uses player journey context
- What `/ux-review` checks (not just "spec exists?")
- HUD doc (`design/ux/hud.md`) vs per-screen specs
- Accessibility: design time vs impl time

---

### [Session: Brownfield Onboarding with /ags-adopt](session-adopt-brownfield.md)
**Type:** Brownfield Adoption
**Skill:** `/ags-adopt`
**Duration:** ~30 min (8 turns)
**Complexity:** Low-Medium

**Scenario:**
Dev has 3 months of code + rough design notes, nothing in right format. `/ags-adopt` audits format compliance (not just existence), classifies 4 gaps by severity, builds ordered 7-step migration plan, fixes BLOCKING gap (missing systems index) by inferring from codebase.

**Key Moments:**
- FORMAT audit: "file exists" ≠ "file has required structure"
- BLOCKING gap: missing systems index blocks 4+ skills
- Migration plan ordered: blocking → high → medium
- Systems index bootstrapped from code structure
- Retrofit mode vs new authoring: `/ags-design-system retrofit` fills gaps without overwrite

**Learn:**
- `/ags-adopt` vs `/ags-project-stage-detect`
- Format compliance check (section detection, not just presence)
- Brownfield onboarding without losing existing work
- When retrofit vs full authoring

---

### FOUNDATIONAL EXAMPLES

### [Session: Designing the Crafting System](session-design-crafting-system.md)
**Type:** Design
**Agent:** game-designer
**Duration:** ~45 min (12 turns)
**Complexity:** Medium

**Scenario:**
Solo dev designs crafting for Pillar 2 ("Emergent Discovery Through Experimentation"). Agent: Q&A → 3 options with game theory analysis → user mods → iterative GDD draft with approval each step.

**Key Moments:**
- Agent asks 5 clarifying Qs upfront
- 3 distinct options with pros/cons + MDA alignment
- User modifies recommended option, agent incorporates
- Edge case flagged proactively ("non-recipe combo?")
- Each section approved before next
- Explicit "May I write to [file]?"

**Learn:**
- How design agents ask about goals/constraints/refs
- Present options via theory (MDA, SDT, Bartle)
- Iterate section-by-section
- When to delegate to specialists

---

### [Session: Implementing Combat Damage Calculation](session-implement-combat-damage.md)
**Type:** Implementation
**Agent:** gameplay-programmer
**Duration:** ~30 min (10 turns)
**Complexity:** Low-Medium

**Scenario:**
User has design doc, wants damage calc implemented. Agent reads spec, finds 7 ambiguities, asks Qs, proposes architecture for approval, implements with rule enforcement, writes tests proactively.

**Key Moments:**
- Agent reads design doc, finds 7 ambiguities
- Architecture + code samples BEFORE impl
- User requests type safety, agent refines
- Rules catch hardcoded values, agent fixes transparently
- Tests written proactively (verification-driven)
- Agent offers next steps, doesn't assume

**Learn:**
- Impl agents clarify specs before code
- Propose architecture with code samples
- Rules enforce standards auto
- Handle spec gaps (ask, don't assume)
- Verification-driven dev

---

### [Session: Scope Crisis - Strategic Decision Making](session-scope-crisis-decision.md)
**Type:** Strategic Decision
**Agent:** creative-director
**Duration:** ~25 min (8 turns)
**Complexity:** High

**Scenario:**
Solo dev: Alpha in 2 weeks, crafting needs 3 weeks, investor demo make-or-break. Creative director gathers context, frames decision, presents 3 options with honest trade-offs, recommends but defers, documents with ADR + demo script.

**Key Moments:**
- Agent reads context docs first
- 5 Qs to understand constraints
- Frames decision (stakes, evaluation criteria)
- 3 options with risk + historical precedent
- Strong rec but explicit: "your call"
- Documents decision + demo script

**Learn:**
- How leadership agents frame strategic decisions
- Present options with trade-offs
- Use precedent + theory in recs
- Document decisions (ADRs)
- Cascade decisions to depts

---

### [Reverse Documentation Workflow](reverse-document-workflow-example.md)
**Type:** Brownfield Documentation
**Agent:** game-designer
**Duration:** ~20 min
**Complexity:** Low

**Scenario:**
Dev built skill tree, no design doc. Agent reads code, infers intent, asks about ambiguous decisions, produces retroactive GDD.

---

## What These Examples Demonstrate

All follow collaborative pattern:

```
Question → Options → Decision → Draft → Approval
```

> **Note:** Examples show collaborative pattern as conversational text. In practice, agents now use `AskUserQuestion` tool at decision points for structured pickers (labels, descriptions, multi-select). Pattern is **Explain → Capture**: agents explain in conversation, then present structured UI picker.

### Collaborative Behaviors:

1. **Agents Ask Before Assuming**
   - Design agents: goals, constraints, refs
   - Impl agents: clarify spec ambiguities
   - Leadership: gather context before recommending

2. **Agents Present Options, Not Dictates**
   - 2-4 options with pros/cons
   - Reasoning from theory, precedent, pillars
   - Recommendation + user decides

3. **Agents Show Work Before Finalizing**
   - Drafts section-by-section
   - Architecture before impl
   - Strategic analysis before decisions

4. **Agents Get Approval Before Writing Files**
   - Explicit "May I write to [file]?"
   - Multi-file: list all first
   - User says "Yes" before any file

5. **Agents Iterate on Feedback**
   - User mods incorporated immediately
   - No defensiveness on changes
   - Celebrate when user improves suggestion

---

## How to Use

### New Users:
Read examples BEFORE first session. Realistic expectations:
- Agents = consultants, not autonomous executors
- You make creative/strategic decisions
- Agents provide expert guidance + options

### For Specific Workflows:
- **New?** → skill-flow-diagrams.md first
- **First /ags-design-system?** → session-design-system-skill.md
- **Picking up story?** → session-story-lifecycle.md
- **Finishing phase?** → session-gate-check-phase-transition.md
- **Starting UI?** → session-ux-pipeline.md
- **Existing project?** → session-adopt-brownfield.md
- **Designing system (agent-driven)?** → session-design-crafting-system.md
- **Implementing code?** → session-implement-combat-damage.md
- **Strategic decisions?** → session-scope-crisis-decision.md

### For Training:
Walk through one example turn-by-turn. Show:
- Good questions
- Evaluating options
- Approve vs request changes
- Maintain creative control while leveraging AI

---

## Common Patterns Across Examples

### Turn 1-2: **Understand Before Acting**
- Agent reads context (docs, specs, constraints)
- Agent asks clarifying Qs
- No assumptions

### Turn 3-5: **Present Options with Reasoning**
- 2-4 distinct approaches
- Pros/cons each
- Theory/precedent
- Rec made, decision deferred

### Turn 6-8: **Iterate on Drafts**
- Show work incrementally
- Incorporate feedback
- Flag edge cases proactively

### Turn 9-10: **Approval and Completion**
- "May I write to [file]?"
- User: "Yes"
- Agent writes
- Agent offers next steps

---

## Try It Yourself

Exercise:

1. Pick a system (combat, inventory, progression, etc.)
2. Ask relevant agent to design or implement
3. Notice if agent:
   - Asks clarifying Qs upfront
   - Presents options with reasoning
   - Shows drafts before finalizing
   - Requests approval before writing files

If agent skips any:
> "Please follow the collaborative protocol from docs/COLLABORATIVE-DESIGN-PRINCIPLE.md"

---

## Additional Resources

- **Full Principle Doc:** [docs/COLLABORATIVE-DESIGN-PRINCIPLE.md](../COLLABORATIVE-DESIGN-PRINCIPLE.md)
- **Workflow Guide:** [docs/WORKFLOW-GUIDE.md](../WORKFLOW-GUIDE.md)
- **Agent Roster:** [.claude/agents/](../../.claude/agents/)
- **CLAUDE.md (Collaboration Protocol):** [CLAUDE.md](../../CLAUDE.md#collaboration-protocol)
