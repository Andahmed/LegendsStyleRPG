# Architecture — LegendsStyleRPG

## Overview

```
┌─────────────────────────────────────────────┐
│                Android Device               │
│  ┌───────────────────────────────────────┐  │
│  │        GameClient (Unity 2022.3)      │  │
│  │  C# runtime — everything procedural   │  │
│  └──────────────────┬────────────────────┘  │
│                     │ HTTPS/JSON (later)    │
└─────────────────────┼───────────────────────┘
                      ▼
        ┌──────────────────────────┐
        │   Backend (Python/FastAPI)│
        │   Auth · Save · Economy   │
        └──────────┬───────────────┘
                   ▼
             SQLite → PostgreSQL
```

## Design principles

1. **Client/Server separation (hard rule):** the phone app never owns accounts,
   inventory truth, or economy. The backend is authoritative. In Prototype 0.1
   the game runs fully offline; `ApiClient` is stubbed and ready for wiring.
2. **Zero manual scene setup:** a single `Boot.unity` contains one `GameRoot`
   component. Everything else (arena, characters, UI, VFX) is built in code —
   this is what makes the project re-buildable on any machine from source.
3. **Data-driven content:** heroes, monsters, skills, tower floors and waves
   are data + formulas (`GameConfig`, `HeroDatabase`, `TowerSystem`), never
   duplicated hand-authored scenes. 50+ heroes = data entries, not code paths.
4. **Pooled everything:** projectiles, VFX and popups use `ObjectPool` to keep
   mobile GC pressure low.

## Client module map (Assets/Scripts)

| Folder | Responsibility | Key classes |
|--------|----------------|-------------|
| `Core` | bootstrapping, state, config, save | `GameRoot`, `GameManager`, `GameConfig`, `SaveSystem`, `ObjectPool` |
| `CameraRig` | isometric camera: follows the team center, fixed-angle buttons, zoom, shake | `IsometricCameraController` |
| `InputControl` | pinch zoom only — all battle input is uGUI (portraits/buttons) | `TouchInputController` |
| `Characters` | stats, HP, movement, procedural rigs, hero & monster logic, roster data | `HeroCharacter`, `MonsterCharacter`, `CharacterStats`, `Health`, `CharacterMotor`, `CharacterAnimator`, `CharacterBodyFactory`, `HeroData`, `MonsterData`, `HeroDatabase` |
| `Combat` | damage pipeline, elements, skills, projectiles, popups, unit HP bars | `CombatUtil`, `ElementalMatrix`, `SkillData`, `SkillCaster`, `Projectile`, `DamagePopup`, `CombatEvents`, `UnitHealthBar` |
| `Team` | hero team, full-battle AI (no switching), auto skills | `TeamManager`, `TeamAIBrain`, `AutoSkills` |
| `Items` | 235 formula-generated items (weapons/armor/amulets/wings/pets/mounts × 5 rarities), ownership, equipping, loot rolls, wing & pet visuals | `ItemData`, `ItemDatabase`, `InventorySystem`, `WingVisual`, `PetOrb` |
| `World` | arena construction, waves, experience tower | `ArenaBuilder`, `WaveSpawner`, `TowerSystem` |
| `VFX` | procedural particles & materials | `VFXFactory`, `MaterialLibrary` |
| `UI` | code-built HUD | `UIBuilder`, `HUDController` |
| `Audio` | runtime-synthesized SFX/BGM | `AudioSystem` |

## v1.1 combat model (Legends Reborn style — final design decision)

- Every hero is fully autonomous: `TeamAIBrain` hunts the nearest monster,
  chases it, holds a front (melee) / back (ranged) formation around the team
  center and always auto-attacks. There is no joystick, no manual attack and
  no active-hero switching.
- The player controls: tapping a ready portrait (fires that hero's Ultimate
  when its Rage bar is full — the portrait glows), the AUTO toggle (auto-casts
  skills + ultimates), battle speed x1/x2/x3, pause, and the side camera
  buttons (angle / reset — the camera never rotates by dragging).
- The camera follows `TeamManager.FocusTransform` (live centroid of alive
  heroes) and its follow uses `Time.unscaledDeltaTime` so it keeps moving
  while the game is paused.

## Damage formula (single source of truth — `CombatUtil.DealDamage`)

```
final = base
      × elementalMultiplier[attacker.element → defender.element]   // 1.35 strong / 0.7 weak
      × (1 - defense / (defense + K × (attackerLevel + 10)))        // K = 60
      × (crit ? critDamage : 1)                                     // crit chance roll
```

Element wheel: Fire ▸ Ice ▸ Nature ▸ Water ▸ Fire; Lightning ▸ Water, Nature ▸ Lightning; Light ↔ Dark.

## Items & equipment (formula-driven, zero duplication)

```
235 items = 6 categories × (8 templates or 7 pet elements) × 5 rarities
rarity stat multiplier  = {1.0, 1.4, 1.9, 2.6, 3.5}
weapon ATK(base)        = 4 + 1.5 × templateIndex
armor HP(base)          = 30 + 10 × templateIndex
mount SPD(base)         = 0.7 + 0.12 × templateIndex
loot roll (floor clear) = 30% + 3%/difficulty, rarity odds shift with floor/50
```

Slots per hero: Weapon · Armor · Amulet · Wings · Pet · Mount (6). Equipping calls
`CharacterStats.AddEquipmentBonus` (real stat change + HP bar sync) and spawns
procedural visuals: flapping `WingVisual` panels + an orbiting `PetOrb`.
Ownership and per-hero loadouts persist in the save file (schema v2,
backward-compatible: v1 saves load with empty inventories and get the starter kit).

## Tower progression (formula-driven, zero duplication)

```
hpScale  = (1 + 0.115 × (floor-1)) × difficultyHp[floor/200 difficulty]
atkScale = (1 + 0.075 × (floor-1)) × difficultyAtk[...]
expScale = difficultyExp[...] × (1 + 0.01 × (floor-1))
4 difficulties (Easy/Hard/Hell/Nightmare) × 200 floors, boss every 10th floor.
```

## Backend (Phase: later prototypes)

- FastAPI + SQLAlchemy. SQLite for dev, PostgreSQL for prod (URL via env).
- Endpoints (auth, player save/load, health) are already scaffolded under `Backend/app/api`.
- The client's future `ApiClient` reads the server URL from a configurable
  constant — never hard-coded production secrets inside the APK.

## Runtime bootstrap sequence

1. `GameRoot.Awake` (Boot scene) → builds AudioSystem → ObjectPool → VFXFactory
2. `ArenaBuilder.Build()` — arena, lights, fog, props
3. Camera → GameManager + Team/Tower/Wave/AutoSkills components
4. `SaveSystem.Load()` → `InventorySystem.Initialize()` (items + starter kit)
   → roster (62 heroes) → `TeamManager.CreateTeam` (equipment applied per hero) → camera follows the team center
5. HUD build + pinch-zoom wiring → `WaveSpawner.StartFloor()` → battle state

## Extension points (per roadmap)

- 0.2: swap `CharacterAnimator` for Animator+FBX/GLB clips (same Play* API); add quality settings presets.
- 0.3: team roster UI screens; since v1.1 `TeamAIBrain` powers every hero (full auto combat); `InventorySystem` already exposes `GetEquippedName/IsOwned/Equip` for an inventory UI.
- 0.4: `CharacterStats.AddEquipmentBonus` + `StatBonus` implemented AND live via `InventorySystem` (6 slots, loot, persistence).
- 0.5: `TowerSystem.SetDifficulty/Floor` already drives everything; UI screens only.
- Online: replace `SaveSystem` local persistence with backend sync via `ApiClient` (schema-compatible JSON).
