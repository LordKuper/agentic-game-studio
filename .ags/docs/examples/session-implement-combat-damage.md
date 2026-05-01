# Example Session: Implementing Combat Damage Calculation

**Context:** Solo indie dev, week 4, implementing gameplay.

**Starting State:**
- `design/gdd/combat-system.md` exists, reviewed
- Unity 6000.0.30f1 set up with basic scene
- No gameplay code yet
- Sprint goal: "Implement core combat damage calculation"

**Session Duration:** ~30 min
**Turns:** 10
**Outcome:** Working damage calculator with tests, ready for integration

---

## Turn 1: User Requests Implementation

**User:**
> Implement the damage calculation system from design/gdd/combat-system.md. The formula is in section 4.

## Turn 2: Agent Reads Design, Asks Clarifying Questions

**Agent (gameplay-programmer):**
> Read `design/gdd/combat-system.md`. Damage formula:
>
> ```
> final_damage = base_damage * (1 + attack_stat * 0.1) * crit_multiplier * type_effectiveness * (1 - target_defense)
> ```
>
> Need clarification on details not in doc:
>
> **Architecture:**
> 1. `DamageCalculator` static utility (no state) or singleton MonoBehaviour?
> 2. Where does `base_damage` come from? (Weapon ScriptableObject? Character stats? Ability data?)
> 3. Where do `type_effectiveness` values live? (Per weapon? Per damage type? Separate file?)
>
> **Spec Ambiguities:**
> 4. Should `attack_stat` be clamped? (Prevents exponential bugs)
> 5. Doc says "crits do 2x damage" but no rounding. `floor()`, `ceil()`, `round()`?
> 6. What if `target_defense` >= 1.0? Negative damage or clamp to 0?
>
> **Integration:**
> 7. No design doc for Health/HP system. Create minimal one, or coming separately?
>
> Clarify so I implement right first time.

## Turn 3: User Resolves Ambiguities

**User:**
> 1. Static utility - no state needed
> 2. Base damage from Weapon ScriptableObject. I'll create a simple Weapon class.
> 3. Type effectiveness in a separate data file `assets/data/combat_damage.json`
> 4. Yes, clamp attack_stat to 0-100
> 5. Use `floor()` to round down to integers
> 6. Clamp damage to minimum of 1 (always do at least 1 damage, even with 100% defense)
> 7. Create a minimal health component - just current_hp, max_hp, and take_damage() method

## Turn 4: Agent Proposes Architecture

**Agent:**
> Got it. Proposed architecture:
>
> **Files I'll create:**
> ```
> Assets/Scripts/Gameplay/Combat/DamageCalculator.cs   # Static damage formula
> Assets/Scripts/Gameplay/Combat/HealthComponent.cs    # HP tracking MonoBehaviour
> Assets/Scripts/Gameplay/Combat/Weapon.cs             # Weapon ScriptableObject
> assets/data/combat_damage.json                       # Type effectiveness values
> design/gdd/health-system.md                          # Minimal health doc
> tests/Combat/DamageCalculatorTests.cs                # Unit tests
> ```
>
> **Architecture Details:**
>
> **DamageCalculator** (static):
> ```csharp
> namespace Game.Gameplay.Combat
> {
>     public static class DamageCalculator
>     {
>         public static int CalculateDamage(
>             Dictionary<string, float> attackerStats,
>             Weapon weapon,
>             Dictionary<string, float> defenderStats,
>             bool isCrit = false)
>         {
>             // Implementation here
>             return Mathf.FloorToInt(finalDamage);
>         }
>     }
> }
> ```
>
> **HealthComponent** (MonoBehaviour):
> ```csharp
> public class HealthComponent : MonoBehaviour
> {
>     public event Action<int, int> HealthChanged;
>     public event Action Died;
>
>     [SerializeField] private int maxHp = 100;
>     private int currentHp;
>
>     public void TakeDamage(int amount)
>     {
>         currentHp -= amount;
>         if (currentHp <= 0)
>             Died?.Invoke();
>     }
> }
> ```
>
> **Weapon** (ScriptableObject):
> ```csharp
> [CreateAssetMenu(menuName = "Combat/Weapon")]
> public class Weapon : ScriptableObject
> {
>     [SerializeField] private float baseDamage;
>     [SerializeField] private string damageType; // "physical", "fire", "ice", etc.
> }
> ```
>
> **combat_damage.json**:
> ```json
> {
>   "crit_multiplier": 2.0,
>   "attack_stat_min": 0,
>   "attack_stat_max": 100,
>   "type_effectiveness": {
>     "physical": { "armored": 0.5, "flesh": 1.0, "crystal": 1.5 },
>     "fire": { "armored": 1.0, "flesh": 1.2, "crystal": 0.3 }
>   }
> }
> ```
>
> **Question:** `DamageCalculator` easily testable (pure function, no deps). `HealthComponent` uses C# events for VFX/SFX. Match expectations, or change before impl?

## Turn 5: User Approves with Minor Change

**User:**
> Looks good, but change `attacker_stats` and `defender_stats` to proper typed dictionaries or classes. I don't want untyped Dictionary bags floating around.

## Turn 6: Agent Refines Architecture

**Agent:**
> Type safety important. Creating `CharacterStats` ScriptableObject:
>
> ```csharp
> [CreateAssetMenu(menuName = "Combat/CharacterStats")]
> public class CharacterStats : ScriptableObject
> {
>     [SerializeField] public int attackStat = 0;     // 0-100
>     [SerializeField] public float defense = 0.0f;   // 0.0-1.0 (0-100% mitigation)
>     [SerializeField] public string armorType = "flesh"; // for type effectiveness lookup
> }
> ```
>
> `DamageCalculator.CalculateDamage()` signature:
> ```csharp
> public static int CalculateDamage(
>     CharacterStats attacker,
>     Weapon weapon,
>     CharacterStats defender,
>     bool isCrit = false);
> ```
>
> Cleaner. Proceed with impl?

## Turn 7: User Approves Implementation

**User:**
> Yes, implement it.

## Turn 8: Agent Implements and Reports

**Agent:**
> Done. Created:
>
> **Files:**
> - ✅ `Assets/Scripts/Gameplay/Combat/DamageCalculator.cs` (82 lines)
> - ✅ `Assets/Scripts/Gameplay/Combat/HealthComponent.cs` (45 lines)
> - ✅ `Assets/Scripts/Gameplay/Combat/Weapon.cs` (12 lines)
> - ✅ `Assets/Scripts/Gameplay/Combat/CharacterStats.cs` (16 lines)
> - ✅ `assets/data/combat_damage.json` (validated JSON)
> - ✅ `design/gdd/health-system.md` (minimal 8-section doc)
>
> **Rules Enforcement:**
>
> `gameplay-code` rule flagged 2 issues during impl:
> 1. ❌ `critMultiplier` hardcoded `2.0f` in DamageCalculator.cs
>    - **Fixed:** Moved to `combat_damage.json`, loaded from config
> 2. ❌ `HealthComponent` had direct `Debug.Log()` debug
>    - **Fixed:** Removed, added events instead
>
> `data-files` rule validated:
> - ✅ `combat_damage.json` valid JSON
> - ✅ Naming matches `[system]_[name].json`
> - ✅ All values have comment docs
>
> **Notes:**
> - Attack stat clamped in `CalculateDamage()` before use
> - Damage min 1 (per requirement)
> - Type effectiveness defaults 1.0 if combo not found
> - All numbers loaded from `combat_damage.json` for tuning
>
> **Manual Test:**
> - Base 10, attack_stat 50 → Damage: 15 ✓
> - Same + crit → Damage: 30 ✓
> - 50% defense → Damage: 15 (before) → 7 (after) ✓
> - 100% defense → Damage: 1 (clamped) ✓
>
> **Next Steps - Your Choice:**
> A) Write formal unit tests now (recommended - verification-driven)
> B) Review code first via `/ags-code-review Assets/Scripts/Gameplay/Combat/DamageCalculator.cs`
> C) Skip tests, integrate into combat
>
> What?

## Turn 9: User Requests Tests

**User:**
> A - write unit tests. Cover all the edge cases.

## Turn 10: Agent Writes Tests, Completes

**Agent:**
> Tests created: `tests/Combat/DamageCalculatorTests.cs`
>
> **Test Coverage (8 tests):**
> ```
> ✓ Damage_BasicCalculation() - Verify formula with known values
> ✓ Damage_WithCrit() - Critical hits double damage
> ✓ Damage_WithDefense() - Defense reduces damage correctly
> ✓ Damage_TypeEffectiveness() - Type modifiers apply
> ✓ Damage_MinClamp() - Never goes below 1
> ✓ Damage_AttackStatClamp() - Attack stat clamped to 0-100
> ✓ Damage_MissingTypeCombo() - Defaults to 1.0 if not in config
> ✓ Damage_Rounding() - Verifies Mathf.FloorToInt() behavior
> ```
>
> **Results:**
> ```
> Running tests/Combat/DamageCalculatorTests.cs...
> ✓ All 8 tests PASSED
> Execution time: 12ms
> ```
>
> **Self-Check:**
> - ✅ All tests follow `[System]_[Scenario]_[Expected]` naming
> - ✅ Arrange/Act/Assert structure
> - ✅ No external state deps
> - ✅ Each test independent, any order
>
> **Ready for integration!**
>
> Files to commit:
> ```bash
> git add Assets/Scripts/Gameplay/Combat/*.cs
> git add assets/data/combat_damage.json
> git add design/gdd/health-system.md
> git add tests/Combat/DamageCalculatorTests.cs
> git commit -m "Implement damage calculation system per design/gdd/combat-system.md"
> ```
>
> `validate-commit` hook checks:
> - ✅ Design doc referenced
> - ✅ JSON validity
> - ✅ No hardcoded values (already fixed)
>
> Commit?

---

## Session Summary

**What Worked:**
- Agent read design first, found 7 ambiguities upfront
- Didn't guess — asked every unclear point
- Full architecture proposal before code
- Type safety feedback incorporated immediately
- Rules caught issues, agent fixed transparently
- Tests written proactively (verification-driven)
- Agent offered next-step options, didn't assume

**Key Collaborative Moments:**
- Turn 2: 7 spec gaps, asked instead of guessing
- Turn 4: Full architecture proposal before impl
- Turn 5: User type safety request, agent adapted
- Turn 8: Transparent rule violation report + fixes
- Turn 10: AC verified with tests

**Files Created:**
- 4 source files (DamageCalculator, HealthComponent, Weapon, CharacterStats)
- 1 config (combat_damage.json)
- 1 design doc (health-system.md)
- 1 test (DamageCalculatorTests.cs)

**Context Usage:** ~25%
**User Satisfaction:** High — ambiguities resolved early, no rework
