# Release Checklist: [Version] -- [Platform]

**Release Date**: [Target Date]
**Release Manager**: [Name]
**Status**: [ ] GO / [ ] NO-GO

---

## Build Verification

- [ ] Clean build succeeds on all target platforms
- [ ] No compiler warnings (zero-warning policy)
- [ ] Build version set correctly: `[version]`
- [ ] Build reproducible from tagged commit: `[commit hash]`
- [ ] Build size within budget: [actual] / [budget]
- [ ] All assets included + loading correctly
- [ ] No debug/dev features in release build

---

## Quality Gates

### Critical Bugs
- [ ] Zero S1 (Critical) open
- [ ] Zero S2 (Major) -- or documented exceptions:

| Bug ID | Description | Exception Rationale | Approved By |
| ---- | ---- | ---- | ---- |
| | | | |

### Test Coverage
- [ ] All critical path features tested + signed off
- [ ] Full regression passed: [pass rate]%
- [ ] Soak test passed (4+ hours continuous)
- [ ] Edge case testing complete

### Performance
- [ ] Target FPS met on min spec: [actual] / [target]
- [ ] Memory within budget: [actual] / [budget] MB
- [ ] Load times within budget: [actual] / [target] s
- [ ] No memory leaks over extended play (soak)
- [ ] No frame drops below [threshold] in normal gameplay

---

## Content Complete

- [ ] All placeholders replaced with final
- [ ] All player-facing text proofread
- [ ] All text loc-ready (no hardcoded strings)
- [ ] Loc complete for: [list locales]
- [ ] Audio mix finalized + approved
- [ ] Credits complete + accurate
- [ ] Legal notices + third-party attributions complete

---

## Platform: PC

- [ ] Min + recommended specs documented
- [ ] Kb+mouse fully functional
- [ ] Controller tested (Xbox, PS, generic)
- [ ] Resolution scaling: 1080p, 1440p, 4K, ultrawide
- [ ] Windowed, borderless, fullscreen working
- [ ] Graphics settings save + load
- [ ] Store SDK integrated + tested: [Steam/Epic/GOG]
- [ ] Achievements functional
- [ ] Cloud saves functional

## Platform: Console (if applicable)

- [ ] TRC/TCR/Lotcheck requirements met
- [ ] Platform controller prompts correct
- [ ] Suspend/resume works
- [ ] User switching handled
- [ ] Network loss handled gracefully
- [ ] Storage full handled
- [ ] Parental controls respected
- [ ] Cert submission prepared

---

## Store and Distribution

- [ ] Store metadata complete + proofread
- [ ] Screenshots current + meet requirements
- [ ] Trailer current
- [ ] Key art + capsule images final
- [ ] Age ratings: [ ] ESRB [ ] PEGI [ ] Other
- [ ] Legal: EULA, Privacy Policy, ToS
- [ ] Pricing configured all regions

---

## Launch Readiness

- [ ] Analytics/telemetry verified + receiving data
- [ ] Crash reporting configured: [service]
- [ ] Day-one patch prepared (if needed)
- [ ] On-call schedule set first 72 hours
- [ ] Community announcements drafted
- [ ] Press/influencer keys prepared
- [ ] Support team briefed on known issues
- [ ] Rollback plan documented + tested

---

## Sign-offs

| Role | Name | Status | Date |
| ---- | ---- | ---- | ---- |
| QA Lead | | [ ] Approved | |
| Technical Director | | [ ] Approved | |
| Producer | | [ ] Approved | |
| Creative Director | | [ ] Approved | |

---

## Final Decision

**GO / NO-GO**: ____________

**Rationale**: [Readiness summary. NO-GO → list specific blocking items + ETA.]

**Notes**: [Additional context, accepted risks, release conditions.]
