# Retained graphics inventory

The clean rebuild keeps the original art assets and drops the old gameplay/application code from this branch. The original `main` branch remains unchanged.

## Verified source asset count
The rebuilt branch currently contains the same 954 image/model source files counted in the original `main` tree:

| Type | Count |
|---|---:|
| GLB | 5 |
| FBX | 208 |
| PNG | 730 |
| JPG | 8 |
| PSD | 3 |
| **Total** | **954** |

## Where the graphics live

- `GameClient/Assets/Resources/GameArt/` — character, monster, nature and modular-dungeon FBX libraries. Includes Warrior/Wizard/Ranger/Rogue/Cleric/Monk, Bat/Dragon/Skeleton/Slime, modular structures, banners, torches and chests.
- `GameClient/Assets/Resources/UISkin/` — original fantasy RPG UI and skill-icon source art.
- `GameClient/Assets/Resources/Models/Diorama/` — the five original diorama model payloads retained for the existing Unity loading path.
- `GameClient/Assets/Graphics/GLBOriginals/` — original Guild Castle, Arena Banner, Beast Banner, Continent Portal and Summon Altar GLB files.
- `GameClient/Assets/Graphics/Textures/` — terrain texture source files.
- `GameClient/Assets/Graphics/ReferenceImages/` — guild, arena, summon and hero reference images.
- `GameClient/Assets/Graphics/Icons/` — class and skill icon images.

These are preserved source assets. They have not all been placed into final levels, assigned animation controllers, optimized, or bundled into an Android build yet.
