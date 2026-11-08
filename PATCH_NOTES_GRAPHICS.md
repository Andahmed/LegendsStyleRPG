# Graphics & UI pass (v0.1.1)

Copy `GameClient/Assets/Scripts` over your project. No new assets or packages needed.

## Bugs fixed (visible in your screenshot)
- **Heroes looked like domes on the floor with heads floating above**: `CharacterAnimator` reset the torso's local position to (0, bob, 0) while walking. Now uses the saved base position (torso + head bob together).
- **Eyes were inside the head sphere** (never visible) - moved to the surface.
- **Limbs were the same grey as rocks/pillars** - now tinted with the hero colour.
- **Boss/monster bodies ignored `scale` for torso/head height** - fixed.
- **HUD**: HP/EXP/portrait bars were centred on their left edge (half off-screen); the Ult charge mask and skill cooldown masks were anchored to a corner (the stray black rectangle). Pivots/anchors fixed.
- **Buttons overlapped** (ATTACK / S1 / ULT) - re-laid out in a thumb arc.
- **Heroes stacked on each other** - 3x2 formation (2.6m spacing), wider follow offsets.

## Visual upgrades
- Stone-tile floor texture (noise, grout, trilinear + aniso) instead of a point-filtered checkerboard.
- Warm key light, stronger shadows, brighter ambient, procedural gradient sky (guarded fallback), MSAA x4.
- Coloured ground ring under each unit (bigger for the active hero; red for enemies), belt + glowing chest gem.
- Rocks kept out of the party area and lowered; lighter pillars.
- Camera: FOV 42, default distance 16.
- UI: rounded 9-slice buttons/panels with drop shadow, bold outlined text.

# Menu & economy pass (v0.2.0)

Landscape (1920x1080) + a main menu inspired by the structure of the reference game (all art/names/items are original).

New files
- `Scripts/Core/Economy.cs` - items, inventory, gold/gems, daily missions, daily shop (all saved in the existing JSON save, schema 2; old saves are migrated and get a starter pack).
- `Scripts/UI/Menu/MenuTheme.cs` - code-generated "tavern ledger" widgets (wood / parchment / gold).
- `Scripts/UI/Menu/MenuController.cs` - Home, Heroes, Bag, Shop, Daily Missions screens.

Changed
- `GameRoot` opens the menu at boot (battle paused until ENTER BATTLE); forces landscape.
- `HUDController`: MENU button (top-left) opens the menu and pauses; `SetVisible`.
- `UIBuilder`: reference resolution 1920x1080.
- `GameManager`: floor clear gives gold + advances the "clear floors" mission.
- `ProjectBootstrapEditor`: landscape-only player settings.

Screens
- Home: tower card (floor/difficulty), ENTER BATTLE, lineup, bottom nav.
- Heroes: grid with level/stars, element filter, detail panel (stats, skills, EXP).
- Bag: Items / Shards / Equip tabs, detail panel, USE (EXP Elixir, Mystic Chest).
- Shop: 8 daily offers with discount ribbons, daily limits, gem prices, restock timer.
- Missions: 4 daily missions with progress bars and CLAIM.
- Forge: placeholder ("coming soon").

# v0.5 merge note
Summon / hub / art kit come from the v0.4 branch. This branch contributes: equipment system, full hero screen (EQUIP / LEVEL UP),
per-hero level persistence. See PATCH_NOTES.md (v0.5).
