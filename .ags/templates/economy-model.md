# Economy Model: [System Name]

*Created: [Date]*
*Owner: systems-designer*
*Status: [Draft / Balanced / Live]*

---

## Overview

[Resources, currencies, exchange systems covered. Player behaviors incentivized.]

---

## Currencies

| Currency | Type | Earn Rate | Sink Rate | Cap | Notes |
| ---- | ---- | ---- | ---- | ---- | ---- |
| [Gold] | Soft | [per hour] | [per hour] | [max or none] | [Primary transaction] |
| [Gems] | Premium | [per day F2P] | [varies] | [max] | [Premium, purchasable] |
| [XP] | Progression | [per action] | [level-up cost] | [none] | [Cannot trade] |

### Currency Rules
- [Rule 1 — e.g., "Soft has no cap, inflation controlled via sinks"]
- [Rule 2 — e.g., "Premium not convertible back to real money"]
- [Rule 3]

---

## Sources (Faucets)

| Source | Currency | Amount | Frequency | Conditions |
| ---- | ---- | ---- | ---- | ---- |
| [Quest completion] | Gold | [50-200] | [per quest] | [Scales with difficulty] |
| [Enemy drops] | Gold | [1-10] | [per kill] | [Modified by luck stat] |
| [Daily login] | Gems | [5] | [daily] | [Streak: +1 per consecutive] |
| [Achievement] | XP | [100-500] | [one-time] | [Per tier] |

---

## Sinks (Drains)

| Sink | Currency | Cost | Frequency | Purpose |
| ---- | ---- | ---- | ---- | ---- |
| [Equipment] | Gold | [100-5000] | [as needed] | [Power progression] |
| [Repair] | Gold | [10-100] | [per death] | [Death penalty, drain] |
| [Cosmetic shop] | Gems | [50-500] | [optional] | [Vanity, premium sink] |
| [Respec] | Gold | [1000] | [rare] | [Build experimentation tax] |

---

## Balance Targets

| Metric | Target | Rationale |
| ---- | ---- | ---- |
| Time to first meaningful purchase | [X min] | [Spending power early] |
| Hourly gold earn (mid-game) | [X gold/hr] | [Session length + purchase cadence] |
| Days to max level (F2P) | [X days] | [Retain without frustration] |
| Sink-to-source ratio | [0.7-0.9] | [Slight surplus = wealthy feel] |
| Premium F2P earn rate | [X/week] | [Buy something monthly, not everything] |

---

## Progression Curves

### Level XP Requirements
| Level | XP Required | Cumulative | Estimated Time |
| ---- | ---- | ---- | ---- |
| 1→2 | [100] | [100] | [10 min] |
| 5→6 | [500] | [1,500] | [2 hrs] |
| 10→11 | [1,500] | [7,500] | [8 hrs] |
| 20→21 | [5,000] | [50,000] | [40 hrs] |

*Formula*: `XP(n) = [e.g., 100 * n^1.5]`

### Item Price Scaling
*Formula*: `Price(tier) = [e.g., base_price * 2^(tier-1)]`

---

## Loot Tables

### [Drop Source]
| Item | Rarity | Drop Rate | Pity Timer | Notes |
| ---- | ---- | ---- | ---- | ---- |
| [Common] | Common | [60%] | [N/A] | [Always useful] |
| [Uncommon] | Uncommon | [25%] | [N/A] | [Noticeable upgrade] |
| [Rare] | Rare | [12%] | [10 drops] | [Build-defining] |
| [Legendary] | Legendary | [3%] | [30 drops] | [Game-changing celebration] |

### Pity System
[How pity works — prevents extreme bad luck streaks.]

---

## Economy Health Metrics

| Metric | Healthy Range | Warning | Action if Breached |
| ---- | ---- | ---- | ---- |
| Average player gold | [X-Y at level Z] | [>Y or <X] | [Adjust faucets/sinks] |
| Gold Gini coefficient | [<0.4] | [>0.5] | [Wealth too concentrated] |
| % at currency cap | [<5%] | [>10%] | [Raise cap or add sinks] |
| Premium conversion rate | [2-5%] | [<1% or >10%] | [Rebalance F2P earn] |
| Avg time between purchases | [X min] | [>Y min] | [Nothing worth buying] |

---

## Ethical Guardrails

- [No pay-to-win: premium can't buy gameplay power]
- [Pity timers on all random: guaranteed within X attempts]
- [Transparent drop rates displayed]
- [Spending limits for minor accounts]
- [No FOMO timers on essential items]

---

## Simulation Results

[Economy simulation results if available: wealth distribution, sink effectiveness, inflation rate.]

---

## Dependencies

- Depends on: [combat balance, quest design, crafting]
- Affects: [difficulty curve, retention, monetization]
- Coordinate with: `game-designer`, `producer`
