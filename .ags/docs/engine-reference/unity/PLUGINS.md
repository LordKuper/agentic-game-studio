# Unity 6.3 LTS — Optional Packages & Systems

**Last verified:** 2026-02-13

Index of optional packages/systems in Unity 6.3 LTS. NOT core engine. Common per game type.

---

## How to Use This Guide

**✅ Detailed Documentation Available** - See `plugins/` dir
**🟡 Brief Overview Only** - Links to official docs, WebSearch for details
**⚠️ Preview** - May break in future versions
**📦 Package Required** - Install via Package Manager

---

## Production-Ready Packages (Detailed Docs Available)

### ✅ Cinemachine
- **Purpose:** Virtual camera system (dynamic cameras, cutscenes, blending).
- **When to use:** 3rd person, cinematics, complex camera behavior.
- **Knowledge Gap:** Cinemachine 3.0+ (Unity 6) major API changes vs 2.x.
- **Status:** Production-Ready
- **Package:** `com.unity.cinemachine` (Package Manager)
- **Detailed Docs:** [plugins/cinemachine.md](plugins/cinemachine.md)
- **Official:** https://docs.unity3d.com/Packages/com.unity.cinemachine@3.0/manual/index.html

---

### ✅ Addressables
- **Purpose:** Advanced asset management (async loading, remote content, memory control).
- **When to use:** Large projects, DLC, remote content delivery.
- **Knowledge Gap:** Unity 6 perf improvements.
- **Status:** Production-Ready
- **Package:** `com.unity.addressables` (Package Manager)
- **Detailed Docs:** [plugins/addressables.md](plugins/addressables.md)
- **Official:** https://docs.unity3d.com/Packages/com.unity.addressables@2.0/manual/index.html

---

### ✅ DOTS / Entities (ECS)
- **Purpose:** Data-Oriented Technology Stack (high-perf ECS, massive scale).
- **When to use:** 1000s of entities, RTS, simulations.
- **Knowledge Gap:** Entities 1.3+ (Unity 6) production-ready, big rewrite from 0.x.
- **Status:** Production-Ready (Unity 6.3 LTS)
- **Package:** `com.unity.entities` (Package Manager)
- **Detailed Docs:** [plugins/dots-entities.md](plugins/dots-entities.md)
- **Official:** https://docs.unity3d.com/Packages/com.unity.entities@1.3/manual/index.html

---

## Other Production-Ready Packages (Brief Overview)

### 🟡 Input System (Already Covered)
- **Purpose:** Modern input (rebindable, cross-platform).
- **Status:** Production-Ready (default in Unity 6)
- **Package:** `com.unity.inputsystem`
- **Docs:** See [modules/input.md](../modules/input.md)
- **Official:** https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/manual/index.html

---

### 🟡 UI Toolkit (Already Covered)
- **Purpose:** Modern runtime UI (HTML/CSS-like, performant).
- **Status:** Production-Ready (Unity 6)
- **Package:** Built-in
- **Docs:** See [modules/ui.md](../modules/ui.md)
- **Official:** https://docs.unity3d.com/Packages/com.unity.ui@2.0/manual/index.html

---

### 🟡 Visual Effect Graph (VFX Graph)
- **Purpose:** GPU particles (millions).
- **When to use:** Large VFX, fire, smoke, magic, explosions.
- **Status:** Production-Ready
- **Package:** `com.unity.visualeffectgraph` (URP/HDRP only)
- **Official:** https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@17.0/manual/index.html

---

### 🟡 Shader Graph
- **Purpose:** Visual shader editor (node-based).
- **When to use:** Custom shaders without HLSL.
- **Status:** Production-Ready
- **Package:** `com.unity.shadergraph` (URP/HDRP)
- **Official:** https://docs.unity3d.com/Packages/com.unity.shadergraph@17.0/manual/index.html

---

### 🟡 Timeline
- **Purpose:** Cinematic sequencing (cutscenes, scripted events).
- **When to use:** Story games, cinematics, scripted sequences.
- **Status:** Production-Ready
- **Package:** `com.unity.timeline` (built-in)
- **Official:** https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/index.html

---

### 🟡 Animation Rigging
- **Purpose:** Runtime IK, procedural animation.
- **When to use:** Foot IK, aim offsets, procedural limb placement.
- **Status:** Production-Ready (Unity 6)
- **Package:** `com.unity.animation.rigging`
- **Official:** https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/index.html

---

### 🟡 ProBuilder
- **Purpose:** In-editor 3D modeling (level prototyping, greyboxing).
- **When to use:** Rapid prototyping, level blockout.
- **Status:** Production-Ready
- **Package:** `com.unity.probuilder`
- **Official:** https://docs.unity3d.com/Packages/com.unity.probuilder@6.0/manual/index.html

---

### 🟡 Netcode for GameObjects
- **Purpose:** Official Unity multiplayer networking.
- **When to use:** Multiplayer (client-server).
- **Status:** Production-Ready
- **Package:** `com.unity.netcode.gameobjects`
- **Official:** https://docs-multiplayer.unity3d.com/netcode/current/about/

---

### 🟡 Burst Compiler
- **Purpose:** LLVM compiler for C# Jobs (massive perf).
- **When to use:** Perf-critical code, DOTS, Jobs.
- **Status:** Production-Ready
- **Package:** `com.unity.burst` (auto-installed with DOTS)
- **Official:** https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/index.html

---

### 🟡 Jobs System
- **Purpose:** Multi-threaded job scheduling (CPU parallelism).
- **When to use:** Perf optimization, parallel processing.
- **Status:** Production-Ready
- **Package:** Built-in
- **Official:** https://docs.unity3d.com/Manual/JobSystem.html

---

### 🟡 Mathematics
- **Purpose:** SIMD math library (Burst-optimized).
- **When to use:** DOTS, high-perf math.
- **Status:** Production-Ready
- **Package:** `com.unity.mathematics`
- **Official:** https://docs.unity3d.com/Packages/com.unity.mathematics@1.3/manual/index.html

---

### 🟡 ML-Agents (Machine Learning)
- **Purpose:** Train AI with reinforcement learning.
- **When to use:** Advanced AI training, procedural behavior.
- **Status:** Production-Ready
- **Package:** `com.unity.ml-agents`
- **Official:** https://github.com/Unity-Technologies/ml-agents

---

### 🟡 Recorder
- **Purpose:** Capture gameplay footage, screenshots, animation clips.
- **When to use:** Trailers, replays, debug recording.
- **Status:** Production-Ready
- **Package:** `com.unity.recorder`
- **Official:** https://docs.unity3d.com/Packages/com.unity.recorder@5.0/manual/index.html

---

## Preview/Experimental Packages (Use with Caution)

### ⚠️ Splines
- **Purpose:** Runtime spline creation/editing.
- **When to use:** Roads, paths, procedural content.
- **Status:** Production-Ready (Unity 6)
- **Package:** `com.unity.splines`
- **Official:** https://docs.unity3d.com/Packages/com.unity.splines@2.6/manual/index.html

---

### ⚠️ Muse (AI Assistant)
- **Purpose:** AI asset creation (textures, sprites, animations).
- **Status:** Preview (Unity 6)
- **Package:** `com.unity.muse.*`
- **Official:** https://unity.com/products/muse

---

### ⚠️ Sentis (Neural Network Inference)
- **Purpose:** Run neural networks in Unity (AI inference).
- **Status:** Preview
- **Package:** `com.unity.sentis`
- **Official:** https://docs.unity3d.com/Packages/com.unity.sentis@2.0/manual/index.html

---

## Deprecated Packages (Avoid for New Projects)

### ❌ UGUI (Canvas UI)
- **Deprecated:** Still supported; UI Toolkit recommended.
- **Use Instead:** UI Toolkit

---

### ❌ Legacy Particle System
- **Deprecated:** Use Visual Effect Graph (VFX Graph).
- **Use Instead:** VFX Graph

---

### ❌ Legacy Animation
- **Deprecated:** Use Animator (Mecanim).
- **Use Instead:** Animator Controller

---

## On-Demand WebSearch Strategy

For packages NOT listed:

1. **WebSearch** latest docs: `"Unity 6.3 [package name]"`.
2. Verify package:
   - Post-cutoff (beyond May 2025)
   - Preview vs Production-Ready
   - Still supported in Unity 6.3 LTS
3. Optionally cache findings in `plugins/[package-name].md`.

---

## Quick Decision Guide

**Need virtual cameras** → **Cinemachine**
**Need async asset loading / DLC** → **Addressables**
**Need 1000s entities (RTS, sim)** → **DOTS/Entities**
**Need modern input** → **Input System** (see modules/input.md)
**Need GPU particles** → **Visual Effect Graph**
**Need visual shaders** → **Shader Graph**
**Need cinematics** → **Timeline**
**Need runtime IK** → **Animation Rigging**
**Need level prototyping** → **ProBuilder**
**Need multiplayer** → **Netcode for GameObjects**

---

**Last Updated:** 2026-02-13
**Engine Version:** Unity 6.3 LTS
**LLM Knowledge Cutoff:** May 2025
