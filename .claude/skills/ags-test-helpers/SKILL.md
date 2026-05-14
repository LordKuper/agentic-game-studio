---
name: ags-test-helpers
description: "Generate engine-specific test helper libraries for the project's test suite. Reads existing test patterns and produces tests/helpers/ with assertion utilities, factory functions, and mock objects tailored to the project's systems. Reduces boilerplate in new test files."
argument-hint: "[system-name | all | scaffold]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, AskUserQuestion
---

**Language**: Talk to user in language from `.ags/project/user-interaction.md`. Fall back to English if file missing. Files on disk always English per `.ags/rules/user-interaction.md`.

# Test Helpers

Generates `tests/helpers/` library tailored to project's engine, language, and
systems. Abstracts common setup/teardown/assertion patterns — less boilerplate,
more assertions.

**Output:** `tests/helpers/` directory with engine-specific helper files

**When to run:**
- After `/test-setup` scaffolds the framework (first time)
- When multiple test files repeat the same setup boilerplate
- When starting to write tests for a new system

---

## 0. Prerequisites

| Artifact | Created by | If missing |
|---|---|---|
| `tests/` directory | `/ags-test-setup` | STOP. "Test framework not set up. Run `/ags-test-setup` first." |
| `.ags/rules/technical-preferences.md` | `/ags-setup-engine` | STOP. "Engine not configured." |

If STOP triggers, exit verdict **BLOCKED**.

---

## 1. Parse Arguments

**Modes:**
- `/test-helpers [system-name]` — generate helpers for a specific system
  (e.g., `/test-helpers combat`)
- `/test-helpers all` — generate helpers for all systems with test files
- `/test-helpers scaffold` — generate only the base helper library (no
  system-specific helpers); use this on first run
- No argument — run `scaffold` if no helpers exist, else `all`

---

## 2. Detect Engine and Language

Read `.ags/rules/technical-preferences.md` and extract:
- `Engine:` value
- `Language:` value
- `Framework:` from the Testing section

If engine is not configured: "Engine not configured. Run `/ags-setup-engine` first."

---

## 3. Load Existing Test Patterns

Scan the test directory for patterns already in use:

```
Glob pattern="tests/**/*_test.*" (all test files)
```

For a representative sample (up to 5 files), read the test files and extract:
- Setup patterns (how `before_each` / `setUp` / fixtures are written)
- Common assertion patterns (what is being asserted most often)
- Object creation patterns (how game objects or scenes are instantiated in tests)
- Mock/stub patterns (how dependencies are replaced)

This ensures generated helpers match the project's existing style, not a
generic template.

Also read:
- `design/gdd/systems-index.md` — to know which systems exist
- In-scope GDD(s) — to understand what data types and values need testing
- `design/architecture/tr-registry.yaml` — to map requirements to tested systems

---

## 4. Generate Engine-Specific Helpers

### Unity (NUnit / C#)

**Base helper** (`tests/helpers/GameAssertions.cs`):

```csharp
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Game-specific assertion utilities for [Project Name] tests.
/// Extends NUnit's Assert with domain-specific helpers.
/// </summary>
public static class GameAssertions
{
    /// <summary>
    /// Assert a value is within an inclusive range [min, max].
    /// Use for any formula output defined in GDD Formulas sections.
    /// </summary>
    public static void AssertInRange(float value, float min, float max, string label = "value")
    {
        Assert.That(value, Is.InRange(min, max),
            $"{label} ({value:F2}) is outside expected range [{min:F2}, {max:F2}]");
    }

    /// <summary>Assert a UnityEvent or C# event was raised during an action.</summary>
    public static void AssertEventRaised(ref bool wasCalled, System.Action action, string eventName)
    {
        wasCalled = false;
        action();
        Assert.IsTrue(wasCalled, $"Expected event '{eventName}' to be raised, but it was not.");
    }

    /// <summary>Assert a component exists on a GameObject.</summary>
    public static void AssertHasComponent<T>(GameObject obj) where T : Component
    {
        var component = obj.GetComponent<T>();
        Assert.IsNotNull(component,
            $"Expected GameObject '{obj.name}' to have component {typeof(T).Name}.");
    }
}
```

**Factory helper** (`tests/helpers/GameFactory.cs`):

```csharp
using UnityEngine;

/// <summary>
/// Factory methods for creating minimal test objects without loading scenes.
/// </summary>
public static class GameFactory
{
    /// <summary>Create a minimal GameObject with a named component for testing.</summary>
    public static GameObject MakeGameObject(string name = "TestObject")
    {
        var go = new GameObject(name);
        return go;
    }

    /// <summary>
    /// Create a ScriptableObject of type T for data-driven tests.
    /// Dispose with Object.DestroyImmediate after test.
    /// </summary>
    public static T MakeScriptableObject<T>() where T : ScriptableObject
    {
        return ScriptableObject.CreateInstance<T>();
    }
}
```

---

## 5. Generate System-Specific Helpers

For `[system-name]` or `all` modes, generate a helper per system:

Read the system's GDD to extract:
- Data types (entity types, component names)
- Formula variables and their bounds
- Common test scenarios mentioned in Edge Cases

Generate `tests/helpers/[system]_factory.[ext]` with factory functions
specific to that system's objects.

Example pattern for a `combat` system (Unity / C#):

```csharp
// Factory and assertion helpers for Combat system tests.
// Generated by /test-helpers combat on [date].
// Based on: design/gdd/combat.md

using NUnit.Framework;
using UnityEngine;

public static class CombatTestFactory
{
    public const float DamageMin = 0f;
    public const float DamageMax = 999f; // From GDD: damage formula upper bound

    /// <summary>Create a minimal attacker GameObject for damage formula tests.</summary>
    public static GameObject MakeAttacker(float attack = 10f, float critChance = 0f)
    {
        var go = new GameObject("Attacker");
        // attach test stub components / set fields here
        return go;
    }

    /// <summary>Create a minimal target GameObject for damage receive tests.</summary>
    public static GameObject MakeTarget(float defense = 0f, float health = 100f)
    {
        var go = new GameObject("Target");
        return go;
    }

    /// <summary>Assert damage output is within GDD-specified bounds.</summary>
    public static void AssertDamageInBounds(float damage)
    {
        GameAssertions.AssertInRange(damage, DamageMin, DamageMax, "damage");
    }
}
```

---

## 6. Write Output

Present a summary of what will be created:

```
## Test Helpers to Create

Base helpers (engine: [engine]):
- tests/helpers/game_assertions.[ext]
- tests/helpers/game_factory.[ext]
[engine-specific extras]

System helpers ([mode]):
- tests/helpers/[system]_factory.[ext]  ← from [system] GDD
```

Ask: "May I write these helper files to `tests/helpers/`?"

**Never overwrite existing files.** If a file already exists, report:
"Skipping `[path]` — already exists. Remove the file manually if you want it
regenerated."

After writing: Verdict: **COMPLETE** — helper files created.

"Helper files created. To use them in a Unity test, add a `using` directive or reference the helper assembly definition (e.g. `Tests.Helpers.asmdef`)."

---

## Collaborative Protocol

- **Never overwrite existing helpers** — they may contain hand-written
  customisations. Only generate new files that don't exist yet
- **Generated code is a starting point** — the generated factory functions use
  metadata patterns for simplicity; adapt to the actual class structure once
  the code exists
- **Helpers should reflect the GDD** — bounds and constants in helpers should
  trace to GDD Formulas sections, not invented values
- **Ask before writing** — always confirm before creating files in `tests/`

## Next Steps

- Run `/test-setup` if the test framework has not been scaffolded yet.
- Use `/ags-dev-story` to implement stories — helpers reduce boilerplate in new test files.
- Run `/ags-skill-test` to validate other skills that may need helper coverage.
