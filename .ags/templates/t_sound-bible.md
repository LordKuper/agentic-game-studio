# Sound Bible: [Project Name]

## Audio Vision

### Sonic Identity
[2-3 sentences. Overall audio personality. What does game "sound like"? Emotions audio evokes?]

### Audio Pillars
1. **[Pillar 1]**: [How manifests in audio]
2. **[Pillar 2]**: [How manifests in audio]
3. **[Pillar 3]**: [How manifests in audio]

### Reference Games / Media
| Reference | Take From | Avoid |
| ---- | ---- | ---- |
| [Game/Film 1] | [Audio quality to emulate] | [Doesn't fit vision] |
| [Game/Film 2] | [Audio quality] | [Doesn't fit] |

---

## Music Direction

### Style and Genre
[Primary style, instrumentation palette, tempo ranges]

### Instrumentation Palette
- **Core**: [Primary instruments/synths defining sound]
- **Accent**: [Emphasis, transitions, special moments]
- **Avoid**: [Instruments/styles NOT fitting]

### Adaptive Music System
| Game State | Music Behavior | Transition |
| ---- | ---- | ---- |
| Exploration | [Tempo, energy, instrumentation] | [How transitions to next] |
| Combat | [Tempo, energy, instrumentation] | [Trigger + crossfade time] |
| Stealth/Tension | [Tempo, energy, instrumentation] | [Trigger + transition] |
| Victory/Reward | [Stinger or transition] | [Return to exploration] |
| Menu/UI | [Menu style] | [Fade on game start] |

### Music Rules
- [Looping — e.g., "Exploration tracks loop seamlessly after 2-4 min"]
- [Silence — e.g., "10-15s silence between exploration loops"]
- [Intensity — e.g., "Combat reaches full intensity within 3s of combat start"]
- [Transitions — e.g., "All transitions use 1.5s crossfades"]

---

## Sound Effects

### SFX Palette
| Category | Description | Style |
| ---- | ---- | ---- |
| Player Actions | [Movement, attacks, abilities] | [Punchy, responsive, front-of-mix] |
| Enemy Actions | [Attacks, abilities, death] | [Distinct from player, recessed] |
| UI | [Clicks, transitions, notifications] | [Clean, subtle, non-annoying on repeat] |
| Environment | [Ambient, weather, objects] | [Immersive, layered, spatial] |
| Feedback | [Damage, pickup, level up] | [Clear, satisfying, non-fatiguing] |

### Audio Feedback Priority
Multiple sounds compete → priority:
1. Player damage / critical warnings (always audible)
2. Player actions (attacks, abilities)
3. Enemy actions (nearby first)
4. UI feedback
5. Environment / ambient

### SFX Rules
- [Repetition — e.g., "Every SFX with >3 plays/min needs 3+ variations"]
- [Spatial — e.g., "Gameplay SFX 3D positioned, UI 2D"]
- [Ducking — e.g., "Player hit ducks others -3dB for 200ms"]
- [Response time — e.g., "Action SFX trigger within 1 frame of action"]

---

## Mixing

### Mix Bus Structure
| Bus | Content | Target Level |
| ---- | ---- | ---- |
| Master | Everything | 0 dB |
| Music | All music | [target dBFS] |
| SFX | All SFX | [target dBFS] |
| Dialogue | Voice/narration | [target dBFS] |
| UI | Interface | [target dBFS] |
| Ambient | Environment loops | [target dBFS] |

### Mixing Rules
- Dialogue priority — duck music + SFX during dialogue
- Music felt, not dominant — SFX audible over music
- Master never clips — limiter on master
- All volumes player-adjustable (per bus)
- Default mix sounds good speakers + headphones

### Dynamic Range
- [Loudness — e.g., "Target -14 LUFS integrated, -1 dBTP true peak"]
- [Compression — e.g., "Light on SFX bus, none on music"]

---

## Technical Specifications

### Format Requirements
| Type | Format | Sample Rate | Bit Depth | Notes |
| ---- | ---- | ---- | ---- | ---- |
| Music | [OGG/WAV] | [44.1/48 kHz] | [16/24 bit] | [Streaming from disk] |
| SFX | [WAV/OGG] | [44.1/48 kHz] | [16 bit] | [Loaded into memory] |
| Ambient | [OGG] | [44.1 kHz] | [16 bit] | [Streaming, loopable] |
| Dialogue | [OGG/WAV] | [44.1 kHz] | [16 bit] | [Streaming] |

### Naming Convention
`[category]_[subcategory]_[name]_[variation].ext`
- `sfx_weapon_sword_swing_01.wav`
- `music_exploration_forest_loop.ogg`
- `amb_environment_cave_drip_loop.ogg`

### Memory Budget
- Total audio: [target, e.g., 128 MB]
- SFX pool: [target]
- Music streaming buffer: [target]
- Voice streaming buffer: [target]

---

## Accessibility

- All critical audio cues need visual alternatives (subtitles, screen flash, icon)
- Mono audio option for hearing-impaired
- Separate volume per bus
- Option to disable sudden loud sounds
- Subtitle support for all dialogue with speaker ID
