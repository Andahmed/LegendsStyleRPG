# Asset Store graphics integration

The image assets under `GameClient/Assets/Resources/UISkin/AssetStore/` were downloaded from the project's Unity Asset Store account and are used as runtime UI sprites by `LegendsRPG.UI.UISkin`.

Imported image packs:

- Cute Cartoon Mobile GUI
- FREE 2D Archers Sprites
- Pixel HUD UI Free Kit
- RPG Fantasy Spell Icons
- Fantasy RPG UI Kit - Free
- GUI Parts

Only image files were included in the game project. Demo scenes, editor tooling, scripts, and unrelated template files were intentionally excluded.

## v0.5.2 review notes

- **Only the sprites that the code uses stay in `Resources/UISkin`** (see `UISkin.cs`: GUIParts `button`, `big_roundframe`, `lil_roundframe`, Cute Cartoon `panel_start` / `panel_top`, ...). Everything inside a `Resources` folder is packed into the APK, so heavy / unused material (PSD sources, preview images, Archers sprite frames) was moved to `Assets/GameArt/AssetStore_Unused/` (not in the build, GUIDs kept).
- `UISkin` no longer calls `Resources.LoadAll` (it used to load ~700 sprites at once); it loads single sprites by path and caches them.
- Frames / panels / buttons had no 9-slice border in their import settings, so they were stretched and distorted. `Editor/UISkinImporter.cs` now sets a border automatically (only when none was set by hand) and `UISkin.Apply` keeps the on-screen border size sensible.
- Name clashes: many packs contain the same file names (`button`, `icon`...). Do not rely on short names: reference the full Resources path.
