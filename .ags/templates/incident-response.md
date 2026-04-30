# Incident Response: [Incident Title]

**Severity**: [S1-Critical / S2-Major / S3-Moderate / S4-Minor]
**Status**: [Active / Mitigated / Resolved / Post-Mortem Complete]
**Detected**: [Date Time UTC]
**Resolved**: [Date Time UTC or ONGOING]
**Duration**: [Detection to resolution]
**Incident Commander**: [Name/Role]

---

## Impact Summary

[2-3 sentences: what players experienced. Player perspective, not technical.]

- **Players affected**: [count or %]
- **Platforms**: [PC / Console / Mobile / All]
- **Regions**: [All / specific]
- **Revenue impact**: [if applicable]

---

## Timeline

| Time (UTC) | Event | Action |
| ---- | ---- | ---- |
| [HH:MM] | Detected via [monitoring/player report/etc.] | Commander assigned |
| [HH:MM] | Root cause identified | [Brief cause] |
| [HH:MM] | Mitigation deployed | [What done] |
| [HH:MM] | Service restored / Fix confirmed | Monitor recurrence |
| [HH:MM] | All-clear declared | Post-mortem scheduled |

---

## Root Cause

### What Happened
[Technical description. Specific chain of events.]

### Why It Happened
[Systemic cause — why processes/tests/safeguards failed. More important than technical cause.]

### Contributing Factors
- [Factor 1 — e.g., "Insufficient load testing for new matchmaking"]
- [Factor 2 — e.g., "Alert threshold too high"]
- [Factor 3]

---

## Mitigation and Resolution

### Immediate Actions (during incident)
1. [Stop bleeding]
2. [Restore service]
3. [Verify resolution]

### Follow-Up Actions (after resolution)
1. [Permanent fix if immediate was workaround]
2. [Additional testing/monitoring added]
3. [Process changes preventing recurrence]

---

## Player Communication

### Initial Acknowledgment
*Sent: [Time] via [channel]*
> [Exact text of first public message]

### Status Updates
*Sent: [Time] via [channel]*
> [Each subsequent update]

### Resolution Notice
*Sent: [Time] via [channel]*
> [Fix announcement + compensation]

### Compensation (if applicable)
- **What**: [e.g., "500 premium currency + 24-hr XP boost"]
- **Who**: [all / affected only / players logged in during]
- **When**: [delivery + method]
- **Rationale**: [why appropriate for impact]

---

## Prevention

### What We Are Changing

| Action | Owner | Deadline | Status |
| ---- | ---- | ---- | ---- |
| [Preventive measure] | [Role] | [Date] | [TODO/Done] |
| [Add monitoring for X] | [Role] | [Date] | [TODO/Done] |
| [Add test coverage for Y] | [Role] | [Date] | [TODO/Done] |
| [Update runbook for Z] | [Role] | [Date] | [TODO/Done] |

### Process Improvements
- [Process change preventing similar]
- [Monitoring/alerting improvement]
- [Testing improvement]

---

## Lessons Learned

### What Went Well
- [Positive — e.g., "Detection fast due to alerts"]
- [Positive]

### What Went Poorly
- [Problem — e.g., "20 min to identify on-call person"]
- [Problem]

### Where We Got Lucky
- [Reduced impact by chance not design — hidden risks]

---

## Sign-Offs

- [ ] Technical Director — Root cause accurate, prevention sufficient
- [ ] QA Lead — Test coverage gaps addressed
- [ ] Producer — Timeline + communication reviewed
- [ ] Community Manager — Player communication reviewed

---

*Filed in `.ags/project/hotfixes/`. Linked from release notes for fix version.*
