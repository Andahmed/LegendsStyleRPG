# Architecture — LegendsStyleRPG

## Canonical project

The repository contains legacy/root-level material and the actual Unity project. The only supported Unity project root is **GameClient/**, which contains Assets, Packages, and ProjectSettings.

Pinned editor version: **Unity 2022.3.62f3**.

## Runtime architecture

The game is a hybrid runtime-built 3D RPG:

- GameRoot is the entry point in the generated Boot.unity scene.
- City/diorama, arena, heroes, monsters, UI, VFX, audio and meta systems are assembled by C# at runtime.
- Imported FBX/GLB assets are used where mapped and available; procedural geometry remains a fallback and is still used by some room/table/terrain and character construction paths.
- Content such as heroes, monsters, skills, equipment, floors and waves is data-driven.

## Client modules

| Folder | Responsibility | Key classes |
|---|---|---|
| Core | Bootstrap, save, profile, economy, meta systems, battle session, API client | GameRoot, GameManager, GameConfig, SaveSystem, BattleSession, ApiClient |
| Characters | Hero/monster data, health, stats, motor, animation and body fallback | HeroCharacter, MonsterCharacter, CharacterBodyFactory, HeroDatabase |
| Combat | Damage pipeline, elements, skills, projectiles, damage display and health bars | CombatUtil, ElementalMatrix, SkillCaster |
| Team | Six-hero team, targeting AI, auto skills | TeamManager, TeamAIBrain, AutoSkills |
| Items | Item database, ownership, inventory, equipment visuals | ItemDatabase, InventorySystem, WingVisual, PetOrb |
| World | Diorama home, arena, guild hall, progression, tower and waves | DioramaHomeBuilder, ArenaBuilder, TowerSystem, WaveSpawner |
| Art | Runtime bridge for imported Quaternius resources | QuaterniusAssetLibrary |
| Social | Guild progression and features | GuildSystem |
| UI | Runtime-built screens, shell and HUD | UIManager, HUDController, UIBuilder |
| VFX / Audio | Effects, materials and synthesized audio | VFXFactory, MaterialLibrary, AudioSystem |
| CameraRig / InputControl | Camera follow/zoom and pinch input | IsometricCameraController, TouchInputController |

## Gameplay baseline in source

- 62 heroes (20 authored plus 42 generated) and a six-hero team.
- Autonomous team combat: every hero is driven by TeamAIBrain; no joystick, manual attack button or active-hero switching in the current combat model.
- Player controls include portrait Ultimate, AUTO, x1/x2/x3 speed, pause and fixed camera controls.
- Campaign: 12 chapters × 15 stages.
- Tower: four difficulties × 200 floors, with formula-based progression.
- Equipment database: 235 generated items, six equipment slots per hero.

These are source-level facts; they are not a claim that every gameplay path has passed runtime testing.

## Art asset pipeline

The current repository includes 208 FBX files under:
GameClient/Assets/Resources/GameArt/Free3D/

- 150 Nature
- 48 Modular Dungeon
- 4 Animated Monsters
- 6 RPG Characters

QuaterniusAssetLibrary loads resources at runtime. HeroCharacter and MonsterCharacter hide their procedural body only after an imported model is successfully instantiated. QuaterniusFbxImporter sets model import defaults. DioramaGlbRuntimeLoader tries to replace fallback landmarks with bundled GLB payloads.

Model reuse is intentional in the current mapping: six humanoid models cover hero classes, and four monster models cover multiple monster IDs. Some diorama room/table/terrain parts remain procedural.

## Build configuration

The source of truth is GameClient/Assets/Editor/BuildTools.cs plus GameClient/ProjectSettings/ProjectVersion.txt.

- Unity 2022.3.62f3
- Android APK
- Application ID com.ahmed.legendsstylerpg
- IL2CPP; ARMv7 + ARM64
- min API 23; target API 34
- Landscape orientation
- output Builds/Android/LegendsRPG-v1.2.0.apk

GitHub Actions invokes LegendsRPG.EditorTools.BuildTools.BuildAndroid. The workflow requires UNITY_LICENSE, UNITY_EMAIL and UNITY_PASSWORD repository secrets.

## Backend boundary

The FastAPI backend is a separate service scaffold. The game client remains offline/local until the client API calls are wired and validated end-to-end. Do not infer online authentication, cloud save or server-authoritative economy from the existence of backend endpoints alone.

## Verification boundary

A source tree inspection does not prove Unity import, C# compilation, Play Mode, Android packaging, device compatibility or performance. See KNOWN-UNVERIFIED.md for the current evidence boundary and TestingGuide.md for acceptance tests.
