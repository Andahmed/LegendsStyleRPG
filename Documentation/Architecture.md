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
| `CameraRig` | isometric camera: follow/orbit/zoom/shake | `IsometricCameraController` |
| `InputControl` | touch joystick, gestures, editor fallback | `TouchInputController` |
| `Characters` | stats, HP, movement, procedural rigs, hero & monster logic, roster data | `HeroCharacter`, `MonsterCharacter`, `CharacterStats`, `Health`, `CharacterMotor`, `CharacterAnimator`, `CharacterBodyFactory`, `HeroData`, `MonsterData`, `HeroDatabase` |
| `Combat` | damage pipeline, elements, skills, projectiles, popups | `CombatUtil`, `ElementalMatrix`, `SkillData`, `SkillCaster`, `Projectile`, `DamagePopup`, `CombatEvents` |
| `Team` | 6-hero team, switching, AI, auto battle | `TeamManager`, `TeamAIBrain`, `AutoBattleController` |
| `World` | arena construction, waves, experience tower | `ArenaBuilder`, `WaveSpawner`, `TowerSystem` |
| `VFX` | procedural particles & materials | `VFXFactory`, `MaterialLibrary` |
| `UI` | code-built HUD | `UIBuilder`, `HUDController` |
| `Audio` | runtime-synthesized SFX/BGM | `AudioSystem` |

## Damage formula (single source of truth — `CombatUtil.DealDamage`)

```
final = base
      × elementalMultiplier[attacker.element → defender.element]   // 1.35 strong / 0.7 weak
      × (1 - defense / (defense + K × (attackerLevel + 10)))        // K = 60
      × (crit ? critDamage : 1)                                     // crit chance roll
```

Element wheel: Fire ▸ Ice ▸ Nature ▸ Water ▸ Fire; Lightning ▸ Water, Nature ▸ Lightning; Light ↔ Dark.

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
3. Camera → GameManager + Team/Tower/Wave/AutoBattle components
4. `SaveSystem.Load()` → roster → `TeamManager.CreateTeam` → camera target
5. HUD build + input wiring → `WaveSpawner.StartFloor()` → battle state

## Extension points (per roadmap)

- 0.2: swap `CharacterAnimator` for Animator+FBX/GLB clips (same Play* API); add quality settings presets.
- 0.3: team roster UI screens; TeamAIBrain already powers non-active heroes.
- 0.4: `CharacterStats.AddEquipmentBonus` + `StatBonus` already implemented for items/wings/pets.
- 0.5: `TowerSystem.SetDifficulty/Floor` already drives everything; UI screens only.
- Online: replace `SaveSystem` local persistence with backend sync via `ApiClient` (schema-compatible JSON).
