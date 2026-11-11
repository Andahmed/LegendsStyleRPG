# Legends Reborn — Clean Rebuild

This branch is a clean Unity rebuild. The original `main` branch is preserved unchanged.

## Project
- Unity Editor: 2022.3.62f3
- Client project: `GameClient/`
- Initial target: Android, portrait/landscape responsive runtime UI, third-person action RPG foundation.
- Graphics are retained under `GameClient/Assets/Resources` and `GameClient/Assets/Graphics`.

## First playable foundation
- Runtime-generated training ground using retained modular-dungeon and nature models.
- Warrior player model, third-person follow camera and mobile virtual joystick.
- Skeleton/slime enemies with chase and melee behavior.
- Basic attack, mana skill, health, XP and gold feedback.

## Open
Open `GameClient/` with Unity 2022.3.62f3. Open `Assets/Scenes/Main.unity` and press Play.

## Rebuild stages
1. Stable player movement, camera, scene and combat.
2. Character classes, animation controllers and skills.
3. World zones, dungeon instances, bosses and drops.
4. Guild, summon, inventory, equipment and progression.
5. Backend authority, persistence, matchmaking and Android release hardening.

The first stage is a local playable foundation, not a finished online MMORPG. Networking and server-authoritative progression are intentionally deferred until core gameplay is stable.

## Validation status
- GitHub tree, retained asset count, scene reference and runtime resource paths have been checked.
- Unity Editor compilation, Play Mode, and Android APK/AAB build have **not** yet been run for this clean branch.
